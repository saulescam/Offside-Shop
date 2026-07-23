using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;


    namespace OFFSIDESHOP
    {
        public class data
        {
            public static MySqlConnection ObtenerConexion()
            {
                // Usamos el nombre exacto de tu Web.config: "ConnectionDataBase"
                string strConex = ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;
                return new MySqlConnection(strConex);
            }
        }
    }
