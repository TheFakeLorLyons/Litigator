using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Litigator.DataAccess.ValueObjects
{
    [Owned]
    public class Address : IEquatable<Address>
    {
        [Column("AddressLine1")]
        [MaxLength(500)]
        public string? Line1 { get; set; }

        [Column("AddressLine2")]
        [MaxLength(500)]
        public string? Line2 { get; set; }

        [Column("City")]
        [MaxLength(100)]
        public string? City { get; set; }

        [Column("County")]
        [MaxLength(100)]
        public string? County { get; set; }

        [Column("State")]
        [MaxLength(50)]
        public string? State { get; set; }

        [Column("PostalCode")]
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Column("Country")]
        [MaxLength(100)]
        public string? Country { get; set; } = "United States";

        // This satisfies EF Core's requirement for a non-nullable property
        [Column("HasPrimaryAddress")]
        [Required]
        public bool IsPopulated { get; set; } = false;

        // Helper properties
        [NotMapped]
        [JsonIgnore]
        public bool HasMeaningfulData => !string.IsNullOrWhiteSpace(Line1) ||
                                       !string.IsNullOrWhiteSpace(City) ||
                                       !string.IsNullOrWhiteSpace(State);

        [NotMapped]
        [JsonIgnore]
        public string Display => FormatAddress();

        // Add these properties for AutoMapper compatibility
        [NotMapped]
        [JsonIgnore]
        public string SingleLine => FormatAddress();

        [NotMapped]
        [JsonIgnore]
        public string MultiLine => FormatAddressMultiLine();

        // Factory method
        public static Address Create(string? line1 = null, string? line2 = null,
            string? city = null, string? county = null, string? state = null,
            string? postalCode = null, string? country = "United States")
        {
            var address = new Address
            {
                Line1 = line1,
                Line2 = line2,
                City = city,
                County = county,
                State = state,
                PostalCode = postalCode,
                Country = country
            };

            address.SetPopulated();
            return address;
        }

        // Method to mark as populated when setting data
        public void SetPopulated()
        {
            IsPopulated = HasMeaningfulData;
        }

        // Helper method to clear the address
        public void Clear()
        {
            Line1 = null;
            Line2 = null;
            City = null;
            County = null;
            State = null;
            PostalCode = null;
            Country = "United States";
            IsPopulated = false;
        }

        private string FormatAddress()
        {
            if (!HasMeaningfulData) return string.Empty;

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Line1))
                parts.Add(Line1);

            if (!string.IsNullOrWhiteSpace(Line2))
                parts.Add(Line2);

            var cityStateZip = new List<string>();
            if (!string.IsNullOrWhiteSpace(City))
                cityStateZip.Add(City);

            if (!string.IsNullOrWhiteSpace(State))
                cityStateZip.Add(State);

            if (!string.IsNullOrWhiteSpace(PostalCode))
                cityStateZip.Add(PostalCode);

            if (cityStateZip.Count > 0)
                parts.Add(string.Join(", ", cityStateZip));

            return string.Join(", ", parts);
        }

        private string FormatAddressMultiLine()
        {
            if (!HasMeaningfulData) return string.Empty;

            var lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(Line1))
                lines.Add(Line1);

            if (!string.IsNullOrWhiteSpace(Line2))
                lines.Add(Line2);

            var cityStateZip = new List<string>();
            if (!string.IsNullOrWhiteSpace(City))
                cityStateZip.Add(City);

            if (!string.IsNullOrWhiteSpace(State))
                cityStateZip.Add(State);

            if (!string.IsNullOrWhiteSpace(PostalCode))
                cityStateZip.Add(PostalCode);

            if (cityStateZip.Count > 0)
                lines.Add(string.Join(", ", cityStateZip));

            return string.Join(Environment.NewLine, lines);
        }

        public override string ToString() => Display;

        public bool Equals(Address? other)
        {
            if (other is null) return false;
            return Line1 == other.Line1 && Line2 == other.Line2 &&
                   City == other.City && County == other.County &&
                   State == other.State && PostalCode == other.PostalCode &&
                   Country == other.Country;
        }

        public override bool Equals(object? obj) => Equals(obj as Address);

        public override int GetHashCode() =>
            HashCode.Combine(Line1, Line2, City, County, State, PostalCode, Country);
    }
}