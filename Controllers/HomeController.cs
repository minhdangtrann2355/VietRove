using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models;
using VietRove.Entities;

namespace VietRove.Controllers;

public class HomeController : Controller
{
    private readonly VietRoveDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public HomeController(VietRoveDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        List<Article> articles = new List<Article>();
        if (_context.Articles != null)
        {
            articles = await _context.Articles.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }
        return View("Home", articles);
    }

    public async Task<IActionResult> Guides(string? keyword, string? cardType)
    {
        IQueryable<GuideProfile> query = _context.GuideProfiles.Where(g => g.Status == 1);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string cleanKeyword = keyword.Trim().ToLower();
            query = query.Where(g => g.FullName.ToLower().Contains(cleanKeyword) || g.Address.ToLower().Contains(cleanKeyword));
        }

        if (!string.IsNullOrWhiteSpace(cardType))
        {
            query = query.Where(g => g.CardType == cardType);
        }

        List<GuideProfile> guides = await query.OrderByDescending(g => g.GuideId).ToListAsync();

        ViewBag.Keyword = keyword;
        ViewBag.CardType = cardType;

        return View(guides);
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

    [HttpGet]
    public IActionResult GetAvatar(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        string safeFileName = Path.GetFileName(fileName);
        string filePath = Path.Combine(_environment.ContentRootPath, "ProtectedUploads", safeFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        string ext = Path.GetExtension(safeFileName).ToLowerInvariant();
        string contentType = ext switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };

        return PhysicalFile(filePath, contentType);
    }

    [HttpGet]
    public IActionResult GetThumbnail(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        string safeFileName = Path.GetFileName(fileName);
        string filePath = Path.Combine(_environment.ContentRootPath, "ProtectedUploads", safeFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        string ext = Path.GetExtension(safeFileName).ToLowerInvariant();
        string contentType = ext switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };

        return PhysicalFile(filePath, contentType);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        string requestId = string.Empty;
        if (Activity.Current != null)
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
