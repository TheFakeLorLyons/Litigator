using Litigator.DataAccess.Enums;
using Litigator.Models.DTOs.Shared;

namespace Litigator.Models.DTOs.ClassDTOs
{
    public class JudgeDTO
    {
        public int JudgeId { get; set; }
        public required string Name { get; set; }
        public required string BarNumber { get; set; }
        public string? Email { get; set; }
        public string? PrimaryPhone { get; set; }
        public LegalSpecialization Specialization { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
    }

    public class JudgeDetailDTO
    {
        public int JudgeId { get; set; }

        // Name information
        public required string DisplayName { get; set; }
        public required string ProfessionalName { get; set; }
        public required string FullName { get; set; }
        public required string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? Title { get; set; }
        public string? Suffix { get; set; }
        public string? PreferredName { get; set; }

        // Contact information
        public string? Email { get; set; }
        public string? PrimaryPhone { get; set; }
        public string? PrimaryAddress { get; set; }

        // All addresses and phones
        public List<AddressDTO> AllAddresses { get; set; } = new();
        public List<PhoneNumberDTO> AllPhones { get; set; } = new();

        // Judge-specific
        public required string BarNumber { get; set; }
        public LegalSpecialization Specialization { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Case statistics
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int ClosedCases { get; set; }
        public DateTime? LastCaseDate { get; set; }
        public string? MostRecentCaseTitle { get; set; }
        public decimal? TotalCaseValue { get; set; }
    }
}
