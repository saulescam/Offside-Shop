using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class ManageOrders : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;
        private static readonly HttpClient httpClient = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (Session["UserRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            int role = Convert.ToInt32(Session["UserRole"]);
            if (role != 1 && role != 2)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!Security.HasPermission(Session, "Perm_Orders"))
            {
                Response.Redirect("Dashboard.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                ViewState["ActiveTab"] = "ORDERS";
                LoadFilterStatuses();
                LoadOrders();
                UpdateRefundBadgeCount();

                // DETECCIÓN DE REDIRECCIÓN EXTERNA (?id=XX)
                if (Request.QueryString["id"] != null)
                {
                    if (int.TryParse(Request.QueryString["id"], out int directOrderId))
                    {
                        ShowOrderDetailsModal(directOrderId);
                    }
                }
            }
        }

        #region DIRECT ORDER DETAILS VIEW ENGINE Y CONTROL DE SCROLL
        private void LockBackgroundScroll()
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "LockBodyScroll", "document.body.style.overflow = 'hidden';", true);
        }

        private void UnlockBackgroundScroll()
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "UnlockBodyScroll", "document.body.style.overflow = '';", true);
        }

        protected void lnkViewDetails_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            int orderId = Convert.ToInt32(btn.CommandArgument);
            ShowOrderDetailsModal(orderId);
        }

        private void ShowOrderDetailsModal(int orderId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string orderQuery = @"SELECT o.Id_Order, CONCAT(o.Name, ' ', o.LastName) AS CustomerName, 
                                                 o.Mail, o.Phone, o.Address, o.Total, o.shipping_cost, o.OrderNotes,
                                                 c.city_name, m.Municipality_Name, d.District_Name
                                          FROM orders o
                                          LEFT JOIN cities c ON o.Id_City = c.id_city
                                          LEFT JOIN municipalities m ON o.Id_Municipality = m.Id_Municipality
                                          LEFT JOIN districts d ON o.Id_District = d.Id_District
                                          WHERE o.Id_Order = @OrderId;";

                    using (MySqlCommand cmd = new MySqlCommand(orderQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litDetOrderId.Text = reader["Id_Order"].ToString();
                                litDetCustomer.Text = reader["CustomerName"].ToString();
                                litDetEmail.Text = reader["Mail"].ToString();
                                litDetPhone.Text = reader["Phone"].ToString();
                                litDetAddress.Text = reader["Address"].ToString();
                                litDetNotes.Text = string.IsNullOrEmpty(reader["OrderNotes"].ToString()) ? "No delivery instructions provided." : reader["OrderNotes"].ToString();

                                litDetShipping.Text = Convert.ToDecimal(reader["shipping_cost"]).ToString("C");
                                litDetTotal.Text = Convert.ToDecimal(reader["Total"]).ToString("C");

                                litDetLocation.Text = $"{reader["city_name"]}, {reader["Municipality_Name"]}, {reader["District_Name"]}";
                            }
                            else
                            {
                                TriggerSweetAlert("Not Found", "The requested order document reference does not exist.", "warning");
                                return;
                            }
                        }
                    }

                    string detailsQuery = "SELECT ProductName, Size, Price, Quantity, Subtotal, CustomName, CustomNumber FROM order_details WHERE Id_Order = @OrderId;";
                    using (MySqlCommand cmdDetails = new MySqlCommand(detailsQuery, con))
                    {
                        cmdDetails.Parameters.AddWithValue("@OrderId", orderId);
                        DataTable dtItems = new DataTable();
                        new MySqlDataAdapter(cmdDetails).Fill(dtItems);

                        gvOrderDetailItems.DataSource = dtItems;
                        gvOrderDetailItems.DataBind();
                    }

                    phOrderDetailsModal.Visible = true;
                    LockBackgroundScroll(); // Bloquea el scroll del fondo
                }
            }
            catch (Exception ex)
            {
                TriggerSweetAlert("Error", $"Could not load order details breakdown: {ex.Message}", "error");
            }
        }

        protected void btnCloseOrderDetails_Click(object sender, EventArgs e)
        {
            phOrderDetailsModal.Visible = false;
            UnlockBackgroundScroll(); // Libera el scroll del fondo
        }
        #endregion

        #region NAVIGATION TABS LOGIC
        protected void btnTabOrders_Click(object sender, EventArgs e)
        {
            ViewState["ActiveTab"] = "ORDERS";
            btnTabOrders.CssClass = "nav-link active";
            btnTabRefunds.CssClass = "nav-link";
            phOrdersView.Visible = true;
            phRefundsView.Visible = false;
            phRefundModal.Visible = false;
            phOrderDetailsModal.Visible = false;
            LoadOrders();
            UpdateRefundBadgeCount();
        }

        protected void btnTabRefunds_Click(object sender, EventArgs e)
        {
            ViewState["ActiveTab"] = "REFUNDS";
            btnTabOrders.CssClass = "nav-link";
            btnTabRefunds.CssClass = "nav-link active";
            phOrdersView.Visible = false;
            phRefundsView.Visible = true;
            phRefundModal.Visible = false;
            phOrderDetailsModal.Visible = false;
            LoadRefundTickets();
            UpdateRefundBadgeCount();
        }

        private void UpdateRefundBadgeCount()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(1) FROM orders WHERE Id_Status = 6;";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        litRefundBadgeCount.Text = count.ToString();
                    }
                }
            }
            catch (Exception)
            {
                litRefundBadgeCount.Text = "0";
            }
        }
        #endregion

        #region STANDARD ORDERS RENDERING, FILTERING AND PAGING
        private void LoadFilterStatuses()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT Id_Status, Status_Name FROM order_statuses ORDER BY Id_Status ASC;", con);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    ddlFilterStatus.Items.Clear();
                    ddlFilterStatus.Items.Add(new ListItem("-- All Statuses --", "0"));
                    foreach (DataRow row in dt.Rows)
                    {
                        string idStat = row["Id_Status"].ToString();
                        if (idStat != "6" && idStat != "7" && idStat != "8")
                        {
                            ddlFilterStatus.Items.Add(new ListItem(row["Status_Name"].ToString(), idStat));
                        }
                    }
                }
            }
            catch (Exception)
            {
                ddlFilterStatus.Items.Clear();
                ddlFilterStatus.Items.Add(new ListItem("-- All Statuses --", "0"));
            }
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            gvOrders.PageIndex = 0; // Regresa a la primera página al aplicar un nuevo filtro
            LoadOrders();
        }

        protected void LoadOrders()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT o.Id_Order, o.OrderDate, o.Total, o.Id_Status,
                                            CONCAT(o.Name, ' ', o.LastName) AS CustomerName, o.Mail, s.Status_Name,
                                            IFNULL(c.city_name, '') AS City_Name
                                     FROM orders o
                                     INNER JOIN order_statuses s ON o.Id_Status = s.Id_Status
                                     LEFT JOIN cities c ON o.Id_City = c.id_city
                                     WHERE o.Id_Status NOT IN (6, 7, 8)";

                    MySqlCommand cmd = new MySqlCommand();

                    // Aplicar Filtro de Estado
                    if (ddlFilterStatus.SelectedValue != "0" && !string.IsNullOrEmpty(ddlFilterStatus.SelectedValue))
                    {
                        query += " AND o.Id_Status = @StatusId";
                        cmd.Parameters.AddWithValue("@StatusId", Convert.ToInt32(ddlFilterStatus.SelectedValue));
                    }

                    // Aplicar Filtro de Fecha de Inicio
                    if (!string.IsNullOrEmpty(txtStartDate.Text))
                    {
                        query += " AND DATE(o.OrderDate) >= @StartDate";
                        cmd.Parameters.AddWithValue("@StartDate", Convert.ToDateTime(txtStartDate.Text).ToString("yyyy-MM-dd"));
                    }

                    // Aplicar Filtro de Fecha Final
                    if (!string.IsNullOrEmpty(txtEndDate.Text))
                    {
                        query += " AND DATE(o.OrderDate) <= @EndDate";
                        cmd.Parameters.AddWithValue("@EndDate", Convert.ToDateTime(txtEndDate.Text).ToString("yyyy-MM-dd"));
                    }

                    query += " ORDER BY o.OrderDate DESC;";
                    cmd.CommandText = query;
                    cmd.Connection = con;

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    gvOrders.DataSource = dt;
                    gvOrders.DataBind();
                }
            }
            catch (Exception ex)
            {
                TriggerSweetAlert("Error", $"Error loading systems telemetry orders: {ex.Message}", "error");
            }
        }

        protected void gvOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;
            LoadOrders(); // Recarga la tabla de datos en la página seleccionada
        }

        protected void gvOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlGridStatus = (DropDownList)e.Row.FindControl("ddlGridStatus");
                if (ddlGridStatus != null)
                {
                    DataRowView rowView = (DataRowView)e.Row.DataItem;
                    int currentStatusId = Convert.ToInt32(rowView["Id_Status"]);

                    ListItem item = ddlGridStatus.Items.FindByValue(currentStatusId.ToString());
                    if (item != null)
                    {
                        ddlGridStatus.SelectedValue = currentStatusId.ToString();
                    }
                    else
                    {
                        ddlGridStatus.Items.Add(new ListItem(rowView["Status_Name"].ToString(), currentStatusId.ToString()));
                        ddlGridStatus.SelectedValue = currentStatusId.ToString();
                    }
                }
            }
        }

        protected void ddlGridStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DropDownList ddl = (DropDownList)sender;
                GridViewRow row = (GridViewRow)ddl.NamingContainer;
                int rowIndex = row.RowIndex;

                int orderId = Convert.ToInt32(gvOrders.DataKeys[rowIndex].Value);
                int newStatusId = Convert.ToInt32(ddl.SelectedValue);

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE orders SET Id_Status = @StatusId WHERE Id_Order = @IdOrder;", con);
                    cmd.Parameters.AddWithValue("@StatusId", newStatusId);
                    cmd.Parameters.AddWithValue("@IdOrder", orderId);
                    cmd.ExecuteNonQuery();
                }

                TriggerSweetAlert("Success", "Order status operational scope updated successfully!", "success");
                LoadOrders();
                UpdateRefundBadgeCount();
            }
            catch (Exception ex) { TriggerSweetAlert("Error", $"Error updating administrative order status: {ex.Message}", "error"); }
        }
        #endregion

        #region REFUND PIPELINE RESOLUTION LOGIC
        private void LoadRefundTickets()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT o.Id_Order, CONCAT(o.Name, ' ', o.LastName) AS CustomerName, o.Mail, o.Total,
                                            IFNULL(p.Method_Name, 'Unspecified / Unknown') AS Method_Name,
                                            IFNULL(rc.Reason_Title, 'Missing Reason Ticket Data') AS Reason_Title,
                                            IFNULL(r.Created_At, o.OrderDate) AS Created_At
                                     FROM orders o
                                     LEFT JOIN payment_methods p ON o.Id_PaymentMethod = p.Id_PaymentMethod
                                     LEFT JOIN order_reasons r ON o.Id_Order = r.Id_Order
                                     LEFT JOIN reason_catalog rc ON r.Id_CatalogReason = rc.Id_CatalogReason
                                     WHERE o.Id_Status = 6 ORDER BY o.OrderDate ASC;";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    gvRefunds.DataSource = dt;
                    gvRefunds.DataBind();
                }
            }
            catch (Exception ex) { TriggerSweetAlert("Error", $"Error loading outstanding refund request queues: {ex.Message}", "error"); }
        }

        protected void lnkEvaluateRefund_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            int orderId = Convert.ToInt32(btn.CommandArgument);
            ViewState["TargetEvaluationOrderId"] = orderId;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT o.Id_Order, CONCAT(o.Name, ' ', o.LastName) AS CustomerName, o.Total,
                                             IFNULL(o.Id_PaymentMethod, 0) AS Id_PaymentMethod,
                                             IFNULL(rc.Reason_Title, 'No systemic reason title logged') AS Reason_Title,
                                             IFNULL(r.Reason_Text, 'No additional descriptions provided.') AS Reason_Text
                                     FROM orders o
                                     LEFT JOIN order_reasons r ON o.Id_Order = r.Id_Order
                                     LEFT JOIN reason_catalog rc ON r.Id_CatalogReason = rc.Id_CatalogReason
                                     WHERE o.Id_Order = @IdOrder AND o.Id_Status = 6;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IdOrder", orderId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litModalOrderId.Text = reader["Id_Order"].ToString();
                                litModalCustomer.Text = reader["CustomerName"].ToString();
                                litModalTotal.Text = Convert.ToDecimal(reader["Total"]).ToString("C");
                                litModalReasonTitle.Text = reader["Reason_Title"].ToString();
                                litModalReasonText.Text = reader["Reason_Text"].ToString().Trim();

                                int paymentMethodId = Convert.ToInt32(reader["Id_PaymentMethod"]);
                                ViewState["EvaluationPaymentMethodId"] = paymentMethodId;

                                if (paymentMethodId == 2)
                                {
                                    btnApproveRefund.Text = "<i class='fab fa-paypal mr-1'></i> Execute PayPal Sandbox Refund";
                                    btnApproveRefund.CssClass = "btn btn-primary font-weight-bold px-3";
                                }
                                else
                                {
                                    btnApproveRefund.Text = "<i class='fas fa-check-circle mr-1'></i> Approve Manual Cash Refund";
                                    btnApproveRefund.CssClass = "btn btn-success font-weight-bold px-3";
                                }

                                txtAdminComment.Text = string.Empty;
                                lblModalError.Visible = false;
                                phRefundModal.Visible = true;
                                LockBackgroundScroll(); // Bloquea el scroll
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { TriggerSweetAlert("Error", $"Critical database verification exception: {ex.Message}", "error"); }
        }

        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            phRefundModal.Visible = false;
            UnlockBackgroundScroll(); // Libera el scroll al cancelar o cerrar el ticket de reembolso
        }

        protected async void btnApproveRefund_Click(object sender, EventArgs e)
        {
            if (ViewState["TargetEvaluationOrderId"] == null) return;
            int orderId = Convert.ToInt32(ViewState["TargetEvaluationOrderId"]);
            int paymentMethodId = Convert.ToInt32(ViewState["EvaluationPaymentMethodId"]);
            string resolutionNotes = txtAdminComment.Text.Trim();

            if (string.IsNullOrEmpty(resolutionNotes) || resolutionNotes.Length < 10)
            {
                lblModalError.Text = "Please type a detailed settlement note explaining the structural authorization grounds (minimum 10 characters).";
                lblModalError.Visible = true;
                return;
            }

            string customerEmail = ""; string customerName = ""; decimal refundAmount = 0; string payPalCaptureId = "";

            using (MySqlConnection conFetch = new MySqlConnection(connectionString))
            {
                string fetchSql = "SELECT o.TransactionID, o.Total, u.Mail, u.Name FROM orders o INNER JOIN users u ON o.Id_User = u.Id_User WHERE o.Id_Order = @IdOrder;";
                using (MySqlCommand cmdFetch = new MySqlCommand(fetchSql, conFetch))
                {
                    cmdFetch.Parameters.AddWithValue("@IdOrder", orderId);
                    conFetch.Open();
                    using (MySqlDataReader reader = cmdFetch.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            payPalCaptureId = reader["TransactionID"].ToString().Trim();
                            refundAmount = Convert.ToDecimal(reader["Total"]);
                            customerEmail = reader["Mail"].ToString().Trim();
                            customerName = reader["Name"].ToString().Trim();
                        }
                    }
                }
            }

            if (paymentMethodId == 2)
            {
                if (string.IsNullOrEmpty(payPalCaptureId))
                {
                    lblModalError.Text = "Error: No valid TransactionID (PayPal Capture ID) found in the database.";
                    lblModalError.Visible = true;
                    return;
                }
                bool payPalSuccess = await ExecutePayPalSandboxRefundAPIAsync(payPalCaptureId, refundAmount);
                if (!payPalSuccess)
                {
                    lblModalError.Text = "Error: PayPal SANDBOX API rejected the refund claim.";
                    lblModalError.Visible = true;
                    return;
                }
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                using (MySqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE orders SET Id_Status = 7 WHERE Id_Order = @IdOrder;", con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdOrder", orderId);
                            cmd.ExecuteNonQuery();
                        }
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE order_reasons SET Admin_Comment = @AdminComment, Resolved_At = NOW() WHERE Id_Order = @IdOrder;", con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@AdminComment", resolutionNotes);
                            cmd.Parameters.AddWithValue("@IdOrder", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        DataTable dtDetails = new DataTable();
                        using (MySqlCommand cmdDetails = new MySqlCommand("SELECT Id_Tshirt, Id_Size, Quantity FROM order_details WHERE Id_Order = @IdOrder;", con, transaction))
                        {
                            cmdDetails.Parameters.AddWithValue("@IdOrder", orderId);
                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmdDetails)) { da.Fill(dtDetails); }
                        }

                        foreach (DataRow row in dtDetails.Rows)
                        {
                            if (row["Id_Tshirt"] != DBNull.Value && row["Id_Size"] != DBNull.Value)
                            {
                                using (MySqlCommand cmdStock = new MySqlCommand("UPDATE tshirt_variants SET Stock = Stock + @Qty WHERE Id_Tshirt = @IdTshirt AND Id_Size = @IdSize;", con, transaction))
                                {
                                    cmdStock.Parameters.AddWithValue("@Qty", row["Quantity"]);
                                    cmdStock.Parameters.AddWithValue("@IdTshirt", row["Id_Tshirt"]);
                                    cmdStock.Parameters.AddWithValue("@IdSize", row["Id_Size"]);
                                    cmdStock.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        phRefundModal.Visible = false;
                        UnlockBackgroundScroll();

                        EmailService.SendRefundApprovedNotification(customerEmail, customerName, orderId.ToString(), refundAmount, resolutionNotes);
                        TriggerSweetAlert("Approved", "Sandbox refund authorization cleared successfully.", "success");
                        LoadRefundTickets();
                        UpdateRefundBadgeCount();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        lblModalError.Text = "Exception inside transactional environment: " + ex.Message;
                        lblModalError.Visible = true;
                    }
                }
            }
        }

        private async Task<bool> ExecutePayPalSandboxRefundAPIAsync(string captureId, decimal amount)
        {
            string clientId = System.Configuration.ConfigurationManager.AppSettings["PayPal.ClientId"];
            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["PayPal.ClientSecret"];
            try
            {
                var authBytes = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api-m.sandbox.paypal.com/v1/oauth2/token");
                tokenRequest.Headers.Authorization = authHeader;
                tokenRequest.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var tokenResponse = await httpClient.SendAsync(tokenRequest);
                if (!tokenResponse.IsSuccessStatusCode) return false;

                string tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var serializer = new JavaScriptSerializer();
                var tokenData = serializer.Deserialize<Dictionary<string, object>>(tokenJson);
                string accessToken = tokenData["access_token"].ToString();

                var refundRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api-m.sandbox.paypal.com/v2/payments/captures/{captureId}/refund");
                refundRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var refundBody = new
                {
                    amount = new { value = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), currency_code = "USD" },
                    note_to_payer = "Refund processed successfully by OFFSIDESHOP Administrator Center."
                };

                refundRequest.Content = new StringContent(serializer.Serialize(refundBody), Encoding.UTF8, "application/json");
                var refundResponse = await httpClient.SendAsync(refundRequest);
                if (refundResponse.IsSuccessStatusCode)
                {
                    string responseJson = await refundResponse.Content.ReadAsStringAsync();
                    var refundData = serializer.Deserialize<Dictionary<string, object>>(responseJson);
                    string status = refundData["status"].ToString();
                    return (status == "COMPLETED" || status == "PENDING");
                }
                return false;
            }
            catch { return false; }
        }

        protected void btnRejectRefund_Click(object sender, EventArgs e)
        {
            if (ViewState["TargetEvaluationOrderId"] == null) return;
            int orderId = Convert.ToInt32(ViewState["TargetEvaluationOrderId"]);
            string resolutionNotes = txtAdminComment.Text.Trim();

            if (string.IsNullOrEmpty(resolutionNotes) || resolutionNotes.Length < 10)
            {
                lblModalError.Text = "Please write clear justification notes clarifying why the consumer refund claim has been denied.";
                lblModalError.Visible = true;
                return;
            }

            string customerEmail = ""; string customerName = "";
            using (MySqlConnection conFetch = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmdFetch = new MySqlCommand("SELECT u.Mail, u.Name FROM orders o INNER JOIN users u ON o.Id_User = u.Id_User WHERE o.Id_Order = @IdOrder;", conFetch))
                {
                    cmdFetch.Parameters.AddWithValue("@IdOrder", orderId);
                    conFetch.Open();
                    using (MySqlDataReader reader = cmdFetch.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customerEmail = reader["Mail"].ToString().Trim();
                            customerName = reader["Name"].ToString().Trim();
                        }
                    }
                }
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                using (MySqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE orders SET Id_Status = 2 WHERE Id_Order = @IdOrder;", con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdOrder", orderId);
                            cmd.ExecuteNonQuery();
                        }
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE order_reasons SET Admin_Comment = @AdminComment, Resolved_At = NOW() WHERE Id_Order = @IdOrder;", con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@AdminComment", "[DENIED] " + resolutionNotes);
                            cmd.Parameters.AddWithValue("@IdOrder", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        phRefundModal.Visible = false;
                        UnlockBackgroundScroll();

                        EmailService.SendRefundDeniedNotification(customerEmail, customerName, orderId.ToString(), resolutionNotes);
                        TriggerSweetAlert("Ticket Denied", "The request has been officially closed and rejected.", "info");
                        LoadRefundTickets();
                        UpdateRefundBadgeCount();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        lblModalError.Text = "Ledger modification rollback exception: " + ex.Message;
                        lblModalError.Visible = true;
                    }
                }
            }
        }
        #endregion

        #region UTILITY HELPER METHOD WITH SCALED CUSTOM SWEETALERT
        private void TriggerSweetAlert(string title, string text, string icon)
        {
            string cleanTitle = title.Replace("'", "\\'");
            string cleanText = text.Replace("'", "\\'").Replace("\r\n", " ").Replace("\n", " ");
            string script = $@"Swal.fire({{ title: '{cleanTitle}', text: '{cleanText}', icon: '{icon}', confirmButtonColor: '#FFC800', customClass: 'custom-swal-popup' }});";
            string styleSetup = @"var style = document.getElementById('swal-custom-sizes'); if(!style) { style = document.createElement('style'); style.id = 'swal-custom-sizes'; style.innerHTML = `.custom-swal-popup { width: 550px !important; padding: 2.5rem !important; border-radius: 15px !important; } .custom-swal-popup .swal2-title { font-size: 2.2rem !important; font-weight: bold !important; } .custom-swal-popup .swal2-content, .custom-swal-popup .swal2-html-container { font-size: 1.3rem !important; line-height: 1.6 !important; color: #444 !important; } .custom-swal-popup .swal2-confirm { font-size: 1.2rem !important; padding: 12px 30px !important; font-weight: bold !important; }`; document.head.appendChild(style); }";
            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), styleSetup + script, true);
        }
        #endregion

        protected void btnManageProducts_Click(object sender, EventArgs e) { Response.Redirect("ManageProducts.aspx"); }
        protected void btnManageOrders_Click(object sender, EventArgs e) { Response.Redirect("ManageOrders.aspx"); }
        protected void btncerrar_Click(object sender, EventArgs e) { Session.Clear(); Session.Abandon(); Response.Redirect("Login.aspx"); }
        protected void btnAddLeague_Click(object sender, EventArgs e) { Response.Redirect("AddLeague.aspx"); }
        protected void btnAddTeam_Click(object sender, EventArgs e) { Response.Redirect("AddTeam.aspx"); }
        protected void btnManageOffers_Click(object sender, EventArgs e) { Response.Redirect("ManageOffers.aspx"); }
        protected void btnAddBrand_Click(object sender, EventArgs e) { Response.Redirect("AddBrand.aspx"); }
        protected void btnManageUsers_Click(object sender, EventArgs e) { Response.Redirect("ManageUsers.aspx"); }
        protected void btnAdminBanners_Click(object sender, EventArgs e) { Response.Redirect("AdminBanners.aspx"); }
        protected void btnSmtpSettings_Click(object sender, EventArgs e) { Response.Redirect("SmtpSettings.aspx"); }
        protected void btnStats_Click(object sender, EventArgs e) { Response.Redirect("AdminStats.aspx"); }
        protected void btnManageCoupons_Click(object sender, EventArgs e) { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e) { Response.Redirect("AdminAudit.aspx"); }
    }
}