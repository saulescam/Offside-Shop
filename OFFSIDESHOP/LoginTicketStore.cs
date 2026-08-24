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

        public string PendingShirtId { get; set; }
        public string PendingSizeId { get; set; }
        public string PendingQuantity { get; set; }
        public bool PendingIsCustom { get; set; }
        public string PendingCustomName { get; set; }
        public string PendingCustomNumber { get; set; }
    }

    public static class PendingShirtHelper
    {
        public static string SerializePendingState(string shirtId, string sizeId, string qty, bool isCustom, string name, string number)
        {
            if (string.IsNullOrEmpty(shirtId)) return null;
            string encName = System.Web.HttpUtility.UrlEncode(name ?? "");
            string encNum = System.Web.HttpUtility.UrlEncode(number ?? "");
            return $"{shirtId}|{sizeId ?? ""}|{qty ?? ""}|{isCustom}|{encName}|{encNum}";
        }

        public static void DeserializeAndRestore(string stateString, out string shirtId, out string sizeId, out string qty, out bool isCustom, out string name, out string number)
        {
            shirtId = null;
            sizeId = null;
            qty = null;
            isCustom = false;
            name = null;
            number = null;

            if (string.IsNullOrEmpty(stateString)) return;

            string[] parts = stateString.Split('|');
            if (parts.Length >= 6)
            {
                shirtId = parts[0];
                sizeId = parts[1];
                qty = parts[2];
                bool.TryParse(parts[3], out isCustom);
                name = System.Web.HttpUtility.UrlDecode(parts[4]);
                number = System.Web.HttpUtility.UrlDecode(parts[5]);
            }
        }
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