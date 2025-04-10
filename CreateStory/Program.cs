using System.Text;
using System.Text.Json;

class Program
{
    private static readonly string apiKey = "apiKey";

    static async Task Main()
    {
        try
        {
            Console.Write("Hikaye Türünü Seçiniz (Macera, Korku, Bilim Kurgu, Fantastik, Komedi): ");
            string genre = Console.ReadLine() ?? "";

            Console.Write("Ana karakteriniz kim: ");
            string character = Console.ReadLine() ?? "";

            Console.Write("Hikaye nerede geçiyor: ");
            string setting = Console.ReadLine() ?? "";

            Console.Write("Hikayenin uzunluğu (kısa/orta/uzun): ");
            string length = Console.ReadLine() ?? "";

            string prompt = $"""
                Lütfen aşağıdaki kriterlere uygun bir hikaye yaz:

                TÜR: {genre}
                KARAKTER: {character}
                MEKAN: {setting}
                UZUNLUK: {length}

                Hikaye yapısı şöyle olmalıdır:

                GİRİŞ (3-4 cümle):
                - Karakteri tanıt
                - Mekanı detaylı anlat
                - Karakterin günlük hayatını özetle

                GELİŞME (4-5 cümle):
                - Beklenmedik bir olay olsun
                - Karakterin karşılaştığı zorlukları anlat
                - Duygusal ve fiziksel detaylar ekle

                SONUÇ (3-4 cümle):
                - Olayın nasıl çözüldüğünü anlat
                - Karakterin nasıl değiştiğini göster
                - Hikayeyi tatmin edici bir şekilde bitir

                ÖNEMLİ KURALLAR:
                - Sadece Türkçe karakterler kullan
                - Tekrar eden cümleler kullanma
                - Her bölüm için belirtilen cümle sayısına uy
                - Diyalog ekle
                - Duygusal ve fiziksel detaylar kullan
                - Hikayeyi akıcı ve ilgi çekici yap
                """;

            string story = await GenerateStory(prompt);
            Console.WriteLine("\n--- AI Tarafında Oluşturulan Hikaye ---\n");
            Console.WriteLine(story);
            Console.WriteLine("\n--- Hikaye Sonu ---");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
        }
    }

    static async Task<string> GenerateStory(string prompt)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "https://openrouter.ai/");
        client.DefaultRequestHeaders.Add("X-Title", "CreateStory");

        var requestBody = new
        {
            model = "google/gemini-2.5-pro-exp-03-25:free",
            messages = new[]
            {
                new { 
                    role = "system", 
                    content = """
                        Sen yaratıcı bir hikaye yazarısın. 
                        Hikayeleri SADECE Türkçe yazıyorsun.
                        Sadece Türkçe karakterler kullanıyorsun.
                        Başka hiçbir dilde karakter kullanmıyorsun.
                        Hikayelerde giriş, gelişme ve sonuç bölümleri olmalı.
                        Karakterler ve mekanlar detaylı anlatılmalı.
                        Tekrar eden cümleler kullanma.
                        Her bölüm için belirtilen cümle sayısına uy.
                        Diyalog ekle.
                        Duygusal ve fiziksel detaylar kullan.
                        Hikayeyi akıcı ve ilgi çekici yap.
                        """
                },
                new { 
                    role = "user", 
                    content = prompt 
                }
            },
            max_tokens = 2000
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", jsonContent);
        
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API hatası: {response.StatusCode} - {errorContent}");
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(responseContent);
        
        if (!doc.RootElement.TryGetProperty("choices", out var choices) || 
            choices.GetArrayLength() == 0 ||
            !choices[0].TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var content))
        {
            throw new Exception("API yanıtı beklenen formatta değil");
        }

        return content.GetString() ?? "Hikaye oluşturulamadı.";
    }
}