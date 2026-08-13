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
                ddlShirtBrand.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Dropdown_AllBrands"), "0"));
                foreach (DataRow row in dtBrands.Rows)
                    ddlShirtBrand.Items.Add(new ListItem(row["Name_Brand"].ToString(), row["Id_Brand"].ToString()));

                MySqlCommand cmdLeagues = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                DataTable dtLeagues = new DataTable();
                new MySqlDataAdapter(cmdLeagues).Fill(dtLeagues);
                ddlShirtLeague.Items.Clear();
                ddlShirtLeague.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Dropdown_AllLeagues"), "0"));
                foreach (DataRow row in dtLeagues.Rows)
                    ddlShirtLeague.Items.Add(new ListItem(row["Name_League"].ToString(), row["Id_League"].ToString()));
            }
            LoadTeamsByLeague(ddlShirtLeague.SelectedValue);
        }

        private void LoadTeamsByLeague(string leagueId)
        {
            ddlShirtTeam.Items.Clear();
            ddlShirtTeam.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Dropdown_AllTeams"), "0"));

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
                TriggerAlert("Alert_DatabaseErrorTitle", ex.Message, "error");
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

                int currentOfferId = 0;
                if (!string.IsNullOrEmpty(hfSelectedOfferId.Value) && int.TryParse(hfSelectedOfferId.Value, out int parsedId))
                {
                    currentOfferId = parsedId;
                }

                string sql = @"SELECT t.ID, t.Name, t.Price, b.Name_Brand AS BrandName, tm.Name_Team AS TeamName,
                                      (SELECT COUNT(1) 
                                       FROM offer_tshirts ot 
                                       INNER JOIN offers o ON ot.Id_Offer = o.Id_Offer 
                                       WHERE ot.Id_Tshirt = t.ID 
                                         AND o.IsActive = 1 
                                         AND NOW() <= o.EndDate
                                         AND (@CurrentOfferId = 0 OR o.Id_Offer <> @CurrentOfferId)
                                      ) AS ExistingOfferCount,
                                      (SELECT o.Name_Offer 
                                       FROM offer_tshirts ot 
                                       INNER JOIN offers o ON ot.Id_Offer = o.Id_Offer 
                                       WHERE ot.Id_Tshirt = t.ID 
                                         AND o.IsActive = 1 
                                         AND NOW() <= o.EndDate
                                         AND (@CurrentOfferId = 0 OR o.Id_Offer <> @CurrentOfferId)
                                       LIMIT 1
                                      ) AS ExistingOfferName
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
                    cmd.Parameters.AddWithValue("@CurrentOfferId", currentOfferId);
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
                TriggerAlert("Alert_Offers_LoadCatalogError", ex.Message, "error");
            }
        }

        protected void gvShirtSelection_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView drv = (DataRowView)e.Row.DataItem;
                int existingCount = 0;
                if (drv["ExistingOfferCount"] != DBNull.Value)
                {
                    existingCount = Convert.ToInt32(drv["ExistingOfferCount"]);
                }

                CheckBox chk = (CheckBox)e.Row.FindControl("chkSelectShirt");
                Label lblStatus = (Label)e.Row.FindControl("lblShirtOfferStatus");

                if (existingCount > 0)
                {
                    string activeStatusText = AlertHelper.GetResourceString(this, "Status_Active");
                    string offerName = drv["ExistingOfferName"] != DBNull.Value ? drv["ExistingOfferName"].ToString() : activeStatusText;
                    if (chk != null)
                    {
                        chk.Checked = false;
                        chk.Enabled = false;
                    }
                    if (lblStatus != null)
                    {
                        string onOfferText = AlertHelper.GetResourceString(this, "Status_OnOffer");
                        string tooltipPattern = AlertHelper.GetResourceString(this, "Admin_Offers_OfferTooltip");
                        string tooltip = string.Format(tooltipPattern, offerName);
                        lblStatus.Text = $"<span class=\"badge bg-warning text-dark font-weight-bold\" title=\"{HttpUtility.HtmlEncode(tooltip)}\"><i class=\"fas fa-tag mr-1\"></i>{HttpUtility.HtmlEncode(onOfferText)}</span>";
                    }
                    string shirtOfferTooltipPattern = AlertHelper.GetResourceString(this, "Admin_Offers_ShirtInOfferTooltip");
                    e.Row.ToolTip = string.Format(shirtOfferTooltipPattern, offerName);
                }
                else
                {
                    if (chk != null)
                    {
                        chk.Enabled = true;
                    }
                    if (lblStatus != null)
                    {
                        string availableText = AlertHelper.GetResourceString(this, "Status_Available");
                        lblStatus.Text = $"<span class=\"badge bg-secondary text-light\" style=\"font-size:0.75rem; opacity:0.7;\">{HttpUtility.HtmlEncode(availableText)}</span>";
                    }
                }
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

                    if (chk != null && chk.Enabled)
                    {
                        if (chk.Checked)
                            selectedIds.Add(shirtId);
                        else
                            selectedIds.Remove(shirtId);
                    }
                    else if (chk != null && !chk.Enabled)
                    {
                        selectedIds.Remove(shirtId);
                    }
                }
            }
            Session["SelectedShirtIds"] = selectedIds;
        }

        private void RestoreCheckboxSelection()
        {
            HashSet<int> selectedIds = Session["SelectedShirtIds"] as HashSet<int>;

            foreach (GridViewRow row in gvShirtSelection.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    int shirtId = Convert.ToInt32(gvShirtSelection.DataKeys[row.RowIndex].Value);
                    CheckBox chk = (CheckBox)row.FindControl("chkSelectShirt");
                    if (chk != null && chk.Enabled)
                    {
                        chk.Checked = selectedIds != null && selectedIds.Contains(shirtId);
                    }
                    else if (chk != null && !chk.Enabled)
                    {
                        chk.Checked = false;
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
            lblFormTitle.Text = AlertHelper.GetResourceString(this, "Offer_Title_Create");
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
                TriggerAlert("Alert_ValidationErrorTitle", "Alert_Offers_NameRequired", "error");
                return;
            }
            if (!int.TryParse(txtDiscountPercentage.Text.Trim(), out int pct) || pct <= 0 || pct > 99)
            {
                TriggerAlert("Alert_ValidationErrorTitle", "Alert_Offers_DiscountInvalid", "error");
                return;
            }
            if (!DateTime.TryParse(txtStartDate.Text, out DateTime start) || !DateTime.TryParse(txtEndDate.Text, out DateTime end))
            {
                TriggerAlert("Alert_ValidationErrorTitle", "Alert_Offers_DatesRequired", "error");
                return;
            }
            if (end <= start)
            {
                TriggerAlert("Alert_ValidationErrorTitle", "Alert_Offers_DatesInvalid", "error");
                return;
            }

            HashSet<int> selectedShirtIds = Session["SelectedShirtIds"] as HashSet<int>;
            if (selectedShirtIds == null || selectedShirtIds.Count == 0)
            {
                TriggerAlert("Alert_ValidationErrorTitle", "Alert_Offers_ShirtsRequired", "error");
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
                TriggerAlert("Alert_SuccessTitle", "Alert_Offers_SavedSuccess", "success");
                ClearFormPanel();
                pnlOfferForm.Visible = false;
                LoadOffers();
                AuditLogger.LogActivity("CREATE", "ManageOffers", $"Created offer ID #{targetOfferId}");

            }
            catch (Exception ex)
            {
                if (trans != null) trans.Rollback();
                TriggerAlert("Alert_Offers_TransactionError", ex.Message, "error");
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
                string editTitlePattern = AlertHelper.GetResourceString(this, "Admin_Offers_EditBlockTitle");
                lblFormTitle.Text = string.Format(editTitlePattern, offerId);
                pnlOfferForm.Visible = true;
                LoadShirtsSelection();
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_Offers_DataRetrievalError", ex.Message, "error");
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
                AuditLogger.LogActivity("UPDATE", "ManageOffers", $"Toggled status for offer ID #{offerId   }");

                TriggerToast("Alert_Offers_StatusToggled");
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
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
                AuditLogger.LogActivity("DELETE", "ManageOffers", $"Deleted offer ID #{offerId}");
                TriggerAlert("Alert_DeletedTitle", "Alert_Offers_DeletedText", "success");
            }
            catch (Exception ex)
            {
                TriggerAlert("Alert_ErrorTitle", ex.Message, "error");
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
                    string expiredText = AlertHelper.GetResourceString(this, "Status_Expired");
                    lblStatus.Text = $"<span class=\"status-badge bg-secondary text-light\">{HttpUtility.HtmlEncode(expiredText)}</span>";
                }
                else
                {
                    string activeText = AlertHelper.GetResourceString(this, "Status_Active");
                    string suspendedText = AlertHelper.GetResourceString(this, "Status_Suspended");
                    lblStatus.Text = isActive
                        ? $"<span class=\"status-badge status-active\">{HttpUtility.HtmlEncode(activeText)}</span>"
                        : $"<span class=\"status-badge status-inactive\">{HttpUtility.HtmlEncode(suspendedText)}</span>";
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

                int currentOfferId = 0;
                if (!string.IsNullOrEmpty(hfSelectedOfferId.Value) && int.TryParse(hfSelectedOfferId.Value, out int parsedId))
                {
                    currentOfferId = parsedId;
                }

                // 4. Construimos la consulta para traer ÚNICAMENTE los IDs del universo filtrado que no estén ya en otra oferta activa
                string sql = @"SELECT t.ID 
                       FROM tshirts t
                       INNER JOIN brands b ON t.Id_Brand = b.Id_Brand
                       INNER JOIN teams tm ON t.Id_Team = tm.Id_Team
                       WHERE 1=1
                         AND NOT EXISTS (
                             SELECT 1 FROM offer_tshirts ot
                             INNER JOIN offers o ON ot.Id_Offer = o.Id_Offer
                             WHERE ot.Id_Tshirt = t.ID
                               AND o.IsActive = 1
                               AND NOW() <= o.EndDate
                               AND (@CurrentOfferId = 0 OR o.Id_Offer <> @CurrentOfferId)
                         )";

                if (brand > 0) sql += " AND t.Id_Brand = @Brand";
                if (league > 0) sql += " AND tm.Id_League = @League";
                if (team > 0) sql += " AND t.Id_Team = @Team";
                if (!string.IsNullOrEmpty(queryText)) sql += " AND t.Name LIKE @Search";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@CurrentOfferId", currentOfferId);
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
                TriggerAlert("Alert_Offers_SelectionError", ex.Message, "error");
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
        protected override void InitializeCulture()
        {
            if (Session["Language"] != null)
            {
                string lang = Session["Language"].ToString();
                string cultureName = (lang == "es") ? "es-SV" : "en-US";
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(cultureName);
                ci.NumberFormat.CurrencySymbol = "$";
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            }
            base.InitializeCulture();
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }

        private void TriggerAlert(string titleKey, string messageKey, string iconType)
        {
            alerta.Text = AlertHelper.GetAlertScript(this, titleKey, messageKey, iconType);
        }

        private void TriggerToast(string titleKey)
        {
            string title = AlertHelper.GetResourceString(this, titleKey);
            string script = $"<script>Swal.fire({{toast:true,position:'top-end',icon:'success',title:'{title.Replace("'", "\\'")}',showConfirmButton:false,timer:2500}});</script>";
            alerta.Text = script;
        }
    }
}