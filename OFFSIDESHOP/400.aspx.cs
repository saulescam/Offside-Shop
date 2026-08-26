using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class _400 : System.Web.UI.Page
    {
        protected override void InitializeCulture()
        {
            try
            {
                string lang = "en"; // Idioma por defecto

                // Forma súper segura de acceder a la sesión en páginas de error
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                {
                    if (HttpContext.Current.Session["Language"] != null)
                    {
                        lang = HttpContext.Current.Session["Language"].ToString();
                    }
                }

                string cultureName = (lang == "es") ? "es-SV" : "en-US";

                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(cultureName);
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch
            {
                // Si la sesión o el contexto no están disponibles (típico en errores críticos),
                // ignoramos el error para no causar un bucle 500.
                // La página se cargará en inglés por defecto.
            }

            base.InitializeCulture();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Devuelve el código oficial 404 a los navegadores y motores de búsqueda
            Response.StatusCode = 400; // (O el número que corresponda)
            Response.TrySkipIisCustomErrors = true; // <- Esta es la línea que IIS ahora sí va a respetar
        }
    }
}