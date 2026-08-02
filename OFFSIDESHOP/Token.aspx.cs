using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Token : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                rptCarousel.DataSource = AuthCarousel.GetActiveSlides();
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
                            alerta.Text = "<script>Swal.fire({ title: 'Verification code correct!', text: 'Proceeding to reset your password...', icon: 'success', confirmButtonColor: '#FFC800' });</script>";

                            // Guardamos confirmación en sesión
                            Session["TokenVerified"] = true;

                            // Redirección limpia
                            Response.AddHeader("REFRESH", "3;URL=ChangePassword.aspx");
                            txttoken.Text = "";
                        }
                        else
                        {
                            // Alerta de código erróneo
                            alerta.Text = "<script>Swal.fire({ title: 'Incorrect Code!', text: 'The code does not match, please check your email again.', icon: 'error', confirmButtonColor: '#FFC800' });</script>";
                            txttoken.Text = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Captura de errores de base de datos para prevenir pantallas blancas de caída
                        alerta.Text = $"<script>Swal.fire({{ title: 'Database Error', text: '{HttpUtility.HtmlEncode(ex.Message)}', icon: 'error', confirmButtonColor: '#FFC800' }});</script>";
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
                alerta.Text = "<script>Swal.fire({ title: 'OOPS!', text: 'Do not leave empty spaces.', icon: 'warning', confirmButtonColor: '#FFC800' });</script>";
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