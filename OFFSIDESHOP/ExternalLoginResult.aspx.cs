using MySql.Data.MySqlClient;
using Nemiro.OAuth;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class ExternalLoginResult : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Forzar a IIS a comunicarse usando TLS 1.2 (Requisito estricto de Google)
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            // 2. Registrar las claves aquí mismo para asegurar que IIS no las haya olvidado
            if (!Nemiro.OAuth.OAuthManager.IsRegisteredClient("google"))
            {
                Nemiro.OAuth.OAuthManager.RegisterClient(
                    new Nemiro.OAuth.Clients.GoogleClient(
                        System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"],
                        System.Configuration.ConfigurationManager.AppSettings["GoogleClientSecret"]
                    )
                );
            }

            // ── Verify the OAuth response from Google ─────────────────
            var result = Nemiro.OAuth.OAuthWeb.VerifyAuthorization();

            if (!result.IsSuccessfully)
            {
                string errorReal = result.ErrorInfo != null ? result.ErrorInfo.Message : "Error desconocido al contactar a Google";

                alerta.Text = $"<script>Swal.fire('Problem details', '{HttpUtility.JavaScriptStringEncode(errorReal)}', 'error')" +
                              ".then(() => { window.location.href = 'Login.aspx'; });</script>";
                return;
            }

            // ── Extract Google profile data ───────────────────────────
            var userInfo  = result.UserInfo;
            string googleId = userInfo.UserId.ToString();
            string email    = userInfo.Email;

            // Build display name with fallbacks
            string nombre = userInfo.DisplayName;
            if (string.IsNullOrWhiteSpace(nombre))
            {
                if (!string.IsNullOrWhiteSpace(userInfo.FirstName) || !string.IsNullOrWhiteSpace(userInfo.LastName))
                    nombre = (userInfo.FirstName + " " + userInfo.LastName).Trim();
                else if (!string.IsNullOrWhiteSpace(email))
                    nombre = email.Split('@')[0];
                else
                    nombre = "Google User";
            }

            // ── Clear any legacy session keys ─────────────────────────
            Session.Remove("username");
            Session.Remove("usermane");

            // ── Look up the user by Mail OR GoogleId ──────────────────
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string selectQuery = @"SELECT Id_User, Name_User, Id_Role,
                                                  IFNULL(Perm_Products, 0) AS Perm_Products,
                                                  IFNULL(Perm_Orders, 0) AS Perm_Orders,
                                                  IFNULL(Perm_Offers, 0) AS Perm_Offers,
                                                  IFNULL(Perm_Coupons, 0) AS Perm_Coupons,
                                                  IFNULL(Perm_Banners, 0) AS Perm_Banners,
                                                  IFNULL(Perm_Tickets, 0) AS Perm_Tickets
                                           FROM users
                                           WHERE Mail = @Email OR GoogleId = @GoogleId
                                           LIMIT 1;";

                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, con);
                    selectCmd.Parameters.AddWithValue("@Email",    email);
                    selectCmd.Parameters.AddWithValue("@GoogleId", googleId);

                    using (MySqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // ── EXISTING USER — read their real role ──────
                            string userName = reader["Name_User"].ToString();
                            int idRole      = Convert.ToInt32(reader["Id_Role"]);
                            int userId      = Convert.ToInt32(reader["Id_User"]);

                            bool pProd = Convert.ToBoolean(reader["Perm_Products"]);
                            bool pOrd  = Convert.ToBoolean(reader["Perm_Orders"]);
                            bool pOff  = Convert.ToBoolean(reader["Perm_Offers"]);
                            bool pCoup = Convert.ToBoolean(reader["Perm_Coupons"]);
                            bool pBan  = Convert.ToBoolean(reader["Perm_Banners"]);
                            bool pTick = Convert.ToBoolean(reader["Perm_Tickets"]);
                            reader.Close();

                            // Also update GoogleId if it was missing (e.g. registered via form before)
                            MySqlCommand patchCmd = new MySqlCommand(
                                "UPDATE users SET GoogleId = @GoogleId WHERE Mail = @Email AND (GoogleId IS NULL OR GoogleId = '');",
                                con);
                            patchCmd.Parameters.AddWithValue("@GoogleId", googleId);
                            patchCmd.Parameters.AddWithValue("@Email",    email);
                            patchCmd.ExecuteNonQuery();

                            // Apply session standard and redirect by role
                            ApplySessionAndRedirect(userName, idRole, userId, pProd, pOrd, pOff, pCoup, pBan, pTick);
                        }
                        else
                        {
                            // ── NEW USER — register with default Customer role ──
                            reader.Close();

                            // Build a unique username: part-before-@ + first 5 chars of googleId
                            string nameUser = email.Split('@')[0] + "_"
                                             + googleId.Substring(0, Math.Min(5, googleId.Length));

                            string insertQuery = @"INSERT INTO users
                                (Name_User, Mail, GoogleId, Id_Role)
                                VALUES (@NameUser, @Email, @GoogleId, 3);
                                SELECT LAST_INSERT_ID();";

                            MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                            insertCmd.Parameters.AddWithValue("@NameUser", HttpUtility.HtmlEncode(nameUser));
                            insertCmd.Parameters.AddWithValue("@Email",    email);
                            insertCmd.Parameters.AddWithValue("@GoogleId", googleId);
                            int userId = Convert.ToInt32(insertCmd.ExecuteScalar());

                            // New Google users always become Customers (Id_Role = 3)
                            ApplySessionAndRedirect(nameUser, 3, userId, false, false, false, false, false, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.HtmlEncode(ex.Message)}', 'error')" +
                              ".then(() => { window.location.href = 'Login.aspx'; });</script>";
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Set sessions using the standardized naming convention
        //  and redirect to the correct page for the user's role.
        // ──────────────────────────────────────────────────────────────
        private void ApplySessionAndRedirect(string userName, int idRole, int userId, bool pProd, bool pOrd, bool pOff, bool pCoup, bool pBan, bool pTick)
        {
            var datos = new LoginTicketData
            {
                UserId = userId,
                UserName = userName,
                IdRole = idRole,
                PermProducts = idRole == 1 ? true : pProd,
                PermOrders = idRole == 1 ? true : pOrd,
                PermOffers = idRole == 1 ? true : pOff,
                PermCoupons = idRole == 1 ? true : pCoup,
                PermBanners = idRole == 1 ? true : pBan,
                PermTickets = idRole == 1 ? true : pTick
            };

            string ticket = LoginTicketStore.CrearTicket(datos);

            string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "session_bridge_log.txt");
            string logMsg = $"{DateTime.Now}: ExternalLoginResult ApplySessionAndRedirect called. UserName: {userName}, IdRole: {idRole}, UserId: {userId}, Ticket: {ticket}";
            System.IO.File.AppendAllText(logPath, logMsg + Environment.NewLine);

            Response.Redirect(LoginTicketStore.LocalUrl("SessionBridge.aspx?ticket=" + ticket));
        }
    }
}