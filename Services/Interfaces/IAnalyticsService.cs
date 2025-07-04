using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Analytics;

namespace Litigator.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<IEnumerable<AttorneyPerformanceDTO>> GetAttorneyPerformanceAsync();
        Task<IEnumerable<CaseOutcomePredictionDTO>> GetCaseOutcomePredictionsAsync();
        Task<IEnumerable<CriticalCaseDTO>> GetCriticalCasesAsync();
        Task<IEnumerable<MonthlyTrendDTO>> GetMonthlyTrendsAsync();
        Task<IEnumerable<DeadlinePerformanceDTO>> GetDeadlinePerformanceAsync();
    }
}