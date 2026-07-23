using MySql.Data.MySqlClient;
using System;
using System.Web;
using System.Web.UI;

namespace OFFSIDESHOP
{
    public partial class SmtpSettings : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        // ──────────────────────────────────────────────────────────────
        //  Page_Load — Restricción estricta de seguridad (Solo Owner)
        // ──────────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            // Control de caché para prevenir la recarga de datos sensibles con el botón "Atrás"
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            // GUARDA EXCLUSIVA: Solo se permite la entrada si el rol de sesión es exactamente 1 (Owner)
            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 1)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                LoadCurrentSmtpSettings();
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Carga Inicial de los datos de Configuración SMTP
        // ──────────────────────────────────────────────────────────────
        private void LoadCurrentSmtpSettings()
        {
            string query = "SELECT SenderName, SenderEmail, AppPassword FROM smtp_settings WHERE ID = 1;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtSenderName.Text = reader["SenderName"].ToString();
                                txtSenderEmail.Text = reader["SenderEmail"].ToString();
                                txtAppPassword.Text = reader["AppPassword"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', 'Could not load SMTP settings: {HttpUtility.HtmlEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Guardar / Actualizar Configuración SMTP de Forma Segura
        // ──────────────────────────────────────────────────────────────
        protected void btnSaveSettings_Click(object sender, EventArgs e)
        {
            // Doble validación en el servidor por seguridad extrema
            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 1)
            {
                throw new UnauthorizedAccessException("Only Owners are allowed to change mail server settings.");
            }

            string name = txtSenderName.Text.Trim();
            string email = txtSenderEmail.Text.Trim();

            // Reemplazamos los espacios que el usuario pueda ingresar por accidente de la clave de Google
            string password = txtAppPassword.Text.Trim().Replace(" ", "");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'All fields with an asterisk (*) are required.', 'warning');</script>";
                return;
            }

            string query = "UPDATE smtp_settings SET SenderName = @Name, SenderEmail = @Email, AppPassword = @Password WHERE ID = 1;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            alerta.Text = "<script>Swal.fire('Success', 'SMTP configuration updated successfully.', 'success');</script>";
                            LoadCurrentSmtpSettings(); // Volvemos a leer para mantener refrescado el front
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Database Error', '{HttpUtility.HtmlEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Botones de Redirección e Interfaz de Navegación Lateral (Sidebar)
        // ──────────────────────────────────────────────────────────────
        protected void btnManageProducts_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageProducts.aspx");
        }

        protected void btnManageOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageOrders.aspx");
        }

        protected void btnAddLeague_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddLeague.aspx");
        }

        protected void btnAddTeam_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddTeam.aspx");
        }

        protected void btnAddBrand_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddBrand.aspx");
        }

        protected void btnManageUsers_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageUsers.aspx");
        }

        protected void btnAdminBanners_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminBanners.aspx");
        }

        protected void btnSmtpSettings_Click(object sender, EventArgs e)
        {
            Response.Redirect("SmtpSettings.aspx");
        }
        protected void btnManageOffers_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageOffers.aspx");
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
        protected void btnStats_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminStats.aspx");
        }
        protected void btnManageCoupons_Click(object sender, EventArgs e)
        { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e)
        { Response.Redirect("AdminAudit.aspx"); }
    }
}