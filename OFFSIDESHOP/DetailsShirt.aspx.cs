using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace OFFSIDESHOP
{
    public partial class DetailsShirt : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            phNavbarGuest.Visible = false;
            phNavbarUser.Visible = false;
            phNavbarAdmin.Visible = false;

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
                }
                else
                {
                    phNavbarGuest.Visible = true;
                }
            }

            if (!IsPostBack)
            {
                // Restaurar la talla si venía de un intento previo, de lo contrario limpiarla
                if (Session["PendingSizeId"] != null)
                {
                    Session["SelectedSizeId"] = Session["PendingSizeId"];
                    Session.Remove("PendingSizeId"); // Limpiamos para que no afecte futuras visitas
                }
                else
                {
                    Session["SelectedSizeId"] = null;
                }

                LoadFilterDropdowns();

                string idParam = Request.QueryString["id"];
                if (string.IsNullOrEmpty(idParam) || !int.TryParse(idParam, out int id))
                {
                    Response.Redirect("Homepage.aspx");
                    return;
                }

                LoadShirtDetails(id);
                LoadSizes(id);
                LoadSimilarShirts(id);
                ActualizarContadorCarrito();
                CargarDatosPerfilUsuario();
                CargarReviews();
                // ==========================================
                // RESTAURAR DATOS DE PERSONALIZACIÓN
                // ==========================================
                if (Session["PendingIsCustom"] != null)
                {
                    bool isCustom = Convert.ToBoolean(Session["PendingIsCustom"]);
                    string pendingName = Session["PendingCustomName"]?.ToString() ?? "";
                    string pendingNum = Session["PendingCustomNumber"]?.ToString() ?? "";

                    if (isCustom || !string.IsNullOrEmpty(pendingName))
                    {
                        // 1. Marcar los valores en el servidor
                        chkCustomize.Checked = true;
                        txtCustomName.Text = pendingName;
                        txtCustomNumber.Text = pendingNum;

                        // 2. Inyectamos el script con un retraso estratégico para ganarle a $(document).ready
                        string jsRestore = $@"
            setTimeout(function() {{
                // Llenamos las cajas
                var txtName = document.getElementById('txtCustomName');
                if (txtName) txtName.value = '{pendingName}';
                
                var txtNum = document.getElementById('txtCustomNumber');
                if (txtNum) txtNum.value = '{pendingNum}';

                var chk = document.getElementById('chkCustomize');
                if (chk) chk.checked = true;

                // Ejecutamos tu función para que muestre la caja y aplique el precio
                if (typeof togglePersonalizacion === 'function') {{
                    togglePersonalizacion();
                }}

                // MAGIA: Obligamos a jQuery a procesar el texto como si el usuario lo hubiera tecleado
                if (window.jQuery) {{
                    if (txtName) $(txtName).trigger('input');
                    if (txtNum) $(txtNum).trigger('input');
                }}
            }}, 300); // 300 milisegundos aseguran que tu frontend ya se haya reiniciado
        ";
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "forceUI", jsRestore, true);
                    }

                    if (Session["PendingQuantity"] != null)
                    {
                        hfQuantity.Value = Session["PendingQuantity"].ToString();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "restoreQty",
                            $"setTimeout(function() {{ var qty = document.getElementById('txtDisplayQty'); if (qty) qty.value = {Session["PendingQuantity"]}; }}, 300);", true);
                    }

                    // Limpiamos memoria
                    Session.Remove("PendingIsCustom");
                    Session.Remove("PendingCustomName");
                    Session.Remove("PendingCustomNumber");
                    Session.Remove("PendingQuantity");
                }

                // Lógica de reviews...
                bool isLoggedIn = (Session["UserRole"] != null && Session["Id_User"] != null);
                bool hasPurchased = isLoggedIn && HasUserPurchasedShirt(Convert.ToInt32(Session["Id_User"]), id);

                phLeaveReview.Visible = isLoggedIn && hasPurchased;
                phMustPurchaseToReview.Visible = isLoggedIn && !hasPurchased;
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

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
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

            if (upPerfil != null)
            {
                upPerfil.Update();
            }
        }

        private void LoadFilterDropdowns()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmdL = new MySqlCommand("SELECT Id_League, Name_League FROM leagues ORDER BY Name_League ASC;", con);
                    using (MySqlDataReader rdr = cmdL.ExecuteReader())
                    {
                        ddlLeague.Items.Clear();
                        ddlLeague.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Search_AllLeagues"), ""));
                        while (rdr.Read())
                        {
                            ddlLeague.Items.Add(new ListItem(rdr["Name_League"].ToString(), rdr["Id_League"].ToString()));
                        }
                    }

                    MySqlCommand cmdB = new MySqlCommand("SELECT Id_Brand, Name_Brand FROM brands ORDER BY Name_Brand ASC;", con);
                    using (MySqlDataReader rdr = cmdB.ExecuteReader())
                    {
                        ddlBrand.Items.Clear();
                        ddlBrand.Items.Add(new ListItem(AlertHelper.GetResourceString(this, "Search_AllBrands"), ""));
                        while (rdr.Read())
                        {
                            ddlBrand.Items.Add(new ListItem(rdr["Name_Brand"].ToString(), rdr["Id_Brand"].ToString()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dropdown filters: " + ex.Message);
            }
        }

        private string ResolveImageUrl(string img)
        {
            if (string.IsNullOrWhiteSpace(img))
            {
                return "assets/img/default-product.jpg";
            }
            if (img.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                img.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) ||
                img.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            {
                return img;
            }
            return "images/camisetas/" + img;
        }

        private void LoadShirtDetails(int shirtId)
        {
            try
            {
                // 1. Obtenemos el idioma actual seleccionado (ej: "es" o "en")
                string currentLang = Session["Language"] != null ? Session["Language"].ToString() : "en";

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    // 2. Query actualizada con LEFT JOIN a tshirt_translations y COALESCE
                    string query = @"
            SELECT t.ID, 
                   COALESCE(tr.Name, t.Name) AS Name, 
                   COALESCE(tr.Description, t.Description) AS Description, 
                   t.ImageURL, t.Year,
                   t.ImageURL2, t.ImageURL3, t.ImageURL4, t.ImageURL5,
                   t.IsCustomizable, t.Id_Brand,
                   t.Price AS OriginalPrice,
                   CASE 
                       WHEN o.Id_Offer IS NOT NULL THEN (t.Price - (t.Price * (o.DiscountPercentage / 100.0))) 
                       ELSE t.Price 
                   END AS FinalPrice,
                   CASE WHEN o.Id_Offer IS NOT NULL THEN 1 ELSE 0 END AS IsOnSale,
                   IFNULL(o.DiscountPercentage, 0) AS DiscountPercentage,
                   COALESCE(b.Name_Brand, 'OffsideBrand') AS Brand, 
                   COALESCE(tm.Name_Team, 'OffsideTeam') AS Team, 
                   CASE 
                       WHEN @Lang = 'es' THEN COALESCE(kt.Name_KitType_es, kt.Name_KitType, 'Edición Especial')
                       ELSE COALESCE(kt.Name_KitType, 'Special Edition')
                   END AS Type
            FROM tshirts t
            LEFT JOIN tshirt_translations tr ON t.ID = tr.Id_Tshirt AND tr.LanguageCode = @Lang
            LEFT JOIN brands b ON t.Id_Brand = b.Id_Brand
            LEFT JOIN teams tm ON t.Id_Team = tm.Id_Team
            LEFT JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
            LEFT JOIN offer_tshirts ot ON t.ID = ot.Id_Tshirt
            LEFT JOIN offers o ON ot.Id_Offer = o.Id_Offer AND o.IsActive = 1 AND NOW() BETWEEN o.StartDate AND o.EndDate
            WHERE t.ID = @ID AND t.IsActive = 1;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", shirtId);
                        cmd.Parameters.AddWithValue("@Lang", currentLang); // Pasamos el idioma como parámetro

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblShirtName.Text = FormatJerseyName(Server.HtmlDecode(reader["Name"].ToString()));
                                lblBrand.Text = reader["Brand"].ToString();
                                lblType.Text = reader["Type"].ToString();
                                lblTeam.Text = reader["Team"].ToString();
                                lblYear.Text = reader["Year"].ToString();

                                int idBrand = Convert.ToInt32(reader["Id_Brand"]);
                                lblGuideBrand.Text = reader["Brand"].ToString();

                                // Llama al método para poblar la tabla de medidas
                                LoadSizeGuide(idBrand);

                                decimal originalPrice = Convert.ToDecimal(reader["OriginalPrice"]);
                                decimal finalPrice = Convert.ToDecimal(reader["FinalPrice"]);

                                if (originalPrice > finalPrice)
                                {
                                    lblPrice.Text = $"{finalPrice:F2} <span style='text-decoration: line-through; font-size: 0.6em; color: #888; margin-left: 8px;'>${originalPrice:F2}</span>";
                                }
                                else
                                {
                                    lblPrice.Text = $"{finalPrice:F2}";
                                }

                                bool isCustomizable = false;
                                if (reader["IsCustomizable"] != DBNull.Value)
                                {
                                    isCustomizable = Convert.ToBoolean(reader["IsCustomizable"]);
                                }
                                phPersonalizacion.Visible = isCustomizable;

                                string description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : "";
                                if (string.IsNullOrWhiteSpace(description))
                                {
                                    // Mensaje por defecto en el idioma correspondiente
                                    lblDescription.Text = currentLang == "es"
                                        ? "Camiseta auténtica de colección de OffsideShop. Diseñada con tela transpirable y costuras de alta calidad, perfecta para lucir tu pasión dentro y fuera del campo."
                                        : "Authentic OffsideShop collector jersey. Engineered with breathable fabric and premium stitching, perfect for showing your passion on and off the pitch.";
                                }
                                else
                                {
                                    lblDescription.Text = Server.HtmlDecode(description);
                                }

                                List<string> images = new List<string>();
                                for (int i = 1; i <= 5; i++)
                                {
                                    string colName = i == 1 ? "ImageURL" : "ImageURL" + i;
                                    if (reader[colName] != DBNull.Value)
                                    {
                                        string imgVal = reader[colName].ToString();
                                        if (!string.IsNullOrWhiteSpace(imgVal))
                                        {
                                            images.Add(imgVal);
                                        }
                                    }
                                }

                                if (images.Count == 0)
                                {
                                    images.Add("");
                                }

                                System.Text.StringBuilder sbIndicators = new System.Text.StringBuilder();
                                System.Text.StringBuilder sbItems = new System.Text.StringBuilder();

                                for (int i = 0; i < images.Count; i++)
                                {
                                    string activeClassIndicator = i == 0 ? "class=\"active\" aria-current=\"true\"" : "";
                                    sbIndicators.AppendLine($"<button type=\"button\" data-bs-target=\"#jerseyCarousel\" data-bs-slide-to=\"{i}\" {activeClassIndicator} aria-label=\"Slide {i + 1}\"></button>");

                                    string activeClassItem = i == 0 ? "active" : "";
                                    sbItems.AppendLine($"<div class=\"carousel-item {activeClassItem}\">");
                                    sbItems.AppendLine($"  <img src=\"{ResolveImageUrl(images[i])}\" class=\"d-block w-100\" alt=\"Jersey Image {i + 1}\" onerror=\"this.src='assets/img/default-product.jpg';\" />");
                                    sbItems.AppendLine($"</div>");
                                }

                                litCarouselIndicators.Text = sbIndicators.ToString();
                                litCarouselItems.Text = sbItems.ToString();
                                phCarouselControls.Visible = images.Count > 1;
                            }
                            else
                            {
                                Response.Redirect("Homepage.aspx");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading shirt details: " + ex.Message);
                TriggerAlert("Alert_ErrorTitle", "Alert_Details_LoadErrorText", "error");
            }
        }

        private void LoadSizeGuide(int brandId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT s.Size_Code, sg.Chest_cm, sg.Length_cm 
                                     FROM size_guides sg 
                                     INNER JOIN sizes s ON sg.Id_Size = s.Id_Size 
                                     WHERE sg.Id_Brand = @BrandId 
                                     ORDER BY s.Id_Size ASC;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@BrandId", brandId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            while (reader.Read())
                            {
                                sb.AppendLine("<tr>");
                                sb.AppendLine($"<td class='fw-bold'>{reader["Size_Code"]}</td>");
                                sb.AppendLine($"<td>{reader["Chest_cm"]}</td>");
                                sb.AppendLine($"<td>{reader["Length_cm"]}</td>");
                                sb.AppendLine("</tr>");
                            }

                            if (sb.Length == 0)
                            {
                                litSizeGuideTable.Text = $"<tr><td colspan='3' class='text-muted'>{AlertHelper.GetResourceString(this, "Modal_NoSizeGuide")}</td></tr>";
                            }
                            else
                            {
                                litSizeGuideTable.Text = sb.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading size guide: " + ex.Message);
            }
        }

        private void LoadSizes(int shirtId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                SELECT s.Id_Size, s.Size_Code AS SizeName, COALESCE(tv.Stock, 0) AS Stock
                FROM sizes s
                LEFT JOIN tshirt_variants tv ON s.Id_Size = tv.Id_Size AND tv.Id_Tshirt = @ID
                ORDER BY s.Id_Size;";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", shirtId);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dtSizes = new DataTable();
                        da.Fill(dtSizes);

                        rptSizes.DataSource = dtSizes;
                        rptSizes.DataBind();

                        bool hasAnyStock = false;
                        foreach (DataRow row in dtSizes.Rows)
                        {
                            if (Convert.ToInt32(row["Stock"]) > 0)
                            {
                                hasAnyStock = true;
                                break;
                            }
                        }

                        if (!hasAnyStock)
                        {
                            btnAddCart.Enabled = false;
                            btnAddCart.CssClass = "btn btn-secondary w-100 disabled";
                            btnAddCart.Text = AlertHelper.GetResourceString(this, "Detail_OutOfStock");
                        }
                        else
                        {
                            btnAddCart.Enabled = true;
                            btnAddCart.CssClass = "btn btn-buy w-100";
                            btnAddCart.Text = AlertHelper.GetResourceString(this, "Detail_AddToCart");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading sizes: " + ex.Message);
            }
        }

        protected string GetSizeClass(object idSize, object stock)
        {
            int currentStock = Convert.ToInt32(stock);

            if (currentStock <= 0)
            {
                return "btn btn-size-option out-of-stock";
            }

            string selectedSizeId = Session["SelectedSizeId"]?.ToString();
            if (selectedSizeId == idSize.ToString())
            {
                return "btn btn-size-option active";
            }

            return "btn btn-size-option";
        }

        protected void rptSizes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "SelectSize")
            {
                string selectedSizeId = e.CommandArgument.ToString();
                Session["SelectedSizeId"] = selectedSizeId;

                string idParam = Request.QueryString["id"];
                if (int.TryParse(idParam, out int shirtId) && int.TryParse(selectedSizeId, out int sizeId))
                {
                    int availableStock = 0;
                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(connectionString))
                        {
                            con.Open();
                            string query = "SELECT Stock FROM tshirt_variants WHERE Id_Tshirt = @IdTshirt AND Id_Size = @IdSize;";
                            using (MySqlCommand cmd = new MySqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@IdTshirt", shirtId);
                                cmd.Parameters.AddWithValue("@IdSize", sizeId);
                                object result = cmd.ExecuteScalar();
                                if (result != null) availableStock = Convert.ToInt32(result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error fetching variant stock: " + ex.Message);
                    }

                    hfMaxStock.Value = availableStock.ToString();

                    int currentQty = 1;
                    if (!string.IsNullOrEmpty(hfQuantity.Value)) int.TryParse(hfQuantity.Value, out currentQty);

                    if (currentQty > availableStock)
                    {
                        hfQuantity.Value = availableStock.ToString();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateQtyInput",
                            $"document.getElementById('txtDisplayQty').value = {availableStock};", true);
                    }

                    LoadSizes(shirtId);
                }
            }
        }

        private void LoadSimilarShirts(int shirtId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string leagueQuery = @"
                SELECT t.Id_Team, tm.Id_League
                FROM tshirts t
                LEFT JOIN teams tm ON t.Id_Team = tm.Id_Team
                WHERE t.ID = @ID;";

                    int teamId = 0;
                    int leagueId = 0;
                    using (MySqlCommand cmd = new MySqlCommand(leagueQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", shirtId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                teamId = reader["Id_Team"] != DBNull.Value ? Convert.ToInt32(reader["Id_Team"]) : 0;
                                leagueId = reader["Id_League"] != DBNull.Value ? Convert.ToInt32(reader["Id_League"]) : 0;
                            }
                        }
                    }

                    string similarQuery = @"
                SELECT t.ID, t.Name, t.ImageURL, t.Year,
                       t.Price AS OriginalPrice,
                       CASE WHEN o.Id_Offer IS NOT NULL THEN (t.Price - (t.Price * (o.DiscountPercentage / 100.0))) ELSE t.Price END AS FinalPrice,
                       CASE WHEN o.Id_Offer IS NOT NULL THEN 1 ELSE 0 END AS IsOnSale,
                       IFNULL(o.DiscountPercentage, 0) AS DiscountPercentage,
                       COALESCE(b.Name_Brand, 'OffsideBrand') AS Brand,
                       COALESCE(tm.Name_Team, 'OffsideTeam') AS Team,
                       COALESCE(kt.Name_KitType, 'Special Edition') AS Type,
                       GROUP_CONCAT(
                           CASE tv.Id_Size
                               WHEN 1 THEN 'S' WHEN 2 THEN 'M' WHEN 3 THEN 'L'
                               WHEN 4 THEN 'XL' WHEN 5 THEN 'XXL'
                           END ORDER BY tv.Id_Size SEPARATOR ', '
                       ) AS Sizes,
                       CASE WHEN t.Id_Team = @TeamId THEN 1 ELSE 0 END AS IsSameTeam,
                       COALESCE(sales.SalesCount, 0) AS SalesCount
                FROM tshirts t
                LEFT JOIN brands b ON t.Id_Brand = b.Id_Brand
                LEFT JOIN teams tm ON t.Id_Team = tm.Id_Team
                LEFT JOIN kit_types kt ON t.Id_KitType = kt.Id_KitType
                LEFT JOIN tshirt_variants tv ON tv.Id_Tshirt = t.ID AND tv.Stock > 0
                LEFT JOIN offer_tshirts ot ON t.ID = ot.Id_Tshirt
                LEFT JOIN offers o ON ot.Id_Offer = o.Id_Offer AND o.IsActive = 1 AND NOW() BETWEEN o.StartDate AND o.EndDate
                LEFT JOIN (
                    SELECT Id_Tshirt, SUM(Quantity) AS SalesCount
                    FROM order_details
                    GROUP BY Id_Tshirt
                ) sales ON t.ID = sales.Id_Tshirt
                WHERE (t.Id_Team = @TeamId OR (tm.Id_League = @LeagueId AND tm.Id_League > 0))
                  AND t.ID != @ID
                  AND t.IsActive = 1
                GROUP BY t.ID, t.Name, t.Price, t.ImageURL, t.Year, b.Name_Brand, tm.Name_Team, kt.Name_KitType, o.Id_Offer, o.DiscountPercentage, sales.SalesCount, t.Id_Team
                ORDER BY IsSameTeam DESC,
                         CASE WHEN t.Id_Team = @TeamId THEN RAND() ELSE 1 END,
                         SalesCount DESC
                LIMIT 8;";

                    using (MySqlCommand cmd = new MySqlCommand(similarQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@TeamId", teamId);
                        cmd.Parameters.AddWithValue("@LeagueId", leagueId);
                        cmd.Parameters.AddWithValue("@ID", shirtId);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptSimilar.DataSource = dt;
                        rptSimilar.DataBind();

                        divSimilar.Visible = dt.Rows.Count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading similar shirts: " + ex.Message);
            }
        }

        protected void btnAddCart_Click(object sender, EventArgs e)
        {
            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 3)
            {
                // Forzamos la lectura directa desde el navegador por si ASP.NET se pierde
                bool isCustomChecked = chkCustomize.Checked || Request.Form[chkCustomize.UniqueID] == "on";
                string pendingName = !string.IsNullOrEmpty(txtCustomName.Text) ? txtCustomName.Text : Request.Form[txtCustomName.UniqueID];
                string pendingNumber = !string.IsNullOrEmpty(txtCustomNumber.Text) ? txtCustomNumber.Text : Request.Form[txtCustomNumber.UniqueID];

                Session["PendingShirtId"] = Request.QueryString["id"];
                Session["PendingSizeId"] = Session["SelectedSizeId"];
                Session["PendingQuantity"] = hfQuantity.Value;

                Session["PendingIsCustom"] = isCustomChecked;
                Session["PendingCustomName"] = pendingName;
                Session["PendingCustomNumber"] = pendingNumber;

                string loginUrl = ResolveUrl("~/Login.aspx");
                string authTitle = AlertHelper.GetResourceString(this, "Alert_Details_AuthRequiredTitle").Replace("'", "\\'");
                string authText = AlertHelper.GetResourceString(this, "Alert_Details_AuthRequiredText").Replace("'", "\\'");
                string confirmBtn = AlertHelper.GetResourceString(this, "Alert_Details_GoToLogin").Replace("'", "\\'");
                string cancelBtn = AlertHelper.GetResourceString(this, "Alert_Details_Cancel").Replace("'", "\\'");
                string script = $@"
                    setTimeout(function() {{
                        if (typeof Swal !== 'undefined') {{
                            Swal.fire({{
                                title: '{authTitle}',
                                text: '{authText}',
                                icon: 'info',
                                showCancelButton: true,
                                confirmButtonColor: '#FFC800',
                                confirmButtonText: '{confirmBtn}',
                                cancelButtonText: '{cancelBtn}'
                            }}).then((result) => {{
                                if (result.isConfirmed) {{ window.location.href = '{loginUrl}'; }}
                            }});
                        }} else {{
                            if (confirm('{authTitle}\n\n{authText}')) {{
                                window.location.href = '{loginUrl}';
                            }}
                        }}
                    }}, 50);";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "authReq", script, true);
                return;
            }
            string idParam = Request.QueryString["id"];
            if (string.IsNullOrEmpty(idParam) || !int.TryParse(idParam, out int shirtId))
            {
                Response.Redirect("Homepage.aspx");
                return;
            }

            if (Session["SelectedSizeId"] == null)
            {
                TriggerAlert("Alert_Details_NoSizeTitle", "Alert_Details_NoSizeText", "warning");
                return;
            }
            int sizeId = Convert.ToInt32(Session["SelectedSizeId"]);
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string checkQuery = "SELECT Stock FROM tshirt_variants WHERE Id_Tshirt = @IdTshirt AND Id_Size = @IdSize;";
                    int realStock = 0;
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@IdTshirt", shirtId);
                        checkCmd.Parameters.AddWithValue("@IdSize", sizeId);
                        object result = checkCmd.ExecuteScalar();
                        if (result == null || Convert.ToInt32(result) <= 0)
                        {
                            TriggerAlert("Alert_Details_OutOfStockTitle", "Alert_Details_OutOfStockText", "error");
                            LoadSizes(shirtId);
                            return;
                        }
                        realStock = Convert.ToInt32(result);
                    }
                    string sizeName = "";
                    switch (sizeId)
                    {
                        case 1: sizeName = "S"; break;
                        case 2: sizeName = "M"; break;
                        case 3: sizeName = "L"; break;
                        case 4: sizeName = "XL"; break;
                        case 5: sizeName = "XXL"; break;
                        default: sizeName = "Custom"; break;
                    }

                    string shirtQuery = @"
                        SELECT t.Name, t.ImageURL, t.IsCustomizable,
                               CASE 
                                   WHEN o.Id_Offer IS NOT NULL THEN (t.Price - (t.Price * (o.DiscountPercentage / 100.0))) 
                                   ELSE t.Price 
                               END AS FinalPrice 
                        FROM tshirts t
                        LEFT JOIN offer_tshirts ot ON t.ID = ot.Id_Tshirt
                        LEFT JOIN offers o ON ot.Id_Offer = o.Id_Offer AND o.IsActive = 1 AND NOW() BETWEEN o.StartDate AND o.EndDate
                        WHERE t.ID = @ID;";

                    string shirtName = "Jersey";
                    decimal price = 0.00m;
                    string imageUrl = "";
                    bool isCustomizable = false;

                    using (MySqlCommand shirtCmd = new MySqlCommand(shirtQuery, con))
                    {
                        shirtCmd.Parameters.AddWithValue("@ID", shirtId);
                        using (MySqlDataReader reader = shirtCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                shirtName = reader["Name"].ToString();
                                price = Convert.ToDecimal(reader["FinalPrice"]);
                                imageUrl = reader["ImageURL"].ToString();

                                if (reader["IsCustomizable"] != DBNull.Value)
                                {
                                    isCustomizable = Convert.ToBoolean(reader["IsCustomizable"]);
                                }
                            }
                        }
                    }

                    bool isCustom = isCustomizable && chkCustomize.Checked;
                    string customName = isCustom ? txtCustomName.Text.Trim().ToUpper() : "";
                    string customNumber = isCustom ? txtCustomNumber.Text.Trim() : "";
                    // =========================================================
                    // NUEVA VALIDACIÓN DE PALABRAS PROHIBIDAS Y ALBURES
                    // =========================================================
                    if (isCustom && !string.IsNullOrEmpty(customName))
                    {
                        if (!IsCustomNameAllowed(customName))
                        {
                            TriggerAlert("Alert_Details_AttentionTitle", "Alert_Details_ForbiddenNameText", "warning");
                            return; // Detiene el proceso y no lo agrega al carrito
                        }
                    }

                    if (isCustom)
                    {
                        price += 15.00m;
                    }

                    DataTable dtCart = Session["Cart"] as DataTable;
                    if (dtCart == null)
                    {
                        dtCart = new DataTable();
                        dtCart.Columns.Add("ID", typeof(int));
                        dtCart.Columns.Add("ImageURL", typeof(string));
                        dtCart.Columns.Add("Name", typeof(string));
                        dtCart.Columns.Add("Size", typeof(string));
                        dtCart.Columns.Add("Price", typeof(decimal));
                        dtCart.Columns.Add("Quantity", typeof(int));
                        dtCart.Columns.Add("Subtotal", typeof(decimal));
                        dtCart.Columns.Add("Stock", typeof(int));
                        dtCart.Columns.Add("IsCustomized", typeof(bool));
                        dtCart.Columns.Add("CustomName", typeof(string));
                        dtCart.Columns.Add("CustomNumber", typeof(string));
                    }

                    int inputQty = 1;
                    if (!string.IsNullOrEmpty(hfQuantity.Value))
                    {
                        int.TryParse(hfQuantity.Value, out inputQty);
                    }

                    int totalInCartForThisPhysicalSize = 0;
                    foreach (DataRow r in dtCart.Rows)
                    {
                        if (Convert.ToInt32(r["ID"]) == shirtId && r["Size"].ToString() == sizeName)
                        {
                            totalInCartForThisPhysicalSize += Convert.ToInt32(r["Quantity"]);
                        }
                    }

                    if (totalInCartForThisPhysicalSize + inputQty > realStock)
                    {
                        int allowedAddition = realStock - totalInCartForThisPhysicalSize;
                        if (allowedAddition <= 0)
                        {
                            string title = AlertHelper.GetResourceString(this, "Alert_Details_LimitReachedTitle");
                            string rawMsg = AlertHelper.GetResourceString(this, "Alert_Details_LimitReachedText");
                            string formattedMsg = string.Format(rawMsg, realStock);
                            TriggerAlertWithText(title, formattedMsg, "warning");
                        }
                        else
                        {
                            string title = AlertHelper.GetResourceString(this, "Alert_Details_StockLimitTitle");
                            string rawMsg = AlertHelper.GetResourceString(this, "Alert_Details_StockLimitText");
                            string formattedMsg = string.Format(rawMsg, allowedAddition, totalInCartForThisPhysicalSize);
                            TriggerAlertWithText(title, formattedMsg, "warning");
                        }
                        return;
                    }

                    string filter = string.Format("ID = {0} AND Size = '{1}' AND IsCustomized = {2} AND CustomName = '{3}' AND CustomNumber = '{4}'",
                                                  shirtId,
                                                  sizeName,
                                                  isCustom,
                                                  customName.Replace("'", "''"),
                                                  customNumber.Replace("'", "''"));
                    DataRow[] existingRows = dtCart.Select(filter);

                    if (existingRows.Length > 0)
                    {
                        DataRow row = existingRows[0];
                        int currentCartQty = Convert.ToInt32(row["Quantity"]);
                        int totalProposedQty = currentCartQty + inputQty;

                        row["Quantity"] = totalProposedQty;
                        row["Subtotal"] = totalProposedQty * price;
                        row["Stock"] = realStock;
                    }
                    else
                    {
                        DataRow row = dtCart.NewRow();
                        row["ID"] = shirtId;
                        row["ImageURL"] = imageUrl;
                        row["Name"] = shirtName;
                        row["Size"] = sizeName;
                        row["Price"] = price;
                        row["Quantity"] = inputQty;
                        row["Subtotal"] = inputQty * price;
                        row["Stock"] = realStock;
                        row["IsCustomized"] = isCustom;
                        row["CustomName"] = isCustom ? customName : "";
                        row["CustomNumber"] = isCustom ? customNumber : "";

                        dtCart.Rows.Add(row);
                    }

                    Session["Cart"] = dtCart;
                    ActualizarContadorCarrito();

                    string successTitle = AlertHelper.GetResourceString(this, "Alert_SuccessTitle").Replace("'", "\\'");
                    string successText = AlertHelper.GetResourceString(this, "Alert_Details_AddedToCartText").Replace("'", "\\'");
                    string script = $@"
                        setTimeout(function() {{
                            if (typeof Swal !== 'undefined') {{
                                Swal.fire({{
                                    title: '{successTitle}',
                                    text: '{successText}',
                                    icon: 'success',
                                    confirmButtonColor: '#FFC800'
                                }}).then(() => {{
                                    window.location.reload();
                                }});
                            }} else {{
                                alert('{successTitle}: {successText}');
                                window.location.reload();
                            }}
                        }}, 50);";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "successAdd", script, true);
                }
            }
            catch (Exception ex)
            {
                string rawMsg = AlertHelper.GetResourceString(this, "Alert_Details_ErrorText");
                string formattedMsg = string.Format(rawMsg, ex.Message);
                TriggerAlertWithText(AlertHelper.GetResourceString(this, "Alert_ErrorTitle"), formattedMsg, "error");
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string search = HttpUtility.UrlEncode(txtSearch.Text.Trim());
            string league = ddlLeague.SelectedValue;
            string brand = ddlBrand.SelectedValue;
            string kit = ddlKitType.SelectedValue;

            Response.Redirect($"Homepage.aspx?search={search}&league={league}&brand={brand}&kit={kit}");
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlLeague.SelectedIndex = 0;
            ddlBrand.SelectedIndex = 0;
            ddlKitType.SelectedIndex = 0;
        }

        protected void btnNavCart_Click(object sender, EventArgs e)
        {
            Response.Redirect("Cart.aspx");
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

        protected void btnbackshop_Click(object sender, EventArgs e)
        {
            Response.Redirect("Homepage.aspx");
        }

        public bool IsCurrentUserAdminOrOwner()
        {
            if (Session["UserRole"] != null)
            {
                int role = Convert.ToInt32(Session["UserRole"]);
                return role == 1 || role == 2;
            }
            return false;
        }

        private bool HasUserPurchasedShirt(int idUser, int idTshirt)
        {
            if (idUser <= 0 || idTshirt <= 0) return false;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT COUNT(*) 
                                  FROM orders o
                                  INNER JOIN order_details od ON od.Id_Order = o.Id_Order
                                  WHERE o.Id_User = @IdUser 
                                    AND od.Id_Tshirt = @IdTshirt
                                    AND o.Id_Status NOT IN (5, 7)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUser", idUser);
                    cmd.Parameters.AddWithValue("@IdTshirt", idTshirt);

                    conn.Open();
                    long count = Convert.ToInt64(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool CanDeleteReview(object idUserParam)
        {
            if (IsCurrentUserAdminOrOwner()) return true;

            if (Session["Id_User"] != null && idUserParam != null && idUserParam != DBNull.Value)
            {
                return Session["Id_User"].ToString() == idUserParam.ToString();
            }
            return false;
        }

        protected void CargarReviews()
        {
            if (Request.QueryString["id"] == null) return;
            int idTshirt = Convert.ToInt32(Request.QueryString["id"]);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT r.*, u.Name, u.LastName, u.Id_Role 
                                 FROM product_reviews r 
                                 INNER JOIN users u ON r.Id_User = u.Id_User 
                                 WHERE r.Id_Tshirt = @IdTshirt 
                                 ORDER BY r.ReviewDate DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdTshirt", idTshirt);

                    DataTable dt = new DataTable();
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    rptReviews.DataSource = dt;
                    rptReviews.DataBind();

                    phNoReviews.Visible = (dt.Rows.Count == 0);
                }
            }
        }

        protected void btnSubmitReview_Click(object sender, EventArgs e)
        {
            if (Session["Id_User"] == null)
            {
                TriggerAlert("Alert_Details_AttentionTitle", "Alert_Details_ReviewAuthText", "warning");
                return;
            }

            int idUser = Convert.ToInt32(Session["Id_User"]);
            int idTshirt = Convert.ToInt32(Request.QueryString["id"]);

            if (!HasUserPurchasedShirt(idUser, idTshirt))
            {
                TriggerAlert("Alert_Details_AttentionTitle", "Alert_Details_ReviewPurchaseText", "warning");
                return;
            }

            int rating = 5;
            if (!string.IsNullOrEmpty(hfRatingInput.Value))
            {
                int.TryParse(hfRatingInput.Value, out rating);
            }

            string comment = txtComment.Text.Trim();

            if (string.IsNullOrEmpty(comment))
            {
                TriggerAlert("Alert_Details_AttentionTitle", "Alert_Details_ReviewEmptyText", "warning");
                return;
            }

            if (!IsCustomNameAllowed(comment))
            {
                TriggerAlert("Alert_Details_AttentionTitle", "Alert_Details_ForbiddenReviewText", "warning");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = "INSERT INTO product_reviews (Id_Tshirt, Id_User, Rating, Comment) VALUES (@IdTshirt, @IdUser, @Rating, @Comment)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdTshirt", idTshirt);
                        cmd.Parameters.AddWithValue("@IdUser", idUser);
                        cmd.Parameters.AddWithValue("@Rating", rating);
                        cmd.Parameters.AddWithValue("@Comment", HttpUtility.HtmlEncode(comment));

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                txtComment.Text = "";
                hfRatingInput.Value = "5";
                TriggerAlert("Alert_SuccessTitle", "Alert_Details_ReviewSuccessText", "success");
                CargarReviews();
            }
            catch (Exception ex)
            {
                string rawMsg = AlertHelper.GetResourceString(this, "Alert_Details_ErrorText");
                string formattedMsg = string.Format(rawMsg, ex.Message);
                TriggerAlertWithText(AlertHelper.GetResourceString(this, "Alert_ErrorTitle"), formattedMsg, "error");
            }
        }

        protected void rptReviews_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int idReview = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "DeleteReview")
            {
                int currentUserId = Session["Id_User"] != null ? Convert.ToInt32(Session["Id_User"]) : 0;
                bool isAdminOrOwner = IsCurrentUserAdminOrOwner();

                if (currentUserId == 0 && !isAdminOrOwner) return;

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        string query = "DELETE FROM product_reviews WHERE Id_Review = @IdReview AND (@IsAdminOrOwner = 1 OR Id_User = @IdUser)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@IdReview", idReview);
                            cmd.Parameters.AddWithValue("@IsAdminOrOwner", isAdminOrOwner ? 1 : 0);
                            cmd.Parameters.AddWithValue("@IdUser", currentUserId);

                            conn.Open();
                            int affectedRows = cmd.ExecuteNonQuery();

                            if (affectedRows > 0)
                            {
                                TriggerAlert("Alert_DeletedTitle", "Alert_Details_ReviewDeletedText", "success");
                            }
                            else
                            {
                                TriggerAlert("Alert_ErrorTitle", "Alert_Details_DeleteDeniedText", "error");
                            }
                        }
                    }
                    CargarReviews();
                }
                catch (Exception ex)
                {
                    string rawMsg = AlertHelper.GetResourceString(this, "Alert_Details_ErrorText");
                    string formattedMsg = string.Format(rawMsg, ex.Message);
                    TriggerAlertWithText(AlertHelper.GetResourceString(this, "Alert_ErrorTitle"), formattedMsg, "error");
                }
            }
            else if (e.CommandName == "SubmitReply")
            {
                if (!IsCurrentUserAdminOrOwner()) return;

                TextBox txtReply = (TextBox)e.Item.FindControl("txtReply");
                string replyText = txtReply.Text.Trim();

                if (string.IsNullOrEmpty(replyText))
                {
                    TriggerAlert("Alert_Details_AttentionTitle", "Alert_Details_ReplyEmptyText", "warning");
                    return;
                }

                if (!IsCustomNameAllowed(replyText))
                {
                    TriggerAlert("Alert_Details_AttentionTitle", "Alert_Details_ForbiddenReviewText", "warning");
                    return;
                }

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        string query = "UPDATE product_reviews SET ReplyComment = @Reply, ReplyDate = NOW() WHERE Id_Review = @IdReview";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Reply", HttpUtility.HtmlEncode(replyText));
                            cmd.Parameters.AddWithValue("@IdReview", idReview);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    TriggerAlert("Alert_Details_RepliedTitle", "Alert_Details_ReplySuccessText", "success");
                    CargarReviews();
                }
                catch (Exception ex)
                {
                    string rawMsg = AlertHelper.GetResourceString(this, "Alert_Details_ErrorText");
                    string formattedMsg = string.Format(rawMsg, ex.Message);
                    TriggerAlertWithText(AlertHelper.GetResourceString(this, "Alert_ErrorTitle"), formattedMsg, "error");
                }
            }
        }

        protected string FormatJerseyName(object nameObj)
        {
            if (nameObj == null || nameObj == DBNull.Value) return "";
            
            string name = nameObj.ToString().ToLower().Trim();
            System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(name);
        }
        private bool IsCustomNameAllowed(string customName)
        {
            if (string.IsNullOrWhiteSpace(customName)) return true;

            string rawText = customName.Trim().ToLower();
            string cleanedName = rawText.Replace(" ", "").Replace("\r", "").Replace("\n", "");

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                // Verificamos si el texto original o sin espacios contiene algún patrón censurado
                string query = @"SELECT COUNT(*) FROM censorship 
                                 WHERE LOWER(@RawText) LIKE CONCAT('%', LOWER(pattern), '%')
                                    OR LOWER(@CleanedName) LIKE CONCAT('%', LOWER(pattern), '%');";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RawText", rawText);
                    cmd.Parameters.AddWithValue("@CleanedName", cleanedName);
                    long count = Convert.ToInt64(cmd.ExecuteScalar());

                    return count == 0; // Si es 0, el contenido ES PERMITIDO
                }
            }
        }
        protected void btnGoToAccount_Click(object sender, EventArgs e)
        {
            Response.Redirect("MyAccount.aspx");
        }

        private void TriggerAlert(string titleKey, string messageKey, string iconType)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString("N"),
                AlertHelper.GetSafeAlertScript(this, titleKey, messageKey, iconType), true);
        }

        private void TriggerAlertWithText(string title, string message, string iconType)
        {
            title = title.Replace("'", "\\'");
            message = message.Replace("'", "\\'").Replace("\r\n", " ").Replace("\n", " ");

            string script = $@"
                setTimeout(function() {{
                    if (typeof Swal !== 'undefined') {{
                        Swal.fire({{
                            title: '{title}',
                            text: '{message}',
                            icon: '{iconType}',
                            confirmButtonColor: '#FFC800'
                        }});
                    }} else {{
                        alert('{title}: {message}');
                    }}
                }}, 50);";

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString("N"), script, true);
        }
    }
}