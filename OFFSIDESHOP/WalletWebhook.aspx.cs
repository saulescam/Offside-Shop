using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Web;

namespace OFFSIDESHOP
{
    public partial class WalletWebhook : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "application/json";
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
                string jsonPayload;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    jsonPayload = reader.ReadToEnd();
                }

                try
                {
                    string logPath = Server.MapPath("~/webhook_log.txt");
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.WriteLine("--- NEW WEBHOOK REQUEST ---");
                        sw.WriteLine("Time: " + DateTime.Now.ToString());
                        sw.WriteLine("Method: " + Request.HttpMethod);
                        sw.WriteLine("URL: " + Request.Url.ToString());
                        sw.WriteLine("Headers: " + Request.Headers.ToString());
                        sw.WriteLine("QueryString: " + Request.QueryString.ToString());
                        sw.WriteLine("Form: " + Request.Form.ToString());
                        sw.WriteLine("Raw Body: " + jsonPayload);
                        sw.WriteLine("---------------------------\n");
                    }
                }
                catch { }

                if (string.IsNullOrWhiteSpace(jsonPayload))
                {
                    if (Request.HttpMethod == "GET" && Request.QueryString.Count > 0)
                    {
                        ProcessFrontendRedirect();
                        return;
                    }

                    Response.StatusCode = 200;
                    Response.Write("{\"status\":\"ping_ok\"}");
                    Response.End();
                    return;
                }

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
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write("{\"error\":\"" + HttpUtility.JavaScriptStringEncode(ex.Message) + "\"}");
            }
            Response.End();
        }

        private void ProcessFrontendRedirect()
        {
            string getTxId = Request.QueryString["transaction_id"] ?? Request.QueryString["tx_id"] ?? Request.QueryString["id"] ?? Request.QueryString["reference"];
            
            System.Data.DataTable dtCart = Session["Cart"] as System.Data.DataTable;
            var checkoutData = Session["CheckoutData"] as System.Collections.Generic.Dictionary<string, string>;

            if (dtCart != null && dtCart.Rows.Count > 0 && checkoutData != null)
            {
                decimal total = checkoutData.ContainsKey("total") ? Convert.ToDecimal(checkoutData["total"], System.Globalization.CultureInfo.InvariantCulture) : 0m;
                InsertOrder(dtCart, checkoutData, getTxId, total, true);
            }
            else
            {
                Response.Clear();
                Response.ContentType = "text/html";
                Response.Write("<script>alert('Orden procesada o faltan datos.'); window.location.href='MyOrders.aspx';</script>");
                Response.End();
            }
        }

        private void ProcessFrontendAjax(JObject webhookData)
        {
            System.Data.DataTable dtCart = Session["Cart"] as System.Data.DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0)
            {
                Response.StatusCode = 400;
                Response.Write("{\"error\":\"Cart is empty\"}");
                Response.End();
                return;
            }

            string txId = webhookData["transaction_id"]?.ToString();
            decimal total = webhookData["total"] != null ? Convert.ToDecimal(webhookData["total"].ToString(), System.Globalization.CultureInfo.InvariantCulture) : 0m;
            
            var checkoutData = new System.Collections.Generic.Dictionary<string, string>();
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

        private void InsertOrder(System.Data.DataTable dtCart, System.Collections.Generic.Dictionary<string, string> checkoutData, string txId, decimal total, bool isHtmlResponse)
        {
            decimal mapLat = checkoutData.ContainsKey("lat") && !string.IsNullOrEmpty(checkoutData["lat"]) ? Convert.ToDecimal(checkoutData["lat"], System.Globalization.CultureInfo.InvariantCulture) : 0m;
            decimal mapLng = checkoutData.ContainsKey("lng") && !string.IsNullOrEmpty(checkoutData["lng"]) ? Convert.ToDecimal(checkoutData["lng"], System.Globalization.CultureInfo.InvariantCulture) : 0m;
            int userId = Session["Id_User"] != null ? Convert.ToInt32(Session["Id_User"]) : 0;
            object idCoupon = Session["CouponId"] ?? DBNull.Value;
            decimal discountApplied = Session["DiscountAmount"] != null ? Convert.ToDecimal(Session["DiscountAmount"], System.Globalization.CultureInfo.InvariantCulture) : 0m;
            decimal shippingCost = 3.50m;

            string safeNotes = checkoutData.ContainsKey("notes") && checkoutData["notes"] != null ? checkoutData["notes"] : "";
            if (safeNotes.Length > 200) safeNotes = safeNotes.Substring(0, 200);

            string safeName = checkoutData.ContainsKey("name") && checkoutData["name"] != null ? checkoutData["name"] : "";
            if (safeName.Length > 50) safeName = safeName.Substring(0, 50);

            string safeLastName = checkoutData.ContainsKey("lastName") && checkoutData["lastName"] != null ? checkoutData["lastName"] : "";
            if (safeLastName.Length > 50) safeLastName = safeLastName.Substring(0, 50);

            string email = checkoutData.ContainsKey("email") ? checkoutData["email"] : "";
            string address = checkoutData.ContainsKey("address") ? checkoutData["address"] : "";
            string tel = checkoutData.ContainsKey("tel") ? checkoutData["tel"] : "";
            string idCity = checkoutData.ContainsKey("city") ? checkoutData["city"] : "";
            string idMun = checkoutData.ContainsKey("municipality") ? checkoutData["municipality"] : "";
            string idDist = checkoutData.ContainsKey("district") ? checkoutData["district"] : "";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string orderQuery = @"INSERT INTO orders 
                         (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, TransactionID, shipping_cost, Id_Status) 
                         VALUES 
                         (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, @IdPaymentMethod, @TransactionID, @ShippingCost, @IdStatus); 
                         SELECT LAST_INSERT_ID();";

                        int orderId;
                        using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@IdUser", userId);
                            cmd.Parameters.AddWithValue("@Name", safeName);
                            cmd.Parameters.AddWithValue("@LastName", safeLastName);
                            cmd.Parameters.AddWithValue("@Mail", email);
                            cmd.Parameters.AddWithValue("@Address", address);
                            cmd.Parameters.AddWithValue("@Lat", mapLat);
                            cmd.Parameters.AddWithValue("@Lng", mapLng);
                            cmd.Parameters.AddWithValue("@IdCity", string.IsNullOrEmpty(idCity) ? (object)DBNull.Value : idCity);
                            cmd.Parameters.AddWithValue("@IdMunicipality", string.IsNullOrEmpty(idMun) ? (object)DBNull.Value : idMun);
                            cmd.Parameters.AddWithValue("@IdDistrict", string.IsNullOrEmpty(idDist) ? (object)DBNull.Value : idDist);
                            cmd.Parameters.AddWithValue("@Phone", tel);
                            cmd.Parameters.AddWithValue("@Notes", safeNotes);
                            cmd.Parameters.AddWithValue("@Total", total);
                            cmd.Parameters.AddWithValue("@IdCoupon", idCoupon);
                            cmd.Parameters.AddWithValue("@DiscountApplied", discountApplied);
                            cmd.Parameters.AddWithValue("@IdPaymentMethod", 3);
                            cmd.Parameters.AddWithValue("@TransactionID", string.IsNullOrEmpty(txId) ? (object)DBNull.Value : txId);
                            cmd.Parameters.AddWithValue("@ShippingCost", shippingCost);
                            cmd.Parameters.AddWithValue("@IdStatus", 2);
                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

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

                            foreach (System.Data.DataRow row in dtCart.Rows)
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

                        if (idCoupon != DBNull.Value)
                        {
                            string updateCoupon = "UPDATE coupons SET UsedCount = UsedCount + 1 WHERE Id_Coupon = @IdCoupon;";
                            using (MySqlCommand cmdCoupon = new MySqlCommand(updateCoupon, conn, trans))
                            {
                                cmdCoupon.Parameters.AddWithValue("@IdCoupon", idCoupon);
                                cmdCoupon.ExecuteNonQuery();
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
                            Response.Write($"<script>alert('Error: {HttpUtility.JavaScriptStringEncode(ex.Message)}'); window.location.href='Checkout.aspx';</script>");
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

            Session.Remove("Cart");
            Session.Remove("CouponId");
            Session.Remove("DiscountAmount");

            if (isHtmlResponse)
            {
                Response.Clear();
                Response.ContentType = "text/html";
                Response.Write(@"
<!DOCTYPE html>
<html>
<head>
    <title>Processing Payment...</title>
    <script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
    <style>body { background-color: #0f172a; color: white; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; font-family: sans-serif; }</style>
</head>
<body>
    <h2>Confirmando tu pago...</h2>
    <script>
        Swal.fire({
            title: 'Payment & Order Completed!',
            text: 'Your order has been placed successfully via Virtual Wallet.',
            icon: 'success',
            confirmButtonColor: '#FFC800',
            allowOutsideClick: false
        }).then(() => {
            window.location.href = 'MyOrders.aspx';
        });
    </script>
</body>
</html>");
                Response.End();
            }
            else
            {
                Response.StatusCode = 200;
                Response.Write("{\"status\":\"success\"}");
                Response.End();
            }
        }

        private void ProcessBackendWebhook(JObject webhookData)
        {
            string paymentStatus = webhookData["status"]?.ToString()?.ToLower();
            string transactionId = webhookData["transaction_id"]?.ToString() ?? webhookData["tx_id"]?.ToString() ?? webhookData["id"]?.ToString();
            string referenceStr = webhookData["reference"]?.ToString() ?? webhookData["order_id"]?.ToString();

            int.TryParse(referenceStr, out int orderIdFromRef);

            if (paymentStatus == "success" || paymentStatus == "paid" || paymentStatus == "completed")
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
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

                    if (targetOrderId == 0)
                    {
                        Response.StatusCode = 404;
                        Response.Write("{\"error\":\"Orden no encontrada\"}");
                        Response.End();
                        return;
                    }

                    if (currentStatus == 2)
                    {
                        Response.StatusCode = 200;
                        Response.Write("{\"status\":\"already_processed\"}");
                        Response.End();
                        return;
                    }

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
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

            Response.StatusCode = 200;
            Response.Write("{\"status\":\"received\"}");
        }
    }
}