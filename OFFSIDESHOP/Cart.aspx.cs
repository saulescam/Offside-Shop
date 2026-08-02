using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Cart : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (Session["UserRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

          

            if (!IsPostBack)
            {
                LoadCart();
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

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }

        private void LoadCart()
        {
            DataTable dtCart = Session["Cart"] as DataTable;

            if (dtCart != null && dtCart.Rows.Count > 0)
            {
                gvCart.DataSource = dtCart;
                gvCart.DataBind();

                // Calcular el Gran Total sumando la columna Subtotal
                decimal total = 0;
                foreach (DataRow row in dtCart.Rows)
                {
                    total += Convert.ToDecimal(row["Subtotal"]);
                }
                lblTotal.Text = string.Format("${0:F2}", total);
                btnCheckout.Enabled = true;
            }
            else
            {
                gvCart.DataSource = null;
                gvCart.DataBind();
                lblTotal.Text = "$0.00";
                btnCheckout.Enabled = false;
            }
        }

        protected void gvCart_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable dtCart = Session["Cart"] as DataTable;

            if (dtCart != null)
            {
                
                dtCart.Rows[e.RowIndex].Delete();
                Session["Cart"] = dtCart; 
                LoadCart(); 
            }
        }

        protected void gvCart_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "increase" || e.CommandName == "decrease")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);

                DataTable dtCart = Session["Cart"] as DataTable;

                if (dtCart != null && rowIndex < dtCart.Rows.Count)
                {
                    DataRow row = dtCart.Rows[rowIndex];

                    int currentQty = Convert.ToInt32(row["Quantity"]);

                    if (e.CommandName == "increase")
                    {
                       
                        int stockDisponible = Convert.ToInt32(row["Stock"]);

                        if (currentQty < stockDisponible)
                        {
                            currentQty += 1;
                        }
                    
                    }
                    else if (e.CommandName == "decrease")
                    {
                        if (currentQty > 1)
                        {
                            currentQty -= 1;
                        }
                    }

                    row["Quantity"] = currentQty;

                    decimal price = Convert.ToDecimal(row["Price"]);
                    row["Subtotal"] = price * currentQty;

                    Session["Cart"] = dtCart;

                    LoadCart();
                }
            }
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            // Validar si el usuario inició sesión antes de dejarlo pagar
            if (Session["UserRole"] == null)
            {
                Response.Redirect("Login.aspx?returnUrl=Checkout.aspx");
            }
            else
            {
                Response.Redirect("Checkout.aspx");
            }
        }
        
        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
        protected void btnMyOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyOrders.aspx");
        }
        protected void btnbackshop_Click(object sender, EventArgs e)
        {
            Response.Redirect("Homepage.aspx");
        }
        protected void gvCart_SelectedIndexChanged(object sender, EventArgs e)
        {

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