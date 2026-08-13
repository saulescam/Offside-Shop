using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class AdminAudit : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            // Strict Security Check: Only Owner (Role 1) can access Audit Log
            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 1)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                PopulateFilters();
                LoadLogs();
            }
        }

        private void PopulateFilters()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Load Action Types
                    string queryActions = "SELECT DISTINCT Action_Type FROM activity_logs WHERE Action_Type IS NOT NULL AND Action_Type != '' ORDER BY Action_Type;";
                    using (MySqlCommand cmd = new MySqlCommand(queryActions, con))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlFilterAction.Items.Clear();
                            ddlFilterAction.Items.Add(new ListItem("-- All Action Types --", ""));
                            while (reader.Read())
                            {
                                string val = reader["Action_Type"].ToString();
                                ddlFilterAction.Items.Add(new ListItem(val, val));
                            }
                        }
                    }

                    // Load Modules
                    string queryModules = "SELECT DISTINCT Module FROM activity_logs WHERE Module IS NOT NULL AND Module != '' ORDER BY Module;";
                    using (MySqlCommand cmd = new MySqlCommand(queryModules, con))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlFilterModule.Items.Clear();
                            ddlFilterModule.Items.Add(new ListItem("-- All Modules --", ""));
                            while (reader.Read())
                            {
                                string val = reader["Module"].ToString();
                                ddlFilterModule.Items.Add(new ListItem(val, val));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading filters: " + ex.Message);
            }
        }

        private void LoadLogs()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"SELECT a.Id_Log, a.Id_User, a.Action_Type, a.Module, a.Description, a.IP_Address, a.Created_At,
                                            CONCAT(u.Name, ' ', u.Lastname) AS AdminName
                                     FROM activity_logs a
                                     INNER JOIN users u ON a.Id_User = u.Id_User
                                     WHERE 1=1 ";

                    List<MySqlParameter> parameters = new List<MySqlParameter>();

                    if (ddlFilterAction != null && !string.IsNullOrEmpty(ddlFilterAction.SelectedValue))
                    {
                        query += " AND a.Action_Type = @ActionType ";
                        parameters.Add(new MySqlParameter("@ActionType", ddlFilterAction.SelectedValue));
                    }

                    if (ddlFilterModule != null && !string.IsNullOrEmpty(ddlFilterModule.SelectedValue))
                    {
                        query += " AND a.Module = @Module ";
                        parameters.Add(new MySqlParameter("@Module", ddlFilterModule.SelectedValue));
                    }

                    if (txtSearch != null && !string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        query += " AND (a.Description LIKE @Search OR CONCAT(u.Name, ' ', u.Lastname) LIKE @Search OR a.IP_Address LIKE @Search) ";
                        parameters.Add(new MySqlParameter("@Search", "%" + txtSearch.Text.Trim() + "%"));
                    }

                    query += " ORDER BY a.Created_At DESC;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                        DataTable dt = new DataTable();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        gvAuditLogs.DataSource = dt;
                        gvAuditLogs.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading audit logs: " + ex.Message);
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvAuditLogs.PageIndex = 0;
            LoadLogs();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            if (ddlFilterAction.Items.Count > 0) ddlFilterAction.SelectedIndex = 0;
            if (ddlFilterModule.Items.Count > 0) ddlFilterModule.SelectedIndex = 0;
            txtSearch.Text = string.Empty;
            gvAuditLogs.PageIndex = 0;
            LoadLogs();
        }

        protected void gvAuditLogs_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAuditLogs.PageIndex = e.NewPageIndex;
            LoadLogs();
        }

        protected void btnManageProducts_Click(object sender, EventArgs e) { Response.Redirect("ManageProducts.aspx"); }
        protected void btnManageOffers_Click(object sender, EventArgs e) { Response.Redirect("ManageOffers.aspx"); }
        protected void btnManageCoupons_Click(object sender, EventArgs e) { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAddLeague_Click(object sender, EventArgs e) { Response.Redirect("AddLeague.aspx"); }
        protected void btnAddTeam_Click(object sender, EventArgs e) { Response.Redirect("AddTeam.aspx"); }
        protected void btnAddBrand_Click(object sender, EventArgs e) { Response.Redirect("AddBrand.aspx"); }
        protected void btnManageUsers_Click(object sender, EventArgs e) { Response.Redirect("ManageUsers.aspx"); }
        protected void btnSmtpSettings_Click(object sender, EventArgs e) { Response.Redirect("SmtpSettings.aspx"); }
        protected void btnStats_Click(object sender, EventArgs e) { Response.Redirect("AdminStats.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e) { Response.Redirect("AdminAudit.aspx"); }
        protected void btnAdminBanners_Click(object sender, EventArgs e) { Response.Redirect("AdminBanners.aspx"); }
        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
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
