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
                            ddlReason.DataSource = reader;
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

        protected void ddlReason_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlOrderIssue.Visible = false;
            pnlSellJersey.Visible = false;

            if (string.IsNullOrEmpty(ddlReason.SelectedValue)) return;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT Requires_Order, Requires_Images FROM contact_reasons WHERE Id_ContactReason = @Id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", ddlReason.SelectedValue);
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
                                pnlSellJersey.Visible = reqImages;
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // 1. VALIDACIÃ“N MAESTRA DE SESIÃ“N COACTIVA
            if (Session["Id_User"] == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Login Required', 'You must be logged into your account to submit a support request. Please Log In or Sign Up first.', 'warning');", true);
                return;
            }

            if (string.IsNullOrEmpty(ddlReason.SelectedValue))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'Please select a reason for your request.', 'warning');", true);
                return;
            }

            // 2. VALIDACIONES GENERALES DE TEXTO (Anti-Espacios en blanco y Sanidad)
            if (string.IsNullOrWhiteSpace(txtSubject.Text) || string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Validation Error', 'Subject and Description fields cannot consist of empty spaces.', 'error');", true);
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
                // CORREGIDO: parsedOrderId reemplaza a breweryId
                if (string.IsNullOrEmpty(txtOrderId.Text) || !int.TryParse(txtOrderId.Text, out int parsedOrderId) || parsedOrderId <= 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'Please provide a valid, positive numeric Order ID.', 'warning');", true);
                    return;
                }
                idOrder = parsedOrderId;
            }

            if (pnlSellJersey.Visible)
            {
                // A) ValidaciÃ³n de Presencia de Campos Obligatorios
                if (string.IsNullOrEmpty(txtCondition.Text) || string.IsNullOrEmpty(txtPrice.Text) || string.IsNullOrEmpty(ddlSize.SelectedValue))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'Condition, Jersey Size, and Proposed Price are strictly required fields.', 'warning');", true);
                    return;
                }

                // B) ValidaciÃ³n Estricta de Precio (CORREGIDO: parsedPrice reemplaza a priceVal)
                if (!decimal.TryParse(txtPrice.Text, out decimal parsedPrice) || parsedPrice <= 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Invalid Price', 'Proposed price must be a positive number greater than zero.', 'error');", true);
                    return;
                }

                // C) ValidaciÃ³n Estricta de Escala NumÃ©rica de CondiciÃ³n (CORREGIDO: parsedCondition reemplaza a conditionVal)
                if (!int.TryParse(txtCondition.Text, out int parsedCondition) || parsedCondition < 1 || parsedCondition > 10)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Invalid Condition', 'Condition grade must be a strict integer scale between 1 and 10.', 'error');", true);
                    return;
                }

                // D) AsignaciÃ³n de Datos Validados Seguros
                propPrice = parsedPrice;
                condition = parsedCondition.ToString();
                size = ddlSize.SelectedValue;

                // E) ValidaciÃ³n de ImÃ¡genes Corporativa
                if (!fileImages.HasFiles)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'At least one image is required to evaluate your jersey.', 'warning');", true);
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
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('Invalid Format', 'El archivo {file.FileName} no estÃ¡ permitido. Solo se aceptan JPG, JPEG, PNG y WEBP.', 'error');", true);
                            return;
                        }

                        if (file.ContentLength > maxFileSizeBytes)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('File too large', 'La imagen {file.FileName} supera los 2MB permitidos.', 'error');", true);
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

            // 3. INSERCIÃ“N PARAMETRIZADA INMUNE A INYECCIÃ“N SQL
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

                        // Limpiar formulario tras Ã©xito transaccional
                        ddlReason.SelectedIndex = 0;
                        txtSubject.Text = "";
                        txtMessage.Text = "";
                        txtOrderId.Text = "";
                        txtCondition.Text = "";
                        txtPrice.Text = "";
                        if (ddlSize.Items.Count > 0) ddlSize.SelectedIndex = 0;
                        pnlOrderIssue.Visible = false;
                        pnlSellJersey.Visible = false;

                        ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Request Submitted', 'Your request has been successfully submitted to our support team.', 'success');", true);
                    }
                    catch (Exception ex)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('Error', 'Could not submit your request. Error: {ex.Message.Replace("'", "")}', 'error');", true);
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
