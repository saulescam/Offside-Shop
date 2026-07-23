using Nemiro.OAuth;
using Nemiro.OAuth.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace OFFSIDESHOP
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // Registra el cliente de Google con tus credenciales
            OAuthManager.RegisterClient(new GoogleClient(
                "1092341039059-9sav1qu2lnbfo77204gmt4l18h0n9hki.apps.googleusercontent.com",      // ← Pon aquí tu Client ID
                "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"   // ← Pon aquí tu Client Secret
            ));
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();

            // Verificar si el error fue causado por una inyección peligrosa (Request Validation)
            if (ex is HttpRequestValidationException)
            {
                // Limpiamos el error en el servidor para que no explote la pantalla amarilla
                Server.ClearError();

                // Opción A: Redirigir a una página de error amigable personalizada
                Response.Redirect("Homepage.aspx");

            }
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}