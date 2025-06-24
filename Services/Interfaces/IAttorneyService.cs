using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Interfaces
{
    public interface IAttorneyService
    {
        Task<IEnumerable<Attorney>> GetAllAttorneysAsync();
        Task<Attorney?> GetAttorneyByIdAsync(int id);
        Task<Attorney?> GetAttorneyByBarNumberAsync(string barNumber);
        Task<Attorney> CreateAttorneyAsync(Attorney attorney);
        Task<Attorney> UpdateAttorneyAsync(Attorney attorney);
        Task<bool> DeleteAttorneyAsync(int id);
        Task<IEnumerable<Attorney>> GetActiveAttorneysAsync();
    }
}