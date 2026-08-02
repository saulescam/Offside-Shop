using System;
using System.Globalization;
using System.Threading;
using System.Web.UI;

public class BasePage : Page
{
    protected override void InitializeCulture()
    {
        // Por defecto será inglés, a menos que la sesión diga lo contrario
        string language = "en";

        if (Session["Language"] != null)
        {
            language = Session["Language"].ToString();
        }

        // Se aplica la cultura al hilo que está procesando la petición
        Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);

        base.InitializeCulture();
    }
}