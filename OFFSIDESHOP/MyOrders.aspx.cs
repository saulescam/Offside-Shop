using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class MyOrders : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. CONTROL DE CACHÉ
            Response.Buffer = true;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            // 2. FILTRO DE SEGURIDAD
            if (Session["UserRole"] == null || Session["Customer"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // 3. ADMINISTRACIÓN DE NAVBARS
            phNavbarGuest.Visible = false;
            phNavbarUser.Visible = false;

            int userRole = Convert.ToInt32(Session["UserRole"]);
            if (userRole == 1 || userRole == 2 || userRole == 3)
            {
                phNavbarUser.Visible = true;
            }
            else
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // 4. CARGA INICIAL DE DATOS
            if (!IsPostBack)
            {
                LoadOrderHistory();
                CargarDatosPerfilUsuario();
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
                            MySqlDataReader reader = cmd.ExecuteReader();

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

            // Solicitamos al UpdatePanel actualizarse con los datos recuperados
            if (upPerfil != null)
            {
                upPerfil.Update();
            }
        }
        private void LoadOrderHistory()
        {
            string activeUser = Session["Customer"].ToString();

            // Consulta ajustada EXACTAMENTE a tus tablas 'orders', 'users', 'cities' y 'order_statuses'
            string queryOrders = @"
                SELECT o.Id_Order AS id_order, 
                       o.OrderDate AS order_date, 
                       o.Address AS shipping_address, 
                       c.city_name AS city, 
                       o.Total AS total_amount,
                       s.Status_Name AS order_status
                FROM orders o
                INNER JOIN users u ON o.Id_User = u.Id_User
                INNER JOIN cities c ON o.Id_City = c.id_city
                INNER JOIN order_statuses s ON o.Id_Status = s.Id_Status
                WHERE u.Name_User = @activeUser OR u.Mail = @activeUser
                ORDER BY o.Id_Order DESC;";

            using (MySqlConnection connection = data.ObtenerConexion())
            {
                using (MySqlCommand cmd = new MySqlCommand(queryOrders, connection))
                {
                    cmd.Parameters.AddWithValue("@activeUser", activeUser);
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dtOrders = new DataTable();
                        try
                        {
                            connection.Open();
                            da.Fill(dtOrders);

                            if (dtOrders.Rows.Count > 0)
                            {
                                // Vinculamos el evento para cargar los productos de cada orden
                                rptOrders.ItemDataBound -= rptOrders_ItemDataBound;
                                rptOrders.ItemDataBound += new RepeaterItemEventHandler(rptOrders_ItemDataBound);

                                rptOrders.DataSource = dtOrders;
                                rptOrders.DataBind();

                                lblNoOrders.Visible = false;
                                rptOrders.Visible = true;
                            }
                            else
                            {
                                lblNoOrders.Text = "You haven't placed any orders yet!";
                                lblNoOrders.Visible = true;
                                rptOrders.Visible = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.Write("<script>alert('Error loading orders: " + ex.Message.Replace("'", "\\'") + "');</script>");
                        }
                    }
                }
            }
        }

        // CONTROLADOR PARA CARGAR LOS PRODUCTOS DE CADA COMPRA
        protected void rptOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView rowView = (DataRowView)e.Item.DataItem;
                int idOrder = Convert.ToInt32(rowView["id_order"]);

                Repeater rptProducts = (Repeater)e.Item.FindControl("rptProducts");

                if (rptProducts != null)
                {
                    // Ajustado exactamente a las columnas de tu tabla `order_details`
                    // Como tu tabla no incluye la columna 'Brand', quitamos el tag de la consulta
                    string queryProducts = @"
                        SELECT ProductName AS Product, 
                               Size, 
                               Quantity, 
                               Price 
                        FROM order_details 
                        WHERE Id_Order = @id_order;";

                    using (MySqlConnection connection = data.ObtenerConexion())
                    {
                        using (MySqlCommand cmd = new MySqlCommand(queryProducts, connection))
                        {
                            cmd.Parameters.AddWithValue("@id_order", idOrder);
                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                            {
                                DataTable dtProducts = new DataTable();
                                try
                                {
                                    connection.Open();
                                    da.Fill(dtProducts);

                                    rptProducts.DataSource = dtProducts;
                                    rptProducts.DataBind();
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("Error loading details: " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.RemoveAll();
            Session.Abandon();
            Response.Redirect("Homepage.aspx");
        }
        protected void btnNavCart_Click(object sender, EventArgs e)
        {
            // Redirect straight to cart page on click
            Response.Redirect("Cart.aspx");
        }
        protected void btnbackshop_Click(object sender, EventArgs e)
        {
            Response.Redirect("Homepage.aspx");
        }
        protected void btnGoToAccount_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyAccount.aspx");
        }
        protected string FormatJerseyName(object nameObj)
        {
            if (nameObj == null || nameObj == DBNull.Value) return "";
            
            string name = nameObj.ToString().ToLower().Trim();
            System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(name);
        }
    }
}