using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RecipeSuggestion.Models;

namespace RecipeSuggestion.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly OpenAiService _openAiService;

    public HomeController(ILogger<HomeController> logger, OpenAiService openAiService)
    {
        _logger = logger;
        _openAiService = openAiService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("CreateRecipe");
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult CreateRecipe()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecipe(string ingredients)
    {
        var result = await _openAiService.GetRecipeAsync(ingredients);
        ViewBag.recipe = result;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}