using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace OFFSIDESHOP
{
    public class WalletWebhook : IHttpHandler
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            // Configurar cabeceras de respuesta y permitir llamadas CORS
            context.Response.ContentType = "application/json";
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                return;
            }

            try
            {
                // 1. Leer el cuerpo de la petición si viene como Stream
                string payload = string.Empty;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    payload = reader.ReadToEnd();
                }

                string orderIdStr = null;
                string transactionId = null;

                // 2. Extraer datos (soporta tanto JSON como Form/QueryString)
                if (!string.IsNullOrWhiteSpace(payload))
                {
                    try
                    {
                        JObject data = JObject.Parse(payload);
                        orderIdStr = data["order_id"]?.ToString()
                                  ?? data["reference"]?.ToString()
                                  ?? data["Id_Order"]?.ToString()
                                  ?? data["custom"]?.ToString();

                        transactionId = data["transaction_id"]?.ToString()
                                     ?? data["tx_id"]?.ToString()
                                     ?? data["id"]?.ToString();
                    }
                    catch
                    {
                        // Si no es JSON estándar, continúa buscando en parámetros normales
                    }
                }

                // Si no se encontró en el cuerpo JSON, buscar en Form o QueryString
                if (string.IsNullOrEmpty(orderIdStr))
                {
                    orderIdStr = context.Request["order_id"]
                              ?? context.Request["reference"]
                              ?? context.Request["Id_Order"]
                              ?? context.Request["custom"];
                }

                if (string.IsNullOrEmpty(transactionId))
                {
                    transactionId = context.Request["transaction_id"]
                                 ?? context.Request["tx_id"]
                                 ?? context.Request["id"];
                }

                // Respuesta rápida para pings o pruebas de conectividad sin datos
                if (string.IsNullOrEmpty(orderIdStr) && string.IsNullOrEmpty(transactionId))
                {
                    context.Response.StatusCode = 200;
                    context.Response.Write("{\"status\":\"ok\",\"message\":\"Webhook activo\"}");
                    return;
                }

                int.TryParse(orderIdStr, out int orderId);

                // 3. Procesar en Base de Datos
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    int targetOrderId = 0;
                    int currentStatus = 0;

                    // Buscar la orden por ID o por TransactionID
                    string findOrderQuery = @"SELECT Id_Order, Id_Status FROM orders 
                                              WHERE (@OrderId > 0 AND Id_Order = @OrderId) 
                                                 OR (TransactionID IS NOT NULL AND TransactionID = @TransId) 
                                              LIMIT 1";

                    using (MySqlCommand findCmd = new MySqlCommand(findOrderQuery, conn))
                    {
                        findCmd.Parameters.AddWithValue("@OrderId", orderId);
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

                    if (targetOrderId == 0)
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Write("{\"error\":\"Orden no encontrada\"}");
                        return;
                    }

                    // Si ya está pagada (Id_Status = 2), no procesar de nuevo
                    if (currentStatus == 2)
                    {
                        context.Response.StatusCode = 200;
                        context.Response.Write("{\"status\":\"already_paid\"}");
                        return;
                    }

                    // 4. Actualizar Estado a Pagado (2) y descontar inventario
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // Actualizar pedido a estado 2 (Paid)
                            string updateOrderQuery = @"UPDATE orders 
                                                        SET Id_Status = 2, 
                                                            TransactionID = COALESCE(@TransId, TransactionID) 
                                                        WHERE Id_Order = @OrderId";

                            using (MySqlCommand cmdOrder = new MySqlCommand(updateOrderQuery, conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@TransId", (object)transactionId ?? DBNull.Value);
                                cmdOrder.Parameters.AddWithValue("@OrderId", targetOrderId);
                                cmdOrder.ExecuteNonQuery();
                            }

                            // Descontar inventario
                            string updateStockQuery = @"
                                UPDATE tshirt_variants tv
                                INNER JOIN sizes s ON tv.Id_Size = s.Id_Size
                                INNER JOIN order_details od ON tv.Id_Tshirt = od.Id_Tshirt AND s.Size_Code = od.Size
                                SET tv.Stock = GREATEST(0, tv.Stock - od.Quantity)
                                WHERE od.Id_Order = @OrderId";

                            using (MySqlCommand cmdStock = new MySqlCommand(updateStockQuery, conn, trans))
                            {
                                cmdStock.Parameters.AddWithValue("@OrderId", targetOrderId);
                                cmdStock.ExecuteNonQuery();
                            }

                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }

                context.Response.StatusCode = 200;
                context.Response.Write("{\"status\":\"success\",\"message\":\"Pago procesado con exito\"}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
            }
        }

        public bool IsReusable => false;
    }
}