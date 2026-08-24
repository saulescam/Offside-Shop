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

            string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "session_bridge_log.txt");
            string logMsg = $"{DateTime.Now}: SessionBridge loaded. Ticket: {ticket}, Datos is null: {datos == null}";
            if (datos != null)
            {
                logMsg += $", IdRole: {datos.IdRole}, UserName: {datos.UserName}, UserId: {datos.UserId}";
            }
            System.IO.File.AppendAllText(logPath, logMsg + Environment.NewLine);

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

            // Restablecer datos del producto pendiente en la sesión local si venían en el ticket
            if (!string.IsNullOrEmpty(datos.PendingShirtId))
            {
                Session["PendingShirtId"] = datos.PendingShirtId;
                Session["PendingSizeId"] = datos.PendingSizeId;
                Session["PendingQuantity"] = datos.PendingQuantity;
                Session["PendingIsCustom"] = datos.PendingIsCustom;
                Session["PendingCustomName"] = datos.PendingCustomName;
                Session["PendingCustomNumber"] = datos.PendingCustomNumber;
            }

            if (datos.IdRole == 1 || datos.IdRole == 2)
            {
                Session["Admin"] = System.Web.HttpUtility.HtmlEncode(datos.UserName);

                // Si por alguna razón un admin intentaba comprar, lo devolvemos a la camiseta
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
            else if (datos.IdRole == 4)
            {
                Session["Delivery"] = System.Web.HttpUtility.HtmlEncode(datos.UserName);
                Response.Redirect("DeliveryDashboard.aspx");
            }
            else
            {
                // Entra aquí si es Cliente (Rol 3)
                Session["Customer"] = System.Web.HttpUtility.HtmlEncode(datos.UserName);

                // MAGIA AQUÍ: Verificamos si venía de intentar agregar algo al carrito
                if (Session["PendingShirtId"] != null)
                {
                    string redirectId = Session["PendingShirtId"].ToString();
                    Session.Remove("PendingShirtId");

                    // Lo enviamos directo a los detalles de la camiseta
                    Response.Redirect($"DetailsShirt.aspx?id={redirectId}");
                }
                else
                {
                    // Si entró haciendo click en Login normal, lo mandamos al inicio
                    Response.Redirect("Homepage.aspx");
                }
            }
        }
    }
}