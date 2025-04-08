using Tesseract;

Console.WriteLine("karakter okumasi yapilacak yol: ");
string imagePath = Console.ReadLine();
string tessData = @"/desktop/tessdata";
try
{
    using (var engine = new TesseractEngine(tessData, "eng", EngineMode.Default))
    {
        using (var img = Pix.LoadFromFile(imagePath))
        {
            using (var page = engine.Process(img))
            {
                string pageText = page.GetText();
                Console.WriteLine("resimden okunan text: ", pageText);
            }
        }
    }
}
catch (Exception e)
{
    Console.WriteLine($"hata: {e.Message}");
}
Console.ReadLine();