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
            // 1. Validar que la petición sea estrictamente POST
            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405; // Method Not Allowed
                context.Response.Write("Solo se permiten peticiones POST");
                return;
            }

            try
            {
                // 2. Leer el cuerpo de la petición (JSON) que envía la Billetera
                string jsonPayload;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    jsonPayload = reader.ReadToEnd();
                }

                // Validación de payload vacío
                if (string.IsNullOrWhiteSpace(jsonPayload))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    context.Response.Write("Payload vacío");
                    return;
                }

                // 3. Convertir el texto a un Objeto JSON
                JObject webhookData = JObject.Parse(jsonPayload);

                // Extracción de datos del JSON recibido
                string paymentStatus = webhookData["status"]?.ToString()?.ToLower();
                string transactionId = webhookData["transaction_id"]?.ToString();
                string orderIdStr = webhookData["reference"]?.ToString() ?? webhookData["order_id"]?.ToString();

                if (!int.TryParse(orderIdStr, out int orderId))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    context.Response.Write("ID de orden no válido o ausente");
                    return;
                }

                // 4. Procesar el pago si el estado es 'success' o 'paid'
                if (paymentStatus == "success" || paymentStatus == "paid" || paymentStatus == "completed")
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();

                        // Verificar primero si la orden ya fue procesada previa/idempotencia
                        string checkStatusQuery = "SELECT Id_Status FROM orders WHERE Id_Order = @OrderId";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkStatusQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@OrderId", orderId);
                            object currentStatus = checkCmd.ExecuteScalar();

                            if (currentStatus == null)
                            {
                                context.Response.StatusCode = 444; // Not Found
                                context.Response.Write("Orden no encontrada");
                                return;
                            }

                            // Si la orden ya está pagada (Id_Status = 2), respondemos 200 OK y no duplicamos descuento
                            if (Convert.ToInt32(currentStatus) == 2)
                            {
                                context.Response.StatusCode = 200;
                                context.Response.ContentType = "application/json";
                                context.Response.Write("{\"status\":\"already_processed\"}");
                                return;
                            }
                        }

                        // Iniciar Transacción SQL para asegurar consistencia
                        using (MySqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // A) Actualizar la orden a Estado 2 (Paid / Pagado) y guardar TransactionID
                                string updateOrderQuery = @"UPDATE orders 
                                                            SET Id_Status = 2, 
                                                                TransactionID = @TransId 
                                                            WHERE Id_Order = @OrderId;";

                                using (MySqlCommand cmdOrder = new MySqlCommand(updateOrderQuery, conn, transaction))
                                {
                                    cmdOrder.Parameters.AddWithValue("@TransId", (object)transactionId ?? DBNull.Value);
                                    cmdOrder.Parameters.AddWithValue("@OrderId", orderId);
                                    cmdOrder.ExecuteNonQuery();
                                }

                                // B) Descontar el stock en `tshirt_variants` cruzando `order_details` y `sizes`
                                string updateStockQuery = @"
                                    UPDATE tshirt_variants tv
                                    INNER JOIN sizes s ON tv.Id_Size = s.Id_Size
                                    INNER JOIN order_details od ON tv.Id_Tshirt = od.Id_Tshirt 
                                        AND (s.Size_Code = od.Size OR (od.Id_Size IS NOT NULL AND tv.Id_Size = od.Id_Size))
                                    SET tv.Stock = GREATEST(0, tv.Stock - od.Quantity)
                                    WHERE od.Id_Order = @OrderId;";

                                using (MySqlCommand cmdStock = new MySqlCommand(updateStockQuery, conn, transaction))
                                {
                                    cmdStock.Parameters.AddWithValue("@OrderId", orderId);
                                    cmdStock.ExecuteNonQuery();
                                }

                                // Confirmar cambios en la BD
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception("Error durante la transacción de la BD: " + ex.Message);
                            }
                        }
                    }
                }

                // 5. Responder confirmación con HTTP 200 OK
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"status\":\"received\"}");
            }
            catch (Exception ex)
            {
                // Devolver error 500 para indicar al pasador de pagos que reintente más tarde
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}