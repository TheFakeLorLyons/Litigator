using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Services.Interfaces
{
    public interface IJudgeService
    {
        // Read operations - return DTOs
        Task<IEnumerable<JudgeDTO>> GetAllJudgesAsync();
        Task<JudgeDetailDTO?> GetJudgeByIdAsync(int id);
        Task<JudgeDetailDTO?> GetJudgeByBarNumberAsync(string barNumber);
        Task<IEnumerable<JudgeDTO>> GetActiveJudgesAsync();

        // Write operations - accept and return DTOs
        Task<JudgeDetailDTO> CreateJudgeAsync(JudgeDetailDTO judgeDto);
        Task<JudgeDetailDTO> UpdateJudgeAsync(JudgeDetailDTO judgeDto);
        Task<bool> DeleteJudgeAsync(int id);

        // Entity access for business logic
        Task<Judge?> GetJudgeEntityByIdAsync(int id);
    }
}