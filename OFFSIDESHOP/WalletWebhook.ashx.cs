using System;
using System.Web;
using System.IO;
using System.Configuration;
using MySql.Data.MySqlClient;
// Necesitarás tener instalada la librería Newtonsoft.Json desde NuGet
using Newtonsoft.Json.Linq;

namespace OFFSIDESHOP
{
    public class WalletWebhook : IHttpHandler
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            // 1. Validar que la petición sea estrictamente POST (como pide Virtual Wallet)
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

                // Si no hay datos, terminamos
                if (string.IsNullOrEmpty(jsonPayload))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    return;
                }

                // 3. Convertir el texto a un Objeto JSON
                JObject webhookData = JObject.Parse(jsonPayload);

                /* =========================================================
                 * AQUI DEBES EXTRAER LOS DATOS SEGÚN EL JSON DE TU COMPAÑERO
                 * (Ejemplo hipotético asumiendo que envía 'status' y 'order_id')
                 * ========================================================= */
                string paymentStatus = webhookData["status"]?.ToString();
                string transactionId = webhookData["transaction_id"]?.ToString();

                // Suponiendo que logramos pasarle el ID de la orden en la descripción o referencia
                string orderIdStr = webhookData["reference"]?.ToString();

                // 4. Lógica de Base de Datos: Actualizar estado y descontar stock
                if (paymentStatus == "success" || paymentStatus == "paid")
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();

                        // Ejemplo: Actualizar la orden a "Pagada" (Id_Status = 2) y guardar el TransactionID
                        string updateOrder = "UPDATE orders SET Id_Status = 2, TransactionID = @TransId WHERE Id_Order = @OrderId";
                        using (MySqlCommand cmd = new MySqlCommand(updateOrder, conn))
                        {
                            cmd.Parameters.AddWithValue("@TransId", transactionId);
                            cmd.Parameters.AddWithValue("@OrderId", orderIdStr);
                            cmd.ExecuteNonQuery();
                        }

                        // ---> AQUÍ IRÍA TU LÓGICA DE DESCONTAR STOCK <---
                        // Idealmente, harías un SELECT de los items de esa orden y les restarías el stock
                    }
                }

                // 5. Responder a Virtual Wallet que recibimos el mensaje con un código 200 OK
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"status\":\"received\"}");
            }
            catch (Exception ex)
            {
                // Si algo falla, devolver un error 500
                context.Response.StatusCode = 500;
                context.Response.Write("Error interno: " + ex.Message);
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}