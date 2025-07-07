using Litigator.Models.DTOs.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Litigator.Models.DTOs.ClassDTOs
{
    public class CourtDTO
    {
        public int CourtId { get; set; }
        public required string CourtName { get; set; }
        public required string County { get; set; }
        public required string State { get; set; }
        public string? CourtType { get; set; }
        public string? Division { get; set; }
        public string? Phone { get; set; }
        public required string Email { get; set; }
        public required string Website { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
    }

    public class CourtDetailDTO
    {
        public int CourtId { get; set; }

        // Basic court information
        public required string CourtName { get; set; }
        public required string County { get; set; }
        public required string State { get; set; }
        public string? CourtType { get; set; }
        public string? Division { get; set; }
        public string? Description { get; set; }

        // Contact information
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Full address and phone details
        public AddressDTO? AddressDetails { get; set; }
        public PhoneNumberDTO? PhoneDetails { get; set; }

        // Administrative information
        public string? ChiefJudge { get; set; }
        public string? ClerkOfCourt { get; set; }
        public string? BusinessHours { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Case statistics
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int ClosedCases { get; set; }
        public DateTime? LastCaseDate { get; set; }
        public string? MostRecentCaseTitle { get; set; }
    }

    public class CreateCourtDTO
    {
        [Required]
        [MaxLength(200)]
        public required string CourtName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string County { get; set; }

        [Required]
        [MaxLength(50)]
        public required string State { get; set; }

        [MaxLength(20)]
        public string? CourtType { get; set; }

        [MaxLength(50)]
        public string? Division { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public required string Email { get; set; }

        [MaxLength(500)]
        public required string Website { get; set; }

        public string? Phone { get; set; }

        public AddressDTO? Address { get; set; }

        [MaxLength(100)]
        public string? ChiefJudge { get; set; }

        [MaxLength(100)]
        public string? ClerkOfCourt { get; set; }

        [MaxLength(200)]
        public string? BusinessHours { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCourtDTO
    {
        public int CourtId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string CourtName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string County { get; set; }

        [Required]
        [MaxLength(50)]
        public required string State { get; set; }

        [MaxLength(20)]
        public string? CourtType { get; set; }

        [MaxLength(50)]
        public string? Division { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public required string Email { get; set; }

        [MaxLength(500)]
        public required string Website { get; set; }

        public string? Phone { get; set; }
        public AddressDTO? Address { get; set; }

        [MaxLength(100)]
        public string? ChiefJudge { get; set; }

        [MaxLength(100)]
        public string? ClerkOfCourt { get; set; }

        [MaxLength(200)]
        public string? BusinessHours { get; set; }

        public bool IsActive { get; set; } = true;
    }
}