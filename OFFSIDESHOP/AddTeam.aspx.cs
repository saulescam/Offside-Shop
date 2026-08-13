using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class AddTeam : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;
        // ──────────────────────────────────────────────────────────────
        //  Page_Load
        // ──────────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            // Cache control
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            // Role guard: Owner (1) or Admin (2) only
            if (Session["UserRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }
            int role = Convert.ToInt32(Session["UserRole"]);
            if (role != 1 && role != 2)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // PBAC guard: require Perm_Products
            if (!Security.HasPermission(Session, "Perm_Products"))
            {
                Response.Redirect("Dashboard.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                LoadLeaguesDropdown();
                LoadTeams();
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Populate the Leagues DropDownList
        // ──────────────────────────────────────────────────────────────
        private void LoadLeaguesDropdown()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                string selectLeagueText = isSpanish ? "-- Seleccionar Liga --" : "-- Select League --";
                ddlLeagues.Items.Clear();
                ddlLeagues.Items.Add(new ListItem(selectLeagueText, ""));
                foreach (DataRow row in dt.Rows)
                    ddlLeagues.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Load all teams with league name into GridView
        // ──────────────────────────────────────────────────────────────
        private void LoadTeams()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT t.Id_Team, t.Name_Team, l.Name_League
                      FROM teams t
                      INNER JOIN leagues l ON t.Id_League = l.Id_League
                      ORDER BY l.Name_League ASC, t.Name_Team ASC;", con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                gvTeams.DataSource = dt;
                gvTeams.DataBind();
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Save Team button
        // ──────────────────────────────────────────────────────────────
        protected void btnSaveTeam_Click(object sender, EventArgs e)
        {
            string teamName = txtTeamName.Text.Trim();

            if (string.IsNullOrWhiteSpace(teamName))
            {
                alerta.Text = AlertHelper.Error(this, "Alert_Team_Empty");
                return;
            }

            if (string.IsNullOrEmpty(ddlLeagues.SelectedValue))
            {
                alerta.Text = AlertHelper.Error(this, "Alert_Team_SelectLeague");
                return;
            }

            if (!int.TryParse(ddlLeagues.SelectedValue, out int leagueId))
            {
                alerta.Text = AlertHelper.Error(this, "Alert_Team_InvalidLeague");
                return;
            }

            // XSS protection
            string safeTeamName = HttpUtility.HtmlEncode(teamName);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO teams (Name_Team, Id_League) VALUES (@Name, @IdLeague);", con);
                    cmd.Parameters.AddWithValue("@Name", safeTeamName);
                    cmd.Parameters.AddWithValue("@IdLeague", leagueId);
                    cmd.ExecuteNonQuery();
                }

                alerta.Text = AlertHelper.Success(this, "Alert_Team_Saved");
                AuditLogger.LogActivity("CREATE", "AddTeam", $"Created new team '{safeTeamName}' in League ID #{leagueId}");
                txtTeamName.Text = "";
                LoadTeams();
            }
            catch (Exception ex)
            {
                alerta.Text = AlertHelper.GetAlertScript(this, "Alert_ErrorTitle", HttpUtility.HtmlEncode(ex.Message), "error");
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Delete Team row
        // ──────────────────────────────────────────────────────────────
        protected void gvTeams_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // Safely retrieve the primary key from DataKeys
            int idTeam = Convert.ToInt32(gvTeams.DataKeys[e.RowIndex].Value);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "DELETE FROM teams WHERE Id_Team = @IdTeam;", con);
                    cmd.Parameters.AddWithValue("@IdTeam", idTeam);
                    cmd.ExecuteNonQuery();
                }

                alerta.Text = AlertHelper.GetAlertScript(this, "Alert_DeletedTitle", "Alert_Team_Deleted", "success");
                AuditLogger.LogActivity("DELETE", "AddTeam", $"Deleted team ID #{idTeam}");
            }
            catch (Exception ex)
            {
                alerta.Text = AlertHelper.GetAlertScript(this, "Alert_ErrorTitle", HttpUtility.HtmlEncode(ex.Message), "error");
            }
            finally
            {
                LoadTeams();
            }
        }
        protected void btnManageProducts_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageProducts.aspx");
        }

        protected void btnManageOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageOrders.aspx");
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
        protected void btnManageOffers_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageOffers.aspx");
        }
        protected void btnAddLeague_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddLeague.aspx");
        }

        protected void btnAddTeam_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddTeam.aspx");
        }

        protected void btnAddBrand_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddBrand.aspx");
        }

        protected void btnManageUsers_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageUsers.aspx");
        }

        protected void btnAdminBanners_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminBanners.aspx");
        }
        protected void btnSmtpSettings_Click(object sender, EventArgs e)
        {
            Response.Redirect("SmtpSettings.aspx");
        }
        protected void btnStats_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminStats.aspx");
        }
        protected void btnManageCoupons_Click(object sender, EventArgs e)
        { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e)
        { Response.Redirect("AdminAudit.aspx"); }
        protected override void InitializeCulture()
        {
            string lang = Session["Language"] != null ? Session["Language"].ToString() : "en";
            string cultureName = (lang == "es") ? "es-SV" : "en-US";
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(cultureName);
            ci.NumberFormat.CurrencySymbol = "$";
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            base.InitializeCulture();
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }
    }
}

