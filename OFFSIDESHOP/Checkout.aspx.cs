using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Checkout : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;
        public bool ShowMap = true;
        public bool ShowPaymentLoader = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            bool isLoggedIn = (Session["UserRole"] != null && Convert.ToInt32(Session["UserRole"]) == 3);
            phNavbarUser.Visible = isLoggedIn;
            phNavbarGuest.Visible = !isLoggedIn;

            if (Session["Cart"] == null)
            {
                Response.Redirect("Homepage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDepartments();
                LoadDataUsers();
                LoadOrderSummary();
                CargarDatosPerfilUsuario();
                CheckInitialCouponLockoutState();
            }
        }

        private void CheckInitialCouponLockoutState()
        {
            CouponRateLimitInfo rateLimit = GetCouponRateLimitInfo();
            DateTime now = DateTime.UtcNow;
            if (rateLimit != null && rateLimit.LockoutUntil.HasValue && rateLimit.LockoutUntil.Value > now)
            {
                bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");
                TimeSpan remaining = rateLimit.LockoutUntil.Value - now;
                int remainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
                string lockMsg = isSpanish
                    ? $"<span class='text-danger font-weight-bold'><i class='fas fa-ban'></i> Función de cupones bloqueada por intentos fallidos. Intenta de nuevo en {remainingMinutes} minuto(s).</span>"
                    : $"<span class='text-danger font-weight-bold'><i class='fas fa-ban'></i> Coupon entry is temporarily locked due to failed attempts. Try again in {remainingMinutes} minute(s).</span>";

                lblCouponMessage.Text = lockMsg;
                lblCouponMessage.Visible = true;
                txtCouponCode.Enabled = false;
                btnApplyCoupon.Enabled = false;
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

        private void LoadDataUsers()
        {
            if (Session["Id_User"] == null) return;
            int userId = Convert.ToInt32(Session["Id_User"]);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT Name, LastName, Mail, Phone, Address, id_city, Id_Municipality, Id_District, Default_Latitude, Default_Longitude FROM users WHERE Id_User = @UserId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader["Name"] != DBNull.Value) txtName.Text = reader["Name"].ToString();
                        if (reader["LastName"] != DBNull.Value) txtLastName.Text = reader["LastName"].ToString();
                        if (reader["Mail"] != DBNull.Value) txtEmail.Text = reader["Mail"].ToString();
                        if (reader["Phone"] != DBNull.Value) txtTel.Text = reader["Phone"].ToString();
                        if (reader["Address"] != DBNull.Value) txtAddress.Text = reader["Address"].ToString();

                        if (reader["Default_Latitude"] != DBNull.Value) hfUserDefaultLat.Value = reader["Default_Latitude"].ToString();
                        if (reader["Default_Longitude"] != DBNull.Value) hfUserDefaultLng.Value = reader["Default_Longitude"].ToString();

                        if (reader["id_city"] != DBNull.Value && !string.IsNullOrEmpty(reader["id_city"].ToString()))
                        {
                            string idCity = reader["id_city"].ToString();
                            if (ddlCity.Items.FindByValue(idCity) != null)
                            {
                                ddlCity.SelectedValue = idCity;
                                LoadMunicipalitiesByDepartment(idCity);

                                if (reader["Id_Municipality"] != DBNull.Value && !string.IsNullOrEmpty(reader["Id_Municipality"].ToString()))
                                {
                                    string idMun = reader["Id_Municipality"].ToString();
                                    if (ddlMunicipality.Items.FindByValue(idMun) != null)
                                    {
                                        ddlMunicipality.SelectedValue = idMun;
                                        ddlMunicipality.Enabled = true;
                                        LoadDistrictsByMunicipality(idMun);

                                        if (reader["Id_District"] != DBNull.Value && !string.IsNullOrEmpty(reader["Id_District"].ToString()))
                                        {
                                            string idDist = reader["Id_District"].ToString();
                                            if (ddlDistrict.Items.FindByValue(idDist) != null)
                                            {
                                                ddlDistrict.SelectedValue = idDist;
                                                ddlDistrict.Enabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadMunicipalitiesByDepartment(string idCity)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT Id_Municipality, Municipality_Name FROM municipalities WHERE id_city = @id_city ORDER BY Municipality_Name ASC;", con);
                cmd.Parameters.AddWithValue("@id_city", idCity);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                ddlMunicipality.Items.Clear();
                ddlMunicipality.Items.Add(new ListItem("- Select Municipality -", ""));
                foreach (DataRow row in dt.Rows)
                    ddlMunicipality.Items.Add(new ListItem(row["Municipality_Name"].ToString(), row["Id_Municipality"].ToString()));
            }
        }

        private void LoadDistrictsByMunicipality(string idMunicipality)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT Id_District, District_Name FROM districts WHERE Id_Municipality = @Id_Municipality ORDER BY District_Name ASC;", con);
                cmd.Parameters.AddWithValue("@Id_Municipality", idMunicipality);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                ddlDistrict.Items.Clear();
                ddlDistrict.Items.Add(new ListItem("- Select District -", ""));
                foreach (DataRow row in dt.Rows)
                    ddlDistrict.Items.Add(new ListItem(row["District_Name"].ToString(), row["Id_District"].ToString()));
            }
        }

        private void LoadDepartments()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id_city, city_name FROM cities ORDER BY city_name ASC;", con);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                ddlCity.Items.Clear();
                ddlCity.Items.Add(new ListItem("- Select Department -", ""));
                foreach (DataRow row in dt.Rows)
                {
                    ddlCity.Items.Add(new ListItem(row["city_name"].ToString(), row["id_city"].ToString()));
                }

                ResetDropdown(ddlMunicipality, "- Select Municipality -");
                ResetDropdown(ddlDistrict, "- Select District -");
            }
        }

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCityId = ddlCity.SelectedValue;
            if (!string.IsNullOrEmpty(selectedCityId))
            {
                LoadMunicipalitiesByDepartment(selectedCityId);
                ddlMunicipality.Enabled = true;
                ResetDropdown(ddlDistrict, "- Select District -");
            }
            else
            {
                ResetDropdown(ddlMunicipality, "- Select Municipality -");
                ResetDropdown(ddlDistrict, "- Select District -");
            }
            ActualizarResumenPrecios();
        }

        private void ResetDropdown(DropDownList ddl, string defaultText)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(defaultText, ""));
            ddl.Enabled = false;
        }

        private void LoadOrderSummary()
        {
            DataTable dtCart = Session["Cart"] as DataTable;
            if (dtCart == null) return;
            string html = "";
            foreach (DataRow row in dtCart.Rows)
            {
                string name = FormatJerseyName(row["Name"]);
                int qty = Convert.ToInt32(row["Quantity"]);
                decimal subtotal = Convert.ToDecimal(row["Subtotal"]);

                if (dtCart.Columns.Contains("IsCustomized") && Convert.ToBoolean(row["IsCustomized"]))
                {
                    string customLabel = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es") ? "Personalizado" : "Customized";
                    name += $" <br><small class='text-warning'>({customLabel}: {row["CustomName"]} #{row["CustomNumber"]})</small>";
                }

                html += $@"<div class='order-col'>
                               <div style='display:flex; align-items:center; gap:10px;'>
                                   <img src='images/camisetas/{row["ImageURL"]}' style='width:50px; height:50px; object-fit:cover; border-radius:4px;' />
                                   <span>{qty}x {name}</span>
                               </div>
                               <div>${subtotal:F2}</div>
                           </div>";
            }
            orderProducts.InnerHtml = html;
            ActualizarResumenPrecios();
        }

        private void ActualizarResumenPrecios()
        {
            DataTable dtCart = Session["Cart"] as DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0) return;

            decimal subtotalCamisetas = 0;
            foreach (DataRow r in dtCart.Rows)
            {
                subtotalCamisetas += Convert.ToDecimal(r["Subtotal"]);
            }

            decimal discountPercentage = ViewState["DiscountPercentage"] != null 
                ? Convert.ToDecimal(ViewState["DiscountPercentage"]) 
                : (Session["DiscountPercentage"] != null ? Convert.ToDecimal(Session["DiscountPercentage"]) : 0m);
            decimal discountAmount = Math.Round((subtotalCamisetas * discountPercentage) / 100m, 2);
            ViewState["DiscountAmount"] = discountAmount;
            Session["DiscountAmount"] = discountAmount;

            decimal costoEnvio = CalcularCostoEnvio();
            decimal totalFinalConEnvio = (subtotalCamisetas - discountAmount) + costoEnvio;

            lblOrderShipping.Text = costoEnvio == 0 ? "FREE" : $"${costoEnvio:F2}";
            lblOrderTotal.Text = $"${totalFinalConEnvio:F2}";

            if (discountAmount > 0)
            {
                phDiscountRow.Visible = true;
                lblOrderDiscount.Text = $"-${discountAmount:F2} ({discountPercentage:G0}%)";
            }
            else
            {
                phDiscountRow.Visible = false;
            }

            hfTotalAmount.Value = totalFinalConEnvio.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }

        private decimal CalcularCostoEnvio()
        {
            if (ddlCity.SelectedIndex <= 0) return 3.50m;

            string selectedCityId = ddlCity.SelectedValue;
            decimal costoEnvio = 3.50m;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT shipping_cost FROM cities WHERE id_city = @id_city;";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id_city", selectedCityId);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        costoEnvio = Convert.ToDecimal(result);
                    }
                }
            }
            return costoEnvio;
        }

        private class CouponRateLimitInfo
        {
            public int FailedAttempts { get; set; }
            public DateTime? LockoutUntil { get; set; }
            public DateTime LastAttemptUtc { get; set; }
        }

        private string GetClientIpAddress()
        {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ip))
            {
                string[] parts = ip.Split(',');
                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                {
                    return parts[0].Trim();
                }
            }
            return Request.UserHostAddress ?? "Unknown";
        }

        private string GetCouponRateLimitKey()
        {
            string ip = GetClientIpAddress();
            string userId = Session["Id_User"] != null ? Session["Id_User"].ToString() : "guest";
            return $"CouponRL_{ip}_{userId}";
        }

        private CouponRateLimitInfo GetCouponRateLimitInfo()
        {
            string key = GetCouponRateLimitKey();
            if (HttpRuntime.Cache[key] is CouponRateLimitInfo info)
            {
                return info;
            }
            return null;
        }

        private void SaveCouponRateLimitInfo(CouponRateLimitInfo info)
        {
            string key = GetCouponRateLimitKey();
            HttpRuntime.Cache.Insert(key, info, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30));
        }

        private void ResetCouponRateLimit()
        {
            string key = GetCouponRateLimitKey();
            HttpRuntime.Cache.Remove(key);
        }

        protected void btnApplyCoupon_Click(object sender, EventArgs e)
        {
            bool isSpanish = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es");

            CouponRateLimitInfo rateLimit = GetCouponRateLimitInfo();
            DateTime now = DateTime.UtcNow;

            // 1. Verificar si está actualmente bloqueado por exceso de intentos fallidos
            if (rateLimit != null && rateLimit.LockoutUntil.HasValue && rateLimit.LockoutUntil.Value > now)
            {
                TimeSpan remaining = rateLimit.LockoutUntil.Value - now;
                int remainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
                string lockMsg = isSpanish
                    ? $"<span class='text-danger'><i class='fas fa-shield-alt'></i> Demasiados intentos fallidos. Función bloqueada. Intenta de nuevo en {remainingMinutes} minuto(s).</span>"
                    : $"<span class='text-danger'><i class='fas fa-shield-alt'></i> Too many failed attempts. Coupon feature locked. Please try again in {remainingMinutes} minute(s).</span>";

                lblCouponMessage.Text = lockMsg;
                lblCouponMessage.Visible = true;
                return;
            }

            // 2. Control de ráfagas (Burst rate limit: mínimo 1.5s entre intentos para frenar scripts/bots automatizados)
            if (rateLimit != null && (now - rateLimit.LastAttemptUtc).TotalSeconds < 1.5)
            {
                string waitMsg = isSpanish
                    ? "<span class='text-warning'><i class='fas fa-clock'></i> Por favor, espera un momento antes de volver a ingresar un código.</span>"
                    : "<span class='text-warning'><i class='fas fa-clock'></i> Please wait a moment before trying another code.</span>";

                lblCouponMessage.Text = waitMsg;
                lblCouponMessage.Visible = true;
                return;
            }

            // Inicializar o actualizar tracking de intento
            if (rateLimit == null)
            {
                rateLimit = new CouponRateLimitInfo { FailedAttempts = 0, LastAttemptUtc = now };
            }
            else
            {
                if (rateLimit.LockoutUntil.HasValue && rateLimit.LockoutUntil.Value <= now)
                {
                    rateLimit.FailedAttempts = 0;
                    rateLimit.LockoutUntil = null;
                }
                rateLimit.LastAttemptUtc = now;
            }

            string rawCode = txtCouponCode.Text.Trim();
            if (string.IsNullOrEmpty(rawCode) || rawCode.Length > 20 || !System.Text.RegularExpressions.Regex.IsMatch(rawCode, @"^[a-zA-Z0-9_-]+$"))
            {
                rateLimit.FailedAttempts++;
                SaveCouponRateLimitInfo(rateLimit);
                int attemptsLeft = Math.Max(0, 5 - rateLimit.FailedAttempts);

                string invalidFormatMsg = isSpanish
                    ? $"<span class='text-danger'><i class='fas fa-times-circle'></i> Código inválido. Intentos restantes: {attemptsLeft} de 5.</span>"
                    : $"<span class='text-danger'><i class='fas fa-times-circle'></i> Invalid code format. Remaining attempts: {attemptsLeft} of 5.</span>";

                lblCouponMessage.Text = invalidFormatMsg;
                lblCouponMessage.Visible = true;
                return;
            }

            string code = rawCode.ToUpper();

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT Id_Coupon, DiscountPercentage, MaxUses, UsedCount, IsActive FROM coupons WHERE Code = @Code";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isActive = Convert.ToInt32(reader["IsActive"]) == 1;
                            int maxUses = Convert.ToInt32(reader["MaxUses"]);
                            int usedCount = Convert.ToInt32(reader["UsedCount"]);

                            if (!isActive)
                            {
                                RegisterFailedCouponAttempt(rateLimit, isSpanish, code, isSpanish ? "Este cupón ya no se encuentra activo." : "This coupon is no longer active.");
                            }
                            else if (usedCount >= maxUses)
                            {
                                RegisterFailedCouponAttempt(rateLimit, isSpanish, code, isSpanish ? "Este cupón ha alcanzado su límite máximo de usos." : "This coupon has reached its usage limit.");
                            }
                            else
                            {
                                // Cupón Válido: Limpiar contador de intentos fallidos
                                ResetCouponRateLimit();

                                ViewState["CouponId"] = reader["Id_Coupon"];
                                ViewState["DiscountPercentage"] = reader["DiscountPercentage"];
                                Session["CouponId"] = reader["Id_Coupon"];
                                Session["DiscountPercentage"] = reader["DiscountPercentage"];

                                string successMsg = isSpanish
                                    ? $"<span class='text-success font-weight-bold'><i class='fas fa-check-circle'></i> ¡Cupón aplicado! ({reader["DiscountPercentage"]}% de descuento)</span>"
                                    : $"<span class='text-success font-weight-bold'><i class='fas fa-check-circle'></i> Coupon applied! ({reader["DiscountPercentage"]}% OFF)</span>";

                                lblCouponMessage.Text = successMsg;
                                txtCouponCode.Enabled = false;
                                btnApplyCoupon.Enabled = false;
                            }
                        }
                        else
                        {
                            // Código inexistente
                            RegisterFailedCouponAttempt(rateLimit, isSpanish, code, isSpanish ? "El código de cupón no existe." : "Invalid coupon code.");
                        }
                    }
                }
            }
            lblCouponMessage.Visible = true;
            ActualizarResumenPrecios();
        }

        private void RegisterFailedCouponAttempt(CouponRateLimitInfo rateLimit, bool isSpanish, string attemptedCode, string baseErrorMsg)
        {
            rateLimit.FailedAttempts++;
            int maxAllowed = 5;
            int lockoutMinutes = 15;

            if (rateLimit.FailedAttempts >= maxAllowed)
            {
                rateLimit.LockoutUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                SaveCouponRateLimitInfo(rateLimit);

                try
                {
                    AuditLogger.LogActivity("SUSPICIOUS_ACTIVITY", "CHECKOUT", $"Anti-Brute Force Lockout triggered for IP {GetClientIpAddress()}. Failed attempts: {rateLimit.FailedAttempts}. Last code tried: {attemptedCode}");
                }
                catch { }

                string lockMsg = isSpanish
                    ? $"<span class='text-danger font-weight-bold'><i class='fas fa-ban'></i> Has alcanzado el límite de {maxAllowed} intentos fallidos. La función de cupones se ha bloqueado por {lockoutMinutes} minutos.</span>"
                    : $"<span class='text-danger font-weight-bold'><i class='fas fa-ban'></i> You have exceeded {maxAllowed} failed attempts. Coupon entry is locked for {lockoutMinutes} minutes.</span>";

                lblCouponMessage.Text = lockMsg;
            }
            else
            {
                SaveCouponRateLimitInfo(rateLimit);
                int attemptsRemaining = maxAllowed - rateLimit.FailedAttempts;
                string warningMsg = isSpanish
                    ? $"<span class='text-danger'><i class='fas fa-times-circle'></i> {baseErrorMsg} (Te quedan {attemptsRemaining} de {maxAllowed} intentos antes del bloqueo).</span>"
                    : $"<span class='text-danger'><i class='fas fa-times-circle'></i> {baseErrorMsg} ({attemptsRemaining} of {maxAllowed} attempts remaining before temporary lockout).</span>";

                lblCouponMessage.Text = warningMsg;
            }
        }

        protected void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (!IsFormValid()) return;

            if (!IsPhoneValid(txtTel.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "invalidPhoneCOD", "Swal.fire('Invalid Phone', 'The phone number must contain exactly 8 digits.', 'error');", true);
                return;
            }

            DataTable dtCart = Session["Cart"] as DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0) return;

            UpdateUserProfileOnCheckout();

            decimal total = Convert.ToDecimal(hfTotalAmount.Value, System.Globalization.CultureInfo.InvariantCulture);
            decimal shippingCost = CalcularCostoEnvio();
            int userId = Convert.ToInt32(Session["Id_User"]);

            object idCoupon = ViewState["CouponId"] ?? DBNull.Value;
            decimal discountApplied = ViewState["DiscountAmount"] != null ? Convert.ToDecimal(ViewState["DiscountAmount"]) : 0m;
            string cityName = ddlCity.SelectedIndex > 0 ? ddlCity.SelectedItem.Text : "";
            string munName = ddlMunicipality.SelectedIndex > 0 ? ddlMunicipality.SelectedItem.Text : "";
            string distName = ddlDistrict.SelectedIndex > 0 ? ddlDistrict.SelectedItem.Text : "";

            decimal mapLat, mapLng;
            if (!decimal.TryParse(hfLatitude.Value, out mapLat))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }
            if (!decimal.TryParse(hfLongitude.Value, out mapLng))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }

            // Recorte de seguridad para notas (máximo 200 caracteres) y nombre/apellido (máximo 50 caracteres)
            string safeNotes = txtNotes.Text.Length > 200 ? txtNotes.Text.Substring(0, 200) : txtNotes.Text;
            string safeName = txtName.Text.Trim().Length > 50 ? txtName.Text.Trim().Substring(0, 50) : txtName.Text.Trim();
            string safeLastName = txtLastName.Text.Trim().Length > 50 ? txtLastName.Text.Trim().Substring(0, 50) : txtLastName.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                var groupedCart = dtCart.AsEnumerable()
                      .GroupBy(r => new { Id = r.Field<int>("ID"), Size = r.Field<string>("Size"), Name = r.Field<string>("Name") })
                      .Select(g => new
                      {
                          IdTshirt = g.Key.Id,
                          SizeName = g.Key.Size,
                          ProductName = g.Key.Name,
                          TotalRequestedQty = g.Sum(r => r.Field<int>("Quantity"))
                      }).ToList();

                foreach (var item in groupedCart)
                {
                    string checkStockQuery = @"SELECT Stock FROM tshirt_variants WHERE Id_Tshirt = @IdTshirt AND Id_Size = (SELECT Id_Size FROM sizes WHERE Size_Code = @Size)";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkStockQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@IdTshirt", item.IdTshirt);
                        checkCmd.Parameters.AddWithValue("@Size", item.SizeName);

                        object stockObj = checkCmd.ExecuteScalar();
                        int currentStock = (stockObj != null) ? Convert.ToInt32(stockObj) : 0;

                        if (currentStock < item.TotalRequestedQty)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "outOfStock", $"Swal.fire('Out of Stock', 'Sorry, the jersey \"{item.ProductName}\" in size {item.SizeName} only has {currentStock} units left.', 'error');", true);
                            return;
                        }
                    }
                }

                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string orderQuery = @"INSERT INTO orders 
                        (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, shipping_cost, Id_Status) 
                        VALUES (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, @IdPaymentMethod, @ShippingCost, @IdStatus); 
                        SELECT LAST_INSERT_ID();";

                        MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans);
                        cmd.Parameters.AddWithValue("@IdUser", userId);
                        cmd.Parameters.AddWithValue("@Name", safeName);
                        cmd.Parameters.AddWithValue("@LastName", safeLastName);
                        cmd.Parameters.AddWithValue("@Mail", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@Lat", mapLat);
                        cmd.Parameters.AddWithValue("@Lng", mapLng);
                        cmd.Parameters.AddWithValue("@IdCity", ddlCity.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdMunicipality", string.IsNullOrEmpty(ddlMunicipality.SelectedValue) ? (object)DBNull.Value : ddlMunicipality.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdDistrict", string.IsNullOrEmpty(ddlDistrict.SelectedValue) ? (object)DBNull.Value : ddlDistrict.SelectedValue);
                        cmd.Parameters.AddWithValue("@Phone", txtTel.Text);
                        cmd.Parameters.AddWithValue("@Notes", safeNotes);
                        cmd.Parameters.AddWithValue("@Total", total);
                        cmd.Parameters.AddWithValue("@IdCoupon", idCoupon);
                        cmd.Parameters.AddWithValue("@DiscountApplied", discountApplied);
                        cmd.Parameters.AddWithValue("@IdPaymentMethod", 1);
                        cmd.Parameters.AddWithValue("@ShippingCost", shippingCost);
                        cmd.Parameters.AddWithValue("@IdStatus", 1);

                        int orderId = Convert.ToInt32(cmd.ExecuteScalar());

                        foreach (DataRow row in dtCart.Rows)
                        {
                            string detailQuery = @"INSERT INTO order_details (Id_Order, Id_Tshirt, ProductName, Size, Price, Quantity, Subtotal) VALUES (@IdOrder, @IdTshirt, @Name, @Size, @Price, @Qty, @Subtotal)";
                            MySqlCommand detailCmd = new MySqlCommand(detailQuery, conn, trans);

                            string updateStock = @"UPDATE tshirt_variants SET Stock = Stock - @Qty WHERE Id_Tshirt = @IdTshirt AND Id_Size = (SELECT Id_Size FROM sizes WHERE Size_Code = @Size)";
                            MySqlCommand stockCmd = new MySqlCommand(updateStock, conn, trans);
                            stockCmd.Parameters.AddWithValue("@Qty", row["Quantity"]);
                            stockCmd.Parameters.AddWithValue("@IdTshirt", row["ID"]);
                            stockCmd.Parameters.AddWithValue("@Size", row["Size"]);
                            stockCmd.ExecuteNonQuery();

                            string dbProductName = row["Name"].ToString();
                            if (dtCart.Columns.Contains("IsCustomized") && Convert.ToBoolean(row["IsCustomized"]))
                            {
                                string customLabel = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es") ? "Personalizado" : "Customized";
                                dbProductName += $" ({customLabel}: {row["CustomName"]} #{row["CustomNumber"]})";
                            }
                            detailCmd.Parameters.AddWithValue("@IdOrder", orderId);
                            detailCmd.Parameters.AddWithValue("@IdTshirt", row["ID"]);
                            detailCmd.Parameters.AddWithValue("@Name", dbProductName);
                            detailCmd.Parameters.AddWithValue("@Size", row["Size"]);
                            detailCmd.Parameters.AddWithValue("@Price", row["Price"]);
                            detailCmd.Parameters.AddWithValue("@Qty", row["Quantity"]);
                            detailCmd.Parameters.AddWithValue("@Subtotal", row["Subtotal"]);
                            detailCmd.ExecuteNonQuery();
                        }

                        if (idCoupon != DBNull.Value)
                        {
                            string updateCoupon = "UPDATE coupons SET UsedCount = UsedCount + 1 WHERE Id_Coupon = @IdCoupon;";
                            using (MySqlCommand cmdCoupon = new MySqlCommand(updateCoupon, conn, trans))
                            {
                                cmdCoupon.Parameters.AddWithValue("@IdCoupon", idCoupon);
                                cmdCoupon.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();

                        int createdOrderId = orderId;
                        DataTable dtCartCopy = dtCart.Copy();
                        string emailTo = txtEmail.Text.Trim();
                        string nameTo = $"{safeName} {safeLastName}".Trim();
                        string shipText = shippingCost == 0 ? "FREE" : $"${shippingCost:F2}";
                        List<string> locParts = new List<string>();
                        if (!string.IsNullOrEmpty(distName)) locParts.Add(distName);
                        if (!string.IsNullOrEmpty(munName)) locParts.Add(munName);
                        if (!string.IsNullOrEmpty(cityName)) locParts.Add(cityName);
                        string locStr = string.Join(", ", locParts);
                        string shipAddrHtml = $@"
                            <p style='color: #555; font-size: 15px; margin: 0; line-height: 1.5;'>
                                <strong>Address:</strong> {HttpUtility.HtmlEncode(txtAddress.Text.Trim())}<br/>
                                {(string.IsNullOrEmpty(locStr) ? "" : $"<strong>Location:</strong> {HttpUtility.HtmlEncode(locStr)}<br/>")}
                                <strong>Phone:</strong> {HttpUtility.HtmlEncode(txtTel.Text.Trim())}
                            </p>";

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            EmailService.SendOrderConfirmation(createdOrderId, total, dtCartCopy, "Cash on Delivery", shipText, shipAddrHtml, emailTo, nameTo);
                        });
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "errorTrans", $"Swal.fire('Database Error', '{ex.Message.Replace("'", "\\'")}', 'error');", true);
                        return;
                    }
                }
            }

            ShowMap = false;
            ShowPaymentLoader = true;
            Session.Remove("Cart");
            string successTitle = GetGlobalResourceObject("Strings", "Checkout_SuccessTitle")?.ToString() ?? "Order Placed!";
            string successText = GetGlobalResourceObject("Strings", "Checkout_SuccessText")?.ToString() ?? "Your order has been placed successfully.";
            string script = $"Swal.fire({{ title: '{successTitle.Replace("'", "\\'")}', text: '{successText.Replace("'", "\\'")}', icon: 'success', confirmButtonColor: '#FFC800' }}).then(() => {{ window.location.href = 'MyOrders.aspx'; }});";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "exito", script, true);
        }

        protected void btnConfirmPayPalPayment_Click(object sender, EventArgs e)
        {
            DataTable dtCart = Session["Cart"] as DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0) return;

            if (!IsPhoneValid(txtTel.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "invalidPhonePP", "Swal.fire('Invalid Phone', 'The phone number must contain exactly 8 digits.', 'error');", true);
                return;
            }

            UpdateUserProfileOnCheckout();

            decimal total = Convert.ToDecimal(hfTotalAmount.Value, System.Globalization.CultureInfo.InvariantCulture);
            decimal shippingCost = CalcularCostoEnvio();
            string transactionId = hfTransactionID.Value;
            int userId = Convert.ToInt32(Session["Id_User"]);

            object idCoupon = ViewState["CouponId"] ?? DBNull.Value;
            decimal discountApplied = ViewState["DiscountAmount"] != null ? Convert.ToDecimal(ViewState["DiscountAmount"]) : 0m;

            string cityName = ddlCity.SelectedIndex > 0 ? ddlCity.SelectedItem.Text : "";
            string munName = ddlMunicipality.SelectedIndex > 0 ? ddlMunicipality.SelectedItem.Text : "";
            string distName = ddlDistrict.SelectedIndex > 0 ? ddlDistrict.SelectedItem.Text : "";

            decimal mapLat, mapLng;
            if (!decimal.TryParse(hfLatitude.Value, out mapLat))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }
            if (!decimal.TryParse(hfLongitude.Value, out mapLng))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }

            // Recorte de seguridad para notas (máximo 200 caracteres) y nombre/apellido (máximo 50 caracteres)
            string safeNotes = txtNotes.Text.Length > 200 ? txtNotes.Text.Substring(0, 200) : txtNotes.Text;
            string safeName = txtName.Text.Trim().Length > 50 ? txtName.Text.Trim().Substring(0, 50) : txtName.Text.Trim();
            string safeLastName = txtLastName.Text.Trim().Length > 50 ? txtLastName.Text.Trim().Substring(0, 50) : txtLastName.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string orderQuery = @"INSERT INTO orders 
                             (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, TransactionID, shipping_cost, Id_Status) 
                             VALUES 
                             (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, @IdPaymentMethod, @TransactionID, @ShippingCost, @IdStatus); 
                             SELECT LAST_INSERT_ID();";

                        MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans);
                        cmd.Parameters.AddWithValue("@IdUser", userId);
                        cmd.Parameters.AddWithValue("@Name", safeName);
                        cmd.Parameters.AddWithValue("@LastName", safeLastName);
                        cmd.Parameters.AddWithValue("@Mail", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@Lat", mapLat);
                        cmd.Parameters.AddWithValue("@Lng", mapLng);
                        cmd.Parameters.AddWithValue("@IdCity", ddlCity.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdMunicipality", string.IsNullOrEmpty(ddlMunicipality.SelectedValue) ? (object)DBNull.Value : ddlMunicipality.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdDistrict", string.IsNullOrEmpty(ddlDistrict.SelectedValue) ? (object)DBNull.Value : ddlDistrict.SelectedValue);
                        cmd.Parameters.AddWithValue("@Phone", txtTel.Text);
                        cmd.Parameters.AddWithValue("@Notes", safeNotes);
                        cmd.Parameters.AddWithValue("@Total", total);
                        cmd.Parameters.AddWithValue("@IdCoupon", idCoupon);
                        cmd.Parameters.AddWithValue("@DiscountApplied", discountApplied);
                        cmd.Parameters.AddWithValue("@IdPaymentMethod", 2); // PayPal
                        cmd.Parameters.AddWithValue("@TransactionID", string.IsNullOrEmpty(transactionId) ? (object)DBNull.Value : transactionId);
                        cmd.Parameters.AddWithValue("@ShippingCost", shippingCost);
                        cmd.Parameters.AddWithValue("@IdStatus", 2); // Paid

                        int orderId = Convert.ToInt32(cmd.ExecuteScalar());

                        foreach (DataRow row in dtCart.Rows)
                        {
                            string detailQuery = @"INSERT INTO order_details (Id_Order, Id_Tshirt, ProductName, Size, Price, Quantity, Subtotal) VALUES (@IdOrder, @IdTshirt, @Name, @Size, @Price, @Qty, @Subtotal)";
                            MySqlCommand detailCmd = new MySqlCommand(detailQuery, conn, trans);

                            string updateStock = @"UPDATE tshirt_variants SET Stock = Stock - @Qty WHERE Id_Tshirt = @IdTshirt AND Id_Size = (SELECT Id_Size FROM sizes WHERE Size_Code = @Size)";
                            MySqlCommand stockCmd = new MySqlCommand(updateStock, conn, trans);
                            stockCmd.Parameters.AddWithValue("@Qty", row["Quantity"]);
                            stockCmd.Parameters.AddWithValue("@IdTshirt", row["ID"]);
                            stockCmd.Parameters.AddWithValue("@Size", row["Size"]);
                            stockCmd.ExecuteNonQuery();

                            string dbProductName = row["Name"].ToString();
                            if (dtCart.Columns.Contains("IsCustomized") && Convert.ToBoolean(row["IsCustomized"]))
                            {
                                string customLabel = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es") ? "Personalizado" : "Customized";
                                dbProductName += $" ({customLabel}: {row["CustomName"]} #{row["CustomNumber"]})";
                            }
                            detailCmd.Parameters.AddWithValue("@IdOrder", orderId);
                            detailCmd.Parameters.AddWithValue("@IdTshirt", row["ID"]);
                            detailCmd.Parameters.AddWithValue("@Name", dbProductName);
                            detailCmd.Parameters.AddWithValue("@Size", row["Size"]);
                            detailCmd.Parameters.AddWithValue("@Price", row["Price"]);
                            detailCmd.Parameters.AddWithValue("@Qty", row["Quantity"]);
                            detailCmd.Parameters.AddWithValue("@Subtotal", row["Subtotal"]);
                            detailCmd.ExecuteNonQuery();
                        }

                        if (idCoupon != DBNull.Value)
                        {
                            string updateCoupon = "UPDATE coupons SET UsedCount = UsedCount + 1 WHERE Id_Coupon = @IdCoupon;";
                            using (MySqlCommand cmdCoupon = new MySqlCommand(updateCoupon, conn, trans))
                            {
                                cmdCoupon.Parameters.AddWithValue("@IdCoupon", idCoupon);
                                cmdCoupon.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();

                        int createdOrderId = orderId;
                        DataTable dtCartCopy = dtCart.Copy();
                        string emailTo = txtEmail.Text.Trim();
                        string nameTo = $"{safeName} {safeLastName}".Trim();
                        string shipText = shippingCost == 0 ? "FREE" : $"${shippingCost:F2}";
                        List<string> locParts = new List<string>();
                        if (!string.IsNullOrEmpty(distName)) locParts.Add(distName);
                        if (!string.IsNullOrEmpty(munName)) locParts.Add(munName);
                        if (!string.IsNullOrEmpty(cityName)) locParts.Add(cityName);
                        string locStr = string.Join(", ", locParts);
                        string shipAddrHtml = $@"
                            <p style='color: #555; font-size: 15px; margin: 0; line-height: 1.5;'>
                                <strong>Address:</strong> {HttpUtility.HtmlEncode(txtAddress.Text.Trim())}<br/>
                                {(string.IsNullOrEmpty(locStr) ? "" : $"<strong>Location:</strong> {HttpUtility.HtmlEncode(locStr)}<br/>")}
                                <strong>Phone:</strong> {HttpUtility.HtmlEncode(txtTel.Text.Trim())}
                            </p>";

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            EmailService.SendOrderConfirmation(createdOrderId, total, dtCartCopy, "PayPal", shipText, shipAddrHtml, emailTo, nameTo);
                        });
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "errorPayPalTrans", $"Swal.fire('Database Error', '{ex.Message.Replace("'", "\\'")}', 'error');", true);
                        return;
                    }
                }
            }

            ShowMap = false;
            ShowPaymentLoader = true;
            Session.Remove("Cart");
            string successTitle = GetGlobalResourceObject("Strings", "Checkout_SuccessPaypalTitle")?.ToString() ?? "Payment & Order Completed!";
            string successText = GetGlobalResourceObject("Strings", "Checkout_SuccessPaypalText")?.ToString() ?? "Your order has been verified successfully via PayPal.";
            string script = $"Swal.fire({{ title: '{successTitle.Replace("'", "\\'")}', text: '{successText.Replace("'", "\\'")}', icon: 'success', confirmButtonColor: '#FFC800' }}).then(() => {{ window.location.href = 'MyOrders.aspx'; }});";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "exitoPayPal", script, true);
        }

        protected void btnConfirmWalletPayment_Click(object sender, EventArgs e)
        {
            DataTable dtCart = Session["Cart"] as DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0) return;

            if (!IsPhoneValid(txtTel.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "invalidPhoneWallet", "Swal.fire('Invalid Phone', 'The phone number must contain exactly 8 digits.', 'error');", true);
                return;
            }

            UpdateUserProfileOnCheckout();

            if (!decimal.TryParse(hfTotalAmount.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal total))
            {
                total = 0m;
            }

            decimal shippingCost = CalcularCostoEnvio();
            int userId = Convert.ToInt32(Session["Id_User"]);
            object idCoupon = ViewState["CouponId"] ?? DBNull.Value;
            decimal discountApplied = ViewState["DiscountAmount"] != null ? Convert.ToDecimal(ViewState["DiscountAmount"], System.Globalization.CultureInfo.InvariantCulture) : 0m;
            string transactionId = hfTransactionID.Value;

            string cityName = ddlCity.SelectedIndex > 0 ? ddlCity.SelectedItem.Text : "";
            string munName = ddlMunicipality.SelectedIndex > 0 ? ddlMunicipality.SelectedItem.Text : "";
            string distName = ddlDistrict.SelectedIndex > 0 ? ddlDistrict.SelectedItem.Text : "";

            decimal mapLat, mapLng;
            if (!decimal.TryParse(hfLatitude.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mapLat)) GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            if (!decimal.TryParse(hfLongitude.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mapLng)) GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);

            string safeNotes = txtNotes.Text.Length > 200 ? txtNotes.Text.Substring(0, 200) : txtNotes.Text;
            string safeName = txtName.Text.Trim().Length > 50 ? txtName.Text.Trim().Substring(0, 50) : txtName.Text.Trim();
            string safeLastName = txtLastName.Text.Trim().Length > 50 ? txtLastName.Text.Trim().Substring(0, 50) : txtLastName.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // INSERTA LA ORDEN
                        string orderQuery = @"INSERT INTO orders 
                     (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, TransactionID, shipping_cost, Id_Status) 
                     VALUES 
                     (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, @IdPaymentMethod, @TransactionID, @ShippingCost, @IdStatus); 
                     SELECT LAST_INSERT_ID();";

                        int orderId;
                        using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@IdUser", userId);
                            cmd.Parameters.AddWithValue("@Name", safeName);
                            cmd.Parameters.AddWithValue("@LastName", safeLastName);
                            cmd.Parameters.AddWithValue("@Mail", txtEmail.Text.Trim());
                            cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                            cmd.Parameters.AddWithValue("@Lat", mapLat);
                            cmd.Parameters.AddWithValue("@Lng", mapLng);
                            cmd.Parameters.AddWithValue("@IdCity", string.IsNullOrEmpty(ddlCity.SelectedValue) ? (object)DBNull.Value : ddlCity.SelectedValue);
                            cmd.Parameters.AddWithValue("@IdMunicipality", string.IsNullOrEmpty(ddlMunicipality.SelectedValue) ? (object)DBNull.Value : ddlMunicipality.SelectedValue);
                            cmd.Parameters.AddWithValue("@IdDistrict", string.IsNullOrEmpty(ddlDistrict.SelectedValue) ? (object)DBNull.Value : ddlDistrict.SelectedValue);
                            cmd.Parameters.AddWithValue("@Phone", txtTel.Text.Trim());
                            cmd.Parameters.AddWithValue("@Notes", safeNotes);
                            cmd.Parameters.AddWithValue("@Total", total);
                            cmd.Parameters.AddWithValue("@IdCoupon", idCoupon);
                            cmd.Parameters.AddWithValue("@DiscountApplied", discountApplied);
                            cmd.Parameters.AddWithValue("@IdPaymentMethod", 3); // Billetera Virtual
                            cmd.Parameters.AddWithValue("@TransactionID", string.IsNullOrEmpty(transactionId) ? (object)DBNull.Value : transactionId);
                            cmd.Parameters.AddWithValue("@ShippingCost", shippingCost);

                            // IMPORTANTE: GUARDAR COMO PAGADO (2)
                            cmd.Parameters.AddWithValue("@IdStatus", 2);

                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // INSERTA DETALLES Y DESCUENTA INVENTARIO
                        string detailQuery = @"INSERT INTO order_details (Id_Order, Id_Tshirt, ProductName, Size, Price, Quantity, Subtotal) 
                                       VALUES (@IdOrder, @IdTshirt, @Name, @Size, @Price, @Qty, @Subtotal)";

                        using (MySqlCommand detailCmd = new MySqlCommand(detailQuery, conn, trans))
                        {
                            detailCmd.Parameters.Add("@IdOrder", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@IdTshirt", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@Name", MySqlDbType.VarChar);
                            detailCmd.Parameters.Add("@Size", MySqlDbType.VarChar);
                            detailCmd.Parameters.Add("@Price", MySqlDbType.Decimal);
                            detailCmd.Parameters.Add("@Qty", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@Subtotal", MySqlDbType.Decimal);

                            foreach (DataRow row in dtCart.Rows)
                            {
                                // DESCONTAR INVENTARIO
                                string updateStock = @"UPDATE tshirt_variants SET Stock = Stock - @Qty WHERE Id_Tshirt = @IdTshirt AND Id_Size = (SELECT Id_Size FROM sizes WHERE Size_Code = @Size)";
                                using (MySqlCommand stockCmd = new MySqlCommand(updateStock, conn, trans))
                                {
                                    stockCmd.Parameters.AddWithValue("@Qty", row["Quantity"]);
                                    stockCmd.Parameters.AddWithValue("@IdTshirt", row["ID"]);
                                    stockCmd.Parameters.AddWithValue("@Size", row["Size"]);
                                    stockCmd.ExecuteNonQuery();
                                }

                                string dbProductName = row["Name"].ToString();
                                if (dtCart.Columns.Contains("IsCustomized") && row["IsCustomized"] != DBNull.Value && Convert.ToBoolean(row["IsCustomized"]))
                                {
                                    string customLabel = (Session["Language"] != null && Session["Language"].ToString().ToLower() == "es") ? "Personalizado" : "Customized";
                                    dbProductName += $" ({customLabel}: {row["CustomName"]} #{row["CustomNumber"]})";
                                }

                                detailCmd.Parameters["@IdOrder"].Value = orderId;
                                detailCmd.Parameters["@IdTshirt"].Value = row["ID"];
                                detailCmd.Parameters["@Name"].Value = dbProductName;
                                detailCmd.Parameters["@Size"].Value = row["Size"];
                                detailCmd.Parameters["@Price"].Value = row["Price"];
                                detailCmd.Parameters["@Qty"].Value = row["Quantity"];
                                detailCmd.Parameters["@Subtotal"].Value = row["Subtotal"];

                                detailCmd.ExecuteNonQuery();
                            }
                        }

                        if (idCoupon != DBNull.Value)
                        {
                            string updateCoupon = "UPDATE coupons SET UsedCount = UsedCount + 1 WHERE Id_Coupon = @IdCoupon;";
                            using (MySqlCommand cmdCoupon = new MySqlCommand(updateCoupon, conn, trans))
                            {
                                cmdCoupon.Parameters.AddWithValue("@IdCoupon", idCoupon);
                                cmdCoupon.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();

                        int createdOrderId = orderId;
                        DataTable dtCartCopy = dtCart.Copy();
                        string emailTo = txtEmail.Text.Trim();
                        string nameTo = $"{safeName} {safeLastName}".Trim();
                        string shipText = shippingCost == 0 ? "FREE" : $"${shippingCost:F2}";
                        List<string> locParts = new List<string>();
                        if (!string.IsNullOrEmpty(distName)) locParts.Add(distName);
                        if (!string.IsNullOrEmpty(munName)) locParts.Add(munName);
                        if (!string.IsNullOrEmpty(cityName)) locParts.Add(cityName);
                        string locStr = string.Join(", ", locParts);
                        string shipAddrHtml = $@"
                            <p style='color: #555; font-size: 15px; margin: 0; line-height: 1.5;'>
                                <strong>Address:</strong> {HttpUtility.HtmlEncode(txtAddress.Text.Trim())}<br/>
                                {(string.IsNullOrEmpty(locStr) ? "" : $"<strong>Location:</strong> {HttpUtility.HtmlEncode(locStr)}<br/>")}
                                <strong>Phone:</strong> {HttpUtility.HtmlEncode(txtTel.Text.Trim())}
                            </p>";

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            EmailService.SendOrderConfirmation(createdOrderId, total, dtCartCopy, "Virtual Wallet", shipText, shipAddrHtml, emailTo, nameTo);
                        });
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        string safeErrorMsg = HttpUtility.JavaScriptStringEncode(ex.Message);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "errorWalletTrans", $"Swal.fire('Database Error', '{safeErrorMsg}', 'error');", true);
                        return;
                    }
                }
            }

            ShowMap = false;
            ShowPaymentLoader = true;
            Session.Remove("Cart");

            string successTitle = GetGlobalResourceObject("Strings", "Checkout_SuccessWalletTitle")?.ToString() ?? "Order Placed!";
            string successText = GetGlobalResourceObject("Strings", "Checkout_SuccessWalletText")?.ToString() ?? "Your order has been placed successfully via Virtual Wallet.";

            string safeTitle = HttpUtility.JavaScriptStringEncode(successTitle);
            string safeText = HttpUtility.JavaScriptStringEncode(successText);

            string script = $"Swal.fire({{ title: '{safeTitle}', text: '{safeText}', icon: 'success', confirmButtonColor: '#FFC800' }}).then(() => {{ window.location.href = 'MyOrders.aspx'; }});";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "exitoWallet", script, true);
        }

        private void UpdateUserProfileOnCheckout()
        {
            if (Session["Id_User"] == null) return;
            int userId = Convert.ToInt32(Session["Id_User"]);

            string safeName = txtName.Text.Trim().Length > 50 ? txtName.Text.Trim().Substring(0, 50) : txtName.Text.Trim();
            string safeLastName = txtLastName.Text.Trim().Length > 50 ? txtLastName.Text.Trim().Substring(0, 50) : txtLastName.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE users SET Name = @Name, LastName = @LastName, Phone = @Phone, Address = @Address, id_city = @IdCity, Id_Municipality = @IdMun, Id_District = @IdDist WHERE Id_User = @UserId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", safeName);
                cmd.Parameters.AddWithValue("@LastName", safeLastName);
                cmd.Parameters.AddWithValue("@Phone", txtTel.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@IdCity", ddlCity.SelectedValue);
                cmd.Parameters.AddWithValue("@IdMun", string.IsNullOrEmpty(ddlMunicipality.SelectedValue) ? (object)DBNull.Value : ddlMunicipality.SelectedValue);
                cmd.Parameters.AddWithValue("@IdDist", string.IsNullOrEmpty(ddlDistrict.SelectedValue) ? (object)DBNull.Value : ddlDistrict.SelectedValue);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private bool IsFormValid()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrEmpty(txtLastName.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) ||
                string.IsNullOrEmpty(txtAddress.Text) ||
                string.IsNullOrEmpty(txtTel.Text) ||
                ddlCity.SelectedIndex <= 0)
            {
                return false;
            }

            if (txtName.Text.Trim().Length > 50 || txtLastName.Text.Trim().Length > 50)
            {
                return false;
            }

            if (!IsPhoneValid(txtTel.Text))
            {
                return false;
            }

            return true;
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon(); Response.Redirect("Login.aspx");
        }
        protected void btnMyOrders_Click(object sender, EventArgs e) { Response.Redirect("MyOrders.aspx"); }
        protected void btnbackshop_Click(object sender, EventArgs e) { Response.Redirect("Homepage.aspx"); }

        private void GetCoordinatesFromAddress(string address, string district, string municipality, string city, out decimal lat, out decimal lng)
        {
            lat = 13.6929m;
            lng = -89.2182m;

            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "OffsideShop Delivery System/1.0");

                    string query1 = $"{address}, {district}, {municipality}, {city}, El Salvador";
                    if (TryGetCoordinates(client, query1, out lat, out lng)) return;

                    string query2 = $"{district}, {municipality}, {city}, El Salvador";
                    if (TryGetCoordinates(client, query2, out lat, out lng)) return;

                    string query3 = $"{municipality}, {city}, El Salvador";
                    if (TryGetCoordinates(client, query3, out lat, out lng)) return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Geocoding: " + ex.Message);
            }
        }

        private bool TryGetCoordinates(System.Net.WebClient client, string query, out decimal lat, out decimal lng)
        {
            lat = 0; lng = 0;
            string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
            string json = client.DownloadString(url);

            if (json.Length > 10 && json.Contains("\"lat\":\""))
            {
                int latIndex = json.IndexOf("\"lat\":\"") + 7;
                string latStr = json.Substring(latIndex, json.IndexOf("\"", latIndex) - latIndex);

                int lonIndex = json.IndexOf("\"lon\":\"") + 7;
                string lonStr = json.Substring(lonIndex, json.IndexOf("\"", lonIndex) - lonIndex);

                if (decimal.TryParse(latStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat) &&
                    decimal.TryParse(lonStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lng))
                {
                    return true;
                }
            }
            return false;
        }

        protected void ddlMunicipality_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMunicipalityId = ddlMunicipality.SelectedValue;
            if (!string.IsNullOrEmpty(selectedMunicipalityId))
            {
                LoadDistrictsByMunicipality(selectedMunicipalityId);
                ddlDistrict.Enabled = true;
            }
            else
            {
                ResetDropdown(ddlDistrict, "- Select District -");
            }

            ActualizarResumenPrecios();
        }

        protected string FormatJerseyName(object nameObj)
        {
            if (nameObj == null || nameObj == DBNull.Value) return "";

            string name = nameObj.ToString().ToLower().Trim();
            System.Globalization.TextInfo textInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(name);
        }

        private bool IsPhoneValid(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(phone.Trim(), @"^[0-9]{8}$");
        }
        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string SaveCheckoutData(System.Collections.Generic.Dictionary<string, string> data)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session["CheckoutData"] = data;
                return "OK";
            }
            return "Error";
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static object CreatePendingWalletOrder(System.Collections.Generic.Dictionary<string, string> data)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
            {
                return new { success = false, message = "No session" };
            }

            var session = HttpContext.Current.Session;
            session["CheckoutData"] = data;

            DataTable dtCart = session["Cart"] as DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0)
            {
                return new { success = false, message = "Cart is empty" };
            }

            int userId = session["Id_User"] != null ? Convert.ToInt32(session["Id_User"]) : 0;
            object idCoupon = session["CouponId"] ?? DBNull.Value;
            decimal discountApplied = session["DiscountAmount"] != null ? Convert.ToDecimal(session["DiscountAmount"], System.Globalization.CultureInfo.InvariantCulture) : 0m;

            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

            string idCity = data.ContainsKey("city") ? data["city"] : "";
            string idMun = data.ContainsKey("municipality") ? data["municipality"] : "";
            string idDist = data.ContainsKey("district") ? data["district"] : "";

            decimal shippingCost = 3.50m;
            if (!string.IsNullOrEmpty(idCity))
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    {
                        conn.Open();
                        using (MySqlCommand cmdCity = new MySqlCommand("SELECT shipping_cost FROM cities WHERE id_city = @id_city", conn))
                        {
                            cmdCity.Parameters.AddWithValue("@id_city", idCity);
                            object res = cmdCity.ExecuteScalar();
                            if (res != null && res != DBNull.Value) shippingCost = Convert.ToDecimal(res);
                        }
                    }
                }
                catch { }
            }

            decimal total = 0m;
            if (data.ContainsKey("total") && !string.IsNullOrEmpty(data["total"]))
            {
                decimal.TryParse(data["total"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out total);
            }
            if (total <= 0)
            {
                decimal sub = 0m;
                foreach (DataRow r in dtCart.Rows) sub += Convert.ToDecimal(r["Subtotal"]);
                total = Math.Max(0m, (sub - discountApplied) + shippingCost);
            }

            decimal mapLat = 13.6929m;
            decimal mapLng = -89.2182m;
            if (data.ContainsKey("lat") && !string.IsNullOrEmpty(data["lat"])) decimal.TryParse(data["lat"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mapLat);
            if (data.ContainsKey("lng") && !string.IsNullOrEmpty(data["lng"])) decimal.TryParse(data["lng"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mapLng);

            string safeNotes = data.ContainsKey("notes") && data["notes"] != null ? data["notes"] : "";
            if (safeNotes.Length > 200) safeNotes = safeNotes.Substring(0, 200);

            string safeName = data.ContainsKey("name") && data["name"] != null ? data["name"].Trim() : "";
            if (safeName.Length > 50) safeName = safeName.Substring(0, 50);

            string safeLastName = data.ContainsKey("lastName") && data["lastName"] != null ? data["lastName"].Trim() : "";
            if (safeLastName.Length > 50) safeLastName = safeLastName.Substring(0, 50);

            string email = data.ContainsKey("email") ? data["email"].Trim() : "";
            string address = data.ContainsKey("address") ? data["address"].Trim() : "";
            if (address.Length > 200) address = address.Substring(0, 200);
            string tel = data.ContainsKey("tel") ? data["tel"].Trim() : "";

            object valCity = int.TryParse(idCity, out int cityId) ? (object)cityId : DBNull.Value;
            object valMun = int.TryParse(idMun, out int munId) ? (object)munId : DBNull.Value;
            object valDist = int.TryParse(idDist, out int distId) ? (object)distId : DBNull.Value;

            string pendingTxId = "PENDING-VW-" + userId + "-" + DateTime.UtcNow.Ticks;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        string orderQuery = @"INSERT INTO orders 
                         (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, TransactionID, shipping_cost, Id_Status) 
                         VALUES 
                         (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, 3, @TransactionID, @ShippingCost, 1); 
                         SELECT LAST_INSERT_ID();";

                        int orderId;
                        using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@IdUser", userId);
                            cmd.Parameters.AddWithValue("@Name", safeName);
                            cmd.Parameters.AddWithValue("@LastName", safeLastName);
                            cmd.Parameters.AddWithValue("@Mail", email);
                            cmd.Parameters.AddWithValue("@Address", address);
                            cmd.Parameters.AddWithValue("@Lat", mapLat);
                            cmd.Parameters.AddWithValue("@Lng", mapLng);
                            cmd.Parameters.AddWithValue("@IdCity", valCity);
                            cmd.Parameters.AddWithValue("@IdMunicipality", valMun);
                            cmd.Parameters.AddWithValue("@IdDistrict", valDist);
                            cmd.Parameters.AddWithValue("@Phone", tel);
                            cmd.Parameters.AddWithValue("@Notes", safeNotes);
                            cmd.Parameters.AddWithValue("@Total", total);
                            cmd.Parameters.AddWithValue("@IdCoupon", idCoupon);
                            cmd.Parameters.AddWithValue("@DiscountApplied", discountApplied);
                            cmd.Parameters.AddWithValue("@TransactionID", pendingTxId);
                            cmd.Parameters.AddWithValue("@ShippingCost", shippingCost);

                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        string detailQuery = @"INSERT INTO order_details (Id_Order, Id_Tshirt, ProductName, Size, Price, Quantity, Subtotal) 
                                               VALUES (@IdOrder, @IdTshirt, @Name, @Size, @Price, @Qty, @Subtotal)";

                        using (MySqlCommand detailCmd = new MySqlCommand(detailQuery, conn, trans))
                        {
                            detailCmd.Parameters.Add("@IdOrder", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@IdTshirt", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@Name", MySqlDbType.VarChar);
                            detailCmd.Parameters.Add("@Size", MySqlDbType.VarChar);
                            detailCmd.Parameters.Add("@Price", MySqlDbType.Decimal);
                            detailCmd.Parameters.Add("@Qty", MySqlDbType.Int32);
                            detailCmd.Parameters.Add("@Subtotal", MySqlDbType.Decimal);

                            bool isSpanish = session["Language"] != null && session["Language"].ToString().ToLower() == "es";

                            foreach (DataRow row in dtCart.Rows)
                            {
                                string dbProductName = row["Name"].ToString();
                                if (dtCart.Columns.Contains("IsCustomized") && row["IsCustomized"] != DBNull.Value && Convert.ToBoolean(row["IsCustomized"]))
                                {
                                    string customLabel = isSpanish ? "Personalizado" : "Customized";
                                    dbProductName += $" ({customLabel}: {row["CustomName"]} #{row["CustomNumber"]})";
                                }

                                detailCmd.Parameters["@IdOrder"].Value = orderId;
                                detailCmd.Parameters["@IdTshirt"].Value = row["ID"];
                                detailCmd.Parameters["@Name"].Value = dbProductName;
                                detailCmd.Parameters["@Size"].Value = row["Size"];
                                detailCmd.Parameters["@Price"].Value = row["Price"];
                                detailCmd.Parameters["@Qty"].Value = row["Quantity"];
                                detailCmd.Parameters["@Subtotal"].Value = row["Subtotal"];
                                detailCmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        session["PendingWalletOrderId"] = orderId;
                        session["PendingWalletTxId"] = pendingTxId;

                        string returnUrl = data.ContainsKey("returnUrl") && !string.IsNullOrEmpty(data["returnUrl"]) ? data["returnUrl"] : "";
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            WalletWebhook.RegisterReturnUrl(orderId, pendingTxId, returnUrl);
                        }

                        return new { success = true, orderId = orderId, txId = pendingTxId };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }
    }
}