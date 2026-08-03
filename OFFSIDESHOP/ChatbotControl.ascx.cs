using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace OFFSIDESHOP
{
    public partial class ChatbotControl : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CheckChatbotStatus();
            }
        }

        private void CheckChatbotStatus()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;
            string query = "SELECT SettingValue FROM system_settings WHERE SettingKey = 'Chatbot_Enabled';";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            if (result.ToString() == "0")
                            {
                                this.Visible = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatbotControl Error]: {ex.Message}");
            }
        }
    }
}