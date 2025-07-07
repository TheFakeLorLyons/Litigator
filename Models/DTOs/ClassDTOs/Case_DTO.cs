using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Litigator.Models.DTOs.ClassDTOs
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
        public decimal? CurrentRealCost { get; set; }

        public required string ClientFirstName { get; set; }
        public string? ClientLastName { get; set; }
        public string? AttorneyFirstName { get; set; }
        public string? AttorneyLastName { get; set; }
        public string? CourtName { get; set; }
        public int OpenDeadlines { get; set; }
    }

    public class CaseDetailDTO
    {
        public required int CaseId { get; set; }
        public required string CaseNumber { get; set; }
        public required string CaseTitle { get; set; }
        public required string CaseType { get; set; }
        public DateTime FilingDate { get; set; }
        public required string Status { get; set; }
        public decimal? EstimatedValue { get; set; }

        // Client information
        public int ClientId { get; set; }
        public required string ClientFirstName { get; set; }
        public string? ClientLastName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }

        // Attorney information
        public int AssignedAttorneyId { get; set; }
        public string? AttorneyFirstName { get; set; }
        public string? AttorneyLastName { get; set; }
        public string? AttorneyEmail { get; set; }

        // Court information
        public int CourtId { get; set; }
        public string? CourtName { get; set; }
        public string? CourtAddress { get; set; }

        // Deadline and document counts
        public int TotalDeadlines { get; set; }
        public int OpenDeadlines { get; set; }
        public int OverdueDeadlines { get; set; }
        public int TotalDocuments { get; set; }

        // Next deadline information
        public DateTime? NextDeadlineDate { get; set; }
        public string? NextDeadlineDescription { get; set; }
    }

    public class CaseCreateDTO
    {
        [Required, MaxLength(50)]
        public required string CaseNumber { get; set; }
        
        [Required, MaxLength(200)]
        public required string CaseTitle { get; set; }
        
        [MaxLength(50)]
        public required string CaseType { get; set; } // Civil, Criminal, Family, etc.
        
        public DateTime FilingDate { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Closed, Pending
        
        public decimal? EstimatedValue { get; set; }
        
        public decimal? CurrentRealCost { get; set; }
        
        // Foreign Keys
        public int ClientId { get; set; }
        public int AssignedAttorneyId { get; set; }
        public int AssignedJudgeId { get; set; }
        public int CourtId { get; set; }
    }

    public class CaseUpdateDTO
    {
        [Required, MaxLength(50)]
        public required string CaseNumber { get; set; }
        
        [Required, MaxLength(200)]
        public required string CaseTitle { get; set; }
        
        [MaxLength(50)]
        public required string CaseType { get; set; }
        
        public DateTime FilingDate { get; set; }
        
        [MaxLength(20)]
        public required string Status { get; set; }
        
        public decimal? EstimatedValue { get; set; }
        
        public decimal? CurrentRealCost { get; set; }
        
        // Foreign Keys
        public int ClientId { get; set; }
        public int AssignedAttorneyId { get; set; }
        public int AssignedJudgeId { get; set; }
        public int CourtId { get; set; }
    }
}