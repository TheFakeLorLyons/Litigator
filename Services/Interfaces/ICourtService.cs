using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Interfaces
{
    public interface ICourtService
    {
        Task<IEnumerable<Court>> GetAllCourtsAsync();
        Task<Court?> GetCourtByIdAsync(int id);
        Task<Court> CreateCourtAsync(Court court);
        Task<Court> UpdateCourtAsync(Court court);
        Task<bool> DeleteCourtAsync(int id);
        Task<IEnumerable<Court>> GetCourtsByStateAsync(string state);
    }
}