using System.Collections.Generic;
using System.Reflection.Metadata;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

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
        [Precision(18, 2)]
        public decimal? CurrentRealCost { get; set; }

        // Foreign Keys
        public int ClientId { get; set; }
        public int AssignedAttorneyId { get; set; }
        public int AssignedJudgeId { get; set; }
        public int CourtId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Court? Court { get; set; }
        [JsonIgnore]
        public virtual Client Client { get; set; } = default!;
        [JsonIgnore]
        public virtual Attorney AssignedAttorney { get; set; } = default!;
        [JsonIgnore]
        public virtual Judge AssignedJudge { get; set; } = default!;
        [JsonIgnore]
        public virtual ICollection<Attorney> Attorneys { get; set; } = new List<Attorney>();
        [JsonIgnore]
        public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
        [JsonIgnore]
        public virtual ICollection<Deadline> Deadlines { get; set; } = new List<Deadline>();
        [JsonIgnore]
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}