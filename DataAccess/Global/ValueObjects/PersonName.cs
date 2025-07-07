using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Litigator.DataAccess.ValueObjects
{
    [Owned]
    public class PersonName : IEquatable<PersonName>
    {
        [Column("Title")]
        [MaxLength(20)]
        public string? Title { get; set; }

        [Column("FirstName")]
        [MaxLength(100)]
        [Required]
        public required string First { get; set; }

        [Column("MiddleName")]
        [MaxLength(100)]
        public string? Middle { get; set; }

        [Column("LastName")]
        [MaxLength(100)]
        public string? Last { get; set; }

        [Column("Suffix")]
        [MaxLength(20)]
        public string? Suffix { get; set; }

        [Column("PreferredName")]
        [MaxLength(100)]
        public string? Preferred { get; set; }

        // Computed properties for different name formats
        [NotMapped]
        [JsonIgnore]
        public string Full => BuildFullName();

        [NotMapped]
        [JsonIgnore]
        public string Professional => BuildProfessionalName();

        [NotMapped]
        [JsonIgnore]
        public string Display => string.IsNullOrWhiteSpace(Preferred) ?
            (string.IsNullOrWhiteSpace(Last) ? First : $"{First} {Last}") :
            (string.IsNullOrWhiteSpace(Last) ? Preferred : $"{Preferred} {Last}");

        [NotMapped]
        [JsonIgnore]
        public string LastFirst => string.IsNullOrWhiteSpace(Last) ? First : $"{Last}, {First}";

        [NotMapped]
        [JsonIgnore]
        public string Casual => string.IsNullOrWhiteSpace(Preferred) ? First : Preferred;

        private string BuildFullName()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Title)) parts.Add(Title);
            parts.Add(First);
            if (!string.IsNullOrWhiteSpace(Middle)) parts.Add(Middle);
            if (!string.IsNullOrWhiteSpace(Last)) parts.Add(Last);
            if (!string.IsNullOrWhiteSpace(Suffix)) parts.Add(Suffix);
            return string.Join(" ", parts);
        }

        private string BuildProfessionalName()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Title)) parts.Add(Title);
            parts.Add(First);
            if (!string.IsNullOrWhiteSpace(Last)) parts.Add(Last);
            if (!string.IsNullOrWhiteSpace(Suffix)) parts.Add(Suffix);
            return string.Join(" ", parts);
        }

        public static PersonName Create(string first, string? last = null) =>
            new() { First = first, Last = last };

        public bool IsValid() => !string.IsNullOrWhiteSpace(First);

        public override string ToString() => Display;

        public bool Equals(PersonName? other)
        {
            if (other is null) return false;
            return Title == other.Title && First == other.First && Middle == other.Middle &&
                   Last == other.Last && Suffix == other.Suffix && Preferred == other.Preferred;
        }

        public override bool Equals(object? obj) => Equals(obj as PersonName);

        public override int GetHashCode() =>
            HashCode.Combine(Title, First, Middle, Last, Suffix, Preferred);
    }
}