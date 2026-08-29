using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;

namespace OFFSIDESHOP
{
    public static class AuditLogger
    {
          public static void LogActivity(string actionType, string module, string description)
        {
            try
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null || HttpContext.Current.Session["Id_User"] == null)
                {
                    return;
                }

                int idUser = Convert.ToInt32(HttpContext.Current.Session["Id_User"]);
                string ipAddress = HttpContext.Current.Request != null ? HttpContext.Current.Request.UserHostAddress : "Unknown";

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"INSERT INTO activity_logs (Id_User, Action_Type, Module, Description, IP_Address, Created_At) 
                                     VALUES (@Id_User, @Action_Type, @Module, @Description, @IP_Address, @Created_At);";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id_User", idUser);
                        cmd.Parameters.AddWithValue("@Action_Type", actionType);
                        cmd.Parameters.AddWithValue("@Module", module);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@IP_Address", ipAddress);
                        cmd.Parameters.AddWithValue("@Created_At", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error logging activity: " + ex.Message);
            }
        }
    }
}
