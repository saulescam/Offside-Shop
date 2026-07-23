using System;
using System.Collections.Concurrent;

namespace OFFSIDESHOP
{
    public class LoginTicketData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int IdRole { get; set; }
        public bool PermProducts { get; set; }
        public bool PermOrders { get; set; }
        public bool PermOffers { get; set; }
        public bool PermCoupons { get; set; }
        public bool PermBanners { get; set; }
        public bool PermTickets { get; set; }
        public DateTime Expira { get; set; }
    }

    public static class LoginTicketStore
    {
        private static readonly ConcurrentDictionary<string, LoginTicketData> _tickets
            = new ConcurrentDictionary<string, LoginTicketData>();

        public static string CrearTicket(LoginTicketData datos)
        {
            string ticket = Guid.NewGuid().ToString("N");
            datos.Expira = DateTime.UtcNow.AddSeconds(60);
            _tickets[ticket] = datos;
            return ticket;
        }

        public static LoginTicketData ConsumirTicket(string ticket)
        {
            if (string.IsNullOrEmpty(ticket)) return null;

            if (_tickets.TryRemove(ticket, out var datos))
            {
                if (datos.Expira >= DateTime.UtcNow)
                    return datos;
            }
            return null;
        }

        public static string LocalUrl(string rutaRelativa)
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["LocalAppBaseUrl"];
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            return baseUrl + rutaRelativa;
        }
    }
}