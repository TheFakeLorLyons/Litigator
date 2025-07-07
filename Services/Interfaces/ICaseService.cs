using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Services.Interfaces
{
    public interface ICaseService
    {
        // Read operations - return DTOs
        Task<IEnumerable<CaseDetailDTO>> GetAllCasesAsync();
        Task<CaseDetailDTO?> GetCaseByIdAsync(int id);
        Task<CaseDetailDTO?> GetCaseByNumberAsync(string caseNumber);
        Task<IEnumerable<CaseDetailDTO>> GetCasesByClientAsync(int clientId);
        Task<IEnumerable<CaseDetailDTO>> GetCasesByAttorneyAsync(int attorneyId);
        Task<IEnumerable<CaseDetailDTO>> GetActiveCasesAsync();
        Task<IEnumerable<CaseDetailDTO>> SearchCasesAsync(string searchTerm);

        // Write operations - accept and return entities (for business logic)
        Task<CaseDetailDTO> CreateCaseAsync(CaseCreateDTO caseCreateDto);
        Task<CaseDetailDTO> UpdateCaseAsync(int caseId, CaseUpdateDTO caseUpdateDto);
        Task<bool> DeleteCaseAsync(int id);

        // Entity access for business logic
        Task<Case?> GetCaseEntityByIdAsync(int id);
    }
}