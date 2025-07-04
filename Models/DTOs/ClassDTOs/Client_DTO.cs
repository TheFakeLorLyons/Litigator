using Litigator.Models.DTOs.Shared;

namespace Litigator.Models.DTOs.ClassDTOs
{
	public class ClientDTO
	{
		public int ClientId { get; set; }
		public required string Name { get; set; }
		public string? Email { get; set; }
		public string? PrimaryPhone { get; set; }
		public string? PrimaryAddress { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedDate { get; set; }
		public int TotalCases { get; set; }
		public int ActiveCases { get; set; }
	}

	public class ClientDetailDTO
	{
		public int ClientId { get; set; }

		// Name information
		public required string DisplayName { get; set; }
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

		// Client-specific
		public List<AttorneyDTO> Attorneys { get; set; } = new();
		public string? Notes { get; set; }
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
}