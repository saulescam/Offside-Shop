using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class AdminBanners : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserRole"] == null || (Convert.ToInt32(Session["UserRole"]) != 1 && Convert.ToInt32(Session["UserRole"]) != 2))
            {
                Response.Redirect("Login.aspx");
                return;
            }
            if (!Security.HasPermission(Session, "Perm_Banners"))
            {
                Response.Redirect("Dashboard.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                // Banners
                ResetForm();
                LoadBanners();

                // Collections
                LoadCategories();
                ResetColForm();
                LoadCollections();

                // Auth Carousel
                ResetAuthForm();
                LoadAuthCarousel();
            }
        }

        protected string GetImageThumb(string imageUrl, string folder)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return "assets/img/offsideshop_logo_white_letras.png";
            if (imageUrl.StartsWith("http") || imageUrl.StartsWith("assets/")) return imageUrl;
            return $"images/{folder}/{imageUrl}";
        }

        // ========================== BANNERS ==========================
        private void ResetForm()
        {
            hfEditId.Value = "0"; txtTitle.Text = ""; txtSubtitle.Text = ""; txtLinkURL.Text = "";
            ddlIsActive.SelectedIndex = 0; lblFormTitle.Text = "Add New Banner"; lblImageRequired.Visible = true;
            pnlCurrentImage.Visible = false; lblCurrentImagePath.Text = "";
        }

        private void LoadBanners()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT ID, Title, Subtitle, ImageURL, LinkURL, SortOrder, IsActive FROM banners ORDER BY SortOrder ASC;", con);
                    DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt);
                    gvBanners.DataSource = dt; gvBanners.DataBind();
                }
            }
            catch (Exception ex)
            {
                Alert("Error loading banners: " + ex.Message, "error");
            }
        }

        protected void gvBanners_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                LinkButton btnToggle = (LinkButton)e.Row.FindControl("btnToggle");
                bool isActive = Convert.ToInt32(row["IsActive"]) == 1;

                if (lblStatus != null) lblStatus.Text = isActive ? "<span class='status-badge status-active'>Active</span>" : "<span class='status-badge status-inactive'>Inactive</span>";
                if (btnToggle != null)
                {
                    btnToggle.CssClass = isActive ? "action-icon toggle-icon" : "action-icon toggle-on";
                    btnToggle.Text = isActive ? "<i class='fas fa-eye-slash'></i>" : "<i class='fas fa-eye'></i>";
                }
            }
        }

        protected void gvBanners_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditBanner") LoadBannerForEdit(id);
            else if (e.CommandName == "ToggleBanner") { ExecuteToggle("UPDATE banners SET IsActive = 1 - IsActive WHERE ID = @Id", id); LoadBanners(); }
            else if (e.CommandName == "DeleteBanner") { ExecuteDelete("DELETE FROM banners WHERE ID = @Id", id); ResetForm(); LoadBanners(); }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) { Alert("Title is required.", "error"); return; }
            bool isEditing = Convert.ToInt32(hfEditId.Value) > 0;
            string imgFile = HandleImageUpload(fileImagen, "banners", isEditing);
            if (imgFile == "ERROR") return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    if (!isEditing)
                    {
                        int sort = Convert.ToInt32(new MySqlCommand("SELECT IFNULL(MAX(SortOrder), 0) + 1 FROM banners", con).ExecuteScalar());
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO banners (Title, Subtitle, ImageURL, LinkURL, SortOrder, IsActive) VALUES (@T, @S, @Img, @L, @SO, @A)", con);
                        cmd.Parameters.AddWithValue("@T", txtTitle.Text.Trim()); cmd.Parameters.AddWithValue("@S", txtSubtitle.Text);
                        cmd.Parameters.AddWithValue("@Img", imgFile); cmd.Parameters.AddWithValue("@L", txtLinkURL.Text);
                        cmd.Parameters.AddWithValue("@SO", sort); cmd.Parameters.AddWithValue("@A", ddlIsActive.SelectedValue);
                        cmd.ExecuteNonQuery(); Alert("Banner added!", "success");
                    }
                    else
                    {
                        string sql = imgFile != null ? "UPDATE banners SET Title=@T, Subtitle=@S, ImageURL=@Img, LinkURL=@L, IsActive=@A WHERE ID=@Id" : "UPDATE banners SET Title=@T, Subtitle=@S, LinkURL=@L, IsActive=@A WHERE ID=@Id";
                        MySqlCommand cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@T", txtTitle.Text.Trim()); cmd.Parameters.AddWithValue("@S", txtSubtitle.Text);
                        if (imgFile != null) cmd.Parameters.AddWithValue("@Img", imgFile);
                        cmd.Parameters.AddWithValue("@L", txtLinkURL.Text); cmd.Parameters.AddWithValue("@A", ddlIsActive.SelectedValue);
                        cmd.Parameters.AddWithValue("@Id", hfEditId.Value);
                        cmd.ExecuteNonQuery(); Alert("Banner updated!", "success");
                    }
                }
                ResetForm(); LoadBanners();
            }
            catch (Exception ex)
            {
                Alert("Error saving banner: " + ex.Message, "error");
            }
        }

        private void LoadBannerForEdit(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM banners WHERE ID = @Id", con);
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            hfEditId.Value = r["ID"].ToString(); txtTitle.Text = r["Title"].ToString(); txtSubtitle.Text = r["Subtitle"].ToString();
                            txtLinkURL.Text = r["LinkURL"].ToString(); ddlIsActive.SelectedValue = r["IsActive"].ToString();
                            lblFormTitle.Text = "Edit Banner"; lblImageRequired.Visible = false;
                            if (r["ImageURL"] != DBNull.Value) { pnlCurrentImage.Visible = true; lblCurrentImagePath.Text = r["ImageURL"].ToString(); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Alert("Error loading banner for edit: " + ex.Message, "error");
            }
        }

        protected void btnSaveOrder_Click(object sender, EventArgs e) { SaveOrder(hfNewOrder.Value, "UPDATE banners SET SortOrder=@order WHERE ID=@id"); LoadBanners(); }
        protected void btnCancel_Click(object sender, EventArgs e) { ResetForm(); }


        // ========================== CATEGORIES ==========================
        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(new MySqlCommand("SELECT * FROM collection_categories", con)).Fill(dt);
                    gvCategories.DataSource = dt; gvCategories.DataBind();

                    ddlColCategory.DataSource = dt;
                    ddlColCategory.DataTextField = "Name_Category"; ddlColCategory.DataValueField = "Id_Category";
                    ddlColCategory.DataBind(); ddlColCategory.Items.Insert(0, new ListItem("- Select Category -", ""));
                }
            }
            catch (Exception ex)
            {
                Alert("Error loading categories: " + ex.Message, "error");
            }
        }

        protected void btnAddCategory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text)) return;
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO collection_categories (Name_Category) VALUES (@N)", con);
                    cmd.Parameters.AddWithValue("@N", txtCategoryName.Text.Trim());
                    cmd.ExecuteNonQuery();
                }
                txtCategoryName.Text = ""; LoadCategories();
            }
            catch (Exception ex)
            {
                Alert("Error adding category: " + ex.Message, "error");
            }
        }

        protected void gvCategories_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gvCategories.DataKeys[e.RowIndex].Value);
            ExecuteDelete("DELETE FROM collection_categories WHERE Id_Category=@Id", id);
            LoadCategories(); LoadCollections();
        }


        // ========================== COLLECTIONS ==========================
        private void ResetColForm()
        {
            hfColEditId.Value = "0"; txtColTitle.Text = ""; txtColLink.Text = ""; ddlColCategory.SelectedIndex = 0;
            ddlColStatus.SelectedIndex = 0; lblColFormTitle.Text = "Add New Collection"; lblColImgReq.Visible = true;
            pnlColImg.Visible = false; lblColImgPath.Text = "";
        }

        private void LoadCollections()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT c.*, cat.Name_Category FROM collections c INNER JOIN collection_categories cat ON c.Id_Category = cat.Id_Category ORDER BY c.SortOrder ASC", con);
                    DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt);
                    gvCollections.DataSource = dt; gvCollections.DataBind();
                }
            }
            catch (Exception ex)
            {
                Alert("Error loading collections: " + ex.Message, "error");
            }
        }

        protected void gvCollections_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;
                Label lblStatus = (Label)e.Row.FindControl("lblColStatus");
                LinkButton btnToggle = (LinkButton)e.Row.FindControl("btnToggleCol");
                bool isActive = Convert.ToInt32(row["IsActive"]) == 1;

                if (lblStatus != null) lblStatus.Text = isActive ? "<span class='status-badge status-active'>Active</span>" : "<span class='status-badge status-inactive'>Inactive</span>";
                if (btnToggle != null)
                {
                    btnToggle.CssClass = isActive ? "action-icon toggle-icon" : "action-icon toggle-on";
                    btnToggle.Text = isActive ? "<i class='fas fa-eye-slash'></i>" : "<i class='fas fa-eye'></i>";
                }
            }
        }

        protected void gvCollections_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditCol") LoadColForEdit(id);
            else if (e.CommandName == "ToggleCol") { ExecuteToggle("UPDATE collections SET IsActive = 1 - IsActive WHERE Id_Collection = @Id", id); LoadCollections(); }
            else if (e.CommandName == "DeleteCol") { ExecuteDelete("DELETE FROM collections WHERE Id_Collection = @Id", id); ResetColForm(); LoadCollections(); }
        }

        protected void btnColSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtColTitle.Text) || string.IsNullOrWhiteSpace(ddlColCategory.SelectedValue) || string.IsNullOrWhiteSpace(txtColLink.Text))
            { Alert("Title, Category and Link are required.", "error"); return; }

            bool isEditing = Convert.ToInt32(hfColEditId.Value) > 0;
            string imgFile = HandleImageUpload(fileColImagen, "collections", isEditing);
            if (imgFile == "ERROR") return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    if (!isEditing)
                    {
                        int sort = Convert.ToInt32(new MySqlCommand("SELECT IFNULL(MAX(SortOrder), 0) + 1 FROM collections", con).ExecuteScalar());
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO collections (Id_Category, Title, ImageURL, LinkURL, SortOrder, IsActive) VALUES (@Cat, @T, @Img, @L, @SO, @A)", con);
                        cmd.Parameters.AddWithValue("@Cat", ddlColCategory.SelectedValue); cmd.Parameters.AddWithValue("@T", txtColTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Img", imgFile); cmd.Parameters.AddWithValue("@L", txtColLink.Text.Trim());
                        cmd.Parameters.AddWithValue("@SO", sort); cmd.Parameters.AddWithValue("@A", ddlColStatus.SelectedValue);
                        cmd.ExecuteNonQuery(); Alert("Collection added!", "success");
                    }
                    else
                    {
                        string sql = imgFile != null ? "UPDATE collections SET Id_Category=@Cat, Title=@T, ImageURL=@Img, LinkURL=@L, IsActive=@A WHERE Id_Collection=@Id" : "UPDATE collections SET Id_Category=@Cat, Title=@T, LinkURL=@L, IsActive=@A WHERE Id_Collection=@Id";
                        MySqlCommand cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Cat", ddlColCategory.SelectedValue); cmd.Parameters.AddWithValue("@T", txtColTitle.Text.Trim());
                        if (imgFile != null) cmd.Parameters.AddWithValue("@Img", imgFile);
                        cmd.Parameters.AddWithValue("@L", txtColLink.Text.Trim()); cmd.Parameters.AddWithValue("@A", ddlColStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@Id", hfColEditId.Value);
                        cmd.ExecuteNonQuery(); Alert("Collection updated!", "success");
                    }
                }
                ResetColForm(); LoadCollections();
            }
            catch (Exception ex)
            {
                Alert("Error saving collection: " + ex.Message, "error");
            }
        }

        private void LoadColForEdit(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM collections WHERE Id_Collection = @Id", con);
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            hfColEditId.Value = r["Id_Collection"].ToString(); txtColTitle.Text = r["Title"].ToString();
                            ddlColCategory.SelectedValue = r["Id_Category"].ToString(); txtColLink.Text = r["LinkURL"].ToString();
                            ddlColStatus.SelectedValue = r["IsActive"].ToString();
                            lblColFormTitle.Text = "Edit Collection"; lblColImgReq.Visible = false;
                            if (r["ImageURL"] != DBNull.Value) { pnlColImg.Visible = true; lblColImgPath.Text = r["ImageURL"].ToString(); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Alert("Error loading collection for edit: " + ex.Message, "error");
            }
        }

        protected void btnSaveColOrder_Click(object sender, EventArgs e) { SaveOrder(hfColOrder.Value, "UPDATE collections SET SortOrder=@order WHERE Id_Collection=@id"); LoadCollections(); }
        protected void btnColCancel_Click(object sender, EventArgs e) { ResetColForm(); }

        // ========================== AUTH CAROUSEL ==========================
        private void ResetAuthForm()
        {
            hfAuthEditId.Value = "0"; txtAuthQuote.Text = ""; txtAuthAuthorName.Text = ""; txtAuthAuthorRole.Text = "";
            ddlAuthIsActive.SelectedIndex = 0; lblAuthFormTitle.Text = "Add New Slide (Login/SignUp)"; lblAuthImageRequired.Visible = true;
            pnlAuthCurrentImage.Visible = false; lblAuthCurrentImagePath.Text = "";
        }

        private void LoadAuthCarousel()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT Id_Slide, ImageURL, QuoteText, AuthorName, AuthorRole, DisplayOrder, IsActive FROM auth_carousel ORDER BY DisplayOrder ASC;", con);
                    DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt);
                    gvAuthCarousel.DataSource = dt; gvAuthCarousel.DataBind();
                }
            }
            catch (Exception ex)
            {
                Alert("Error loading auth carousel: " + ex.Message, "error");
            }
        }

        protected void gvAuthCarousel_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;
                Label lblStatus = (Label)e.Row.FindControl("lblAuthStatus");
                LinkButton btnToggle = (LinkButton)e.Row.FindControl("btnToggleAuth");
                bool isActive = Convert.ToInt32(row["IsActive"]) == 1;

                if (lblStatus != null) lblStatus.Text = isActive ? "<span class='status-badge status-active'>Active</span>" : "<span class='status-badge status-inactive'>Inactive</span>";
                if (btnToggle != null)
                {
                    btnToggle.CssClass = isActive ? "action-icon toggle-icon" : "action-icon toggle-on";
                    btnToggle.Text = isActive ? "<i class='fas fa-eye-slash'></i>" : "<i class='fas fa-eye'></i>";
                }
            }
        }

        protected void gvAuthCarousel_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditAuth") LoadAuthForEdit(id);
            else if (e.CommandName == "ToggleAuth") { ExecuteToggle("UPDATE auth_carousel SET IsActive = 1 - IsActive WHERE Id_Slide = @Id", id); LoadAuthCarousel(); }
            else if (e.CommandName == "DeleteAuth") { ExecuteDelete("DELETE FROM auth_carousel WHERE Id_Slide = @Id", id); ResetAuthForm(); LoadAuthCarousel(); }
        }

        protected void btnSaveAuth_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAuthQuote.Text) || string.IsNullOrWhiteSpace(txtAuthAuthorName.Text))
            { Alert("Quote and Author Name are required.", "error"); return; }

            bool isEditing = Convert.ToInt32(hfAuthEditId.Value) > 0;
            string imgFile = HandleImageUpload(fileAuthImagen, "auth", isEditing); // Carpeta "auth"
            if (imgFile == "ERROR") return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    if (!isEditing)
                    {
                        int sort = Convert.ToInt32(new MySqlCommand("SELECT IFNULL(MAX(DisplayOrder), 0) + 1 FROM auth_carousel", con).ExecuteScalar());
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO auth_carousel (ImageURL, QuoteText, AuthorName, AuthorRole, DisplayOrder, IsActive) VALUES (@Img, @Q, @AN, @AR, @SO, @A)", con);
                        cmd.Parameters.AddWithValue("@Img", imgFile); cmd.Parameters.AddWithValue("@Q", txtAuthQuote.Text.Trim());
                        cmd.Parameters.AddWithValue("@AN", txtAuthAuthorName.Text.Trim()); cmd.Parameters.AddWithValue("@AR", txtAuthAuthorRole.Text.Trim());
                        cmd.Parameters.AddWithValue("@SO", sort); cmd.Parameters.AddWithValue("@A", ddlAuthIsActive.SelectedValue);
                        cmd.ExecuteNonQuery(); Alert("Slide added!", "success");
                    }
                    else
                    {
                        string sql = imgFile != null ? "UPDATE auth_carousel SET QuoteText=@Q, AuthorName=@AN, AuthorRole=@AR, ImageURL=@Img, IsActive=@A WHERE Id_Slide=@Id" : "UPDATE auth_carousel SET QuoteText=@Q, AuthorName=@AN, AuthorRole=@AR, IsActive=@A WHERE Id_Slide=@Id";
                        MySqlCommand cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Q", txtAuthQuote.Text.Trim()); cmd.Parameters.AddWithValue("@AN", txtAuthAuthorName.Text.Trim());
                        cmd.Parameters.AddWithValue("@AR", txtAuthAuthorRole.Text.Trim());
                        if (imgFile != null) cmd.Parameters.AddWithValue("@Img", imgFile);
                        cmd.Parameters.AddWithValue("@A", ddlAuthIsActive.SelectedValue); cmd.Parameters.AddWithValue("@Id", hfAuthEditId.Value);
                        cmd.ExecuteNonQuery(); Alert("Slide updated!", "success");
                    }
                }
                ResetAuthForm(); LoadAuthCarousel();
            }
            catch (Exception ex)
            {
                Alert("Error saving auth slide: " + ex.Message, "error");
            }
        }

        private void LoadAuthForEdit(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM auth_carousel WHERE Id_Slide = @Id", con);
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            hfAuthEditId.Value = r["Id_Slide"].ToString(); txtAuthQuote.Text = r["QuoteText"].ToString();
                            txtAuthAuthorName.Text = r["AuthorName"].ToString(); txtAuthAuthorRole.Text = r["AuthorRole"].ToString();
                            ddlAuthIsActive.SelectedValue = r["IsActive"].ToString();
                            lblAuthFormTitle.Text = "Edit Slide"; lblAuthImageRequired.Visible = false;
                            if (r["ImageURL"] != DBNull.Value) { pnlAuthCurrentImage.Visible = true; lblAuthCurrentImagePath.Text = r["ImageURL"].ToString(); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Alert("Error loading auth slide for edit: " + ex.Message, "error");
            }
        }

        protected void btnSaveAuthOrder_Click(object sender, EventArgs e) { SaveOrder(hfAuthOrder.Value, "UPDATE auth_carousel SET DisplayOrder=@order WHERE Id_Slide=@id"); LoadAuthCarousel(); }
        protected void btnAuthCancel_Click(object sender, EventArgs e) { ResetAuthForm(); }


        // ========================== HELPERS ==========================
        private string HandleImageUpload(FileUpload fileUpload, string folder, bool isEditing)
        {
            if (fileUpload.HasFile)
            {
                string ext = Path.GetExtension(fileUpload.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png" && ext != ".jpeg" && ext != ".webp") { Alert("Only .jpg, .png, .jpeg, and .webp allowed.", "error"); return "ERROR"; }
                if (fileUpload.PostedFile.ContentLength > 2 * 1024 * 1024) { Alert("Maximum image size is 2 MB.", "error"); return "ERROR"; }

                string path = Server.MapPath($"~/images/{folder}/");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid().ToString("N") + ext;
                fileUpload.SaveAs(Path.Combine(path, fileName));
                return fileName;
            }
            if (!isEditing) { Alert("Please upload an image.", "error"); return "ERROR"; }
            return null;
        }

        private void ExecuteToggle(string sql, int id) { try { using (MySqlConnection con = new MySqlConnection(connectionString)) { con.Open(); MySqlCommand cmd = new MySqlCommand(sql, con); cmd.Parameters.AddWithValue("@Id", id); cmd.ExecuteNonQuery(); } } catch (Exception ex) { Alert("Error: " + ex.Message, "error"); } }
        private void ExecuteDelete(string sql, int id) { try { using (MySqlConnection con = new MySqlConnection(connectionString)) { con.Open(); MySqlCommand cmd = new MySqlCommand(sql, con); cmd.Parameters.AddWithValue("@Id", id); cmd.ExecuteNonQuery(); } } catch (Exception ex) { Alert("Error: " + ex.Message, "error"); } }

        private void SaveOrder(string hfValue, string sql)
        {
            if (string.IsNullOrWhiteSpace(hfValue)) return;
            string[] ids = hfValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    using (MySqlTransaction tx = con.BeginTransaction())
                    {
                        for (int i = 0; i < ids.Length; i++) { MySqlCommand cmd = new MySqlCommand(sql, con, tx); cmd.Parameters.AddWithValue("@order", i + 1); cmd.Parameters.AddWithValue("@id", ids[i]); cmd.ExecuteNonQuery(); }
                        tx.Commit(); Alert("Order updated!", "success");
                    }
                }
            }
            catch (Exception ex) { Alert("Error saving order: " + ex.Message, "error"); }
        }

        private void Alert(string text, string icon) { alerta.Text = $"<script>Swal.fire('Notification', '{HttpUtility.HtmlEncode(text)}', '{icon}');</script>"; }

        // Navigation
        protected void btnManageProducts_Click(object sender, EventArgs e) { Response.Redirect("ManageProducts.aspx"); }
        protected void btnManageOrders_Click(object sender, EventArgs e) { Response.Redirect("ManageOrders.aspx"); }
        protected void btnManageOffers_Click(object sender, EventArgs e) { Response.Redirect("ManageOffers.aspx"); }
        protected void btncerrar_Click(object sender, EventArgs e) { Session.Clear(); Session.Abandon(); Response.Redirect("Login.aspx"); }
        protected void btnAddLeague_Click(object sender, EventArgs e) { Response.Redirect("AddLeague.aspx"); }
        protected void btnAddTeam_Click(object sender, EventArgs e) { Response.Redirect("AddTeam.aspx"); }
        protected void btnAddBrand_Click(object sender, EventArgs e) { Response.Redirect("AddBrand.aspx"); }
        protected void btnManageUsers_Click(object sender, EventArgs e) { Response.Redirect("ManageUsers.aspx"); }
        protected void btnAdminBanners_Click(object sender, EventArgs e) { Response.Redirect("AdminBanners.aspx"); }
        protected void btnSmtpSettings_Click(object sender, EventArgs e) { Response.Redirect("SmtpSettings.aspx"); }
        protected void btnStats_Click(object sender, EventArgs e) { Response.Redirect("AdminStats.aspx"); }
        protected void btnManageCoupons_Click(object sender, EventArgs e) { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e) { Response.Redirect("AdminAudit.aspx"); }
    }
}