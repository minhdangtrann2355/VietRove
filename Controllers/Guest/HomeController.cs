using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Shared;

namespace VietRove.Controllers.Guest;

public class HomeController : Controller
{
    private readonly VietRoveDbContext _context;

    public HomeController(VietRoveDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        List<Article> articles = await _context.Articles
            .Where(a => a.Status == 1)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync();

        return View("Home", articles);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Vision()
    {
        return View();
    }

    public IActionResult Regulations()
    {
        return View();
    }

    public IActionResult Rules()
    {
        return View();
    }

    public IActionResult GuideRegister()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        string requestId = string.Empty;

        if (Activity.Current != null && !string.IsNullOrEmpty(Activity.Current.Id))
        {
            requestId = Activity.Current.Id;
        }
        else
        {
            requestId = HttpContext.TraceIdentifier;
        }

        return View(new ErrorViewModel { RequestId = requestId });
    }
}
