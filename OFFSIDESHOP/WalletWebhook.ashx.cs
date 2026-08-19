using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OFFSIDESHOP
{
    public class WalletWebhook : IHttpHandler
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            // 1. Validar que la petición sea estrictamente POST (enviada por el servidor de la Billetera)
            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405; // Method Not Allowed
                context.Response.Write("Solo se permiten peticiones POST enviadas por el servidor de la Billetera.");
                return;
            }

            try
            {
                // 2. Leer el cuerpo de la petición (JSON)
                string jsonPayload;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    jsonPayload = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(jsonPayload))

                {
                    // Responde 200 OK si es un healthcheck/ping de prueba del panel
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    context.Response.Write("{\"status\":\"ping_ok\"}");
                    return;
                }

                // 3. Parsear JSON
                JObject webhookData = JObject.Parse(jsonPayload);
                Console.WriteLine("webhook" + webhookData.ToString());

                string paymentStatus = webhookData["status"]?.ToString()?.ToLower();
                string transactionId = webhookData["transaction_id"]?.ToString();
                string referenceStr = webhookData["reference"]?.ToString() ?? webhookData["order_id"]?.ToString();

                int.TryParse(referenceStr, out int orderIdFromRef);

                // 4. Procesar el pago si el estado indica éxito
                if (paymentStatus == "success" || paymentStatus == "paid" || paymentStatus == "completed")
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();

                        // Buscar la orden por Id_Order O por TransactionID
                        int targetOrderId = 0;
                        int currentStatus = 0;

                        string findOrderQuery = @"SELECT Id_Order, Id_Status FROM orders 
                                                  WHERE (Id_Order = @OrderId AND @OrderId > 0) 
                                                     OR (TransactionID IS NOT NULL AND TransactionID = @TransId) 
                                                  LIMIT 1";

                        using (MySqlCommand findCmd = new MySqlCommand(findOrderQuery, conn))
                        {
                            findCmd.Parameters.AddWithValue("@OrderId", orderIdFromRef);
                            findCmd.Parameters.AddWithValue("@TransId", (object)transactionId ?? DBNull.Value);

                            using (MySqlDataReader reader = findCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    targetOrderId = Convert.ToInt32(reader["Id_Order"]);
                                    currentStatus = Convert.ToInt32(reader["Id_Status"]);
                                }
                            }
                        }

                        // Si no se encuentra la orden
                        if (targetOrderId == 0)
                        {
                            context.Response.StatusCode = 404; // Not Found
                            context.Response.Write("{\"error\":\"Orden no encontrada en la base de datos\"}");
                            return;
                        }

                        // Idempotencia: Si ya está en Estado 2 (Paid/Pagado), respondemos OK y no duplicamos el descuento
                        if (currentStatus == 2)
                        {
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            context.Response.Write("{\"status\":\"already_processed\"}");
                            return;
                        }

                        // Transacción SQL para actualizar estado y descontar inventario
                        using (MySqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // A) Cambiar orden a Estado 2 (Paid) y asegurar TransactionID
                                string updateOrderQuery = @"UPDATE orders 
                                                            SET Id_Status = 2, 
                                                                TransactionID = COALESCE(TransactionID, @TransId) 
                                                            WHERE Id_Order = @OrderId;";

                                using (MySqlCommand cmdOrder = new MySqlCommand(updateOrderQuery, conn, transaction))
                                {
                                    cmdOrder.Parameters.AddWithValue("@TransId", (object)transactionId ?? DBNull.Value);
                                    cmdOrder.Parameters.AddWithValue("@OrderId", targetOrderId);
                                    cmdOrder.ExecuteNonQuery();
                                }

                                // B) Descontar el stock en `tshirt_variants`
                                string updateStockQuery = @"
                                    UPDATE tshirt_variants tv
                                    INNER JOIN sizes s ON tv.Id_Size = s.Id_Size
                                    INNER JOIN order_details od ON tv.Id_Tshirt = od.Id_Tshirt 
                                        AND (s.Size_Code = od.Size OR (od.Id_Size IS NOT NULL AND tv.Id_Size = od.Id_Size))
                                    SET tv.Stock = GREATEST(0, tv.Stock - od.Quantity)
                                    WHERE od.Id_Order = @OrderId;";

                                using (MySqlCommand cmdStock = new MySqlCommand(updateStockQuery, conn, transaction))
                                {
                                    cmdStock.Parameters.AddWithValue("@OrderId", targetOrderId);
                                    cmdStock.ExecuteNonQuery();
                                }

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception("Error en transacción de BD: " + ex.Message);
                            }
                        }
                    }
                }

                // 5. Responder 200 OK a la billetera
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"status\":\"received\"}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
            }
        }

        public bool IsReusable => false;
    }
}