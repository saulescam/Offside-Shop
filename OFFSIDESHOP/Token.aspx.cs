using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Token : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";
                rptCarousel.DataSource = AuthCarousel.GetActiveSlides(currentLang);
                rptCarousel.DataBind();
            }

            // Si ya inició sesión, no puede ingresar a recuperar contraseña
            if (Session["Id_User"] != null)
            {
                Response.Redirect("Homepage.aspx");
                return;
            }

            // Validar que exista el usuario global en sesión o variable antes de proceder
            if (string.IsNullOrEmpty(ForgetGlobalPassword.ValorGlobal))
            {
                Response.Redirect("Login.aspx");
            }
        }

        protected void token_Click(object sender, EventArgs e)
        {
            string tokenusu = txttoken.Text.Trim();

            if (!string.IsNullOrEmpty(tokenusu))
            {
                // Obtenemos la conexión limpia desde tu clase de datos
                using (MySqlConnection connection = data.ObtenerConexion())
                {
                    try
                    {
                        // IMPORTANTE: Asegurar que la conexión esté abierta de forma explícita
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Open();
                        }

                        // 1. Consulta Parametrizada para verificar el Token
                        string cmdText = "SELECT COALESCE(Id_User, 0) FROM users WHERE Token = @Token;";
                        int retorno = 0;

                        using (MySqlCommand comando = new MySqlCommand(cmdText, connection))
                        {
                            comando.Parameters.AddWithValue("@Token", tokenusu);
                            object result = comando.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                retorno = Convert.ToInt32(result);
                            }
                        }

                        // Si el token es correcto y pertenece a un usuario válido
                        if (retorno > 0)
                        {
                            string actualUsername = ForgetGlobalPassword.ValorGlobal;

                            // 2. Limpiar el token asignado al usuario
                            string updateQuery = "UPDATE users SET Token = NULL WHERE Name_User = @user";
                            using (MySqlCommand clearToken = new MySqlCommand(updateQuery, connection))
                            {
                                clearToken.Parameters.AddWithValue("@user", actualUsername);
                                clearToken.ExecuteNonQuery();
                            }

                            // 3. Borrar el evento temporal de MySQL de forma segura
                            string eventName = "ev_reset_" + actualUsername.Replace("@", "_").Replace(".", "_").Replace(" ", "_");
                            string dropEventQuery = $"DROP EVENT IF EXISTS `{eventName}`;"; // Encapsulado en backticks por seguridad de sintaxis de MySQL
                            using (MySqlCommand dropEvento = new MySqlCommand(dropEventQuery, connection))
                            {
                                dropEvento.ExecuteNonQuery();
                            }

                            // Alerta de éxito con SweetAlert2
                            alerta.Text = AlertHelper.GetRedirectAlertScript(this, "Alert_Token_SuccessTitle", "Alert_Token_SuccessText", "success", 3000, "ChangePassword.aspx");

                            // Guardamos confirmación en sesión
                            Session["TokenVerified"] = true;

                            txttoken.Text = "";
                        }
                        else
                        {
                            // Alerta de código erróneo
                            alerta.Text = AlertHelper.GetAlertScript(this, "Alert_Token_IncorrectTitle", "Alert_Token_IncorrectText", "error");
                            txttoken.Text = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Captura de errores de base de datos para prevenir pantallas blancas de caída
                        alerta.Text = AlertHelper.GetAlertScript(this, "Alert_DatabaseErrorTitle", HttpUtility.HtmlEncode(ex.Message), "error");
                    }
                    finally
                    {
                        // Asegurar el cierre de la conexión en caso de que falle algún execute
                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            connection.Close();
                        }
                    }
                }
            }
            else
            {
                alerta.Text = AlertHelper.GetAlertScript(this, "Alert_Login_OopsTitle", "Alert_Login_BlankSpaces", "warning");
                txttoken.Text = "";
            }
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }
    }
}