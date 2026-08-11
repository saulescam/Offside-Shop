using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class ChangePassword : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                rptCarousel.DataSource = AuthCarousel.GetActiveSlides();
                rptCarousel.DataBind();
            }

            // Si ya inició sesión, no puede ingresar a esta página
            if (Session["Id_User"] != null)
            {
                Response.Redirect("Homepage.aspx");
                return;
            }

            // Validar que el token haya sido verificado y que exista el usuario global
            if (Session["TokenVerified"] == null || !(bool)Session["TokenVerified"] || string.IsNullOrEmpty(ForgetGlobalPassword.ValorGlobal))
            {
                Response.Redirect("Login.aspx");
            }
        }

        protected void actualizar_Click(object sender, EventArgs e)
        {
            if (txtpassword1.Text.Trim() != "" && txtpassword2.Text.Trim() != "")
            {
                if (txtpassword2.Text == txtpassword1.Text)
                {
                    string userforgot;
                    string contraEsencriptada;
                    userforgot = ForgetGlobalPassword.ValorGlobal;
                    MySqlConnection conexion = data.ObtenerConexion(); string query = "UPDATE users SET Password = @Password WHERE Name_User = @nombre_usuario";
                    conexion.Open();
                    contraEsencriptada = Security.Encrypt(txtpassword2.Text);
                    MySqlCommand comando = new MySqlCommand(query, conexion);
                    comando.Parameters.AddWithValue("@Nombre_Usuario", userforgot);
                    comando.Parameters.AddWithValue("@Password", contraEsencriptada);
                    comando.ExecuteNonQuery();
                    conexion.Close();
                    alerta.Text = AlertHelper.GetRedirectAlertScript(this, "Alert_SuccessTitle", "Alert_ChangePass_Success", "success", 3000, "Login.aspx");
                    
                    // Limpiar estados de recuperación
                    Session["TokenVerified"] = null;
                    ForgetGlobalPassword.ValorGlobal = "";

                    txtpassword1.Text = "";
                    txtpassword2.Text = "";
                }
                else
                {
                    alerta.Text = AlertHelper.GetAlertScript(this, "Alert_ChangePass_MismatchTitle", "Alert_ChangePass_MismatchText", "error");
                    txtpassword1.Text = "";
                    txtpassword2.Text = "";
                }
            }
            else
            {
                alerta.Text = AlertHelper.GetAlertScript(this, "Alert_Login_OopsTitle", "Alert_ChangePass_EmptyFields", "warning");
                txtpassword1.Text = "";
                txtpassword2.Text = "";
            }
        }
        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }
    }
}