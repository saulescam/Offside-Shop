using MySql.Data.MySqlClient;
using Nemiro.OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nemiro.OAuth.Clients;

namespace OFFSIDESHOP
{
    public partial class Login : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!OAuthManager.IsRegisteredClient("google"))
            {
                OAuthManager.RegisterClient
                (
                    new GoogleClient(
                        System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"],
                        System.Configuration.ConfigurationManager.AppSettings["GoogleClientSecret"]
                    )
                );
            }

            if (!IsPostBack)
            {
                rptCarousel.DataSource = AuthCarousel.GetActiveSlides();
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
                    Response.Redirect("DeliveryDashboard.aspx");
            }
        }

        protected void btnEntrar_Click(object sender, EventArgs e)
        {
            string rawUser = TxtUsuario.Text.Trim();
            string rawPass = TxtContra.Text.Trim();

            if (string.IsNullOrWhiteSpace(rawUser) || string.IsNullOrWhiteSpace(rawPass))
            {
                string scriptBlank = @"
            setTimeout(function() {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'OOPS',
                        text: 'Do not leave any blank spaces',
                        icon: 'error',
                        confirmButtonColor: '#FFC800'
                    });
                } else {
                    alert('OOPS: Do not leave any blank spaces');
                }
            }, 50);";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "logBlank", scriptBlank, true);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Retrieve user ID, name, role, permissions, and stored password hash in a single parameterized query
                    string query = @"SELECT u.Id_User, u.Name_User, u.Id_Role, u.Password, 
                                            IFNULL(u.Perm_Products, 0) AS Perm_Products, 
                                            IFNULL(u.Perm_Orders, 0) AS Perm_Orders, 
                                            IFNULL(u.Perm_Offers, 0) AS Perm_Offers, 
                                            IFNULL(u.Perm_Coupons, 0) AS Perm_Coupons, 
                                            IFNULL(u.Perm_Banners, 0) AS Perm_Banners,
                                            IFNULL(u.Perm_Tickets, 0) AS Perm_Tickets
                                     FROM users u WHERE (u.Name_User = @identifier OR u.Mail = @identifier) LIMIT 1;";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@identifier", rawUser);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool loginSuccess = false;
                        if (reader.Read())
                        {
                            string storedHash = reader["Password"].ToString();

                            if (Security.Verificar(rawPass, storedHash))
                            {
                                loginSuccess = true;
                                int userId = Convert.ToInt32(reader["Id_User"]);
                                string userName = reader["Name_User"].ToString();
                                int idRole = Convert.ToInt32(reader["Id_Role"]);

                                // --- Read PBAC boolean values ---
                                bool permProducts = Convert.ToBoolean(reader["Perm_Products"]);
                                bool permOrders = Convert.ToBoolean(reader["Perm_Orders"]);
                                bool permOffers = Convert.ToBoolean(reader["Perm_Offers"]);
                                bool permCoupons = Convert.ToBoolean(reader["Perm_Coupons"]);
                                bool permBanners = Convert.ToBoolean(reader["Perm_Banners"]);
                                bool permTickets = Convert.ToBoolean(reader["Perm_Tickets"]);

                                // --- Clear any legacy / deprecated session keys ---
                                Session.Remove("username");
                                Session.Remove("usermane");

                                // --- Store role integer ---
                                Session["UserRole"] = idRole;

                                // --- Store PBAC Session variables ---
                                if (idRole == 1)
                                {
                                    // Owner siempre tiene acceso a todo
                                    Session["Perm_Products"] = true;
                                    Session["Perm_Orders"] = true;
                                    Session["Perm_Offers"] = true;
                                    Session["Perm_Coupons"] = true;
                                    Session["Perm_Banners"] = true;
                                    Session["Perm_Tickets"] = true;
                                }
                                else if (idRole == 2)
                                {
                                    // Admin obtiene sus permisos exactos
                                    Session["Perm_Products"] = permProducts;
                                    Session["Perm_Orders"] = permOrders;
                                    Session["Perm_Offers"] = permOffers;
                                    Session["Perm_Coupons"] = permCoupons;
                                    Session["Perm_Banners"] = permBanners;
                                    Session["Perm_Tickets"] = permTickets;
                                }
                                else
                                {
                                    // Clientes y Repartidores no tienen permisos administrativos de tienda
                                    Session["Perm_Products"] = false;
                                    Session["Perm_Orders"] = false;
                                    Session["Perm_Offers"] = false;
                                    Session["Perm_Coupons"] = false;
                                    Session["Perm_Banners"] = false;
                                    Session["Perm_Tickets"] = false;
                                }

                                if (idRole == 1 || idRole == 2)
                                {
                                    // Owner or Admin
                                    Session["Admin"] = HttpUtility.HtmlEncode(userName);
                                    Session["Id_User"] = userId;
                                    reader.Close();

                                    // Verificamos AQUÍ si hay un ID pendiente, antes del Redirect general
                                    if (Session["PendingShirtId"] != null)
                                    {
                                        string redirectId = Session["PendingShirtId"].ToString();
                                        Session.Remove("PendingShirtId");
                                        Response.Redirect($"DetailsShirt.aspx?id={redirectId}");
                                    }
                                    else
                                    {
                                        Response.Redirect("Dashboard.aspx");
                                    }
                                }
                                else if (idRole == 3)
                                {
                                    // Customer
                                    Session["Customer"] = HttpUtility.HtmlEncode(userName);
                                    Session["Id_User"] = userId;
                                    reader.Close();

                                    // Verificamos AQUÍ si hay un ID pendiente, antes del Redirect general
                                    if (Session["PendingShirtId"] != null)
                                    {
                                        string redirectId = Session["PendingShirtId"].ToString();
                                        Session.Remove("PendingShirtId");
                                        Response.Redirect($"DetailsShirt.aspx?id={redirectId}");
                                    }
                                    else
                                    {
                                        Response.Redirect("Homepage.aspx");
                                    }
                                }
                                else if (idRole == 4)
                                {
                                    // Delivery (Repartidor)
                                    Session["Delivery"] = HttpUtility.HtmlEncode(userName);
                                    Session["Id_User"] = userId;
                                    reader.Close();
                                    Response.Redirect("DeliveryDashboard.aspx");
                                }
                                else
                                {
                                    // Unknown role — deny access
                                    string scriptDenied = @"
                            setTimeout(function() {
                                if (typeof Swal !== 'undefined') {
                                    Swal.fire({
                                        title: 'Access Denied',
                                        text: 'Your account has an unknown role. Contact an administrator.',
                                        icon: 'error',
                                        confirmButtonColor: '#FFC800'
                                    });
                                } else {
                                    alert('Access Denied: Your account has an unknown role. Contact an administrator.');
                                }
                            }, 50);";
                                    ScriptManager.RegisterStartupScript(this, this.GetType(), "logDenied", scriptDenied, true);
                                    TxtContra.Text = "";
                                }
                            }
                        }

                        if (!loginSuccess)
                        {
                            string scriptWrong = @"
                        setTimeout(function() {
                            if (typeof Swal !== 'undefined') {
                                Swal.fire({
                                    title: 'Something went wrong',
                                    text: 'User or password are incorrect',
                                    icon: 'error',
                                    confirmButtonColor: '#FFC800'
                                });
                            } else {
                                alert('Something went wrong: User or password are incorrect');
                            }
                        }, 50);";
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "logWrong", scriptWrong, true);
                            TxtContra.Text = "";
                            TxtUsuario.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = HttpUtility.JavaScriptStringEncode(ex.Message);
                string scriptError = $@"
            setTimeout(function() {{
                if (typeof Swal !== 'undefined') {{
                    Swal.fire({{
                        title: 'Error',
                        text: '{errorMsg}',
                        icon: 'error',
                        confirmButtonColor: '#FFC800'
                    }});
                }} else {{
                    alert('Error: ' + '{errorMsg}');
                }}
            }}, 50);";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "logError", scriptError, true);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("SignUp.aspx");
        }

        protected void btnregistro_Click(object sender, EventArgs e)
        {
            Response.Redirect("SignUp.aspx");
        }

        protected void btnGoogleLogin_Click(object sender, EventArgs e)
        {
            string callbackUrl = System.Configuration.ConfigurationManager.AppSettings["GoogleRedirectUri"];
            OAuthWeb.RedirectToAuthorization("google", callbackUrl);
        }
    }
}