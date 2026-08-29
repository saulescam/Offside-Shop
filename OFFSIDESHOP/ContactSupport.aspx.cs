using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.IO;
using System.Text.RegularExpressions;

namespace OFFSIDESHOP
{
    public partial class ContactSupport : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.Form != null)
            {
                Page.Form.Enctype = "multipart/form-data";
            }

            phNavbarGuest.Visible = false;
            phNavbarUser.Visible = false;
            phNavbarAdmin.Visible = false;

            if (Session["UserRole"] == null)
            {
                phNavbarGuest.Visible = true;
            }
            else
            {
                int userRole = Convert.ToInt32(Session["UserRole"]);
                if (userRole == 1 || userRole == 2)
                {
                    phNavbarAdmin.Visible = true;
                }
                else if (userRole == 3 || userRole == 4)
                {
                    phNavbarUser.Visible = true;
                }
                else
                {
                    phNavbarGuest.Visible = true;
                }
            }

            if (!IsPostBack)
            {
                CargarDatosPerfilUsuario();
                ActualizarContadorCarrito();
                LoadContactReasons();
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

                using (MySqlConnection conn = new MySqlConnection(connectionString))
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
                                    string email = reader["Mail"].ToString();
                                    lblUserEmail.Text = email;

                                    txtEmail.Text = email;
                                    txtEmail.ReadOnly = true;
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            else
            {
                txtEmail.Text = "";
                txtEmail.ReadOnly = true;
            }

            if (upPerfil != null) upPerfil.Update();
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
                        totalProducts += Convert.ToInt32(row["Quantity"]);
                }
                lblCartCount.Text = totalProducts.ToString();
            }
            else
            {
                lblCartCount.Text = "0";
            }
        }

        private void LoadContactReasons()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT Id_ContactReason, Reason_Name FROM contact_reasons ORDER BY Id_ContactReason ASC";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            foreach (DataRow dr in dt.Rows)
                            {
                                string key = "Reason_" + dr["Id_ContactReason"];
                                string locName = AlertHelper.GetResourceString(this, key);
                                if (!string.IsNullOrEmpty(locName) && !locName.StartsWith("[Resource"))
                                {
                                    dr["Reason_Name"] = locName;
                                }
                            }
                            ddlReason.DataSource = dt;
                            ddlReason.DataTextField = "Reason_Name";
                            ddlReason.DataValueField = "Id_ContactReason";
                            ddlReason.DataBind();
                            ddlReason.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-", ""));
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        private void LoadUserOrders()
        {
            ddlOrders.Items.Clear();
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");

            if (Session["Id_User"] == null)
            {
                ddlOrders.Items.Add(new System.Web.UI.WebControls.ListItem(isSpanish ? "Inicia sesión para ver tus pedidos" : "Log in to view your orders", ""));
                return;
            }

            int userId = Convert.ToInt32(Session["Id_User"]);
            string statusCol = isSpanish ? "s.Status_Name_es" : "s.Status_Name";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = $@"SELECT o.Id_Order, o.OrderDate, o.Total, {statusCol} AS Status_Name 
                                  FROM orders o
                                  INNER JOIN order_statuses s ON o.Id_Status = s.Id_Status
                                  WHERE o.Id_User = @UserId
                                  ORDER BY o.Id_Order DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    try
                    {
                        conn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            bool hasOrders = false;
                            while (reader.Read())
                            {
                                hasOrders = true;
                                int orderId = Convert.ToInt32(reader["Id_Order"]);
                                DateTime orderDate = Convert.ToDateTime(reader["OrderDate"]);
                                decimal total = Convert.ToDecimal(reader["Total"]);
                                string statusName = reader["Status_Name"].ToString();

                                string displayText = $"#{orderId} - {orderDate:yyyy-MM-dd} (${total:F2}) [{statusName}]";
                                ddlOrders.Items.Add(new System.Web.UI.WebControls.ListItem(displayText, orderId.ToString()));
                            }

                            if (hasOrders)
                            {
                                string selectPlaceholder = AlertHelper.GetResourceString(this, "Contact_SelectOrder");
                                if (string.IsNullOrEmpty(selectPlaceholder) || selectPlaceholder.StartsWith("[Resource"))
                                    selectPlaceholder = isSpanish ? "-- Selecciona tu Pedido --" : "-- Select your Order --";
                                ddlOrders.Items.Insert(0, new System.Web.UI.WebControls.ListItem(selectPlaceholder, ""));
                            }
                            else
                            {
                                string noOrdersMsg = AlertHelper.GetResourceString(this, "Contact_NoOrdersFound");
                                if (string.IsNullOrEmpty(noOrdersMsg) || noOrdersMsg.StartsWith("[Resource"))
                                    noOrdersMsg = isSpanish ? "No tienes pedidos registrados en tu cuenta" : "No orders found in your account";
                                ddlOrders.Items.Add(new System.Web.UI.WebControls.ListItem(noOrdersMsg, ""));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        ddlOrders.Items.Add(new System.Web.UI.WebControls.ListItem(isSpanish ? "Error al cargar pedidos" : "Error loading orders", ""));
                    }
                }
            }
        }

        protected void ddlReason_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlOrderIssue.Visible = false;
            pnlRefundEvidence.Visible = false;
            pnlSellJersey.Visible = false;

            if (string.IsNullOrEmpty(ddlReason.SelectedValue)) return;

            int idReason = Convert.ToInt32(ddlReason.SelectedValue);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT Requires_Order, Requires_Images FROM contact_reasons WHERE Id_ContactReason = @Id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idReason);
                    try
                    {
                        conn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool reqOrder = Convert.ToBoolean(reader["Requires_Order"]);
                                bool reqImages = Convert.ToBoolean(reader["Requires_Images"]);

                                pnlOrderIssue.Visible = reqOrder;
                                if (reqOrder)
                                {
                                    LoadUserOrders();
                                }

                                if (idReason == 3)
                                {
                                    // Solicitud de Venta de Camiseta Retro (Consignación)
                                    pnlSellJersey.Visible = reqImages;
                                    pnlRefundEvidence.Visible = false;
                                }
                                else if (idReason == 2 || (reqImages && reqOrder))
                                {
                                    // Solicitud de Reembolso o Cambio que requiere imágenes de prueba
                                    pnlSellJersey.Visible = false;
                                    pnlRefundEvidence.Visible = reqImages;
                                }
                                else
                                {
                                    pnlSellJersey.Visible = false;
                                    pnlRefundEvidence.Visible = false;
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // 1. VALIDACIÓN MAESTRA DE SESIÓN COACTIVA
            if (Session["Id_User"] == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_LoginRequiredTitle", "Alert_Contact_LoginRequiredText", "warning"), true);
                return;
            }

            if (string.IsNullOrEmpty(ddlReason.SelectedValue))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", "Alert_Contact_SelectReason", "warning"), true);
                return;
            }

            // 2. VALIDACIONES GENERALES DE TEXTO (Anti-Espacios en blanco y Sanidad)
            if (string.IsNullOrWhiteSpace(txtSubject.Text) || string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_ValidationErrorTitle", "Alert_Contact_EmptySubjectDesc", "error"), true);
                return;
            }

            int idReason = Convert.ToInt32(ddlReason.SelectedValue);
            string email = txtEmail.Text.Trim();
            string subject = txtSubject.Text.Trim();
            string message = txtMessage.Text.Trim();

            object idUser = Session["Id_User"];
            object idOrder = DBNull.Value;
            object propPrice = DBNull.Value;
            object condition = DBNull.Value;
            object size = DBNull.Value;

            string img1 = null;
            string img2 = null;
            string img3 = null;

            if (pnlOrderIssue.Visible)
            {
                if (string.IsNullOrEmpty(ddlOrders.SelectedValue) || !int.TryParse(ddlOrders.SelectedValue, out int parsedOrderId) || parsedOrderId <= 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", "Alert_Contact_SelectOrderRequired", "warning"), true);
                    return;
                }

                // Validación de seguridad estricta: Verificar en base de datos que el pedido pertenezca al usuario en sesión
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string verifyQuery = "SELECT COUNT(1) FROM orders WHERE Id_Order = @IdOrder AND Id_User = @UserId";
                    using (MySqlCommand cmdVerify = new MySqlCommand(verifyQuery, conn))
                    {
                        cmdVerify.Parameters.AddWithValue("@IdOrder", parsedOrderId);
                        cmdVerify.Parameters.AddWithValue("@UserId", idUser);

                        try
                        {
                            conn.Open();
                            int count = Convert.ToInt32(cmdVerify.ExecuteScalar());
                            if (count == 0)
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", "Alert_Contact_OrderNotOwnedText", "error"), true);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", ex.Message, "error"), true);
                            return;
                        }
                    }
                }

                idOrder = parsedOrderId;
            }

            if (pnlRefundEvidence.Visible)
            {
                // Validación obligatoria de imágenes de prueba para Reembolso o Cambio
                if (!fileRefundImages.HasFiles)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", "Alert_Contact_RefundImageRequired", "warning"), true);
                    return;
                }

                var uploadedRefundFiles = fileRefundImages.PostedFiles;
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
                int maxFileSizeBytes = 2 * 1024 * 1024; // 2 Megabytes

                foreach (var file in uploadedRefundFiles)
                {
                    if (file.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        if (Array.IndexOf(allowedExtensions, ext) == -1)
                        {
                            string invalidFormatMsg = string.Format(AlertHelper.GetResourceString(this, "Alert_Contact_InvalidFormatText"), file.FileName);
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_InvalidFormatTitle", invalidFormatMsg, "error"), true);
                            return;
                        }

                        if (file.ContentLength > maxFileSizeBytes)
                        {
                            string fileTooLargeMsg = string.Format(AlertHelper.GetResourceString(this, "Alert_Contact_FileTooLargeText"), file.FileName);
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_FileTooLargeTitle", fileTooLargeMsg, "error"), true);
                            return;
                        }
                    }
                }

                string uploadPath = Server.MapPath("~/assets/uploads/tickets/");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                if (uploadedRefundFiles.Count > 0) { img1 = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadedRefundFiles[0].FileName); uploadedRefundFiles[0].SaveAs(uploadPath + img1); }
                if (uploadedRefundFiles.Count > 1) { img2 = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadedRefundFiles[1].FileName); uploadedRefundFiles[1].SaveAs(uploadPath + img2); }
                if (uploadedRefundFiles.Count > 2) { img3 = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadedRefundFiles[2].FileName); uploadedRefundFiles[2].SaveAs(uploadPath + img3); }
            }

            if (pnlSellJersey.Visible)
            {
                // A) Validación de Presencia de Campos Obligatorios
                if (string.IsNullOrEmpty(txtCondition.Text) || string.IsNullOrEmpty(txtPrice.Text) || string.IsNullOrEmpty(ddlSize.SelectedValue))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", "Alert_Contact_FieldsRequired", "warning"), true);
                    return;
                }

                // B) Validación Estricta de Precio
                if (!decimal.TryParse(txtPrice.Text, out decimal parsedPrice) || parsedPrice <= 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_InvalidPriceTitle", "Alert_Contact_InvalidPriceText", "error"), true);
                    return;
                }

                // C) Validación Estricta de Escala Numérica de Condición
                if (!int.TryParse(txtCondition.Text, out int parsedCondition) || parsedCondition < 1 || parsedCondition > 10)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_InvalidConditionTitle", "Alert_Contact_InvalidConditionText", "error"), true);
                    return;
                }

                // D) Asignación de Datos Validados Seguros
                propPrice = parsedPrice;
                condition = parsedCondition.ToString();
                size = ddlSize.SelectedValue;

                // E) Validación de Imágenes Corporativa
                if (!fileImages.HasFiles)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", "Alert_Contact_ImageRequired", "warning"), true);
                    return;
                }

                var uploadedFiles = fileImages.PostedFiles;
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
                int maxFileSizeBytes = 2 * 1024 * 1024; // 2 Megabytes

                foreach (var file in uploadedFiles)
                {
                    if (file.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        if (Array.IndexOf(allowedExtensions, ext) == -1)
                        {
                            string invalidFormatMsg = string.Format(AlertHelper.GetResourceString(this, "Alert_Contact_InvalidFormatText"), file.FileName);
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_InvalidFormatTitle", invalidFormatMsg, "error"), true);
                            return;
                        }

                        if (file.ContentLength > maxFileSizeBytes)
                        {
                            string fileTooLargeMsg = string.Format(AlertHelper.GetResourceString(this, "Alert_Contact_FileTooLargeText"), file.FileName);
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_FileTooLargeTitle", fileTooLargeMsg, "error"), true);
                            return;
                        }
                    }
                }

                string uploadPath = Server.MapPath("~/assets/uploads/tickets/");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                if (uploadedFiles.Count > 0) { img1 = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadedFiles[0].FileName); uploadedFiles[0].SaveAs(uploadPath + img1); }
                if (uploadedFiles.Count > 1) { img2 = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadedFiles[1].FileName); uploadedFiles[1].SaveAs(uploadPath + img2); }
                if (uploadedFiles.Count > 2) { img3 = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadedFiles[2].FileName); uploadedFiles[2].SaveAs(uploadPath + img3); }
            }

            // 3. INSERCIÓN PARAMETRIZADA INMUNE A INYECCIÓN SQL
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = @"INSERT INTO contact_tickets 
                                (Id_User, Id_ContactReason, User_Email, Subject, Message_Body, Id_Order, Proposed_Price, Item_Condition, Size, ImageURL1, ImageURL2, ImageURL3) 
                                VALUES (@UserId, @Reason, @Email, @Subj, @Msg, @Order, @Price, @Cond, @Size, @Img1, @Img2, @Img3)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", idUser);
                    cmd.Parameters.AddWithValue("@Reason", idReason);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Subj", subject);
                    cmd.Parameters.AddWithValue("@Msg", message);
                    cmd.Parameters.AddWithValue("@Order", idOrder);
                    cmd.Parameters.AddWithValue("@Price", propPrice);
                    cmd.Parameters.AddWithValue("@Cond", condition);
                    cmd.Parameters.AddWithValue("@Size", size);
                    cmd.Parameters.AddWithValue("@Img1", img1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Img2", img2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Img3", img3 ?? (object)DBNull.Value);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        // Limpiar formulario tras éxito transaccional
                        ddlReason.SelectedIndex = 0;
                        txtSubject.Text = "";
                        txtMessage.Text = "";
                        ddlOrders.Items.Clear();
                        txtCondition.Text = "";
                        txtPrice.Text = "";
                        if (ddlSize.Items.Count > 0) ddlSize.SelectedIndex = 0;
                        pnlOrderIssue.Visible = false;
                        pnlRefundEvidence.Visible = false;
                        pnlSellJersey.Visible = false;

                        ScriptManager.RegisterStartupScript(this, GetType(), "success", AlertHelper.GetSafeAlertScript(this, "Alert_Contact_SuccessTitle", "Alert_Contact_SuccessText", "success"), true);
                    }
                    catch (Exception ex)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "alert", AlertHelper.GetSafeAlertScript(this, "Alert_ErrorTitle", ex.Message, "error"), true);
                    }
                }
            }
        }

        protected void btnGoToAccount_Click(object sender, EventArgs e) { Response.Redirect("MyAccount.aspx"); }
        protected void btnMyOrders_Click(object sender, EventArgs e) { Response.Redirect("MyOrders.aspx"); }
        protected void btnNavCart_Click(object sender, EventArgs e) { Response.Redirect("Cart.aspx"); }
        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}
