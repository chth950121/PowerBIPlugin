using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PowerBIPlugin;

namespace PowerBIPlugin
{
    public class OpenAIService // ⬅ Make this public
    {
        private static readonly string apiKey = "your_openai_api_key"; // Replace with your actual key
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public static async Task<string> GetResponseFromOpenAI(string prompt, List<string> queries)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                string param = prompt;
                foreach (string query in queries)
                {
                    param += $"\n{query}";
                }

                Logger.Log($"Asked OpenAI: \n{param}");

                var requestData = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = "You are an assistant that helps optimize Power BI queries." },
                        new { role = "user", content = param }
                    },
                    max_tokens = 100
                };

                string json = JsonSerializer.Serialize(requestData);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                Logger.Log(result);
                return result;
            }
        }
    }
}
