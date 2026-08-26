using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Homepage : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;
        private const int ITEMS_PER_PAGE = 20;

        private int SearchCurrentPage
        {
            get { return ViewState["SearchCurrentPage"] != null ? Convert.ToInt32(ViewState["SearchCurrentPage"]) : 0; }
            set { ViewState["SearchCurrentPage"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Control de caché habitual para evitar re-ingresos con el botón atrás
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            phNavbarGuest.Visible = false;
            phNavbarUser.Visible = false;
            phNavbarAdmin.Visible = false;

            // Evaluamos si el usuario ha iniciado sesión
            if (Session["UserRole"] == null)
            {
                phNavbarGuest.Visible = true;
            }
            else
            {
                int userRole = Convert.ToInt32(Session["UserRole"]);

                if (userRole == 1 || userRole == 2)
                {
                    phNavbarAdmin.Visible = true;
                }
                else if (userRole == 3)
                {
                    phNavbarUser.Visible = true;
                    if (!IsPostBack)
                    {
                        CargarDatosPerfilUsuario();
                    }
                }
                else if (userRole == 4)
                {
                    Response.Redirect("DeliveryDashboard.aspx");
                }
                else
                {
                    phNavbarGuest.Visible = true;
                }
            }

            if (!IsPostBack)
            {
                LoadFilterDropdowns();
                ActualizarContadorCarrito();
                LoadBanners();
                LoadCollections();

                if (Request.QueryString["search"] != null ||
                    Request.QueryString["league"] != null ||
                    Request.QueryString["brand"] != null ||
                    Request.QueryString["kit"] != null ||
                    Request.QueryString["sale"] != null ||
                    Request.QueryString["print"] != null)
                {
                    string search = Request.QueryString["search"] != null ? HttpUtility.UrlDecode(Request.QueryString["search"]) : "";
                    string league = Request.QueryString["league"] ?? "";
                    string brand = Request.QueryString["brand"] ?? "";
                    string kit = Request.QueryString["kit"] ?? "";
                    string sale = Request.QueryString["sale"] ?? "";
                    string print = Request.QueryString["print"] ?? "";

                    txtSearch.Text = search;

                    if (ddlLeague.Items.FindByValue(league) != null)
                        ddlLeague.SelectedValue = league;

                    if (ddlBrand.Items.FindByValue(brand) != null)
                        ddlBrand.SelectedValue = brand;

                    if (ddlKitType.Items.FindByValue(kit) != null)
                        ddlKitType.SelectedValue = kit;

                    if (sale.ToLower() == "true" && chkSideOnSale != null)
                        chkSideOnSale.Checked = true;

                    if (print.ToLower() == "true" && chkSideCustomizable != null)
                        chkSideCustomizable.Checked = true;
                }

                LoadProducts();
                InitSidebarFilters();
                SynchronizeMainSearchToSidebar();
                LoadProducts();
            }
        }

        private void CargarDatosPerfilUsuario()
        {
            if (Session["Id_User"] != null)
            {
                string userId = Session["Id_User"].ToString();
                string query = "SELECT Name, Mail FROM users WHERE Id_User = @Id";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);

                        try
                        {
                            conn.Open();
                            MySqlDataReader reader = cmd.ExecuteReader();

                            if (reader.Read())
                            {
                                lblFullName.Text = reader["Name"].ToString();
                                lblUserEmail.Text = reader["Mail"].ToString();
                            }
                            else
                            {
                                lblFullName.Text = "User not found";
                            }
                        }
                        catch (Exception ex)
                        {
                            lblFullName.Text = "Error: " + ex.Message;
                        }
                    }
                }
            }
            else
            {
                lblFullName.Text = "No active session";
            }

            if (upPerfil != null)
            {
                upPerfil.Update();
            }
        }

        private void LoadCollections()
        {
            try
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string queryCats = @"
                        SELECT Id_Category,
                               CASE 
                                   WHEN @Lang = 'es' THEN COALESCE(Name_Category_es, Name_Category)
                                   ELSE Name_Category 
                               END AS Name_Category
                        FROM collection_categories;";

                    using (MySqlCommand cmdCats = new MySqlCommand(queryCats, con))
                    {
                        cmdCats.Parameters.AddWithValue("@Lang", currentLang);
                        DataTable dtCats = new DataTable();
                        new MySqlDataAdapter(cmdCats).Fill(dtCats);

                        rptCollectionCats.DataSource = dtCats;
                        rptCollectionCats.DataBind();
                    }

                    string queryCols = @"
                        SELECT c.Id_Collection,
                               c.Id_Category,
                               CASE 
                                   WHEN @Lang = 'es' THEN COALESCE(c.Title_es, c.Title)
                                   ELSE c.Title 
                               END AS Title,
                               c.ImageURL,
                               c.LinkURL,
                               c.SortOrder,
                               c.IsActive,
                               CASE 
                                   WHEN @Lang = 'es' THEN COALESCE(cat.Name_Category_es, cat.Name_Category)
                                   ELSE cat.Name_Category 
                               END AS Name_Category
                        FROM collections c 
                        INNER JOIN collection_categories cat ON c.Id_Category = cat.Id_Category 
                        WHERE c.IsActive = 1
                        ORDER BY c.SortOrder ASC;";

                    using (MySqlCommand cmdCols = new MySqlCommand(queryCols, con))
                    {
                        cmdCols.Parameters.AddWithValue("@Lang", currentLang);
                        DataTable dtCols = new DataTable();
                        new MySqlDataAdapter(cmdCols).Fill(dtCols);

                        if (dtCols.Rows.Count > 0)
                        {
                            phCollectionsSection.Visible = true;
                            rptCollections.DataSource = dtCols;
                            rptCollections.DataBind();
                        }
                        else
                        {
                            phCollectionsSection.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading collections: " + ex.Message);
            }
        }

        protected string GetCollectionImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return "assets/img/default-product.jpg";

            if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            {
                return imageUrl;
            }

            return "images/collections/" + imageUrl;
        }

        private void LoadBanners()
        {
            try
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                        SELECT 
                            b.ID, 
                            COALESCE(bt.Title, b.Title) AS Title, 
                            COALESCE(bt.Subtitle, b.Subtitle) AS Subtitle, 
                            b.ImageURL, 
                            b.LinkURL, 
                            b.SortOrder 
                        FROM banners b
                        LEFT JOIN banner_translations bt 
                            ON b.ID = bt.Id_Banner AND bt.LanguageCode = @Lang
                        WHERE b.IsActive = 1 
                        ORDER BY b.SortOrder ASC;";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Lang", currentLang);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        string img = row["ImageURL"].ToString();
                        if (!string.IsNullOrWhiteSpace(img) &&
                            !img.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                            !img.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) &&
                            !img.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
                        {
                            row["ImageURL"] = "images/banners/" + img;
                        }
                    }

                    if (dt.Rows.Count == 0)
                    {
                        phCarousel.Visible = false;
                    }
                    else
                    {
                        phLeaguesSection.Visible = true;
                        phCarousel.Visible = true;
                        phCollectionsSection.Visible = true;
                        rptBannerIndicators.DataSource = dt;
                        rptBannerIndicators.DataBind();
                        rptBannerItems.DataSource = dt;
                        rptBannerItems.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading banners: " + ex.Message);
                phCarousel.Visible = false;
                phLeaguesSection.Visible = false;
            }
        }

        protected string BuildBannerImage(string imageUrl, string title, string linkUrl)
        {
            string imgTag = $"<img src='{HttpUtility.HtmlEncode(imageUrl)}' alt='{HttpUtility.HtmlEncode(title)}' />";

            if (!string.IsNullOrWhiteSpace(linkUrl))
                return $"<a href='{HttpUtility.HtmlEncode(linkUrl)}'>{imgTag}</a>";

            return imgTag;
        }

        private void ActualizarContadorCarrito()
        {
            DataTable dtCart = Session["Cart"] as DataTable;

            if (dtCart != null && dtCart.Rows.Count > 0)
            {
                int totalProducts = 0;
                foreach (DataRow row in dtCart.Rows)
                {
                    if (row["Quantity"] != DBNull.Value)
                    {
                        totalProducts += Convert.ToInt32(row["Quantity"]);
                    }
                }
                lblCartCount.Text = totalProducts.ToString();
            }
            else
            {
                lblCartCount.Text = "0";
            }
        }

        private void LoadFilterDropdowns()
        {
            try
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Leagues
                    MySqlCommand cmdL = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                    using (MySqlDataReader rdr = cmdL.ExecuteReader())
                    {
                        ddlLeague.Items.Clear();
                        ddlLeague.Items.Add(new ListItem(currentLang == "es" ? "Todas las Ligas" : "All Leagues", ""));
                        while (rdr.Read())
                        {
                            ddlLeague.Items.Add(new ListItem(rdr["Name_League"].ToString(), rdr["Id_League"].ToString()));
                        }
                    }

                    // Brands
                    MySqlCommand cmdB = new MySqlCommand("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                    using (MySqlDataReader rdr = cmdB.ExecuteReader())
                    {
                        ddlBrand.Items.Clear();
                        ddlBrand.Items.Add(new ListItem(currentLang == "es" ? "Todas las Marcas" : "All Brands", ""));
                        while (rdr.Read())
                        {
                            ddlBrand.Items.Add(new ListItem(rdr["Name_Brand"].ToString(), rdr["Id_Brand"].ToString()));
                        }
                    }

                    // Kit Types
                    string queryKits = @"
                        SELECT Id_KitType, 
                               CASE 
                                   WHEN @Lang = 'es' THEN COALESCE(Name_KitType_es, Name_KitType)
                                   ELSE Name_KitType 
                               END AS Name_KitType 
                        FROM kit_types 
                        ORDER BY Name_KitType ASC;";

                    using (MySqlCommand cmdK = new MySqlCommand(queryKits, con))
                    {
                        cmdK.Parameters.AddWithValue("@Lang", currentLang);
                        using (MySqlDataReader rdr = cmdK.ExecuteReader())
                        {
                            ddlKitType.Items.Clear();
                            ddlKitType.Items.Add(new ListItem(currentLang == "es" ? "Todos los Tipos" : "All Kit Types", ""));
                            while (rdr.Read())
                            {
                                ddlKitType.Items.Add(new ListItem(rdr["Name_KitType"].ToString(), rdr["Id_KitType"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dropdown filters: " + ex.Message);
            }
        }

        private void LoadProducts()
        {
            try
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                // 1. Detección de filtros principales
                string searchText = txtSearch.Text.Trim();
                bool hasSearchText = !string.IsNullOrEmpty(searchText);
                bool hasLeagueFilter = ddlLeague.SelectedIndex > 0;
                bool hasBrandFilter = ddlBrand.SelectedIndex > 0;
                bool hasKitTypeFilter = ddlKitType.SelectedIndex > 0;

                // 2. Detección de filtros avanzados
                bool hasSideLeague = ddlSideLeague != null && ddlSideLeague.SelectedIndex > 0;
                bool hasSideBrand = ddlSideBrand != null && ddlSideBrand.SelectedIndex > 0;
                bool hasSideKitType = ddlSideKitType != null && ddlSideKitType.SelectedIndex > 0;
                bool hasSideTeam = ddlSideTeam != null && ddlSideTeam.SelectedIndex > 0;
                bool hasSidePrice = ddlSidePriceRange != null && ddlSidePriceRange.SelectedIndex > 0;
                bool hasSideOnSale = chkSideOnSale != null && chkSideOnSale.Checked;
                bool hasSideCustomizable = chkSideCustomizable != null && chkSideCustomizable.Checked;

                bool hasSizeFilter = false;
                if (cblSideSizes != null)
                {
                    foreach (ListItem item in cblSideSizes.Items)
                    {
                        if (item.Selected) { hasSizeFilter = true; break; }
                    }
                }

                bool isSearchMode = hasSearchText || hasLeagueFilter || hasBrandFilter || hasKitTypeFilter ||
                                    hasSideLeague || hasSideBrand || hasSideKitType || hasSideTeam ||
                                    hasSidePrice || hasSideOnSale || hasSideCustomizable || hasSizeFilter;

                if (!isSearchMode && IsPostBack)
                {
                    Response.Redirect("Homepage.aspx");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"SELECT
                        t.ID,
                        COALESCE(tr.Name, t.Name) AS Name,
                        COALESCE(tr.Description, t.Description) AS Description,
                        COALESCE(b.Name_Brand, '') AS Brand,
                        CASE 
                            WHEN @Lang = 'es' THEN COALESCE(tm.Name_Team_es, tm.Name_Team, '')
                            ELSE COALESCE(tm.Name_Team, '')
                        END AS Team,
                        t.Year,
                        CASE 
                            WHEN @Lang = 'es' THEN COALESCE(kt.Name_KitType_es, kt.Name_KitType, '')
                            ELSE COALESCE(kt.Name_KitType, '') 
                        END AS Type,
                        t.Price AS OriginalPrice,
                        CASE WHEN o.Id_Offer IS NOT NULL THEN (t.Price - (t.Price * (o.DiscountPercentage / 100.0))) ELSE t.Price END AS FinalPrice,
                        CASE WHEN o.Id_Offer IS NOT NULL THEN 1 ELSE 0 END AS IsOnSale,
                        IFNULL(o.DiscountPercentage, 0) AS DiscountPercentage,
                        COALESCE(t.ImageURL, '')     AS ImageURL,
                        COALESCE(tm.Id_League, 0)    AS Id_League,
                        t.IsCustomizable, 
                        IFNULL(
                            (SELECT GROUP_CONCAT(
                                CASE tv2.Id_Size
                                    WHEN 1 THEN 'S'
                                    WHEN 2 THEN 'M'
                                    WHEN 3 THEN 'L'
                                    WHEN 4 THEN 'XL'
                                    WHEN 5 THEN 'XXL'
                                    ELSE CONCAT('Size ',tv2.Id_Size)
                                END
                                ORDER BY tv2.Id_Size SEPARATOR ', ')
                             FROM tshirt_variants tv2
                             WHERE tv2.Id_Tshirt = t.ID AND tv2.Stock > 0),
                        'N/A') AS Sizes
                    FROM tshirts t
                    LEFT JOIN tshirt_translations tr ON t.ID = tr.Id_Tshirt AND tr.LanguageCode = @Lang
                    LEFT JOIN brands    b  ON t.Id_Brand   = b.Id_Brand
                    LEFT JOIN teams     tm ON t.Id_Team    = tm.Id_Team
                    LEFT JOIN leagues   l  ON tm.Id_League = l.Id_League 
                    LEFT JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
                    LEFT JOIN offer_tshirts ot ON t.ID = ot.Id_Tshirt
                    LEFT JOIN offers o ON ot.Id_Offer = o.Id_Offer AND o.IsActive = 1 AND NOW() BETWEEN o.StartDate AND o.EndDate
                    WHERE t.IsActive = 1 ";

                    if (isSearchMode)
                    {
                        if (hasSearchText)
                        {
                            string[] words = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < words.Length; i++)
                            {
                                // Búsqueda adaptada a ambos idiomas y nombres traducidos
                                query += $" AND (t.Name LIKE @Search{i} " +
                                         $"OR tr.Name LIKE @Search{i} " +
                                         $"OR b.Name_Brand LIKE @Search{i} " +
                                         $"OR tm.Name_Team LIKE @Search{i} " +
                                         $"OR tm.Name_Team_es LIKE @Search{i} " +
                                         $"OR CAST(t.Year AS CHAR) LIKE @Search{i} " +
                                         $"OR l.Name_League LIKE @Search{i} " +
                                         $"OR kt.Name_KitType LIKE @Search{i} " +
                                         $"OR kt.Name_KitType_es LIKE @Search{i}) ";
                            }
                        }

                        if (hasSideLeague) query += " AND tm.Id_League = @SideLeagueId ";
                        else if (hasLeagueFilter) query += " AND tm.Id_League = @LeagueId ";

                        if (hasSideBrand) query += " AND t.Id_Brand = @SideBrandId ";
                        else if (hasBrandFilter) query += " AND t.Id_Brand = @BrandId ";

                        if (hasSideKitType) query += " AND t.Id_KitType = @SideKitTypeId ";
                        else if (hasKitTypeFilter) query += " AND t.Id_KitType = @KitTypeId ";

                        if (hasSideTeam) query += " AND t.Id_Team = @SideTeamId ";
                        if (hasSideOnSale) query += " AND o.Id_Offer IS NOT NULL ";
                        if (hasSideCustomizable) query += " AND t.IsCustomizable = 1 ";
                        if (hasSidePrice) query += " AND t.Price BETWEEN @PriceMin AND @PriceMax ";

                        if (hasSizeFilter)
                        {
                            query += " AND EXISTS (SELECT 1 FROM tshirt_variants ev WHERE ev.Id_Tshirt = t.ID AND ev.Stock > 0 AND ev.Id_Size IN (";
                            List<string> sizeParams = new List<string>();
                            int sCount = 0;
                            foreach (ListItem item in cblSideSizes.Items)
                            {
                                if (item.Selected)
                                {
                                    sizeParams.Add("@SizeId" + sCount);
                                    sCount++;
                                }
                            }
                            query += string.Join(",", sizeParams) + ")) ";
                        }

                        query += " ORDER BY t.ID DESC;";
                    }
                    else
                    {
                        query += " AND tm.Id_League IN (1, 2, 3, 4, 5, 6) ORDER BY t.ID DESC;";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Lang", currentLang);

                    if (isSearchMode)
                    {
                        if (hasSearchText)
                        {
                            string[] words = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < words.Length; i++)
                            {
                                cmd.Parameters.AddWithValue($"@Search{i}", "%" + words[i] + "%");
                            }
                        }

                        if (hasLeagueFilter) cmd.Parameters.AddWithValue("@LeagueId", Convert.ToInt32(ddlLeague.SelectedValue));
                        else if (hasSideLeague) cmd.Parameters.AddWithValue("@SideLeagueId", Convert.ToInt32(ddlSideLeague.SelectedValue));

                        if (hasBrandFilter) cmd.Parameters.AddWithValue("@BrandId", Convert.ToInt32(ddlBrand.SelectedValue));
                        else if (hasSideBrand) cmd.Parameters.AddWithValue("@SideBrandId", Convert.ToInt32(ddlSideBrand.SelectedValue));

                        if (hasKitTypeFilter) cmd.Parameters.AddWithValue("@KitTypeId", Convert.ToInt32(ddlKitType.SelectedValue));
                        else if (hasSideKitType) cmd.Parameters.AddWithValue("@SideKitTypeId", Convert.ToInt32(ddlSideKitType.SelectedValue));

                        if (hasSideTeam) cmd.Parameters.AddWithValue("@SideTeamId", Convert.ToInt32(ddlSideTeam.SelectedValue));

                        if (hasSidePrice)
                        {
                            string[] parts = ddlSidePriceRange.SelectedValue.Split('-');
                            if (parts.Length == 2)
                            {
                                cmd.Parameters.AddWithValue("@PriceMin", Convert.ToDecimal(parts[0]));
                                cmd.Parameters.AddWithValue("@PriceMax", Convert.ToDecimal(parts[1]));
                            }
                        }

                        if (hasSizeFilter)
                        {
                            int sCount = 0;
                            foreach (ListItem item in cblSideSizes.Items)
                            {
                                if (item.Selected)
                                {
                                    cmd.Parameters.AddWithValue("@SizeId" + sCount, Convert.ToInt32(item.Value));
                                    sCount++;
                                }
                            }
                        }
                    }

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        string img = row["ImageURL"].ToString();
                        if (!string.IsNullOrWhiteSpace(img) &&
                            !img.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                            !img.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) &&
                            !img.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
                        {
                            row["ImageURL"] = "images/camisetas/" + img;
                        }
                    }

                    if (isSearchMode)
                    {
                        phSectionsView.Visible = false;
                        phSearchResultsView.Visible = true;
                        phCollectionsSection.Visible = false;
                        phCarousel.Visible = false;
                        phLeaguesSection.Visible = false;
                        phTopBarFilters.Visible = false;

                        string lblResultsFor = currentLang == "es" ? "Resultados de búsqueda para:" : "Search results for:";
                        string lblLeagueText = currentLang == "es" ? "Liga" : "League";
                        string lblBrandText = currentLang == "es" ? "Marca" : "Brand";
                        string lblKitText = currentLang == "es" ? "Tipo" : "Kit";
                        string lblClubText = currentLang == "es" ? "Equipo" : "Club";

                        List<string> filterDetails = new List<string>();
                        if (hasSearchText) filterDetails.Add($"\"{searchText}\"");

                        if (hasSideLeague) filterDetails.Add($"{lblLeagueText}: {ddlSideLeague.SelectedItem.Text}");
                        else if (hasLeagueFilter) filterDetails.Add($"{lblLeagueText}: {ddlLeague.SelectedItem.Text}");

                        if (hasSideBrand) filterDetails.Add($"{lblBrandText}: {ddlSideBrand.SelectedItem.Text}");
                        else if (hasBrandFilter) filterDetails.Add($"{lblBrandText}: {ddlBrand.SelectedItem.Text}");

                        if (hasSideKitType) filterDetails.Add($"{lblKitText}: {ddlSideKitType.SelectedItem.Text}");
                        else if (hasKitTypeFilter) filterDetails.Add($"{lblKitText}: {ddlKitType.SelectedItem.Text}");

                        if (hasSideTeam) filterDetails.Add($"{lblClubText}: {ddlSideTeam.SelectedItem.Text}");

                        lblSearchTerm.Text = $"{lblResultsFor} " + string.Join(", ", filterDetails);
                        litMatchesCount.Text = dt.Rows.Count.ToString();

                        if (dt.Rows.Count == 0)
                        {
                            pnlNoMatchesFound.Visible = true;
                            if (FindControl("pnlPagination") != null) FindControl("pnlPagination").Visible = false;

                            rptSearchResults.DataSource = dt;
                            rptSearchResults.DataBind();
                        }
                        else
                        {
                            pnlNoMatchesFound.Visible = false;
                            if (FindControl("pnlPagination") != null) FindControl("pnlPagination").Visible = true;

                            int itemsPerPage = 20;
                            int totalRowsCount = dt.Rows.Count;
                            int maxTotalPages = (int)Math.Ceiling((double)totalRowsCount / itemsPerPage);

                            int currentPage = ViewState["SearchCurrentPage"] != null ? Convert.ToInt32(ViewState["SearchCurrentPage"]) : 0;
                            if (currentPage >= maxTotalPages) currentPage = maxTotalPages - 1;
                            if (currentPage < 0) currentPage = 0;
                            ViewState["SearchCurrentPage"] = currentPage;

                            if (FindControl("lblPageCurrent") != null) ((Label)FindControl("lblPageCurrent")).Text = (currentPage + 1).ToString();
                            if (FindControl("lblPageTotal") != null) ((Label)FindControl("lblPageTotal")).Text = maxTotalPages.ToString();
                            if (FindControl("lnkPrevPage") != null) ((LinkButton)FindControl("lnkPrevPage")).Enabled = (currentPage > 0);
                            if (FindControl("lnkNextPage") != null) ((LinkButton)FindControl("lnkNextPage")).Enabled = (currentPage < maxTotalPages - 1);

                            DataTable dtPagedChunk = dt.Clone();
                            int startIndexOffset = currentPage * itemsPerPage;
                            int endIndexOffset = Math.Min(startIndexOffset + itemsPerPage, totalRowsCount);

                            for (int j = startIndexOffset; j < endIndexOffset; j++)
                            {
                                dtPagedChunk.ImportRow(dt.Rows[j]);
                            }

                            rptSearchResults.DataSource = dtPagedChunk;
                            rptSearchResults.DataBind();
                        }
                    }
                    else
                    {
                        phSectionsView.Visible = true;
                        phSearchResultsView.Visible = false;
                        phCollectionsSection.Visible = true;
                        phCarousel.Visible = true;
                        phLeaguesSection.Visible = true;
                        phTopBarFilters.Visible = true;

                        DataTable dtWC = dt.Clone();
                        DataTable dtLaLiga = dt.Clone();
                        DataTable dtPremier = dt.Clone();
                        DataTable dtSerieA = dt.Clone();
                        DataTable dtBundesliga = dt.Clone();
                        DataTable dtLigueOne = dt.Clone();

                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["Id_League"] != DBNull.Value)
                            {
                                int leagueId = Convert.ToInt32(row["Id_League"]);
                                switch (leagueId)
                                {
                                    case 6: dtWC.ImportRow(row); break;
                                    case 1: dtLaLiga.ImportRow(row); break;
                                    case 2: dtPremier.ImportRow(row); break;
                                    case 3: dtSerieA.ImportRow(row); break;
                                    case 4: dtBundesliga.ImportRow(row); break;
                                    case 5: dtLigueOne.ImportRow(row); break;
                                }
                            }
                        }

                        rptWC.DataSource = dtWC;
                        rptWC.DataBind();

                        rptLaLiga.DataSource = dtLaLiga;
                        rptLaLiga.DataBind();

                        rptPremier.DataSource = dtPremier;
                        rptPremier.DataBind();

                        rptSerieA.DataSource = dtSerieA;
                        rptSerieA.DataBind();

                        rptBundesliga.DataSource = dtBundesliga;
                        rptBundesliga.DataBind();

                        rptLigueOne.DataSource = dtLigueOne;
                        rptLigueOne.DataBind();

                        DataTable dtNewArrivals = dt.Clone();
                        int newCount = Math.Min(dt.Rows.Count, 8);
                        for (int i = 0; i < newCount; i++)
                        {
                            dtNewArrivals.ImportRow(dt.Rows[i]);
                        }
                        rptNewArrivals.DataSource = dtNewArrivals;
                        rptNewArrivals.DataBind();

                        DataTable dtTopSelling = new DataTable();
                        string topSellingQuery = @"SELECT 
                            t.ID,
                            COALESCE(tr.Name, t.Name) AS Name,
                            COALESCE(b.Name_Brand, '')   AS Brand,
                            CASE 
                                WHEN @Lang = 'es' THEN COALESCE(tm.Name_Team_es, tm.Name_Team, '')
                                ELSE COALESCE(tm.Name_Team, '')
                            END AS Team,
                            t.Year,
                            CASE 
                                WHEN @Lang = 'es' THEN COALESCE(kt.Name_KitType_es, kt.Name_KitType, '')
                                ELSE COALESCE(kt.Name_KitType, '') 
                            END AS Type,
                            t.Price AS OriginalPrice,
                            CASE WHEN o.Id_Offer IS NOT NULL THEN (t.Price - (t.Price * (o.DiscountPercentage / 100.0))) ELSE t.Price END AS FinalPrice,
                            CASE WHEN o.Id_Offer IS NOT NULL THEN 1 ELSE 0 END AS IsOnSale,
                            IFNULL(o.DiscountPercentage, 0) AS DiscountPercentage,
                            COALESCE(t.ImageURL, '')     AS ImageURL,
                            t.IsCustomizable,
                            IFNULL(
                                (SELECT GROUP_CONCAT(
                                    CASE tv2.Id_Size
                                        WHEN 1 THEN 'S'
                                        WHEN 2 THEN 'M'
                                        WHEN 3 THEN 'L'
                                        WHEN 4 THEN 'XL'
                                        WHEN 5 THEN 'XXL'
                                        ELSE CONCAT('Size ',tv2.Id_Size)
                                    END
                                    ORDER BY tv2.Id_Size SEPARATOR ', ')
                                 FROM tshirt_variants tv2
                                 WHERE tv2.Id_Tshirt = t.ID AND tv2.Stock > 0),
                            'N/A') AS Sizes
                        FROM order_details od
                        INNER JOIN tshirts t ON od.ProductName = t.Name
                        LEFT JOIN tshirt_translations tr ON t.ID = tr.Id_Tshirt AND tr.LanguageCode = @Lang
                        LEFT JOIN brands    b  ON t.Id_Brand   = b.Id_Brand
                        LEFT JOIN teams     tm ON t.Id_Team    = tm.Id_Team
                        LEFT JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
                        LEFT JOIN offer_tshirts ot ON t.ID = ot.Id_Tshirt
                        LEFT JOIN offers o ON ot.Id_Offer = o.Id_Offer AND o.IsActive = 1 AND NOW() BETWEEN o.StartDate AND o.EndDate
                        WHERE t.IsActive = 1
                        GROUP BY t.ID, tr.Name, t.Name, b.Name_Brand, tm.Name_Team, tm.Name_Team_es, t.Year, kt.Name_KitType_es, kt.Name_KitType, t.Price, t.ImageURL, t.IsCustomizable, o.Id_Offer, o.DiscountPercentage
                        ORDER BY SUM(od.Quantity) DESC
                        LIMIT 8;";

                        try
                        {
                            MySqlCommand cmdTop = new MySqlCommand(topSellingQuery, con);
                            cmdTop.Parameters.AddWithValue("@Lang", currentLang);
                            new MySqlDataAdapter(cmdTop).Fill(dtTopSelling);

                            if (dtTopSelling.Rows.Count == 0)
                            {
                                string fallbackQuery = query + " ORDER BY RAND() LIMIT 8;";
                                MySqlCommand cmdFallback = new MySqlCommand(fallbackQuery, con);
                                cmdFallback.Parameters.AddWithValue("@Lang", currentLang);
                                new MySqlDataAdapter(cmdFallback).Fill(dtTopSelling);
                            }
                        }
                        catch (Exception)
                        {
                            string fallbackQuery = query + " ORDER BY RAND() LIMIT 8;";
                            MySqlCommand cmdFallback = new MySqlCommand(fallbackQuery, con);
                            cmdFallback.Parameters.AddWithValue("@Lang", currentLang);
                            new MySqlDataAdapter(cmdFallback).Fill(dtTopSelling);
                        }

                        foreach (DataRow row in dtTopSelling.Rows)
                        {
                            string img = row["ImageURL"].ToString();
                            if (!string.IsNullOrWhiteSpace(img) &&
                                !img.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                                !img.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) &&
                                !img.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
                            {
                                row["ImageURL"] = "images/camisetas/" + img;
                            }
                        }

                        rptTopSelling.DataSource = dtTopSelling;
                        rptTopSelling.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading products: " + ex.Message);
            }
        }

        protected void btnNavCart_Click(object sender, EventArgs e)
        {
            Response.Redirect("Cart.aspx");
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                LoadSideTeams(conn, "ALL");
            }

            if (ddlSideLeague != null) { ddlSideLeague.ClearSelection(); ddlSideLeague.SelectedIndex = 0; }
            if (ddlSideBrand != null) { ddlSideBrand.ClearSelection(); ddlSideBrand.SelectedIndex = 0; }
            if (ddlSideKitType != null) { ddlSideKitType.ClearSelection(); ddlSideKitType.SelectedIndex = 0; }
            if (ddlSideTeam != null) { ddlSideTeam.ClearSelection(); ddlSideTeam.SelectedIndex = 0; }
            if (ddlSidePriceRange != null) { ddlSidePriceRange.ClearSelection(); ddlSidePriceRange.SelectedIndex = 0; }

            if (chkSideOnSale != null) chkSideOnSale.Checked = false;
            if (chkSideCustomizable != null) chkSideCustomizable.Checked = false;

            if (cblSideSizes != null)
            {
                foreach (ListItem item in cblSideSizes.Items) item.Selected = false;
            }

            SynchronizeMainSearchToSidebar();

            SearchCurrentPage = 0;
            LoadProducts();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlLeague.SelectedIndex = 0;
            ddlBrand.SelectedIndex = 0;
            ddlKitType.SelectedIndex = 0;

            if (ddlSideLeague != null) ddlSideLeague.SelectedIndex = 0;
            if (ddlSideBrand != null) ddlSideBrand.SelectedIndex = 0;
            if (ddlSideKitType != null) ddlSideKitType.SelectedIndex = 0;
            if (ddlSideTeam != null) ddlSideTeam.SelectedIndex = 0;
            if (ddlSidePriceRange != null) ddlSidePriceRange.SelectedIndex = 0;
            if (chkSideOnSale != null) chkSideOnSale.Checked = false;
            if (chkSideCustomizable != null) chkSideCustomizable.Checked = false;
            if (cblSideSizes != null)
            {
                foreach (ListItem item in cblSideSizes.Items)
                    item.Selected = false;
            }

            LoadProducts();
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        protected void btnMyOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyOrders.aspx");
        }

        private void InitSidebarFilters()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC", conn))
                {
                    ddlSideLeague.DataSource = cmd.ExecuteReader();
                    ddlSideLeague.DataTextField = "Name_League";
                    ddlSideLeague.DataValueField = "Id_League";
                    ddlSideLeague.DataBind();
                    ddlSideLeague.Items.Insert(0, new ListItem(currentLang == "es" ? "Todas las Ligas" : "All Leagues", "ALL"));
                }

                LoadSideTeams(conn, "ALL");

                using (MySqlCommand cmd = new MySqlCommand("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC", conn))
                {
                    ddlSideBrand.DataSource = cmd.ExecuteReader();
                    ddlSideBrand.DataTextField = "Name_Brand";
                    ddlSideBrand.DataValueField = "Id_Brand";
                    ddlSideBrand.DataBind();
                    ddlSideBrand.Items.Insert(0, new ListItem(currentLang == "es" ? "Todas las Marcas" : "All Brands", "ALL"));
                }

                string querySideKits = @"
                    SELECT Id_KitType, 
                           CASE 
                               WHEN @Lang = 'es' THEN COALESCE(Name_KitType_es, Name_KitType)
                               ELSE Name_KitType 
                           END AS Name_KitType 
                    FROM kit_types 
                    ORDER BY Name_KitType ASC;";

                using (MySqlCommand cmd = new MySqlCommand(querySideKits, conn))
                {
                    cmd.Parameters.AddWithValue("@Lang", currentLang);
                    ddlSideKitType.DataSource = cmd.ExecuteReader();
                    ddlSideKitType.DataTextField = "Name_KitType";
                    ddlSideKitType.DataValueField = "Id_KitType";
                    ddlSideKitType.DataBind();
                    ddlSideKitType.Items.Insert(0, new ListItem(currentLang == "es" ? "Todos los Estilos" : "All Styles", "ALL"));
                }

                using (MySqlCommand cmd = new MySqlCommand("SELECT Id_Size, Size_Code FROM sizes ORDER BY Id_Size ASC", conn))
                {
                    cblSideSizes.DataSource = cmd.ExecuteReader();
                    cblSideSizes.DataBind();
                }

                if (ddlSidePriceRange != null)
                {
                    string selectedPrice = ddlSidePriceRange.SelectedValue;

                    ddlSidePriceRange.Items.Clear();

                    if (currentLang == "es")
                    {
                        ddlSidePriceRange.Items.Add(new ListItem("Todos los Precios", ""));
                        ddlSidePriceRange.Items.Add(new ListItem("Menos de $50", "0-50"));
                        ddlSidePriceRange.Items.Add(new ListItem("De $50 a $100", "50-100"));
                        ddlSidePriceRange.Items.Add(new ListItem("De $100 a $150", "100-150"));
                        ddlSidePriceRange.Items.Add(new ListItem("Más de $150", "150-99999"));
                    }
                    else
                    {
                        ddlSidePriceRange.Items.Add(new ListItem("All Prices", ""));
                        ddlSidePriceRange.Items.Add(new ListItem("Under $50", "0-50"));
                        ddlSidePriceRange.Items.Add(new ListItem("$50 to $100", "50-100"));
                        ddlSidePriceRange.Items.Add(new ListItem("$100 to $150", "100-150"));
                        ddlSidePriceRange.Items.Add(new ListItem("Over $150", "150-99999"));
                    }

                    if (!string.IsNullOrEmpty(selectedPrice) && ddlSidePriceRange.Items.FindByValue(selectedPrice) != null)
                    {
                        ddlSidePriceRange.SelectedValue = selectedPrice;
                    }
                }
            }
        }

        private void LoadSideTeams(MySqlConnection conn, string leagueId)
        {
            string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

            string query = (leagueId == "ALL")
                ? @"SELECT Id_Team, 
                           CASE 
                               WHEN @Lang = 'es' THEN COALESCE(Name_Team_es, Name_Team) 
                               ELSE Name_Team 
                           END AS Name_Team 
                    FROM teams ORDER BY Name_Team ASC"
                : @"SELECT Id_Team, 
                           CASE 
                               WHEN @Lang = 'es' THEN COALESCE(Name_Team_es, Name_Team) 
                               ELSE Name_Team 
                           END AS Name_Team 
                    FROM teams WHERE Id_League = @IdLeague ORDER BY Name_Team ASC";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Lang", currentLang);
                if (leagueId != "ALL") cmd.Parameters.AddWithValue("@IdLeague", leagueId);

                DataTable dt = new DataTable();
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd)) { da.Fill(dt); }

                ddlSideTeam.ClearSelection();

                ddlSideTeam.DataSource = dt;
                ddlSideTeam.DataTextField = "Name_Team";
                ddlSideTeam.DataValueField = "Id_Team";
                ddlSideTeam.DataBind();

                ddlSideTeam.Items.Insert(0, new ListItem(currentLang == "es" ? "Todos los Equipos" : "All Clubs", "ALL"));
                ddlSideTeam.SelectedIndex = 0;
            }
        }

        protected void ddlSideLeague_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                LoadSideTeams(conn, ddlSideLeague.SelectedValue);
            }
        }

        private void SynchronizeMainSearchToSidebar()
        {
            if (ddlLeague.SelectedIndex > 0)
            {
                if (ddlSideLeague.Items.FindByValue(ddlLeague.SelectedValue) != null)
                {
                    ddlSideLeague.SelectedValue = ddlLeague.SelectedValue;

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        LoadSideTeams(conn, ddlSideLeague.SelectedValue);
                    }
                }
                ddlLeague.SelectedIndex = 0;
            }

            if (ddlBrand.SelectedIndex > 0)
            {
                if (ddlSideBrand.Items.FindByValue(ddlBrand.SelectedValue) != null)
                {
                    ddlSideBrand.SelectedValue = ddlBrand.SelectedValue;
                }
                ddlBrand.SelectedIndex = 0;
            }

            if (ddlKitType.SelectedIndex > 0)
            {
                if (ddlSideKitType.Items.FindByValue(ddlKitType.SelectedValue) != null)
                {
                    ddlSideKitType.SelectedValue = ddlKitType.SelectedValue;
                }
                ddlKitType.SelectedIndex = 0;
            }
        }

        protected void btnApplySideFilters_Click(object sender, EventArgs e)
        {
            SearchCurrentPage = 0;
            LoadProducts();
        }

        protected void lnkPrevPage_Click(object sender, EventArgs e)
        {
            if (SearchCurrentPage > 0)
            {
                SearchCurrentPage--;
                LoadProducts();
            }
        }

        protected void lnkNextPage_Click(object sender, EventArgs e)
        {
            SearchCurrentPage++;
            LoadProducts();
        }

        protected void btnGoToAccount_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyAccount.aspx");
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }

        protected string FormatJerseyName(object nameObj)
        {
            if (nameObj == null || nameObj == DBNull.Value) return "";

            string name = nameObj.ToString().ToLower().Trim();
            System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(name);
        }
    }
}