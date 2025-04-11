using System.Text;
using Newtonsoft.Json;

namespace ImageGeneration;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "api key";
        
        Console.WriteLine("istediginiz gorseli yazin: ");

        //bos gecilirse yazılan text default olarak atanir
        string prompt = Console.ReadLine() ?? "draw a beautiful landscape with mountains";
        
        try
        {
            var imageBytes = await GenerateImageAsync(apiKey, prompt);
            
            if (imageBytes != null && imageBytes.Length > 0)
            {
                string imagePath = "generated_image.png";
                File.WriteAllBytes(imagePath, imageBytes);
                Console.WriteLine($"gorsel basariyla olusturuldu ve kaydedildi: {Path.GetFullPath(imagePath)}");
            }
            else
            {
                Console.WriteLine("hata: gorsel olusturulamadi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"hata: {ex.Message}");
        }
    }
    
    static async Task<byte[]?> GenerateImageAsync(string apiKey, string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        
        string modelEndpoint = "https://api-inference.huggingface.co/models/runwayml/stable-diffusion-v1-5";
        
        var requestBody = new { inputs = prompt };
        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        Console.WriteLine("gorsel olusturuluyor...");
        Console.WriteLine($"Prompt: '{prompt}'");
        Console.WriteLine($"Model: runwayml/stable-diffusion-v1-5");
        
        try
        {
            var response = await client.PostAsync(modelEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("resim ciziliyor...");
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"api hatası: {response.StatusCode}");
                Console.WriteLine($"hata: {responseString}");
                
                if (responseString.Contains("\"estimated_time\""))
                {
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
                        double estimatedTime = errorResponse.estimated_time;
                        Console.WriteLine($"model yukleniyor. tahmini bekleme suresi: {estimatedTime:F1} saniye");
                        
                        int waitTime = (int)(estimatedTime * 1000) + 2000;
                        Console.WriteLine($"bekleniyor... ({waitTime/1000} saniye)");
                        await Task.Delay(waitTime);
                        
                        Console.WriteLine("tekrar deneniyor...");
                        response = await client.PostAsync(modelEndpoint, content);
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("api yaniti alindi, resim işleniyor...");
                            return await response.Content.ReadAsByteArrayAsync();
                        }
                        else
                        {
                            responseString = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"ikinci deneme hatasi: {response.StatusCode}");
                            Console.WriteLine($"hata: {responseString}");
                        }
                    }
                    catch
                    {
                        Console.WriteLine("yeniden deneme sirasinda hata olustu.");
                    }
                }
                
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"istek gonderilirken hata olustu: {ex.Message}");
            return null;
        }
    }
}