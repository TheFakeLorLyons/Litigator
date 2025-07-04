using Litigator.DataAccess.ValueObjects;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Litigator.DataAccess.Entities
{
    public class Court
    {
        public int CourtId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string CourtName { get; set; }

        [MaxLength(20)]
        public string? CourtType { get; set; } // District, Circuit, Supreme, etc.

        [MaxLength(50)]
        public string? Division { get; set; } // Civil, Criminal, Family, etc.

        [MaxLength(500)]
        public string? Description { get; set; }

        public required Address Address { get; set; }

        public required PhoneNumber Phone { get; set; }

        [MaxLength(100)]
        public required string Email { get; set; }

        [MaxLength(500)]
        public required string Website { get; set; }

        // Administrative information
        public int? ChiefJudgeId { get; set; }
        [ForeignKey(nameof(ChiefJudgeId))]
        public Judge? ChiefJudge { get; set; }

        [MaxLength(100)]
        public string? ClerkOfCourt { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<LegalProfessional> LegalProfessionals { get; set; } = new List<LegalProfessional>();

        // Business hours
        [MaxLength(200)]
        public string? BusinessHours { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

        // Helper methods
        public string County => Address.County ?? string.Empty;
        public string State => Address.State ?? string.Empty;
        public string FullName => $"{CourtName}, {County} County, {State}";

        public int ActiveCasesCount => Cases?.Count(c => c.Status == "Active") ?? 0;
        public int TotalCasesCount => Cases?.Count ?? 0;

        public DateTime? LastCaseDate => Cases?.Any() == true
            ? Cases.OrderByDescending(c => c.FilingDate).First().FilingDate
            : null;
    }
}