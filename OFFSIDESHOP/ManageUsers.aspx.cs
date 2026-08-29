using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class ManageUsers : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 1)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                LocalizeDropdowns();
                LoadUsers();
            }
        }

        private void LocalizeDropdowns()
        {
            string selFilterRole = ddlFilterRole.SelectedValue;
            ddlFilterRole.Items.Clear();
            ddlFilterRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Admin_Users_AllRoles"), "0"));
            ddlFilterRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Owner"), "1"));
            ddlFilterRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Admin"), "2"));
            ddlFilterRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Customer"), "3"));
            ddlFilterRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Delivery"), "4"));
            if (!string.IsNullOrEmpty(selFilterRole) && ddlFilterRole.Items.FindByValue(selFilterRole) != null)
            {
                ddlFilterRole.SelectedValue = selFilterRole;
            }

            string selDeliveryStatus = ddlFilterDeliveryStatus.SelectedValue;
            ddlFilterDeliveryStatus.Items.Clear();
            ddlFilterDeliveryStatus.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Admin_Users_AllDeliveryStatuses"), "ALL"));
            ddlFilterDeliveryStatus.Items.Add(new ListItem("🟢 " + AlertHelper.GetResourceString(this, "Admin_Users_StatusAvailable"), "AVAILABLE"));
            ddlFilterDeliveryStatus.Items.Add(new ListItem("🔵 " + AlertHelper.GetResourceString(this, "Admin_Users_StatusDelivering"), "DELIVERING"));
            ddlFilterDeliveryStatus.Items.Add(new ListItem("⚪ " + AlertHelper.GetResourceString(this, "Admin_Users_StatusOffDuty"), "OFFDUTY"));
            if (!string.IsNullOrEmpty(selDeliveryStatus) && ddlFilterDeliveryStatus.Items.FindByValue(selDeliveryStatus) != null)
            {
                ddlFilterDeliveryStatus.SelectedValue = selDeliveryStatus;
            }

            string selNewRole = ddlNewRole.SelectedValue;
            ddlNewRole.Items.Clear();
            ddlNewRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Customer"), "3"));
            ddlNewRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Admin"), "2"));
            ddlNewRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Delivery"), "4"));
            if (!string.IsNullOrEmpty(selNewRole) && ddlNewRole.Items.FindByValue(selNewRole) != null)
            {
                ddlNewRole.SelectedValue = selNewRole;
            }

            string selEditRole = ddlEditRole.SelectedValue;
            ddlEditRole.Items.Clear();
            ddlEditRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Customer"), "3"));
            ddlEditRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Admin"), "2"));
            ddlEditRole.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Role_Delivery"), "4"));
            if (!string.IsNullOrEmpty(selEditRole) && ddlEditRole.Items.FindByValue(selEditRole) != null)
            {
                ddlEditRole.SelectedValue = selEditRole;
            }
        }

        protected string GetLocalizedRoleName(object idRoleObj, object roleNameObj)
        {
            int idRole = 0;
            if (idRoleObj != null && idRoleObj != DBNull.Value)
            {
                int.TryParse(idRoleObj.ToString(), out idRole);
            }

            switch (idRole)
            {
                case 1: return AlertHelper.GetResourceString(this, "Role_Owner");
                case 2: return AlertHelper.GetResourceString(this, "Role_Admin");
                case 3: return AlertHelper.GetResourceString(this, "Role_Customer");
                case 4: return AlertHelper.GetResourceString(this, "Role_Delivery");
                default: return roleNameObj != null ? roleNameObj.ToString() : "";
            }
        }

        private void LoadUsers()
        {
            try
            {
                int filterRole = Convert.ToInt32(ddlFilterRole.SelectedValue);
                string filterDeliveryStatus = ddlFilterDeliveryStatus.SelectedValue;
                string searchUser = txtSearchUser.Text.Trim();

                string query = @"
                    SELECT 
                        u.Id_User, 
                        u.Name_User, 
                        u.Mail, 
                        u.Id_Role, 
                        r.Name_Role,
                        dt.Id_ActiveOrder,
                        dt.LastUpdate,
                        CASE 
                            WHEN u.Id_Role != 4 THEN 'N/A'
                            WHEN dt.Id_ActiveOrder IS NOT NULL THEN 'DELIVERING'
                            WHEN dt.LastUpdate >= DATE_SUB(NOW(), INTERVAL 30 MINUTE) THEN 'AVAILABLE'
                            ELSE 'OFFDUTY'
                        END AS DeliveryStatus
                    FROM users u 
                    INNER JOIN roles r ON u.Id_Role = r.Id_Role 
                    LEFT JOIN driver_tracking dt ON u.Id_User = dt.Id_Driver
                    WHERE 1=1";

                if (filterRole > 0)
                {
                    query += " AND u.Id_Role = @FilterRole";
                }

                if (!string.IsNullOrEmpty(searchUser))
                {
                    query += " AND (u.Name_User LIKE @SearchUser OR u.Mail LIKE @SearchUser)";
                }

                if (filterDeliveryStatus != "ALL")
                {
                    query += " HAVING DeliveryStatus = @DeliveryStatus";
                }

                query += " ORDER BY u.Id_Role ASC, u.Id_User ASC;";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(query, con);

                    if (filterRole > 0) cmd.Parameters.AddWithValue("@FilterRole", filterRole);
                    if (!string.IsNullOrEmpty(searchUser)) cmd.Parameters.AddWithValue("@SearchUser", "%" + searchUser + "%");
                    if (filterDeliveryStatus != "ALL") cmd.Parameters.AddWithValue("@DeliveryStatus", filterDeliveryStatus);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    gvUsers.DataSource = dt;
                    gvUsers.DataBind();
                }
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvUsers.PageIndex = 0;
            LoadUsers();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ddlFilterRole.SelectedIndex = 0;
            ddlFilterDeliveryStatus.SelectedIndex = 0;
            txtSearchUser.Text = "";
            gvUsers.PageIndex = 0;
            LoadUsers();
        }

        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            LoadUsers();
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView drv = (DataRowView)e.Row.DataItem;
            int currentRoleId = Convert.ToInt32(drv["Id_Role"]);
            string deliveryStatus = drv["DeliveryStatus"] != DBNull.Value ? drv["DeliveryStatus"].ToString() : "N/A";
            object activeOrderObj = drv["Id_ActiveOrder"];

            LinkButton btnEdit = (LinkButton)e.Row.FindControl("btnEdit");
            LinkButton btnPermissions = (LinkButton)e.Row.FindControl("btnPermissions");
            LinkButton btnDelete = (LinkButton)e.Row.FindControl("btnDelete");
            Label lblOwnerProtect = (Label)e.Row.FindControl("lblOwnerProtect");
            Label lblDeliveryStatusBadge = (Label)e.Row.FindControl("lblDeliveryStatusBadge");

            // Configurar Badge del Repartidor (Delivery)
            if (lblDeliveryStatusBadge != null)
            {
                if (currentRoleId == 4) // Si es Repartidor
                {
                    switch (deliveryStatus)
                    {
                        case "DELIVERING":
                            string orderId = activeOrderObj != DBNull.Value ? activeOrderObj.ToString() : "";
                            string textOnTheWay = string.Format(AlertHelper.GetResourceString(this, "Driver_Badge_OnTheWay"), orderId);
                            lblDeliveryStatusBadge.Text = $"<span class='badge-driver-delivering'><i class='fas fa-motorcycle mr-1'></i>{textOnTheWay}</span>";
                            break;
                        case "AVAILABLE":
                            string textOnDuty = AlertHelper.GetResourceString(this, "Driver_Badge_OnDuty");
                            lblDeliveryStatusBadge.Text = $"<span class='badge-driver-onduty'><i class='fas fa-check-circle mr-1'></i>{textOnDuty}</span>";
                            break;
                        default:
                            string textOffDuty = AlertHelper.GetResourceString(this, "Driver_Badge_OffDuty");
                            lblDeliveryStatusBadge.Text = $"<span class='badge-driver-offduty'><i class='fas fa-moon mr-1'></i>{textOffDuty}</span>";
                            break;
                    }
                }
                else
                {
                    lblDeliveryStatusBadge.Text = "<span class='text-muted small'>N/A</span>";
                }
            }

            // Proteger al Propietario (Owner)
            if (currentRoleId == 1)
            {
                if (btnEdit != null) btnEdit.Visible = false;
                if (btnPermissions != null) btnPermissions.Visible = false;
                if (btnDelete != null) btnDelete.Visible = false;
                if (lblOwnerProtect != null)
                {
                    lblOwnerProtect.Text = $"<span class='text-muted'><i class='fas fa-crown'></i> {AlertHelper.GetResourceString(this, "Role_Owner")}</span>";
                    lblOwnerProtect.Visible = true;
                }
            }
            else
            {
                if (btnPermissions != null)
                {
                    btnPermissions.Visible = (currentRoleId == 2);
                }
            }
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 1) return;

            int userId = Convert.ToInt32(e.CommandArgument);

            if (EsOwner(userId))
            {
                TriggerAlert("Alert_Users_OwnerProtectTitle", "Alert_Users_OwnerProtectText", "warning");
                return;
            }

            try
            {
                if (e.CommandName == "DeleteUser")
                {
                    using (MySqlConnection con = new MySqlConnection(connectionString))
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("DELETE FROM users WHERE Id_User = @UserId;", con);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    TriggerAlert("Alert_DeletedTitle", "Alert_Users_DeletedText", "success");
                    LoadUsers();
                }
                else if (e.CommandName == "EditUser")
                {
                    using (MySqlConnection con = new MySqlConnection(connectionString))
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("SELECT Name_User, Mail, Id_Role FROM users WHERE Id_User = @UserId;", con);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfEditUserId.Value = userId.ToString();
                                txtEditUsername.Text = reader["Name_User"].ToString();
                                txtEditEmail.Text = reader["Mail"].ToString();
                                ddlEditRole.SelectedValue = reader["Id_Role"].ToString();
                                txtEditPass.Text = "";

                                phEditUserModal.Visible = true;
                            }
                        }
                    }
                }
                else if (e.CommandName == "ManagePermissions")
                {
                    using (MySqlConnection con = new MySqlConnection(connectionString))
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("SELECT Perm_Products, Perm_Orders, Perm_Offers, Perm_Coupons, Perm_Banners, IFNULL(Perm_Tickets, 0) AS Perm_Tickets FROM users WHERE Id_User = @UserId;", con);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfPermUserId.Value = userId.ToString();
                                chkModalPermProducts.Checked = Convert.ToBoolean(reader["Perm_Products"]);
                                chkModalPermOrders.Checked = Convert.ToBoolean(reader["Perm_Orders"]);
                                chkModalPermOffers.Checked = Convert.ToBoolean(reader["Perm_Offers"]);
                                chkModalPermCoupons.Checked = Convert.ToBoolean(reader["Perm_Coupons"]);
                                chkModalPermBanners.Checked = Convert.ToBoolean(reader["Perm_Banners"]);
                                chkModalPermTickets.Checked = Convert.ToBoolean(reader["Perm_Tickets"]);

                                phPermissionsModal.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
            }
        }

        protected void btnCreateUser_Click(object sender, EventArgs e)
        {
            string user = txtNewUser.Text.Trim();
            string mail = txtNewEmail.Text.Trim();
            string pass = txtNewPass.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(pass))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Users_FieldsRequired", "error");
                return;
            }

            int role = int.TryParse(ddlNewRole.SelectedValue, out int r) ? r : 3;
            string hashed = Security.Encrypt(pass);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"INSERT INTO users (Name_User, Mail, Password, Id_Role, Perm_Products, Perm_Orders, Perm_Offers, Perm_Coupons, Perm_Banners, Perm_Tickets)
                                     VALUES (@User, @Mail, @Pass, @Role, 0, 0, 0, 0, 0, 0);";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@User", user);
                    cmd.Parameters.AddWithValue("@Mail", mail);
                    cmd.Parameters.AddWithValue("@Pass", hashed);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.ExecuteNonQuery();
                }

                txtNewUser.Text = ""; txtNewEmail.Text = ""; txtNewPass.Text = ""; ddlNewRole.SelectedIndex = 0;
                TriggerAlert("Alert_SuccessTitle", "Alert_Users_CreatedText", "success");
                LoadUsers();
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
            }
        }

        protected void btnUpdateUser_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(hfEditUserId.Value);
            string user = txtEditUsername.Text.Trim();
            string mail = txtEditEmail.Text.Trim();
            string pass = txtEditPass.Text.Trim();
            int role = Convert.ToInt32(ddlEditRole.SelectedValue);

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(mail))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Users_FieldsEmpty", "warning");
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query;
                    MySqlCommand cmd;

                    if (!string.IsNullOrEmpty(pass))
                    {
                        query = "UPDATE users SET Name_User=@User, Mail=@Mail, Id_Role=@Role, Password=@Pass WHERE Id_User=@UserId";
                        cmd = new MySqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@Pass", Security.Encrypt(pass));
                    }
                    else
                    {
                        query = "UPDATE users SET Name_User=@User, Mail=@Mail, Id_Role=@Role WHERE Id_User=@UserId";
                        cmd = new MySqlCommand(query, con);
                    }

                    cmd.Parameters.AddWithValue("@User", user);
                    cmd.Parameters.AddWithValue("@Mail", mail);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }

                phEditUserModal.Visible = false;
                TriggerAlert("Alert_SuccessTitle", "Alert_Users_UpdatedText", "success");
                LoadUsers();
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
            }
        }

        protected void btnSavePermissions_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(hfPermUserId.Value);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"UPDATE users SET 
                                        Perm_Products = @PProd,
                                        Perm_Orders = @POrd,
                                        Perm_Offers = @POff,
                                        Perm_Coupons = @PCoup,
                                        Perm_Banners = @PBan,
                                        Perm_Tickets = @PTick
                                     WHERE Id_User = @UserId;";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PProd", chkModalPermProducts.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@POrd", chkModalPermOrders.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@POff", chkModalPermOffers.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@PCoup", chkModalPermCoupons.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@PBan", chkModalPermBanners.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@PTick", chkModalPermTickets.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }

                phPermissionsModal.Visible = false;
                TriggerAlert("Alert_SuccessTitle", "Alert_Users_PermissionsSavedText", "success");
                LoadUsers();
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
            }
        }

        protected void btnCloseEdit_Click(object sender, EventArgs e)
        {
            phEditUserModal.Visible = false;
        }

        protected void btnClosePerms_Click(object sender, EventArgs e)
        {
            phPermissionsModal.Visible = false;
        }

        private bool EsOwner(int userId)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT Id_Role FROM users WHERE Id_User = @UserId;", con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }

        private void TriggerAlert(string titleKey, string textKey, string icon)
        {
            string script = AlertHelper.GetSafeAlertScript(this, titleKey, textKey, icon);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "sweetalert", script, true);
        }

        // Side Navigation Redirects
        protected void btnManageProducts_Click(object sender, EventArgs e) { Response.Redirect("ManageProducts.aspx"); }
        protected void btnManageOrders_Click(object sender, EventArgs e) { Response.Redirect("ManageOrders.aspx"); }
        protected void btncerrar_Click(object sender, EventArgs e) { Session.Clear(); Session.Abandon(); Response.Redirect("Login.aspx"); }
        protected void btnAddLeague_Click(object sender, EventArgs e) { Response.Redirect("AddLeague.aspx"); }
        protected void btnAddTeam_Click(object sender, EventArgs e) { Response.Redirect("AddTeam.aspx"); }
        protected void btnAddBrand_Click(object sender, EventArgs e) { Response.Redirect("AddBrand.aspx"); }
        protected void btnManageUsers_Click(object sender, EventArgs e) { Response.Redirect("ManageUsers.aspx"); }
        protected void btnAdminBanners_Click(object sender, EventArgs e) { Response.Redirect("AdminBanners.aspx"); }
        protected void btnSmtpSettings_Click(object sender, EventArgs e) { Response.Redirect("SmtpSettings.aspx"); }
        protected void btnManageOffers_Click(object sender, EventArgs e) { Response.Redirect("ManageOffers.aspx"); }
        protected void btnStats_Click(object sender, EventArgs e) { Response.Redirect("AdminStats.aspx"); }
        protected void btnManageCoupons_Click(object sender, EventArgs e) { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e) { Response.Redirect("AdminAudit.aspx"); }
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