using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Interfaces
{
    public interface IDeadlineService
    {
        Task<IEnumerable<Deadline>> GetUpcomingDeadlinesAsync(int days = 30);
        Task<IEnumerable<Deadline>> GetOverdueDeadlinesAsync();
        Task<IEnumerable<Deadline>> GetDeadlinesByCaseAsync(int caseId);
        Task<Deadline> CreateDeadlineAsync(Deadline deadline);
        Task<Deadline> UpdateDeadlineAsync(Deadline deadline);
        Task<bool> DeleteDeadlineAsync(int id);
        Task<bool> MarkDeadlineCompleteAsync(int id);
        Task<IEnumerable<Deadline>> GetCriticalDeadlinesAsync();
    }
}