
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models;
using VietRove.Entities;
using VietRove.Security;

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
        // chi lay cac bai viet da xuat ban
        List<Article> articles = await _context.Articles
            .Where(a => a.Status == 1)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return View("Home", articles);
    }

    public async Task<IActionResult> Guides(string keyword, string cardType, int provinceId, int languageId)
    {
        IQueryable<GuideProfile> query = _context.GuideProfiles
            .Include(g => g.CardType)
            .Where(g => g.Status == (int)ProfileStatus.Approved);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string cleanKeyword = keyword.Trim().ToLower();

            query = query.Where(g =>
                g.FullName.ToLower().Contains(cleanKeyword) ||
                (g.Address != null && g.Address.ToLower().Contains(cleanKeyword)) ||
                (g.CardNumber != null && g.CardNumber.ToLower().Contains(cleanKeyword)));
        }

        if (!string.IsNullOrWhiteSpace(cardType))
        {
            query = query.Where(g =>
                g.CardType != null &&
                g.CardType.CategoryName == cardType);
        }

        if (provinceId > 0)
        {
            query = query.Where(g => _context.GuideProvinces
                .Any(gp => gp.GuideId == g.GuideId && gp.ProvinceId == provinceId));
        }

        if (languageId > 0)
        {
            // chi loc cac ngoai ngu da duoc phe duyet
            query = query.Where(g => _context.GuideLanguages
                .Any(gl => gl.GuideId == g.GuideId && gl.LanguageId == languageId && gl.VerificationStatus == 1));
        }

        List<GuideProfile> guides = await query
            .OrderByDescending(g => g.GuideId)
            .ToListAsync();

        List<int> guideIds = guides.Select(g => g.GuideId).ToList();

        Dictionary<int, string> activityProvinces = await _context.GuideProvinces
            .Where(gp => guideIds.Contains(gp.GuideId))
            .Join(
                _context.Categories.Where(c => c.CategoryType == "Province"),
                gp => gp.ProvinceId,
                c => c.CategoryId,
                (gp, c) => new { gp.GuideId, c.CategoryName })
            .GroupBy(item => item.GuideId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.First().CategoryName);

        List<Category> provinces = await _context.Categories
            .Where(c => c.CategoryType == "Province" && c.IsActive)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        List<Category> languages = await _context.Categories
            .Where(c => c.CategoryType == "Language" && c.IsActive)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        ViewBag.Keyword = keyword;
        ViewBag.CardType = cardType;
        ViewBag.ProvinceId = provinceId;
        ViewBag.LanguageId = languageId;
        ViewBag.Provinces = provinces;
        ViewBag.Languages = languages;
        ViewBag.ActivityProvinces = activityProvinces;

        return View(guides);
    }

    [HttpGet]
    public async Task<IActionResult> GuideProfile(int id)
    {
        GuideProfile? profile = await _context.GuideProfiles
            .FirstOrDefaultAsync(g =>
                g.GuideId == id &&
                g.Status == (int)ProfileStatus.Approved);

        if (profile == null)
        {
            return NotFound();
        }

        List<int> provinceIds = await _context.GuideProvinces
            .Where(gp => gp.GuideId == profile.GuideId)
            .Select(gp => gp.ProvinceId)
            .ToListAsync();

        List<int> languageIds = await _context.GuideLanguages
            .Where(gl => gl.GuideId == profile.GuideId && gl.VerificationStatus == 1)
            .Select(gl => gl.LanguageId)
            .ToListAsync();

        List<int> categoryIds = provinceIds
            .Concat(languageIds)
            .Distinct()
            .ToList();

        List<Category> selectedCategories = await _context.Categories
            .Where(c => categoryIds.Contains(c.CategoryId))
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.CategoryName)
            .ToListAsync();

        string activityProvinceName = string.Empty;

        Category? activityProvince = selectedCategories
            .FirstOrDefault(c => c.CategoryType == "Province");

        if (activityProvince != null)
        {
            activityProvinceName = activityProvince.CategoryName;
        }

        GuideProfileViewModel model = new GuideProfileViewModel
        {
            Profile = profile,
            SelectedCategories = selectedCategories,

            // chi lay ngoai ngu da duoc phe duyet
            GuideLanguages = await _context.GuideLanguages
                .Include(gl => gl.Language)
                .Where(gl => gl.GuideId == profile.GuideId && gl.VerificationStatus == 1)
                .OrderBy(gl => gl.Language.CategoryName)
                .ToListAsync(),

            GuideExperiences = await _context.GuideExperiences
                .Where(ge =>
                    ge.GuideId == profile.GuideId &&
                    ge.VerificationStatus == 1)
                .OrderByDescending(ge => ge.CreatedAt)
                .ToListAsync(),

            ActivityProvinceName = activityProvinceName,
            StatusText = "Đã xác thực",
            IsApproved = true
        };

        return View(model);
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
