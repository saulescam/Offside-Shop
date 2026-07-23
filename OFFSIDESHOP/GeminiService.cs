using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace OFFSIDESHOP
{
    public class GeminiService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];

        // FIX: Changed default model from 'gemini-1.5-flash' to 'gemini-1.5-flash-latest'
        // Método genérico para llamar a Gemini - Restaurado a flash
        public async Task<string> CallGeminiAsync(string prompt, string model = "gemini-3.5-flash", string systemInstruction = null)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new JObject
            {
                ["contents"] = new JArray
                {
                    new JObject
                    {
                        ["parts"] = new JArray { new JObject { ["text"] = prompt } }
                    }
                }
            };

            if (!string.IsNullOrEmpty(systemInstruction))
            {
                requestBody["system_instruction"] = new JObject
                {
                    ["parts"] = new JObject { ["text"] = systemInstruction }
                };
            }

            var content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = JObject.Parse(responseString);
                return jsonResponse["candidates"][0]["content"]["parts"][0]["text"].ToString();
            }

            throw new Exception("Error en API de Gemini: " + responseString);
        }
    }
}