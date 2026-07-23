using System;
using System.Web.UI;

namespace OFFSIDESHOP
{
    public partial class SessionBridge : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string ticket = Request.QueryString["ticket"];
            var datos = LoginTicketStore.ConsumirTicket(ticket);

            if (datos == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            Session["UserRole"] = datos.IdRole;
            Session["Id_User"] = datos.UserId;
            Session["Perm_Products"] = datos.PermProducts;
            Session["Perm_Orders"] = datos.PermOrders;
            Session["Perm_Offers"] = datos.PermOffers;
            Session["Perm_Coupons"] = datos.PermCoupons;
            Session["Perm_Banners"] = datos.PermBanners;
            Session["Perm_Tickets"] = datos.PermTickets;

            if (datos.IdRole == 1 || datos.IdRole == 2)
            {
                Session["Admin"] = System.Web.HttpUtility.HtmlEncode(datos.UserName);
                Response.Redirect("Dashboard.aspx");
            }
            else
            {
                Session["Customer"] = System.Web.HttpUtility.HtmlEncode(datos.UserName);
                Response.Redirect("Homepage.aspx");
            }
        }
    }
}