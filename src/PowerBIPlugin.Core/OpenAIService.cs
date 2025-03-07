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
        private static readonly string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public static async Task<string> GetResponseFromOpenAI(string prompt)
        {
            Logger.Log($"OPENAI_API_KEY: {apiKey}");
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestData = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = "You are an assistant that helps optimize Power BI queries." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 100
                };

                Logger.Log($"requestData : \"n {requestData}");

                string json = JsonSerializer.Serialize(requestData);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                Logger.Log($"json : \"n {json}");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                Logger.Log($"result : {result}");
                return result;
            }
        }
    }
}
