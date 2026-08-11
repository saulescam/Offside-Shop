using MySql.Data.MySqlClient;
using System;
using System.Web;
using System.Web.UI;

namespace OFFSIDESHOP
{
    public partial class RecuperarContrasena : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                rptCarousel.DataSource = AuthCarousel.GetActiveSlides();
                rptCarousel.DataBind();
            }
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }

        protected void Unnamed1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtcuenta.Text.Trim()))
            {
                try
                {
                    string inputUser = txtcuenta.Text.Trim();

                    // Modificación: Usamos la connectionString centralizada del Web.config
                    using (MySqlConnection conexion = new MySqlConnection(connectionString))
                    {
                        conexion.Open();

                        // Seguridad: Cambiado a consulta parametrizada para evitar SQL Injection
                        string queryUser = "SELECT Name_User FROM users WHERE Name_User = @Input OR Mail = @Input;";
                        string actualUsername = "";

                        using (MySqlCommand obtenerUser = new MySqlCommand(queryUser, conexion))
                        {
                            obtenerUser.Parameters.AddWithValue("@Input", inputUser);
                            object resUser = obtenerUser.ExecuteScalar();
                            if (resUser != null) actualUsername = resUser.ToString();
                        }

                        if (string.IsNullOrEmpty(actualUsername))
                        {
                            alertas.Text = AlertHelper.GetAlertScript(this, "Alert_Login_WrongTitle", "Alert_Recover_UserNotFound", "error");
                            return;
                        }

                        ForgetGlobalPassword.ValorGlobal = actualUsername;

                        // Segmento para generar un número aleatorio de 6 dígitos
                        Random rand = new Random();
                        string randomCode = rand.Next(100000, 999999).ToString();
                        string eventName = "ev_reset_" + actualUsername.Replace("@", "_").Replace(".", "_").Replace(" ", "_");

                        // Limpiar evento anterior si existe para este usuario
                        using (MySqlCommand dropEvento = new MySqlCommand($"DROP EVENT IF EXISTS {eventName}", conexion))
                        {
                            dropEvento.ExecuteNonQuery();
                        }

                        // Actualizar el token del usuario
                        using (MySqlCommand comando = new MySqlCommand("UPDATE users SET Token = @token WHERE Name_User = @user", conexion))
                        {
                            comando.Parameters.AddWithValue("@token", randomCode);
                            comando.Parameters.AddWithValue("@user", actualUsername);
                            comando.ExecuteNonQuery();
                        }

                        // Crear evento para borrar el token después de 2 minutos
                        string createEventQuery = $"CREATE EVENT {eventName} ON SCHEDULE AT CURRENT_TIMESTAMP + INTERVAL 2 MINUTE DO UPDATE users SET Token = NULL WHERE Name_User = @user";
                        using (MySqlCommand evento = new MySqlCommand(createEventQuery, conexion))
                        {
                            evento.Parameters.AddWithValue("@user", actualUsername);
                            evento.ExecuteNonQuery();
                        }

                        // Consultas optimizadas usando parámetros
                        string mail = "";
                        string nombrecliente = "";

                        string queryData = "SELECT Mail, Name FROM users WHERE Name_User = @user;";
                        using (MySqlCommand obtenerDatos = new MySqlCommand(queryData, conexion))
                        {
                            obtenerDatos.Parameters.AddWithValue("@user", actualUsername);
                            using (MySqlDataReader reader = obtenerDatos.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    mail = reader["Mail"].ToString();
                                    nombrecliente = reader["Name"].ToString();
                                }
                            }
                        }

                        // ──────────────────────────────────────────────────────────────
                        // LLAMADA A LA NUEVA CLASE DINÁMICA DE EMAIL
                        // ──────────────────────────────────────────────────────────────
                        EmailService.SendPasswordRecoveryToken(mail, nombrecliente, randomCode);
                    }

                    alertas.Text = AlertHelper.GetRedirectAlertScript(this, "Alert_Recover_SentTitle", "Alert_Recover_EmailSent", "success", 3000, "Token.aspx");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error en recuperación: " + ex.Message);
                    alertas.Text = AlertHelper.GetAlertScript(this, "Alert_Login_WrongTitle", "Alert_Recover_ErrorProcessing", "error");
                }
            }
            else
            {
                alertas.Text = AlertHelper.Error(this, "Alert_Login_BlankSpaces");
            }
        }

        protected void btnregistro_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx");
        }
    }
}