using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class MyOrders : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. CONTROL DE CACHÃ‰
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

            // 3. ADMINISTRACIÃ“N DE NAVBARS
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

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
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
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            string statusColumn = isSpanish ? "s.Status_Name_es" : "s.Status_Name";

            // Consulta ajustada EXACTAMENTE a tus tablas 'orders', 'users', 'cities' y 'order_statuses'
            string queryOrders = $@"
                SELECT o.Id_Order AS id_order, 
                       o.OrderDate AS order_date, 
                       o.Address AS shipping_address, 
                       c.city_name AS city, 
                       o.Total AS total_amount,
                       o.Id_Status AS id_status,
                       {statusColumn} AS order_status
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
                                lblNoOrders.Text = GetGlobalResourceObject("Strings", "MyOrders_NoOrders")?.ToString() ?? "You haven't placed any orders yet!";
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
        protected string GetLocalizedStatusName(object idStatusObj, object statusNameObj)
        {
            int idStatus = 0;
            if (idStatusObj != null && idStatusObj != DBNull.Value)
            {
                int.TryParse(idStatusObj.ToString(), out idStatus);
            }
            
            switch (idStatus)
            {
                case 1: return GetGlobalResourceObject("Strings", "OrderStatus_Pending_Title")?.ToString() ?? "Pending";
                case 2: return GetGlobalResourceObject("Strings", "OrderStatus_Paid_Title")?.ToString() ?? "Paid";
                case 3: return GetGlobalResourceObject("Strings", "OrderStatus_Shipped_Title")?.ToString() ?? "Shipped";
                case 4: return GetGlobalResourceObject("Strings", "OrderStatus_Delivered_Title")?.ToString() ?? "Delivered";
                case 5: return GetGlobalResourceObject("Strings", "OrderStatus_Cancelled_Title")?.ToString() ?? "Cancelled";
                case 6: return GetGlobalResourceObject("Strings", "OrderStatus_RefundReq_Title")?.ToString() ?? "Refund Requested";
                case 7: return GetGlobalResourceObject("Strings", "OrderStatus_Refunded_Title")?.ToString() ?? "Refunded";
                case 8: return GetGlobalResourceObject("Strings", "OrderStatus_RefundDeclined_Title")?.ToString() ?? "Refund Rejected";
                case 9: return GetGlobalResourceObject("Strings", "OrderStatus_Packaged_Title")?.ToString() ?? "Ready for Pickup";
                default:
                    return statusNameObj?.ToString() ?? "";
            }
        }

        protected string FormatJerseyName(object nameObj)
        {
            if (nameObj == null || nameObj == DBNull.Value) return "";
            
            string name = nameObj.ToString().Trim();
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");

            int customIndex = name.IndexOf(" (customized:", StringComparison.OrdinalIgnoreCase);
            if (customIndex < 0)
            {
                customIndex = name.IndexOf(" (personalizado:", StringComparison.OrdinalIgnoreCase);
            }

            if (customIndex >= 0)
            {
                string baseName = name.Substring(0, customIndex).ToLower().Trim();
                string customPart = name.Substring(customIndex).Trim();
                
                System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
                string formattedBase = textInfo.ToTitleCase(baseName);
                
                int colonIndex = customPart.IndexOf(':');
                if (colonIndex >= 0 && customPart.EndsWith(")"))
                {
                    string customValue = customPart.Substring(colonIndex + 1, customPart.Length - colonIndex - 2).Trim();
                    string label = isSpanish ? "Personalizado" : "Customized";
                    return $"{formattedBase} ({label}: {customValue})";
                }
                else
                {
                    string label = isSpanish ? "Personalizado" : "Customized";
                    if (customPart.IndexOf("customized", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        customPart = System.Text.RegularExpressions.Regex.Replace(customPart, "customized", label, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    else if (customPart.IndexOf("personalizado", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        customPart = System.Text.RegularExpressions.Regex.Replace(customPart, "personalizado", label, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    return $"{formattedBase} {customPart}";
                }
            }
            else
            {
                System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
                return textInfo.ToTitleCase(name.ToLower());
            }
        }
    }
}
