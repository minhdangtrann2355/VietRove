using VietRove.Entities;
using VietRove.Models.Guide;

namespace VietRove.Services.Interfaces.Guide.Contract
{
    public interface IContractService
    {
        Task<VietRove.Entities.Contract> CreateContractAsync(int userId, ContractViewModel model);
    }
}