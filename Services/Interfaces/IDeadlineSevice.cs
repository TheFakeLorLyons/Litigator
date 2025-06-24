using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Deadline;

namespace Litigator.Services.Interfaces
{
    public interface IDeadlineService
    {
        Task<IEnumerable<DeadlineDTO>> GetUpcomingDeadlinesAsync(int days = 30);
        Task<IEnumerable<DeadlineDTO>> GetOverdueDeadlinesAsync();
        Task<IEnumerable<DeadlineDTO>> GetCriticalDeadlinesAsync();
        Task<bool> MarkDeadlineCompleteAsync(int id);

        Task<IEnumerable<DeadlineDTO>> GetDeadlinesByCaseAsync(int caseId);
        Task<IEnumerable<DeadlineDTO>> GetAllDeadlinesAsync();
        Task<DeadlineDTO?> GetDeadlineByIdAsync(int id);
        Task<DeadlineDTO> CreateDeadlineAsync(DeadlineCreateDTO createDto);
        Task<DeadlineDTO?> UpdateDeadlineAsync(int id, DeadlineUpdateDTO updateDto);
        Task<bool> DeleteDeadlineAsync(int id); 
    }
}