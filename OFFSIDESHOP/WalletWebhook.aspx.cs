using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace OFFSIDESHOP
{
    public partial class WalletWebhook : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        private static System.Collections.Concurrent.ConcurrentDictionary<int, string> OrderReturnUrls = new System.Collections.Concurrent.ConcurrentDictionary<int, string>();
        private static System.Collections.Concurrent.ConcurrentDictionary<string, string> TxReturnUrls = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();

        public static void RegisterReturnUrl(int orderId, string txId, string returnUrl)
        {
            if (orderId > 0 && !string.IsNullOrEmpty(returnUrl))
            {
                OrderReturnUrls[orderId] = returnUrl;
            }
            if (!string.IsNullOrEmpty(txId) && !string.IsNullOrEmpty(returnUrl))
            {
                TxReturnUrls[txId] = returnUrl;
            }
        }

        public static string GetReturnUrl(int orderId, string txId)
        {
            if (orderId > 0 && OrderReturnUrls.TryGetValue(orderId, out string url1) && !string.IsNullOrEmpty(url1))
            {
                return url1;
            }
            if (!string.IsNullOrEmpty(txId) && TxReturnUrls.TryGetValue(txId, out string url2) && !string.IsNullOrEmpty(url2))
            {
                return url2;
            }
            return GetFallbackReturnUrl();
        }

        public static string GetFallbackReturnUrl()
        {
            string localBase = System.Configuration.ConfigurationManager.AppSettings["LocalAppBaseUrl"];
            if (!string.IsNullOrEmpty(localBase))
            {
                if (!localBase.EndsWith("/")) localBase += "/";
                return localBase + "MyOrders.aspx";
            }
            return "MyOrders.aspx";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            Response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");

            if (Request.HttpMethod == "OPTIONS")
            {
                Response.StatusCode = 200;
                Response.End();
                return;
            }

            try
            {
                string jsonPayload = string.Empty;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    jsonPayload = reader.ReadToEnd();
                }

                // Log opcional para auditoría
                try
                {
                    string logPath = Server.MapPath("~/webhook_log.txt");
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.WriteLine("--- NEW WEBHOOK REQUEST ---");
                        sw.WriteLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        sw.WriteLine("Method: " + Request.HttpMethod);
                        sw.WriteLine("URL: " + Request.Url.ToString());
                        sw.WriteLine("QueryString: " + Request.QueryString.ToString());
                        sw.WriteLine("Raw Body: " + jsonPayload);
                        sw.WriteLine("---------------------------\n");
                    }
                }
                catch { }

                // 1. Redirección del navegador desde el widget de la billetera virtual (GET con parámetros)
                if (Request.HttpMethod == "GET" && Request.QueryString.Count > 0)
                {
                    ProcessFrontendRedirect();
                    return;
                }

                // 2. Ping simple GET o petición vacía
                if (string.IsNullOrWhiteSpace(jsonPayload))
                {
                    Response.ContentType = "application/json";
                    Response.StatusCode = 200;
                    Response.Write("{\"status\":\"ping_ok\"}");
                    Response.End();
                    return;
                }

                // 3. Petición POST con payload JSON
                Response.ContentType = "application/json";
                JObject webhookData = JObject.Parse(jsonPayload);

                if (webhookData["is_frontend"] != null && webhookData["is_frontend"].ToObject<bool>())
                {
                    ProcessFrontendAjax(webhookData);
                    return;
                }

                ProcessBackendWebhook(webhookData);
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Manejo normal de Response.End()
            }
            catch (Exception ex)
            {
                if (Request.HttpMethod == "GET")
                {
                    Response.Clear();
                    Response.ContentType = "text/html";
                    string safeErr = HttpUtility.JavaScriptStringEncode(ex.Message);
                    Response.Write($"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script><script>window.onload = function() {{ Swal.fire('Error', '{safeErr}', 'error').then(() => {{ window.location.href='Checkout.aspx'; }}); }};</script>");
                }
                else
                {
                    Response.StatusCode = 500;
                    Response.ContentType = "application/json";
                    Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
                }
            }
            Response.End();
        }

        private void ProcessFrontendRedirect()
        {
            string getTxId = Request.QueryString["intent_id"] 
                          ?? Request.QueryString["transaction_id"] 
                          ?? Request.QueryString["tx_id"] 
                          ?? Request.QueryString["id"] 
                          ?? Request.QueryString["reference"]
                          ?? ("VW-" + DateTime.Now.Ticks);

            int userId = Session["Id_User"] != null ? Convert.ToInt32(Session["Id_User"]) : 0;

            // 1. Si la orden ya fue procesada y marcada como Pagada con este TransactionID
            if (IsOrderAlreadyCreated(getTxId))
            {
                Session.Remove("Cart");
                Session.Remove("CheckoutData");
                string returnUrl = GetReturnUrl(0, getTxId);
                RenderSuccessHtmlResponse(returnUrl);
                return;
            }

            // 2. Si existe una orden pendiente (Id_Status = 1) en base de datos para este usuario o reciente
            int pendingOrderId = FindPendingWalletOrderId(userId);
            if (pendingOrderId > 0)
            {
                bool confirmed = ConfirmPendingOrder(pendingOrderId, getTxId);
                if (confirmed)
                {
                    Session.Remove("Cart");
                    Session.Remove("CheckoutData");
                    Session.Remove("CouponId");
                    Session.Remove("DiscountAmount");
                    Session.Remove("DiscountPercentage");
                    string returnUrl = GetReturnUrl(pendingOrderId, getTxId);
                    RenderSuccessHtmlResponse(returnUrl);
                    return;
                }
            }

            // 3. Si no había orden pendiente pero el carrito aún está en sesión
            DataTable dtCart = Session["Cart"] as DataTable;
            var checkoutData = Session["CheckoutData"] as Dictionary<string, string>;

            if (dtCart != null && dtCart.Rows.Count > 0)
            {
                if (checkoutData == null)
                {
                    checkoutData = LoadFallbackUserData();
                }

                decimal total = 0m;
                if (checkoutData.ContainsKey("total") && !string.IsNullOrEmpty(checkoutData["total"]))
                {
                    decimal.TryParse(checkoutData["total"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out total);
                }

                InsertOrder(dtCart, checkoutData, getTxId, total, true);
                return;
            }

            // 4. Si el carrito ya se vació o no hay pendientes, buscar cualquier orden reciente de billetera virtual
            int recentOrderId = FindRecentWalletOrderId(userId);
            if (recentOrderId > 0)
            {
                Session.Remove("Cart");
                string returnUrl = GetReturnUrl(recentOrderId, getTxId);
                RenderSuccessHtmlResponse(returnUrl);
                return;
            }

            // 5. Redirección final con éxito al origen local
            RenderSuccessHtmlResponse(GetReturnUrl(0, getTxId));
        }

        private void ProcessFrontendAjax(JObject webhookData)
        {
            DataTable dtCart = Session["Cart"] as DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0)
            {
                Response.StatusCode = 400;
                Response.Write("{\"error\":\"Cart is empty\"}");
                Response.End();
                return;
            }

            string txId = webhookData["transaction_id"]?.ToString() 
                       ?? webhookData["intent_id"]?.ToString() 
                       ?? ("VW-" + DateTime.Now.Ticks);

            decimal total = 0m;
            if (webhookData["total"] != null)
            {
                decimal.TryParse(webhookData["total"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out total);
            }

            var checkoutData = new Dictionary<string, string>();
            checkoutData["lat"] = webhookData["lat"]?.ToString();
            checkoutData["lng"] = webhookData["lng"]?.ToString();
            checkoutData["notes"] = webhookData["notes"]?.ToString();
            checkoutData["name"] = webhookData["name"]?.ToString();
            checkoutData["lastName"] = webhookData["lastName"]?.ToString();
            checkoutData["email"] = webhookData["email"]?.ToString();
            checkoutData["address"] = webhookData["address"]?.ToString();
            checkoutData["tel"] = webhookData["tel"]?.ToString();
            checkoutData["city"] = webhookData["city"]?.ToString();
            checkoutData["municipality"] = webhookData["municipality"]?.ToString();
            checkoutData["district"] = webhookData["district"]?.ToString();

            InsertOrder(dtCart, checkoutData, txId, total, false);
        }

        private void InsertOrder(DataTable dtCart, Dictionary<string, string> checkoutData, string txId, decimal total, bool isHtmlResponse)
        {
            int orderId = 0;
            int userId = Session["Id_User"] != null ? Convert.ToInt32(Session["Id_User"]) : 0;
            object idCoupon = Session["CouponId"] ?? DBNull.Value;
            decimal discountApplied = Session["DiscountAmount"] != null ? Convert.ToDecimal(Session["DiscountAmount"], System.Globalization.CultureInfo.InvariantCulture) : 0m;

            string idCity = checkoutData.ContainsKey("city") ? checkoutData["city"] : "";
            string idMun = checkoutData.ContainsKey("municipality") ? checkoutData["municipality"] : "";
            string idDist = checkoutData.ContainsKey("district") ? checkoutData["district"] : "";

            decimal shippingCost = CalcularCostoEnvio(idCity);

            // Si el total no venía calculado, calcularlo aquí
            if (total <= 0)
            {
                decimal subtotalCamisetas = 0m;
                foreach (DataRow r in dtCart.Rows)
                {
                    subtotalCamisetas += Convert.ToDecimal(r["Subtotal"]);
                }
                total = Math.Max(0m, (subtotalCamisetas - discountApplied) + shippingCost);
            }

            decimal mapLat = 13.6929m;
            decimal mapLng = -89.2182m;
            if (checkoutData.ContainsKey("lat") && !string.IsNullOrEmpty(checkoutData["lat"]))
            {
                decimal.TryParse(checkoutData["lat"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mapLat);
            }
            if (checkoutData.ContainsKey("lng") && !string.IsNullOrEmpty(checkoutData["lng"]))
            {
                decimal.TryParse(checkoutData["lng"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mapLng);
            }

            string safeNotes = checkoutData.ContainsKey("notes") && checkoutData["notes"] != null ? checkoutData["notes"] : "";
            if (safeNotes.Length > 200) safeNotes = safeNotes.Substring(0, 200);

            string safeName = checkoutData.ContainsKey("name") && checkoutData["name"] != null ? checkoutData["name"].Trim() : "";
            if (safeName.Length > 50) safeName = safeName.Substring(0, 50);

            string safeLastName = checkoutData.ContainsKey("lastName") && checkoutData["lastName"] != null ? checkoutData["lastName"].Trim() : "";
            if (safeLastName.Length > 50) safeLastName = safeLastName.Substring(0, 50);

            string email = checkoutData.ContainsKey("email") ? checkoutData["email"].Trim() : "";
            string address = checkoutData.ContainsKey("address") ? checkoutData["address"].Trim() : "";
            if (address.Length > 200) address = address.Substring(0, 200);

            string tel = checkoutData.ContainsKey("tel") ? checkoutData["tel"].Trim() : "";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // 1. Verificación de Stock previo a la inserción
                var groupedCart = dtCart.AsEnumerable()
                    .GroupBy(r => new { Id = r.Field<int>("ID"), Size = r.Field<string>("Size"), Name = r.Field<string>("Name") })
                    .Select(g => new
                    {
                        IdTshirt = g.Key.Id,
                        SizeName = g.Key.Size,
                        ProductName = g.Key.Name,
                        TotalRequestedQty = g.Sum(r => r.Field<int>("Quantity"))
                    }).ToList();

                foreach (var item in groupedCart)
                {
                    string checkStockQuery = @"SELECT Stock FROM tshirt_variants WHERE Id_Tshirt = @IdTshirt AND Id_Size = (SELECT Id_Size FROM sizes WHERE Size_Code = @Size)";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkStockQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@IdTshirt", item.IdTshirt);
                        checkCmd.Parameters.AddWithValue("@Size", item.SizeName);

                        object stockObj = checkCmd.ExecuteScalar();
                        int currentStock = (stockObj != null) ? Convert.ToInt32(stockObj) : 0;

                        if (currentStock < item.TotalRequestedQty)
                        {
                            string outOfStockMsg = $"Disculpas, la camiseta \"{item.ProductName}\" en talla {item.SizeName} solo tiene {currentStock} unidades disponibles.";
                            if (isHtmlResponse)
                            {
                                Response.Clear();
                                Response.ContentType = "text/html";
                                Response.Write($"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script><script>window.onload = function() {{ Swal.fire('Sin Stock Suficiente', '{HttpUtility.JavaScriptStringEncode(outOfStockMsg)}', 'error').then(() => {{ window.location.href='Checkout.aspx'; }}); }};</script>");
                                Response.End();
                            }
                            else
                            {
                                Response.StatusCode = 400;
                                Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(outOfStockMsg) + "\"}");
                                Response.End();
                            }
                            return;
                        }
                    }
                }

                // 2. Transacción de Inserción
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string orderQuery = @"INSERT INTO orders 
                         (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, TransactionID, shipping_cost, Id_Status) 
                         VALUES 
                         (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, @IdPaymentMethod, @TransactionID, @ShippingCost, @IdStatus); 
                         SELECT LAST_INSERT_ID();";

                        using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@IdUser", userId);
                            cmd.Parameters.AddWithValue("@Name", safeName);
                            cmd.Parameters.AddWithValue("@LastName", safeLastName);
                            cmd.Parameters.AddWithValue("@Mail", email);
                            cmd.Parameters.AddWithValue("@Address", address);
                            cmd.Parameters.AddWithValue("@Lat", mapLat);
                            cmd.Parameters.AddWithValue("@Lng", mapLng);
                            object valCity = int.TryParse(idCity, out int cityId) ? (object)cityId : DBNull.Value;
                            object valMun = int.TryParse(idMun, out int munId) ? (object)munId : DBNull.Value;
                            object valDist = int.TryParse(idDist, out int distId) ? (object)distId : DBNull.Value;

                            cmd.Parameters.AddWithValue("@IdCity", valCity);
                            cmd.Parameters.AddWithValue("@IdMunicipality", valMun);
                            cmd.Parameters.AddWithValue("@IdDistrict", valDist);
                            cmd.Parameters.AddWithValue("@Phone", tel);
                            cmd.Parameters.AddWithValue("@Notes", safeNotes);
                            cmd.Parameters.AddWithValue("@Total", total);
                            cmd.Parameters.AddWithValue("@IdCoupon", idCoupon);
                            cmd.Parameters.AddWithValue("@DiscountApplied", discountApplied);
                            cmd.Parameters.AddWithValue("@IdPaymentMethod", 3); // 3 = Billetera Virtual
                            cmd.Parameters.AddWithValue("@TransactionID", string.IsNullOrEmpty(txId) ? (object)DBNull.Value : txId);
                            cmd.Parameters.AddWithValue("@ShippingCost", shippingCost);
                            cmd.Parameters.AddWithValue("@IdStatus", 2); // 2 = Pagado (Paid)

                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Insertar detalles de la orden y descontar stock
                        string detailQuery = @"INSERT INTO order_details (Id_Order, Id_Tshirt, ProductName, Size, Price, Quantity, Subtotal) 
                                               VALUES (@IdOrder, @IdTshirt, @Name, @Size, @Price, @Qty, @Subtotal)";

                        using (MySqlCommand detailCmd = new MySqlCommand(detailQuery, conn, trans))
                        {
                            detailCmd.Parameters.Add("@IdOrder", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@IdTshirt", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@Name", MySqlDbType.VarChar);
                            detailCmd.Parameters.Add("@Size", MySqlDbType.VarChar);
                            detailCmd.Parameters.Add("@Price", MySqlDbType.Decimal);
                            detailCmd.Parameters.Add("@Qty", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@Subtotal", MySqlDbType.Decimal);

                            foreach (DataRow row in dtCart.Rows)
                            {
                                string updateStock = @"UPDATE tshirt_variants SET Stock = Stock - @Qty WHERE Id_Tshirt = @IdTshirt AND Id_Size = (SELECT Id_Size FROM sizes WHERE Size_Code = @Size)";
                                using (MySqlCommand stockCmd = new MySqlCommand(updateStock, conn, trans))
                                {
                                    stockCmd.Parameters.AddWithValue("@Qty", row["Quantity"]);
                                    stockCmd.Parameters.AddWithValue("@IdTshirt", row["ID"]);
                                    stockCmd.Parameters.AddWithValue("@Size", row["Size"]);
                                    stockCmd.ExecuteNonQuery();
                                }

                                string dbProductName = row["Name"].ToString();
                                if (dtCart.Columns.Contains("IsCustomized") && row["IsCustomized"] != DBNull.Value && Convert.ToBoolean(row["IsCustomized"]))
                                {
                                    string customLabel = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es") ? "Personalizado" : "Customized";
                                    dbProductName += $" ({customLabel}: {row["CustomName"]} #{row["CustomNumber"]})";
                                }

                                detailCmd.Parameters["@IdOrder"].Value = orderId;
                                detailCmd.Parameters["@IdTshirt"].Value = row["ID"];
                                detailCmd.Parameters["@Name"].Value = dbProductName;
                                detailCmd.Parameters["@Size"].Value = row["Size"];
                                detailCmd.Parameters["@Price"].Value = row["Price"];
                                detailCmd.Parameters["@Qty"].Value = row["Quantity"];
                                detailCmd.Parameters["@Subtotal"].Value = row["Subtotal"];
                                detailCmd.ExecuteNonQuery();
                            }
                        }

                        // Actualizar cupón si aplica
                        if (idCoupon != DBNull.Value)
                        {
                            string updateCoupon = "UPDATE coupons SET UsedCount = UsedCount + 1 WHERE Id_Coupon = @IdCoupon;";
                            using (MySqlCommand cmdCoupon = new MySqlCommand(updateCoupon, conn, trans))
                            {
                                cmdCoupon.Parameters.AddWithValue("@IdCoupon", idCoupon);
                                cmdCoupon.ExecuteNonQuery();
                            }
                        }

                        // Actualizar datos del usuario si está registrado
                        if (userId > 0)
                        {
                            string updateUser = @"UPDATE users SET Name = @Name, LastName = @LastName, Phone = @Phone, Address = @Address, id_city = @IdCity, Id_Municipality = @IdMun, Id_District = @IdDist WHERE Id_User = @UserId";
                            using (MySqlCommand cmdUser = new MySqlCommand(updateUser, conn, trans))
                            {
                                cmdUser.Parameters.AddWithValue("@Name", safeName);
                                cmdUser.Parameters.AddWithValue("@LastName", safeLastName);
                                cmdUser.Parameters.AddWithValue("@Phone", tel);
                                cmdUser.Parameters.AddWithValue("@Address", address);
                                cmdUser.Parameters.AddWithValue("@IdCity", string.IsNullOrEmpty(idCity) ? (object)DBNull.Value : idCity);
                                cmdUser.Parameters.AddWithValue("@IdMun", string.IsNullOrEmpty(idMun) ? (object)DBNull.Value : idMun);
                                cmdUser.Parameters.AddWithValue("@IdDist", string.IsNullOrEmpty(idDist) ? (object)DBNull.Value : idDist);
                                cmdUser.Parameters.AddWithValue("@UserId", userId);
                                cmdUser.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        if (isHtmlResponse)
                        {
                            Response.Clear();
                            Response.ContentType = "text/html";
                            Response.Write($"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script><script>window.onload = function() {{ Swal.fire('Error al procesar pedido', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error').then(() => {{ window.location.href='Checkout.aspx'; }}); }};</script>");
                            Response.End();
                        }
                        else
                        {
                            Response.StatusCode = 500;
                            Response.Write("{\"error\":\"Database Error: " + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
                            Response.End();
                        }
                        return;
                    }
                }
            }

            // Limpieza de Sesión
            Session.Remove("Cart");
            Session.Remove("CheckoutData");
            Session.Remove("CouponId");
            Session.Remove("DiscountAmount");
            Session.Remove("DiscountPercentage");

            if (isHtmlResponse)
            {
                string retUrl = checkoutData.ContainsKey("returnUrl") && !string.IsNullOrEmpty(checkoutData["returnUrl"]) 
                    ? checkoutData["returnUrl"] 
                    : GetReturnUrl(orderId, txId);
                RenderSuccessHtmlResponse(retUrl);
            }
            else
            {
                Response.StatusCode = 200;
                Response.Write("{\"status\":\"success\"}");
                Response.End();
            }
        }

        private void RenderSuccessHtmlResponse(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = GetFallbackReturnUrl();
            }

            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            string title = isSpanish ? "¡Pago y Pedido Completados!" : "Payment & Order Completed!";
            string text = isSpanish 
                ? "Tu pedido ha sido verificado con éxito a través de Virtual Wallet." 
                : "Your order has been verified successfully via Virtual Wallet.";

            Response.Clear();
            Response.ContentType = "text/html";
            Response.Write($@"<!DOCTYPE html>
<html lang='{(isSpanish ? "es" : "en")}'>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1' />
    <title>{HttpUtility.HtmlEncode(title)}</title>
    <script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
    <link href='https://fonts.googleapis.com/css?family=Montserrat:400,700' rel='stylesheet' type='text/css' />
    <style>
        body {{
            background-color: #0f172a;
            color: #ffffff;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            font-family: 'Montserrat', sans-serif;
        }}
    </style>
</head>
<body>
    <script>
        Swal.fire({{
            title: '{HttpUtility.JavaScriptStringEncode(title)}',
            text: '{HttpUtility.JavaScriptStringEncode(text)}',
            icon: 'success',
            confirmButtonColor: '#FFC800',
            confirmButtonText: 'OK',
            allowOutsideClick: false,
            allowEscapeKey: false
        }}).then(() => {{
            window.location.href = '{HttpUtility.JavaScriptStringEncode(returnUrl)}';
        }});
    </script>
</body>
</html>");
            Response.End();
        }

        private bool IsOrderAlreadyCreated(string txId)
        {
            if (string.IsNullOrEmpty(txId)) return false;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(1) FROM orders WHERE TransactionID = @TransId;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransId", txId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private Dictionary<string, string> LoadFallbackUserData()
        {
            var dict = new Dictionary<string, string>();
            if (Session["Id_User"] == null) return dict;

            int userId = Convert.ToInt32(Session["Id_User"]);
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Name, LastName, Mail, Phone, Address, id_city, Id_Municipality, Id_District, Default_Latitude, Default_Longitude FROM users WHERE Id_User = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                dict["name"] = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : "";
                                dict["lastName"] = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : "";
                                dict["email"] = reader["Mail"] != DBNull.Value ? reader["Mail"].ToString() : "";
                                dict["tel"] = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "";
                                dict["address"] = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : "";
                                dict["city"] = reader["id_city"] != DBNull.Value ? reader["id_city"].ToString() : "";
                                dict["municipality"] = reader["Id_Municipality"] != DBNull.Value ? reader["Id_Municipality"].ToString() : "";
                                dict["district"] = reader["Id_District"] != DBNull.Value ? reader["Id_District"].ToString() : "";
                                dict["lat"] = reader["Default_Latitude"] != DBNull.Value ? reader["Default_Latitude"].ToString() : "13.6929";
                                dict["lng"] = reader["Default_Longitude"] != DBNull.Value ? reader["Default_Longitude"].ToString() : "-89.2182";
                                dict["notes"] = "";
                                dict["total"] = "0.00";
                            }
                        }
                    }
                }
            }
            catch { }

            return dict;
        }

        private decimal CalcularCostoEnvio(string idCity)
        {
            if (string.IsNullOrEmpty(idCity)) return 3.50m;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT shipping_cost FROM cities WHERE id_city = @id_city;";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id_city", idCity);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch { }

            return 3.50m;
        }

        private int FindPendingWalletOrderId(int userId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT Id_Order FROM orders 
                                     WHERE Id_PaymentMethod = 3 
                                       AND Id_Status = 1 
                                       AND (@UserId = 0 OR Id_User = @UserId)
                                       AND OrderDate >= DATE_SUB(NOW(), INTERVAL 1 HOUR)
                                     ORDER BY Id_Order DESC LIMIT 1;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch { }
            return 0;
        }

        private bool ConfirmPendingOrder(int orderId, string txId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        // 1. Actualizar orden a Pagado (2) y registrar TransactionID
                        string updateOrder = @"UPDATE orders 
                                              SET Id_Status = 2, 
                                                  TransactionID = @TransId 
                                              WHERE Id_Order = @OrderId AND Id_Status = 1;";
                        using (MySqlCommand cmd = new MySqlCommand(updateOrder, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@TransId", txId);
                            cmd.Parameters.AddWithValue("@OrderId", orderId);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                trans.Rollback();
                                return false;
                            }
                        }

                        // 2. Descontar stock
                        string updateStock = @"
                            UPDATE tshirt_variants tv
                            INNER JOIN sizes s ON tv.Id_Size = s.Id_Size
                            INNER JOIN order_details od ON tv.Id_Tshirt = od.Id_Tshirt 
                                AND (s.Size_Code = od.Size OR tv.Id_Size = od.Id_Size)
                            SET tv.Stock = GREATEST(0, tv.Stock - od.Quantity)
                            WHERE od.Id_Order = @OrderId;";
                        using (MySqlCommand cmdStock = new MySqlCommand(updateStock, conn, trans))
                        {
                            cmdStock.Parameters.AddWithValue("@OrderId", orderId);
                            cmdStock.ExecuteNonQuery();
                        }

                        // 3. Actualizar cupón si aplica
                        string updateCoupon = @"UPDATE coupons c 
                                               INNER JOIN orders o ON o.Id_Coupon = c.Id_Coupon 
                                               SET c.UsedCount = c.UsedCount + 1 
                                               WHERE o.Id_Order = @OrderId AND o.Id_Coupon IS NOT NULL;";
                        using (MySqlCommand cmdCoupon = new MySqlCommand(updateCoupon, conn, trans))
                        {
                            cmdCoupon.Parameters.AddWithValue("@OrderId", orderId);
                            cmdCoupon.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private int FindRecentWalletOrderId(int userId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT Id_Order FROM orders 
                                     WHERE Id_PaymentMethod = 3 
                                       AND (@UserId = 0 OR Id_User = @UserId)
                                       AND OrderDate >= DATE_SUB(NOW(), INTERVAL 15 MINUTE)
                                     ORDER BY Id_Order DESC LIMIT 1;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch { }
            return 0;
        }

        private void ProcessBackendWebhook(JObject webhookData)
        {
            string paymentStatus = webhookData["status"]?.ToString()?.ToLower();
            string transactionId = webhookData["transaction_id"]?.ToString() 
                                ?? webhookData["tx_id"]?.ToString() 
                                ?? webhookData["intent_id"]?.ToString() 
                                ?? webhookData["id"]?.ToString();
            string referenceStr = webhookData["reference"]?.ToString() ?? webhookData["order_id"]?.ToString();

            int.TryParse(referenceStr, out int orderIdFromRef);

            if (paymentStatus == "success" || paymentStatus == "paid" || paymentStatus == "completed")
            {
                int pendingId = orderIdFromRef > 0 ? orderIdFromRef : FindPendingWalletOrderId(0);
                if (pendingId > 0)
                {
                    ConfirmPendingOrder(pendingId, transactionId);
                }
            }

            Response.StatusCode = 200;
            Response.Write("{\"status\":\"received\"}");
        }
    }
}