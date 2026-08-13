using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net; // Solución al problema de WebClient
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Layout.Borders;

// Definición estricta de Alias para evitar colisiones con System.Web.UI.WebControls
using ITextTable = iText.Layout.Element.Table;
using ITextCell = iText.Layout.Element.Cell;
using ITextImage = iText.Layout.Element.Image;
using ITextParagraph = iText.Layout.Element.Paragraph;

namespace OFFSIDESHOP
{
    public partial class AdminStats : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1500;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 1)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            Security.ConfigureAdminSidebar(this);
            if (phOwnerMenu != null)
            {
                phOwnerMenu.Visible = (Convert.ToInt32(Session["UserRole"]) == 1);
            }

            if (!IsPostBack)
            {
                LoadKPIs();
                LoadChartsData();
            }
        }

        private void LoadKPIs()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            (SELECT IFNULL(SUM(Total), 0) FROM orders WHERE Id_Status IN (2, 3, 4)) AS TotalRevenue,
                            (SELECT COUNT(Id_Order) FROM orders) AS TotalOrders,
                            (SELECT COUNT(Id_User) FROM users WHERE Id_Role = 3) AS TotalCustomers,
                            (SELECT COUNT(Id_Order) FROM orders WHERE Id_Status = 1) AS PendingOrders;
                    ";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblTotalRevenue.Text = Convert.ToDecimal(reader["TotalRevenue"]).ToString("C");
                                lblTotalOrders.Text = reader["TotalOrders"].ToString();
                                lblTotalUsers.Text = reader["TotalCustomers"].ToString();
                                lblPendingOrders.Text = reader["PendingOrders"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private void LoadChartsData()
        {
            try
            {
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string queryRevenue = @"SELECT DATE_FORMAT(OrderDate, '%b %d') as OrderDay, SUM(Total) as DailyRevenue FROM orders WHERE Id_Status IN (2, 3, 4) AND OrderDate >= DATE_SUB(CURDATE(), INTERVAL 7 DAY) GROUP BY DATE(OrderDate) ORDER BY DATE(OrderDate) ASC;";
                    List<string> revDates = new List<string>();
                    List<decimal> revData = new List<decimal>();

                    using (MySqlCommand cmd = new MySqlCommand(queryRevenue, con))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string dayLabel = reader["OrderDay"].ToString();
                                if (currentLang == "es")
                                {
                                    dayLabel = dayLabel.Replace("Jan", "Ene")
                                                       .Replace("Apr", "Abr")
                                                       .Replace("Aug", "Ago")
                                                       .Replace("Dec", "Dic");
                                }
                                revDates.Add("\"" + dayLabel + "\"");
                                revData.Add(Convert.ToDecimal(reader["DailyRevenue"]));
                            }
                        }
                    }
                    hfRevenueDates.Value = "[" + string.Join(",", revDates) + "]";
                    hfRevenueData.Value = "[" + string.Join(",", revData.Select(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";

                    string queryStatus = @"
                        SELECT 
                            CASE 
                                WHEN @Lang = 'es' THEN COALESCE(os.Status_Name_es, os.Status_Name)
                                ELSE os.Status_Name 
                            END AS Status_Name, 
                            COUNT(o.Id_Order) as Count 
                        FROM orders o 
                        JOIN order_statuses os ON o.Id_Status = os.Id_Status 
                        GROUP BY os.Id_Status, os.Status_Name, os.Status_Name_es;";
                    List<string> statLabels = new List<string>();
                    List<int> statData = new List<int>();

                    using (MySqlCommand cmd = new MySqlCommand(queryStatus, con))
                    {
                        cmd.Parameters.AddWithValue("@Lang", currentLang);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                statLabels.Add("\"" + reader["Status_Name"].ToString() + "\"");
                                statData.Add(Convert.ToInt32(reader["Count"]));
                            }
                        }
                    }
                    hfStatusLabels.Value = "[" + string.Join(",", statLabels) + "]";
                    hfStatusData.Value = "[" + string.Join(",", statData) + "]";

                    string queryTop = @"
                        SELECT 
                            CASE 
                                WHEN @Lang = 'es' THEN COALESCE(tt.Name, t.Name)
                                ELSE t.Name 
                            END AS ProductName, 
                            SUM(od.Quantity) as Qty 
                        FROM order_details od 
                        INNER JOIN tshirts t ON od.Id_Tshirt = t.ID 
                        LEFT JOIN tshirt_translations tt ON t.ID = tt.Id_Tshirt AND tt.LanguageCode = 'es' 
                        GROUP BY t.ID, t.Name, tt.Name 
                        ORDER BY Qty DESC LIMIT 5;";
                    List<string> topLabels = new List<string>();
                    List<int> topData = new List<int>();

                    using (MySqlCommand cmd = new MySqlCommand(queryTop, con))
                    {
                        cmd.Parameters.AddWithValue("@Lang", currentLang);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string safeName = reader["ProductName"].ToString().Replace("\"", "\\\"");
                                topLabels.Add("\"" + safeName + "\"");
                                topData.Add(Convert.ToInt32(reader["Qty"]));
                            }
                        }
                    }
                    hfTopProductsLabels.Value = "[" + string.Join(",", topLabels) + "]";
                    hfTopProductsData.Value = "[" + string.Join(",", topData) + "]";
                }
            }
            catch (Exception)
            {
                hfRevenueDates.Value = "[]"; hfRevenueData.Value = "[]";
                hfStatusLabels.Value = "[]"; hfStatusData.Value = "[]";
                hfTopProductsLabels.Value = "[]"; hfTopProductsData.Value = "[]";
            }
        }

        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";
            bool isSpanish = (currentLang == "es");

            string period = ddlReportPeriod.SelectedValue;
            int intervalDays = (period == "MONTH") ? 30 : (period == "YEAR") ? 365 : 7;

            string periodText = isSpanish
                ? ((period == "MONTH") ? "Reporte Mensual" : (period == "YEAR") ? "Reporte Anual" : "Reporte Semanal")
                : ((period == "MONTH") ? "Monthly Report" : (period == "YEAR") ? "Annual Report" : "Weekly Report");

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
                doc.SetMargins(40, 40, 40, 40);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // --- 1. HEADER (Logo + Textos) usando Alias ---
                ITextTable headerTable = new ITextTable(new float[] { 1, 3 }).UseAllAvailableWidth().SetMarginBottom(20);

                ITextCell logoCell = new ITextCell().SetBorder(Border.NO_BORDER).SetPadding(10).SetTextAlignment(TextAlignment.CENTER);
                string logoPath = Server.MapPath("~/assets/img/newlogo_nosv.png");
                if (File.Exists(logoPath))
                {
                    ITextImage logo = new ITextImage(ImageDataFactory.Create(logoPath)).SetAutoScale(true);
                    logoCell.Add(logo);
                }
                else
                {
                    logoCell.Add(new ITextParagraph("LOGO").SetFontColor(ColorConstants.WHITE));
                }

                ITextCell textCell = new ITextCell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE);
                textCell.Add(new ITextParagraph("OFFSIDE SHOP S.A DE C.V").SetFont(boldFont).SetFontSize(18));
                textCell.Add(new ITextParagraph(isSpanish ? "REPORTE DE VENTAS" : "SALES REPORT").SetFont(boldFont).SetFontSize(14));
                textCell.Add(new ITextParagraph(isSpanish ? "Gerente: ADMINISTRADOR" : "Manager: ADMINISTRATOR").SetFont(normalFont).SetFontSize(11));
                textCell.Add(new ITextParagraph(isSpanish ? $"Período: {periodText} | Fecha: {DateTime.Now:dd/MM/yyyy}" : $"Period: {periodText} | Date: {DateTime.Now:MM/dd/yyyy}").SetFont(normalFont).SetFontSize(10));

                headerTable.AddCell(logoCell);
                headerTable.AddCell(textCell);
                doc.Add(headerTable);

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // --- 2. ALL PRODUCTS SOLD ---
                    AddSectionTitle(doc, isSpanish ? "1. Rendimiento de Ventas de Productos" : "1. Product Sales Performance", boldFont);
                    string queryProducts = @"SELECT 
                                                 CASE 
                                                     WHEN @Lang = 'es' THEN COALESCE(tt.Name, t.Name)
                                                     ELSE t.Name 
                                                 END AS Name, 
                                                 SUM(od.Quantity) AS Qty 
                                             FROM order_details od 
                                             INNER JOIN orders o ON od.Id_Order = o.Id_Order 
                                             INNER JOIN tshirts t ON od.Id_Tshirt = t.ID 
                                             LEFT JOIN tshirt_translations tt ON t.ID = tt.Id_Tshirt AND tt.LanguageCode = 'es' 
                                             WHERE o.Id_Status IN (2, 3, 4) AND o.OrderDate >= DATE_SUB(CURDATE(), INTERVAL @Days DAY) 
                                             GROUP BY t.ID, t.Name, tt.Name ORDER BY Qty DESC;";

                    List<string> prodLabels = new List<string>();
                    List<decimal> prodData = new List<decimal>();
                    string[] prodHeaders = isSpanish 
                        ? new string[] { "Posición", "Nombre de Camiseta", "Unidades Vendidas" }
                        : new string[] { "Rank", "Shirt Name", "Units Sold" };
                    ITextTable prodTable = CreateBaseTable(prodHeaders, boldFont);

                    int rank = 1;
                    using (MySqlCommand cmd = new MySqlCommand(queryProducts, con))
                    {
                        cmd.Parameters.AddWithValue("@Days", intervalDays);
                        cmd.Parameters.AddWithValue("@Lang", currentLang);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader["Name"].ToString();
                                decimal qty = Convert.ToDecimal(reader["Qty"]);
                                prodLabels.Add(name);
                                prodData.Add(qty);

                                prodTable.AddCell(CreateCell(rank.ToString(), normalFont, TextAlignment.CENTER));
                                prodTable.AddCell(CreateCell(name, normalFont, TextAlignment.LEFT));
                                prodTable.AddCell(CreateCell(qty.ToString(), normalFont, TextAlignment.CENTER));
                                rank++;
                            }
                        }
                    }
                    doc.Add(prodTable);


                    // --- 3. LEAGUES PERFORMANCE ---
                    AddSectionTitle(doc, isSpanish ? "2. Rendimiento por Ligas" : "2. Top Leagues Performance", boldFont);
                    string queryLeagues = @"SELECT l.Id_League, l.Name_League, SUM(od.Quantity) AS Qty 
                                            FROM order_details od 
                                            INNER JOIN orders o ON od.Id_Order = o.Id_Order 
                                            INNER JOIN tshirts t ON od.Id_Tshirt = t.ID 
                                            INNER JOIN teams tm ON t.Id_Team = tm.Id_Team 
                                            INNER JOIN leagues l ON tm.Id_League = l.Id_League 
                                            WHERE o.Id_Status IN (2, 3, 4) AND o.OrderDate >= DATE_SUB(CURDATE(), INTERVAL @Days DAY) 
                                            GROUP BY l.Id_League, l.Name_League ORDER BY Qty DESC;";

                    List<string> legLabels = new List<string>();
                    List<decimal> legData = new List<decimal>();
                    List<int> leagueIds = new List<int>();
                    string[] legHeaders = isSpanish 
                        ? new string[] { "Nombre de Liga", "Unidades Vendidas" }
                        : new string[] { "League Name", "Units Sold" };
                    ITextTable legTable = CreateBaseTable(legHeaders, boldFont);

                    using (MySqlCommand cmd = new MySqlCommand(queryLeagues, con))
                    {
                        cmd.Parameters.AddWithValue("@Days", intervalDays);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                leagueIds.Add(Convert.ToInt32(reader["Id_League"]));
                                legLabels.Add(reader["Name_League"].ToString());
                                legData.Add(Convert.ToDecimal(reader["Qty"]));

                                legTable.AddCell(CreateCell(reader["Name_League"].ToString(), normalFont, TextAlignment.LEFT));
                                legTable.AddCell(CreateCell(reader["Qty"].ToString(), normalFont, TextAlignment.CENTER));
                            }
                        }
                    }
                    doc.Add(legTable);

                    if (legLabels.Count > 0)
                    {
                        ITextImage chart = GenerateChartImage("pie", legLabels, legData, isSpanish ? "Participación por Ligas" : "League Share");
                        if (chart != null) doc.Add(chart);
                    }

                    // --- 4. TEAMS (Per League) ---
                    AddSectionTitle(doc, isSpanish ? "3. Desglose de Ventas por Equipos" : "3. Team Sales Breakdown by League", boldFont);
                    foreach (int idLeague in leagueIds)
                    {
                        string queryTeams = @"SELECT l.Name_League, tm.Name_Team, SUM(od.Quantity) AS Qty 
                                              FROM order_details od 
                                              INNER JOIN orders o ON od.Id_Order = o.Id_Order 
                                              INNER JOIN tshirts t ON od.Id_Tshirt = t.ID 
                                              INNER JOIN teams tm ON t.Id_Team = tm.Id_Team 
                                              INNER JOIN leagues l ON tm.Id_League = l.Id_League 
                                              WHERE o.Id_Status IN (2, 3, 4) AND l.Id_League = @LeagueId AND o.OrderDate >= DATE_SUB(CURDATE(), INTERVAL @Days DAY) 
                                              GROUP BY tm.Id_Team, tm.Name_Team, l.Name_League ORDER BY Qty DESC;";

                        List<string> teamLabels = new List<string>();
                        List<decimal> teamData = new List<decimal>();
                        string currentLeagueName = "";

                        using (MySqlCommand cmd = new MySqlCommand(queryTeams, con))
                        {
                            cmd.Parameters.AddWithValue("@LeagueId", idLeague);
                            cmd.Parameters.AddWithValue("@Days", intervalDays);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    currentLeagueName = reader["Name_League"].ToString();
                                    teamLabels.Add(reader["Name_Team"].ToString());
                                    teamData.Add(Convert.ToDecimal(reader["Qty"]));
                                }
                            }
                        }

                        if (teamLabels.Count > 0)
                        {
                            doc.Add(new ITextParagraph(isSpanish ? $"Liga: {currentLeagueName}" : $"League: {currentLeagueName}").SetFont(boldFont).SetFontSize(12).SetMarginTop(10));
                            string[] teamHeaders = isSpanish 
                                ? new string[] { "Nombre de Equipo", "Unidades Vendidas" }
                                : new string[] { "Team Name", "Units Sold" };
                            ITextTable teamTable = CreateBaseTable(teamHeaders, boldFont);
                            for (int i = 0; i < teamLabels.Count; i++)
                            {
                                teamTable.AddCell(CreateCell(teamLabels[i], normalFont, TextAlignment.LEFT));
                                teamTable.AddCell(CreateCell(teamData[i].ToString(), normalFont, TextAlignment.CENTER));
                            }
                            doc.Add(teamTable);

                            ITextImage chart = GenerateChartImage("bar", teamLabels, teamData, isSpanish ? $"{currentLeagueName} - Equipos Top" : $"{currentLeagueName} - Top Teams");
                            if (chart != null) doc.Add(chart);
                        }
                    }

                    // --- 5. BRANDS ---
                    AddSectionTitle(doc, isSpanish ? "4. Cuota de Mercado de Marcas" : "4. Brand Market Share", boldFont);
                    string queryBrands = @"SELECT b.Name_Brand, SUM(od.Quantity) AS Qty 
                                           FROM order_details od 
                                           INNER JOIN orders o ON od.Id_Order = o.Id_Order 
                                           INNER JOIN tshirts t ON od.Id_Tshirt = t.ID 
                                           INNER JOIN brands b ON t.Id_Brand = b.Id_Brand 
                                           WHERE o.Id_Status IN (2, 3, 4) AND o.OrderDate >= DATE_SUB(CURDATE(), INTERVAL @Days DAY) 
                                           GROUP BY b.Id_Brand, b.Name_Brand ORDER BY Qty DESC;";

                    List<string> brandLabels = new List<string>();
                    List<decimal> brandData = new List<decimal>();
                    string[] brandHeaders = isSpanish 
                        ? new string[] { "Marca", "Unidades Vendidas" }
                        : new string[] { "Brand", "Units Sold" };
                    ITextTable brandTable = CreateBaseTable(brandHeaders, boldFont);

                    using (MySqlCommand cmd = new MySqlCommand(queryBrands, con))
                    {
                        cmd.Parameters.AddWithValue("@Days", intervalDays);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                brandLabels.Add(reader["Name_Brand"].ToString());
                                brandData.Add(Convert.ToDecimal(reader["Qty"]));
                                brandTable.AddCell(CreateCell(reader["Name_Brand"].ToString(), normalFont, TextAlignment.LEFT));
                                brandTable.AddCell(CreateCell(reader["Qty"].ToString(), normalFont, TextAlignment.CENTER));
                            }
                        }
                    }
                    doc.Add(brandTable);

                    if (brandLabels.Count > 0)
                    {
                        ITextImage chart = GenerateChartImage("pie", brandLabels, brandData, isSpanish ? "Desglose por Marcas" : "Brands Breakdown");
                        if (chart != null) doc.Add(chart);
                    }

                    // --- 6. MONTHLY REVENUE (ONLY FOR YEAR) ---
                    if (period == "YEAR")
                    {
                        AddSectionTitle(doc, isSpanish ? "5. Ingresos Brutos Anuales por Mes" : "5. Annual Gross Revenue by Month", boldFont);
                        string queryMonthly = @"SELECT DATE_FORMAT(o.OrderDate, '%Y-%m') AS MonthSort, DATE_FORMAT(o.OrderDate, '%b %Y') AS MonthName, SUM(o.Total) AS TotalRevenue 
                                                FROM orders o 
                                                WHERE o.Id_Status IN (2, 3, 4) AND o.OrderDate >= DATE_SUB(CURDATE(), INTERVAL 365 DAY) 
                                                GROUP BY DATE_FORMAT(o.OrderDate, '%Y-%m'), DATE_FORMAT(o.OrderDate, '%b %Y') 
                                                ORDER BY MonthSort ASC;";

                        List<string> monthLabels = new List<string>();
                        List<decimal> monthData = new List<decimal>();
                        string[] monthHeaders = isSpanish 
                            ? new string[] { "Mes", "Ingresos Brutos" }
                            : new string[] { "Month", "Gross Revenue" };
                        ITextTable monthTable = CreateBaseTable(monthHeaders, boldFont);

                        using (MySqlCommand cmd = new MySqlCommand(queryMonthly, con))
                        {
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string mName = reader["MonthName"].ToString();
                                    if (isSpanish)
                                    {
                                        mName = mName.Replace("Jan", "Ene")
                                                     .Replace("Apr", "Abr")
                                                     .Replace("Aug", "Ago")
                                                     .Replace("Dec", "Dic");
                                    }
                                    monthLabels.Add(mName);
                                    decimal monthlyRev = Convert.ToDecimal(reader["TotalRevenue"]);
                                    monthData.Add(monthlyRev);
                                    monthTable.AddCell(CreateCell(mName, normalFont, TextAlignment.LEFT));
                                    monthTable.AddCell(CreateCell(monthlyRev.ToString("C"), normalFont, TextAlignment.RIGHT));
                                }
                            }
                        }
                        doc.Add(monthTable);

                        if (monthLabels.Count > 0)
                        {
                            ITextImage chart = GenerateChartImage("line", monthLabels, monthData, isSpanish ? "Historial de Ingresos ($)" : "Revenue History ($)");
                            if (chart != null) doc.Add(chart);
                        }
                    }
                }

                doc.Close();

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename=OFFSIDESHOP_SalesReport_{DateTime.Now:yyyyMMdd}.pdf");
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }

        // --- HELPER METHODS FOR PDF UTILIZANDO ALIASES ---
        private void AddSectionTitle(Document doc, string title, PdfFont font)
        {
            doc.Add(new ITextParagraph("\n"));
            doc.Add(new ITextParagraph(title)
                .SetFont(font)
                .SetFontSize(14)
                .SetFontColor(new DeviceRgb(255, 200, 0)) // #FFC800
                .SetMarginBottom(10)
                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1)));
        }

        private ITextTable CreateBaseTable(string[] headers, PdfFont font)
        {
            ITextTable table = new ITextTable(headers.Length).UseAllAvailableWidth().SetMarginBottom(10);
            foreach (string h in headers)
            {
                table.AddHeaderCell(new ITextCell().Add(new ITextParagraph(h).SetFont(font))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetPadding(5)
                    .SetTextAlignment(TextAlignment.CENTER));
            }
            return table;
        }

        private ITextCell CreateCell(string text, PdfFont font, TextAlignment align)
        {
            return new ITextCell().Add(new ITextParagraph(text).SetFont(font))
                                 .SetPadding(5)
                                 .SetTextAlignment(align);
        }

        private ITextImage GenerateChartImage(string chartType, List<string> labels, List<decimal> data, string datasetLabel)
        {
            try
            {
                var chartLabels = labels.Take(10).Select(l => "'" + l.Replace("'", "").Replace("\"", "") + "'").ToList();
                var chartData = data.Take(10).ToList();

                string labelsStr = string.Join(",", chartLabels);
                string dataStr = string.Join(",", chartData.Select(d => d.ToString(System.Globalization.CultureInfo.InvariantCulture)));

                string config = $"{{type:'{chartType}',data:{{labels:[{labelsStr}],datasets:[{{label:'{datasetLabel}',data:[{dataStr}]}}]}}}}";
                string url = "https://quickchart.io/chart?w=500&h=250&bkg=white&c=" + Uri.EscapeDataString(config);

                using (var wc = new WebClient())
                {
                    byte[] imageBytes = wc.DownloadData(url);
                    ImageData imageData = ImageDataFactory.Create(imageBytes);
                    ITextImage img = new ITextImage(imageData).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(20);
                    return img;
                }
            }
            catch
            {
                return null; // En caso de caída de la API externa, no rompe la generación del reporte principal.
            }
        }

        // --- RUTAS COMUNES ---
        protected void btnManageProducts_Click(object sender, EventArgs e) { Response.Redirect("ManageProducts.aspx"); }
        protected void btnManageOffers_Click(object sender, EventArgs e) { Response.Redirect("ManageOffers.aspx"); }
        protected void btncerrar_Click(object sender, EventArgs e) { Session.Clear(); Session.Abandon(); Response.Redirect("Login.aspx"); }
        protected void btnAddLeague_Click(object sender, EventArgs e) { Response.Redirect("AddLeague.aspx"); }
        protected void btnAddTeam_Click(object sender, EventArgs e) { Response.Redirect("AddTeam.aspx"); }
        protected void btnAddBrand_Click(object sender, EventArgs e) { Response.Redirect("AddBrand.aspx"); }
        protected void btnManageUsers_Click(object sender, EventArgs e) { Response.Redirect("ManageUsers.aspx"); }
        protected void btnSmtpSettings_Click(object sender, EventArgs e) { Response.Redirect("SmtpSettings.aspx"); }
        protected void btnStats_Click(object sender, EventArgs e) { Response.Redirect("AdminStats.aspx"); }
        protected void btnAdminBanners_Click(object sender, EventArgs e) { Response.Redirect("AdminBanners.aspx"); }
        protected void btnManageCoupons_Click(object sender, EventArgs e) { Response.Redirect("ManageCoupons.aspx"); }
        protected void btnAuditLogs_Click(object sender, EventArgs e) { Response.Redirect("AdminAudit.aspx"); }
        protected override void InitializeCulture()
        {
            string lang = Session["Language"] != null ? Session["Language"].ToString() : "en";
            string cultureName = (lang == "es") ? "es-SV" : "en-US";
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(cultureName);
            ci.NumberFormat.CurrencySymbol = "$";
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            base.InitializeCulture();
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }
    }
}