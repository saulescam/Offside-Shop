using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class AddLeague : System.Web.UI.Page
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
                LoadLeagues();
        }

        // ──────────────────────────────────────────────────────────────
        //  Load all leagues into the GridView
        // ──────────────────────────────────────────────────────────────
        private void LoadLeagues()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT Id_League, Name_League FROM leagues ORDER BY Id_League ASC;", con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                gvLeagues.DataSource = dt;
                gvLeagues.DataBind();
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Save League button
        // ──────────────────────────────────────────────────────────────
        protected void btnSaveLeague_Click(object sender, EventArgs e)
        {
            string leagueName = txtLeagueName.Text.Trim();

            if (string.IsNullOrWhiteSpace(leagueName))
            {
                alerta.Text = "<script>Swal.fire('Error', 'Please enter a league name.', 'error');</script>";
                return;
            }

            // XSS protection
            string safeLeagueName = HttpUtility.HtmlEncode(leagueName);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO leagues (Name_League) VALUES (@Name);", con);
                    cmd.Parameters.AddWithValue("@Name", safeLeagueName);
                    cmd.ExecuteNonQuery();
                }

                alerta.Text = "<script>Swal.fire('Success', 'League saved successfully!', 'success');</script>";
                AuditLogger.LogActivity("CREATE", "AddLeague", $"Created new league '{safeLeagueName}'");
                txtLeagueName.Text = "";
                LoadLeagues();
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.HtmlEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Delete League row
        // ──────────────────────────────────────────────────────────────
        protected void gvLeagues_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // Safely retrieve the primary key from DataKeys
            int idLeague = Convert.ToInt32(gvLeagues.DataKeys[e.RowIndex].Value);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "DELETE FROM leagues WHERE Id_League = @IdLeague;", con);
                    cmd.Parameters.AddWithValue("@IdLeague", idLeague);
                    cmd.ExecuteNonQuery();
                }

                alerta.Text = "<script>Swal.fire('Deleted', 'League deleted successfully.', 'success');</script>";
                AuditLogger.LogActivity("DELETE", "AddLeague", $"Deleted league ID #{idLeague}");
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.HtmlEncode(ex.Message)}', 'error');</script>";
            }
            finally
            {
                LoadLeagues();
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
    }
}

