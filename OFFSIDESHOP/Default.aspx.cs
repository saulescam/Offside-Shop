using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
            else
            {
                Response.Redirect("Homepage.aspx");
            }
        }
    }
}