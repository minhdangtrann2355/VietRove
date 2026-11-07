using System.Collections.Generic;
using System.Threading.Tasks;
using VietRove.Entities;

namespace VietRove.Services.Interfaces.Category
{
    public interface ICategoryService
    {
        Task<List<VietRove.Entities.Category>> GetActiveCategoriesAsync(string categoryType);
    }
}