using System;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace OFFSIDESHOP
{
    public partial class ManageSellerRequests : BasePage
    {
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

                // Fix: Set Text programmatically so UpdatePanel partial postbacks preserve the HTML content
                btnViewTickets.Text = "<i class=\"fas fa-ticket-alt mr-2\"></i>" + (GetGlobalResourceObject("Strings", "Admin_Seller_TabTickets") ?? "Support Tickets");
                btnViewReviews.Text = "<i class=\"fas fa-star mr-2\"></i>" + (GetGlobalResourceObject("Strings", "Admin_Seller_TabReviews") ?? "Product Reviews");

                btnTabPending.Text = "<i class=\"fas fa-clock mr-2\"></i>" + (GetGlobalResourceObject("Strings", "Admin_Seller_StatusPending") ?? "Pending");
                btnTabUnderReview.Text = "<i class=\"fas fa-search mr-2\"></i>" + (GetGlobalResourceObject("Strings", "Admin_Seller_StatusUnderReview") ?? "Under Review");
                btnTabResolved.Text = "<i class=\"fas fa-check-circle mr-2\"></i>" + (GetGlobalResourceObject("Strings", "Admin_Seller_StatusResolved") ?? "Resolved");
                btnTabDenied.Text = "<i class=\"fas fa-times-circle mr-2\"></i>" + (GetGlobalResourceObject("Strings", "Admin_Seller_StatusDenied") ?? "Denied");

                LoadFilterReasons();
                LoadMappingDropdowns();
                LoadTickets();
            }
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }

        private void LoadFilterReasons()
        {
            // Request type filter is hardcoded in .aspx
        }

        private void LoadMappingDropdowns()
        {
            using (MySqlConnection conn = data.ObtenerConexion())
            {
                try
                {
                    conn.Open();

                    bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                    string selectBrandText = isSpanish ? "-- Seleccionar Marca --" : "-- Select Brand --";
                    string selectLeagueText = isSpanish ? "-- Seleccionar Liga --" : "-- Select League --";
                    string selectTeamText = isSpanish ? "-- Seleccionar Equipo --" : "-- Select Team --";

                    // Brands
                    MySqlDataAdapter daBrand = new MySqlDataAdapter("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC", conn);
                    DataTable dtBrand = new DataTable();
                    daBrand.Fill(dtBrand);
                    ddlBrand.DataSource = dtBrand;
                    ddlBrand.DataTextField = "Name_Brand";
                    ddlBrand.DataValueField = "Id_Brand";
                    ddlBrand.DataBind();
                    ddlBrand.Items.Insert(0, new ListItem(selectBrandText, ""));

                    // Leagues
                    MySqlDataAdapter daLeague = new MySqlDataAdapter("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC", conn);
                    DataTable dtLeague = new DataTable();
                    daLeague.Fill(dtLeague);
                    ddlLeague.DataSource = dtLeague;
                    ddlLeague.DataTextField = "Name_League";
                    ddlLeague.DataValueField = "Id_League";
                    ddlLeague.DataBind();
                    ddlLeague.Items.Insert(0, new ListItem(selectLeagueText, ""));

                    // Teams (Initialized empty until league is chosen)
                    ddlTeam.Items.Clear();
                    ddlTeam.Items.Insert(0, new ListItem(selectTeamText, ""));
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

            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            string reasonCol = isSpanish ? "r.Reason_Name_es" : "r.Reason_Name";

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = $@"SELECT t.Id_Ticket, t.Created_At, t.User_Email, t.Status, {reasonCol} AS Reason_Name, t.Subject, t.ImageURL1 
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

        public string GetThumbnailHtml(object dbImg)
        {
            string url = ResolveTicketImageUrl(dbImg);
            if (string.IsNullOrEmpty(url))
            {
                return "<span class='text-muted' style='font-size: 0.85rem;'>-</span>";
            }
            return $"<img src='{url}' style='width: 44px; height: 44px; object-fit: cover; border-radius: 6px; cursor: pointer; border: 1px solid var(--border-color);' onclick='openFullscreenImage(this);' onerror=\"this.style.display='none';\" title='Click to zoom' />";
        }

        private string GetTicketImage(MySqlDataReader reader, int imageIndex)
        {
            string[] possibleCols;
            if (imageIndex == 1)
                possibleCols = new[] { "ImageURL1", "ImageURL", "Image1", "image_url1", "image_url", "ImageUrl1", "ImageUrl" };
            else if (imageIndex == 2)
                possibleCols = new[] { "ImageURL2", "Image2", "image_url2", "ImageUrl2" };
            else
                possibleCols = new[] { "ImageURL3", "Image3", "image_url3", "ImageUrl3" };

            foreach (var col in possibleCols)
            {
                try
                {
                    int ordinal = reader.GetOrdinal(col);
                    if (!reader.IsDBNull(ordinal))
                    {
                        string val = reader[ordinal]?.ToString();
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            return val.Trim();
                        }
                    }
                }
                catch { }
            }
            return string.Empty;
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

                string pendingText = GetGlobalResourceObject("Strings", "Admin_Seller_StatusPending")?.ToString() ?? "Pending";
                string underReviewText = GetGlobalResourceObject("Strings", "Admin_Seller_StatusUnderReview")?.ToString() ?? "Under Review";
                string resolvedText = GetGlobalResourceObject("Strings", "Admin_Seller_StatusResolved")?.ToString() ?? "Resolved / Approved";
                string deniedText = GetGlobalResourceObject("Strings", "Admin_Seller_StatusDenied")?.ToString() ?? "Denied";

                if (status == 1)
                {
                    lblBadge.Text = $"<span class='badge bg-warning text-dark'>{pendingText}</span>";
                }
                else if (status == 2)
                {
                    lblBadge.Text = $"<span class='badge bg-info text-white'>{underReviewText}</span>";
                }
                else if (status == 3)
                {
                    lblBadge.Text = $"<span class='badge bg-success text-white'>{resolvedText}</span>";
                }
                else if (status == 4)
                {
                    lblBadge.Text = $"<span class='badge bg-danger text-white'>{deniedText}</span>";
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
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            string reasonCol = isSpanish ? "r.Reason_Name_es" : "r.Reason_Name";

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                string query = $@"SELECT t.*, r.Requires_Order, r.Requires_Images, {reasonCol} AS Reason_Name 
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
                                int idContactReason = Convert.ToInt32(reader["Id_ContactReason"]);
                                bool isSeller = (idContactReason == 3);
                                bool isRefundOrExchange = (idContactReason == 2);
                                bool reqOrder = Convert.ToBoolean(reader["Requires_Order"]);
                                bool reqImages = Convert.ToBoolean(reader["Requires_Images"]);
                                int status = Convert.ToInt32(reader["Status"]);

                                string rawImg1 = GetTicketImage(reader, 1);
                                string rawImg2 = GetTicketImage(reader, 2);
                                string rawImg3 = GetTicketImage(reader, 3);

                                ViewState["ActiveTicketId"] = ticketId;
                                ViewState["ActiveContactReasonId"] = idContactReason;
                                ViewState["ActiveIsSeller"] = isSeller;
                                ViewState["ActiveRequiresImages"] = reqImages;
                                ViewState["ActiveUserId"] = reader["Id_User"];
                                ViewState["ActiveProposedPrice"] = reader["Proposed_Price"];
                                ViewState["ActiveItemCondition"] = reader["Item_Condition"];
                                ViewState["ActiveDescription"] = reader["Message_Body"];
                                ViewState["ActiveImage1"] = rawImg1;
                                ViewState["ActiveImage2"] = rawImg2;
                                ViewState["ActiveImage3"] = rawImg3;

                                litModalTicketId.Text = reader["Id_Ticket"].ToString();
                                litModalUserEmail.Text = reader["User_Email"].ToString();
                                litModalCreatedAt.Text = Convert.ToDateTime(reader["Created_At"]).ToString("yyyy-MM-dd HH:mm");
                                litModalSubject.Text = reader["Subject"].ToString();
                                litModalMessage.Text = reader["Message_Body"].ToString();

                                txtAdminNotes.Text = reader["Admin_Notes"] != DBNull.Value ? reader["Admin_Notes"].ToString() : "";
                                lblModalError.Visible = false;

                                // 1. Dynamic Panel: Order ID (for Reason 1, 2, or any Requires_Order)
                                pnlModalOrder.Visible = reqOrder;
                                if (reqOrder)
                                {
                                    litModalOrderId.Text = reader["Id_Order"] != DBNull.Value ? reader["Id_Order"].ToString() : "N/A";
                                }

                                // 2. Dynamic Panel: Consignment Info (Price & Condition - ONLY for Reason 3: Sell Jersey)
                                pnlModalSeller.Visible = isSeller;
                                if (isSeller)
                                {
                                    litModalProposedPrice.Text = reader["Proposed_Price"] != DBNull.Value ? Convert.ToDecimal(reader["Proposed_Price"]).ToString("F2") : "0.00";
                                    litModalItemCondition.Text = reader["Item_Condition"] != DBNull.Value ? reader["Item_Condition"].ToString() : "Unknown";
                                }

                                // 3. Dynamic Panel: Images Gallery (for Reason 2 Refund/Exchange, Reason 3 Seller, or any ticket with images)
                                string url1 = ResolveTicketImageUrl(rawImg1);
                                string url2 = ResolveTicketImageUrl(rawImg2);
                                string url3 = ResolveTicketImageUrl(rawImg3);

                                bool has1 = !string.IsNullOrEmpty(url1);
                                bool has2 = !string.IsNullOrEmpty(url2);
                                bool has3 = !string.IsNullOrEmpty(url3);
                                bool hasAnyImage = has1 || has2 || has3;

                                if (isRefundOrExchange)
                                {
                                    litModalImagesTitle.Text = isSpanish ? "Evidencia Fotográfica del Problema" : "Photographic Evidence of the Issue";
                                }
                                else if (isSeller)
                                {
                                    litModalImagesTitle.Text = isSpanish ? "Fotos de la Camiseta a Vender" : "Jersey Proof Photos";
                                }
                                else
                                {
                                    litModalImagesTitle.Text = isSpanish ? "Imágenes Adjuntas" : "Attached Proof Images";
                                }

                                if (hasAnyImage)
                                {
                                    pnlModalImages.Visible = true;
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("<div class='row mb-0'>");

                                    string fullscreenText = GetGlobalResourceObject("Strings", "Admin_Seller_ModalFullscreen")?.ToString() ?? "Full Screen";

                                    if (has1)
                                    {
                                        sb.Append("<div class='col-md-4 text-center mb-2'>");
                                        sb.Append($"<img src='{url1}' class='img-fluid rounded border zoom-effect' style='height: 145px; object-fit: cover; width: 100%; cursor: pointer;' onclick='openFullscreenImage(this);' title='Click to zoom' onerror=\"this.onerror=null; this.src='assets/img/default-product.jpg';\" />");
                                        sb.Append($"<button type='button' class='btn btn-sm btn-outline-warning mt-2 w-100 font-weight-bold' onclick='openFullscreenImage(this.previousElementSibling);'>");
                                        sb.Append($"<i class='fas fa-expand-arrows-alt'></i> {fullscreenText}</button>");
                                        sb.Append("</div>");
                                    }

                                    if (has2)
                                    {
                                        sb.Append("<div class='col-md-4 text-center mb-2'>");
                                        sb.Append($"<img src='{url2}' class='img-fluid rounded border zoom-effect' style='height: 145px; object-fit: cover; width: 100%; cursor: pointer;' onclick='openFullscreenImage(this);' title='Click to zoom' onerror=\"this.onerror=null; this.src='assets/img/default-product.jpg';\" />");
                                        sb.Append($"<button type='button' class='btn btn-sm btn-outline-warning mt-2 w-100 font-weight-bold' onclick='openFullscreenImage(this.previousElementSibling);'>");
                                        sb.Append($"<i class='fas fa-expand-arrows-alt'></i> {fullscreenText}</button>");
                                        sb.Append("</div>");
                                    }

                                    if (has3)
                                    {
                                        sb.Append("<div class='col-md-4 text-center mb-2'>");
                                        sb.Append($"<img src='{url3}' class='img-fluid rounded border zoom-effect' style='height: 145px; object-fit: cover; width: 100%; cursor: pointer;' onclick='openFullscreenImage(this);' title='Click to zoom' onerror=\"this.onerror=null; this.src='assets/img/default-product.jpg';\" />");
                                        sb.Append($"<button type='button' class='btn btn-sm btn-outline-warning mt-2 w-100 font-weight-bold' onclick='openFullscreenImage(this.previousElementSibling);'>");
                                        sb.Append($"<i class='fas fa-expand-arrows-alt'></i> {fullscreenText}</button>");
                                        sb.Append("</div>");
                                    }

                                    sb.Append("</div>");
                                    litModalImagesGallery.Text = sb.ToString();
                                }
                                else if (reqImages)
                                {
                                    pnlModalImages.Visible = true;
                                    string noImagesMsg = isSpanish ? "No se adjuntaron imágenes en esta solicitud." : "No proof images were attached to this request.";
                                    litModalImagesGallery.Text = $"<p class='text-muted small font-italic mb-0'>{noImagesMsg}</p>";
                                }
                                else
                                {
                                    pnlModalImages.Visible = false;
                                    litModalImagesGallery.Text = "";
                                }

                                // 4. Dynamic Panel: Catalog Mapping (ONLY for Reason 3: Sell Jersey and when pending/under review)
                                pnlModalCatalogMapping.Visible = (isSeller && (status == 1 || status == 2));
                                if (pnlModalCatalogMapping.Visible)
                                {
                                    txtNewProductName.Text = reader["Subject"].ToString();
                                    txtYear.Text = "";
                                    ddlBrand.SelectedIndex = 0;
                                    ddlLeague.SelectedIndex = 0;
                                    ddlTeam.Items.Clear();
                                    ddlTeam.Items.Add(new ListItem(isSpanish ? "-- Seleccionar Equipo --" : "-- Select Team --", ""));
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

                                    if (isSeller)
                                    {
                                        btnApprove.Text = isSpanish ? "Aprobar y Publicar en Catálogo" : "Approve & Publish Catalog";
                                    }
                                    else
                                    {
                                        btnApprove.Text = isSpanish ? "Aceptar / Resolver Solicitud" : "Accept / Resolve Request";
                                    }
                                    btnReject.Text = isSpanish ? "Rechazar Solicitud" : "Reject & Deny Request";
                                }

                                btnCancel.Text = GetGlobalResourceObject("Strings", "Admin_Seller_ModalClose")?.ToString() ?? "Close Details";
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
                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                lblModalError.Text = isSpanish ? "Debe proporcionar un motivo de rechazo en el campo de respuesta para explicárselo al usuario." : "You must provide a rejection reason in the response field to explain the rejection to the user.";
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

                        ShowAlert("Alert_Seller_RequestDeniedTitle", "Alert_Seller_RequestDeniedText", "info");
                        phDetailModal.Visible = false;
                        LoadTickets();
                        AuditLogger.LogActivity("DENY", "ManageSellerRequests", $"Denied ticket ID #{ticketId}");

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
            bool isSeller = ViewState["ActiveIsSeller"] != null ? Convert.ToBoolean(ViewState["ActiveIsSeller"]) : false;
            string notes = txtAdminNotes.Text.Trim();

            if (isSeller)
            {
                // Seller Request Approval - Catalog Publication
                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                if (string.IsNullOrEmpty(txtNewProductName.Text.Trim()))
                {
                    lblModalError.Text = isSpanish ? "El nombre del producto es obligatorio para registrar esta camiseta en el catálogo." : "Product Name is required to register this jersey in the catalog.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(ddlBrand.SelectedValue))
                {
                    lblModalError.Text = isSpanish ? "Por favor seleccione una Marca." : "Please select a Brand mapping.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(ddlTeam.SelectedValue))
                {
                    lblModalError.Text = isSpanish ? "Por favor seleccione un Equipo." : "Please select a Team mapping.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(txtYear.Text.Trim()) || txtYear.Text.Trim().Length != 4 || !int.TryParse(txtYear.Text.Trim(), out _))
                {
                    lblModalError.Text = isSpanish ? "Por favor ingrese un año válido de 4 dígitos." : "Please enter a valid 4-digit Year.";
                    lblModalError.Visible = true;
                    return;
                }

                if (string.IsNullOrEmpty(notes))
                {
                    lblModalError.Text = isSpanish ? "Por favor proporcione notas en el campo de respuesta para notificar al vendedor." : "Please provide some notes in the response field to notify the seller.";
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

                            // 2. Insert into tshirts catalog (Id_KitType 7 = Retro)
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

                            ShowAlert("Alert_Seller_ApprovedTitle", "Alert_Seller_ApprovedText", "success");
                            phDetailModal.Visible = false;
                            LoadTickets();
                            AuditLogger.LogActivity("APPROVE", "ManageSellerRequests", $"Approved ticket ID #{ticketId}");

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

                            ShowAlert("Alert_Seller_ResolvedTitle", "Alert_Seller_ResolvedText", "success");
                            AuditLogger.LogActivity("RESOLVE", "ManageSellerRequests", $"Resolved ticket ID #{ticketId}");

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

        private void ShowAlert(string titleKey, string textKey, string icon)
        {
            string script = AlertHelper.GetSafeAlertScript(this, titleKey, textKey, icon);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", script, true);
        }

        private void ShowAlert(string textKey, string icon)
        {
            string titleKey = icon == "error" ? "Alert_ErrorTitle" : "Alert_WarningTitle";
            ShowAlert(titleKey, textKey, icon);
        }

        public string ResolveTicketImageUrl(object dbVal)
        {
            if (dbVal == null || dbVal == DBNull.Value) return string.Empty;
            string val = dbVal.ToString().Trim();
            if (string.IsNullOrEmpty(val)) return string.Empty;

            if (val.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                val.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return val;
            }

            string clean = val.TrimStart('~', '/');

            if (clean.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) || 
                clean.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveUrl("~/" + clean);
            }

            return ResolveUrl("~/assets/uploads/tickets/" + clean);
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
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
            string selectTeamText = isSpanish ? "-- Seleccionar Equipo --" : "-- Select Team --";
            ddlTeam.Items.Clear();
            ddlTeam.Items.Add(new ListItem(selectTeamText, ""));

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
                            ShowAlert("Alert_DeletedTitle", "Alert_Seller_ReviewDeletedText", "success");
                            AuditLogger.LogActivity("DELETE", "ManageSellerRequests", $"Deleted review ID #{reviewId}");

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
                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                lblReplyModalError.Text = isSpanish ? "Por favor escriba una respuesta." : "Please write a reply.";
                lblReplyModalError.Visible = true;
                return;
            }

            if (!IsTextAllowed(reply))
            {
                lblReplyModalError.Text = GetGlobalResourceObject("Strings", "Alert_Details_ForbiddenReviewText")?.ToString() ?? "The reply contains restricted terms or inappropriate language.";
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
                        ShowAlert("Alert_SuccessTitle", "Alert_Seller_ReplySavedText", "success");
                        AuditLogger.LogActivity("REPLY", "ManageSellerRequests", $"Replied to review ID #{reviewId}");

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

        private bool IsTextAllowed(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;

            string rawText = text.Trim().ToLower();
            string cleanedName = rawText.Replace(" ", "").Replace("\r", "").Replace("\n", "");

            using (MySqlConnection conn = data.ObtenerConexion())
            {
                conn.Open();
                string query = @"SELECT COUNT(*) FROM censorship 
                                 WHERE LOWER(@RawText) LIKE CONCAT('%', LOWER(pattern), '%')
                                    OR LOWER(@CleanedName) LIKE CONCAT('%', LOWER(pattern), '%');";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RawText", rawText);
                    cmd.Parameters.AddWithValue("@CleanedName", cleanedName);
                    long count = Convert.ToInt64(cmd.ExecuteScalar());

                    return count == 0;
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