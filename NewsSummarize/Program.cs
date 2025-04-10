using System.Text;
using System.Text.Json;
using System.Xml.Linq;

class Program
{
    private static readonly string apiKey = "apikey";
    private static readonly string rssFeedUrl = "habersitesi";

    static async Task Main()
    {
        try
        {
            Console.WriteLine("Haberler Sistemden Alınıyor...");
            List<string> articles = await FetchLatestNews(10);

            foreach (var article in articles)
            {
                try
                {
                    Console.WriteLine("\nHaber özeti oluşturuluyor...");
                    string summary = await SummarizeArticle(article);
                    Console.WriteLine("--- AI tarafından özetlenen haber ---\n");
                    Console.WriteLine(summary);
                    Console.WriteLine("-------------------------------------------------\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Haber özetlenirken hata oluştu: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Program çalışırken hata oluştu: {ex.Message}");
        }
    }

    static async Task<List<string>> FetchLatestNews(int count)
    {
        try
        {
            var client = new HttpClient();
            string rssContent = await client.GetStringAsync(rssFeedUrl);

            XDocument doc = XDocument.Parse(rssContent);
            var items = doc.Descendants("item").Take(count);

            List<string> articles = items.Select(item =>
            {
                string title = item.Element("title")?.Value ?? "";
                string description = item.Element("description")?.Value ?? "";
                return $"{title}. {description}";
            }).ToList();

            return articles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Haberler çekilirken hata oluştu: {ex.Message}");
            return new List<string>();
        }
    }

    static async Task<string> SummarizeArticle(string articleText)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
        client.DefaultRequestHeaders.Add("X-Title", "NewsSummarize");

        var requestBody = new
        {
            model = "deepseek/deepseek-v3-base:free",
            messages = new[]
            {
                new { 
                    role = "system", 
                    content = "Sen bir haber özetleme uzmanısın. Verilen haberi 3 cümlede özetle. Özeti Türkçe olarak ver."
                },
                new { 
                    role = "user", 
                    content = $"Bu haberi özetle: {articleText}" 
                }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", jsonContent);
        
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API hatası: {response.StatusCode} - {errorContent}");
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        JsonDocument doc = JsonDocument.Parse(responseContent);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Özet oluşturulamadı.";
    }
}