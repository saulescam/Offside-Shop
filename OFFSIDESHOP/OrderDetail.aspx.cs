using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class OrderDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserRole"] == null || Session["Customer"] == null || Session["Id_User"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            phNavbarGuest.Visible = false;
            phNavbarUser.Visible = false;
            phNavbarAdmin.Visible = false;

            int userRole = Convert.ToInt32(Session["UserRole"]);
            if (userRole == 1 || userRole == 2)
            {
                phNavbarAdmin.Visible = true;
            }
            else if (userRole == 3)
            {
                phNavbarUser.Visible = true;
                if (!IsPostBack)
                {
                    CargarDatosPerfilUsuario();
                }
            }

            if (!IsPostBack)
            {
                ActualizarContadorCarrito();

                if (Request.QueryString["id"] != null)
                {
                    int idOrder;
                    if (int.TryParse(Request.QueryString["id"], out idOrder))
                    {
                        int idUser = Convert.ToInt32(Session["Id_User"]);

                        if (!IsUserOrderOwner(idOrder, idUser))
                        {
                            Response.Redirect("MyOrders.aspx");
                            return;
                        }

                        lblOrderId.Text = idOrder.ToString();
                        ViewState["IdOrder"] = idOrder;
                        LoadOrderDetails(idOrder);
                    }
                    else
                    {
                        Response.Redirect("MyOrders.aspx");
                    }
                }
                else
                {
                    Response.Redirect("MyOrders.aspx");
                }
            }
        }

        private void CargarDatosPerfilUsuario()
        {
            if (Session["Id_User"] != null)
            {
                string userId = Session["Id_User"].ToString();
                string query = "SELECT Name, Mail FROM users WHERE Id_User = @Id";

                using (MySqlConnection conn = data.ObtenerConexion())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);

                        try
                        {
                            conn.Open();
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    lblFullName.Text = reader["Name"].ToString();
                                    lblUserEmail.Text = reader["Mail"].ToString();
                                }
                                else
                                {
                                    lblFullName.Text = "User not found";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            lblFullName.Text = "Error: " + ex.Message;
                        }
                    }
                }
            }
            else
            {
                lblFullName.Text = "No active session";
            }

            if (upPerfil != null)
            {
                upPerfil.Update();
            }
        }

        private void ActualizarContadorCarrito()
        {
            DataTable dtCart = Session["Cart"] as DataTable;

            if (dtCart != null && dtCart.Rows.Count > 0)
            {
                int totalProducts = 0;
                foreach (DataRow row in dtCart.Rows)
                {
                    if (row["Quantity"] != DBNull.Value)
                    {
                        totalProducts += Convert.ToInt32(row["Quantity"]);
                    }
                }
                lblCartCount.Text = totalProducts.ToString();
            }
            else
            {
                lblCartCount.Text = "0";
            }
        }

        protected void btnGoToAccount_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyAccount.aspx");
        }

        protected void btnMyOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyOrders.aspx");
        }

        protected void btnNavCart_Click(object sender, EventArgs e)
        {
            Response.Redirect("Cart.aspx");
        }

        private bool IsUserOrderOwner(int idOrder, int idUser)
        {
            bool isValidOwner = false;
            string query = "SELECT COUNT(1) FROM orders WHERE Id_Order = @IdOrder AND Id_User = @IdUser;";

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                    cmd.Parameters.AddWithValue("@IdUser", idUser);

                    try
                    {
                        conn.Open();
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            isValidOwner = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Critical error inside ownership validation checkpoint: " + ex.Message);
                        isValidOwner = false;
                    }
                }
            }

            return isValidOwner;
        }

        private void LoadOrderDetails(int idOrder)
        {
            string queryMaster = @"
    SELECT o.OrderDate AS order_date, 
           o.Address AS shipping_address, 
           o.Total AS total_amount, 
           o.Phone AS customer_phone,
           CONCAT(o.Name, ' ', o.LastName) AS customer_name, 
           o.OrderNotes AS order_notes,
           o.shipping_cost AS shipping_cost,
           o.Id_Status,
           o.Id_PaymentMethod,
           o.Latitude,           
           o.Longitude,        
           c.city_name AS city, 
           m.Municipality_Name AS municipality, 
           d.District_Name AS district,
           s.Status_Name AS order_status, 
           p.Method_Name AS payment_method
    FROM orders o
    LEFT JOIN cities c ON o.Id_City = c.id_city
    LEFT JOIN municipalities m ON o.Id_Municipality = m.Id_Municipality
    LEFT JOIN districts d ON o.Id_District = d.Id_District
    LEFT JOIN order_statuses s ON o.Id_Status = s.Id_Status
    LEFT JOIN payment_methods p ON o.Id_PaymentMethod = p.Id_PaymentMethod
    WHERE o.Id_Order = @IdOrder;";

            string queryDetails = @"
                SELECT od.ProductName, od.Size, od.Price, od.Quantity, od.Subtotal, t.ImageURL 
                FROM order_details od
                LEFT JOIN tshirts t ON TRIM(od.ProductName) = TRIM(t.Name)
                WHERE od.Id_Order = @IdOrder;";

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                try
                {
                    conn.Open();

                    int idStatus = 0;
                    int idPaymentMethod = 0;

                    using (MySqlCommand cmdMaster = new MySqlCommand(queryMaster, conn))
                    {
                        cmdMaster.Parameters.AddWithValue("@IdOrder", idOrder);
                        using (MySqlDataReader reader = cmdMaster.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                idStatus = reader["Id_Status"] != DBNull.Value ? Convert.ToInt32(reader["Id_Status"]) : 0;
                                idPaymentMethod = reader["Id_PaymentMethod"] != DBNull.Value ? Convert.ToInt32(reader["Id_PaymentMethod"]) : 0;

                                ViewState["IdStatus"] = idStatus;
                                ViewState["IdPaymentMethod"] = idPaymentMethod;

                                // --- CÓDIGO NUEVO: LLENAR LOS HIDDEN FIELDS PARA EL MAPA ---
                                hfTrackOrderId.Value = idOrder.ToString();

                                if (reader["Latitude"] != DBNull.Value && reader["Longitude"] != DBNull.Value)
                                {
                                    hfOrderLat.Value = reader["Latitude"].ToString();
                                    hfOrderLng.Value = reader["Longitude"].ToString();
                                }

                                // LÓGICA DE INTERFAZ DE ESTADOS 
                                UpdateStatusUI(idStatus);

                                lblOrderDate.Text = Convert.ToDateTime(reader["order_date"]).ToString("dd/MM/yyyy");
                                lblStatusBadge.Text = reader["order_status"] != DBNull.Value ? reader["order_status"].ToString() : "Pending";

                                lblCustomerName.Text = reader["customer_name"].ToString();
                                lblPhone.Text = (reader["customer_phone"] != DBNull.Value && reader["customer_phone"].ToString() != "") ? reader["customer_phone"].ToString() : "N/A";
                                lblAddress.Text = reader["shipping_address"].ToString();

                                lblCity.Text = reader["city"] != DBNull.Value ? reader["city"].ToString() : "N/A";
                                lblMunicipality.Text = reader["municipality"] != DBNull.Value ? reader["municipality"].ToString() : "N/A";
                                lblDistrict.Text = reader["district"] != DBNull.Value ? reader["district"].ToString() : "N/A";

                                lblPaymentMethod.Text = reader["payment_method"] != DBNull.Value ? reader["payment_method"].ToString() : "Not Specified";

                                if (idPaymentMethod == 2) // PayPal
                                {
                                    phPayPal.Visible = true;
                                    lblTransactionId.Text = "PP-" + idOrder.ToString().PadLeft(6, '0');
                                }
                                else
                                {
                                    phPayPal.Visible = false;
                                }

                                string notes = reader["order_notes"] != DBNull.Value ? reader["order_notes"].ToString().Trim() : "";
                                lblNotes.Text = notes != "" ? notes : "No notes provided.";

                                decimal orderTotal = Convert.ToDecimal(reader["total_amount"]);
                                decimal shippingCost = Convert.ToDecimal(reader["shipping_cost"]);

                                lblShippingCost.Text = shippingCost == 0 ? "FREE" : "$" + shippingCost.ToString("F2");
                                lblItemsSubtotal.Text = (orderTotal - shippingCost).ToString("F2");
                                lblOrderTotal.Text = orderTotal.ToString("F2");
                            }
                        }
                    }

                    using (MySqlCommand cmdDetails = new MySqlCommand(queryDetails, conn))
                    {
                        cmdDetails.Parameters.AddWithValue("@IdOrder", idOrder);
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmdDetails))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            rptOrderProducts.DataSource = dt;
                            rptOrderProducts.DataBind();
                        }
                    }

                    EvaluateActionButtonsPolicy(idStatus, idPaymentMethod);
                }
                catch (Exception ex)
                {
                    ShowAlert("Database error while retrieving layout: " + ex.Message);
                }
            }
        }

        private void UpdateStatusUI(int idStatus)
        {
            statusAlertBox.Attributes["class"] = "alert d-flex align-items-center mb-0";
            trackerButtonContainer.Visible = false;

            switch (idStatus)
            {
                case 1: // Pending
                    statusAlertBox.Style["background-color"] = "#fffbeb";
                    statusAlertBox.Style["border-left"] = "5px solid #f59e0b";
                    statusIcon.Attributes["class"] = "fas fa-clock fa-2x me-3 text-warning";
                    statusTitle.InnerText = "Awaiting Payment";
                    statusTitle.Style["color"] = "#d97706";
                    statusDescription.InnerText = "Your order has been placed successfully. If you chose Cash on Delivery, please have the exact amount ready for the driver.";
                    statusDescription.Style["color"] = "#92400e";
                    break;
                case 2: // Paid
                    statusAlertBox.Style["background-color"] = "#f0fdf4";
                    statusAlertBox.Style["border-left"] = "5px solid #10b981";
                    statusIcon.Attributes["class"] = "fas fa-check-circle fa-2x me-3 text-success";
                    statusTitle.InnerText = "Payment Confirmed";
                    statusTitle.Style["color"] = "#047857";
                    statusDescription.InnerText = "Payment successful! We are currently preparing your jerseys. A delivery driver will be assigned shortly.";
                    statusDescription.Style["color"] = "#065f46";
                    break;
                case 3: // Shipped
                    statusAlertBox.Style["background-color"] = "#eff6ff";
                    statusAlertBox.Style["border-left"] = "5px solid #3b82f6";
                    statusIcon.Attributes["class"] = "fas fa-motorcycle fa-2x me-3 text-primary";
                    statusTitle.InnerText = "Order on the Way!";
                    statusTitle.Style["color"] = "#1d4ed8";
                    statusDescription.InnerText = "Your order has left our store! Our delivery driver is currently heading to your location.";
                    statusDescription.Style["color"] = "#1e40af";
                    // SE REVELA EL BOTÓN DE TRACKING AL ESTAR EN CAMINO
                    trackerButtonContainer.Visible = true;
                    break;
                case 4: // Delivered
                    statusAlertBox.Style["background-color"] = "#f3f4f6";
                    statusAlertBox.Style["border-left"] = "5px solid #6b7280";
                    statusIcon.Attributes["class"] = "fas fa-box-open fa-2x me-3 text-secondary";
                    statusTitle.InnerText = "Delivered";
                    statusTitle.Style["color"] = "#374151";
                    statusDescription.InnerText = "This order has been successfully delivered. We hope you enjoy your new gear!";
                    statusDescription.Style["color"] = "#1f2937";
                    break;
                case 5: // Cancelled
                    statusAlertBox.Style["background-color"] = "#fef2f2";
                    statusAlertBox.Style["border-left"] = "5px solid #ef4444";
                    statusIcon.Attributes["class"] = "fas fa-times-circle fa-2x me-3 text-danger";
                    statusTitle.InnerText = "Order Cancelled";
                    statusTitle.Style["color"] = "#b91c1c";
                    statusDescription.InnerText = "This order has been cancelled. If you believe this is an error, please contact support.";
                    statusDescription.Style["color"] = "#991b1b";
                    break;
                case 6: // Refund Requested
                    statusAlertBox.Style["background-color"] = "#fff7ed";
                    statusAlertBox.Style["border-left"] = "5px solid #f97316";
                    statusIcon.Attributes["class"] = "fas fa-undo fa-2x me-3";
                    statusIcon.Style["color"] = "#ea580c";
                    statusTitle.InnerText = "Refund Under Review";
                    statusTitle.Style["color"] = "#c2410c";
                    statusDescription.InnerText = "You have requested a refund. Our administrative team is currently reviewing your ticket.";
                    statusDescription.Style["color"] = "#9a3412";
                    break;
                case 7: // Refunded
                    statusAlertBox.Style["background-color"] = "#ecfdf5";
                    statusAlertBox.Style["border-left"] = "5px solid #059669";
                    statusIcon.Attributes["class"] = "fas fa-hand-holding-usd fa-2x me-3 text-success";
                    statusTitle.InnerText = "Refund Processed";
                    statusTitle.Style["color"] = "#047857";
                    statusDescription.InnerText = "Your refund request was approved and the funds have been returned to your account.";
                    statusDescription.Style["color"] = "#065f46";
                    break;
                case 8: // Refund Rejected
                    statusAlertBox.Style["background-color"] = "#fef2f2";
                    statusAlertBox.Style["border-left"] = "5px solid #dc2626";
                    statusIcon.Attributes["class"] = "fas fa-exclamation-circle fa-2x me-3 text-danger";
                    statusTitle.InnerText = "Refund Declined";
                    statusTitle.Style["color"] = "#b91c1c";
                    statusDescription.InnerText = "Your refund request was reviewed and declined. The order remains valid and paid.";
                    statusDescription.Style["color"] = "#991b1b";
                    break;
                case 9: // Ready for Pickup (El cliente lo ve como Empaquetado)
                    statusAlertBox.Style["background-color"] = "#fdf4ff";
                    statusAlertBox.Style["border-left"] = "5px solid #d946ef";
                    statusIcon.Attributes["class"] = "fas fa-box fa-2x me-3";
                    statusIcon.Style["color"] = "#c026d3";
                    statusTitle.InnerText = "Order Packaged";
                    statusTitle.Style["color"] = "#a21caf";
                    statusDescription.InnerText = "Your items have been carefully packaged and are currently waiting for a delivery driver to pick them up from our store.";
                    statusDescription.Style["color"] = "#86198f";
                    break;
            }
        }

        private void EvaluateActionButtonsPolicy(int idStatus, int idPaymentMethod)
        {
            lnkCancelOrder.Visible = false;
            lnkRequestRefund.Visible = false;

            if (idStatus == 1)
            {
                lnkCancelOrder.Visible = true;
            }
            else if (idStatus == 2)
            {
                if (idPaymentMethod == 2)
                {
                    lnkRequestRefund.Visible = true;
                }
            }
        }

        protected void lnkAction_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string actionType = btn.CommandArgument;

            ViewState["CurrentActionType"] = actionType;
            litActionType.Text = actionType == "CANCEL" ? "Cancellation" : "Refund";

            txtReason.Text = string.Empty;
            lblModalError.Visible = false;

            PopulateReasonsCatalog(actionType);

            phReasonModal.Visible = true;
        }

        private void PopulateReasonsCatalog(string actionType)
        {
            ddlReasons.Items.Clear();

            string query = "SELECT Id_CatalogReason, Reason_Title FROM reason_catalog WHERE Action_Type = @ActionType ORDER BY Id_CatalogReason ASC;";

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ActionType", actionType);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlReasons.DataSource = reader;
                            ddlReasons.DataValueField = "Id_CatalogReason";
                            ddlReasons.DataTextField = "Reason_Title";
                            ddlReasons.DataBind();
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblModalError.Text = "Error loading reasons catalog: " + ex.Message;
                    lblModalError.Visible = true;
                }
            }

            ddlReasons.Items.Insert(0, new ListItem("-- Select a Reason --", ""));
        }

        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            phReasonModal.Visible = false;
        }

        protected void btnSubmitAction_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlReasons.SelectedValue))
            {
                lblModalError.Text = "Please select a valid reason from the list.";
                lblModalError.Visible = true;
                return;
            }

            int idCatalogReason = Convert.ToInt32(ddlReasons.SelectedValue);
            string selectedReasonTitle = ddlReasons.SelectedItem.Text;
            string reasonText = txtReason.Text.Trim();

            if (selectedReasonTitle.Contains("Other"))
            {
                if (string.IsNullOrEmpty(reasonText) || reasonText.Length < 10)
                {
                    lblModalError.Text = "Since you selected 'Other', please provide more details in the comments field (minimum 10 characters).";
                    lblModalError.Visible = true;
                    return;
                }
            }

            int idOrder = Convert.ToInt32(ViewState["IdOrder"]);
            string actionType = ViewState["CurrentActionType"].ToString();
            int currentStatus = Convert.ToInt32(ViewState["IdStatus"]);

            int targetStatusId = 1;

            if (actionType == "CANCEL" && currentStatus == 1)
            {
                targetStatusId = 5;
            }
            else if (actionType == "REFUND" && currentStatus == 2)
            {
                targetStatusId = 6;
            }
            else
            {
                lblModalError.Text = "This action is no longer valid for the current state of the order.";
                lblModalError.Visible = true;
                return;
            }

            string updateOrderQuery = "UPDATE orders SET Id_Status = @TargetStatusId WHERE Id_Order = @IdOrder;";
            string insertReasonQuery = @"INSERT INTO order_reasons (Id_Order, Id_CatalogReason, Action_Type, Reason_Text, Created_At) 
                                         VALUES (@IdOrder, @IdCatalogReason, @ActionType, @ReasonText, NOW());";

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                try
                {
                    conn.Open();

                    string customerEmail = "";
                    string customerName = "";
                    decimal orderTotal = 0;

                    string queryDetails = @"
                        SELECT u.Mail, u.Name, o.Total 
                        FROM users u 
                        INNER JOIN orders o ON o.Id_User = u.Id_User 
                        WHERE o.Id_Order = @IdOrder AND u.Id_User = @IdUser;";

                    using (MySqlCommand cmdDetails = new MySqlCommand(queryDetails, conn))
                    {
                        cmdDetails.Parameters.AddWithValue("@IdOrder", idOrder);
                        cmdDetails.Parameters.AddWithValue("@IdUser", Convert.ToInt32(Session["Id_User"]));

                        using (MySqlDataReader readerDetails = cmdDetails.ExecuteReader())
                        {
                            if (readerDetails.Read())
                            {
                                customerEmail = readerDetails["Mail"].ToString();
                                customerName = readerDetails["Name"].ToString();
                                orderTotal = Convert.ToDecimal(readerDetails["Total"]);
                            }
                        }
                    }

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            using (MySqlCommand cmdUpdate = new MySqlCommand(updateOrderQuery, conn, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@TargetStatusId", targetStatusId);
                                cmdUpdate.Parameters.AddWithValue("@IdOrder", idOrder);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            using (MySqlCommand cmdInsert = new MySqlCommand(insertReasonQuery, conn, transaction))
                            {
                                cmdInsert.Parameters.AddWithValue("@IdOrder", idOrder);
                                cmdInsert.Parameters.AddWithValue("@IdCatalogReason", idCatalogReason);
                                cmdInsert.Parameters.AddWithValue("@ActionType", actionType);
                                cmdInsert.Parameters.AddWithValue("@ReasonText", string.IsNullOrEmpty(reasonText) ? (object)DBNull.Value : reasonText);
                                cmdInsert.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            phReasonModal.Visible = false;

                            if (actionType == "REFUND")
                            {
                                EmailService.SendRefundNotification(customerEmail, customerName, idOrder.ToString(), orderTotal);
                            }

                            string titleAlert = actionType == "CANCEL" ? "Order Cancelled" : "Refund Requested";
                            string successMessage = actionType == "CANCEL"
                                ? "Order cancelled successfully."
                                : "Your refund request has been submitted to the administrator for review.";

                            string swalScript = $@"
                                Swal.fire({{
                                    title: '{titleAlert}',
                                    text: '{successMessage}',
                                    icon: 'success',
                                    confirmButtonColor: '#FFC800'
                                }}).then((result) => {{
                                    window.location.href = window.location.href;
                                }});";

                            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", swalScript, true);
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblModalError.Text = "Transaction Error: " + ex.Message;
                    lblModalError.Visible = true;
                }
            }
        }

        [System.Web.Services.WebMethod]
        public static string GetLiveDriverLocation(int orderId)
        {
            // Asegúrate de usar el nombre correcto de tu cadena de conexión
            string connString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

            using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString))
            {
                string query = @"
            SELECT dt.CurrentLat, dt.CurrentLng 
            FROM orders o
            INNER JOIN driver_tracking dt ON o.Id_DeliveryMan = dt.Id_Driver
            WHERE o.Id_Order = @orderId AND o.Id_Status = 3";

                using (MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    conn.Open();

                    using (MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return $"{{\"lat\": {reader["CurrentLat"]}, \"lng\": {reader["CurrentLng"]}}}";
                        }
                    }
                }
            }
            return "null"; // Retorna texto "null" si no hay datos
        }

        private void ShowAlert(string message)
        {
            string cleanMessage = message.Replace("'", "\\'").Replace("\r\n", " ");
            ScriptManager.RegisterStartupScript(this, this.GetType(), "errorAlert", $"alert('{cleanMessage}');", true);
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
        protected string FormatJerseyName(object nameObj)
        {
            if (nameObj == null || nameObj == DBNull.Value) return "";
            
            string name = nameObj.ToString().ToLower().Trim();
            System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(name);
        }
    }
}