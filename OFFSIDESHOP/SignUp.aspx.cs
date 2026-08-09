using Nemiro.OAuth;
using MySql.Data.MySqlClient;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class SignUp : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;
        protected override void InitializeCulture()
        {
            if (Session["Language"] != null)
            {
                string lang = Session["Language"].ToString();
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
            }
            base.InitializeCulture();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. Obtener idioma actual de la sesión (por defecto "en")
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                // 2. Cargar el carrusel pasando el idioma
                rptCarousel.DataSource = AuthCarousel.GetActiveSlides(currentLang);
                rptCarousel.DataBind();
            }

            // If already authenticated, redirect to the appropriate page
            if (Session["UserRole"] != null)
            {
                int role = Convert.ToInt32(Session["UserRole"]);
                if (role == 1 || role == 2)
                    Response.Redirect("Dashboard.aspx");
                else if (role == 3)
                    Response.Redirect("Homepage.aspx");
                else if (role == 4)
                    Response.Redirect("DeliveryDashboard.aspx"); // Redirección automática para el Repartidor
            }
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (txtusuario.Text.Trim() != "" && txtclave.Text.Trim() != "" && txtconfirm.Text.Trim() != ""
            && txtfirst.Text.Trim() != "" && txtapellido.Text.Trim() != "" && txtgmail.Text.Trim() != "")
            {
                if (txtclave.Text == txtconfirm.Text)
                {
                    string nombre = txtfirst.Text.Trim();
                    string apellido = txtapellido.Text.Trim();
                    string usuario = txtusuario.Text.Trim();
                    string correo = txtgmail.Text.Trim();
                    string hash = Security.Encrypt(txtclave.Text);

                    // 1. Verificamos duplicados en la BD
                    using (MySqlConnection con = new MySqlConnection(connectionString))
                    {
                        con.Open();
                        string checkQuery = "SELECT COUNT(*) FROM users WHERE Name_User = @u OR Mail = @m";
                        MySqlCommand cmd = new MySqlCommand(checkQuery, con);
                        cmd.Parameters.AddWithValue("@u", usuario);
                        cmd.Parameters.AddWithValue("@m", correo);

                        int exists = Convert.ToInt32(cmd.ExecuteScalar());
                        if (exists > 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "regExists", "Swal.fire('User Already Exists', 'The username or email is already registered.', 'error');", true);
                            return;
                        }
                    }

                    // 2. Generamos el Token
                    Random rnd = new Random();
                    string verificationToken = rnd.Next(100000, 999999).ToString();

                    // 3. Guardamos en Session
                    Session["Reg_Name"] = nombre;
                    Session["Reg_LastName"] = apellido;
                    Session["Reg_User"] = usuario;
                    Session["Reg_Email"] = correo;
                    Session["Reg_Hash"] = hash;
                    Session["Reg_Token"] = verificationToken;

                    try
                    {
                        // 4. Enviamos el Correo
                        EmailService.SendRegistrationToken(correo, nombre, verificationToken);

                        // 5. Ocultar Formulario, Mostrar Verificación y Cerrar Spinner
                        pnlRegister.Visible = false;
                        pnlVerify.Visible = true;
                        displayEmail.InnerText = correo;

                        ScriptManager.RegisterStartupScript(this, this.GetType(), "closeSwal", "Swal.close();", true);
                    }
                    catch (Exception)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "mailError", "Swal.fire('Error', 'Failed to send the verification email. Ensure your SMTP settings are correct.', 'error');", true);
                    }
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "regPassword", "Swal.fire('Incorrect Password', 'Please repeat your password correctly.', 'error');", true);
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "regBlank", "Swal.fire('OOPS...', 'Do not leave any blank fields.', 'warning');", true);
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            if (Session["Reg_Token"] != null && txtToken.Text.Trim() == Session["Reg_Token"].ToString())
            {
                // Código válido: Insertar usuario
                try
                {
                    using (MySqlConnection con = new MySqlConnection(connectionString))
                    {
                        con.Open();
                        string query = "INSERT INTO users (Name, LastName, Name_User, Password, Mail, Id_Role) VALUES (@n, @l, @u, @p, @m, 3)";
                        MySqlCommand cmd = new MySqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@n", Session["Reg_Name"].ToString());
                        cmd.Parameters.AddWithValue("@l", Session["Reg_LastName"].ToString());
                        cmd.Parameters.AddWithValue("@u", Session["Reg_User"].ToString());
                        cmd.Parameters.AddWithValue("@p", Session["Reg_Hash"].ToString());
                        cmd.Parameters.AddWithValue("@m", Session["Reg_Email"].ToString());
                        cmd.ExecuteNonQuery();
                    }

                    // Limpiar Sesión
                    Session.Remove("Reg_Name"); Session.Remove("Reg_LastName");
                    Session.Remove("Reg_User"); Session.Remove("Reg_Email");
                    Session.Remove("Reg_Hash"); Session.Remove("Reg_Token");

                    string scriptSuccess = @"
                        Swal.fire({
                            title: 'Verified & Registered!',
                            text: 'Your account has been created successfully.',
                            icon: 'success',
                            showConfirmButton: false,
                            timer: 2000
                        }).then(() => {
                            window.location.href = 'Login.aspx';
                        });";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "regSuccess", scriptSuccess, true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "dbError", $"Swal.fire('Error', 'Failed to save user: {ex.Message}', 'error');", true);
                }
            }
            else
            {
                // Código Inválido
                ScriptManager.RegisterStartupScript(this, this.GetType(), "invalidToken", "Swal.fire('Invalid Code', 'The verification code you entered is incorrect.', 'error');", true);
            }
        }

        // Si el usuario se equivocó al escribir su correo, le permitimos regresar al formulario
        protected void btnBack_Click(object sender, EventArgs e)
        {
            pnlVerify.Visible = false;
            pnlRegister.Visible = true;
            txtToken.Text = "";
            Session.Remove("Reg_Token");
        }

        protected void btnGoogleSign_Click(object sender, EventArgs e)
        {
            string callbackUrl = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/ExternalLoginResult.aspx");
            OAuthWeb.RedirectToAuthorization("google", callbackUrl);
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }
    }
}