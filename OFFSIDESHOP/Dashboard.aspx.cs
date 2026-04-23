using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Primero verifica que no sea null
                if (Session["username"] != null)
                {
                    String nombre = Session["username"].ToString();
                    // Usuario logueado
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
            catch (Exception)
            {
                Response.Redirect("Login.aspx");
            }
        }
        protected void btningresar_Click(object sender, EventArgs e)
        {
            Response.Redirect("AgregarProducto.aspx");
        }
        protected void btneliminar_Click(object sender, EventArgs e)
        {
            Response.Redirect("EliminarProducto.aspx");
        }
        protected void btneditar_Click(object sender, EventArgs e)
        {
            Response.Redirect("EditarProducto.aspx");
        }
        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Remove("usermane");
            Response.Redirect("Login.aspx");
        }
    }
}