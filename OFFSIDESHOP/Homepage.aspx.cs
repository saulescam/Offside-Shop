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

            // 2. Evaluamos si el usuario ha iniciado sesión
            if (Session["UserRole"] == null)
            {
                // Escenario A: No hay sesión activa (Usuario Invitado)
                phNavbarGuest.Visible = true;
            }
            else
            {
                int userRole = Convert.ToInt32(Session["UserRole"]);

                if (userRole == 1 || userRole == 2)
                {
                    // Escenario B: El usuario es Owner (1) o Admin (2) -> Muestra la barra con el Dashboard
                    phNavbarAdmin.Visible = true;
                }
                else if (userRole == 3)
                {
                    // Escenario C: El usuario es un Cliente normal -> Muestra la barra con el Cart
                    phNavbarUser.Visible = true;
                    if (!IsPostBack)
                    {
                        CargarDatosPerfilUsuario();
                    }
                }
                else if (userRole == 4)
                {
                    // Si un repartidor entra a la página de inicio, lo mandamos a su panel
                    Response.Redirect("DeliveryDashboard.aspx");
                }
                else
                {
                    // Por seguridad, si hay un rol desconocido, se trata como invitado
                    phNavbarGuest.Visible = true;
                }
            }

            if (!IsPostBack)
            {
                // 1. Primero cargamos los dropdowns para que existan las opciones en el HTML
                LoadFilterDropdowns();
                ActualizarContadorCarrito();

                // 2. Cargar banners dinámicos desde BD
                LoadBanners();
                LoadCollections();

                // 3. Comprobar si venimos redirigidos (por ejemplo, desde DetailsShirt) con filtros en la URL
               // 3. Comprobar si venimos redirigidos (por ejemplo, desde DetailsShirt o Banners) con filtros en la URL
                if (Request.QueryString["search"] != null ||
                    Request.QueryString["league"] != null ||
                    Request.QueryString["brand"] != null ||
                    Request.QueryString["kit"] != null ||
                    Request.QueryString["sale"] != null ||    // NUEVO: Escucha ofertas
                    Request.QueryString["print"] != null)     // NUEVO: Escucha print
                {
                    // Extraemos los datos de la QueryString de forma segura
                    string search = Request.QueryString["search"] != null ? HttpUtility.UrlDecode(Request.QueryString["search"]) : "";
                    string league = Request.QueryString["league"] ?? "";
                    string brand = Request.QueryString["brand"] ?? "";
                    string kit = Request.QueryString["kit"] ?? "";
                    string sale = Request.QueryString["sale"] ?? "";   // NUEVO
                    string print = Request.QueryString["print"] ?? ""; // NUEVO

                    // Asignamos los valores directamente a tus controles superiores
                    txtSearch.Text = search;

                    if (ddlLeague.Items.FindByValue(league) != null)
                        ddlLeague.SelectedValue = league;

                    if (ddlBrand.Items.FindByValue(brand) != null)
                        ddlBrand.SelectedValue = brand;

                    if (ddlKitType.Items.FindByValue(kit) != null)
                        ddlKitType.SelectedValue = kit;

                    // Asignamos los valores a los Checkboxes laterales
                    if (sale.ToLower() == "true" && chkSideOnSale != null)
                        chkSideOnSale.Checked = true;

                    if (print.ToLower() == "true" && chkSideCustomizable != null)
                        chkSideCustomizable.Checked = true;
                }

                // 4. Ejecutamos tu método sin parámetros. 
                // Si la URL traía datos, LoadProducts() los leerá automáticamente desde los controles que acabamos de llenar.
                // Si la URL estaba vacía, los controles estarán vacíos y cargará la vitrina normal (Modo Vitrina).
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

            // Solicitamos al UpdatePanel actualizarse con los datos recuperados
            if (upPerfil != null)
            {
                upPerfil.Update();
            }
        }
        // ──────────────────────────────────────────────────────────────
        //  Load Banners from DB (IsActive = 1, ordered by SortOrder)
        // ──────────────────────────────────────────────────────────────
        private void LoadCollections()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // 1. Cargar Píldoras de Categorías
                    MySqlCommand cmdCats = new MySqlCommand("SELECT * FROM collection_categories", con);
                    DataTable dtCats = new DataTable(); new MySqlDataAdapter(cmdCats).Fill(dtCats);
                    rptCollectionCats.DataSource = dtCats;
                    rptCollectionCats.DataBind();

                    // 2. Cargar Colecciones Activas
                    MySqlCommand cmdCols = new MySqlCommand("SELECT c.*, cat.Name_Category FROM collections c INNER JOIN collection_categories cat ON c.Id_Category = cat.Id_Category WHERE c.IsActive = 1 ORDER BY c.SortOrder ASC", con);
                    DataTable dtCols = new DataTable(); new MySqlDataAdapter(cmdCols).Fill(dtCols);

                    if (dtCols.Rows.Count > 0)
                    {
                        phCollectionsSection.Visible = true;
                        rptCollections.DataSource = dtCols;
                        rptCollections.DataBind();
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
            if (string.IsNullOrWhiteSpace(imageUrl)) return "assets/img/default-product.jpg";
            if (imageUrl.StartsWith("http") || imageUrl.StartsWith("assets/")) return imageUrl;
            return "images/collections/" + imageUrl;
        }
        private void LoadBanners()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT ID, Title, Subtitle, ImageURL, LinkURL, SortOrder " +
                        "FROM Banners WHERE IsActive = 1 ORDER BY SortOrder ASC;", con);
                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    // Prefix relative image paths
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
                        // No active banners – hide the carousel container
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

        /// <summary>
        /// Returns an HTML string for the banner image.
        /// Wraps in an anchor tag only when linkUrl is not empty.
        /// </summary>
        protected string BuildBannerImage(string imageUrl, string title, string linkUrl)
        {
            string imgTag = $"<img src='{HttpUtility.HtmlEncode(imageUrl)}' alt='{HttpUtility.HtmlEncode(title)}' />";

            if (!string.IsNullOrWhiteSpace(linkUrl))
                return $"<a href='{HttpUtility.HtmlEncode(linkUrl)}'>{imgTag}</a>";

            return imgTag;
        }

        // ──────────────────────────────────────────────────────────────
        //  Dynamic Cart Counter (Sums the Quantity column)
        // ──────────────────────────────────────────────────────────────
        private void ActualizarContadorCarrito()
        {
            // Retrieve the cart DataTable from session safely
            DataTable dtCart = Session["Cart"] as DataTable;

            if (dtCart != null && dtCart.Rows.Count > 0)
            {
                int totalProducts = 0;

                // Loop through rows adding actual quantities of each item
                foreach (DataRow row in dtCart.Rows)
                {
                    if (row["Quantity"] != DBNull.Value)
                    {
                        totalProducts += Convert.ToInt32(row["Quantity"]);
                    }
                }

                // Assign cumulative total to the navbar Label
                lblCartCount.Text = totalProducts.ToString();
            }
            else
            {
                // If cart is empty or session doesn't exist, show 0
                lblCartCount.Text = "0";
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Load filter dropdowns from DB
        // ──────────────────────────────────────────────────────────────
        private void LoadFilterDropdowns()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // Leagues
                    MySqlCommand cmdL = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                    using (MySqlDataReader rdr = cmdL.ExecuteReader())
                    {
                        ddlLeague.Items.Clear();
                        ddlLeague.Items.Add(new ListItem("All Leagues", ""));
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
                        ddlBrand.Items.Add(new ListItem("All Brands", ""));
                        while (rdr.Read())
                        {
                            ddlBrand.Items.Add(new ListItem(rdr["Name_Brand"].ToString(), rdr["Id_Brand"].ToString()));
                        }
                    }

                    // Kit Types
                    MySqlCommand cmdK = new MySqlCommand("SELECT Id_KitType, Name_KitType FROM kit_types ORDER BY Name_KitType ASC;", con);
                    using (MySqlDataReader rdr = cmdK.ExecuteReader())
                    {
                        ddlKitType.Items.Clear();
                        ddlKitType.Items.Add(new ListItem("All Kit Types", ""));
                        while (rdr.Read())
                        {
                            ddlKitType.Items.Add(new ListItem(rdr["Name_KitType"].ToString(), rdr["Id_KitType"].ToString()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dropdown filters: " + ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Load products with dynamic filters matching DB schema
        // ──────────────────────────────────────────────────────────────
        private void LoadProducts()
        {
            try
            {
                // 1. Detección de filtros principales (Superiores)
                string searchText = txtSearch.Text.Trim();
                bool hasSearchText = !string.IsNullOrEmpty(searchText);
                bool hasLeagueFilter = ddlLeague.SelectedIndex > 0;
                bool hasBrandFilter = ddlBrand.SelectedIndex > 0;
                bool hasKitTypeFilter = ddlKitType.SelectedIndex > 0;

                // 2. Detección de filtros avanzados (Laterales del Sidebar)
                bool hasSideLeague = ddlSideLeague != null && ddlSideLeague.SelectedIndex > 0;
                bool hasSideBrand = ddlSideBrand != null && ddlSideBrand.SelectedIndex > 0;
                bool hasSideKitType = ddlSideKitType != null && ddlSideKitType.SelectedIndex > 0;
                bool hasSideTeam = ddlSideTeam != null && ddlSideTeam.SelectedIndex > 0;
                bool hasSidePrice = ddlSidePriceRange != null && ddlSidePriceRange.SelectedIndex > 0;
                bool hasSideOnSale = chkSideOnSale != null && chkSideOnSale.Checked;
                bool hasSideCustomizable = chkSideCustomizable != null && chkSideCustomizable.Checked;

                // Validar si hay algún CheckBox de talle marcado
                bool hasSizeFilter = false;
                if (cblSideSizes != null)
                {
                    foreach (ListItem item in cblSideSizes.Items)
                    {
                        if (item.Selected) { hasSizeFilter = true; break; }
                    }
                }

                // El modo búsqueda se activa si se usa CUALQUIER control superior o lateral
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

                    // Base SELECT idéntica a tu consulta original
                    string query = @"SELECT
             t.ID,
             t.Name,
             COALESCE(b.Name_Brand, '')   AS Brand,
             COALESCE(tm.Name_Team, '')   AS Team,
             t.Year,
             COALESCE(kt.Name_KitType,'') AS Type,
             
             /* AQUI ESTÁN TODAS LAS COLUMNAS DE PRECIOS Y OFERTAS */
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
            LEFT JOIN brands    b  ON t.Id_Brand   = b.Id_Brand
            LEFT JOIN teams     tm ON t.Id_Team    = tm.Id_Team
            LEFT JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
            
            /* LOS JOINS VITALES PARA QUE SEPAN SI HAY OFERTA */
            LEFT JOIN offer_tshirts ot ON t.ID = ot.Id_Tshirt
            LEFT JOIN offers o ON ot.Id_Offer = o.Id_Offer AND o.IsActive = 1 AND NOW() BETWEEN o.StartDate AND o.EndDate
            
            WHERE t.IsActive = 1 ";

                    if (isSearchMode)
                    {
                        // Inyección segura de condiciones dinámicas cruzadas (Top Bar + Sidebar)
                        if (hasSearchText)
                        {
                            query += " AND (t.Name LIKE @Search OR b.Name_Brand LIKE @Search OR tm.Name_Team LIKE @Search OR CAST(t.Year AS CHAR) LIKE @Search) ";
                        }

                        // Liga (Filtro lateral toma prioridad sobre el superior)
                        if (hasSideLeague)
                        {
                            query += " AND tm.Id_League = @SideLeagueId ";
                        }
                        else if (hasLeagueFilter)
                        {
                            query += " AND tm.Id_League = @LeagueId ";
                        }

                        // Marca
                        if (hasSideBrand)
                        {
                            query += " AND t.Id_Brand = @SideBrandId ";
                        }
                        else if (hasBrandFilter)
                        {
                            query += " AND t.Id_Brand = @BrandId ";
                        }

                        // Tipo de Kit
                        if (hasSideKitType)
                        {
                            query += " AND t.Id_KitType = @SideKitTypeId ";
                        }
                        else if (hasKitTypeFilter)
                        {
                            query += " AND t.Id_KitType = @KitTypeId ";
                        }

                        // Club/Equipo (Exclusivo lateral)
                        if (hasSideTeam)
                        {
                            query += " AND t.Id_Team = @SideTeamId ";
                        }

                        // Ofertas liquidación (Exclusivo lateral)
                        if (hasSideOnSale)
                        {
                           query += " AND o.Id_Offer IS NOT NULL ";
                        }
                        // Estampado personalizable (Exclusivo lateral)
                        if (hasSideCustomizable)
                        {
                            query += " AND t.IsCustomizable = 1 ";
                        }

                        // Rango de Precios numérico (Exclusivo lateral)
                        if (hasSidePrice)
                        {
                            query += " AND t.Price BETWEEN @PriceMin AND @PriceMax ";
                        }

                        // Talles (Usa EXISTS para evitar la duplicación de filas en relaciones Many-to-Many)
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
                        // Modo Vitrina Exacto: Carga inicial por bloques de ligas de la página principal
                        query += " AND tm.Id_League IN (1, 2, 3, 4, 5, 6) ORDER BY t.ID DESC;";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, con);

                    // Vinculación segura de parámetros dinámicos
                    if (isSearchMode)
                    {
                        if (hasSearchText) cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");

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

                    // Mapeo seguro y corrección de rutas relativas de imágenes globales
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
                        // CONTEXTO DE BÚSQUEDA ACTIVA: Oculta secciones comunes, activa resultados estructurados
                        phSectionsView.Visible = false;
                        phSearchResultsView.Visible = true;
                        phCollectionsSection.Visible = false;
                        phCarousel.Visible = false;
                        phLeaguesSection.Visible = false;
                        phTopBarFilters.Visible = false; // Ocultar filtros dropdown del top bar en modo búsqueda

                        // Construcción dinámica de la etiqueta de criterios activos
                        List<string> filterDetails = new List<string>();
                        if (hasSearchText) filterDetails.Add($"\"{searchText}\"");
                        if (hasSideLeague) filterDetails.Add($"League: {ddlSideLeague.SelectedItem.Text}");
                        else if (hasLeagueFilter) filterDetails.Add($"League: {ddlLeague.SelectedItem.Text}");
                        if (hasSideBrand) filterDetails.Add($"Brand: {ddlSideBrand.SelectedItem.Text}");
                        else if (hasBrandFilter) filterDetails.Add($"Brand: {ddlBrand.SelectedItem.Text}");
                        if (hasSideKitType) filterDetails.Add($"Kit: {ddlSideKitType.SelectedItem.Text}");
                        else if (hasKitTypeFilter) filterDetails.Add($"Kit: {ddlKitType.SelectedItem.Text}");
                        if (hasSideTeam) filterDetails.Add($"Club: {ddlSideTeam.SelectedItem.Text}");

                        lblSearchTerm.Text = "Search results for: " + string.Join(", ", filterDetails);
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

                            // Paginación segura en memoria fija a un máximo de 20 elementos (Matriz de 4x5)
                            int itemsPerPage = 20;
                            int totalRowsCount = dt.Rows.Count;
                            int maxTotalPages = (int)Math.Ceiling((double)totalRowsCount / itemsPerPage);

                            int currentPage = ViewState["SearchCurrentPage"] != null ? Convert.ToInt32(ViewState["SearchCurrentPage"]) : 0;
                            if (currentPage >= maxTotalPages) currentPage = maxTotalPages - 1;
                            if (currentPage < 0) currentPage = 0;
                            ViewState["SearchCurrentPage"] = currentPage;

                            // Sincronización de etiquetas numéricas de paginación (si existen en el front)
                            if (FindControl("lblPageCurrent") != null) ((Label)FindControl("lblPageCurrent")).Text = (currentPage + 1).ToString();
                            if (FindControl("lblPageTotal") != null) ((Label)FindControl("lblPageTotal")).Text = maxTotalPages.ToString();
                            if (FindControl("lnkPrevPage") != null) ((LinkButton)FindControl("lnkPrevPage")).Enabled = (currentPage > 0);
                            if (FindControl("lnkNextPage") != null) ((LinkButton)FindControl("lnkNextPage")).Enabled = (currentPage < maxTotalPages - 1);

                            // Fragmentación del segmento de datos para la página actual
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
                        // MODO VITRINA NORMAL: Manteniendo el 100% de tu código original intacto
                        phSectionsView.Visible = true;
                        phSearchResultsView.Visible = false;
                        phCollectionsSection.Visible = true;
                        phCarousel.Visible = true;
                        phLeaguesSection.Visible = true;
                        phTopBarFilters.Visible = true; // Mostrar filtros dropdown del top bar en modo vitrina

                        // Segmentación por Id_League exacta
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
                                    case 6:
                                        dtWC.ImportRow(row);
                                        break;
                                    case 1:
                                        dtLaLiga.ImportRow(row);
                                        break;
                                    case 2:
                                        dtPremier.ImportRow(row);
                                        break;
                                    case 3:
                                        dtSerieA.ImportRow(row);
                                        break;
                                    case 4:
                                        dtBundesliga.ImportRow(row);
                                        break;
                                    case 5:
                                        dtLigueOne.ImportRow(row);
                                        break;
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

                        // 1. NUEVAS CAMISETAS (Rendereo exacto de las primeras 8 obtenidas por ID DESC)
                        DataTable dtNewArrivals = dt.Clone();
                        int newCount = Math.Min(dt.Rows.Count, 8);
                        for (int i = 0; i < newCount; i++)
                        {
                            dtNewArrivals.ImportRow(dt.Rows[i]);
                        }
                        rptNewArrivals.DataSource = dtNewArrivals;
                        rptNewArrivals.DataBind();

                        // 2. MÁS VENDIDOS (Cruce con order_details y contingencia por fallo/vacío)
                        DataTable dtTopSelling = new DataTable();
                        string topSellingQuery = @"SELECT 
             t.ID,
             t.Name,
             COALESCE(b.Name_Brand, '')   AS Brand,
             COALESCE(tm.Name_Team, '')   AS Team,
             t.Year,
             COALESCE(kt.Name_KitType,'') AS Type,
             
             /* COLUMNAS DE OFERTAS */
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
            LEFT JOIN brands    b  ON t.Id_Brand   = b.Id_Brand
            LEFT JOIN teams     tm ON t.Id_Team    = tm.Id_Team
            LEFT JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
            
            /* LOS JOINS VITALES */
            LEFT JOIN offer_tshirts ot ON t.ID = ot.Id_Tshirt
            LEFT JOIN offers o ON ot.Id_Offer = o.Id_Offer AND o.IsActive = 1 AND NOW() BETWEEN o.StartDate AND o.EndDate
            
            WHERE t.IsActive = 1
            GROUP BY t.ID, t.Name, b.Name_Brand, tm.Name_Team, t.Year, kt.Name_KitType, t.Price, t.ImageURL, t.IsCustomizable, o.Id_Offer, o.DiscountPercentage
            ORDER BY SUM(od.Quantity) DESC
            LIMIT 8;";

                        try
                        {
                            MySqlCommand cmdTop = new MySqlCommand(topSellingQuery, con);
                            new MySqlDataAdapter(cmdTop).Fill(dtTopSelling);

                            if (dtTopSelling.Rows.Count == 0)
                            {
                                string fallbackQuery = query + " ORDER BY RAND() LIMIT 8;";
                                new MySqlDataAdapter(new MySqlCommand(fallbackQuery, con)).Fill(dtTopSelling);
                            }
                        }
                        catch (Exception)
                        {
                            string fallbackQuery = query + " ORDER BY RAND() LIMIT 8;";
                            new MySqlDataAdapter(new MySqlCommand(fallbackQuery, con)).Fill(dtTopSelling);
                        }

                        // Prefijo de imágenes para Más Vendidos
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
            // Redirect straight to cart page on click
            Response.Redirect("Cart.aspx");
        }

        // ──────────────────────────────────────────────────────────────
        //  Event Handlers for search/reset buttons
        // ──────────────────────────────────────────────────────────────
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            SynchronizeMainSearchToSidebar();
            LoadProducts();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlLeague.SelectedIndex = 0;
            ddlBrand.SelectedIndex = 0;
            ddlKitType.SelectedIndex = 0;

            // Reset sidebar filters too
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
                conn.Open();

                // Populate Leagues
                using (MySqlCommand cmd = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC", conn))
                {
                    ddlSideLeague.DataSource = cmd.ExecuteReader();
                    ddlSideLeague.DataTextField = "Name_League";
                    ddlSideLeague.DataValueField = "Id_League";
                    ddlSideLeague.DataBind();
                    ddlSideLeague.Items.Insert(0, new ListItem("All Leagues", "ALL"));
                }

                LoadSideTeams(conn, "ALL");

                // Populate Brands
                using (MySqlCommand cmd = new MySqlCommand("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC", conn))
                {
                    ddlSideBrand.DataSource = cmd.ExecuteReader();
                    ddlSideBrand.DataTextField = "Name_Brand";
                    ddlSideBrand.DataValueField = "Id_Brand";
                    ddlSideBrand.DataBind();
                    ddlSideBrand.Items.Insert(0, new ListItem("All Brands", "ALL"));
                }

                // Populate Kit Styles
                using (MySqlCommand cmd = new MySqlCommand("SELECT Id_KitType, Name_KitType FROM kit_types ORDER BY Name_KitType ASC", conn))
                {
                    ddlSideKitType.DataSource = cmd.ExecuteReader();
                    ddlSideKitType.DataTextField = "Name_KitType";
                    ddlSideKitType.DataValueField = "Id_KitType";
                    ddlSideKitType.DataBind();
                    ddlSideKitType.Items.Insert(0, new ListItem("All Styles", "ALL"));
                }

                // Populate Size List checkboxes
                using (MySqlCommand cmd = new MySqlCommand("SELECT Id_Size, Size_Code FROM sizes ORDER BY Id_Size ASC", conn))
                {
                    cblSideSizes.DataSource = cmd.ExecuteReader();
                    cblSideSizes.DataBind();
                }
            }
        }

        private void LoadSideTeams(MySqlConnection conn, string leagueId)
        {
            string query = (leagueId == "ALL")
                ? "SELECT Id_Team, Name_Team FROM teams ORDER BY Name_Team ASC"
                : "SELECT Id_Team, Name_Team FROM teams WHERE Id_League = @IdLeague ORDER BY Name_Team ASC";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                if (leagueId != "ALL") cmd.Parameters.AddWithValue("@IdLeague", leagueId);

                DataTable dt = new DataTable();
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd)) { da.Fill(dt); }

                ddlSideTeam.DataSource = dt;
                ddlSideTeam.DataTextField = "Name_Team";
                ddlSideTeam.DataValueField = "Id_Team";
                ddlSideTeam.DataBind();
                ddlSideTeam.Items.Insert(0, new ListItem("All Clubs", "ALL"));
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
                ddlSideLeague.SelectedValue = ddlLeague.SelectedValue;
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    LoadSideTeams(conn, ddlSideLeague.SelectedValue);
                }
                ddlLeague.SelectedIndex = 0; // Clear top bar so it doesn't conflict
            }
            if (ddlBrand.SelectedIndex > 0) 
            {
                ddlSideBrand.SelectedValue = ddlBrand.SelectedValue;
                ddlBrand.SelectedIndex = 0; // Clear top bar
            }
            if (ddlKitType.SelectedIndex > 0) 
            {
                ddlSideKitType.SelectedValue = ddlKitType.SelectedValue;
                ddlKitType.SelectedIndex = 0; // Clear top bar
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