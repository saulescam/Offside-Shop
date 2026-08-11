using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class DeleteProduct : System.Web.UI.Page
    {
        private string connectionString = "server=127.0.0.1; database=offsideshop; Uid=root; pwd=Info2026/*-";

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

            // Role guard: only Owner (1) or Admin (2)
            if (Session["UserRole"] == null)
                Response.Redirect("Login.aspx");

            int role = Convert.ToInt32(Session["UserRole"]);
            if (role != 1 && role != 2)
                Response.Redirect("Login.aspx");

            if (!IsPostBack)
                LoadProducts();
        }

        // ──────────────────────────────────────────────────────────────
        //  Load product into READ-ONLY fields for visual auditing
        // ──────────────────────────────────────────────────────────────
        protected void btnSeleccionar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_EnterId", "error");
                return;
            }

            if (!int.TryParse(txtID.Text.Trim(), out int id))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_IdNumeric", "error");
                return;
            }

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                // Load all leagues for the read-only dropdown display
                MySqlCommand leagueCmd = new MySqlCommand(
                    "SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable leagueDt = new DataTable();
                new MySqlDataAdapter(leagueCmd).Fill(leagueDt);
                ddlLeague.Items.Clear();
                ddlLeague.Items.Add(new ListItem("-- League --", ""));
                foreach (DataRow lr in leagueDt.Rows)
                    ddlLeague.Items.Add(new ListItem(lr["Name_League"].ToString(), lr["Id_League"].ToString()));

                // Load all brands
                MySqlCommand brandCmd = new MySqlCommand(
                    "SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                DataTable brandDt = new DataTable();
                new MySqlDataAdapter(brandCmd).Fill(brandDt);
                ddlMarca.Items.Clear();
                ddlMarca.Items.Add(new ListItem("-- Brand --", ""));
                foreach (DataRow br in brandDt.Rows)
                    ddlMarca.Items.Add(new ListItem(br["Name_Brand"].ToString(), br["Id_Brand"].ToString()));

                // Fetch the product
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT t.ID, t.Name, t.Id_Brand, t.Id_Team, t.Year, t.Id_KitType,
                             t.Price, t.Description, tm.Id_League
                      FROM tshirts t
                      LEFT JOIN teams tm ON t.Id_Team = tm.Id_Team
                      WHERE t.ID = @id;", con);
                cmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        TriggerAlert("Alert_ErrorTitle", "Alert_Delete_NotFound", "error");
                        return;
                    }

                    txtNombre.Text = reader["Name"].ToString();
                    txtAnio.Text = Convert.ToInt32(reader["Year"]).ToString();
                    txtPrecio.Text = Convert.ToDecimal(reader["Price"]).ToString("0.00");
                    txtDescripcion.Text = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString();

                    string leagueId = reader["Id_League"] == DBNull.Value ? "" : reader["Id_League"].ToString();
                    string brandId = reader["Id_Brand"] == DBNull.Value ? "" : reader["Id_Brand"].ToString();
                    string teamId = reader["Id_Team"] == DBNull.Value ? "" : reader["Id_Team"].ToString();
                    string kitTypeId = reader["Id_KitType"] == DBNull.Value ? "" : reader["Id_KitType"].ToString();
                    reader.Close();

                    // Load teams for the product's league
                    MySqlCommand teamCmd = new MySqlCommand(
                        "SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @lid ORDER BY Name_Team ASC;", con);
                    teamCmd.Parameters.AddWithValue("@lid", string.IsNullOrEmpty(leagueId) ? (object)DBNull.Value : (object)int.Parse(leagueId));
                    DataTable teamDt = new DataTable();
                    new MySqlDataAdapter(teamCmd).Fill(teamDt);
                    ddlEquipo.Items.Clear();
                    ddlEquipo.Items.Add(new ListItem("-- Team --", ""));
                    foreach (DataRow tr in teamDt.Rows)
                        ddlEquipo.Items.Add(new ListItem(tr["Name_Team"].ToString(), tr["Id_Team"].ToString()));

                    // Pre-select values
                    if (ddlLeague.Items.FindByValue(leagueId) != null) ddlLeague.SelectedValue = leagueId;
                    if (ddlMarca.Items.FindByValue(brandId) != null) ddlMarca.SelectedValue = brandId;
                    if (ddlEquipo.Items.FindByValue(teamId) != null) ddlEquipo.SelectedValue = teamId;
                    if (ddlTipo.Items.FindByValue(kitTypeId) != null) ddlTipo.SelectedValue = kitTypeId;
                }

                // Load size variants
                TextBox[] stockBoxes = { txtStockS, txtStockM, txtStockL, txtStockXL, txtStockXXL };
                foreach (var tb in stockBoxes) tb.Text = "0";

                MySqlCommand varCmd = new MySqlCommand(
                    "SELECT Id_Size, Stock FROM tshirt_variants WHERE Id_Tshirt = @id;", con);
                varCmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader vr = varCmd.ExecuteReader())
                {
                    while (vr.Read())
                    {
                        int sizeId = Convert.ToInt32(vr["Id_Size"]);
                        int stock = Convert.ToInt32(vr["Stock"]);
                        if (sizeId >= 1 && sizeId <= 5)
                            stockBoxes[sizeId - 1].Text = stock.ToString();
                    }
                }
            }

            TriggerAlert("Alert_Delete_LoadedTitle", "Alert_Delete_LoadedText", "warning");
            LoadProducts();
        }

        // ──────────────────────────────────────────────────────────────
        //  Delete product (CASCADE wipes tshirt_variants automatically)
        // ──────────────────────────────────────────────────────────────
        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_FirstLoad", "error");
                return;
            }

            if (!int.TryParse(txtID.Text.Trim(), out int id))
            {
                TriggerAlert("Alert_ErrorTitle", "Alert_Delete_InvalidId", "error");
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    // ON DELETE CASCADE handles tshirt_variants automatically
                    MySqlCommand cmd = new MySqlCommand(
                        "DELETE FROM tshirts WHERE ID = @ID;", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        TriggerAlert("Alert_DeletedTitle", "Alert_Delete_Success", "success");
                        ClearForm();
                        txtID.Text = "";
                        LoadProducts();
                    }
                    else
                    {
                        TriggerAlert("Alert_ErrorTitle", "Alert_Delete_NotFound", "error");
                    }
                }
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Helper: load inventory GridView
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
        //  Helper: clear display fields
        // ──────────────────────────────────────────────────────────────
        private void ClearForm()
        {
            txtNombre.Text = txtAnio.Text = txtPrecio.Text = txtDescripcion.Text = "";
            txtStockS.Text = txtStockM.Text = txtStockL.Text = txtStockXL.Text = txtStockXXL.Text = "0";
            ddlLeague.Items.Clear();
            ddlMarca.Items.Clear();
            ddlEquipo.Items.Clear();
            ddlTipo.SelectedIndex = 0;
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
