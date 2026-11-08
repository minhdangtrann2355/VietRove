
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Services.Interfaces.FileStorage;

namespace VietRove.Controllers.Guest;

public class GuestArticleController : Controller
{
    private readonly VietRoveDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public GuestArticleController(
        VietRoveDbContext context,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> ArticleDetail(int id)
    {
        Article? article = await _context.Articles
            .FirstOrDefaultAsync(a =>
                a.ArticleId == id &&
                a.Status == 1);

        if (article == null)
        {
            return NotFound();
        }

        return View("~/Views/Home/ArticleDetail.cshtml", article);
    }

    [HttpGet]
    public async Task<IActionResult> ArticleThumbnail(int articleId)
    {
        string? thumbnail = await _context.Articles
            .Where(a => a.ArticleId == articleId && a.Status == 1)
            .Select(a => a.Thumbnail)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(thumbnail))
        {
            return NotFound();
        }

        string extension = Path.GetExtension(thumbnail).ToLowerInvariant();
        string contentType = string.Empty;

        if (extension == ".jpg" || extension == ".jpeg")
        {
            contentType = "image/jpeg";
        }
        else if (extension == ".png")
        {
            contentType = "image/png";
        }
        else
        {
            return NotFound();
        }

        Stream? stream = await _fileStorageService.OpenReadAsync(thumbnail);

        if (stream == null)
        {
            return NotFound();
        }

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "no-store";

        return File(stream, contentType);
    }
}
