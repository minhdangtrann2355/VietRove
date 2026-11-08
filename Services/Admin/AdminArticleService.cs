
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Admin;
using VietRove.Services.Interfaces.Admin;
using VietRove.Services.Interfaces.FileStorage;

namespace VietRove.Services.Admin
{
    public class AdminArticleService : IAdminArticleService
    {
        private readonly VietRoveDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        private const int PageSize = 10;

        public AdminArticleService(
            VietRoveDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<AdminArticleResultViewModel> GetArticlesAsync(
            AdminArticleSearchViewModel model)
        {
            IQueryable<Article> query = _context.Articles
                .AsNoTracking();

            if (string.IsNullOrWhiteSpace(model.Search) == false)
            {
                string keyword = model.Search.Trim();

                query = query.Where(a => a.Title.Contains(keyword));
            }

            if (model.ArticleType.HasValue)
            {
                if (AdminArticleDefinitions.IsValidArticleType(
                    model.ArticleType.Value))
                {
                    query = query.Where(a =>
                        a.ArticleType == model.ArticleType.Value);
                }
                else
                {
                    query = query.Where(a => false);
                }
            }

            if (model.Status.HasValue)
            {
                if (AdminArticleDefinitions.IsValidStatus(
                    model.Status.Value))
                {
                    query = query.Where(a =>
                        a.Status == model.Status.Value);
                }
                else
                {
                    query = query.Where(a => false);
                }
            }

            int totalRecords = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                (double)totalRecords / PageSize);

            int currentPage = model.Page;

            if (currentPage < 1)
            {
                currentPage = 1;
            }

            if (totalPages > 0 && currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            var articles = await query
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.ArticleId)
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(a => new AdminArticleListItemViewModel
                {
                    ArticleId = a.ArticleId,
                    Title = a.Title,
                    Thumbnail = a.Thumbnail,
                    ArticleType = a.ArticleType,
                    Status = a.Status,
                    AuthorName = a.Author.Email,
                    CreatedAt = a.CreatedAt,
                    PublishedAt = a.PublishedAt
                })
                .ToListAsync();

            return new AdminArticleResultViewModel
            {
                Items = articles,
                CurrentPage = currentPage,
                PageSize = PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<AdminArticleDetailViewModel?> GetArticleDetailAsync(
            int articleId)
        {
            if (articleId <= 0)
            {
                return null;
            }

            return await _context.Articles
                .AsNoTracking()
                .Where(a => a.ArticleId == articleId)
                .Select(a => new AdminArticleDetailViewModel
                {
                    ArticleId = a.ArticleId,
                    Title = a.Title,
                    Content = a.Content,
                    Thumbnail = a.Thumbnail,
                    ArticleType = a.ArticleType,
                    Status = a.Status,
                    AuthorName = a.Author.Email,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    PublishedAt = a.PublishedAt
                })
                .FirstOrDefaultAsync();
        }
        public async Task<string?> GetThumbnailFileNameAsync(int articleId)
        {
            if (articleId <= 0)
            {
                return null;
            }

            return await _context.Articles
                .AsNoTracking()
                .Where(a => a.ArticleId == articleId)
                .Select(a => a.Thumbnail)
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message)> CreateArticleAsync(
            AdminArticleCreateViewModel model,
            int adminId)
        {
            if (model == null || adminId <= 0)
            {
                return (false, "Thông tin tạo bài viết không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return (false, "Vui lòng nhập tiêu đề bài viết.");
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                return (false, "Vui lòng nhập nội dung bài viết.");
            }

            string title = model.Title.Trim();
            string content = model.Content.Trim();

            if (title.Length > 250)
            {
                return (false, "Tiêu đề không được vượt quá 250 ký tự.");
            }

            if (AdminArticleDefinitions.IsValidArticleType(
                model.ArticleType) == false)
            {
                return (false, "Loại bài viết không hợp lệ.");
            }

            bool adminExists = await _context.Users
                .AnyAsync(u =>
                    u.UserId == adminId &&
                    u.Status == 1);

            if (adminExists == false)
            {
                return (false, "Không tìm thấy tài khoản Admin hợp lệ.");
            }

            string? thumbnailFileName = null;

            if (model.Thumbnail != null)
            {
                try
                {
                    string validationMessage =
                        await _fileStorageService.ValidateAvatarAsync(
                            model.Thumbnail);

                    if (string.IsNullOrWhiteSpace(validationMessage) == false)
                    {
                        return (false, validationMessage);
                    }

                    thumbnailFileName =
                        await _fileStorageService.SaveFileAsync(
                            model.Thumbnail);
                }
                catch
                {
                    return (false, "Không thể tải ảnh đại diện lên hệ thống.");
                }
            }

            Article article = new Article
            {
                Title = title,
                Content = content,
                Thumbnail = thumbnailFileName,
                ArticleType = model.ArticleType,
                Status = AdminArticleDefinitions.Draft,
                AuthorId = adminId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                PublishedAt = null
            };

            try
            {
                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                return (true, "Tạo bài viết thành công.");
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(thumbnailFileName) == false)
                {
                    try
                    {
                        _fileStorageService.DeleteFile(thumbnailFileName);
                    }
                    catch
                    {
                    }
                }

                return (false, "Không thể tạo bài viết. Vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateArticleAsync(
            AdminArticleUpdateViewModel model)
        {
            if (model == null || model.ArticleId <= 0)
            {
                return (false, "Thông tin bài viết không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return (false, "Vui lòng nhập tiêu đề bài viết.");
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                return (false, "Vui lòng nhập nội dung bài viết.");
            }

            string title = model.Title.Trim();
            string content = model.Content.Trim();

            if (title.Length > 250)
            {
                return (false, "Tiêu đề không được vượt quá 250 ký tự.");
            }

            if (AdminArticleDefinitions.IsValidArticleType(
                model.ArticleType) == false)
            {
                return (false, "Loại bài viết không hợp lệ.");
            }

            Article? article = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == model.ArticleId);

            if (article == null)
            {
                return (false, "Không tìm thấy bài viết.");
            }

            string? newThumbnailFileName = null;

            if (model.Thumbnail != null)
            {
                try
                {
                    string validationMessage =
                        await _fileStorageService.ValidateAvatarAsync(
                            model.Thumbnail);

                    if (string.IsNullOrWhiteSpace(validationMessage) == false)
                    {
                        return (false, validationMessage);
                    }

                    newThumbnailFileName =
                        await _fileStorageService.SaveFileAsync(
                            model.Thumbnail);
                }
                catch
                {
                    return (false, "Không thể tải ảnh đại diện lên hệ thống.");
                }
            }

            string? oldThumbnailFileName = article.Thumbnail;

            article.Title = title;
            article.Content = content;
            article.ArticleType = model.ArticleType;
            article.UpdatedAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(newThumbnailFileName) == false)
            {
                article.Thumbnail = newThumbnailFileName;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(newThumbnailFileName) == false)
                {
                    try
                    {
                        _fileStorageService.DeleteFile(newThumbnailFileName);
                    }
                    catch
                    {
                    }
                }

                return (false, "Không thể cập nhật bài viết. Vui lòng thử lại.");
            }

            if (string.IsNullOrWhiteSpace(newThumbnailFileName) == false &&
                string.IsNullOrWhiteSpace(oldThumbnailFileName) == false)
            {
                try
                {
                    _fileStorageService.DeleteFile(oldThumbnailFileName);
                }
                catch
                {
                    // khong lam that bai cap nhat khi xoa anh cu loi
                }
            }

            return (true, "Cập nhật bài viết thành công.");
        }

        public async Task<(bool Success, string Message)> ChangeArticleStatusAsync(
            int articleId,
            int status)
        {
            if (articleId <= 0)
            {
                return (false, "Mã bài viết không hợp lệ.");
            }

            if (AdminArticleDefinitions.IsValidStatus(status) == false)
            {
                return (false, "Trạng thái bài viết không hợp lệ.");
            }

            Article? article = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);

            if (article == null)
            {
                return (false, "Không tìm thấy bài viết.");
            }

            if (article.Status == status)
            {
                return (true, "Bài viết hiện đã ở trạng thái này.");
            }

            DateTime now = DateTime.Now;

            article.Status = status;
            article.UpdatedAt = now;

            if (status == AdminArticleDefinitions.Published)
            {
                article.PublishedAt = now;
            }
            else if (status == AdminArticleDefinitions.Draft)
            {
                article.PublishedAt = null;
            }

            try
            {
                await _context.SaveChangesAsync();

                if (status == AdminArticleDefinitions.Published)
                {
                    return (true, "Xuất bản bài viết thành công.");
                }

                if (status == AdminArticleDefinitions.Hidden)
                {
                    return (true, "Đã ẩn bài viết.");
                }

                return (true, "Đã chuyển bài viết về bản nháp.");
            }
            catch
            {
                return (false, "Không thể cập nhật trạng thái bài viết.");
            }
        }
    }
}
