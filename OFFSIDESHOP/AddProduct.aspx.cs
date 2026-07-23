using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class AddProduct : System.Web.UI.Page
    {
        private string connectionString = "server=127.0.0.1; database=offsideshop; Uid=root; pwd=Info2026/*-";

        // Size ID map: 1=S, 2=M, 3=L, 4=XL, 5=XXL
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

            // Role-based access control: only Admin (2) or Owner (1)
            if (Session["UserRole"] == null)
                Response.Redirect("Login.aspx");

            int role = Convert.ToInt32(Session["UserRole"]);
            if (role != 1 && role != 2)
                Response.Redirect("Login.aspx");

            if (!IsPostBack)
            {
                LoadLeagues();
                LoadBrands();
                // Load teams for the first league selected
                LoadTeamsByLeague(ddlLeague.SelectedValue);
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
        //  Load products for the GridView
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

        // ──────────────────────────────────────────────────────────────
        //  Submit: Add Product
        // ──────────────────────────────────────────────────────────────
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // ── Validation ────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrEmpty(ddlLeague.SelectedValue) ||
                string.IsNullOrEmpty(ddlMarca.SelectedValue) ||
                string.IsNullOrEmpty(ddlEquipo.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtAnio.Text) ||
                string.IsNullOrEmpty(ddlTipo.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text))
            {
                alerta.Text = "<script>Swal.fire('Error', 'Please fill all required fields (*).', 'error');</script>";
                return;
            }

            // Year must be a valid 4-digit integer
            if (!int.TryParse(txtAnio.Text.Trim(), out int year) || txtAnio.Text.Trim().Length != 4)
            {
                alerta.Text = "<script>Swal.fire('Invalid Year', 'Year must be exactly 4 digits (e.g. 2024).', 'error');</script>";
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal precio) || precio <= 0)
            {
                alerta.Text = "<script>Swal.fire('Invalid Price', 'Price must be greater than zero.', 'error');</script>";
                return;
            }

            // Kit type ID
            if (!int.TryParse(ddlTipo.SelectedValue, out int kitTypeId))
            {
                alerta.Text = "<script>Swal.fire('Error', 'Please select a valid kit type.', 'error');</script>";
                return;
            }

            if (!int.TryParse(ddlMarca.SelectedValue,  out int brandId) ||
                !int.TryParse(ddlEquipo.SelectedValue, out int teamId))
            {
                alerta.Text = "<script>Swal.fire('Error', 'Please select a valid brand and team.', 'error');</script>";
                return;
            }

            // ── Image upload ─────────────────────────────────────────
            string imageFileName = null;
            if (fileImagen.HasFile)
            {
                string ext = Path.GetExtension(fileImagen.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png")
                {
                    alerta.Text = "<script>Swal.fire('Invalid File', 'Only .jpg and .png images are allowed.', 'error');</script>";
                    return;
                }
                if (fileImagen.PostedFile.ContentLength > 2 * 1024 * 1024)
                {
                    alerta.Text = "<script>Swal.fire('File Too Large', 'Maximum image size is 2 MB.', 'error');</script>";
                    return;
                }

                // Save to ~/images/camisetas/
                string uploadFolder = Server.MapPath("~/images/camisetas/");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                imageFileName = Guid.NewGuid().ToString("N") + ext;
                fileImagen.SaveAs(Path.Combine(uploadFolder, imageFileName));
            }

            // ── Database insert ──────────────────────────────────────
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Insert main tshirt record
                    string insertQuery = @"INSERT INTO tshirts
                        (Name, Id_Brand, Id_Team, Year, Id_KitType, Price, ImageURL, Description)
                        VALUES
                        (@Name, @IdBrand, @IdTeam, @Year, @IdKitType, @Price, @ImageURL, @Description);
                        SELECT LAST_INSERT_ID();";

                    MySqlCommand cmd = new MySqlCommand(insertQuery, con);
                    cmd.Parameters.AddWithValue("@Name",      HttpUtility.HtmlEncode(txtNombre.Text.Trim()));
                    cmd.Parameters.AddWithValue("@IdBrand",   brandId);
                    cmd.Parameters.AddWithValue("@IdTeam",    teamId);
                    cmd.Parameters.AddWithValue("@Year",      year);
                    cmd.Parameters.AddWithValue("@IdKitType", kitTypeId);
                    cmd.Parameters.AddWithValue("@Price",     precio);
                    cmd.Parameters.AddWithValue("@ImageURL",
                        imageFileName == null ? (object)DBNull.Value : imageFileName);
                    cmd.Parameters.AddWithValue("@Description",
                        string.IsNullOrWhiteSpace(txtDescripcion.Text)
                            ? (object)DBNull.Value
                            : HttpUtility.HtmlEncode(txtDescripcion.Text.Trim()));

                    long newId = Convert.ToInt64(cmd.ExecuteScalar());

                    // ── Insert size variants ──────────────────────────
                    // Size IDs: 1=S, 2=M, 3=L, 4=XL, 5=XXL
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
                        varCmd.Parameters.AddWithValue("@IdSize",   SizeIds[i]);
                        varCmd.Parameters.AddWithValue("@Stock",    qty);
                        varCmd.ExecuteNonQuery();
                    }
                }

                alerta.Text = "<script>Swal.fire('Success', 'Product added successfully!', 'success').then(() => { window.location.href = 'AddProduct.aspx'; });</script>";

                // Clear form fields
                txtNombre.Text = txtAnio.Text = txtPrecio.Text = txtDescripcion.Text = "";
                txtStockS.Text = txtStockM.Text = txtStockL.Text = txtStockXL.Text = txtStockXXL.Text = "0";
                ddlTipo.SelectedIndex = 0;

                LoadProducts();
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.HtmlEncode(ex.Message)}', 'error');</script>";
            }
        }
    }
}
