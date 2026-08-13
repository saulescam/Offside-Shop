using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class DeliveryDashboard : BasePage
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        // Enumeración de estados de orden
        private enum OrderStatus
        {
            Pending = 1,
            Paid = 2,
            Shipped = 3,         // En camino / Misión activa
            Delivered = 4,       // Entregado
            Cancelled = 5,
            RefundRequested = 6,
            Refunded = 7,
            RefundRejected = 8,
            ReadyForPickup = 9   // En radar / Listo para que repartidor lo tome
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // BLOQUEO ABSOLUTO: Solo el Rol 4 (Delivery) puede acceder
            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 4)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CheckForActiveMission();
                LoadDriverProfile();
            }
        }

        // Verifica si el repartidor tiene una misión en curso activa
        private void CheckForActiveMission()
        {
            int driverId = Convert.ToInt32(Session["Id_User"]);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id_Order FROM orders WHERE Id_Status = @StatusShipped AND Id_DeliveryMan = @DriverId LIMIT 1";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StatusShipped", (int)OrderStatus.Shipped);
                    cmd.Parameters.AddWithValue("@DriverId", driverId);
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        chkDutySwitch.Checked = true;
                        UpdateDutyUI();
                        LoadMissionDetails(Convert.ToInt32(result));
                    }
                    else
                    {
                        UpdateDutyUI();
                        LoadRadar();
                    }
                }
            }
        }

        private void LoadDriverProfile()
        {
            if (Session["Id_User"] != null)
            {
                int driverId = Convert.ToInt32(Session["Id_User"]);
                try
                {
                    using (MySqlConnection con = new MySqlConnection(connectionString))
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("SELECT Name, Surname, Mail FROM users WHERE Id_User = @Id", con);
                        cmd.Parameters.AddWithValue("@Id", driverId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (lblDriverName != null) lblDriverName.Text = $"{reader["Name"]} {reader["Surname"]}".Trim();
                                if (lblDriverEmail != null) lblDriverEmail.Text = reader["Mail"].ToString();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    if (lblDriverName != null) lblDriverName.Text = GetGlobalResourceObject("Strings", "Driver_DefaultName")?.ToString() ?? "Driver";
                }
            }
        }

        protected void chkDuty_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDutyUI();
            LoadRadar();
        }

        private void UpdateDutyUI()
        {
            bool isOnDuty = chkDutySwitch.Checked;
            UpdateDutyBadgeText(isOnDuty);

            if (isOnDuty)
            {
                phOnline.Visible = true;
                phOffline.Visible = false;
            }
            else
            {
                phOnline.Visible = false;
                phOffline.Visible = true;
            }
        }

        private void LoadRadar()
        {
            if (!chkDutySwitch.Checked) return;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Extraemos órdenes listas para retirar (Estado 9) sin repartidor asignado
                string query = @"
                    SELECT o.Id_Order, o.Total, c.city_name, m.Municipality_Name, 
                           (SELECT SUM(Quantity) FROM order_details WHERE Id_Order = o.Id_Order) AS TotalItems
                    FROM orders o
                    LEFT JOIN cities c ON o.Id_City = c.id_city
                    LEFT JOIN municipalities m ON o.Id_Municipality = m.Id_Municipality
                    WHERE o.Id_Status = @StatusReady AND o.Id_DeliveryMan IS NULL
                    ORDER BY o.OrderDate ASC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StatusReady", (int)OrderStatus.ReadyForPickup);

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptRadar.DataSource = dt;
                            rptRadar.DataBind();
                            rptRadar.Visible = true;
                            phNoOrders.Visible = false;
                        }
                        else
                        {
                            rptRadar.Visible = false;
                            phNoOrders.Visible = true;
                        }
                    }
                }
            }
        }

        protected void rptRadar_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ACCEPT")
            {
                int orderId = Convert.ToInt32(e.CommandArgument);
                int driverId = Convert.ToInt32(Session["Id_User"]);

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Control de condición de carrera
                    string query = @"UPDATE orders 
                                    SET Id_Status = @StatusShipped, Id_DeliveryMan = @DriverId 
                                    WHERE Id_Order = @OrderId AND Id_Status = @StatusReady AND Id_DeliveryMan IS NULL";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StatusShipped", (int)OrderStatus.Shipped);
                        cmd.Parameters.AddWithValue("@DriverId", driverId);
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        cmd.Parameters.AddWithValue("@StatusReady", (int)OrderStatus.ReadyForPickup);

                        int affected = cmd.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            string initTracking = @"INSERT INTO driver_tracking (Id_Driver, Id_ActiveOrder, CurrentLat, CurrentLng) 
                                                    VALUES (@DriverId, @OrderId, 13.7370, -89.2868) 
                                                    ON DUPLICATE KEY UPDATE Id_ActiveOrder = @OrderId;";
                            using (MySqlCommand cmdTrack = new MySqlCommand(initTracking, conn))
                            {
                                cmdTrack.Parameters.AddWithValue("@DriverId", driverId);
                                cmdTrack.Parameters.AddWithValue("@OrderId", orderId);
                                cmdTrack.ExecuteNonQuery();
                            }

                            LoadMissionDetails(orderId);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "orderTaken",
                                AlertHelper.GetSafeAlertScript(this, "Alert_Driver_TooLateTitle", "Alert_Driver_TooLateText", "info"), true);
                            LoadRadar();
                        }
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static string UpdateLocation(decimal currentLat, decimal currentLng)
        {
            if (HttpContext.Current.Session["Id_User"] == null) return "No Session";

            int driverId = Convert.ToInt32(HttpContext.Current.Session["Id_User"]);
            string connString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                string query = @"INSERT INTO driver_tracking (Id_Driver, CurrentLat, CurrentLng) 
                                 VALUES (@id, @lat, @lng) 
                                 ON DUPLICATE KEY UPDATE CurrentLat = @lat, CurrentLng = @lng, LastUpdate = NOW()";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", driverId);
                    cmd.Parameters.AddWithValue("@lat", currentLat);
                    cmd.Parameters.AddWithValue("@lng", currentLng);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return "OK";
        }

        private void LoadMissionDetails(int orderId)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = @"
                    SELECT o.Id_Order, CONCAT(o.Name, ' ', o.LastName) AS ClientName, o.Phone, o.Address, o.OrderNotes,
                           o.Latitude, o.Longitude, c.city_name, m.Municipality_Name, o.Total, o.Id_PaymentMethod
                    FROM orders o
                    LEFT JOIN cities c ON o.Id_City = c.id_city
                    LEFT JOIN municipalities m ON o.Id_Municipality = m.Id_Municipality
                    WHERE o.Id_Order = @OrderId";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);

                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string clientName = reader["ClientName"].ToString();
                        string address = $"{reader["Address"]}, {reader["Municipality_Name"]}, {reader["city_name"]}";
                        string phone = reader["Phone"].ToString().Replace("-", "").Replace(" ", "");
                        string notes = reader["OrderNotes"] != DBNull.Value ? reader["OrderNotes"].ToString() : "";
                        string lat = reader["Latitude"] != DBNull.Value ? reader["Latitude"].ToString() : "";
                        string lng = reader["Longitude"] != DBNull.Value ? reader["Longitude"].ToString() : "";

                        decimal total = reader["Total"] != DBNull.Value ? Convert.ToDecimal(reader["Total"]) : 0;
                        int paymentMethodId = reader["Id_PaymentMethod"] != DBNull.Value ? Convert.ToInt32(reader["Id_PaymentMethod"]) : 1;

                        lblMissionOrderId.Text = reader["Id_Order"].ToString();
                        lblClientName.Text = clientName;
                        lblClientAddress.Text = address;

                        // Evaluación del Método de Pago:
                        if (paymentMethodId == 1) // 1 = Cash on Delivery
                        {
                            string collectFormat = GetGlobalResourceObject("Strings", "Driver_PaymentStatus_Collect")?.ToString() ?? "Collect in cash: {0:C}";
                            lblPaymentStatus.Text = string.Format(collectFormat, total);
                            lblPaymentStatus.CssClass = "badge bg-warning text-dark fs-6 py-2 px-3 w-100";
                        }
                        else // Pago Online (PayPal / Tarjeta)
                        {
                            string paidText = GetGlobalResourceObject("Strings", "Driver_PaymentStatus_Paid")?.ToString() ?? "Paid (Online)";
                            lblPaymentStatus.Text = paidText;
                            lblPaymentStatus.CssClass = "badge bg-success text-white fs-6 py-2 px-3 w-100";
                        }

                        // Configuración de botones de contacto
                        btnCallClient.HRef = "tel:" + phone;
                        string greetingPattern = GetGlobalResourceObject("Strings", "Driver_WhatsappGreeting")?.ToString() ?? "Hola {0}, soy tu repartidor de OffsideShop con tu pedido #{1}.";
                        btnWhatsappClient.HRef = $"https://wa.me/503{phone}?text=" + HttpUtility.UrlEncode(string.Format(greetingPattern, clientName, orderId));

                        // Configuración de botones GPS externos
                        if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lng))
                        {
                            btnGoogleMaps.HRef = $"https://www.google.com/maps/dir/?api=1&destination={lat},{lng}&travelmode=driving";
                            btnWaze.HRef = $"https://waze.com/ul?ll={lat},{lng}&navigate=yes";
                            hfDestLat.Value = lat;
                            hfDestLng.Value = lng;
                        }
                        else
                        {
                            btnGoogleMaps.HRef = "#";
                            btnWaze.HRef = "#";
                            hfDestLat.Value = "";
                            hfDestLng.Value = "";
                        }

                        // Mostrar notas del cliente si existen
                        if (!string.IsNullOrWhiteSpace(notes))
                        {
                            phOrderNotes.Visible = true;
                            lblOrderNotes.Text = HttpUtility.HtmlEncode(notes);
                        }
                        else
                        {
                            phOrderNotes.Visible = false;
                        }
                    }
                }

                // Generar lista de productos
                string contentsQuery = "SELECT Quantity, ProductName FROM order_details WHERE Id_Order = @OrderId";
                MySqlCommand cmdContents = new MySqlCommand(contentsQuery, conn);
                cmdContents.Parameters.AddWithValue("@OrderId", orderId);

                string items = "";
                using (MySqlDataReader readerContents = cmdContents.ExecuteReader())
                {
                    while (readerContents.Read())
                    {
                        items += $"• {readerContents["Quantity"]}x {readerContents["ProductName"]}<br/>";
                    }
                }
                lblPackageContents.Text = items;
            }

            mvDriver.ActiveViewIndex = 1; // Cambiamos a la vista Misión Activa
        }

        protected void btnCompleteMission_Click(object sender, EventArgs e)
        {
            int orderId = Convert.ToInt32(lblMissionOrderId.Text);
            int driverId = Convert.ToInt32(Session["Id_User"]);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE orders SET Id_Status = @StatusDelivered WHERE Id_Order = @OrderId";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StatusDelivered", (int)OrderStatus.Delivered);
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                string clearTrack = "UPDATE driver_tracking SET Id_ActiveOrder = NULL WHERE Id_Driver = @DriverId";
                using (MySqlCommand cmdTrack = new MySqlCommand(clearTrack, conn))
                {
                    cmdTrack.Parameters.AddWithValue("@DriverId", driverId);
                    cmdTrack.ExecuteNonQuery();
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                AlertHelper.GetSafeAlertScript(this, "Alert_Driver_SuccessTitle", "Alert_Driver_SuccessText", "success"), true);
            mvDriver.ActiveViewIndex = 0;
            LoadRadar();
        }

        protected void btnCancelMission_Click(object sender, EventArgs e)
        {
            int orderId = Convert.ToInt32(lblMissionOrderId.Text);
            int driverId = Convert.ToInt32(Session["Id_User"]);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE orders SET Id_Status = @StatusReady, Id_DeliveryMan = NULL WHERE Id_Order = @OrderId";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StatusReady", (int)OrderStatus.ReadyForPickup);
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                string clearTrack = "UPDATE driver_tracking SET Id_ActiveOrder = NULL WHERE Id_Driver = @DriverId";
                using (MySqlCommand cmdTrack = new MySqlCommand(clearTrack, conn))
                {
                    cmdTrack.Parameters.AddWithValue("@DriverId", driverId);
                    cmdTrack.ExecuteNonQuery();
                }
            }

            mvDriver.ActiveViewIndex = 0;
            LoadRadar();
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            if (Session["Id_User"] != null && int.TryParse(Session["Id_User"].ToString(), out int driverId))
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();

                        string releaseOrderQuery = @"UPDATE orders 
                                                      SET Id_Status = @StatusReady, Id_DeliveryMan = NULL 
                                                      WHERE Id_DeliveryMan = @DriverId AND Id_Status = @StatusShipped";
                        using (MySqlCommand cmdOrder = new MySqlCommand(releaseOrderQuery, conn))
                        {
                            cmdOrder.Parameters.AddWithValue("@StatusReady", (int)OrderStatus.ReadyForPickup);
                            cmdOrder.Parameters.AddWithValue("@StatusShipped", (int)OrderStatus.Shipped);
                            cmdOrder.Parameters.AddWithValue("@DriverId", driverId);
                            cmdOrder.ExecuteNonQuery();
                        }

                        string clearTrackingQuery = @"UPDATE driver_tracking 
                                                      SET Id_ActiveOrder = NULL, 
                                                          LastUpdate = DATE_SUB(NOW(), INTERVAL 1 HOUR) 
                                                      WHERE Id_Driver = @DriverId";
                        using (MySqlCommand cmdTrack = new MySqlCommand(clearTrackingQuery, conn))
                        {
                            cmdTrack.Parameters.AddWithValue("@DriverId", driverId);
                            cmdTrack.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al procesar logout de repartidor: " + ex.Message);
                }
            }

            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

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

        private void UpdateDutyBadgeText(bool isOnDuty)
        {
            string textOnDuty = GetGlobalResourceObject("Strings", "Driver_OnDuty")?.ToString() ?? "On Duty";
            string textOffline = GetGlobalResourceObject("Strings", "Driver_Offline")?.ToString() ?? "Offline";

            if (isOnDuty)
            {
                lblDutyStatus.Text = textOnDuty;
                lblDutyStatus.CssClass = "status-badge bg-online ms-2";
            }
            else
            {
                lblDutyStatus.Text = textOffline;
                lblDutyStatus.CssClass = "status-badge bg-offline ms-2";
            }
        }
    }
}