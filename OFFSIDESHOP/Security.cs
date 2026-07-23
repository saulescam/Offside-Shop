using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OFFSIDESHOP
{
    using BCrypt.Net;
    public class Security
    {
        public static string Encrypt(string password)
        {
            return BCrypt.HashPassword(password, workFactor: 12);
        }

        // Verificar contraseña
        public static bool Verificar(string passwordentered, string hashstored)
        {
            return BCrypt.Verify(passwordentered, hashstored);
        }

        // PBAC Helper: Evalúa permiso del usuario
        public static bool HasPermission(System.Web.SessionState.HttpSessionState session, string permissionKey)
        {
            if (session == null || session["UserRole"] == null) return false;
            int role = Convert.ToInt32(session["UserRole"]);
            if (role == 1) return true; // El Owner siempre tiene acceso a todo
            if (role != 2) return false; // Roles que no sean Owner o Admin no tienen permiso
            
            if (session[permissionKey] != null)
            {
                if (session[permissionKey] is bool b) return b;
                string val = session[permissionKey].ToString();
                return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        // PBAC Helper: Configura dinámicamente la visibilidad de los botones del Sidebar en las páginas de Admin
        public static void ConfigureAdminSidebar(System.Web.UI.Page page)
        {
            if (page == null || page.Session == null) return;

            var s = page.Session;
            bool permProducts = HasPermission(s, "Perm_Products");
            bool permOrders   = HasPermission(s, "Perm_Orders");
            bool permOffers   = HasPermission(s, "Perm_Offers");
            bool permCoupons  = HasPermission(s, "Perm_Coupons");
            bool permBanners  = HasPermission(s, "Perm_Banners");
            bool permTickets  = HasPermission(s, "Perm_Tickets");

            void SetBtnVisibility(string id, bool visible)
            {
                var c = page.FindControl(id) ?? FindControlRecursive(page, id);
                if (c != null) c.Visible = visible;
            }

            SetBtnVisibility("btnManageProducts", permProducts);
            SetBtnVisibility("btnAddLeague",      permProducts);
            SetBtnVisibility("btnAddTeam",        permProducts);
            SetBtnVisibility("btnAddBrand",       permProducts);

            SetBtnVisibility("btnManageOrders",   permOrders);
            // También manejamos enlaces <a> si tuvieran runat="server", o controles Button
            SetBtnVisibility("btnManageOffers",   permOffers);
            SetBtnVisibility("btnManageCoupons",  permCoupons);
            SetBtnVisibility("btnAdminBanners",   permBanners);
            SetBtnVisibility("btnManageTickets",  permTickets);

            // Menú exclusivo de Owner
            bool isOwner = s["UserRole"] != null && Convert.ToInt32(s["UserRole"]) == 1;
            SetBtnVisibility("phOwnerMenu", isOwner);
        }

        private static System.Web.UI.Control FindControlRecursive(System.Web.UI.Control root, string id)
        {
            if (root.ID == id) return root;
            foreach (System.Web.UI.Control c in root.Controls)
            {
                var found = FindControlRecursive(c, id);
                if (found != null) return found;
            }
            return null;
        }
    }
}