using System.Reflection;
using AssemblyAI;
using AssemblyAI.Transcripts;

namespace SpeechToText;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "apikeyburaya";
        
        string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        string audioFilePath = Path.Combine(exePath, "ses.mp3");
        
        Console.WriteLine($"Looking for audio file at: {audioFilePath}");
        
        if (!File.Exists(audioFilePath))
        {
            Console.WriteLine("Audio file not found! Please ensure 'sarki.mp3' is in the same directory as the executable.");
            return;
        }

        try
        {
            Console.WriteLine("Initializing AssemblyAI client...");
            var client = new AssemblyAIClient(apiKey);
            
            Console.WriteLine("Ses dosyasi isleniyor...");
            
            Console.WriteLine("transcription basladi...");
            var transcript = await client.Transcripts.TranscribeAsync(
                new FileInfo(audioFilePath)
            );
            
            Console.WriteLine($"transcription tamamlandı: {transcript.Status}");
            
            if (transcript.Status == TranscriptStatus.Error)
            {
                Console.WriteLine("hata: " + transcript.Error);
                return;
            }
            
            Console.WriteLine("transkript: ");
            if (string.IsNullOrEmpty(transcript.Text))
            {
                Console.WriteLine("transcription tamamlandı ancak metne donusturulemedi.");
            }
            else
            {
                Console.WriteLine(transcript.Text);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}