using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Services.Interfaces.Category;

namespace VietRove.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly VietRoveDbContext _context;

        public CategoryService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<List<VietRove.Entities.Category>> GetActiveCategoriesAsync(string categoryType)
        {
            if (string.IsNullOrWhiteSpace(categoryType))
            {
                return new List<VietRove.Entities.Category>();
            }

            return await _context.Categories
                .Where(c => c.CategoryType == categoryType && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}