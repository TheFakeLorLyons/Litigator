using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Services.Interfaces
{
    public interface IAttorneyService
    {
        // Read operations - return DTOs
        Task<IEnumerable<AttorneyDTO>> GetAllAttorneysAsync();
        Task<AttorneyDetailDTO?> GetAttorneyByIdAsync(int id);
        Task<AttorneyDetailDTO?> GetAttorneyByBarNumberAsync(string barNumber);
        Task<IEnumerable<AttorneyDTO>> GetActiveAttorneysAsync();

        // Write operations - accept and return DTOs
        Task<AttorneyDetailDTO> CreateAttorneyAsync(AttorneyDetailDTO attorneyDto);
        Task<AttorneyDetailDTO> UpdateAttorneyAsync(AttorneyDetailDTO attorneyDto);
        Task<bool> DeleteAttorneyAsync(int id);

        // Entity access for business logic
        Task<Attorney?> GetAttorneyEntityByIdAsync(int id);
    }
}