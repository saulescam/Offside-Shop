using MySql.Data.MySqlClient;
using System;
using System.Web;
using System.Web.UI;
using BCrypt.Net;

namespace OFFSIDESHOP
{
    public partial class MyAccount : BasePage
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Buffer = true;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (Session["UserRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadNavbarInfo();
                LoadUserData();
            }
        }

        private void LoadNavbarInfo()
        {
            lblFullName.Text = Session["UserFullName"]?.ToString() ?? "User";
            lblUserEmail.Text = Session["UserEmail"]?.ToString() ?? "";

            int role = Session["UserRole"] != null ? Convert.ToInt32(Session["UserRole"]) : 3;
            phNavbarUser.Visible = (role == 3);
            phNavbarAdmin.Visible = (role == 1 || role == 2);
        }

        protected void btnLanguageToggle_Click(object sender, EventArgs e)
        {
            Session["Language"] = (Session["Language"] == null || Session["Language"].ToString() == "en") ? "es" : "en";
            Response.Redirect(Request.RawUrl);
        }

        private void LoadUserData()
        {
            if (Session["Id_User"] == null)
            {
                ShowSweetAlert("Error", "Session Id_User is empty.", "error");
                return;
            }

            int userId = Convert.ToInt32(Session["Id_User"]);

            // Consulta que obtiene también el nombre del Rol mediante INNER JOIN
            string query = @"SELECT u.Name, u.LastName, u.Name_User, u.Mail, u.Phone, u.Address, 
                                    u.Default_Latitude, u.Default_Longitude, r.Name_Role 
                             FROM users u 
                             INNER JOIN roles r ON u.Id_Role = r.Id_Role 
                             WHERE u.Id_User = @ID";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", userId);
                    try
                    {
                        conn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtFirstName.Text = reader["Name"].ToString();
                                txtLastName.Text = reader["LastName"].ToString();
                                txtUsername.Text = reader["Name_User"].ToString();
                                txtEmail.Text = reader["Mail"].ToString();
                                txtPhone.Text = reader["Phone"].ToString();
                                txtAddress.Text = reader["Address"].ToString();

                                // Asignar Nombre del Rol
                                lblAccountRole.Text = reader["Name_Role"].ToString();

                                // Asignamos las coordenadas a los HiddenFields para el mapa
                                hfDefaultLat.Value = reader["Default_Latitude"] != DBNull.Value ? reader["Default_Latitude"].ToString() : "";
                                hfDefaultLng.Value = reader["Default_Longitude"] != DBNull.Value ? reader["Default_Longitude"].ToString() : "";
                            }
                            else
                            {
                                ShowSweetAlert("Data Error", "User ID " + userId + " not found in database.", "warning");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowSweetAlert("Database Error", ex.Message, "error");
                    }
                }
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (Session["Id_User"] == null) return;
            int userId = Convert.ToInt32(Session["Id_User"]);

            string query = @"UPDATE users SET 
                                Name = @Name, 
                                LastName = @LastName, 
                                Name_User = @Username, 
                                Phone = @Phone, 
                                Address = @Address, 
                                Default_Latitude = @Lat, 
                                Default_Longitude = @Lng 
                             WHERE Id_User = @ID";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", txtFirstName.Text.Trim());
                    cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@ID", userId);

                    decimal? lat = null;
                    decimal? lng = null;

                    if (!string.IsNullOrEmpty(hfDefaultLat.Value) && !string.IsNullOrEmpty(hfDefaultLng.Value))
                    {
                        lat = Convert.ToDecimal(hfDefaultLat.Value, System.Globalization.CultureInfo.InvariantCulture);
                        lng = Convert.ToDecimal(hfDefaultLng.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    cmd.Parameters.AddWithValue("@Lat", (object)lat ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Lng", (object)lng ?? DBNull.Value);

                    try
                    {
                        conn.Open();
                        int affected = cmd.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            ShowSweetAlert("Success", "Profile and Default Location updated successfully.", "success");
                            LoadUserData();
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowSweetAlert("Error", ex.Message, "error");
                    }
                }
            }
        }

        protected void btnUpdatePassword_Click(object sender, EventArgs e)
        {
            if (Session["Id_User"] == null) return;
            int userId = Convert.ToInt32(Session["Id_User"]);

            string currentPass = txtCurrentPassword.Text;
            string newPass = txtNewPassword.Text;

            string selectQuery = "SELECT Password FROM users WHERE Id_User = @ID";
            string dbHash = "";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", userId);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null) dbHash = result.ToString();
                }
            }

            if (!string.IsNullOrEmpty(dbHash) && BCrypt.Net.BCrypt.Verify(currentPass, dbHash))
            {
                string newHash = BCrypt.Net.BCrypt.HashPassword(newPass);
                string updateQuery = "UPDATE users SET Password = @Pass WHERE Id_User = @ID";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Pass", newHash);
                        cmd.Parameters.AddWithValue("@ID", userId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        ShowSweetAlert("Success", "Password updated.", "success");
                    }
                }
            }
            else
            {
                ShowSweetAlert("Error", "Invalid current password.", "error");
            }
        }

        private void ShowSweetAlert(string title, string text, string iconType)
        {
            string script = $@"Swal.fire({{title: '{title}', text: '{text}', icon: '{iconType}', confirmButtonColor: '#FFC800'}});";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", script, true);
        }

        protected void btncerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        protected void btnMyOrders_Click(object sender, EventArgs e) { Response.Redirect("MyOrders.aspx"); }
        protected void btnGoToAccount_Click(object sender, EventArgs e) { Response.Redirect("MyAccount.aspx"); }
        protected void btnNavCart_Click(object sender, EventArgs e) { Response.Redirect("Cart.aspx"); }
        protected void lnkForgotPassword_Click(object sender, EventArgs e)
        {
            Session["RecoverEmail"] = txtEmail.Text;
            Response.Redirect("RecoverAccount.aspx");
        }
        protected void btnbackshop_Click(object sender, EventArgs e) { Response.Redirect("Homepage.aspx"); }
    }
}