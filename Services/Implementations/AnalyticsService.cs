using Litigator.DataAccess.Data;
using Litigator.Models.DTOs.Analytics;
using Litigator.Services.Interfaces;

namespace Litigator.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly LitigatorDbContext _context; // Changed from ApplicationDbContext

        public AnalyticsService(LitigatorDbContext context) // Changed from ApplicationDbContext
        {
            _context = context;
        }

        public async Task<IEnumerable<AttorneyPerformanceDTO>> GetAttorneyPerformanceAsync() // Changed return type
        {
            var attorneys = await _context.Attorneys
                .Where(a => a.IsActive)
                .Include(a => a.Cases)
                    .ThenInclude(c => c.Deadlines)
                .ToListAsync();

            var result = attorneys
                .Select(a => {
                    var attorneyCases = a.Cases.ToList();
                    var closedCases = attorneyCases.Where(c => c.Status == "Closed").ToList();
                    var activeCases = attorneyCases.Where(c => c.Status == "Active" || c.Status == "Open").ToList();

                    return new
                    {
                        Attorney = a,
                        TotalRevenue = attorneyCases.Sum(c => c.EstimatedValue ?? 0),
                        AvgCaseValue = attorneyCases.Any() ? attorneyCases.Average(c => c.EstimatedValue ?? 0) : 0,
                        CompletedCases = closedCases.Count, // Fixed: using closedCases here
                        ActiveCases = activeCases.Count,    // Fixed: using activeCases here
                        TotalCases = attorneyCases.Count,
                        OverdueDeadlines = attorneyCases
                            .SelectMany(c => c.Deadlines)
                            .Count(d => d.DeadlineDate < DateTime.Now && !d.IsCompleted)
                    };
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Select((x, index) => new AttorneyPerformanceDTO
                {
                    Rank = index + 1,
                    AttorneyName = $"{x.Attorney.Name.First} {x.Attorney.Name.Last}",
                    BarNumber = x.Attorney.BarNumber,
                    Email = x.Attorney.Email,
                    TotalRevenue = x.TotalRevenue,
                    AvgCaseValue = x.AvgCaseValue,
                    CompletedCases = x.CompletedCases,
                    ActiveCases = x.ActiveCases,
                    TotalCases = x.TotalCases,
                    OverdueDeadlines = x.OverdueDeadlines,
                    CompletionRate = x.TotalCases > 0 ? (decimal)x.CompletedCases / x.TotalCases * 100 : 0,
                    PerformanceScore = x.CompletedCases * 100 - x.OverdueDeadlines * 50
                })
                .ToList();

            return result;
        }

        public async Task<IEnumerable<CaseOutcomePredictionDTO>> GetCaseOutcomePredictionsAsync() // Changed return type
        {
            var activeCases = await _context.Cases
                .Where(c => c.Status == "Open" || c.Status == "Active")
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Deadlines)
                .ToListAsync();

            var result = activeCases
                .Select(c => {
                    var attorneyCases = _context.Cases.Where(ac => ac.AssignedAttorneyId == c.AssignedAttorneyId).ToList();
                    var attorneyClosedCases = attorneyCases.Where(ac => ac.Status == "Closed").ToList();

                    var totalDeadlines = c.Deadlines.Count;
                    var completedDeadlines = c.Deadlines.Count(d => d.IsCompleted);
                    var overdueDeadlines = c.Deadlines.Count(d => d.DeadlineDate < DateTime.Now && !d.IsCompleted);
                    var daysOpen = (DateTime.Now - c.FilingDate).Days;
                    var attorneySuccessRate = attorneyCases.Any() ?
                        (double)attorneyClosedCases.Count / attorneyCases.Count * 100 : 0;

                    var riskScore = (overdueDeadlines * 30) +
                                   (daysOpen > 365 ? 50 : 0) +
                                   (100 - attorneySuccessRate) +
                                   (c.EstimatedValue > 1000000 ? 25 : 0);

                    return new CaseOutcomePredictionDTO
                    {
                        CaseNumber = c.CaseNumber,
                        CaseTitle = c.CaseTitle,
                        ClientName = $"{c.Client.Name.First} {c.Client.Name.Last}",
                        AttorneyName = $"{c.AssignedAttorney.Name.First} {c.AssignedAttorney.Name.Last}",
                        DaysOpen = daysOpen,
                        CompletionRate = totalDeadlines > 0 ? (double)completedDeadlines / totalDeadlines * 100 : 100,
                        RiskScore = riskScore,
                        PredictedOutcome = (overdueDeadlines == 0 && attorneySuccessRate > 80) ? "Likely Success" :
                                          (overdueDeadlines > 3 || attorneySuccessRate < 50) ? "High Risk" : "Moderate Risk",
                        EstimatedValue = c.EstimatedValue ?? 0
                    };
                })
                .OrderByDescending(x => x.RiskScore)
                .ToList();

            return result;
        }

        public async Task<IEnumerable<CriticalCaseDTO>> GetCriticalCasesAsync() // Changed return type
        {
            var criticalCases = await _context.Cases
                .Where(c => c.Status == "Open" || c.Status == "Active")
                .Where(c => c.Deadlines.Any(d => d.DeadlineDate <= DateTime.Now.AddDays(7) && !d.IsCompleted))
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Deadlines)
                .ToListAsync();

            var result = criticalCases
                .Select(c => {
                    var upcomingDeadlines = c.Deadlines
                        .Where(d => d.DeadlineDate <= DateTime.Now.AddDays(7) && !d.IsCompleted)
                        .OrderBy(d => d.DeadlineDate)
                        .ToList();
                    var totalOverdueDeadlines = c.Deadlines.Count(d => d.DeadlineDate < DateTime.Now && !d.IsCompleted);
                    var attorneyWorkload = _context.Cases.Count(ac => ac.AssignedAttorneyId == c.AssignedAttorneyId &&
                                                                    (ac.Status == "Open" || ac.Status == "Active"));
                    var caseAge = (DateTime.Now - c.FilingDate).Days;
                    var nextDeadline = upcomingDeadlines.FirstOrDefault();

                    var priorityScore = (totalOverdueDeadlines * 50) +
                                       (nextDeadline != null ? Math.Max(0, 7 - (nextDeadline.DeadlineDate - DateTime.Now).Days) * 10 : 0) +
                                       ((c.EstimatedValue ?? 0) > 100000 ? 25 : 0);

                    return new CriticalCaseDTO
                    {
                        CaseNumber = c.CaseNumber,
                        CaseTitle = c.CaseTitle,
                        ClientName = $"{c.Client.Name.First} {c.Client.Name.Last}",
                        AttorneyName = $"{c.AssignedAttorney.Name.First} {c.AssignedAttorney.Name.Last}",
                        CaseValue = c.EstimatedValue ?? 0,
                        CaseAge = caseAge,
                        NextDeadline = nextDeadline?.Description ?? "No upcoming deadlines",
                        NextDeadlineDate = nextDeadline?.DeadlineDate,
                        DaysUntilDeadline = nextDeadline != null
                            ? (nextDeadline.DeadlineDate - DateTime.Now).Days
                            : (int?)null,
                        OverdueCount = totalOverdueDeadlines,
                        AttorneyWorkload = attorneyWorkload,
                        PriorityScore = priorityScore
                    };
                })
                .Where(x => x.OverdueCount > 0 || x.DaysUntilDeadline <= 7)
                .OrderByDescending(x => x.PriorityScore)
                .ToList();

            return result;
        }

        public async Task<IEnumerable<MonthlyTrendDTO>> GetMonthlyTrendsAsync() // Changed return type
        {
            var monthlyData = await _context.Cases
                .GroupBy(c => new { c.FilingDate.Year, c.FilingDate.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    NewCases = g.Count(),
                    TotalRevenue = g.Sum(c => c.EstimatedValue ?? 0),
                    AvgCaseValue = g.Average(c => c.EstimatedValue ?? 0)
                })
                .ToListAsync();

            var result = monthlyData
                .Select(x => new MonthlyTrendDTO
                {
                    Period = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                    NewCases = x.NewCases,
                    TotalRevenue = x.TotalRevenue,
                    AvgCaseValue = x.AvgCaseValue
                })
                .OrderBy(x => DateTime.ParseExact(x.Period, "MMM yyyy", null))
                .ToList();

            return result;
        }

        public async Task<IEnumerable<DeadlinePerformanceDTO>> GetDeadlinePerformanceAsync() // Changed return type
        {
            var deadlineData = await _context.Deadlines
                .Include(d => d.Case)
                    .ThenInclude(c => c.AssignedAttorney)
                .GroupBy(d => d.CaseId)
                .Select(g => new {
                    CaseNumber = g.First().Case.CaseNumber,
                    AttorneyName = $"{g.First().Case.AssignedAttorney.Name.First} {g.First().Case.AssignedAttorney.Name.Last}",
                    TotalDeadlines = g.Count(),
                    CompletedOnTime = g.Count(d => d.IsCompleted && d.CompletedDate <= d.DeadlineDate),
                    CompletedLate = g.Count(d => d.IsCompleted && d.CompletedDate > d.DeadlineDate),
                    StillPending = g.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now),
                    Overdue = g.Count(d => !d.IsCompleted && d.DeadlineDate < DateTime.Now)
                })
                .ToListAsync();

            var result = deadlineData
                .Select(x => new DeadlinePerformanceDTO
                {
                    CaseNumber = x.CaseNumber,
                    AttorneyName = x.AttorneyName,
                    TotalDeadlines = x.TotalDeadlines,
                    CompletedOnTime = x.CompletedOnTime,
                    CompletedLate = x.CompletedLate,
                    StillPending = x.StillPending,
                    Overdue = x.Overdue,
                    OnTimePercentage = x.TotalDeadlines > 0 ? (double)x.CompletedOnTime / x.TotalDeadlines * 100 : 0
                })
                .OrderByDescending(x => x.OnTimePercentage)
                .ToList();

            return result;
        }
    }
}