using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class ManageCoupons : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Control de caché estricto
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (Session["UserRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            int role = Convert.ToInt32(Session["UserRole"]);
            if (role != 1 && role != 2) // Accesible por Admin y Owner
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // PBAC guard: require Perm_Coupons
            if (!Security.HasPermission(Session, "Perm_Coupons"))
            {
                Response.Redirect("Dashboard.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                LoadCoupons();
            }
        }

        private void LoadCoupons()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT Id_Coupon, Code, DiscountPercentage, MaxUses, UsedCount, IsActive FROM coupons ORDER BY CreatedAt DESC;";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    gvCoupons.DataSource = dt;
                    gvCoupons.DataBind();
                }
            }
            catch (Exception ex)
            {
                TriggerSweetAlert("Error", $"Error loading coupons: {ex.Message}", "error");
            }
        }

        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            ResetForm();
            pnlCouponForm.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            hfEditId.Value = "0";
            lblFormTitle.Text = "Create New Coupon";
            txtCouponCode.Text = "";
            txtDiscount.Text = "";
            txtMaxUses.Text = "10";
            ddlStatus.SelectedValue = "1";
            pnlCouponForm.Visible = false;
            txtCouponCode.Enabled = true; // Se puede editar al crear
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string code = txtCouponCode.Text.Trim().ToUpper();
            string discountStr = txtDiscount.Text.Trim();
            string maxUsesStr = txtMaxUses.Text.Trim();

            // Validaciones
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(discountStr) || string.IsNullOrEmpty(maxUsesStr))
            {
                TriggerSweetAlert("Fields Required", "Please fill in all required fields.", "warning");
                return;
            }

            if (!int.TryParse(discountStr, out int discount) || discount < 1 || discount > 100)
            {
                TriggerSweetAlert("Invalid Discount", "Discount percentage must be between 1 and 100.", "warning");
                return;
            }

            if (!int.TryParse(maxUsesStr, out int maxUses) || maxUses < 1)
            {
                TriggerSweetAlert("Invalid Limit", "Maximum uses must be at least 1.", "warning");
                return;
            }

            int isActive = Convert.ToInt32(ddlStatus.SelectedValue);
            int editId = Convert.ToInt32(hfEditId.Value);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Validar si el código ya existe (excluyendo el actual si estamos editando)
                    MySqlCommand cmdCheck = new MySqlCommand("SELECT COUNT(*) FROM coupons WHERE Code = @Code AND Id_Coupon != @Id", con);
                    cmdCheck.Parameters.AddWithValue("@Code", code);
                    cmdCheck.Parameters.AddWithValue("@Id", editId);
                    int exists = Convert.ToInt32(cmdCheck.ExecuteScalar());

                    if (exists > 0)
                    {
                        TriggerSweetAlert("Code Exists", "This coupon code already exists. Please choose a different one.", "error");
                        return;
                    }

                    if (editId == 0) // INSERT
                    {
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO coupons (Code, DiscountPercentage, MaxUses, IsActive) VALUES (@Code, @Discount, @MaxUses, @IsActive);", con);
                        cmd.Parameters.AddWithValue("@Code", code);
                        cmd.Parameters.AddWithValue("@Discount", discount);
                        cmd.Parameters.AddWithValue("@MaxUses", maxUses);
                        cmd.Parameters.AddWithValue("@IsActive", isActive);
                        cmd.ExecuteNonQuery();
                        TriggerSweetAlert("Created", "Coupon created successfully.", "success");
                    }
                    else // UPDATE
                    {
                        // Nota de Diseño: Al editar, normalmente se prohíbe cambiar el código si ya se ha usado, pero permitiremos actualizar usos y estado.
                        MySqlCommand cmd = new MySqlCommand("UPDATE coupons SET DiscountPercentage = @Discount, MaxUses = @MaxUses, IsActive = @IsActive WHERE Id_Coupon = @Id;", con);
                        cmd.Parameters.AddWithValue("@Discount", discount);
                        cmd.Parameters.AddWithValue("@MaxUses", maxUses);
                        cmd.Parameters.AddWithValue("@IsActive", isActive);
                        cmd.Parameters.AddWithValue("@Id", editId);
                        cmd.ExecuteNonQuery();
                        TriggerSweetAlert("Updated", "Coupon updated successfully.", "success");
                    }
                }

                ResetForm();
                LoadCoupons();
            }
            catch (Exception ex)
            {
                TriggerSweetAlert("Error", $"Database error: {ex.Message}", "error");
            }
        }

        protected void gvCoupons_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");

                int used = Convert.ToInt32(rowView["UsedCount"]);
                int max = Convert.ToInt32(rowView["MaxUses"]);
                bool isActive = Convert.ToInt32(rowView["IsActive"]) == 1;

                if (used >= max)
                {
                    lblStatus.Text = "<span class='status-badge status-depleted'><i class='fas fa-ban mr-1'></i> Depleted</span>";
                }
                else if (isActive)
                {
                    lblStatus.Text = "<span class='status-badge status-active'><i class='fas fa-check mr-1'></i> Active</span>";
                }
                else
                {
                    lblStatus.Text = "<span class='status-badge status-inactive'><i class='fas fa-times mr-1'></i> Inactive</span>";
                }
            }
        }

        protected void gvCoupons_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int couponId = Convert.ToInt32(e.CommandArgument);

            try
            {
                switch (e.CommandName)
                {
                    case "EditCoupon":
                        using (MySqlConnection con = new MySqlConnection(connectionString))
                        {
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("SELECT * FROM coupons WHERE Id_Coupon = @Id", con);
                            cmd.Parameters.AddWithValue("@Id", couponId);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    hfEditId.Value = reader["Id_Coupon"].ToString();
                                    txtCouponCode.Text = reader["Code"].ToString();
                                    txtCouponCode.Enabled = false; // Prevent code change on edit to maintain data integrity
                                    txtDiscount.Text = reader["DiscountPercentage"].ToString();
                                    txtMaxUses.Text = reader["MaxUses"].ToString();

                                    // SOLUCIÓN AL BUG: Normalizar el valor TinyInt(1) / Boolean de MySQL
                                    string isActiveRaw = reader["IsActive"].ToString();
                                    if (isActiveRaw == "True" || isActiveRaw == "1")
                                    {
                                        ddlStatus.SelectedValue = "1";
                                    }
                                    else
                                    {
                                        ddlStatus.SelectedValue = "0";
                                    }

                                    lblFormTitle.Text = "Edit Coupon";
                                    pnlCouponForm.Visible = true;
                                }
                            }
                        }
                        break;

                    case "ToggleCoupon":
                        using (MySqlConnection con = new MySqlConnection(connectionString))
                        {
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("UPDATE coupons SET IsActive = 1 - IsActive WHERE Id_Coupon = @Id", con);
                            cmd.Parameters.AddWithValue("@Id", couponId);
                            cmd.ExecuteNonQuery();
                        }
                        LoadCoupons();
                        break;

                    case "DeleteCoupon":
                        using (MySqlConnection con = new MySqlConnection(connectionString))
                        {
                            con.Open();
                            // El borrado en cascada está configurado a SET NULL en la base de datos para no romper órdenes históricas
                            MySqlCommand cmd = new MySqlCommand("DELETE FROM coupons WHERE Id_Coupon = @Id", con);
                            cmd.Parameters.AddWithValue("@Id", couponId);
                            cmd.ExecuteNonQuery();
                            TriggerSweetAlert("Deleted", "Coupon permanently removed.", "success");
                        }
                        LoadCoupons();
                        break;
                }
            }
            catch (Exception ex)
            {
                TriggerSweetAlert("Error", $"Operation failed: {ex.Message}", "error");
            }
        }

        // Helper para la barra de progreso en la grilla
        protected int GetPercentage(int used, int max)
        {
            if (max == 0) return 100;
            double p = ((double)used / max) * 100;
            return p > 100 ? 100 : (int)p;
        }

        private void TriggerSweetAlert(string title, string text, string icon)
        {
            string cleanTitle = title.Replace("'", "\\'");
            string cleanText = text.Replace("'", "\\'").Replace("\r\n", " ").Replace("\n", " ");

            string script = $@"Swal.fire({{
                title: '{cleanTitle}',
                text: '{cleanText}',
                icon: '{icon}',
                confirmButtonColor: '#FFC800'
            }});";

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
        }

        // Redirecciones del menú lateral
        protected void btnManageProducts_Click(object sender, EventArgs e) { Response.Redirect("ManageProducts.aspx"); }
        protected void btnManageOrders_Click(object sender, EventArgs e) { Response.Redirect("ManageOrders.aspx"); }
        protected void btnManageOffers_Click(object sender, EventArgs e) { Response.Redirect("ManageOffers.aspx"); }
        protected void btnManageCoupons_Click(object sender, EventArgs e) { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAddLeague_Click(object sender, EventArgs e) { Response.Redirect("AddLeague.aspx"); }
        protected void btnAddTeam_Click(object sender, EventArgs e) { Response.Redirect("AddTeam.aspx"); }
        protected void btnAddBrand_Click(object sender, EventArgs e) { Response.Redirect("AddBrand.aspx"); }
        protected void btnManageUsers_Click(object sender, EventArgs e) { Response.Redirect("ManageUsers.aspx"); }
        protected void btnSmtpSettings_Click(object sender, EventArgs e) { Response.Redirect("SmtpSettings.aspx"); }
        protected void btnAdminBanners_Click(object sender, EventArgs e) { Response.Redirect("AdminBanners.aspx"); }
        protected void btnStats_Click(object sender, EventArgs e) { Response.Redirect("AdminStats.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e) { Response.Redirect("AdminAudit.aspx"); }
        protected void btncerrar_Click(object sender, EventArgs e) { Session.Clear(); Session.Abandon(); Response.Redirect("Login.aspx"); }
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