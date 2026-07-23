using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web;
using System.Web.UI;

namespace OFFSIDESHOP
{
    public partial class AboutUs : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            phNavbarGuest.Visible = false;
            phNavbarUser.Visible = false;
            phNavbarAdmin.Visible = false;

            // Evaluamos si el usuario ha iniciado sesión
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
                else if (userRole == 3)
                {
                    phNavbarUser.Visible = true;
                    if (!IsPostBack)
                    {
                        CargarDatosPerfilUsuario();
                    }
                }
                else
                {
                    phNavbarGuest.Visible = true;
                }
            }

            if (!IsPostBack)
            {
                ActualizarContadorCarrito();
            }
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
                                    lblUserEmail.Text = reader["Mail"].ToString();
                                }
                                else
                                {
                                    lblFullName.Text = "User not found";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            lblFullName.Text = "Error: " + ex.Message;
                        }
                    }
                }
            }
            else
            {
                lblFullName.Text = "No active session";
            }

            if (upPerfil != null)
            {
                upPerfil.Update();
            }
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
                    {
                        totalProducts += Convert.ToInt32(row["Quantity"]);
                    }
                }
                lblCartCount.Text = totalProducts.ToString();
            }
            else
            {
                lblCartCount.Text = "0";
            }
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        protected void btnGoToAccount_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyAccount.aspx");
        }

        protected void btnMyOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyOrders.aspx");
        }

        protected void btnNavCart_Click(object sender, EventArgs e)
        {
            Response.Redirect("Cart.aspx");
        }
    }
}