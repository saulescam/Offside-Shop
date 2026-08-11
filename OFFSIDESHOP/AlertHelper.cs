using System;
using System.Web;
using System.Web.UI;

namespace OFFSIDESHOP
{
    public static class AlertHelper
    {
        /// <summary>
        /// Obtiene el script de SweetAlert2 con títulos e información traducidos dinámicamente desde Strings.resx.
        /// </summary>
        public static string GetAlertScript(Page page, string titleKey, string messageKey, string iconType)
        {
            string title = HttpContext.GetGlobalResourceObject("Strings", titleKey)?.ToString() ?? titleKey;
            string message = HttpContext.GetGlobalResourceObject("Strings", messageKey)?.ToString() ?? messageKey;

            // Escapar comillas simples y saltos de línea para evitar romper la sintaxis de JS
            title = title.Replace("'", "\\'");
            message = message.Replace("'", "\\'").Replace("\r\n", " ").Replace("\n", " ");

            return $"<script>Swal.fire('{title}', '{message}', '{iconType}');</script>";
        }

        /// <summary>
        /// Genera el script seguro con setTimeout y fallback de alerta nativa, ideal para usar con ScriptManager.
        /// </summary>
        public static string GetSafeAlertScript(Page page, string titleKey, string messageKey, string iconType)
        {
            string title = HttpContext.GetGlobalResourceObject("Strings", titleKey)?.ToString() ?? titleKey;
            string message = HttpContext.GetGlobalResourceObject("Strings", messageKey)?.ToString() ?? messageKey;

            // Escapar comillas simples y saltos de línea para evitar romper la sintaxis de JS
            title = title.Replace("'", "\\'");
            message = message.Replace("'", "\\'").Replace("\r\n", " ").Replace("\n", " ");

            return $@"
                setTimeout(function() {{
                    if (typeof Swal !== 'undefined') {{
                        Swal.fire({{
                            title: '{title}',
                            text: '{message}',
                            icon: '{iconType}',
                            confirmButtonColor: '#FFC800'
                        }});
                    }} else {{
                        alert('{title}: {message}');
                    }}
                }}, 50);";
        }

        /// <summary>
        /// Genera el script de SweetAlert2 con redirección posterior al cierre o timer (envoltura de script incluida).
        /// </summary>
        public static string GetRedirectAlertScript(Page page, string titleKey, string messageKey, string iconType, int timerMs, string redirectUrl)
        {
            return $"<script>{GetRedirectAlertScriptNoTags(page, titleKey, messageKey, iconType, timerMs, redirectUrl)}</script>";
        }

        /// <summary>
        /// Genera el script de SweetAlert2 sin etiquetas script, útil para ScriptManager con redirección.
        /// </summary>
        public static string GetRedirectAlertScriptNoTags(Page page, string titleKey, string messageKey, string iconType, int timerMs, string redirectUrl)
        {
            string title = HttpContext.GetGlobalResourceObject("Strings", titleKey)?.ToString() ?? titleKey;
            string message = HttpContext.GetGlobalResourceObject("Strings", messageKey)?.ToString() ?? messageKey;

            title = title.Replace("'", "\\'");
            message = message.Replace("'", "\\'").Replace("\r\n", " ").Replace("\n", " ");

            return $@"
                Swal.fire({{
                    title: '{title}',
                    text: '{message}',
                    icon: '{iconType}',
                    showConfirmButton: false,
                    timer: {timerMs}
                }}).then(() => {{
                    window.location.href = '{redirectUrl}';
                }});";
        }

        public static string Success(Page page, string messageKey)
        {
            return GetAlertScript(page, "Alert_SuccessTitle", messageKey, "success");
        }

        public static string Error(Page page, string messageKey)
        {
            return GetAlertScript(page, "Alert_ErrorTitle", messageKey, "error");
        }

        public static string Warning(Page page, string messageKey)
        {
            return GetAlertScript(page, "Alert_WarningTitle", messageKey, "warning");
        }

        public static string Info(Page page, string messageKey)
        {
            return GetAlertScript(page, "Alert_InfoTitle", messageKey, "info");
        }

        public static string Deleted(Page page, string messageKey)
        {
            return GetAlertScript(page, "Alert_DeletedTitle", messageKey, "success");
        }

        public static string GetResourceString(Page page, string key)
        {
            return HttpContext.GetGlobalResourceObject("Strings", key)?.ToString() ?? key;
        }
    }
}
