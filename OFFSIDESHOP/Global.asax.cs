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
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

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
            Session["InitOAuth"] = true;

            // Por defecto el idioma de la sesión será inglés
            Session["Language"] = "en";
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

            // Si el error original está envuelto en un TargetInvocationException, lo obtenemos
            if (ex is System.Reflection.TargetInvocationException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            // Verificar si fue causado por inyección de código / caracteres no válidos
            if (ex is HttpRequestValidationException)
            {
                // Limpiamos el error en el servidor
                Server.ClearError();
                Response.Clear();

                // Construimos la respuesta con SweetAlert o alert tradicional de JS
                string script = @"
            <!DOCTYPE html>
            <html>
            <head>
                <script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
            </head>
            <body style='background-color: #111;'>
                <script type='text/javascript'>
                    Swal.fire({
                        title: 'Invalid Input',
                        text: 'Invalid characters.',
                        icon: 'warning',
                        confirmButtonColor: '#FFC800'
                    }).then((result) => {
                        window.location.href = 'Homepage.aspx';
                    });
                </script>
            </body>
            </html>";

                Response.Write(script);
                Response.End(); // Finaliza la respuesta para enviar el HTML/JS al navegador
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