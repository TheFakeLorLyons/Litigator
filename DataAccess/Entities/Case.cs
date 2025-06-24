using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Litigator.DataAccess.Entities
{
    public class Case
    {
        public int CaseId { get; set; }
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

        // Foreign Keys
        public int ClientId { get; set; }
        public int AssignedAttorneyId { get; set; }
        public int CourtId { get; set; }

        // Navigation properties
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual required Client Client { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Attorney? AssignedAttorney { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Court? Court { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Deadline> Deadlines { get; set; } = new List<Deadline>();
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}