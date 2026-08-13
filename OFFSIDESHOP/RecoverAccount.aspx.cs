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
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";
                rptCarousel.DataSource = AuthCarousel.GetActiveSlides(currentLang);
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

                    using (MySqlConnection conexion = new MySqlConnection(connectionString))
                    {
                        conexion.Open();

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
                            // CAMBIO AQUI: Usar el Literal 'alertas'
                            string scriptNotFound = AlertHelper.GetSafeAlertScript(this, "Alert_Login_WrongTitle", "Alert_Recover_UserNotFound", "error");
                            alertas.Text = scriptNotFound;
                            return;
                        }

                        ForgetGlobalPassword.ValorGlobal = actualUsername;

                        Random rand = new Random();
                        string randomCode = rand.Next(100000, 999999).ToString();
                        string eventName = "ev_reset_" + actualUsername.Replace("@", "_").Replace(".", "_").Replace(" ", "_");

                        using (MySqlCommand dropEvento = new MySqlCommand($"DROP EVENT IF EXISTS {eventName}", conexion))
                        {
                            dropEvento.ExecuteNonQuery();
                        }

                        using (MySqlCommand comando = new MySqlCommand("UPDATE users SET Token = @token WHERE Name_User = @user", conexion))
                        {
                            comando.Parameters.AddWithValue("@token", randomCode);
                            comando.Parameters.AddWithValue("@user", actualUsername);
                            comando.ExecuteNonQuery();
                        }

                        string createEventQuery = $"CREATE EVENT {eventName} ON SCHEDULE AT CURRENT_TIMESTAMP + INTERVAL 2 MINUTE DO UPDATE users SET Token = NULL WHERE Name_User = @user";
                        using (MySqlCommand evento = new MySqlCommand(createEventQuery, conexion))
                        {
                            evento.Parameters.AddWithValue("@user", actualUsername);
                            evento.ExecuteNonQuery();
                        }

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

                        EmailService.SendPasswordRecoveryToken(mail, nombrecliente, randomCode);
                    }

                    // CAMBIO AQUI: Usar el Literal 'alertas'
                    string scriptSent = AlertHelper.GetRedirectAlertScript(this, "Alert_Recover_SentTitle", "Alert_Recover_EmailSent", "success", 3000, "Token.aspx");
                    alertas.Text = scriptSent;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error en recuperación: " + ex.Message);

                    // CAMBIO AQUI: Usar el Literal 'alertas'
                    string scriptErr = AlertHelper.GetSafeAlertScript(this, "Alert_Login_WrongTitle", "Alert_Recover_ErrorProcessing", "error");
                    alertas.Text = scriptErr;
                }
            }
            else
            {
                // CAMBIO AQUI: Usar el Literal 'alertas'
                string scriptBlank = AlertHelper.GetSafeAlertScript(this, "Alert_Login_OopsTitle", "Alert_Login_BlankSpaces", "error");
                alertas.Text = scriptBlank;
            }
        }

        protected void btnregistro_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx");
        }
    }
}