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
                LoadUsers();
            }
        }

        private void LoadUsers()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = @"SELECT u.Id_User, u.Name_User, u.Mail, u.Id_Role, r.Name_Role 
                                 FROM users u 
                                 INNER JOIN roles r ON u.Id_Role = r.Id_Role 
                                 ORDER BY u.Id_Role ASC, u.Id_User ASC;";
                MySqlCommand cmd = new MySqlCommand(query, con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView drv = (DataRowView)e.Row.DataItem;
            int currentRoleId = Convert.ToInt32(drv["Id_Role"]);

            LinkButton btnEdit = (LinkButton)e.Row.FindControl("btnEdit");
            LinkButton btnPermissions = (LinkButton)e.Row.FindControl("btnPermissions");
            LinkButton btnDelete = (LinkButton)e.Row.FindControl("btnDelete");
            Label lblOwnerProtect = (Label)e.Row.FindControl("lblOwnerProtect");

            // Protect the Owner
            if (currentRoleId == 1)
            {
                if (btnEdit != null) btnEdit.Visible = false;
                if (btnPermissions != null) btnPermissions.Visible = false;
                if (btnDelete != null) btnDelete.Visible = false;
                if (lblOwnerProtect != null) lblOwnerProtect.Visible = true;
            }
            else
            {
                // Only show permissions button if the user is an Admin (Role 2)
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

            // Double security check to prevent hacking the UI
            if (EsOwner(userId))
            {
                TriggerAlert("Access Denied", "The main Owner role cannot be modified or deleted.", "warning");
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
                    TriggerAlert("Deleted", "User has been permanently deleted.", "success");
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
                                txtEditPass.Text = ""; // Clear password field

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
                TriggerAlert("Error", ex.Message, "error");
            }
        }

        protected void btnCreateUser_Click(object sender, EventArgs e)
        {
            string user = txtNewUser.Text.Trim();
            string mail = txtNewEmail.Text.Trim();
            string pass = txtNewPass.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(pass))
            {
                TriggerAlert("Error", "Please fill all required fields.", "error");
                return;
            }

            int role = int.TryParse(ddlNewRole.SelectedValue, out int r) ? r : 3;
            string hashed = Security.Encrypt(pass);

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    // New admins are created with 0 permissions by default
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
                TriggerAlert("Success", "User created. If it is an Admin, click 'Permissions' in the grid to grant access.", "success");
                LoadUsers();
            }
            catch (Exception ex)
            {
                TriggerAlert("Error", ex.Message, "error");
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
                TriggerAlert("Error", "Username and Email cannot be empty.", "warning");
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
                TriggerAlert("Updated", "User information updated successfully.", "success");
                LoadUsers();
            }
            catch (Exception ex)
            {
                TriggerAlert("Error", ex.Message, "error");
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
                TriggerAlert("Saved", "Permissions have been configured successfully.", "success");
                LoadUsers();
            }
            catch (Exception ex)
            {
                TriggerAlert("Error", ex.Message, "error");
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

        private void TriggerAlert(string title, string text, string icon)
        {
            string safeTitle = title.Replace("'", "\\'");
            string safeText = text.Replace("'", "\\'").Replace("\r\n", " ");
            string script = $"Swal.fire('{safeTitle}', '{safeText}', '{icon}');";
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
    }
}