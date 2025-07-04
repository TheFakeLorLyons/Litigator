using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Litigator.Models.DTOs.Analytics
{
    public class AttorneyPerformanceDTO
    {
        public int? Rank { get; set; }
        public string? AttorneyName { get; set; }
        public string? BarNumber { get; set; }
        public string? Email { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? AvgCaseValue { get; set; }
        public int? CompletedCases { get; set; }
        public int? ActiveCases { get; set; }
        public int? TotalCases { get; set; }
        public int? OverdueDeadlines { get; set; }
        public decimal? CompletionRate { get; set; }
        public int? PerformanceScore { get; set; }
    }

    public class CaseOutcomePredictionDTO
    {
        public required string CaseNumber { get; set; }
        public required string CaseTitle { get; set; }
        public required string ClientName { get; set; }
        public required string AttorneyName { get; set; }
        public int DaysOpen { get; set; }
        public double CompletionRate { get; set; }
        public double RiskScore { get; set; }
        public string? PredictedOutcome { get; set; }
        public decimal EstimatedValue { get; set; }
    }

    public class CriticalCaseDTO
    {
        public required string CaseNumber { get; set; }
        public required string CaseTitle { get; set; }
        public required string ClientName { get; set; }
        public string? AttorneyName { get; set; }
        public decimal CaseValue { get; set; }
        public int CaseAge { get; set; }
        public required string NextDeadline { get; set; }
        public DateTime? NextDeadlineDate { get; set; }
        public int? DaysUntilDeadline { get; set; }
        public int OverdueCount { get; set; }
        public int AttorneyWorkload { get; set; }
        public double PriorityScore { get; set; }
    }

    public class MonthlyTrendDTO
    {
        public required string Period { get; set; }
        public int NewCases { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgCaseValue { get; set; }
    }

    public class DeadlinePerformanceDTO
    {
        public required string CaseNumber { get; set; }
        public required string AttorneyName { get; set; }
        public int TotalDeadlines { get; set; }
        public int CompletedOnTime { get; set; }
        public int CompletedLate { get; set; }
        public int StillPending { get; set; }
        public int Overdue { get; set; }
        public double OnTimePercentage { get; set; }
    }
}