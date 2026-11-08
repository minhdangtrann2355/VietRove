using CategoryEntity = VietRove.Entities.Category;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Admin;
using VietRove.Services.Interfaces.Admin;

namespace VietRove.Services.Admin
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly VietRoveDbContext _context;
        private const int PageSize = 10;

        public AdminCategoryService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<AdminCategoryResultViewModel> GetCategoriesAsync(
            AdminCategorySearchViewModel model)
        {
            int page = model.Page;

            if (page < 1)
            {
                page = 1;
            }

            IQueryable<CategoryEntity> query = _context.Categories.AsNoTracking();

            if (string.IsNullOrWhiteSpace(model.Search) == false)
            {
                string search = model.Search.Trim();

                query = query.Where(c => c.CategoryName.Contains(search));
            }

            if (string.IsNullOrWhiteSpace(model.CategoryType) == false)
            {
                if (IsValidCategoryType(model.CategoryType) == false)
                {
                    return new AdminCategoryResultViewModel
                    {
                        CurrentPage = 1,
                        PageSize = PageSize
                    };
                }

                query = query.Where(c => c.CategoryType == model.CategoryType);
            }

            int totalRecords = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)PageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var items = await query
                .OrderBy(c => c.CategoryType)
                .ThenBy(c => c.CategoryName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new AdminCategoryListItemViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryType = c.CategoryType,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return new AdminCategoryResultViewModel
            {
                Items = items,
                CurrentPage = page,
                PageSize = PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<(bool Success, string Message)> CreateCategoryAsync(
            AdminCategoryCreateViewModel model)
        {
            string categoryName = model.CategoryName.Trim();
            string categoryType = model.CategoryType.Trim();

            if (IsValidCategoryType(categoryType) == false)
            {
                return (false, "Loại danh mục không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return (false, "Tên danh mục không được để trống.");
            }

            bool exists = await _context.Categories.AnyAsync(c =>
                c.CategoryType == categoryType &&
                c.CategoryName.ToLower() == categoryName.ToLower());

            if (exists)
            {
                return (false, "Tên danh mục đã tồn tại trong loại danh mục này.");
            }

            CategoryEntity category = new CategoryEntity
            {
                CategoryName = categoryName,
                CategoryType = categoryType,
                IsActive = true
            };

            _context.Categories.Add(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return (false, "Không thể thêm danh mục. Có thể tên danh mục đã tồn tại.");
            }

            return (true, "Thêm danh mục thành công.");
        }

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(
            int categoryId,
            AdminCategoryUpdateViewModel model)
        {
            string categoryName = model.CategoryName.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return (false, "Tên danh mục không được để trống.");
            }

            CategoryEntity? category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return (false, "Không tìm thấy danh mục.");
            }

            bool exists = await _context.Categories.AnyAsync(c =>
                c.CategoryId != categoryId &&
                c.CategoryType == category.CategoryType &&
                c.CategoryName.ToLower() == categoryName.ToLower());

            if (exists)
            {
                return (false, "Tên danh mục đã tồn tại trong loại danh mục này.");
            }

            category.CategoryName = categoryName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return (false, "Không thể cập nhật danh mục. Có thể tên danh mục đã tồn tại.");
            }

            return (true, "Cập nhật danh mục thành công.");
        }

        public async Task<(bool Success, string Message)> ToggleCategoryStatusAsync(
            int categoryId)
        {
            CategoryEntity? category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return (false, "Không tìm thấy danh mục.");
            }

            category.IsActive = !category.IsActive;

            await _context.SaveChangesAsync();

            if (category.IsActive)
            {
                return (true, "Kích hoạt danh mục thành công.");
            }

            return (true, "Ngừng hoạt động danh mục thành công.");
        }

        private bool IsValidCategoryType(string categoryType)
        {
            return categoryType == "Province" ||
                   categoryType == "Language" ||
                   categoryType == "CardType";
        }
    }
}
