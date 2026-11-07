using System.Collections.Generic;
using System.Threading.Tasks;
using VietRove.Entities;

namespace VietRove.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetActiveCategoriesAsync(string categoryType);
    }
}