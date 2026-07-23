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
    public partial class DeliveryDashboard : System.Web.UI.Page
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // BLOQUEO ABSOLUTO: Solo el Rol 4 (Delivery) puede ver esto
            if (Session["UserRole"] == null || Convert.ToInt32(Session["UserRole"]) != 4)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CheckForActiveMission();
            }
        }

        // Verifica si el repartidor cerró la app mientras tenía un pedido en la mochila
        private void CheckForActiveMission()
        {
            int driverId = Convert.ToInt32(Session["Id_User"]);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Buscamos si tiene alguna orden en estado Shipped (3) asignada a su ID
                string query = "SELECT Id_Order FROM orders WHERE Id_Status = 3 AND Id_DeliveryMan = @DriverId LIMIT 1";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DriverId", driverId);
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // Si ya tiene una misión en curso, lo mandamos directo a esa pantalla
                        chkDutySwitch.Checked = true;
                        UpdateDutyUI();
                        LoadMissionDetails(Convert.ToInt32(result));
                    }
                    else
                    {
                        // Si no, cargamos el Radar
                        UpdateDutyUI();
                        LoadRadar();
                    }
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
            if (chkDutySwitch.Checked)
            {
                lblDutyStatus.Text = "On Duty";
                lblDutyStatus.CssClass = "status-badge bg-online";
                phOnline.Visible = true;
                phOffline.Visible = false;
            }
            else
            {
                lblDutyStatus.Text = "Offline";
                lblDutyStatus.CssClass = "status-badge bg-offline";
                phOnline.Visible = false;
                phOffline.Visible = true;
            }
        }

        private void LoadRadar()
        {
            if (!chkDutySwitch.Checked) return;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Extraemos todas las órdenes pagadas (Estado 2) que nadie haya tomado aún
                string query = @"
                    SELECT o.Id_Order, o.Total, c.city_name, m.Municipality_Name, 
                           (SELECT SUM(Quantity) FROM order_details WHERE Id_Order = o.Id_Order) AS TotalItems
                    FROM orders o
                    LEFT JOIN cities c ON o.Id_City = c.id_city
                    LEFT JOIN municipalities m ON o.Id_Municipality = m.Id_Municipality
                    WHERE o.Id_Status = 9 AND o.Id_DeliveryMan IS NULL
                    ORDER BY o.OrderDate ASC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
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
                    // Al aceptar, bloqueamos la orden para este repartidor y pasamos a estado Shipped (3)
                    // Cambia la consulta UPDATE por esta:
                    string query = "UPDATE orders SET Id_Status = 3, Id_DeliveryMan = @DriverId WHERE Id_Order = @OrderId AND Id_Status = 9"; using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DriverId", driverId);
                        cmd.Parameters.AddWithValue("@OrderId", orderId);

                        int affected = cmd.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            // INSERTAR el inicio del rastreo en la tabla driver_tracking
                            string initTracking = "INSERT INTO driver_tracking (Id_Driver, CurrentLat, CurrentLng) VALUES (@DriverId, 13.7370, -89.2868) ON DUPLICATE KEY UPDATE CurrentLat=13.7370, CurrentLng=-89.2868;";
                            using (MySqlCommand cmdTrack = new MySqlCommand(initTracking, conn))
                            {
                                cmdTrack.Parameters.AddWithValue("@DriverId", driverId);
                                cmdTrack.ExecuteNonQuery();
                            }
                            LoadMissionDetails(orderId);
                        }
                    }
                }
            }
        }
        [WebMethod(EnableSession = true)]
        public static string UpdateLocation(decimal currentLat, decimal currentLng)
        {
            // Aseguramos capturar el ID correcto
            if (HttpContext.Current.Session["Id_User"] == null) return "No Session";

            int driverId = Convert.ToInt32(HttpContext.Current.Session["Id_User"]);
            string connString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                // UPSERT: Si el repartidor ya existe en la tabla, actualiza. Si no, crea.
                string query = "INSERT INTO driver_tracking (Id_Driver, CurrentLat, CurrentLng) VALUES (@id, @lat, @lng) " +
                               "ON DUPLICATE KEY UPDATE CurrentLat = @lat, CurrentLng = @lng";

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
                    SELECT o.Id_Order, CONCAT(o.Name, ' ', o.LastName) AS ClientName, o.Phone, o.Address, 
                           o.Latitude, o.Longitude, c.city_name, m.Municipality_Name
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
                        lblMissionOrderId.Text = reader["Id_Order"].ToString();
                        lblClientName.Text = reader["ClientName"].ToString();
                        lblClientAddress.Text = $"{reader["Address"]}, {reader["Municipality_Name"]}, {reader["city_name"]}";

                        string phone = reader["Phone"].ToString();
                        btnCallClient.HRef = "tel:" + phone;

                        // Insertamos coordenadas para pintar el mapa
                        hfDestLat.Value = reader["Latitude"] != DBNull.Value ? reader["Latitude"].ToString() : "";
                        hfDestLng.Value = reader["Longitude"] != DBNull.Value ? reader["Longitude"].ToString() : "";
                    }
                }

                // Generar un resumen de qué camisetas lleva en la mochila
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

            mvDriver.ActiveViewIndex = 1; // Cambiamos la vista a Misión Activa
        }

        protected void btnCompleteMission_Click(object sender, EventArgs e)
        {
            int orderId = Convert.ToInt32(lblMissionOrderId.Text);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Pasamos el pedido a Entregado (Estado 4)
                string query = "UPDATE orders SET Id_Status = 4 WHERE Id_Order = @OrderId";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "success", "Swal.fire('Great Job!', 'Order delivered successfully.', 'success');", true);
            mvDriver.ActiveViewIndex = 0;
            ScriptManager.RegisterStartupScript(this, this.GetType(), "stopGPS", "if(navigator.geolocation && trackingWatchId !== null) { navigator.geolocation.clearWatch(trackingWatchId); trackingWatchId = null; }", true);
            LoadRadar();

        }

        protected void btnCancelMission_Click(object sender, EventArgs e)
        {
            int orderId = Convert.ToInt32(lblMissionOrderId.Text);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Si el repartidor tiene un problema (se pinchó la llanta, etc), soltamos el pedido de vuelta a la piscina (Estado 2) y quitamos su ID
                // Cambia la consulta UPDATE por esta:
                string query = "UPDATE orders SET Id_Status = 9, Id_DeliveryMan = NULL WHERE Id_Order = @OrderId"; using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            mvDriver.ActiveViewIndex = 0;
            ScriptManager.RegisterStartupScript(this, this.GetType(), "stopGPS", "if(navigator.geolocation && trackingWatchId !== null) { navigator.geolocation.clearWatch(trackingWatchId); trackingWatchId = null; }", true);
            LoadRadar();
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}