using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Web;

namespace OFFSIDESHOP
{
    public partial class WalletWebhook : System.Web.UI.Page
    {
        // Ruta calificada completa para evitar cualquier error de ambigüedad
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Configurar la respuesta como JSON y limpiar cualquier cosa previa
            Response.Clear();
            Response.ContentType = "application/json";
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            Response.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS");

            // Permitir peticiones preflight (CORS)
            if (Request.HttpMethod == "OPTIONS")
            {
                Response.StatusCode = 200;
                Response.End();
                return;
            }

            // Validar que sea método POST
            if (Request.HttpMethod != "POST")
            {
                Response.StatusCode = 405; // Method Not Allowed
                Response.Write("{\"error\": \"Solo se permiten peticiones POST\"}");
                Response.End();
                return;
            }

            try
            {
                // 2. Leer el cuerpo de la petición (JSON Payload)
                string jsonPayload;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    jsonPayload = reader.ReadToEnd();
                }

                // Si el body está vacío (Healthcheck/Ping de la billetera)
                if (string.IsNullOrWhiteSpace(jsonPayload))
                {
                    Response.StatusCode = 200;
                    Response.Write("{\"status\":\"ping_ok\"}");
                    Response.End();
                    return;
                }

                // 3. Parsear el JSON
                JObject webhookData = JObject.Parse(jsonPayload);
                System.Diagnostics.Debug.WriteLine("Webhook Recibido: " + webhookData.ToString());

                // Extraer los datos importantes
                string paymentStatus = webhookData["status"]?.ToString()?.ToLower();
                string transactionId = webhookData["transaction_id"]?.ToString() ?? webhookData["tx_id"]?.ToString() ?? webhookData["id"]?.ToString();
                string referenceStr = webhookData["reference"]?.ToString() ?? webhookData["order_id"]?.ToString();

                int.TryParse(referenceStr, out int orderIdFromRef);

                // 4. Lógica de procesamiento (Si es exitoso, idéntica a PayPal)
                if (paymentStatus == "success" || paymentStatus == "paid" || paymentStatus == "completed")
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();

                        int targetOrderId = 0;
                        int currentStatus = 0;

                        // A) Buscar la orden por Id_Order O por TransactionID
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

                        // Validar si la orden existe
                        if (targetOrderId == 0)
                        {
                            Response.StatusCode = 404; // Not Found
                            Response.Write("{\"error\":\"Orden no encontrada\"}");
                            Response.End();
                            return;
                        }

                        // Idempotencia: Si ya está en Estado 2 (Pagado), respondemos OK y no procesamos de nuevo
                        if (currentStatus == 2)
                        {
                            Response.StatusCode = 200;
                            Response.Write("{\"status\":\"already_processed\"}");
                            Response.End();
                            return;
                        }

                        // B) Iniciar transacción SQL para actualizar la orden y descontar el stock
                        using (MySqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // B.1) Marcar orden como pagada (Estado = 2) y guardar TransactionID
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

                                // B.2) Descontar el stock en `tshirt_variants` basándose en los detalles de la orden
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
                                Response.StatusCode = 500;
                                Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
                                Response.End();
                                return;
                            }
                        }
                    }
                }

                // 5. Responder a la billetera que todo fue procesado con éxito
                Response.StatusCode = 200;
                Response.Write("{\"status\":\"received\"}");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
            }

            // Terminar inmediatamente la respuesta para no enviar nada más
            Response.End();
        }
    }
}