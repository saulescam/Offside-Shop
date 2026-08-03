using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace OFFSIDESHOP
{
    public partial class ManageSellerRequests : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Role validation: Admin (2) or Owner (1) only
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

            // PBAC guard: require Perm_Tickets
            if (!Security.HasPermission(Session, "Perm_Tickets"))
            {
                Response.Redirect("Dashboard.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                // Set default status tab in ViewState (1 = Pending)
                ViewState["ActiveStatus"] = 1;
                phOwnerMenu.Visible = (role == 1);

                LoadFilterReasons();
                LoadMappingDropdowns();
                LoadTickets();
            }
        }

        private void LoadFilterReasons()
        {
            // We load the filter reasons for the dropdown if needed, 
            // but the request type filter (ALL, GENERAL, ORDER, SELLER) is hardcoded in .aspx
        }

        private void LoadMappingDropdowns()
        {
            using (MySqlConnection conn = data.ObtenerConexion())
            {
                try
                {
                    conn.Open();
                    
                    // Brands
                    MySqlDataAdapter daBrand = new MySqlDataAdapter("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC", conn);
                    DataTable dtBrand = new DataTable();
                    daBrand.Fill(dtBrand);
                    ddlBrand.DataSource = dtBrand;
                    ddlBrand.DataTextField = "Name_Brand";
                    ddlBrand.DataValueField = "Id_Brand";
                    ddlBrand.DataBind();
                    ddlBrand.Items.Insert(0, new ListItem("-- Select Brand --", ""));

                    // Leagues
                    MySqlDataAdapter daLeague = new MySqlDataAdapter("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC", conn);
                    DataTable dtLeague = new DataTable();
                    daLeague.Fill(dtLeague);
                    ddlLeague.DataSource = dtLeague;
                    ddlLeague.DataTextField = "Name_League";
                    ddlLeague.DataValueField = "Id_League";
                    ddlLeague.DataBind();
                    ddlLeague.Items.Insert(0, new ListItem("-- Select League --", ""));

                    // Teams (Initialized empty until league is chosen)
                    ddlTeam.Items.Clear();
                    ddlTeam.Items.Insert(0, new ListItem("-- Select Team --", ""));
                }
                catch (Exception ex)
                {
                    ShowAlert("Error loading mapping dropdowns: " + ex.Message, "error");
                }
            }
        }

        private void LoadTickets()
        {
            int status = ViewState["ActiveStatus"] != null ? Convert.ToInt32(ViewState["ActiveStatus"]) : 1;
            string typeFilter = ddlFilterType.SelectedValue;

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = @"SELECT t.Id_Ticket, t.Created_At, t.User_Email, t.Status, r.Reason_Name, t.Subject 
                                 FROM contact_tickets t
                                 INNER JOIN contact_reasons r ON t.Id_ContactReason = r.Id_ContactReason
                                 WHERE t.Status = @Status";

                if (typeFilter == "GENERAL")
                {
                    query += " AND r.Requires_Order = 0 AND r.Requires_Images = 0";
                }
                else if (typeFilter == "ORDER")
                {
                    query += " AND r.Requires_Order = 1";
                }
                else if (typeFilter == "SELLER")
                {
                    query += " AND r.Requires_Images = 1";
                }

                query += " ORDER BY t.Created_At DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);

                    try
                    {
                        conn.Open();
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvTickets.DataSource = dt;
                        gvTickets.DataBind();
                    }
                    catch (Exception ex)
                    {
                        ShowAlert("Error loading tickets: " + ex.Message, "error");
                    }
                }
            }
        }

        protected void StatusTab_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            int status = Convert.ToInt32(btn.CommandArgument);
            ViewState["ActiveStatus"] = status;

            // Reset active classes
            btnTabPending.CssClass = "nav-link" + (status == 1 ? " active" : "");
            btnTabUnderReview.CssClass = "nav-link" + (status == 2 ? " active" : "");
            btnTabResolved.CssClass = "nav-link" + (status == 3 ? " active" : "");
            btnTabDenied.CssClass = "nav-link" + (status == 4 ? " active" : "");

            LoadTickets();
        }

        protected void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTickets();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ViewState["ActiveStatus"] = 1;
            ddlFilterType.SelectedIndex = 0;

            btnTabPending.CssClass = "nav-link active";
            btnTabUnderReview.CssClass = "nav-link";
            btnTabResolved.CssClass = "nav-link";
            btnTabDenied.CssClass = "nav-link";

            LoadTickets();
        }

        protected void gvTickets_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                Label lblBadge = (Label)e.Row.FindControl("lblStatusBadge");
                int status = Convert.ToInt32(rowView["Status"]);

                if (status == 1)
                {
                    lblBadge.Text = "<span class='badge bg-warning text-dark'>Pending</span>";
                }
                else if (status == 2)
                {
                    lblBadge.Text = "<span class='badge bg-info text-white'>Under Review</span>";
                }
                else if (status == 3)
                {
                    lblBadge.Text = "<span class='badge bg-success text-white'>Resolved / Approved</span>";
                }
                else if (status == 4)
                {
                    lblBadge.Text = "<span class='badge bg-danger text-white'>Denied</span>";
                }
            }
        }

        protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int ticketId = Convert.ToInt32(e.CommandArgument);
                LoadTicketDetails(ticketId);
            }
        }

        private void LoadTicketDetails(int ticketId)
        {
            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = @"SELECT t.*, r.Requires_Order, r.Requires_Images, r.Reason_Name 
                                 FROM contact_tickets t
                                 INNER JOIN contact_reasons r ON t.Id_ContactReason = r.Id_ContactReason
                                 WHERE t.Id_Ticket = @Id";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", ticketId);

                    try
                    {
                        conn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewState["ActiveTicketId"] = ticketId;
                                ViewState["ActiveRequiresImages"] = Convert.ToBoolean(reader["Requires_Images"]);
                                ViewState["ActiveUserId"] = reader["Id_User"];
                                ViewState["ActiveProposedPrice"] = reader["Proposed_Price"];
                                ViewState["ActiveItemCondition"] = reader["Item_Condition"];
                                ViewState["ActiveDescription"] = reader["Message_Body"];
                                ViewState["ActiveImage1"] = reader["ImageURL1"];
                                ViewState["ActiveImage2"] = reader["ImageURL2"];
                                ViewState["ActiveImage3"] = reader["ImageURL3"];

                                litModalTicketId.Text = reader["Id_Ticket"].ToString();
                                litModalUserEmail.Text = reader["User_Email"].ToString();
                                litModalCreatedAt.Text = Convert.ToDateTime(reader["Created_At"]).ToString("yyyy-MM-dd HH:mm");
                                litModalSubject.Text = reader["Subject"].ToString();
                                litModalMessage.Text = reader["Message_Body"].ToString();

                                txtAdminNotes.Text = reader["Admin_Notes"] != DBNull.Value ? reader["Admin_Notes"].ToString() : "";
                                lblModalError.Visible = false;

                                bool reqOrder = Convert.ToBoolean(reader["Requires_Order"]);
                                bool reqImages = Convert.ToBoolean(reader["Requires_Images"]);
                                int status = Convert.ToInt32(reader["Status"]);

                                // Dynamic panels configuration
                                pnlModalOrder.Visible = reqOrder;
                                if (reqOrder)
                                {
                                    litModalOrderId.Text = reader["Id_Order"] != DBNull.Value ? reader["Id_Order"].ToString() : "N/A";
                                }

                                pnlModalSeller.Visible = reqImages;
                                if (reqImages)
                                {
                                    litModalProposedPrice.Text = reader["Proposed_Price"] != DBNull.Value ? Convert.ToDecimal(reader["Proposed_Price"]).ToString("F2") : "0.00";
                                    litModalItemCondition.Text = reader["Item_Condition"] != DBNull.Value ? reader["Item_Condition"].ToString() : "Unknown";

                                    string path = "~/assets/uploads/tickets/";
                                    
                                    if (reader["ImageURL1"] != DBNull.Value && !string.IsNullOrEmpty(reader["ImageURL1"].ToString()))
                                    {
                                        imgModal1.Visible = true;
                                        imgModal1.ImageUrl = path + reader["ImageURL1"].ToString();
                                    }
                                    else
                                    {
                                        imgModal1.Visible = false;
                                    }

                                    if (reader["ImageURL2"] != DBNull.Value && !string.IsNullOrEmpty(reader["ImageURL2"].ToString()))
                                    {
                                        imgModal2.Visible = true;
                                        imgModal2.ImageUrl = path + reader["ImageURL2"].ToString();
                                    }
                                    else
                                    {
                                        imgModal2.Visible = false;
                                    }

                                    if (reader["ImageURL3"] != DBNull.Value && !string.IsNullOrEmpty(reader["ImageURL3"].ToString()))
                                    {
                                        imgModal3.Visible = true;
                                        imgModal3.ImageUrl = path + reader["ImageURL3"].ToString();
                                    }
                                    else
                                    {
                                        imgModal3.Visible = false;
                                    }

                                    // Display Catalog mapping if request is not yet resolved/denied
                                    pnlModalCatalogMapping.Visible = (status == 1 || status == 2);
                                    txtNewProductName.Text = reader["Subject"].ToString();
                                    txtYear.Text = "";
                                    ddlBrand.SelectedIndex = 0;
                                    ddlLeague.SelectedIndex = 0;
                                    ddlTeam.Items.Clear();
                                    ddlTeam.Items.Add(new ListItem("-- Select Team --", ""));
                                }

                                // Configuration of buttons based on state
                                if (status == 3 || status == 4)
                                {
                                    btnApprove.Visible = false;
                                    btnReject.Visible = false;
                                    txtAdminNotes.Enabled = false;
                                }
                                else
                                {
                                    btnApprove.Visible = true;
                                    btnReject.Visible = true;
                                    txtAdminNotes.Enabled = true;

                                    if (reqImages)
                                    {
                                        btnApprove.Text = "Approve & Add to Catalog";
                                    }
                                    else
                                    {
                                        btnApprove.Text = "Save Resolution";
                                    }
                                }

                                phDetailModal.Visible = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowAlert("Error loading ticket details: " + ex.Message, "error");
                    }
                }
            }
        }

        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            phDetailModal.Visible = false;
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            if (ViewState["ActiveTicketId"] == null) return;
            int ticketId = Convert.ToInt32(ViewState["ActiveTicketId"]);
            string notes = txtAdminNotes.Text.Trim();

            if (string.IsNullOrEmpty(notes))
            {
                lblModalError.Text = "You must provide a rejection reason in the response field to explain the rejection to the user.";
                lblModalError.Visible = true;
                return;
            }

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = "UPDATE contact_tickets SET Status = 4, Admin_Notes = @Notes, Resolved_At = NOW() WHERE Id_Ticket = @Id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Notes", notes);
                    cmd.Parameters.AddWithValue("@Id", ticketId);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        // Send Rejection Email
                        try
                        {
                            string email = litModalUserEmail.Text.Trim();
                            string subject = litModalSubject.Text.Trim();
                            EmailService.SendTicketDeniedNotification(email, ticketId.ToString(), subject, notes);
                        }
                        catch (Exception) { }

                        ShowAlert("Request Denied", "The support ticket has been marked as Denied.", "info");
                        phDetailModal.Visible = false;
                        LoadTickets();
                    }
                    catch (Exception ex)
                    {
                        lblModalError.Text = "Database error: " + ex.Message;
                        lblModalError.Visible = true;
                    }
                }
            }
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            if (ViewState["ActiveTicketId"] == null) return;
            int ticketId = Convert.ToInt32(ViewState["ActiveTicketId"]);
            bool isSeller = ViewState["ActiveRequiresImages"] != null ? Convert.ToBoolean(ViewState["ActiveRequiresImages"]) : false;
            string notes = txtAdminNotes.Text.Trim();

            if (isSeller)
            {
                // Seller Request Approval - Catalog Publication
                if (string.IsNullOrEmpty(txtNewProductName.Text.Trim()))
                {
                    lblModalError.Text = "Product Name is required to register this jersey in the catalog.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(ddlBrand.SelectedValue))
                {
                    lblModalError.Text = "Please select a Brand mapping.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(ddlTeam.SelectedValue))
                {
                    lblModalError.Text = "Please select a Team mapping.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(txtYear.Text.Trim()) || txtYear.Text.Trim().Length != 4 || !int.TryParse(txtYear.Text.Trim(), out _))
                {
                    lblModalError.Text = "Please enter a valid 4-digit Year.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(notes))
                {
                    lblModalError.Text = "Please provide some notes in the response field to notify the seller.";
                    lblModalError.Visible = true;
                    return;
                }

                decimal price = ViewState["ActiveProposedPrice"] != DBNull.Value ? Convert.ToDecimal(ViewState["ActiveProposedPrice"]) : 0;
                object userId = ViewState["ActiveUserId"] != null ? ViewState["ActiveUserId"] : DBNull.Value;
                string condition = ViewState["ActiveItemCondition"] != null ? ViewState["ActiveItemCondition"].ToString() : "";
                string description = ViewState["ActiveDescription"] != null ? ViewState["ActiveDescription"].ToString() : "";

                string img1 = ViewState["ActiveImage1"] != null ? ViewState["ActiveImage1"].ToString() : "";
                string img2 = ViewState["ActiveImage2"] != null ? ViewState["ActiveImage2"].ToString() : "";
                string img3 = ViewState["ActiveImage3"] != null ? ViewState["ActiveImage3"].ToString() : "";

                using (MySqlConnection conn = data.ObtenerConexion())
                {
                    conn.Open();
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Copy images from uploads/tickets to images/camisetas
                            string sourcePath = Server.MapPath("~/assets/uploads/tickets/");
                            string targetPath = Server.MapPath("~/images/camisetas/");

                            if (!Directory.Exists(targetPath))
                            {
                                Directory.CreateDirectory(targetPath);
                            }

                            if (!string.IsNullOrEmpty(img1) && File.Exists(sourcePath + img1))
                            {
                                File.Copy(sourcePath + img1, targetPath + img1, true);
                            }
                            if (!string.IsNullOrEmpty(img2) && File.Exists(sourcePath + img2))
                            {
                                File.Copy(sourcePath + img2, targetPath + img2, true);
                            }
                            if (!string.IsNullOrEmpty(img3) && File.Exists(sourcePath + img3))
                            {
                                File.Copy(sourcePath + img3, targetPath + img3, true);
                            }

                            // 2. Insert into tshirts catalog - CORREGIDO: Id_KitType cambiado de 8 a 4 (Retro) y nombres de columnas sincronizados
                            string insertSql = @"INSERT INTO tshirts 
                                         (Name, Id_Brand, Id_Team, Year, Id_KitType, Price, ImageURL, ImageURL2, ImageURL3, Description, IsPreOwned, ItemCondition, Id_OwnerUser) 
                                         VALUES (@Name, @BrandId, @TeamId, @Year, 7, @Price, @Img1, @Img2, @Img3, @Desc, 1, @Cond, @OwnerId);
                                         SELECT LAST_INSERT_ID();";

                            int newTshirtId = 0;
                            using (MySqlCommand cmdInsert = new MySqlCommand(insertSql, conn, transaction))
                            {
                                cmdInsert.Parameters.AddWithValue("@Name", txtNewProductName.Text.Trim());
                                cmdInsert.Parameters.AddWithValue("@BrandId", Convert.ToInt32(ddlBrand.SelectedValue));
                                cmdInsert.Parameters.AddWithValue("@TeamId", Convert.ToInt32(ddlTeam.SelectedValue));
                                cmdInsert.Parameters.AddWithValue("@Year", Convert.ToInt32(txtYear.Text.Trim()));
                                cmdInsert.Parameters.AddWithValue("@Price", price);
                                cmdInsert.Parameters.AddWithValue("@Img1", string.IsNullOrEmpty(img1) ? (object)DBNull.Value : img1);
                                cmdInsert.Parameters.AddWithValue("@Img2", string.IsNullOrEmpty(img2) ? (object)DBNull.Value : img2);
                                cmdInsert.Parameters.AddWithValue("@Img3", string.IsNullOrEmpty(img3) ? (object)DBNull.Value : img3);
                                cmdInsert.Parameters.AddWithValue("@Desc", description);
                                cmdInsert.Parameters.AddWithValue("@Cond", string.IsNullOrEmpty(condition) ? (object)DBNull.Value : condition);
                                cmdInsert.Parameters.AddWithValue("@OwnerId", userId);

                                newTshirtId = Convert.ToInt32(cmdInsert.ExecuteScalar());
                            }

                            // 3. Add default size variant (Size L, Stock 1)
                            string insertVariantSql = "INSERT INTO tshirt_variants (Id_Tshirt, Id_Size, Stock) VALUES (@TshirtId, 3, 1)";
                            using (MySqlCommand cmdVariant = new MySqlCommand(insertVariantSql, conn, transaction))
                            {
                                cmdVariant.Parameters.AddWithValue("@TshirtId", newTshirtId);
                                cmdVariant.ExecuteNonQuery();
                            }

                            // 4. Update support ticket status
                            string updateTicketSql = "UPDATE contact_tickets SET Status = 3, Admin_Notes = @Notes, Resolved_At = NOW() WHERE Id_Ticket = @Id";
                            using (MySqlCommand cmdUpdate = new MySqlCommand(updateTicketSql, conn, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@Notes", notes);
                                cmdUpdate.Parameters.AddWithValue("@Id", ticketId);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            // Send Approval Email for Seller Request
                            try
                            {
                                string email = litModalUserEmail.Text.Trim();
                                string subject = litModalSubject.Text.Trim();
                                EmailService.SendTicketApprovedNotification(email, ticketId.ToString(), subject, notes, true);
                            }
                            catch (Exception) { }

                            ShowAlert("Approved & Published", "The request was successfully approved and the jersey added to the catalog.", "success");
                            phDetailModal.Visible = false;
                            LoadTickets();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            lblModalError.Text = "Transaction failed: " + ex.Message;
                            lblModalError.Visible = true;
                        }
                    }
                }
            }
            else
            {
                // Standard Support Ticket / Order Issue Approval
                using (MySqlConnection conn = data.ObtenerConexion())
                {
                    string query = "UPDATE contact_tickets SET Status = 3, Admin_Notes = @Notes, Resolved_At = NOW() WHERE Id_Ticket = @Id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Notes", notes);
                        cmd.Parameters.AddWithValue("@Id", ticketId);

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();

                            // Send Approval Email for Standard Support
                            try
                            {
                                string email = litModalUserEmail.Text.Trim();
                                string subject = litModalSubject.Text.Trim();
                                EmailService.SendTicketApprovedNotification(email, ticketId.ToString(), subject, notes, false);
                            }
                            catch (Exception) { }

                            ShowAlert("Resolved", "The ticket has been marked as Resolved.", "success");
                            phDetailModal.Visible = false;
                            LoadTickets();
                        }
                        catch (Exception ex)
                        {
                            lblModalError.Text = "Database error: " + ex.Message;
                            lblModalError.Visible = true;
                        }
                    }
                }
            }
        }

        private void ShowAlert(string title, string text, string icon)
        {
            string script = $"Swal.fire('{title.Replace("'", "\\'")}', '{text.Replace("'", "\\'")}', '{icon}');";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", script, true);
        }

        private void ShowAlert(string text, string icon)
        {
            ShowAlert("Alert", text, icon);
        }

        // Sidebar Redirections
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

        protected void ddlLeague_SelectedIndexChanged(object sender, EventArgs e)
        {
            string leagueId = ddlLeague.SelectedValue;
            LoadTeamsByLeague(leagueId);
        }

        private void LoadTeamsByLeague(string leagueId)
        {
            ddlTeam.Items.Clear();
            ddlTeam.Items.Add(new ListItem("-- Select Team --", ""));

            if (string.IsNullOrEmpty(leagueId)) return;

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = "SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @LeagueId ORDER BY Name_Team ASC";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LeagueId", leagueId);
                    try
                    {
                        conn.Open();
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlTeam.DataSource = dt;
                        ddlTeam.DataTextField = "Name_Team";
                        ddlTeam.DataValueField = "Id_Team";
                        ddlTeam.DataBind();
                    }
                    catch (Exception ex)
                    {
                        ShowAlert("Error loading teams: " + ex.Message, "error");
                    }
                }
            }
        }

        protected void MainView_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string view = btn.CommandArgument;

            if (view == "Tickets")
            {
                btnViewTickets.CssClass = "nav-link active";
                btnViewReviews.CssClass = "nav-link";
                pnlTickets.Visible = true;
                pnlReviews.Visible = false;
                LoadTickets();
            }
            else if (view == "Reviews")
            {
                btnViewTickets.CssClass = "nav-link";
                btnViewReviews.CssClass = "nav-link active";
                pnlTickets.Visible = false;
                pnlReviews.Visible = true;
                LoadReviews();
            }
        }

        private void LoadReviews()
        {
            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = @"SELECT r.Id_Review, r.Id_Tshirt, r.Rating, r.Comment, r.ReviewDate, 
                                        r.ReplyComment, r.ReplyDate, u.Name AS UserName, u.LastName AS UserLastName, 
                                        u.Mail AS UserEmail, t.Name AS ShirtName 
                                 FROM product_reviews r 
                                 INNER JOIN users u ON r.Id_User = u.Id_User 
                                 INNER JOIN tshirts t ON r.Id_Tshirt = t.ID 
                                 ORDER BY r.ReviewDate DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvReviews.DataSource = dt;
                        gvReviews.DataBind();
                    }
                    catch (Exception ex)
                    {
                        ShowAlert("Error loading reviews: " + ex.Message, "error");
                    }
                }
            }
        }

        protected void gvReviews_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteReview")
            {
                int reviewId = Convert.ToInt32(e.CommandArgument);
                using (MySqlConnection conn = data.ObtenerConexion())
                {
                    string query = "DELETE FROM product_reviews WHERE Id_Review = @Id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", reviewId);
                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            ShowAlert("Deleted", "The review has been deleted.", "success");
                            LoadReviews();
                        }
                        catch (Exception ex)
                        {
                            ShowAlert("Error deleting review: " + ex.Message, "error");
                        }
                    }
                }
            }
            else if (e.CommandName == "ReplyReview")
            {
                int reviewId = Convert.ToInt32(e.CommandArgument);
                ViewState["ActiveReviewId"] = reviewId;
                
                // Get existing comment to display in the modal
                using (MySqlConnection conn = data.ObtenerConexion())
                {
                    string query = "SELECT Comment, ReplyComment FROM product_reviews WHERE Id_Review = @Id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", reviewId);
                        try
                        {
                            conn.Open();
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    litReplyReviewId.Text = reviewId.ToString();
                                    litOriginalReview.Text = reader["Comment"].ToString();
                                    txtReplyComment.Text = reader["ReplyComment"] != DBNull.Value ? reader["ReplyComment"].ToString() : "";
                                    lblReplyModalError.Visible = false;
                                    phReplyModal.Visible = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowAlert("Error loading review details: " + ex.Message, "error");
                        }
                    }
                }
            }
        }

        protected void btnCloseReplyModal_Click(object sender, EventArgs e)
        {
            phReplyModal.Visible = false;
        }

        protected void btnSubmitReply_Click(object sender, EventArgs e)
        {
            if (ViewState["ActiveReviewId"] == null) return;
            int reviewId = Convert.ToInt32(ViewState["ActiveReviewId"]);
            string reply = txtReplyComment.Text.Trim();

            if (string.IsNullOrEmpty(reply))
            {
                lblReplyModalError.Text = "Please write a reply.";
                lblReplyModalError.Visible = true;
                return;
            }

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = "UPDATE product_reviews SET ReplyComment = @Reply, ReplyDate = NOW() WHERE Id_Review = @Id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Reply", reply);
                    cmd.Parameters.AddWithValue("@Id", reviewId);
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        ShowAlert("Success", "Your reply has been saved.", "success");
                        phReplyModal.Visible = false;
                        LoadReviews();
                    }
                    catch (Exception ex)
                    {
                        lblReplyModalError.Text = "Error saving reply: " + ex.Message;
                        lblReplyModalError.Visible = true;
                    }
                }
            }
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}