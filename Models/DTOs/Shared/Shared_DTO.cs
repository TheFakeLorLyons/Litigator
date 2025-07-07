using Litigator.DataAccess.Entities;

namespace Litigator.Models.DTOs.Shared
{
    public class AddressDTO
    {
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public AddressType Type { get; set; }
        public string SingleLine { get; set; } = string.Empty;
        public string MultiLine { get; set; } = string.Empty;
    }

    public class PhoneNumberDTO
    {
        public string? Number { get; set; }
        public string? Extension { get; set; }
        public PhoneType Type { get; set; }
        public string Formatted { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }
}