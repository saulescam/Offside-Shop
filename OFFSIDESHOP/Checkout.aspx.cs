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
    public partial class Checkout : System.Web.UI.Page
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

                        // Hidratar coordenadas predeterminadas de perfil para el mapa de Leaflet
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
                    name += $" <br><small class='text-warning'>(Customized: {row["CustomName"]} #{row["CustomNumber"]})</small>";
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

            decimal discountPercentage = ViewState["DiscountPercentage"] != null ? Convert.ToDecimal(ViewState["DiscountPercentage"]) : 0m;
            decimal discountAmount = Math.Round((subtotalCamisetas * discountPercentage) / 100m, 2);
            ViewState["DiscountAmount"] = discountAmount;

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

        protected void btnApplyCoupon_Click(object sender, EventArgs e)
        {
            string code = txtCouponCode.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                lblCouponMessage.Text = "<span class='text-danger'>Please enter a valid coupon code.</span>";
                lblCouponMessage.Visible = true;
                return;
            }

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
                                lblCouponMessage.Text = "<span class='text-danger'>This coupon is no longer active.</span>";
                            }
                            else if (usedCount >= maxUses)
                            {
                                lblCouponMessage.Text = "<span class='text-danger'>This coupon has reached its usage limit.</span>";
                            }
                            else
                            {
                                ViewState["CouponId"] = reader["Id_Coupon"];
                                ViewState["DiscountPercentage"] = reader["DiscountPercentage"];
                                lblCouponMessage.Text = $"<span class='text-success'><i class='fas fa-check-circle'></i> Coupon applied! ({reader["DiscountPercentage"]}% OFF)</span>";

                                txtCouponCode.Enabled = false;
                                btnApplyCoupon.Enabled = false;
                            }
                        }
                        else
                        {
                            lblCouponMessage.Text = "<span class='text-danger'>Invalid coupon code.</span>";
                        }
                    }
                }
            }
            lblCouponMessage.Visible = true;
            ActualizarResumenPrecios();
        }

        protected void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (!IsFormValid()) return;

            // Validación de Backend para Teléfono
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
            // Intentamos obtener del HiddenField (si el usuario movió el pin) o del Geocoding (si solo eligió en menús)
            if (!decimal.TryParse(hfLatitude.Value, out mapLat))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }
            if (!decimal.TryParse(hfLongitude.Value, out mapLng))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }

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
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@Mail", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@Lat", mapLat);
                        cmd.Parameters.AddWithValue("@Lng", mapLng);
                        cmd.Parameters.AddWithValue("@IdCity", ddlCity.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdMunicipality", string.IsNullOrEmpty(ddlMunicipality.SelectedValue) ? (object)DBNull.Value : ddlMunicipality.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdDistrict", string.IsNullOrEmpty(ddlDistrict.SelectedValue) ? (object)DBNull.Value : ddlDistrict.SelectedValue);
                        cmd.Parameters.AddWithValue("@Phone", txtTel.Text);
                        cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
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
                                dbProductName += $" (Customized: {row["CustomName"]} #{row["CustomNumber"]})";
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
            ScriptManager.RegisterStartupScript(this, this.GetType(), "exito", "Swal.fire({ title: 'Order Placed!', text: 'Your order has been placed successfully.', icon: 'success', confirmButtonColor: '#FFC800' }).then(() => { window.location.href = 'MyOrders.aspx'; });", true);
        }

        protected void btnConfirmPayPalPayment_Click(object sender, EventArgs e)
        {
            DataTable dtCart = Session["Cart"] as DataTable;
            if (dtCart == null || dtCart.Rows.Count == 0) return;

            // Validación de Backend para Teléfono en transacción de PayPal
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

            // Captura de coordenadas espaciales con Geocoding Automático
            string cityName = ddlCity.SelectedIndex > 0 ? ddlCity.SelectedItem.Text : "";
            string munName = ddlMunicipality.SelectedIndex > 0 ? ddlMunicipality.SelectedItem.Text : "";
            string distName = ddlDistrict.SelectedIndex > 0 ? ddlDistrict.SelectedItem.Text : "";

            decimal mapLat, mapLng;
            // Intentamos obtener del HiddenField (si el usuario movió el pin) o del Geocoding (si solo eligió en menús)
            if (!decimal.TryParse(hfLatitude.Value, out mapLat))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }
            if (!decimal.TryParse(hfLongitude.Value, out mapLng))
            {
                GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Inyección de las coordenadas espaciales en la orden de PayPal
                        string orderQuery = @"INSERT INTO orders 
                             (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, TransactionID, shipping_cost, Id_Status) 
                             VALUES 
                             (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, @IdPaymentMethod, @TransactionID, @ShippingCost, @IdStatus); 
                             SELECT LAST_INSERT_ID();";

                        MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans);
                        cmd.Parameters.AddWithValue("@IdUser", userId);
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@Mail", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@Lat", mapLat);
                        cmd.Parameters.AddWithValue("@Lng", mapLng);
                        cmd.Parameters.AddWithValue("@IdCity", ddlCity.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdMunicipality", string.IsNullOrEmpty(ddlMunicipality.SelectedValue) ? (object)DBNull.Value : ddlMunicipality.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdDistrict", string.IsNullOrEmpty(ddlDistrict.SelectedValue) ? (object)DBNull.Value : ddlDistrict.SelectedValue);
                        cmd.Parameters.AddWithValue("@Phone", txtTel.Text);
                        cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
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
                                dbProductName += $" (Customized: {row["CustomName"]} #{row["CustomNumber"]})";
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
            ScriptManager.RegisterStartupScript(this, this.GetType(), "exitoPayPal", "Swal.fire({ title: 'Payment & Order Completed!', text: 'Your order has been verified successfully via PayPal.', icon: 'success', confirmButtonColor: '#FFC800' }).then(() => { window.location.href = 'MyOrders.aspx'; });", true);
        }

        private void UpdateUserProfileOnCheckout()
        {
            if (Session["Id_User"] == null) return;
            int userId = Convert.ToInt32(Session["Id_User"]);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE users SET Name = @Name, LastName = @LastName, Phone = @Phone, Address = @Address, id_city = @IdCity, Id_Municipality = @IdMun, Id_District = @IdDist WHERE Id_User = @UserId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
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

            // Validar que el formato del teléfono sea estrictamente numérico de 8 dígitos
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

        // --- MOTOR DE GEOCODING (OPCIÓN 2) ---
        private void GetCoordinatesFromAddress(string address, string district, string municipality, string city, out decimal lat, out decimal lng)
        {
            lat = 13.6929m; // Por defecto San Salvador si todo falla
            lng = -89.2182m;

            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    // OpenStreetMap exige un User-Agent para funcionar
                    client.Headers.Add("User-Agent", "OffsideShop Delivery System/1.0");

                    // Intento 1: Dirección exacta (Ej. santa tecla, Ciudad Arce, La Libertad Centro, La Libertad, El Salvador)
                    string query1 = $"{address}, {district}, {municipality}, {city}, El Salvador";
                    if (TryGetCoordinates(client, query1, out lat, out lng)) return;

                    // Intento 2 (Fallback): Solo Distrito y Municipio (Más seguro si la calle está mal escrita)
                    string query2 = $"{district}, {municipality}, {city}, El Salvador";
                    if (TryGetCoordinates(client, query2, out lat, out lng)) return;

                    // Intento 3 (Fallback Seguro): Solo Municipio (A prueba de fallos)
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

            // Buscamos las coordenadas en la respuesta de la API usando lógica de texto simple
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

            // Ejecutar la actualización del total sin llamar a APIs externas síncronas.
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
            // Expresión regular: Asegura que el string contenga exactamente 8 dígitos del 0 al 9
            return System.Text.RegularExpressions.Regex.IsMatch(phone.Trim(), @"^[0-9]{8}$");
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

            decimal total = Convert.ToDecimal(hfTotalAmount.Value, System.Globalization.CultureInfo.InvariantCulture);
            decimal shippingCost = CalcularCostoEnvio();
            int userId = Convert.ToInt32(Session["Id_User"]);
            object idCoupon = ViewState["CouponId"] ?? DBNull.Value;
            decimal discountApplied = ViewState["DiscountAmount"] != null ? Convert.ToDecimal(ViewState["DiscountAmount"]) : 0m;

            string cityName = ddlCity.SelectedIndex > 0 ? ddlCity.SelectedItem.Text : "";
            string munName = ddlMunicipality.SelectedIndex > 0 ? ddlMunicipality.SelectedItem.Text : "";
            string distName = ddlDistrict.SelectedIndex > 0 ? ddlDistrict.SelectedItem.Text : "";

            decimal mapLat, mapLng;
            if (!decimal.TryParse(hfLatitude.Value, out mapLat)) GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);
            if (!decimal.TryParse(hfLongitude.Value, out mapLng)) GetCoordinatesFromAddress(txtAddress.Text, distName, munName, cityName, out mapLat, out mapLng);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string orderQuery = @"INSERT INTO orders 
                     (Id_User, Name, LastName, Mail, Address, Latitude, Longitude, id_City, Id_Municipality, Id_District, Phone, OrderNotes, Total, Id_Coupon, DiscountApplied, Id_PaymentMethod, shipping_cost, Id_Status) 
                     VALUES 
                     (@IdUser, @Name, @LastName, @Mail, @Address, @Lat, @Lng, @IdCity, @IdMunicipality, @IdDistrict, @Phone, @Notes, @Total, @IdCoupon, @DiscountApplied, @IdPaymentMethod, @ShippingCost, @IdStatus); 
                     SELECT LAST_INSERT_ID();";

                        MySqlCommand cmd = new MySqlCommand(orderQuery, conn, trans);
                        cmd.Parameters.AddWithValue("@IdUser", userId);
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@Mail", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@Lat", mapLat);
                        cmd.Parameters.AddWithValue("@Lng", mapLng);
                        cmd.Parameters.AddWithValue("@IdCity", ddlCity.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdMunicipality", string.IsNullOrEmpty(ddlMunicipality.SelectedValue) ? (object)DBNull.Value : ddlMunicipality.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdDistrict", string.IsNullOrEmpty(ddlDistrict.SelectedValue) ? (object)DBNull.Value : ddlDistrict.SelectedValue);
                        cmd.Parameters.AddWithValue("@Phone", txtTel.Text);
                        cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
                        cmd.Parameters.AddWithValue("@Total", total);
                        cmd.Parameters.AddWithValue("@IdCoupon", idCoupon);
                        cmd.Parameters.AddWithValue("@DiscountApplied", discountApplied);

                        // AQUÍ ESTÁ LA CLAVE: Asignar el ID correspondiente a la Billetera Virtual
                        cmd.Parameters.AddWithValue("@IdPaymentMethod", 3); // Ajusta este número según tu base de datos
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
                                dbProductName += $" (Customized: {row["CustomName"]} #{row["CustomNumber"]})";
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
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "errorWalletTrans", $"Swal.fire('Database Error', '{ex.Message.Replace("'", "\\'")}', 'error');", true);
                        return;
                    }
                }
            }

            ShowMap = false;
            ShowPaymentLoader = true;
            Session.Remove("Cart");
            ScriptManager.RegisterStartupScript(this, this.GetType(), "exitoWallet", "Swal.fire({ title: 'Payment & Order Completed!', text: 'Your order has been verified successfully via Virtual Wallet.', icon: 'success', confirmButtonColor: '#FFC800' }).then(() => { window.location.href = 'MyOrders.aspx'; });", true);
        }
    }
}