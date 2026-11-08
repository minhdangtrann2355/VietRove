
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using VietRove.Models.Admin;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;

namespace VietRove.Controllers.Admin
{
    [Authorize(Policy = AppRoles.Admin)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminCategoryController : Controller
    {
        private readonly IAdminCategoryService _categoryService;

        public AdminCategoryController(IAdminCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Admin/CategoryManagement.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] AdminCategorySearchViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            if (model.Search != null && model.Search.Length > 150)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Từ khóa tìm kiếm không được vượt quá 150 ký tự."
                });
            }

            AdminCategoryResultViewModel result =
                await _categoryService.GetCategoriesAsync(model);

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromForm] AdminCategoryCreateViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                string message = string.Join(" ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                return BadRequest(new
                {
                    success = false,
                    message = message
                });
            }

            var result = await _categoryService.CreateCategoryAsync(model);

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
        public async Task<IActionResult> Update(
            int categoryId,
            [FromForm] AdminCategoryUpdateViewModel model)
        {
            if (categoryId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã danh mục không hợp lệ."
                });
            }

            if (ModelState.IsValid == false)
            {
                string message = string.Join(" ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                return BadRequest(new
                {
                    success = false,
                    message = message
                });
            }

            var result = await _categoryService.UpdateCategoryAsync(
                categoryId,
                model);

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
        public async Task<IActionResult> ToggleStatus(int categoryId)
        {
            if (categoryId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã danh mục không hợp lệ."
                });
            }

            var result = await _categoryService.ToggleCategoryStatusAsync(
                categoryId);

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
