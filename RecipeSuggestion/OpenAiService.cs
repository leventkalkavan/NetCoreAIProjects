using System.Text;
using System.Text.Json;

namespace RecipeSuggestion
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private const string OpenAiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private const string apiKey = "apiKey";

        public OpenAiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://your-app-url.com");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "RecipeSuggestion");
        }

        public async Task<string> GetRecipeAsync(string ingredients)
        {
            var requestBody = new
            {
                model = "google/gemini-2.5-pro-exp-03-25:free",
                messages = new[]
                {
                    new {role="system",content="Sen profesyonel bir aşçısın. Kullanıcının elindeki malzemelere göre yaratıcı ve lezzetli yemek tarifleri öner. Adım adım talimatlar ver."},
                    new {role="user",content=$"Elimde sadece şu malzemeler var: {ingredients}. Bu malzemelerle yapabileceğim en iyi tarif nedir?"}
                },
                temperature = 0.7
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(OpenAiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code {response.StatusCode}: {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0 &&
                    choices[0].TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var messageContent))
                {
                    return messageContent.GetString() ?? "Tarif alınamadı.";
                }
                else
                {
                    throw new JsonException("API response format is unexpected.");
                }
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse API response: {ex.Message}. Response body: {responseBody}", ex);
            }
        }
    }
}