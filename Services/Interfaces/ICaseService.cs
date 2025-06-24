using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Case;

namespace Litigator.Services.Interfaces
{
    public interface ICaseService
    {
        Task<IEnumerable<CaseDTO>> GetAllCasesAsync();
        Task<Case?> GetCaseByIdAsync(int id);
        Task<Case?> GetCaseByNumberAsync(string caseNumber);
        Task<Case> CreateCaseAsync(Case case_);
        Task<Case> UpdateCaseAsync(Case case_);
        Task<bool> DeleteCaseAsync(int id);
        Task<IEnumerable<Case>> GetCasesByClientAsync(int clientId);
        Task<IEnumerable<Case>> GetCasesByAttorneyAsync(int attorneyId);
        Task<IEnumerable<Case>> GetActiveCasesAsync();
        Task<IEnumerable<Case>> SearchCasesAsync(string searchTerm);
    }
}