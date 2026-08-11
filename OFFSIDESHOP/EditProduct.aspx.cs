using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class EditProduct : System.Web.UI.Page
    {
        private string connectionString = "server=127.0.0.1; database=offsideshop; Uid=root; pwd=Info2026/*-";

        // Size ID map: index 0=S(1), 1=M(2), 2=L(3), 3=XL(4), 4=XXL(5)
        private static readonly int[] SizeIds = { 1, 2, 3, 4, 5 };

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

            // Role guard
            if (Session["UserRole"] == null)
                Response.Redirect("Login.aspx");

            int role = Convert.ToInt32(Session["UserRole"]);
            if (role != 1 && role != 2)
                Response.Redirect("Login.aspx");

            if (!IsPostBack)
            {
                // Check if a product ID was passed via QueryString (e.g., from GridView link)
                if (!string.IsNullOrEmpty(Request.QueryString["id"]))
                {
                    txtID.Text = Request.QueryString["id"];
                    LoadProductById(txtID.Text);
                }
                else
                {
                    LoadLeagues();
                    LoadBrands();
                    LoadTeamsByLeague(ddlLeague.SelectedValue);
                }
                LoadProducts();
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Populate Dropdowns
        // ──────────────────────────────────────────────────────────────
        private void LoadLeagues()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                ddlLeague.Items.Clear();
                ddlLeague.Items.Add(new ListItem("-- Select League --", ""));
                foreach (DataRow row in dt.Rows)
                    ddlLeague.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));
            }
        }

        private void LoadBrands()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                ddlMarca.Items.Clear();
                ddlMarca.Items.Add(new ListItem("-- Select Brand --", ""));
                foreach (DataRow row in dt.Rows)
                    ddlMarca.Items.Add(new ListItem(row["Name_Brand"].ToString(), row["Id_Brand"].ToString()));
            }
        }

        private void LoadTeamsByLeague(string leagueId)
        {
            ddlEquipo.Items.Clear();
            ddlEquipo.Items.Add(new ListItem("-- Select Team --", ""));

            if (string.IsNullOrEmpty(leagueId)) return;
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
                    ddlEquipo.Items.Add(new ListItem(row["Name_Team"].ToString(), row["Id_Team"].ToString()));
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  AutoPostBack: League changed → reload teams
        // ──────────────────────────────────────────────────────────────
        protected void ddlLeague_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTeamsByLeague(ddlLeague.SelectedValue);
        }

        // ──────────────────────────────────────────────────────────────
        //  Load product into form fields (including size variants)
        // ──────────────────────────────────────────────────────────────
        private void LoadProductById(string rawId)
        {
            if (!int.TryParse(rawId, out int id))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_IdNumeric", "error");
                LoadLeagues(); LoadBrands(); LoadTeamsByLeague("");
                return;
            }

            // Always load dropdowns first
            LoadLeagues();
            LoadBrands();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // 1) Load main tshirt row
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT t.ID, t.Name, t.Id_Brand, t.Id_Team, t.Year, t.Id_KitType,
                             t.Price, t.ImageURL, t.Description,
                             tm.Id_League
                      FROM tshirts t
                      LEFT JOIN teams tm ON t.Id_Team = tm.Id_Team
                      WHERE t.ID = @id;", con);
                cmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        TriggerAlert("Alert_ErrorTitle", "Alert_Delete_NotFound", "error");
                        reader.Close();
                        LoadTeamsByLeague("");
                        return;
                    }

                    txtNombre.Text    = reader["Name"].ToString();
                    txtAnio.Text      = Convert.ToInt32(reader["Year"]).ToString(); // YEAR → int
                    txtPrecio.Text    = Convert.ToDecimal(reader["Price"]).ToString("0.00");
                    txtDescripcion.Text = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString();
                    lblCurrentImage.Text = reader["ImageURL"] == DBNull.Value
                        ? "" : $"Current image: {reader["ImageURL"]}";

                    // Pre-select League (needed to load teams)
                    string leagueId = reader["Id_League"] == DBNull.Value ? "" : reader["Id_League"].ToString();
                    string brandId  = reader["Id_Brand"]  == DBNull.Value ? "" : reader["Id_Brand"].ToString();
                    string teamId   = reader["Id_Team"]   == DBNull.Value ? "" : reader["Id_Team"].ToString();
                    string kitTypeId = reader["Id_KitType"] == DBNull.Value ? "" : reader["Id_KitType"].ToString();

                    reader.Close();

                    // Load teams for this league before trying to set SelectedValue
                    LoadTeamsByLeague(leagueId);

                    // Set dropdown selections
                    if (ddlLeague.Items.FindByValue(leagueId) != null)
                        ddlLeague.SelectedValue = leagueId;

                    if (ddlMarca.Items.FindByValue(brandId) != null)
                        ddlMarca.SelectedValue = brandId;

                    if (ddlEquipo.Items.FindByValue(teamId) != null)
                        ddlEquipo.SelectedValue = teamId;

                    if (ddlTipo.Items.FindByValue(kitTypeId) != null)
                        ddlTipo.SelectedValue = kitTypeId;
                }

                // 2) Load size variants
                TextBox[] stockBoxes = { txtStockS, txtStockM, txtStockL, txtStockXL, txtStockXXL };
                // Reset all to 0
                foreach (var tb in stockBoxes) tb.Text = "0";

                MySqlCommand varCmd = new MySqlCommand(
                    "SELECT Id_Size, Stock FROM tshirt_variants WHERE Id_Tshirt = @id;", con);
                varCmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader vReader = varCmd.ExecuteReader())
                {
                    while (vReader.Read())
                    {
                        int sizeId = Convert.ToInt32(vReader["Id_Size"]);
                        int stock  = Convert.ToInt32(vReader["Stock"]);
                        // sizeId 1-5 maps to index 0-4
                        if (sizeId >= 1 && sizeId <= 5)
                            stockBoxes[sizeId - 1].Text = stock.ToString();
                    }
                }
            }

            TriggerAlert("Alert_SuccessTitle", "Alert_Edit_LoadedText", "success");
        }

        // ──────────────────────────────────────────────────────────────
        //  "Load Product" button
        // ──────────────────────────────────────────────────────────────
        protected void btnSeleccionar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_EnterId", "error");
                return;
            }
            LoadProductById(txtID.Text.Trim());
            LoadProducts();
        }

        // ──────────────────────────────────────────────────────────────
        //  "Save Changes" button
        // ──────────────────────────────────────────────────────────────
        protected void btnEditar_Click(object sender, EventArgs e)
        {
            // ── Validation ────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_FirstLoad", "error");
                return;
            }

            if (!int.TryParse(txtID.Text.Trim(), out int productId))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_InvalidId", "error");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrEmpty(ddlMarca.SelectedValue) ||
                string.IsNullOrEmpty(ddlEquipo.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtAnio.Text) ||
                string.IsNullOrEmpty(ddlTipo.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Edit_FillRequired", "error");
                return;
            }

            if (!int.TryParse(txtAnio.Text.Trim(), out int year) || txtAnio.Text.Trim().Length != 4)
            {
                TriggerAlert("Alert_Edit_YearTitle", "Alert_Edit_YearText", "error");
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal precio) || precio <= 0)
            {
                TriggerAlert("Alert_Edit_PriceTitle", "Alert_Edit_PriceText", "error");
                return;
            }

            if (!int.TryParse(ddlTipo.SelectedValue,   out int kitTypeId) ||
                !int.TryParse(ddlMarca.SelectedValue,  out int brandId) ||
                !int.TryParse(ddlEquipo.SelectedValue, out int teamId))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Edit_InvalidSelections", "error");
                return;
            }

            // ── Image upload (optional) ───────────────────────────────
            string newImageFileName = null;
            if (fileImagen.HasFile)
            {
                string ext = Path.GetExtension(fileImagen.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png")
                {
                    TriggerAlert("Alert_Edit_InvalidFileTitle", "Alert_Edit_InvalidFileText", "error");
                    return;
                }
                if (fileImagen.PostedFile.ContentLength > 2 * 1024 * 1024)
                {
                    TriggerAlert("Alert_Edit_FileTooLargeTitle", "Alert_Edit_FileTooLargeText", "error");
                    return;
                }

                string uploadFolder = Server.MapPath("~/images/camisetas/");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                newImageFileName = Guid.NewGuid().ToString("N") + ext;
                fileImagen.SaveAs(Path.Combine(uploadFolder, newImageFileName));
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // ── UPDATE tshirts ────────────────────────────────
                    string updateQuery;
                    MySqlCommand cmd;

                    if (newImageFileName != null)
                    {
                        // Replace image URL
                        updateQuery = @"UPDATE tshirts SET
                            Name         = @Name,
                            Id_Brand     = @IdBrand,
                            Id_Team      = @IdTeam,
                            Year         = @Year,
                            Id_KitType  = @IdKitType,
                            Price        = @Price,
                            ImageURL     = @ImageURL,
                            Description  = @Description
                            WHERE ID = @ID;";
                        cmd = new MySqlCommand(updateQuery, con);
                        cmd.Parameters.AddWithValue("@ImageURL", newImageFileName);
                    }
                    else
                    {
                        // Keep existing image
                        updateQuery = @"UPDATE tshirts SET
                            Name         = @Name,
                            Id_Brand     = @IdBrand,
                            Id_Team      = @IdTeam,
                            Year         = @Year,
                            Id_KitType  = @IdKitType,
                            Price        = @Price,
                            Description  = @Description
                            WHERE ID = @ID;";
                        cmd = new MySqlCommand(updateQuery, con);
                    }

                    cmd.Parameters.AddWithValue("@Name",        HttpUtility.HtmlEncode(txtNombre.Text.Trim()));
                    cmd.Parameters.AddWithValue("@IdBrand",     brandId);
                    cmd.Parameters.AddWithValue("@IdTeam",      teamId);
                    cmd.Parameters.AddWithValue("@Year",        year);
                    cmd.Parameters.AddWithValue("@IdKitType",   kitTypeId);
                    cmd.Parameters.AddWithValue("@Price",       precio);
                    cmd.Parameters.AddWithValue("@Description",
                        string.IsNullOrWhiteSpace(txtDescripcion.Text)
                            ? (object)DBNull.Value
                            : HttpUtility.HtmlEncode(txtDescripcion.Text.Trim()));
                    cmd.Parameters.AddWithValue("@ID", productId);
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
                        checkCmd.Parameters.AddWithValue("@t", productId);
                        checkCmd.Parameters.AddWithValue("@s", sizeId);
                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (exists > 0)
                        {
                            // UPDATE
                            MySqlCommand upd = new MySqlCommand(
                                "UPDATE tshirt_variants SET Stock = @Stock WHERE Id_Tshirt = @t AND Id_Size = @s;", con);
                            upd.Parameters.AddWithValue("@Stock", qty);
                            upd.Parameters.AddWithValue("@t", productId);
                            upd.Parameters.AddWithValue("@s", sizeId);
                            upd.ExecuteNonQuery();
                        }
                        else if (qty > 0)
                        {
                            // INSERT only if stock > 0
                            MySqlCommand ins = new MySqlCommand(
                                "INSERT INTO tshirt_variants (Id_Tshirt, Id_Size, Stock) VALUES (@t, @s, @Stock);", con);
                            ins.Parameters.AddWithValue("@t",     productId);
                            ins.Parameters.AddWithValue("@s",     sizeId);
                            ins.Parameters.AddWithValue("@Stock", qty);
                            ins.ExecuteNonQuery();
                        }
                        // If doesn't exist and qty == 0 → nothing to do
                    }
                }

                TriggerAlert("Alert_SuccessTitle", "Alert_Edit_SuccessText", "success");
                LoadProducts();

                // Clear form
                txtID.Text = txtNombre.Text = txtAnio.Text = txtPrecio.Text = txtDescripcion.Text = "";
                txtStockS.Text = txtStockM.Text = txtStockL.Text = txtStockXL.Text = txtStockXXL.Text = "0";
                ddlTipo.SelectedIndex = 0;
                lblCurrentImage.Text = "";
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Load products into GridView
        // ──────────────────────────────────────────────────────────────
        private void LoadProducts()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT t.ID, t.Name, b.Name_Brand AS Brand, 
                     l.Name_League AS League, tm.Name_Team AS Team, 
                     t.Year, kt.Name_KitType AS Type, t.Price                 
              FROM tshirts t                 
              LEFT JOIN brands    b  ON t.Id_Brand   = b.Id_Brand                 
              LEFT JOIN teams     tm ON t.Id_Team    = tm.Id_Team                 
              LEFT JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
              LEFT JOIN leagues   l  ON tm.Id_League = l.Id_League 
              ORDER BY tm.Id_League ASC, t.ID DESC;", con);

                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                gvdlista.DataSource = dt;
                gvdlista.DataBind();
            }
        }

        protected void btnInicio_Click(object sender, EventArgs e)
        {
            Response.Redirect("Dashboard.aspx");
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

        private void TriggerAlert(string titleKey, string messageKey, string iconType)
        {
            alerta.Text = AlertHelper.GetAlertScript(this, titleKey, messageKey, iconType);
        }
    }
}
