
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models;
using VietRove.Services.Interfaces;

namespace VietRove.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly IRegisterService _registerService;
        private readonly ICategoryService _categoryService;

        public RegisterController(
            IRegisterService registerService,
            ICategoryService categoryService)
        {
            _registerService = registerService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadCategoriesAsync();

            RegisterViewModel model = new RegisterViewModel
            {
                DateOfBirth = DateTime.Today,
                IssueDate = DateTime.Today,
                ExpiryDate = DateTime.Today.AddYears(2)
            };

            return View("Register", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (model == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Thông tin đăng ký không hợp lệ."
                });
            }

            // kiem tra honeypot
            if (string.IsNullOrWhiteSpace(model.Website) == false)
            {
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Index", "Login")
                });
            }

            // chuan hoa du lieu dau vao
            if (model.Email != null)
            {
                model.Email = model.Email.Trim().ToLowerInvariant();
            }

            if (model.FullName != null)
            {
                model.FullName = model.FullName.Trim();
            }

            if (model.Phone != null)
            {
                model.Phone = model.Phone.Trim();
            }

            if (model.IdCard != null)
            {
                model.IdCard = model.IdCard.Trim();
            }

            if (model.Address != null)
            {
                model.Address = model.Address.Trim();
            }

            if (model.CardNumber != null)
            {
                model.CardNumber = model.CardNumber.Trim();
            }

            bool isInternationalCard = string.Equals(
                model.CardType,
                "Hướng dẫn viên quốc tế",
                StringComparison.OrdinalIgnoreCase);

            // khong yeu cau ngoai ngu voi huong dan vien noi dia
            if (isInternationalCard == false)
            {
                ModelState.Remove(nameof(model.LanguageIds));
                ModelState.Remove(nameof(model.LanguageNames));
                ModelState.Remove(nameof(model.LanguageCertificates));

                model.LanguageIds = new List<int>();
                model.LanguageNames = new List<string>();
                model.LanguageCertificates = new List<IFormFile>();
            }

            if (ModelState.IsValid == false)
            {
                return await ReturnValidationErrorsAsync();
            }

            var result = await _registerService.RegisterAsync(model);

            if (result.Success)
            {
                return Json(new
                {
                    success = true,
                    message = result.Message,
                    redirectUrl = Url.Action("Index", "Login")
                });
            }

            List<object> errors = new List<object>();

            if (string.IsNullOrEmpty(result.Field) == false)
            {
                errors.Add(new
                {
                    field = result.Field,
                    message = result.Message
                });
            }

            return Json(new
            {
                success = false,
                message = result.Message,
                errors = errors
            });
        }

        private async Task LoadCategoriesAsync()
        {
            ViewBag.Provinces =
                await _categoryService.GetActiveCategoriesAsync("Province");

            ViewBag.Languages =
                await _categoryService.GetActiveCategoriesAsync("Language");

            ViewBag.CardTypes =
                await _categoryService.GetActiveCategoriesAsync("CardType");
        }

        private async Task<IActionResult> ReturnValidationErrorsAsync()
        {
            List<object> errors = new List<object>();

            foreach (var state in ModelState)
            {
                if (state.Value != null &&
                    state.Value.Errors.Count > 0)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errors.Add(new
                        {
                            field = state.Key,
                            message = error.ErrorMessage
                        });
                    }
                }
            }

            await LoadCategoriesAsync();

            return Json(new
            {
                success = false,
                message = "Vui lòng kiểm tra lại các trường không hợp lệ.",
                errors = errors
            });
        }
    }
}
