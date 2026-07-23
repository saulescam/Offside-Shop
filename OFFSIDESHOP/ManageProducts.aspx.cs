using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class ManageProducts : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        // Size ID map: 1=S, 2=M, 3=L, 4=XL, 5=XXL
        private static readonly int[] SizeIds = { 1, 2, 3, 4, 5 };

        // Threshold used to flag a product as "Low Stock" (sum of all size variants)
        private const int LowStockThreshold = 5;

        // ══════════════════════════════════════════════════════════════════════
        //  Page_Load
        // ══════════════════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            // Cache control – prevent back-button access after logout
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            // Security guard: only Owner (1) or Admin (2) may access this page
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
                // Populate filter dropdowns
                PopulateFilterDropDowns();

                // Populate form dropdowns
                PopulateFormDropDowns();

                // Load the product grid
                LoadProducts();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Populate Helpers
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Fills the filter DropDownLists (Brand, League, KitType, Stock Level) with
        /// a leading "All" option and the data from the master tables.
        /// Teams start with "-- All Teams --" and are populated once the user picks a league.
        /// </summary>
        private void PopulateFilterDropDowns()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Filter – Brands
                MySqlCommand cmdBrands = new MySqlCommand(
                    "SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                DataTable dtBrands = new DataTable();
                new MySqlDataAdapter(cmdBrands).Fill(dtBrands);

                ddlFilterBrand.Items.Clear();
                ddlFilterBrand.Items.Add(new ListItem("-- All Brands --", "0"));
                foreach (DataRow row in dtBrands.Rows)
                    ddlFilterBrand.Items.Add(new ListItem(row["Name_Brand"].ToString(), row["Id_Brand"].ToString()));

                // Filter – Leagues
                MySqlCommand cmdLeagues = new MySqlCommand(
                    "SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dtLeagues = new DataTable();
                new MySqlDataAdapter(cmdLeagues).Fill(dtLeagues);

                ddlFilterLeague.Items.Clear();
                ddlFilterLeague.Items.Add(new ListItem("-- All Leagues --", "0"));
                foreach (DataRow row in dtLeagues.Rows)
                    ddlFilterLeague.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));

                // Filter – Kit Types (Ordered by ID)
                MySqlCommand cmdKits = new MySqlCommand(
                    "SELECT Id_KitType, Name_KitType FROM kit_types ORDER BY Id_KitType ASC;", con);
                DataTable dtKits = new DataTable();
                new MySqlDataAdapter(cmdKits).Fill(dtKits);

                ddlFilterKitType.Items.Clear();
                ddlFilterKitType.Items.Add(new ListItem("-- All Kit Types --", "0"));
                foreach (DataRow row in dtKits.Rows)
                {
                    ddlFilterKitType.Items.Add(new ListItem(row["Name_KitType"].ToString(), row["Id_KitType"].ToString()));
                }

                // Filter – Stock Level (static options, no DB lookup needed)
                ddlFilterStock.Items.Clear();
                ddlFilterStock.Items.Add(new ListItem("-- All Stock Levels --", "0"));
                ddlFilterStock.Items.Add(new ListItem($"Low Stock Only (< {LowStockThreshold})", "1"));
            }

            // Init team list based on current league selection (empty = all teams placeholder)
            LoadFilterTeamsByLeague(ddlFilterLeague.SelectedValue);
        }

        /// <summary>
        /// Fills ddlFilterTeam with teams belonging to the given league.
        /// When leagueId is 0 / empty it shows just the "All Teams" placeholder.
        /// </summary>
        private void LoadFilterTeamsByLeague(string leagueId)
        {
            ddlFilterTeam.Items.Clear();
            ddlFilterTeam.Items.Add(new ListItem("-- All Teams --", "0"));

            if (string.IsNullOrEmpty(leagueId) || leagueId == "0") return;

            if (!int.TryParse(leagueId, out int idLeague)) return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @IdLeague ORDER BY Name_Team ASC;", con);
                cmd.Parameters.AddWithValue("@IdLeague", idLeague);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                foreach (DataRow row in dt.Rows)
                    ddlFilterTeam.Items.Add(new ListItem(row["Name_Team"].ToString(), row["Id_Team"].ToString()));
            }
        }

        /// <summary>
        /// When the user changes the League filter, reload teams and refresh the grid.
        /// </summary>
        protected void ddlFilterLeague_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reload teams for the selected league; reset team selection to "All"
            LoadFilterTeamsByLeague(ddlFilterLeague.SelectedValue);
            ddlFilterTeam.SelectedIndex = 0;
            LoadProducts();
        }

        /// <summary>
        /// Fills the form DropDownLists (Brand, League, KitType) that appear
        /// inside pnlProductForm for Add / Edit operations.
        /// </summary>
        private void PopulateFormDropDowns()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Form – Brands
                MySqlCommand cmdBrands = new MySqlCommand(
                    "SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                DataTable dtBrands = new DataTable();
                new MySqlDataAdapter(cmdBrands).Fill(dtBrands);

                ddlFormBrand.Items.Clear();
                ddlFormBrand.Items.Add(new ListItem("-- Select Brand --", "0"));
                foreach (DataRow row in dtBrands.Rows)
                    ddlFormBrand.Items.Add(new ListItem(row["Name_Brand"].ToString(), row["Id_Brand"].ToString()));

                // Form – Leagues
                MySqlCommand cmdLeagues = new MySqlCommand(
                    "SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dtLeagues = new DataTable();
                new MySqlDataAdapter(cmdLeagues).Fill(dtLeagues);

                ddlFormLeague.Items.Clear();
                ddlFormLeague.Items.Add(new ListItem("-- Select League --", "0"));
                foreach (DataRow row in dtLeagues.Rows)
                    ddlFormLeague.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));
                // Form – Kit Types (Ordered by ID)
                MySqlCommand cmdKits = new MySqlCommand(
                    "SELECT Id_KitType, Name_KitType FROM kit_types ORDER BY Id_KitType ASC;", con);
                DataTable dtKits = new DataTable();
                new MySqlDataAdapter(cmdKits).Fill(dtKits);

                ddlFormKitType.Items.Clear();
                ddlFormKitType.Items.Add(new ListItem("-- Select Kit Type --", "0"));
                foreach (DataRow row in dtKits.Rows)
                {
                    ddlFormKitType.Items.Add(new ListItem(row["Name_KitType"].ToString(), row["Id_KitType"].ToString()));
                }
            }

            // Initially load teams based on the selected league
            LoadFormTeamsByLeague(ddlFormLeague.SelectedValue);
        }

        /// <summary>
        /// Helper to load teams belonging to a specific league into ddlFormTeam.
        /// </summary>
        private void LoadFormTeamsByLeague(string leagueId)
        {
            ddlFormTeam.Items.Clear();
            ddlFormTeam.Items.Add(new ListItem("-- Select Team --", "0"));

            if (string.IsNullOrEmpty(leagueId) || leagueId == "0") return;

            if (!int.TryParse(leagueId, out int idLeague)) return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @IdLeague ORDER BY Name_Team ASC;", con);
                cmd.Parameters.AddWithValue("@IdLeague", idLeague);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                foreach (DataRow row in dt.Rows)
                    ddlFormTeam.Items.Add(new ListItem(row["Name_Team"].ToString(), row["Id_Team"].ToString()));
            }
        }

        protected void ddlFormLeague_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFormTeamsByLeague(ddlFormLeague.SelectedValue);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LoadProducts – Dynamic filtered SELECT with INNER JOINs
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds a parameterized SELECT query that returns ALL shirts (active and
        /// inactive) and applies optional WHERE clauses based on the filter controls.
        /// Also computes the TotalStock (sum of every size variant) for each shirt
        /// so the grid can flag low-stock products and the Stock Level filter can work.
        /// </summary>
        /// 

        protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // 1. Cambias el índice de la página actual al que seleccionó el usuario
            gvProducts.PageIndex = e.NewPageIndex;

            // 2. Vuelves a consultar la base de datos y enlazar los datos
            LoadProducts();
        }
        private void LoadProducts()
        {
            try
            {
                // Read current filter values
                int filterBrand = Convert.ToInt32(ddlFilterBrand.SelectedValue);
                int filterLeague = Convert.ToInt32(ddlFilterLeague.SelectedValue);
                int filterTeam = Convert.ToInt32(ddlFilterTeam.SelectedValue);
                int filterKitType = Convert.ToInt32(ddlFilterKitType.SelectedValue);
                int filterStock = Convert.ToInt32(ddlFilterStock.SelectedValue); // 0 = All, 1 = Low Stock Only
                string searchName = txtSearchName.Text.Trim();

                // Build base query with INNER JOINs to master tables.
                // LEFT JOIN to a per-shirt stock subquery gives us TotalStock,
                // used both for the low-stock warning icon and the stock filter.
                string sql =
                    @"SELECT
                        t.ID,
                        t.Name,
                        t.Price,
                        t.Year,
                        t.ImageURL,
                        t.IsActive,
                        b.Name_Brand  AS BrandName,
                        tm.Name_Team  AS TeamName,
                        kt.Name_KitType AS KitTypeName,
                        COALESCE(sv.TotalStock, 0) AS TotalStock
                    FROM tshirts t
                    INNER JOIN brands    b  ON t.Id_Brand   = b.Id_Brand
                    INNER JOIN teams     tm ON t.Id_Team    = tm.Id_Team
                    INNER JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
                    LEFT JOIN (
                        SELECT Id_Tshirt, SUM(Stock) AS TotalStock
                        FROM tshirt_variants
                        GROUP BY Id_Tshirt
                    ) sv ON t.ID = sv.Id_Tshirt
                    WHERE 1=1";

                // Append dynamic filter conditions
                if (filterBrand > 0)
                    sql += " AND t.Id_Brand = @FilterBrand";

                if (filterLeague > 0)
                    sql += " AND tm.Id_League = @FilterLeague";

                if (filterTeam > 0)
                    sql += " AND t.Id_Team = @FilterTeam";

                if (filterKitType > 0)
                    sql += " AND t.Id_KitType = @FilterKitType";

                if (!string.IsNullOrEmpty(searchName))
                    sql += " AND t.Name LIKE @SearchName";

                // Low-stock filter applies to the computed TotalStock column,
                // so it must be expressed as HAVING rather than WHERE.
                if (filterStock == 1)
                    sql += " HAVING TotalStock < @LowStockThreshold";

                sql += " ORDER BY t.ID DESC;";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, con);

                    // Bind parameters only when the filter is active
                    if (filterBrand > 0)
                        cmd.Parameters.AddWithValue("@FilterBrand", filterBrand);

                    if (filterLeague > 0)
                        cmd.Parameters.AddWithValue("@FilterLeague", filterLeague);

                    if (filterTeam > 0)
                        cmd.Parameters.AddWithValue("@FilterTeam", filterTeam);

                    if (filterKitType > 0)
                        cmd.Parameters.AddWithValue("@FilterKitType", filterKitType);

                    if (!string.IsNullOrEmpty(searchName))
                        cmd.Parameters.AddWithValue("@SearchName", "%" + searchName + "%");

                    if (filterStock == 1)
                        cmd.Parameters.AddWithValue("@LowStockThreshold", LowStockThreshold);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    gvProducts.DataSource = dt;
                    gvProducts.DataBind();
                }
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Database Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GridView – RowDataBound  (renders status badge, low-stock alert & toggle icon)
        // ══════════════════════════════════════════════════════════════════════
        protected void gvProducts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView row = (DataRowView)e.Row.DataItem;
            bool isActive = Convert.ToInt32(row["IsActive"]) == 1;
            int totalStock = row["TotalStock"] != DBNull.Value ? Convert.ToInt32(row["TotalStock"]) : 0;

            // Render colored status badge
            Label lblStatus = (Label)e.Row.FindControl("lblStatus");
            if (lblStatus != null)
            {
                lblStatus.Text = isActive
                    ? "<span class=\"status-badge status-active\">Active</span>"
                    : "<span class=\"status-badge status-inactive\">Inactive</span>";
            }

            // Render a small warning icon to the left of the shirt name when stock is low
            Label lblLowStockIcon = (Label)e.Row.FindControl("lblLowStockIcon");
            if (lblLowStockIcon != null)
            {
                lblLowStockIcon.Text = totalStock < LowStockThreshold
                    ? $"<i class=\"fas fa-exclamation-triangle low-stock-icon\" title=\"Low stock: only {totalStock} unit(s) left\"></i> "
                    : "";
            }

            // Update toggle button tooltip to reflect current action
            Button btnToggle = (Button)e.Row.FindControl("btnToggle");
            if (btnToggle != null)
                btnToggle.ToolTip = isActive ? "Deactivate (set Inactive)" : "Activate (set Active)";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GridView – RowCommand  (dispatches Edit / Toggle / Delete actions)
        // ══════════════════════════════════════════════════════════════════════
        protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int productId)) return;

            switch (e.CommandName)
            {
                case "EditProduct":
                    LoadProductForEdit(productId);
                    break;

                case "ToggleStatus":
                    ToggleProductStatus(productId);
                    LoadProducts();
                    break;

                case "PermanentDelete":
                    PermanentDeleteProduct(productId);
                    LoadProducts();
                    break;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Filters Event Handler
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unified handler for all filter DropDownLists (including Stock Level) and the search TextBox.
        /// Re-invokes LoadProducts() each time any filter value changes.
        /// </summary>
        protected void Filters_Changed(object sender, EventArgs e)
        {
            LoadProducts();
        }

        /// <summary>
        /// Resets all filters to their default "All" state and reloads the grid.
        /// </summary>
        protected void lbClearFilters_Click(object sender, EventArgs e)
        {
            ddlFilterBrand.SelectedIndex = 0;
            ddlFilterLeague.SelectedIndex = 0;
            LoadFilterTeamsByLeague("0");     // reset team list to empty placeholder
            ddlFilterTeam.SelectedIndex = 0;
            ddlFilterKitType.SelectedIndex = 0;
            ddlFilterStock.SelectedIndex = 0;
            txtSearchName.Text = "";
            LoadProducts();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Add New – opens the form panel in creation mode
        // ══════════════════════════════════════════════════════════════════════
        protected void lbAddNew_Click(object sender, EventArgs e)
        {
            // Ensure form dropdowns are populated before showing the panel
            PopulateFormDropDowns();
            ClearFormPanel();
            lblFormTitle.Text = "Add New Shirt";
            pnlProductForm.Visible = true;
            LoadProducts();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CRUD – Load product into the edit form
        // ══════════════════════════════════════════════════════════════════════
        private void LoadProductForEdit(int productId)
        {
            try
            {
                PopulateFormDropDowns();

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        @"SELECT t.ID, t.Name, t.Price, t.Year, t.ImageURL, t.Description, t.IsCustomizable, 
                                 t.Id_Brand, t.Id_Team, t.Id_KitType, tm.Id_League,
                                 t.ImageURL2, t.ImageURL3, t.ImageURL4, t.ImageURL5
                          FROM tshirts t
                          LEFT JOIN teams tm ON t.Id_Team = tm.Id_Team
                          WHERE t.ID = @Id;", con);
                    cmd.Parameters.AddWithValue("@Id", productId);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    if (dt.Rows.Count == 0) return;

                    DataRow r = dt.Rows[0];

                    // Populate text fields
                    hfSelectedProductId.Value = r["ID"].ToString();
                    txtName.Text = r["Name"].ToString();
                    txtPrice.Text = Convert.ToDecimal(r["Price"]).ToString("0.00");
                    txtYear.Text = Convert.ToInt32(r["Year"]).ToString();
                    if (r["IsCustomizable"] != DBNull.Value)
                        chkIsCustomizable.Checked = Convert.ToBoolean(r["IsCustomizable"]);
                    else
                        chkIsCustomizable.Checked = false;
                    lblCurrentImage.Text = r["ImageURL"] == DBNull.Value ? "" : "Current image: " + r["ImageURL"].ToString();
                    txtDescription.Text = r["Description"] == DBNull.Value ? "" : r["Description"].ToString();

                    // Display current gallery files
                    System.Collections.Generic.List<string> currentExtras = new System.Collections.Generic.List<string>();
                    for (int i = 2; i <= 5; i++)
                    {
                        string colName = "ImageURL" + i;
                        if (r[colName] != DBNull.Value && !string.IsNullOrEmpty(r[colName].ToString()))
                        {
                            currentExtras.Add(r[colName].ToString());
                        }
                    }
                    if (currentExtras.Count > 0)
                    {
                        lblCurrentExtraImages.Text = "Current gallery: " + string.Join(", ", currentExtras);
                    }
                    else
                    {
                        lblCurrentExtraImages.Text = "No gallery images uploaded.";
                    }

                    // Reset stocks to 0 first
                    txtStockS.Text = "0";
                    txtStockM.Text = "0";
                    txtStockL.Text = "0";
                    txtStockXL.Text = "0";
                    txtStockXXL.Text = "0";

                    TextBox[] stockBoxes = { txtStockS, txtStockM, txtStockL, txtStockXL, txtStockXXL };
                    MySqlCommand varCmd = new MySqlCommand(
                        "SELECT Id_Size, Stock FROM tshirt_variants WHERE Id_Tshirt = @id;", con);
                    varCmd.Parameters.AddWithValue("@id", productId);

                    using (MySqlDataReader vReader = varCmd.ExecuteReader())
                    {
                        while (vReader.Read())
                        {
                            int sizeId = Convert.ToInt32(vReader["Id_Size"]);
                            int stock = Convert.ToInt32(vReader["Stock"]);
                            if (sizeId >= 1 && sizeId <= 5)
                            {
                                stockBoxes[sizeId - 1].Text = stock.ToString();
                            }
                        }
                    }

                    // Set DropDownList selected values to match the stored FK values
                    string brandId = r["Id_Brand"].ToString();
                    string teamId = r["Id_Team"].ToString();
                    string kitTypeId = r["Id_KitType"].ToString();
                    string leagueId = r["Id_League"] == DBNull.Value ? "0" : r["Id_League"].ToString();

                    if (ddlFormBrand.Items.FindByValue(brandId) != null)
                        ddlFormBrand.SelectedValue = brandId;

                    if (ddlFormLeague.Items.FindByValue(leagueId) != null)
                        ddlFormLeague.SelectedValue = leagueId;

                    // Load teams for the selected league first
                    LoadFormTeamsByLeague(leagueId);

                    if (ddlFormTeam.Items.FindByValue(teamId) != null)
                        ddlFormTeam.SelectedValue = teamId;

                    if (ddlFormKitType.Items.FindByValue(kitTypeId) != null)
                        ddlFormKitType.SelectedValue = kitTypeId;

                    // Update form title to show which product is being edited
                    lblFormTitle.Text = $"Edit Product #{productId}";
                    pnlProductForm.Visible = true;
                }

                LoadProducts();
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CRUD – ToggleStatus  (1 - IsActive trick, same as banners)
        // ══════════════════════════════════════════════════════════════════════
        private void ToggleProductStatus(int productId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "UPDATE tshirts SET IsActive = 1 - IsActive WHERE ID = @Id;", con);
                    cmd.Parameters.AddWithValue("@Id", productId);
                    cmd.ExecuteNonQuery();
                }
                alerta.Text = "<script>Swal.fire({toast:true,position:'top-end',icon:'success',title:'Status toggled',showConfirmButton:false,timer:1800});</script>";
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CRUD – PermanentDelete  (real SQL DELETE, not a soft delete)
        // ══════════════════════════════════════════════════════════════════════
        private void PermanentDeleteProduct(int productId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "DELETE FROM tshirts WHERE ID = @Id;", con);
                    cmd.Parameters.AddWithValue("@Id", productId);
                    cmd.ExecuteNonQuery();
                }
                alerta.Text = "<script>Swal.fire('Deleted', 'The shirt has been permanently removed.', 'success');</script>";
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  [Save Product] Button – INSERT or UPDATE depending on hfSelectedProductId
        // ══════════════════════════════════════════════════════════════════════
        protected void btnSaveProduct_Click(object sender, EventArgs e)
        {
            // ── Input validation ──────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Shirt Name is required.', 'error');</script>";
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal price) || price <= 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Price must be a positive number (e.g. 89.99).', 'error');</script>";
                return;
            }

            if (!int.TryParse(txtYear.Text.Trim(), out int year) || txtYear.Text.Trim().Length != 4)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Year must be a valid 4-digit number (e.g. 2024).', 'error');</script>";
                return;
            }

            if (!int.TryParse(ddlFormBrand.SelectedValue, out int brandId) || brandId == 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Please select a Brand.', 'error');</script>";
                return;
            }

            if (!int.TryParse(ddlFormLeague.SelectedValue, out int leagueId) || leagueId == 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Please select a League.', 'error');</script>";
                return;
            }

            if (!int.TryParse(ddlFormTeam.SelectedValue, out int teamId) || teamId == 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Please select a Team.', 'error');</script>";
                return;
            }

            if (!int.TryParse(ddlFormKitType.SelectedValue, out int kitTypeId) || kitTypeId == 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Please select a Kit Type.', 'error');</script>";
                return;
            }

            // Validate stock sizes represent non-negative integers
            if (!int.TryParse(txtStockS.Text.Trim(), out int stockS) || stockS < 0 ||
                !int.TryParse(txtStockM.Text.Trim(), out int stockM) || stockM < 0 ||
                !int.TryParse(txtStockL.Text.Trim(), out int stockL) || stockL < 0 ||
                !int.TryParse(txtStockXL.Text.Trim(), out int stockXL) || stockXL < 0 ||
                !int.TryParse(txtStockXXL.Text.Trim(), out int stockXXL) || stockXXL < 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Stock values must be non-negative integers.', 'error');</script>";
                return;
            }

            // Determine if this is an INSERT or an UPDATE
            int editId = 0;
            bool isEditing = !string.IsNullOrEmpty(hfSelectedProductId.Value) &&
                             int.TryParse(hfSelectedProductId.Value, out editId) &&
                             editId > 0;

            // Prepare description field
            string description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();

            // ── Image upload ─────────────────────────────────────────
            string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".webp" };
            string imageFileName = null;

            if (fileImagen.HasFile)
            {
                string ext = Path.GetExtension(fileImagen.FileName).ToLower();

                // 1. Validar extensión de la imagen principal
                if (!allowedExtensions.Contains(ext))
                {
                    alerta.Text = "<script>Swal.fire('Invalid File', 'Only .jpg, .jpeg, .png and .webp images are allowed for the main image.', 'error');</script>";
                    return; // Cortar ejecución
                }

                // 2. Validar peso de la imagen principal (2MB máx)
                if (fileImagen.PostedFile.ContentLength > 2 * 1024 * 1024)
                {
                    alerta.Text = "<script>Swal.fire('File Too Large', 'Maximum main image size is 2 MB.', 'error');</script>";
                    return; // Cortar ejecución
                }

                try
                {
                    // Save to ~/images/camisetas/
                    string uploadFolder = Server.MapPath("~/images/camisetas/");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    imageFileName = Guid.NewGuid().ToString("N") + ext;
                    fileImagen.SaveAs(Path.Combine(uploadFolder, imageFileName));
                }
                catch (Exception ex)
                {
                    alerta.Text = $"<script>Swal.fire('Upload Error', 'Could not save the uploaded main file: {HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
                    return;
                }
            }

            // ── Extra gallery images upload ──────────────────────────
            string[] extraImageNames = new string[4];
            int uploadedCount = 0;
            bool hasNewExtraImages = false;

            // Verificamos si se subieron archivos y si el primer archivo tiene contenido real
            if (fuExtraImages.PostedFiles != null && fuExtraImages.PostedFiles.Count > 0 && fuExtraImages.PostedFiles[0].ContentLength > 0)
            {
                // FASE 1: VALIDACIÓN ESTRICTA (Si hay un solo archivo malo, abortamos TODO)
                foreach (HttpPostedFile postedFile in fuExtraImages.PostedFiles)
                {
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(postedFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(ext))
                        {
                            alerta.Text = $"<script>Swal.fire('Invalid Gallery File', 'The file {HttpUtility.JavaScriptStringEncode(postedFile.FileName)} is invalid. Only .jpg, .jpeg, .png and .webp are allowed.', 'error');</script>";
                            return; // Cortar ejecución, no se guarda el producto ni las imágenes
                        }

                        if (postedFile.ContentLength > 2 * 1024 * 1024)
                        {
                            alerta.Text = $"<script>Swal.fire('File Too Large', 'The gallery file {HttpUtility.JavaScriptStringEncode(postedFile.FileName)} exceeds the 2 MB limit.', 'error');</script>";
                            return; // Cortar ejecución
                        }
                    }
                }

                // FASE 2: GUARDADO (Solo se ejecuta si todas las imágenes pasaron la prueba)
                foreach (HttpPostedFile postedFile in fuExtraImages.PostedFiles)
                {
                    if (uploadedCount >= 4)
                        break;

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(postedFile.FileName).ToLower();
                        try
                        {
                            string uploadFolder = Server.MapPath("~/images/camisetas/");
                            if (!Directory.Exists(uploadFolder))
                                Directory.CreateDirectory(uploadFolder);

                            string uniqueName = Guid.NewGuid().ToString("N") + ext;
                            postedFile.SaveAs(Path.Combine(uploadFolder, uniqueName));
                            extraImageNames[uploadedCount] = uniqueName;
                            uploadedCount++;
                            hasNewExtraImages = true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error saving gallery image: " + ex.Message);
                        }
                    }
                }
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    if (!isEditing)
                    {
                        // ── INSERT new product ──────────────────────────────
                        MySqlCommand cmd = new MySqlCommand(
                            @"INSERT INTO tshirts
                                (Name, Price, Year, ImageURL, Description, Id_Brand, Id_Team, Id_KitType, IsActive,
                                 ImageURL2, ImageURL3, ImageURL4, ImageURL5, IsCustomizable)
                              VALUES
                                (@Name, @Price, @Year, @ImageURL, @Description, @IdBrand, @IdTeam, @IdKitType, 1,
                                 @ImageURL2, @ImageURL3, @ImageURL4, @ImageURL5, @IsCustomizable);
                              SELECT LAST_INSERT_ID();",
                            con);

                        cmd.Parameters.AddWithValue("@Name", HttpUtility.HtmlEncode(txtName.Text.Trim()));
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@ImageURL", (object)imageFileName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdBrand", brandId);
                        cmd.Parameters.AddWithValue("@IdTeam", teamId);
                        cmd.Parameters.AddWithValue("@IdKitType", kitTypeId);
                        cmd.Parameters.AddWithValue("@ImageURL2", (object)extraImageNames[0] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImageURL3", (object)extraImageNames[1] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImageURL4", (object)extraImageNames[2] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImageURL5", (object)extraImageNames[3] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsCustomizable", chkIsCustomizable.Checked ? 1 : 0);

                        long newId = Convert.ToInt64(cmd.ExecuteScalar());

                        // ── Insert size variants ──────────────────────────
                        TextBox[] stockBoxes = { txtStockS, txtStockM, txtStockL, txtStockXL, txtStockXXL };
                        for (int i = 0; i < stockBoxes.Length; i++)
                        {
                            int qty = 0;
                            int.TryParse(stockBoxes[i].Text.Trim(), out qty);
                            if (qty <= 0) continue; // skip sizes with 0 stock

                            MySqlCommand varCmd = new MySqlCommand(
                                @"INSERT INTO tshirt_variants (Id_Tshirt, Id_Size, Stock)
                                  VALUES (@IdTshirt, @IdSize, @Stock);", con);
                            varCmd.Parameters.AddWithValue("@IdTshirt", newId);
                            varCmd.Parameters.AddWithValue("@IdSize", SizeIds[i]);
                            varCmd.Parameters.AddWithValue("@Stock", qty);
                            varCmd.ExecuteNonQuery();
                        }

                        alerta.Text = "<script>Swal.fire('Success', 'Shirt added successfully!', 'success');</script>";
                    }
                    else
                    {
                        // ── UPDATE existing product ─────────────────────────
                        string updateQuery = @"UPDATE tshirts SET
                            Name        = @Name,
                            Price       = @Price,
                            Year        = @Year,
                            Description = @Description,
                            Id_Brand    = @IdBrand,
                            Id_Team     = @IdTeam,
                            Id_KitType  = @IdKitType,
                            IsCustomizable = @IsCustomizable";

                        if (imageFileName != null)
                        {
                            updateQuery += ", ImageURL = @ImageURL";
                        }

                        if (hasNewExtraImages)
                        {
                            updateQuery += ", ImageURL2 = @ImageURL2, ImageURL3 = @ImageURL3, ImageURL4 = @ImageURL4, ImageURL5 = @ImageURL5";
                        }

                        updateQuery += " WHERE ID = @Id;";

                        MySqlCommand cmd = new MySqlCommand(updateQuery, con);

                        cmd.Parameters.AddWithValue("@Name", HttpUtility.HtmlEncode(txtName.Text.Trim()));
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdBrand", brandId);
                        cmd.Parameters.AddWithValue("@IdTeam", teamId);
                        cmd.Parameters.AddWithValue("@IdKitType", kitTypeId);
                        cmd.Parameters.AddWithValue("@Id", editId);
                        cmd.Parameters.AddWithValue("@IsCustomizable", chkIsCustomizable.Checked ? 1 : 0);

                        if (imageFileName != null)
                        {
                            cmd.Parameters.AddWithValue("@ImageURL", imageFileName);
                        }

                        if (hasNewExtraImages)
                        {
                            cmd.Parameters.AddWithValue("@ImageURL2", (object)extraImageNames[0] ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ImageURL3", (object)extraImageNames[1] ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ImageURL4", (object)extraImageNames[2] ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ImageURL5", (object)extraImageNames[3] ?? DBNull.Value);
                        }

                        cmd.ExecuteNonQuery();

                        // ── UPSERT size variants ──────────────────────────
                        TextBox[] stockBoxes = { txtStockS, txtStockM, txtStockL, txtStockXL, txtStockXXL };
                        for (int i = 0; i < stockBoxes.Length; i++)
                        {
                            int.TryParse(stockBoxes[i].Text.Trim(), out int qty);
                            int sizeId = SizeIds[i];

                            // Check if variant row exists
                            MySqlCommand checkCmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM tshirt_variants WHERE Id_Tshirt = @t AND Id_Size = @s;", con);
                            checkCmd.Parameters.AddWithValue("@t", editId);
                            checkCmd.Parameters.AddWithValue("@s", sizeId);
                            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (exists > 0)
                            {
                                // UPDATE
                                MySqlCommand upd = new MySqlCommand(
                                    "UPDATE tshirt_variants SET Stock = @Stock WHERE Id_Tshirt = @t AND Id_Size = @s;", con);
                                upd.Parameters.AddWithValue("@Stock", qty);
                                upd.Parameters.AddWithValue("@t", editId);
                                upd.Parameters.AddWithValue("@s", sizeId);
                                upd.ExecuteNonQuery();
                            }
                            else if (qty > 0)
                            {
                                // INSERT only if stock > 0
                                MySqlCommand ins = new MySqlCommand(
                                    "INSERT INTO tshirt_variants (Id_Tshirt, Id_Size, Stock) VALUES (@t, @s, @Stock);", con);
                                ins.Parameters.AddWithValue("@t", editId);
                                ins.Parameters.AddWithValue("@s", sizeId);
                                ins.Parameters.AddWithValue("@Stock", qty);
                                ins.ExecuteNonQuery();
                            }
                        }

                        alerta.Text = "<script>Swal.fire('Success', 'Shirt updated successfully!', 'success');</script>";
                    }
                }

                // Clear the form panel, hide it, and refresh the grid
                ClearFormPanel();
                pnlProductForm.Visible = false;
                LoadProducts();
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Database Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }
        protected async void btnGenerateDescription_Click(object sender, EventArgs e)
        {
            // 1. Validate that we have enough context to generate a good description
            if (string.IsNullOrWhiteSpace(txtName.Text) || ddlFormBrand.SelectedIndex <= 0 || ddlFormTeam.SelectedIndex <= 0)
            {
                alerta.Text = "<script>Swal.fire('Missing Information', 'Please select at least a Team, Brand, and provide a Shirt Name before generating a description.', 'warning');</script>";
                return;
            }

            try
            {
                // 2. Gather context variables
                string productName = txtName.Text.Trim();
                string brand = ddlFormBrand.SelectedItem.Text;
                string team = ddlFormTeam.SelectedItem.Text;
                string year = txtYear.Text.Trim();
                string kitType = ddlFormKitType.SelectedIndex > 0 ? ddlFormKitType.SelectedItem.Text : "Jersey";

                // 3. Construct the prompt
                string prompt = $@"Write a highly engaging, historical e-commerce product description for the following football jersey. 
                           Focus on its historical significance, the team's legacy during that era, or the design features.
                           - Product Name: {productName}
                           - Team: {team}
                           - Brand: {brand}
                           - Year/Season: {year}
                           - Kit Type: {kitType}";

                // 4. Enforce instructions (Including the strict English requirement)
                string systemInstruction = "You are an expert football historian and elite copywriter for OFFSIDESHOP, an e-commerce platform for football jerseys. Your task is to write captivating, emotional, and sales-oriented product descriptions. Keep it under 120 words. Format with short paragraphs. YOU MUST ANSWER STRICTLY IN ENGLISH.";

                // 5. Call the API using your GeminiService
                GeminiService gemini = new GeminiService();

                // FIX: Usamos 'gemini-1.5-flash' que es el modelo universal y más rápido,
                // ideal para tareas de redacción corta en e-commerce.
                string generatedDescription = await gemini.CallGeminiAsync(prompt, "gemini-3.5-flash", systemInstruction);

                // 6. Populate the text box and show a success toast
                txtDescription.Text = generatedDescription.Trim();
                alerta.Text = "<script>Swal.fire({toast:true,position:'top-end',icon:'success',title:'Description generated successfully!',showConfirmButton:false,timer:2500});</script>";
            }
            catch (Exception ex)
            {
                // Handle API or network errors smoothly
                alerta.Text = $"<script>Swal.fire('AI Generation Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  [Cancel] Button – hide the form panel and clear inputs
        // ══════════════════════════════════════════════════════════════════════
        protected void btnCancelForm_Click(object sender, EventArgs e)
        {
            ClearFormPanel();
            pnlProductForm.Visible = false;
            LoadProducts();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Helper – resets all form panel fields to their empty defaults
        // ══════════════════════════════════════════════════════════════════════
        private void ClearFormPanel()
        {
            hfSelectedProductId.Value = "";
            txtName.Text = "";
            txtPrice.Text = "";
            txtYear.Text = "";
            txtDescription.Text = "";
            ddlFormBrand.SelectedIndex = 0;
            ddlFormLeague.SelectedIndex = 0;
            ddlFormKitType.SelectedIndex = 0;
            LoadFormTeamsByLeague("0");
            lblFormTitle.Text = "Add New Shirt";

            // Clear size stocks
            txtStockS.Text = "0";
            txtStockM.Text = "0";
            txtStockL.Text = "0";
            txtStockXL.Text = "0";
            txtStockXXL.Text = "0";

            lblCurrentImage.Text = "";
            lblCurrentExtraImages.Text = "";
            chkIsCustomizable.Checked = false;
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
        { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e)
        { Response.Redirect("AdminAudit.aspx"); }
    }
}