using System.Text;
using HtmlAgilityPack;
using System.Text.Json;

class Program
{
    private static readonly string apiKey = "apikeyburaya";

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Analiz yapmak istediğiniz sayfa URL'sini girin: ");
            string url = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("URL boş olamaz!");
                return;
            }

            Console.WriteLine("Web sayfasından içerik çekiliyor...");
            string webContent = await ExtractTextFromWebAsync(url);
            
            if (string.IsNullOrWhiteSpace(webContent))
            {
                Console.WriteLine("Web sayfasından içerik çekilemedi!");
                return;
            }

            // İçeriği 2000 karaktere kısalt
            if (webContent.Length > 2000)
            {
                webContent = webContent.Substring(0, 2000) + "...";
            }

            Console.WriteLine("İçerik analiz ediliyor...");
            await AnalyzeContent(webContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
        }
    }

    static async Task<string> ExtractTextFromWebAsync(string url)
    {
        try
        {
            var web = new HtmlWeb();
            var document = await web.LoadFromWebAsync(url);
            var bodyText = document.DocumentNode.SelectSingleNode("//body")?.InnerText;
            return bodyText ?? "Sayfa bulunamadı veya içerik çekilemedi";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Web sayfası çekilirken hata: {ex.Message}");
            return string.Empty;
        }
    }

    static async Task AnalyzeContent(string text)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
        client.DefaultRequestHeaders.Add("X-Title", "WebScraping");

        var requestBody = new
        {
            model = "openai/gpt-3.5-turbo",
            messages = new[]
            {
                new { 
                    role = "system", 
                    content = @"Sen bir web sayfası içerik analizcisisin. Verilen metni şu şekilde özetle:
                    1. Sayfanın ana konusu nedir?
                    2. Sayfada hangi önemli bilgiler var?
                    3. Varsa öne çıkan özellikler neler?
                    4. Kullanıcıya sunulan temel hizmetler neler?
                    
                    Özeti maddeler halinde ve detaylı bir şekilde yap. Her madde en az 2-3 cümle içermeli."
                },
                new { 
                    role = "user", 
                    content = $"Aşağıdaki web sayfası içeriğini analiz et ve özetle:\n\n{text}" 
                }
            },
            max_tokens = 1000,
            temperature = 0.7
        };

        string json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            Console.WriteLine("OpenRouter API'ye bağlanılıyor...");
            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseString);
                var answer = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                Console.WriteLine("\nSayfa Analizi:");
                Console.WriteLine(answer);
            }
            else
            {
                Console.WriteLine($"Hata: {response.StatusCode}");
                Console.WriteLine($"Hata Detayı: {responseString}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API isteği sırasında hata: {ex.Message}");
        }
    }
}