using System.Text;
using System.Text.Json;

namespace OpenAIChat;

class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = "apikeyburayagelecek";
        Console.WriteLine("gonder gelsin");
        
        var prompt = Console.ReadLine();
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
        client.DefaultRequestHeaders.Add("X-Title", "OpenAIChat");

        var requestBody = new
        {
            model = "openai/gpt-3.5-turbo",
            messages = new []
            {
                new {role="system", content = "you are asistans"},
                new {role="user", content = prompt ?? ""}
            },
            max_tokens = 500
        };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json,Encoding.UTF8,"application/json");

        try
        {
            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseString);
                var answer = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                Console.WriteLine("cevap: ");
                Console.WriteLine(answer);
            }
            else
            {
                Console.WriteLine($"hata: {response.StatusCode}");
                Console.WriteLine($"message: {responseString}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"hata: {e.Message}");
        }
    }
}