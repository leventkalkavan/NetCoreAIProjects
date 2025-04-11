using System.Text;
using Newtonsoft.Json;
using UglyToad.PdfPig;

namespace PdfAnalyze;

class Program
{
    private static readonly string apiKey = "apikey";
    private static readonly string pdfPath = "pdfYolu";

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("PDF Analizi başlatılıyor...");
            Console.WriteLine($"Analiz edilecek dosya: {pdfPath}");
            Console.WriteLine();

            if (!File.Exists(pdfPath))
            {
                Console.WriteLine("Hata: PDF dosyası bulunamadı!");
                return;
            }

            string pdfText = ExtractTextFromPdf(pdfPath);
            if (string.IsNullOrWhiteSpace(pdfText))
            {
                Console.WriteLine("Hata: PDF'den metin çekilemedi!");
                return;
            }

            await AnalyzeWithAI(pdfText, "PDF İçeriği");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Genel hata: {ex.Message}");
        }
    }

    static string ExtractTextFromPdf(string filePath)
    {
        try
        {
            StringBuilder text = new StringBuilder();
            using (PdfDocument pdf = PdfDocument.Open(filePath))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }
            }
            return text.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PDF okuma hatası: {ex.Message}");
            return string.Empty;
        }
    }

    static async Task AnalyzeWithAI(string text, string sourceType)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "deepseek/deepseek-v3-base:free",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"Aşağıdaki PDF içeriğini analiz et ve özetle. Özellikle şu konulara dikkat et:\n" +
                                  "1. Dokümanın ana konusu ve amacı nedir?\n" +
                                  "2. Önemli bilgiler ve başlıklar neler?\n" +
                                  "3. Varsa öne çıkan bölümler neler?\n" +
                                  "4. Genel değerlendirme ve özet.\n\n" +
                                  $"İçerik:\n{text}"
                    }
                }
            };

            string json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                Console.WriteLine("AI analizi yapılıyor...");
                HttpResponseMessage response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                string responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    if (result?.choices?[0]?.message?.content != null)
                    {
                        Console.WriteLine($"\nAI Analizi ({sourceType}):");
                        Console.WriteLine(result.choices[0].message.content);
                    }
                    else
                    {
                        Console.WriteLine("Hata: API yanıtı beklenen formatta değil.");
                    }
                }
                else
                {
                    Console.WriteLine($"Hata: {response.StatusCode}");
                    Console.WriteLine($"Hata Detayı: {responseJson}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API isteği sırasında hata: {ex.Message}");
            }
        }
    }
}