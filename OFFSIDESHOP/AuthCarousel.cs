using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace OFFSIDESHOP
{
    public class AuthCarousel
    {
        public int Id_Slide { get; set; }
        public string ImageURL { get; set; }
        public string QuoteText { get; set; }
        public string AuthorName { get; set; }
        public string AuthorRole { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        public static List<AuthCarousel> GetActiveSlides()
        {
            List<AuthCarousel> slides = new List<AuthCarousel>();
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT Id_Slide, ImageURL, QuoteText, AuthorName, AuthorRole, DisplayOrder, IsActive 
                                     FROM auth_carousel 
                                     WHERE IsActive = 1 
                                     ORDER BY DisplayOrder ASC;";
                    
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string rawImg = reader["ImageURL"].ToString();
                            string formattedImg = rawImg;
                            if (!string.IsNullOrWhiteSpace(rawImg) && !rawImg.StartsWith("http") && !rawImg.StartsWith("assets/"))
                            {
                                formattedImg = "images/auth/" + rawImg;
                            }

                            slides.Add(new AuthCarousel
                            {
                                Id_Slide = Convert.ToInt32(reader["Id_Slide"]),
                                ImageURL = formattedImg,
                                QuoteText = reader["QuoteText"].ToString(),
                                AuthorName = reader["AuthorName"].ToString(),
                                AuthorRole = reader["AuthorRole"].ToString(),
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
            }

            // Si no hay slides en la BD (o hay un error), agregar uno por defecto para que no quede vacio
            if (slides.Count == 0)
            {
                slides.Add(new AuthCarousel
                {
                    Id_Slide = 1,
                    ImageURL = "assets/img/loginback2.jpeg", // Default
                    QuoteText = "We've been using Untitled to kick start every new project and can't imagine working without it.",
                    AuthorName = "Olivia Rhye",
                    AuthorRole = "Lead Designer, Layers",
                    DisplayOrder = 1,
                    IsActive = true
                });
            }

            return slides;
        }
    }
}
