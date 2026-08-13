using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class OrderDetail : BasePage
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

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
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
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            string statusColumn = isSpanish ? "s.Status_Name_es" : "s.Status_Name";

            string queryMaster = $@"
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
           {statusColumn} AS order_status, 
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
                    DateTime orderDate = DateTime.MinValue;
                    string shippingAddress = "";
                    decimal totalAmount = 0;
                    string customerPhone = "";
                    string customerName = "";
                    string orderNotes = "";
                    decimal shippingCost = 0;
                    string lat = "";
                    string lng = "";
                    string city = "";
                    string municipality = "";
                    string district = "";
                    string orderStatus = "";
                    string paymentMethod = "";
                    bool hasOrder = false;

                    using (MySqlCommand cmdMaster = new MySqlCommand(queryMaster, conn))
                    {
                        cmdMaster.Parameters.AddWithValue("@IdOrder", idOrder);
                        using (MySqlDataReader reader = cmdMaster.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hasOrder = true;
                                idStatus = reader["Id_Status"] != DBNull.Value ? Convert.ToInt32(reader["Id_Status"]) : 0;
                                idPaymentMethod = reader["Id_PaymentMethod"] != DBNull.Value ? Convert.ToInt32(reader["Id_PaymentMethod"]) : 0;

                                if (reader["order_date"] != DBNull.Value) orderDate = Convert.ToDateTime(reader["order_date"]);
                                shippingAddress = reader["shipping_address"] != DBNull.Value ? reader["shipping_address"].ToString() : "";
                                if (reader["total_amount"] != DBNull.Value) totalAmount = Convert.ToDecimal(reader["total_amount"]);
                                customerPhone = reader["customer_phone"] != DBNull.Value ? reader["customer_phone"].ToString() : "";
                                customerName = reader["customer_name"] != DBNull.Value ? reader["customer_name"].ToString() : "";
                                orderNotes = reader["order_notes"] != DBNull.Value ? reader["order_notes"].ToString() : "";
                                if (reader["shipping_cost"] != DBNull.Value) shippingCost = Convert.ToDecimal(reader["shipping_cost"]);

                                lat = reader["Latitude"] != DBNull.Value ? reader["Latitude"].ToString() : "";
                                lng = reader["Longitude"] != DBNull.Value ? reader["Longitude"].ToString() : "";

                                city = reader["city"] != DBNull.Value ? reader["city"].ToString() : "";
                                municipality = reader["municipality"] != DBNull.Value ? reader["municipality"].ToString() : "";
                                district = reader["district"] != DBNull.Value ? reader["district"].ToString() : "";
                                orderStatus = reader["order_status"] != DBNull.Value ? reader["order_status"].ToString() : "Pending";
                                paymentMethod = reader["payment_method"] != DBNull.Value ? reader["payment_method"].ToString() : "";
                            }
                        }
                    }

                    if (!hasOrder) return;

                    ViewState["IdStatus"] = idStatus;
                    ViewState["IdPaymentMethod"] = idPaymentMethod;
                    hfTrackOrderId.Value = idOrder.ToString();

                    if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lng))
                    {
                        hfOrderLat.Value = lat;
                        hfOrderLng.Value = lng;
                    }

                    string reasonTitle = "";
                    string reasonText = "";
                    try
                    {
                        string queryReason = @"
                            SELECT CASE WHEN @Lang = 'es' THEN COALESCE(rc.Reason_Title_es, rc.Reason_Title) ELSE rc.Reason_Title END AS Reason_Title,
                                   r.Reason_Text
                            FROM order_reasons r
                            LEFT JOIN reason_catalog rc ON r.Id_CatalogReason = rc.Id_CatalogReason
                            WHERE r.Id_Order = @IdOrder
                            ORDER BY r.Created_At DESC LIMIT 1;";
                        using (MySqlCommand cmdReason = new MySqlCommand(queryReason, conn))
                        {
                            cmdReason.Parameters.AddWithValue("@Lang", isSpanish ? "es" : "en");
                            cmdReason.Parameters.AddWithValue("@IdOrder", idOrder);
                            using (MySqlDataReader rReader = cmdReason.ExecuteReader())
                            {
                                if (rReader.Read())
                                {
                                    reasonTitle = rReader["Reason_Title"] != DBNull.Value ? rReader["Reason_Title"].ToString() : "";
                                    reasonText = rReader["Reason_Text"] != DBNull.Value ? rReader["Reason_Text"].ToString() : "";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error loading reason title: " + ex.Message);
                    }

                    // LÓGICA DE INTERFAZ DE ESTADOS 
                    UpdateStatusUI(idStatus, reasonTitle);

                    lblOrderDate.Text = orderDate != DateTime.MinValue ? orderDate.ToString("dd/MM/yyyy") : "";
                    lblStatusBadge.Text = GetLocalizedStatusName(idStatus, orderStatus);

                    lblCustomerName.Text = customerName;
                    lblPhone.Text = !string.IsNullOrEmpty(customerPhone) ? customerPhone : "N/A";
                    lblAddress.Text = shippingAddress;

                    lblCity.Text = !string.IsNullOrEmpty(city) ? city : "N/A";
                    lblMunicipality.Text = !string.IsNullOrEmpty(municipality) ? municipality : "N/A";
                    lblDistrict.Text = !string.IsNullOrEmpty(district) ? district : "N/A";

                    if (idPaymentMethod == 1)
                    {
                        lblPaymentMethod.Text = GetGlobalResourceObject("Strings", "Checkout_PayCash")?.ToString() ?? "Cash on Delivery";
                    }
                    else if (idPaymentMethod == 2)
                    {
                        lblPaymentMethod.Text = GetGlobalResourceObject("Strings", "Checkout_PayPaypal")?.ToString() ?? "PayPal";
                    }
                    else if (idPaymentMethod == 3)
                    {
                        lblPaymentMethod.Text = GetGlobalResourceObject("Strings", "Checkout_PayWallet")?.ToString() ?? "Virtual Wallet";
                    }
                    else
                    {
                        lblPaymentMethod.Text = !string.IsNullOrEmpty(paymentMethod) ? paymentMethod : "Not Specified";
                    }

                    if (idPaymentMethod == 2) // PayPal
                    {
                        phPayPal.Visible = true;
                        lblTransactionId.Text = "PP-" + idOrder.ToString().PadLeft(6, '0');
                    }
                    else
                    {
                        phPayPal.Visible = false;
                    }

                    string notes = orderNotes.Trim();
                    lblNotes.Text = notes != "" ? notes : (GetGlobalResourceObject("Strings", "OrderDetail_NoNotes")?.ToString() ?? "No notes provided.");

                    lblShippingCost.Text = shippingCost == 0 ? (isSpanish ? "GRATIS" : "FREE") : "$" + shippingCost.ToString("F2");
                    lblItemsSubtotal.Text = (totalAmount - shippingCost).ToString("F2");
                    lblOrderTotal.Text = totalAmount.ToString("F2");

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

        private void UpdateStatusUI(int idStatus, string reasonTitle = "")
        {
            statusAlertBox.Attributes["class"] = "alert d-flex align-items-center mb-0";
            trackerButtonContainer.Visible = false;

            switch (idStatus)
            {
                case 1: // Pending
                    statusAlertBox.Style["background-color"] = "#fffbeb";
                    statusAlertBox.Style["border-left"] = "5px solid #f59e0b";
                    statusIcon.Attributes["class"] = "fas fa-clock fa-2x me-3 text-warning";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Pending_Title");
                    statusTitle.Style["color"] = "#d97706";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Pending_Desc");
                    statusDescription.Style["color"] = "#92400e";
                    break;
                case 2: // Paid
                    statusAlertBox.Style["background-color"] = "#f0fdf4";
                    statusAlertBox.Style["border-left"] = "5px solid #10b981";
                    statusIcon.Attributes["class"] = "fas fa-check-circle fa-2x me-3 text-success";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Paid_Title");
                    statusTitle.Style["color"] = "#047857";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Paid_Desc");
                    statusDescription.Style["color"] = "#065f46";
                    break;
                case 3: // Shipped
                    statusAlertBox.Style["background-color"] = "#eff6ff";
                    statusAlertBox.Style["border-left"] = "5px solid #3b82f6";
                    statusIcon.Attributes["class"] = "fas fa-motorcycle fa-2x me-3 text-primary";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Shipped_Title");
                    statusTitle.Style["color"] = "#1d4ed8";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Shipped_Desc");
                    statusDescription.Style["color"] = "#1e40af";
                    // SE REVELA EL BOTÓN DE TRACKING AL ESTAR EN CAMINO
                    trackerButtonContainer.Visible = true;
                    break;
                case 4: // Delivered
                    statusAlertBox.Style["background-color"] = "#f3f4f6";
                    statusAlertBox.Style["border-left"] = "5px solid #6b7280";
                    statusIcon.Attributes["class"] = "fas fa-box-open fa-2x me-3 text-secondary";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Delivered_Title");
                    statusTitle.Style["color"] = "#374151";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Delivered_Desc");
                    statusDescription.Style["color"] = "#1f2937";
                    break;
                case 5: // Cancelled
                    statusAlertBox.Style["background-color"] = "#fef2f2";
                    statusAlertBox.Style["border-left"] = "5px solid #ef4444";
                    statusIcon.Attributes["class"] = "fas fa-times-circle fa-2x me-3 text-danger";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Cancelled_Title");
                    statusTitle.Style["color"] = "#b91c1c";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Cancelled_Desc") + (!string.IsNullOrEmpty(reasonTitle) ? $" ({reasonTitle})" : "");
                    statusDescription.Style["color"] = "#991b1b";
                    break;
                case 6: // Refund Requested
                    statusAlertBox.Style["background-color"] = "#fff7ed";
                    statusAlertBox.Style["border-left"] = "5px solid #f97316";
                    statusIcon.Attributes["class"] = "fas fa-undo fa-2x me-3";
                    statusIcon.Style["color"] = "#ea580c";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_RefundReq_Title");
                    statusTitle.Style["color"] = "#c2410c";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_RefundReq_Desc") + (!string.IsNullOrEmpty(reasonTitle) ? $" ({reasonTitle})" : "");
                    statusDescription.Style["color"] = "#9a3412";
                    break;
                case 7: // Refunded
                    statusAlertBox.Style["background-color"] = "#ecfdf5";
                    statusAlertBox.Style["border-left"] = "5px solid #059669";
                    statusIcon.Attributes["class"] = "fas fa-hand-holding-usd fa-2x me-3 text-success";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Refunded_Title");
                    statusTitle.Style["color"] = "#047857";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Refunded_Desc") + (!string.IsNullOrEmpty(reasonTitle) ? $" ({reasonTitle})" : "");
                    statusDescription.Style["color"] = "#065f46";
                    break;
                case 8: // Refund Rejected
                    statusAlertBox.Style["background-color"] = "#fef2f2";
                    statusAlertBox.Style["border-left"] = "5px solid #dc2626";
                    statusIcon.Attributes["class"] = "fas fa-exclamation-circle fa-2x me-3 text-danger";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_RefundDeclined_Title");
                    statusTitle.Style["color"] = "#b91c1c";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_RefundDeclined_Desc") + (!string.IsNullOrEmpty(reasonTitle) ? $" ({reasonTitle})" : "");
                    statusDescription.Style["color"] = "#991b1b";
                    break;
                case 9: // Ready for Pickup (El cliente lo ve como Empaquetado)
                    statusAlertBox.Style["background-color"] = "#fdf4ff";
                    statusAlertBox.Style["border-left"] = "5px solid #d946ef";
                    statusIcon.Attributes["class"] = "fas fa-box fa-2x me-3";
                    statusIcon.Style["color"] = "#c026d3";
                    statusTitle.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Packaged_Title");
                    statusTitle.Style["color"] = "#a21caf";
                    statusDescription.InnerText = AlertHelper.GetResourceString(this, "OrderStatus_Packaged_Desc");
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
            bool isSpanishAction = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            litActionType.Text = actionType == "CANCEL" 
                ? (isSpanishAction ? "la Cancelación" : "Cancellation") 
                : (isSpanishAction ? "el Reembolso" : "Refund");

            txtReason.Text = string.Empty;
            lblModalError.Visible = false;

            PopulateReasonsCatalog(actionType);

            phReasonModal.Visible = true;
        }

        private void PopulateReasonsCatalog(string actionType)
        {
            ddlReasons.Items.Clear();

            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            string reasonColumn = isSpanish ? "COALESCE(Reason_Title_es, Reason_Title) AS Reason_Title" : "Reason_Title";

            string query = $"SELECT Id_CatalogReason, {reasonColumn} FROM reason_catalog WHERE Action_Type = @ActionType ORDER BY Id_CatalogReason ASC;";

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

            string defaultSelect = AlertHelper.GetResourceString(this, "OrderDetail_SelectReason");
            ddlReasons.Items.Insert(0, new ListItem(defaultSelect, ""));
        }

        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            phReasonModal.Visible = false;
        }

        protected void btnSubmitAction_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlReasons.SelectedValue))
            {
                lblModalError.Text = AlertHelper.GetResourceString(this, "OrderDetail_ErrorSelectReason");
                lblModalError.Visible = true;
                return;
            }

            int idCatalogReason = Convert.ToInt32(ddlReasons.SelectedValue);
            string selectedReasonTitle = ddlReasons.SelectedItem.Text;
            string reasonText = txtReason.Text.Trim();

            if (selectedReasonTitle.Contains("Other") || selectedReasonTitle.Contains("Otro"))
            {
                if (string.IsNullOrEmpty(reasonText) || reasonText.Length < 10)
                {
                    lblModalError.Text = AlertHelper.GetResourceString(this, "OrderDetail_ErrorOtherReason");
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
                lblModalError.Text = AlertHelper.GetResourceString(this, "OrderDetail_ErrorInvalidAction");
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

                            string titleAlert = actionType == "CANCEL" 
                                ? AlertHelper.GetResourceString(this, "OrderDetail_CancelSuccessTitle") 
                                : AlertHelper.GetResourceString(this, "OrderDetail_RefundSuccessTitle");

                            string successMessage = actionType == "CANCEL"
                                ? AlertHelper.GetResourceString(this, "OrderDetail_CancelSuccessText")
                                : AlertHelper.GetResourceString(this, "OrderDetail_RefundSuccessText");

                            string swalScript = $@"
                                Swal.fire({{
                                    title: '{titleAlert.Replace("'", "\\'")}',
                                    text: '{successMessage.Replace("'", "\\'")}',
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
            // AsegÃºrate de usar el nombre correcto de tu cadena de conexiÃ³n
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
            string script = AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", message, "error");
            ScriptManager.RegisterStartupScript(this, this.GetType(), "errorAlert", script, true);
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
        protected string GetLocalizedStatusName(object idStatusObj, object statusNameObj)
        {
            int idStatus = 0;
            if (idStatusObj != null && idStatusObj != DBNull.Value)
            {
                int.TryParse(idStatusObj.ToString(), out idStatus);
            }
            
            switch (idStatus)
            {
                case 1: return GetGlobalResourceObject("Strings", "OrderStatus_Pending_Title")?.ToString() ?? "Pending";
                case 2: return GetGlobalResourceObject("Strings", "OrderStatus_Paid_Title")?.ToString() ?? "Paid";
                case 3: return GetGlobalResourceObject("Strings", "OrderStatus_Shipped_Title")?.ToString() ?? "Shipped";
                case 4: return GetGlobalResourceObject("Strings", "OrderStatus_Delivered_Title")?.ToString() ?? "Delivered";
                case 5: return GetGlobalResourceObject("Strings", "OrderStatus_Cancelled_Title")?.ToString() ?? "Cancelled";
                case 6: return GetGlobalResourceObject("Strings", "OrderStatus_RefundReq_Title")?.ToString() ?? "Refund Requested";
                case 7: return GetGlobalResourceObject("Strings", "OrderStatus_Refunded_Title")?.ToString() ?? "Refunded";
                case 8: return GetGlobalResourceObject("Strings", "OrderStatus_RefundDeclined_Title")?.ToString() ?? "Refund Rejected";
                case 9: return GetGlobalResourceObject("Strings", "OrderStatus_Packaged_Title")?.ToString() ?? "Ready for Pickup";
                default:
                    return statusNameObj?.ToString() ?? "";
            }
        }

        protected string FormatJerseyName(object nameObj)
        {
            if (nameObj == null || nameObj == DBNull.Value) return "";
            
            string name = nameObj.ToString().Trim();
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");

            int customIndex = name.IndexOf(" (customized:", StringComparison.OrdinalIgnoreCase);
            if (customIndex < 0)
            {
                customIndex = name.IndexOf(" (personalizado:", StringComparison.OrdinalIgnoreCase);
            }

            if (customIndex >= 0)
            {
                string baseName = name.Substring(0, customIndex).ToLower().Trim();
                string customPart = name.Substring(customIndex).Trim();
                
                System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
                string formattedBase = textInfo.ToTitleCase(baseName);
                
                int colonIndex = customPart.IndexOf(':');
                if (colonIndex >= 0 && customPart.EndsWith(")"))
                {
                    string customValue = customPart.Substring(colonIndex + 1, customPart.Length - colonIndex - 2).Trim();
                    string label = isSpanish ? "Personalizado" : "Customized";
                    return $"{formattedBase} ({label}: {customValue})";
                }
                else
                {
                    string label = isSpanish ? "Personalizado" : "Customized";
                    if (customPart.IndexOf("customized", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        customPart = System.Text.RegularExpressions.Regex.Replace(customPart, "customized", label, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    else if (customPart.IndexOf("personalizado", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        customPart = System.Text.RegularExpressions.Regex.Replace(customPart, "personalizado", label, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    return $"{formattedBase} {customPart}";
                }
            }
            else
            {
                System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
                return textInfo.ToTitleCase(name.ToLower());
            }
        }
    }
}
