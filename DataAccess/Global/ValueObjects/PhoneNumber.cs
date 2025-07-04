using System;
using System.Text.RegularExpressions;

namespace Litigator.DataAccess.ValueObjects
{
    [Owned]
    public class PhoneNumber : IEquatable<PhoneNumber>
    {
        [Column("PhoneNumber")]
        [MaxLength(20)]
        public string? Number { get; set; }

        [Column("PhoneExtension")]
        [MaxLength(10)]
        public string? Extension { get; set; }

        // This satisfies EF Core's requirement for a non-nullable property
        [Column("HasPrimaryPhone")]
        [Required]
        public bool IsPopulated { get; set; } = false;

        // Helper properties
        [NotMapped]
        [JsonIgnore]
        public bool HasMeaningfulData => !string.IsNullOrWhiteSpace(Number);

        [NotMapped]
        [JsonIgnore]
        public string Formatted => FormatNumber();

        [NotMapped]
        [JsonIgnore]
        public string Display => string.IsNullOrWhiteSpace(Extension) ?
            Formatted : $"{Formatted} ext. {Extension}";

        // Factory method
        public static PhoneNumber Create(string? number = null, string? extension = null)
        {
            var phone = new PhoneNumber
            {
                Number = number,
                Extension = extension
            };

            phone.SetPopulated();
            return phone;
        }

        // Method to mark as populated when setting data
        public void SetPopulated()
        {
            IsPopulated = HasMeaningfulData;
        }

        // Helper method to clear the phone
        public void Clear()
        {
            Number = null;
            Extension = null;
            IsPopulated = false;
        }

        private string FormatNumber()
        {
            if (string.IsNullOrWhiteSpace(Number)) return string.Empty;
            var digits = Regex.Replace(Number, @"\D", "");
            return digits.Length switch
            {
                10 => $"({digits[..3]}) {digits[3..6]}-{digits[6..10]}",
                11 when digits.StartsWith("1") => $"+1 ({digits[1..4]}) {digits[4..7]}-{digits[7..11]}",
                _ => Number
            };
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Number)) return false;
            var digits = Regex.Replace(Number, @"\D", "");
            return digits.Length >= 10 && digits.Length <= 11;
        }

        public override string ToString() => Display;

        public bool Equals(PhoneNumber? other)
        {
            if (other is null) return false;
            return Number == other.Number && Extension == other.Extension;
        }

        public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

        public override int GetHashCode() => HashCode.Combine(Number, Extension);
    }
}