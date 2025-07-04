using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Interfaces
{
    public interface ICourtService
    {
        // Read operations - return DTOs
        Task<IEnumerable<CourtDTO>> GetAllCourtsAsync();
        Task<CourtDetailDTO?> GetCourtByIdAsync(int id);
        Task<IEnumerable<CourtDTO>> GetCourtsByStateAsync(string state);
        Task<IEnumerable<CourtDTO>> GetCourtsByCountyAsync(string state, string county);
        Task<IEnumerable<CourtDTO>> GetActiveCourtsAsync();
        Task<CourtDetailDTO?> GetCourtByNameAsync(string courtName, string county, string state);

        // Write operations - accept and return DTOs
        Task<CourtDetailDTO> CreateCourtAsync(CreateCourtDTO createCourtDto);
        Task<CourtDetailDTO> UpdateCourtAsync(UpdateCourtDTO updateCourtDto);
        Task<bool> DeleteCourtAsync(int id);

        // Entity access for business logic (when other services need the entity)
        Task<Court?> GetCourtEntityByIdAsync(int id);

        // Additional utility methods
        Task<bool> CourtExistsAsync(string courtName, string county, string state, int? excludeCourtId = null);
        Task<IEnumerable<string>> GetStatesAsync();
        Task<IEnumerable<string>> GetCountiesByStateAsync(string state);
    }
}