using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Services.Interfaces
{
    public interface ICaseService
    {
        Task<IEnumerable<CaseDTO>> GetAllCasesAsync();
        Task<CaseDetailDTO?> GetCaseByIdAsync(int id);
        Task<CaseDetailDTO?> GetCaseByNumberAsync(string caseNumber);
        Task<IEnumerable<CaseDTO>> GetCasesByClientAsync(int clientId);
        Task<IEnumerable<CaseDTO>> GetCasesByAttorneyAsync(int attorneyId);
        Task<IEnumerable<CaseDTO>> GetActiveCasesAsync();
        Task<IEnumerable<CaseDTO>> SearchCasesAsync(string searchTerm);

        // Write operations - accept and return entities (for business logic)
        Task<CaseDetailDTO> CreateCaseAsync(Case case_);
        Task<CaseDetailDTO> UpdateCaseAsync(Case case_);
        Task<bool> DeleteCaseAsync(int id);

        Task<Case?> GetCaseEntityByIdAsync(int id);
    }
}