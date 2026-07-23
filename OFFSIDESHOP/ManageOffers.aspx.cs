using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class ManageOffers : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

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

            // PBAC guard: require Perm_Offers
            if (!Security.HasPermission(Session, "Perm_Offers"))
            {
                Response.Redirect("Dashboard.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);

            if (!IsPostBack)
            {
                PopulateFilterDropDowns();
                LoadOffers();
                LoadShirtsSelection();
            }

        }

        private void PopulateFilterDropDowns()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                MySqlCommand cmdBrands = new MySqlCommand("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                DataTable dtBrands = new DataTable();
                new MySqlDataAdapter(cmdBrands).Fill(dtBrands);
                ddlShirtBrand.Items.Clear();
                ddlShirtBrand.Items.Add(new ListItem("-- All Brands --", "0"));
                foreach (DataRow row in dtBrands.Rows)
                    ddlShirtBrand.Items.Add(new ListItem(row["Name_Brand"].ToString(), row["Id_Brand"].ToString()));

                MySqlCommand cmdLeagues = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dtLeagues = new DataTable();
                new MySqlDataAdapter(cmdLeagues).Fill(dtLeagues);
                ddlShirtLeague.Items.Clear();
                ddlShirtLeague.Items.Add(new ListItem("-- All Leagues --", "0"));
                foreach (DataRow row in dtLeagues.Rows)
                    ddlShirtLeague.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));
            }
            LoadTeamsByLeague(ddlShirtLeague.SelectedValue);
        }

        private void LoadTeamsByLeague(string leagueId)
        {
            ddlShirtTeam.Items.Clear();
            ddlShirtTeam.Items.Add(new ListItem("-- All Teams --", "0"));

            if (string.IsNullOrEmpty(leagueId) || leagueId == "0") return;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @IdLeague ORDER BY Name_Team ASC;", con);
                cmd.Parameters.AddWithValue("@IdLeague", Convert.ToInt32(leagueId));
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                foreach (DataRow row in dt.Rows)
                    ddlShirtTeam.Items.Add(new ListItem(row["Name_Team"].ToString(), row["Id_Team"].ToString()));
            }
        }

        protected void ddlShirtLeague_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTeamsByLeague(ddlShirtLeague.SelectedValue);
            LoadShirtsSelection();
        }

        protected void ShirtFilters_Changed(object sender, EventArgs e)
        {
            LoadShirtsSelection();
        }

        private void LoadOffers()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT Id_Offer, Name_Offer, DiscountPercentage, StartDate, EndDate, IsActive FROM offers ORDER BY Id_Offer DESC;", con);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    gvOffers.DataSource = dt;
                    gvOffers.DataBind();
                }
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Database Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        private void LoadShirtsSelection()
        {
            try
            {
                int brand = Convert.ToInt32(ddlShirtBrand.SelectedValue);
                int league = Convert.ToInt32(ddlShirtLeague.SelectedValue);
                int team = Convert.ToInt32(ddlShirtTeam.SelectedValue);
                string queryText = txtShirtSearch.Text.Trim();

                string sql = @"SELECT t.ID, t.Name, t.Price, b.Name_Brand AS BrandName, tm.Name_Team AS TeamName 
                               FROM tshirts t
                               INNER JOIN brands b ON t.Id_Brand = b.Id_Brand
                               INNER JOIN teams tm ON t.Id_Team = tm.Id_Team
                               WHERE 1=1";

                if (brand > 0) sql += " AND t.Id_Brand = @Brand";
                if (league > 0) sql += " AND tm.Id_League = @League";
                if (team > 0) sql += " AND t.Id_Team = @Team";
                if (!string.IsNullOrEmpty(queryText)) sql += " AND t.Name LIKE @Search";

                sql += " ORDER BY t.Name ASC";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    if (brand > 0) cmd.Parameters.AddWithValue("@Brand", brand);
                    if (league > 0) cmd.Parameters.AddWithValue("@League", league);
                    if (team > 0) cmd.Parameters.AddWithValue("@Team", team);
                    if (!string.IsNullOrEmpty(queryText)) cmd.Parameters.AddWithValue("@Search", "%" + queryText + "%");

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    gvShirtSelection.DataSource = dt;
                    gvShirtSelection.DataBind();
                }

                RestoreCheckboxSelection();
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error Loading Catalog', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        private void SaveCheckedStates()
        {
            HashSet<int> selectedIds = Session["SelectedShirtIds"] as HashSet<int> ?? new HashSet<int>();

            foreach (GridViewRow row in gvShirtSelection.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    int shirtId = Convert.ToInt32(gvShirtSelection.DataKeys[row.RowIndex].Value);
                    CheckBox chk = (CheckBox)row.FindControl("chkSelectShirt");

                    if (chk != null)
                    {
                        if (chk.Checked)
                            selectedIds.Add(shirtId);
                        else
                            selectedIds.Remove(shirtId);
                    }
                }
            }
            Session["SelectedShirtIds"] = selectedIds;
        }

        private void RestoreCheckboxSelection()
        {
            HashSet<int> selectedIds = Session["SelectedShirtIds"] as HashSet<int>;
            if (selectedIds == null) return;

            foreach (GridViewRow row in gvShirtSelection.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    int shirtId = Convert.ToInt32(gvShirtSelection.DataKeys[row.RowIndex].Value);
                    CheckBox chk = (CheckBox)row.FindControl("chkSelectShirt");
                    if (chk != null)
                    {
                        chk.Checked = selectedIds.Contains(shirtId);
                    }
                }
            }
        }

        protected void gvShirtSelection_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            SaveCheckedStates();
            gvShirtSelection.PageIndex = e.NewPageIndex;
            LoadShirtsSelection();
        }

        protected void gvOffers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOffers.PageIndex = e.NewPageIndex;
            LoadOffers();
        }

        protected void lbAddNewOffer_Click(object sender, EventArgs e)
        {
            ClearFormPanel();
            Session["SelectedShirtIds"] = new HashSet<int>();
            lblFormTitle.Text = "Create New Promo Campaign Window";
            pnlOfferForm.Visible = true;
            LoadShirtsSelection();
        }

        protected void btnCancelForm_Click(object sender, EventArgs e)
        {
            ClearFormPanel();
            pnlOfferForm.Visible = false;
        }

        protected void btnSaveOffer_Click(object sender, EventArgs e)
        {
            SaveCheckedStates();

            if (string.IsNullOrWhiteSpace(txtOfferName.Text))
            {
                alerta.Text = "<script>Swal.fire('Validation Failure', 'Campaign identity context field is mandatory.', 'error');</script>";
                return;
            }
            if (!int.TryParse(txtDiscountPercentage.Text.Trim(), out int pct) || pct <= 0 || pct > 99)
            {
                alerta.Text = "<script>Swal.fire('Validation Failure', 'Discount values must represent ranges between 1% and 99%.', 'error');</script>";
                return;
            }
            if (!DateTime.TryParse(txtStartDate.Text, out DateTime start) || !DateTime.TryParse(txtEndDate.Text, out DateTime end))
            {
                alerta.Text = "<script>Swal.fire('Validation Failure', 'Configure precise initialization and conclusion timetables.', 'error');</script>";
                return;
            }
            if (end <= start)
            {
                alerta.Text = "<script>Swal.fire('Validation Failure', 'Expiration boundaries must extend past commencement windows.', 'error');</script>";
                return;
            }

            HashSet<int> selectedShirtIds = Session["SelectedShirtIds"] as HashSet<int>;
            if (selectedShirtIds == null || selectedShirtIds.Count == 0)
            {
                alerta.Text = "<script>Swal.fire('Target Mapping Missing', 'Attach at least one catalog item structure to this promotional group.', 'error');</script>";
                return;
            }

            int targetOfferId = 0;
            bool isUpdateMode = !string.IsNullOrEmpty(hfSelectedOfferId.Value) && int.TryParse(hfSelectedOfferId.Value, out targetOfferId);

            MySqlConnection con = new MySqlConnection(connectionString);
            MySqlTransaction trans = null;

            try
            {
                con.Open();
                trans = con.BeginTransaction();

                if (!isUpdateMode)
                {
                    MySqlCommand cmd = new MySqlCommand(
                        @"INSERT INTO offers (Name_Offer, DiscountPercentage, StartDate, EndDate, IsActive) 
                          VALUES (@Name, @Pct, @Start, @End, 1);
                          SELECT LAST_INSERT_ID();", con, trans);
                    cmd.Parameters.AddWithValue("@Name", HttpUtility.HtmlEncode(txtOfferName.Text.Trim()));
                    cmd.Parameters.AddWithValue("@Pct", pct);
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);

                    targetOfferId = Convert.ToInt32(cmd.ExecuteScalar());
                }
                else
                {
                    MySqlCommand cmd = new MySqlCommand(
                        @"UPDATE offers SET Name_Offer = @Name, DiscountPercentage = @Pct, StartDate = @Start, EndDate = @End 
                          WHERE Id_Offer = @Id;", con, trans);
                    cmd.Parameters.AddWithValue("@Name", HttpUtility.HtmlEncode(txtOfferName.Text.Trim()));
                    cmd.Parameters.AddWithValue("@Pct", pct);
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);
                    cmd.Parameters.AddWithValue("@Id", targetOfferId);
                    cmd.ExecuteNonQuery();

                    MySqlCommand flushCmd = new MySqlCommand("DELETE FROM offer_tshirts WHERE Id_Offer = @Id;", con, trans);
                    flushCmd.Parameters.AddWithValue("@Id", targetOfferId);
                    flushCmd.ExecuteNonQuery();
                }

                foreach (int shirtId in selectedShirtIds)
                {
                    MySqlCommand linkCmd = new MySqlCommand(
                        "INSERT INTO offer_tshirts (Id_Offer, Id_Tshirt) VALUES (@IdOffer, @IdShirt);", con, trans);
                    linkCmd.Parameters.AddWithValue("@IdOffer", targetOfferId);
                    linkCmd.Parameters.AddWithValue("@IdShirt", shirtId);
                    linkCmd.ExecuteNonQuery();
                }

                trans.Commit();
                alerta.Text = "<script>Swal.fire('Success', 'Promotional campaign parameter space deployed successfully.', 'success');</script>";
                ClearFormPanel();
                pnlOfferForm.Visible = false;
                LoadOffers();
            }
            catch (Exception ex)
            {
                if (trans != null) trans.Rollback();
                alerta.Text = $"<script>Swal.fire('Transaction Exception Failure', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
            finally
            {
                con.Close();
            }
        }

        protected void gvOffers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int offerId)) return;

            if (e.CommandName == "EditOffer")
            {
                LoadOfferForEdit(offerId);
            }
            else if (e.CommandName == "ToggleOffer")
            {
                ToggleOfferStatus(offerId);
                LoadOffers();
            }
            else if (e.CommandName == "DeleteOffer")
            {
                PermanentDeleteOffer(offerId);
                LoadOffers();
            }
        }

        private void LoadOfferForEdit(int offerId)
        {
            try
            {
                ClearFormPanel();
                HashSet<int> connectedShirts = new HashSet<int>();

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT Id_Offer, Name_Offer, DiscountPercentage, StartDate, EndDate FROM offers WHERE Id_Offer = @Id;", con);
                    cmd.Parameters.AddWithValue("@Id", offerId);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    if (dt.Rows.Count == 0) return;
                    DataRow r = dt.Rows[0];

                    hfSelectedOfferId.Value = r["Id_Offer"].ToString();
                    txtOfferName.Text = HttpUtility.HtmlDecode(r["Name_Offer"].ToString());
                    txtDiscountPercentage.Text = r["DiscountPercentage"].ToString();
                    txtStartDate.Text = Convert.ToDateTime(r["StartDate"]).ToString("yyyy-MM-ddTHH:mm");
                    txtEndDate.Text = Convert.ToDateTime(r["EndDate"]).ToString("yyyy-MM-ddTHH:mm");

                    MySqlCommand linkCmd = new MySqlCommand("SELECT Id_Tshirt FROM offer_tshirts WHERE Id_Offer = @Id;", con);
                    linkCmd.Parameters.AddWithValue("@Id", offerId);
                    using (MySqlDataReader reader = linkCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            connectedShirts.Add(Convert.ToInt32(reader["Id_Tshirt"]));
                        }
                    }
                }

                Session["SelectedShirtIds"] = connectedShirts;
                lblFormTitle.Text = $"Edit Promotional Context Block #{offerId}";
                pnlOfferForm.Visible = true;
                LoadShirtsSelection();
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Data Retrieval Failure', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        private void ToggleOfferStatus(int offerId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE offers SET IsActive = 1 - IsActive WHERE Id_Offer = @Id;", con);
                    cmd.Parameters.AddWithValue("@Id", offerId);
                    cmd.ExecuteNonQuery();
                }
                alerta.Text = "<script>Swal.fire({toast:true,position:'top-end',icon:'success',title:'Campaign visibility altered.',showConfirmButton:false,timer:1800});</script>";
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        private void PermanentDeleteOffer(int offerId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM offers WHERE Id_Offer = @Id;", con);
                    cmd.Parameters.AddWithValue("@Id", offerId);
                    cmd.ExecuteNonQuery();
                }
                alerta.Text = "<script>Swal.fire('Purged', 'Promotional configurations stripped from relational records.', 'success');</script>";
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        protected void gvOffers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView row = (DataRowView)e.Row.DataItem;
            bool isActive = Convert.ToInt32(row["IsActive"]) == 1;
            DateTime endWindow = Convert.ToDateTime(row["EndDate"]);
            bool isExpired = DateTime.Now > endWindow;

            Label lblStatus = (Label)e.Row.FindControl("lblOfferStatus");
            if (lblStatus != null)
            {
                if (isExpired)
                {
                    lblStatus.Text = "<span class=\"status-badge bg-secondary text-light\">Expired</span>";
                }
                else
                {
                    lblStatus.Text = isActive
                        ? "<span class=\"status-badge status-active\">Active</span>"
                        : "<span class=\"status-badge status-inactive\">Suspended</span>";
                }
            }
        }
        protected void btnSelectAllShirts_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Salvamos los cambios manuales que el usuario haya hecho en la página actual primero
                SaveCheckedStates();

                // 2. Recuperamos el contenedor de IDs de sesión o creamos uno nuevo
                HashSet<int> selectedIds = Session["SelectedShirtIds"] as HashSet<int> ?? new HashSet<int>();

                // 3. Capturamos los filtros que están aplicados en este preciso momento en la pantalla
                int brand = Convert.ToInt32(ddlShirtBrand.SelectedValue);
                int league = Convert.ToInt32(ddlShirtLeague.SelectedValue);
                int team = Convert.ToInt32(ddlShirtTeam.SelectedValue);
                string queryText = txtShirtSearch.Text.Trim();

                // 4. Construimos la consulta para traer ÚNICAMENTE los IDs de todo el universo filtrado
                string sql = @"SELECT t.ID 
                       FROM tshirts t
                       INNER JOIN brands b ON t.Id_Brand = b.Id_Brand
                       INNER JOIN teams tm ON t.Id_Team = tm.Id_Team
                       WHERE 1=1";

                if (brand > 0) sql += " AND t.Id_Brand = @Brand";
                if (league > 0) sql += " AND tm.Id_League = @League";
                if (team > 0) sql += " AND t.Id_Team = @Team";
                if (!string.IsNullOrEmpty(queryText)) sql += " AND t.Name LIKE @Search";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    if (brand > 0) cmd.Parameters.AddWithValue("@Brand", brand);
                    if (league > 0) cmd.Parameters.AddWithValue("@League", league);
                    if (team > 0) cmd.Parameters.AddWithValue("@Team", team);
                    if (!string.IsNullOrEmpty(queryText)) cmd.Parameters.AddWithValue("@Search", "%" + queryText + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int shirtId = Convert.ToInt32(reader["ID"]);
                            // .Add() no duplica si el ID ya existía en el HashSet
                            selectedIds.Add(shirtId);
                        }
                    }
                }

                // 5. Guardamos la colección completa en la sesión y refrescamos el GridView
                Session["SelectedShirtIds"] = selectedIds;
                LoadShirtsSelection();
            }
            catch (Exception ex)
            {
                alerta.Text = $"<script>Swal.fire('Error de Selección', '{HttpUtility.JavaScriptStringEncode(ex.Message)}', 'error');</script>";
            }
        }

        protected void btnClearShirtSelection_Click(object sender, EventArgs e)
        {
            // Limpia por completo el contenedor de la sesión para reiniciar la selección desde cero
            Session["SelectedShirtIds"] = new HashSet<int>();
            LoadShirtsSelection();
        }

        private void ClearFormPanel()
        {
            hfSelectedOfferId.Value = "";
            txtOfferName.Text = "";
            txtDiscountPercentage.Text = "";
            txtStartDate.Text = "";
            txtEndDate.Text = "";
            Session["SelectedShirtIds"] = null;
            ddlShirtBrand.SelectedIndex = 0;
            ddlShirtLeague.SelectedIndex = 0;
            LoadTeamsByLeague("0");
            txtShirtSearch.Text = "";
        }

        protected void btnManageOffers_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageOffers.aspx");
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