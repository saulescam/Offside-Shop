using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

        // Umbral para considerar stock bajo (suma de todas las tallas)
        private const int LowStockThreshold = 5;

        // ══════════════════════════════════════════════════════════════════════
        //  Page_Load
        // ══════════════════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            // Control de caché para prevenir acceso mediante el botón 'Atrás' tras cerrar sesión
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            // Verificación de seguridad: Solo Owner (1) o Admin (2)
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

            // Verificación de permisos PBAC
            if (!Security.HasPermission(Session, "Perm_Products"))
            {
                Response.Redirect("Dashboard.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                PopulateFilterDropDowns();
                PopulateFormDropDowns();
                LoadProducts();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Populate DropDowns
        // ══════════════════════════════════════════════════════════════════════

        private void PopulateFilterDropDowns()
        {
            string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Marcas
                MySqlCommand cmdBrands = new MySqlCommand("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                DataTable dtBrands = new DataTable();
                new MySqlDataAdapter(cmdBrands).Fill(dtBrands);

                ddlFilterBrand.Items.Clear();
                ddlFilterBrand.Items.Add(new ListItem("-- All Brands --", "0"));
                foreach (DataRow row in dtBrands.Rows)
                    ddlFilterBrand.Items.Add(new ListItem(row["Name_Brand"].ToString(), row["Id_Brand"].ToString()));

                // Ligas
                MySqlCommand cmdLeagues = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dtLeagues = new DataTable();
                new MySqlDataAdapter(cmdLeagues).Fill(dtLeagues);

                ddlFilterLeague.Items.Clear();
                ddlFilterLeague.Items.Add(new ListItem("-- All Leagues --", "0"));
                foreach (DataRow row in dtLeagues.Rows)
                    ddlFilterLeague.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));

                // Tipos de Kit (TRADUCIDO SEGÚN IDIOMA)
                string sqlKits = @"
            SELECT Id_KitType, 
                   CASE 
                       WHEN @Lang = 'es' THEN COALESCE(Name_KitType_es, Name_KitType)
                       ELSE Name_KitType 
                   END AS Name_KitType 
            FROM kit_types 
            ORDER BY Id_KitType ASC;";

                MySqlCommand cmdKits = new MySqlCommand(sqlKits, con);
                cmdKits.Parameters.AddWithValue("@Lang", currentLang);
                DataTable dtKits = new DataTable();
                new MySqlDataAdapter(cmdKits).Fill(dtKits);

                ddlFilterKitType.Items.Clear();
                ddlFilterKitType.Items.Add(new ListItem(currentLang == "es" ? "-- Todos los Tipos --" : "-- All Kit Types --", "0"));
                foreach (DataRow row in dtKits.Rows)
                {
                    ddlFilterKitType.Items.Add(new ListItem(row["Name_KitType"].ToString(), row["Id_KitType"].ToString()));
                }

                // Nivel de Stock
                ddlFilterStock.Items.Clear();
                ddlFilterStock.Items.Add(new ListItem("-- All Stock Levels --", "0"));
                ddlFilterStock.Items.Add(new ListItem($"Low Stock Only (< {LowStockThreshold})", "1"));
            }

            LoadFilterTeamsByLeague(ddlFilterLeague.SelectedValue);
        }

        private void LoadFilterTeamsByLeague(string leagueId)
        {
            ddlFilterTeam.Items.Clear();
            ddlFilterTeam.Items.Add(new ListItem("-- All Teams --", "0"));

            if (string.IsNullOrEmpty(leagueId) || leagueId == "0") return;
            if (!int.TryParse(leagueId, out int idLeague)) return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @IdLeague ORDER BY Name_Team ASC;", con);
                cmd.Parameters.AddWithValue("@IdLeague", idLeague);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                foreach (DataRow row in dt.Rows)
                    ddlFilterTeam.Items.Add(new ListItem(row["Name_Team"].ToString(), row["Id_Team"].ToString()));
            }
        }

        protected void ddlFilterLeague_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFilterTeamsByLeague(ddlFilterLeague.SelectedValue);
            ddlFilterTeam.SelectedIndex = 0;
            LoadProducts();
        }

        private void PopulateFormDropDowns()
        {
            string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Form – Marcas
                MySqlCommand cmdBrands = new MySqlCommand("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                DataTable dtBrands = new DataTable();
                new MySqlDataAdapter(cmdBrands).Fill(dtBrands);

                ddlFormBrand.Items.Clear();
                ddlFormBrand.Items.Add(new ListItem("-- Select Brand --", "0"));
                foreach (DataRow row in dtBrands.Rows)
                    ddlFormBrand.Items.Add(new ListItem(row["Name_Brand"].ToString(), row["Id_Brand"].ToString()));

                // Form – Ligas
                MySqlCommand cmdLeagues = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dtLeagues = new DataTable();
                new MySqlDataAdapter(cmdLeagues).Fill(dtLeagues);

                ddlFormLeague.Items.Clear();
                ddlFormLeague.Items.Add(new ListItem("-- Select League --", "0"));
                foreach (DataRow row in dtLeagues.Rows)
                    ddlFormLeague.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));

                // Form – Tipos de Kit (TRADUCIDO SEGÚN IDIOMA)
                string sqlKits = @"
            SELECT Id_KitType, 
                   CASE 
                       WHEN @Lang = 'es' THEN COALESCE(Name_KitType_es, Name_KitType)
                       ELSE Name_KitType 
                   END AS Name_KitType 
            FROM kit_types 
            ORDER BY Id_KitType ASC;";

                MySqlCommand cmdKits = new MySqlCommand(sqlKits, con);
                cmdKits.Parameters.AddWithValue("@Lang", currentLang);
                DataTable dtKits = new DataTable();
                new MySqlDataAdapter(cmdKits).Fill(dtKits);

                ddlFormKitType.Items.Clear();
                ddlFormKitType.Items.Add(new ListItem(currentLang == "es" ? "-- Seleccionar Tipo de Kit --" : "-- Select Kit Type --", "0"));
                foreach (DataRow row in dtKits.Rows)
                {
                    ddlFormKitType.Items.Add(new ListItem(row["Name_KitType"].ToString(), row["Id_KitType"].ToString()));
                }
            }

            LoadFormTeamsByLeague(ddlFormLeague.SelectedValue);
        }

        private void LoadFormTeamsByLeague(string leagueId)
        {
            ddlFormTeam.Items.Clear();
            ddlFormTeam.Items.Add(new ListItem("-- Select Team --", "0"));

            if (string.IsNullOrEmpty(leagueId) || leagueId == "0") return;
            if (!int.TryParse(leagueId, out int idLeague)) return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @IdLeague ORDER BY Name_Team ASC;", con);
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
        //  LoadProducts – Consulta Multilingüe con Búsqueda y Filtros
        // ══════════════════════════════════════════════════════════════════════

        protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProducts.PageIndex = e.NewPageIndex;
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                int filterBrand = Convert.ToInt32(ddlFilterBrand.SelectedValue);
                int filterLeague = Convert.ToInt32(ddlFilterLeague.SelectedValue);
                int filterTeam = Convert.ToInt32(ddlFilterTeam.SelectedValue);
                int filterKitType = Convert.ToInt32(ddlFilterKitType.SelectedValue);
                int filterStock = Convert.ToInt32(ddlFilterStock.SelectedValue);
                string searchName = txtSearchName.Text.Trim();

                string sql = @"
            SELECT
                t.ID,
                t.Name,
                t.Price,
                t.Year,
                t.ImageURL,
                t.IsActive,
                tt.Name AS Name_ES,
                tt.Description AS Description_ES,
                b.Name_Brand  AS BrandName,
                tm.Name_Team  AS TeamName,
                CASE 
                    WHEN @Lang = 'es' THEN COALESCE(kt.Name_KitType_es, kt.Name_KitType)
                    ELSE kt.Name_KitType 
                END AS KitTypeName,
                COALESCE(sv.TotalStock, 0) AS TotalStock
            FROM tshirts t
            LEFT JOIN tshirt_translations tt ON t.ID = tt.Id_Tshirt AND tt.LanguageCode = 'es'
            INNER JOIN brands b ON t.Id_Brand = b.Id_Brand
            INNER JOIN teams tm ON t.Id_Team = tm.Id_Team
            INNER JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
            LEFT JOIN (
                SELECT Id_Tshirt, SUM(Stock) AS TotalStock
                FROM tshirt_variants
                GROUP BY Id_Tshirt
            ) sv ON t.ID = sv.Id_Tshirt
            WHERE 1=1";

                if (filterBrand > 0)
                    sql += " AND t.Id_Brand = @FilterBrand";

                if (filterLeague > 0)
                    sql += " AND tm.Id_League = @FilterLeague";

                if (filterTeam > 0)
                    sql += " AND t.Id_Team = @FilterTeam";

                if (filterKitType > 0)
                    sql += " AND t.Id_KitType = @FilterKitType";

                if (!string.IsNullOrEmpty(searchName))
                    sql += " AND (t.Name LIKE @SearchName OR tt.Name LIKE @SearchName)";

                if (filterStock == 1)
                    sql += " HAVING TotalStock < @LowStockThreshold";

                sql += " ORDER BY t.ID DESC;";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, con);

                    cmd.Parameters.AddWithValue("@Lang", currentLang);

                    if (filterBrand > 0) cmd.Parameters.AddWithValue("@FilterBrand", filterBrand);
                    if (filterLeague > 0) cmd.Parameters.AddWithValue("@FilterLeague", filterLeague);
                    if (filterTeam > 0) cmd.Parameters.AddWithValue("@FilterTeam", filterTeam);
                    if (filterKitType > 0) cmd.Parameters.AddWithValue("@FilterKitType", filterKitType);
                    if (!string.IsNullOrEmpty(searchName)) cmd.Parameters.AddWithValue("@SearchName", "%" + searchName + "%");
                    if (filterStock == 1) cmd.Parameters.AddWithValue("@LowStockThreshold", LowStockThreshold);

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

        protected void gvProducts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView row = (DataRowView)e.Row.DataItem;
            bool isActive = Convert.ToInt32(row["IsActive"]) == 1;

            Label lblStatus = (Label)e.Row.FindControl("lblStatus");
            if (lblStatus != null)
            {
                lblStatus.Text = isActive
                    ? "<span class=\"status-badge status-active\">Active</span>"
                    : "<span class=\"status-badge status-inactive\">Inactive</span>";
            }

            Button btnToggle = (Button)e.Row.FindControl("btnToggle");
            if (btnToggle != null)
                btnToggle.ToolTip = isActive ? "Deactivate (set Inactive)" : "Activate (set Active)";
        }

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

        protected void Filters_Changed(object sender, EventArgs e)
        {
            LoadProducts();
        }

        protected void lbClearFilters_Click(object sender, EventArgs e)
        {
            ddlFilterBrand.SelectedIndex = 0;
            ddlFilterLeague.SelectedIndex = 0;
            LoadFilterTeamsByLeague("0");
            ddlFilterTeam.SelectedIndex = 0;
            ddlFilterKitType.SelectedIndex = 0;
            ddlFilterStock.SelectedIndex = 0;
            txtSearchName.Text = "";
            LoadProducts();
        }

        protected void lbAddNew_Click(object sender, EventArgs e)
        {
            PopulateFormDropDowns();
            ClearFormPanel();
            lblFormTitle.Text = "Add New Shirt";
            pnlProductForm.Visible = true;
            LoadProducts();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Cargar Producto para Edición Multilingüe
        // ══════════════════════════════════════════════════════════════════════
        private void LoadProductForEdit(int productId)
        {
            try
            {
                PopulateFormDropDowns();

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string sql = @"
                        SELECT t.ID, t.Name, t.Price, t.Year, t.ImageURL, t.Description, t.IsCustomizable, 
                               t.Id_Brand, t.Id_Team, t.Id_KitType, tm.Id_League,
                               t.ImageURL2, t.ImageURL3, t.ImageURL4, t.ImageURL5,
                               tt.Name AS Name_ES, tt.Description AS Description_ES
                        FROM tshirts t
                        LEFT JOIN teams tm ON t.Id_Team = tm.Id_Team
                        LEFT JOIN tshirt_translations tt ON t.ID = tt.Id_Tshirt AND tt.LanguageCode = 'es'
                        WHERE t.ID = @Id;";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Id", productId);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    if (dt.Rows.Count == 0) return;

                    DataRow r = dt.Rows[0];

                    hfSelectedProductId.Value = r["ID"].ToString();
                    txtName.Text = r["Name"].ToString();
                    txtDescription.Text = r["Description"] == DBNull.Value ? "" : r["Description"].ToString();

                    txtName_ES.Text = r["Name_ES"] != DBNull.Value ? r["Name_ES"].ToString() : "";
                    txtDescription_ES.Text = r["Description_ES"] != DBNull.Value ? r["Description_ES"].ToString() : "";

                    txtPrice.Text = Convert.ToDecimal(r["Price"]).ToString("0.00");
                    txtYear.Text = Convert.ToInt32(r["Year"]).ToString();
                    chkIsCustomizable.Checked = r["IsCustomizable"] != DBNull.Value && Convert.ToBoolean(r["IsCustomizable"]);
                    lblCurrentImage.Text = r["ImageURL"] == DBNull.Value ? "" : "Current image: " + r["ImageURL"].ToString();

                    List<string> currentExtras = new List<string>();
                    for (int i = 2; i <= 5; i++)
                    {
                        string colName = "ImageURL" + i;
                        if (r[colName] != DBNull.Value && !string.IsNullOrEmpty(r[colName].ToString()))
                            currentExtras.Add(r[colName].ToString());
                    }
                    lblCurrentExtraImages.Text = currentExtras.Count > 0 ? "Current gallery: " + string.Join(", ", currentExtras) : "No gallery images uploaded.";

                    // Cargar Stocks por Talla
                    txtStockS.Text = "0"; txtStockM.Text = "0"; txtStockL.Text = "0"; txtStockXL.Text = "0"; txtStockXXL.Text = "0";
                    TextBox[] stockBoxes = { txtStockS, txtStockM, txtStockL, txtStockXL, txtStockXXL };

                    MySqlCommand varCmd = new MySqlCommand("SELECT Id_Size, Stock FROM tshirt_variants WHERE Id_Tshirt = @id;", con);
                    varCmd.Parameters.AddWithValue("@id", productId);

                    using (MySqlDataReader vReader = varCmd.ExecuteReader())
                    {
                        while (vReader.Read())
                        {
                            int sizeId = Convert.ToInt32(vReader["Id_Size"]);
                            int stock = Convert.ToInt32(vReader["Stock"]);
                            if (sizeId >= 1 && sizeId <= 5) stockBoxes[sizeId - 1].Text = stock.ToString();
                        }
                    }

                    // Seleccionar dropdowns
                    string brandId = r["Id_Brand"].ToString();
                    string teamId = r["Id_Team"].ToString();
                    string kitTypeId = r["Id_KitType"].ToString();
                    string leagueId = r["Id_League"] == DBNull.Value ? "0" : r["Id_League"].ToString();

                    if (ddlFormBrand.Items.FindByValue(brandId) != null) ddlFormBrand.SelectedValue = brandId;
                    if (ddlFormLeague.Items.FindByValue(leagueId) != null) ddlFormLeague.SelectedValue = leagueId;

                    LoadFormTeamsByLeague(leagueId);

                    if (ddlFormTeam.Items.FindByValue(teamId) != null) ddlFormTeam.SelectedValue = teamId;
                    if (ddlFormKitType.Items.FindByValue(kitTypeId) != null) ddlFormKitType.SelectedValue = kitTypeId;

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

        private void ToggleProductStatus(int productId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE tshirts SET IsActive = 1 - IsActive WHERE ID = @Id;", con);
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

        private void PermanentDeleteProduct(int productId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM tshirts WHERE ID = @Id;", con);
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
        //  Guardar Producto (Transacción Bilingüe)
        // ══════════════════════════════════════════════════════════════════════
        protected void btnSaveProduct_Click(object sender, EventArgs e)
        {
            // Validaciones Backend
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Shirt Name in English is required.', 'error');</script>";
                return;
            }
            if (string.IsNullOrWhiteSpace(txtName_ES.Text))
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'El Nombre en Español es requerido.', 'error');</script>";
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Price must be a positive number.', 'error');</script>";
                return;
            }

            if (!int.TryParse(txtYear.Text.Trim(), out int year) || txtYear.Text.Trim().Length != 4)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Year must be a valid 4-digit number.', 'error');</script>";
                return;
            }

            if (!int.TryParse(ddlFormBrand.SelectedValue, out int brandId) || brandId == 0 ||
                !int.TryParse(ddlFormLeague.SelectedValue, out int leagueId) || leagueId == 0 ||
                !int.TryParse(ddlFormTeam.SelectedValue, out int teamId) || teamId == 0 ||
                !int.TryParse(ddlFormKitType.SelectedValue, out int kitTypeId) || kitTypeId == 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Please select Brand, League, Team, and Kit Type.', 'error');</script>";
                return;
            }

            if (!int.TryParse(txtStockS.Text.Trim(), out int stockS) || stockS < 0 ||
                !int.TryParse(txtStockM.Text.Trim(), out int stockM) || stockM < 0 ||
                !int.TryParse(txtStockL.Text.Trim(), out int stockL) || stockL < 0 ||
                !int.TryParse(txtStockXL.Text.Trim(), out int stockXL) || stockXL < 0 ||
                !int.TryParse(txtStockXXL.Text.Trim(), out int stockXXL) || stockXXL < 0)
            {
                alerta.Text = "<script>Swal.fire('Validation Error', 'Stock values must be non-negative integers.', 'error');</script>";
                return;
            }

            int editId = 0;
            bool isEditing = !string.IsNullOrEmpty(hfSelectedProductId.Value) && int.TryParse(hfSelectedProductId.Value, out editId) && editId > 0;

            string descriptionEN = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
            string descriptionES = string.IsNullOrWhiteSpace(txtDescription_ES.Text) ? null : txtDescription_ES.Text.Trim();

            // Carga de Imagen Principal
            string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".webp" };
            string imageFileName = null;

            if (fileImagen.HasFile)
            {
                string ext = Path.GetExtension(fileImagen.FileName).ToLower();
                if (!allowedExtensions.Contains(ext) || fileImagen.PostedFile.ContentLength > 2 * 1024 * 1024)
                {
                    alerta.Text = "<script>Swal.fire('File Error', 'Main image must be .jpg, .jpeg, .png or .webp under 2MB.', 'error');</script>";
                    return;
                }

                string uploadFolder = Server.MapPath("~/images/camisetas/");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                imageFileName = Guid.NewGuid().ToString("N") + ext;
                fileImagen.SaveAs(Path.Combine(uploadFolder, imageFileName));
            }

            // Carga de Galería Extra
            string[] extraImageNames = new string[4];
            int uploadedCount = 0;
            bool hasNewExtraImages = false;

            if (fuExtraImages.PostedFiles != null && fuExtraImages.PostedFiles.Count > 0 && fuExtraImages.PostedFiles[0].ContentLength > 0)
            {
                foreach (HttpPostedFile postedFile in fuExtraImages.PostedFiles)
                {
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(postedFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(ext) || postedFile.ContentLength > 2 * 1024 * 1024)
                        {
                            alerta.Text = $"<script>Swal.fire('File Error', 'Gallery image {HttpUtility.JavaScriptStringEncode(postedFile.FileName)} is invalid or over 2MB.', 'error');</script>";
                            return;
                        }
                    }
                }

                foreach (HttpPostedFile postedFile in fuExtraImages.PostedFiles)
                {
                    if (uploadedCount >= 4) break;
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(postedFile.FileName).ToLower();
                        string uploadFolder = Server.MapPath("~/images/camisetas/");
                        string uniqueName = Guid.NewGuid().ToString("N") + ext;
                        postedFile.SaveAs(Path.Combine(uploadFolder, uniqueName));
                        extraImageNames[uploadedCount++] = uniqueName;
                        hasNewExtraImages = true;
                    }
                }
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    using (MySqlTransaction tx = con.BeginTransaction())
                    {
                        int targetProductId = editId;

                        if (!isEditing)
                        {
                            // INSERT en `tshirts`
                            string insertSql = @"INSERT INTO tshirts 
                                (Name, Price, Year, ImageURL, Description, Id_Brand, Id_Team, Id_KitType, IsActive, ImageURL2, ImageURL3, ImageURL4, ImageURL5, IsCustomizable)
                                VALUES 
                                (@Name, @Price, @Year, @ImageURL, @Description, @IdBrand, @IdTeam, @IdKitType, 1, @Img2, @Img3, @Img4, @Img5, @IsCustomizable);
                                SELECT LAST_INSERT_ID();";

                            MySqlCommand cmd = new MySqlCommand(insertSql, con, tx);
                            cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.Parameters.AddWithValue("@Year", year);
                            cmd.Parameters.AddWithValue("@ImageURL", (object)imageFileName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Description", (object)descriptionEN ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IdBrand", brandId);
                            cmd.Parameters.AddWithValue("@IdTeam", teamId);
                            cmd.Parameters.AddWithValue("@IdKitType", kitTypeId);
                            cmd.Parameters.AddWithValue("@Img2", (object)extraImageNames[0] ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Img3", (object)extraImageNames[1] ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Img4", (object)extraImageNames[2] ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Img5", (object)extraImageNames[3] ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsCustomizable", chkIsCustomizable.Checked ? 1 : 0);

                            targetProductId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        else
                        {
                            // UPDATE en `tshirts`
                            string updateSql = @"UPDATE tshirts SET
                                Name = @Name, Price = @Price, Year = @Year, Description = @Description,
                                Id_Brand = @IdBrand, Id_Team = @IdTeam, Id_KitType = @IdKitType,
                                IsCustomizable = @IsCustomizable";

                            if (imageFileName != null) updateSql += ", ImageURL = @ImageURL";
                            if (hasNewExtraImages) updateSql += ", ImageURL2 = @Img2, ImageURL3 = @Img3, ImageURL4 = @Img4, ImageURL5 = @Img5";
                            updateSql += " WHERE ID = @Id;";

                            MySqlCommand cmd = new MySqlCommand(updateSql, con, tx);
                            cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.Parameters.AddWithValue("@Year", year);
                            cmd.Parameters.AddWithValue("@Description", (object)descriptionEN ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IdBrand", brandId);
                            cmd.Parameters.AddWithValue("@IdTeam", teamId);
                            cmd.Parameters.AddWithValue("@IdKitType", kitTypeId);
                            cmd.Parameters.AddWithValue("@IsCustomizable", chkIsCustomizable.Checked ? 1 : 0);
                            cmd.Parameters.AddWithValue("@Id", targetProductId);

                            if (imageFileName != null) cmd.Parameters.AddWithValue("@ImageURL", imageFileName);
                            if (hasNewExtraImages)
                            {
                                cmd.Parameters.AddWithValue("@Img2", (object)extraImageNames[0] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Img3", (object)extraImageNames[1] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Img4", (object)extraImageNames[2] ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Img5", (object)extraImageNames[3] ?? DBNull.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }

                        // Guardar o Actualizar la traducción en `tshirt_translations`
                        string transSql = @"
                            INSERT INTO tshirt_translations (Id_Tshirt, LanguageCode, Name, Description)
                            VALUES (@Id, 'es', @NameES, @DescES)
                            ON DUPLICATE KEY UPDATE Name = @NameES, Description = @DescES;";

                        MySqlCommand transCmd = new MySqlCommand(transSql, con, tx);
                        transCmd.Parameters.AddWithValue("@Id", targetProductId);
                        transCmd.Parameters.AddWithValue("@NameES", txtName_ES.Text.Trim());
                        transCmd.Parameters.AddWithValue("@DescES", (object)descriptionES ?? DBNull.Value);
                        transCmd.ExecuteNonQuery();

                        // Guardar o Actualizar variaciones de Tallas
                        TextBox[] stockBoxes = { txtStockS, txtStockM, txtStockL, txtStockXL, txtStockXXL };
                        for (int i = 0; i < stockBoxes.Length; i++)
                        {
                            int.TryParse(stockBoxes[i].Text.Trim(), out int qty);
                            int sizeId = SizeIds[i];

                            string varSql = @"
                                INSERT INTO tshirt_variants (Id_Tshirt, Id_Size, Stock)
                                VALUES (@Id, @SizeId, @Stock)
                                ON DUPLICATE KEY UPDATE Stock = @Stock;";

                            MySqlCommand varCmd = new MySqlCommand(varSql, con, tx);
                            varCmd.Parameters.AddWithValue("@Id", targetProductId);
                            varCmd.Parameters.AddWithValue("@SizeId", sizeId);
                            varCmd.Parameters.AddWithValue("@Stock", qty);
                            varCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        alerta.Text = "<script>Swal.fire('Success', 'Product saved successfully with translations!', 'success');</script>";
                    }
                }

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
            if (string.IsNullOrWhiteSpace(txtName.Text) || ddlFormBrand.SelectedIndex <= 0 || ddlFormTeam.SelectedIndex <= 0)
            {
                alerta.Text = "<script>Swal.fire('Missing Information', 'Please select a Team, Brand, and Name first.', 'warning');</script>";
                return;
            }

            try
            {
                string productName = txtName.Text.Trim();
                string brand = ddlFormBrand.SelectedItem.Text;
                string team = ddlFormTeam.SelectedItem.Text;
                string year = txtYear.Text.Trim();
                string kitType = ddlFormKitType.SelectedIndex > 0 ? ddlFormKitType.SelectedItem.Text : "Jersey";

                string prompt = $@"Write a highly engaging, historical e-commerce product description for the following football jersey.
                           - Product Name: {productName}
                           - Team: {team}
                           - Brand: {brand}
                           - Year/Season: {year}
                           - Kit Type: {kitType}";

                string systemInstruction = "You are an expert copywriter for OFFSIDESHOP. Keep it under 100 words. YOU MUST ANSWER STRICTLY IN ENGLISH.";

                GeminiService gemini = new GeminiService();
                string generatedDescription = await gemini.CallGeminiAsync(prompt, "gemini-1.5-flash", systemInstruction);

                txtDescription.Text = generatedDescription.Trim();
                alerta.Text = "<script>Swal.fire({toast:true,position:'top-end',icon:'success',title:'Description generated with AI!',showConfirmButton:false,timer:2500});</script>";
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('AI Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        protected void btnCancelForm_Click(object sender, EventArgs e)
        {
            ClearFormPanel();
            pnlProductForm.Visible = false;
            LoadProducts();
        }

        private void ClearFormPanel()
        {
            hfSelectedProductId.Value = "";
            txtName.Text = "";
            txtName_ES.Text = "";
            txtPrice.Text = "";
            txtYear.Text = "";
            txtDescription.Text = "";
            txtDescription_ES.Text = "";
            ddlFormBrand.SelectedIndex = 0;
            ddlFormLeague.SelectedIndex = 0;
            ddlFormKitType.SelectedIndex = 0;
            LoadFormTeamsByLeague("0");
            lblFormTitle.Text = "Add New Shirt";

            txtStockS.Text = "0";
            txtStockM.Text = "0";
            txtStockL.Text = "0";
            txtStockXL.Text = "0";
            txtStockXXL.Text = "0";

            lblCurrentImage.Text = "";
            lblCurrentExtraImages.Text = "";
            chkIsCustomizable.Checked = false;
        }
        // Método para forzar la cultura en esta página según la Sesión
        protected override void InitializeCulture()
        {
            if (Session["Language"] != null)
            {
                string lang = Session["Language"].ToString();
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
            }
            base.InitializeCulture();
        }

        // Evento del botón EN / ES
        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }
        // Navegación
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