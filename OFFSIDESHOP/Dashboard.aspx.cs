using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace OFFSIDESHOP
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Control agresivo de caché para evitar volver atrás tras Logout
                Response.Buffer = true;
                Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
                Response.Expires = -1500;
                Response.CacheControl = "no-cache";
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                Response.Cache.SetNoStore();

                // Control de acceso basado en Roles: Dueño (1) o Administrador (2)
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

                Security.ConfigureAdminSidebar(this);

                if (!IsPostBack)
                {
                    // Bind administrator details
                    lblAdminName.Text = Session["Admin"] != null ? Session["Admin"].ToString() : "Admin";
                    bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                    if (role == 2)
                    {
                        phAdminPermissions.Visible = true;
                        List<string> permissions = new List<string>();
                        if (Security.HasPermission(Session, "Perm_Products")) permissions.Add(isSpanish ? "Productos" : "Products");
                        if (Security.HasPermission(Session, "Perm_Orders")) permissions.Add(isSpanish ? "Pedidos" : "Orders");
                        if (Security.HasPermission(Session, "Perm_Offers")) permissions.Add(isSpanish ? "Ofertas" : "Offers");
                        if (Security.HasPermission(Session, "Perm_Coupons")) permissions.Add(isSpanish ? "Cupones" : "Coupons");
                        if (Security.HasPermission(Session, "Perm_Banners")) permissions.Add(isSpanish ? "Banners" : "Banners");
                        if (Security.HasPermission(Session, "Perm_Tickets")) permissions.Add(isSpanish ? "Tickets" : "Tickets");

                        string prefix = isSpanish ? "Tienes permisos para: " : "You have permissions for: ";
                        string noneStr = isSpanish ? "Ninguno" : "None";
                        lblAdminPermissions.Text = prefix + (permissions.Count > 0 ? string.Join(", ", permissions) : noneStr);
                    }
                    else
                    {
                        phAdminPermissions.Visible = false;
                    }

                    LoadStatistics();

                    // 1. Orders Activity Workspace (Perm_Orders)
                    if (Security.HasPermission(Session, "Perm_Orders"))
                    {
                        phRecentOrders.Visible = true;
                        LoadRecentOrdersGrid();
                    }
                    else
                    {
                        phRecentOrders.Visible = false;
                    }

                    // 2. Inventory Stock Alerts Workspace (Perm_Products)
                    if (Security.HasPermission(Session, "Perm_Products"))
                    {
                        phCriticalStock.Visible = true;
                        LoadCriticalStockGrid();
                    }
                    else
                    {
                        phCriticalStock.Visible = false;
                    }

                    // 3. Support & Consignments Workspace (Perm_Tickets)
                    if (Security.HasPermission(Session, "Perm_Tickets"))
                    {
                        phPendingTickets.Visible = true;
                        LoadPendingTicketsGrid();
                    }
                    else
                    {
                        phPendingTickets.Visible = false;
                    }

                    // 4. Live Security Audit Feed Workspace (Owner Level: UserRole == 1 Only)
                    if (Session["UserRole"] != null && Convert.ToInt32(Session["UserRole"]) == 1)
                    {
                        phAuditLogs.Visible = true;
                        LoadAuditLogsGrid();
                    }
                    else
                    {
                        phAuditLogs.Visible = false;
                    }

                    LoadDashBanners();
                }
            }
            catch (Exception)
            {
                Response.Redirect("Login.aspx");
            }
        }

        private void LoadStatistics()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // 1. Usuarios Registrados (Tabla: users)
                    using (MySqlCommand cmdUsers = new MySqlCommand("SELECT COUNT(*) FROM users", con))
                    {
                        int userCount = Convert.ToInt32(cmdUsers.ExecuteScalar());
                        lblUserCount.Text = userCount.ToString();
                    }

                    // 2. Camisetas en Catálogo (Tabla: tshirts)
                    using (MySqlCommand cmdShirts = new MySqlCommand("SELECT COUNT(*) FROM tshirts", con))
                    {
                        int shirtCount = Convert.ToInt32(cmdShirts.ExecuteScalar());
                        lblShirtCount.Text = shirtCount.ToString();
                    }

                    // 3. Ligas Añadidas (Tabla: leagues)
                    using (MySqlCommand cmdLeagues = new MySqlCommand("SELECT COUNT(*) FROM leagues", con))
                    {
                        int leaguesCount = Convert.ToInt32(cmdLeagues.ExecuteScalar());
                        lblLeagues.Text = leaguesCount.ToString();
                    }

                    // 4. Equipos Añadidos (Tabla: teams)
                    using (MySqlCommand cmdTeams = new MySqlCommand("SELECT COUNT(*) FROM teams", con))
                    {
                        int teamsCount = Convert.ToInt32(cmdTeams.ExecuteScalar());
                        lblTeams.Text = teamsCount.ToString();
                    }
                    // 5. Total Ganancias removed from dashboard

                    // 6. Pedidos últimos 7 días (Tabla: orders, Columna: OrderDate)
                    string queryLast7Days = "SELECT COUNT(*) FROM orders WHERE OrderDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)";
                    using (MySqlCommand cmd7Days = new MySqlCommand(queryLast7Days, con))
                    {
                        int orders7Days = Convert.ToInt32(cmd7Days.ExecuteScalar());
                        lblPurchasesLast7Days.Text = orders7Days.ToString();
                    }

                    // 7. Pedidos Pendientes (Tabla: orders, Id_Status = 1 es 'Pending')
                    string queryPending = "SELECT COUNT(*) FROM orders WHERE Id_Status = 1";
                    using (MySqlCommand cmdPending = new MySqlCommand(queryPending, con))
                    {
                        int pendingCount = Convert.ToInt32(cmdPending.ExecuteScalar());
                        lblPendingOrders.Text = pendingCount.ToString();
                    }

                    // 8. Total Procesados (Tabla: orders - cuenta histórica total)
                    using (MySqlCommand cmdTotalOrders = new MySqlCommand("SELECT COUNT(*) FROM orders", con))
                    {
                        int totalOrders = Convert.ToInt32(cmdTotalOrders.ExecuteScalar());
                        lblTotalOrders.Text = totalOrders.ToString();
                    }

                    // 9. Camiseta Más Vendida (Tabla: order_details, Columnas: ProductName, Quantity)
                    bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                    string queryTopShirt = @"
                        SELECT 
                            CASE 
                                WHEN @Lang = 'es' AND tt.Name IS NOT NULL AND tt.Name != '' THEN tt.Name 
                                ELSE od.ProductName 
                            END AS TopShirtName
                        FROM order_details od
                        LEFT JOIN tshirts t ON od.ProductName = t.Name
                        LEFT JOIN tshirt_translations tt ON t.ID = tt.Id_Tshirt AND tt.LanguageCode = 'es'
                        GROUP BY od.ProductName, TopShirtName
                        ORDER BY SUM(od.Quantity) DESC 
                        LIMIT 1;";

                    using (MySqlCommand cmdTopShirt = new MySqlCommand(queryTopShirt, con))
                    {
                        cmdTopShirt.Parameters.AddWithValue("@Lang", isSpanish ? "es" : "en");
                        object result = cmdTopShirt.ExecuteScalar();
                        if (result != null && result != DBNull.Value && !string.IsNullOrWhiteSpace(result.ToString()))
                        {
                            lblTopShirt.Text = result.ToString();
                        }
                        else
                        {
                            lblTopShirt.Text = isSpanish ? "Ninguno aún" : "None yet";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                // Respaldo seguro para evitar caídas de interfaz
                lblUserCount.Text = "0";
                lblShirtCount.Text = "0";
                lblLeagues.Text = "0";
                lblTeams.Text = "0";
                // lblTotalRevenue removed
                lblPurchasesLast7Days.Text = "0";
                lblPendingOrders.Text = "0";
                lblTotalOrders.Text = "0";
                lblTopShirt.Text = isSpanish ? "Error al cargar" : "Error loading";

                System.Diagnostics.Debug.WriteLine("Error crítico en estadísticas del Dashboard: " + ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Carga de Banners dinámicos para el Preview del Carousel
        // ──────────────────────────────────────────────────────────────
        private void LoadDashBanners()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT ID, Title, Subtitle, ImageURL, LinkURL, SortOrder " +
                        "FROM banners WHERE IsActive = 1 ORDER BY SortOrder ASC;", con);

                    DataTable dt = new DataTable();
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    // Prefijar rutas si guardas nombres de archivos locales en lugar de URLs absolutas
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
                        phDashCarousel.Visible = false;
                        phDashNoBanners.Visible = true;
                    }
                    else
                    {
                        phDashCarousel.Visible = true;
                        phDashNoBanners.Visible = false;
                        rptDashIndicators.DataSource = dt;
                        rptDashIndicators.DataBind();
                        rptDashBanners.DataSource = dt;
                        rptDashBanners.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al cargar los banners del dashboard: " + ex.Message);
                phDashCarousel.Visible = false;
                phDashNoBanners.Visible = true;
            }
        }

        protected string BuildDashBannerImage(string imageUrl, string title, string linkUrl)
        {
            string imgTag = $"<img src='{HttpUtility.HtmlEncode(imageUrl)}' alt='{HttpUtility.HtmlEncode(title)}' />";
            if (!string.IsNullOrWhiteSpace(linkUrl))
                return $"<a href='{HttpUtility.HtmlEncode(linkUrl)}'>{imgTag}</a>";
            return imgTag;
        }

        private void LoadRecentOrdersGrid()
        {
            try
            {
                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                string statusColumn = isSpanish ? "s.Status_Name_es" : "s.Status_Name";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = $@"
                        SELECT o.Id_Order, o.Name, o.LastName, c.city_name AS City, o.Total, {statusColumn} AS Status_Name
                        FROM orders o
                        LEFT JOIN cities c ON o.Id_City = c.id_city
                        INNER JOIN order_statuses s ON o.Id_Status = s.Id_Status
                        ORDER BY o.OrderDate DESC
                        LIMIT 5;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        DataTable dt = new DataTable();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        gvRecentOrders.DataSource = dt;
                        gvRecentOrders.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading recent orders grid: " + ex.Message);
            }
        }

        private void LoadCriticalStockGrid()
        {
            try
            {
                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                string shirtNameCol = isSpanish ? "COALESCE(tt.Name, t.Name)" : "t.Name";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = $@"
                        SELECT {shirtNameCol} AS ShirtName, s.Size_Code AS SizeName, v.Stock
                        FROM tshirt_variants v
                        INNER JOIN tshirts t ON v.Id_Tshirt = t.ID
                        LEFT JOIN tshirt_translations tt ON t.ID = tt.Id_Tshirt AND tt.LanguageCode = 'es'
                        INNER JOIN sizes s ON v.Id_Size = s.Id_Size
                        WHERE v.Stock <= 2
                        ORDER BY v.Stock ASC;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        DataTable dt = new DataTable();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        gvCriticalStock.DataSource = dt;
                        gvCriticalStock.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading critical stock grid: " + ex.Message);
            }
        }

        private void LoadPendingTicketsGrid()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT t.Id_Ticket, r.Id_ContactReason, r.Reason_Name, t.Subject, t.User_Email
                        FROM contact_tickets t
                        INNER JOIN contact_reasons r ON t.Id_ContactReason = r.Id_ContactReason
                        WHERE t.Status = 1
                        ORDER BY t.Id_Ticket DESC;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        DataTable dt = new DataTable();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        foreach (DataRow dr in dt.Rows)
                        {
                            string key = "Reason_" + dr["Id_ContactReason"];
                            string locName = AlertHelper.GetResourceString(this, key);
                            if (!string.IsNullOrEmpty(locName) && !locName.StartsWith("[Resource"))
                            {
                                dr["Reason_Name"] = locName;
                            }
                        }

                        gvPendingTickets.DataSource = dt;
                        gvPendingTickets.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading pending tickets grid: " + ex.Message);
            }
        }

        private void LoadAuditLogsGrid()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT l.Created_At, u.Name_User AS Operator, l.Module, l.Description
                        FROM activity_logs l
                        INNER JOIN users u ON l.Id_User = u.Id_User
                        ORDER BY l.Id_Log DESC
                        LIMIT 5;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        DataTable dt = new DataTable();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        gvAuditLogs.DataSource = dt;
                        gvAuditLogs.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading audit logs grid: " + ex.Message);
            }
        }

        protected string GetStatusBadgeClass(string statusName)
        {
            if (string.IsNullOrEmpty(statusName)) return "badge badge-secondary";
            
            switch (statusName.ToLower())
            {
                case "pending":
                case "pendiente":
                    return "badge badge-warning text-dark font-weight-bold px-3 py-2";
                case "paid":
                case "pagado":
                    return "badge badge-primary font-weight-bold px-3 py-2";
                case "shipped":
                case "enviado":
                    return "badge badge-info font-weight-bold px-3 py-2";
                case "delivered":
                case "entregado":
                    return "badge badge-success font-weight-bold px-3 py-2";
                case "refund requested":
                case "reembolso solicitado":
                    return "badge badge-danger font-weight-bold px-3 py-2";
                case "refunded":
                case "reembolsado":
                    return "badge badge-secondary font-weight-bold px-3 py-2";
                default:
                    return "badge badge-secondary font-weight-bold px-3 py-2";
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Eventos de Navegación del Panel de Control
        // ──────────────────────────────────────────────────────────────
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
        {
            Response.Redirect("ManageCoupons.aspx");
        }
        protected void btnAuditLogs_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminAudit.aspx");
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
    }
}