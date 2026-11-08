
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models.Admin;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;
using VietRove.Services.Interfaces.FileStorage;

namespace VietRove.Controllers.Admin
{
    [Authorize(Policy = AppRoles.Admin)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminArticleController : Controller
    {
        private readonly IAdminArticleService _articleService;
        private readonly IFileStorageService _fileStorageService;

        public AdminArticleController(
            IAdminArticleService articleService,
            IFileStorageService fileStorageService)
        {
            _articleService = articleService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Admin/ArticleManagement.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] AdminArticleSearchViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            if (model.Page > 1000000)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Số trang không hợp lệ."
                });
            }

            if (model.Search != null && model.Search.Length > 250)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Từ khóa tìm kiếm không được vượt quá 250 ký tự."
                });
            }

            if (model.ArticleType.HasValue &&
                AdminArticleDefinitions.IsValidArticleType(
                    model.ArticleType.Value) == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Loại bài viết không hợp lệ."
                });
            }

            if (model.Status.HasValue &&
                AdminArticleDefinitions.IsValidStatus(
                    model.Status.Value) == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Trạng thái bài viết không hợp lệ."
                });
            }

            AdminArticleResultViewModel result =
                await _articleService.GetArticlesAsync(model);

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int articleId)
        {
            if (articleId <= 0)
            {
                return BadRequest("Mã bài viết không hợp lệ.");
            }

            AdminArticleDetailViewModel? article =
                await _articleService.GetArticleDetailAsync(articleId);

            if (article == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/ArticleDetail.cshtml", article);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/ArticleCreate.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Thumbnail(int articleId)
        {
            if (articleId <= 0)
            {
                return BadRequest();
            }

            string? fileName =
                await _articleService.GetThumbnailFileNameAsync(articleId);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

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

            Stream? stream =
                await _fileStorageService.OpenReadAsync(fileName);

            if (stream == null)
            {
                return NotFound();
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "no-store";

            return File(stream, contentType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)]
        public async Task<IActionResult> Create(
            [FromForm] AdminArticleCreateViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                string message = string.Join(" ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Dữ liệu bài viết không hợp lệ.";
                }

                return BadRequest(new
                {
                    success = false,
                    message = message
                });
            }

            string adminIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            int adminId = 0;

            if (int.TryParse(adminIdValue, out adminId) == false ||
                adminId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Không xác định được tài khoản Admin."
                });
            }

            var result = await _articleService.CreateArticleAsync(
                model,
                adminId);

            if (result.Success == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Json(new
            {
                success = true,
                message = result.Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)]
        public async Task<IActionResult> Update(
            [FromForm] AdminArticleUpdateViewModel model)
        {
            if (model.ArticleId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã bài viết không hợp lệ."
                });
            }

            if (ModelState.IsValid == false)
            {
                string message = string.Join(" ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Dữ liệu bài viết không hợp lệ.";
                }

                return BadRequest(new
                {
                    success = false,
                    message = message
                });
            }

            var result = await _articleService.UpdateArticleAsync(model);

            if (result.Success == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Json(new
            {
                success = true,
                message = result.Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(
            [FromForm] int articleId,
            [FromForm] int status)
        {
            if (articleId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã bài viết không hợp lệ."
                });
            }

            if (AdminArticleDefinitions.IsValidStatus(status) == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Trạng thái bài viết không hợp lệ."
                });
            }

            var result = await _articleService.ChangeArticleStatusAsync(
                articleId,
                status);

            if (result.Success == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Json(new
            {
                success = true,
                message = result.Message
            });
        }
    }
}
