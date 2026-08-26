using System;
using System.Globalization;
using System.Threading;
using System.Web;

namespace OFFSIDESHOP
{
    public partial class Error500 : System.Web.UI.Page
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
            Response.StatusCode = 500;
            // ESTA ES LA LÍNEA QUE ROMPE EL BUCLE INFINITO
            Response.TrySkipIisCustomErrors = true;
        }
    }
}