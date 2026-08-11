using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class AddBrand : System.Web.UI.Page
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
                LoadBrands();
        }

        // ──────────────────────────────────────────────────────────────
        //  Load all brands into the GridView
        // ──────────────────────────────────────────────────────────────
        private void LoadBrands()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT Id_Brand, Name_Brand FROM brands ORDER BY Id_Brand ASC;", con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                gvBrands.DataSource = dt;
                gvBrands.DataBind();
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Save Brand button
        // ──────────────────────────────────────────────────────────────
        protected void btnSaveBrand_Click(object sender, EventArgs e)
        {
            string brandName = txtBrandName.Text.Trim();

            if (string.IsNullOrWhiteSpace(brandName))
            {
                alerta.Text = AlertHelper.Error(this, "Alert_Brand_Empty");
                return;
            }

            // XSS protection
            string safeBrandName = HttpUtility.HtmlEncode(brandName);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO brands (Name_Brand) VALUES (@Name);", con);
                    cmd.Parameters.AddWithValue("@Name", safeBrandName);
                    cmd.ExecuteNonQuery();
                }

                alerta.Text = AlertHelper.Success(this, "Alert_Brand_Saved");
                AuditLogger.LogActivity("CREATE", "AddBrand", $"Created new brand '{safeBrandName}'");
                txtBrandName.Text = "";
                LoadBrands();
            }
            catch (Exception ex)
            {
                alerta.Text = AlertHelper.GetAlertScript(this, "Alert_ErrorTitle", HttpUtility.HtmlEncode(ex.Message), "error");
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Delete Brand row
        // ──────────────────────────────────────────────────────────────
        protected void gvBrands_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // Safely retrieve the primary key from DataKeys
            int idBrand = Convert.ToInt32(gvBrands.DataKeys[e.RowIndex].Value);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "DELETE FROM brands WHERE Id_Brand = @IdBrand;", con);
                    cmd.Parameters.AddWithValue("@IdBrand", idBrand);
                    cmd.ExecuteNonQuery();
                }

                alerta.Text = AlertHelper.Success(this, "Alert_Brand_Deleted");
                AuditLogger.LogActivity("DELETE", "AddBrand", $"Deleted brand ID #{idBrand}");
            }
            catch (Exception ex)
            {
                alerta.Text = AlertHelper.GetAlertScript(this, "Alert_ErrorTitle", HttpUtility.HtmlEncode(ex.Message), "error");
            }
            finally
            {
                LoadBrands();
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
        protected void btnManageOffers_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageOffers.aspx");
        }
        protected void btnStats_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminStats.aspx");
        }
        protected void btnManageCoupons_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageCoupons.aspx");
        }
        protected void btnAuditLogs_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminAudit.aspx");
        }
        protected override void InitializeCulture()
        {
            if (Session["Language"] != null)
            {
                string lang = Session["Language"].ToString();
                string cultureName = (lang == "es") ? "es-SV" : "en-US";
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(cultureName);
                ci.NumberFormat.CurrencySymbol = "$";
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            }
            base.InitializeCulture();
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }
    }
}

