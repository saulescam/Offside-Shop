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
            // Primero sacas el valor del Web.config
            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["GoogleClientSecret"];

            // Luego usas esa variable en tu cliente
            OAuthManager.RegisterClient(new GoogleClient(
                "1089623650574-8alvihot7uhtkmrqcpmf50r6ko1jppi6.apps.googleusercontent.com",
                clientSecret
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