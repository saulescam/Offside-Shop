using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Homepage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Remove("username");
            Response.Redirect("Inicio.aspx");
        }
    }
}