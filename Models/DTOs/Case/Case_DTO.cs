namespace Litigator.Models.DTOs.Case
{
    public class CaseDTO
    {
        public int CaseId { get; set; }
        public required string CaseNumber { get; set; }
        public required string CaseTitle { get; set; }
        public required string CaseType { get; set; }
        public DateTime FilingDate { get; set; }
        public required string Status { get; set; }
        public decimal? EstimatedValue { get; set; }
        public required string ClientName { get; set; }
        public string? AttorneyName { get; set; }
        public string? CourtName { get; set; }
        public int OpenDeadlines { get; set; }
    }
}