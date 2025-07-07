using Litigator.DataAccess.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Litigator.DataAccess.Entities
{
    public class Client : Person
    {
        public string ClientId => $"C#{SystemId}";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Attorney> Attorneys { get; set; } = new List<Attorney>();

        // Factory method for creating clients
        public static Client Create(PersonName name, string? email = null,
            Address? address = null, PhoneNumber? phone = null, string? notes = null)
        {
            var client = new Client
            {
                Name = name,
                Email = email,
                Notes = notes,
                PrimaryAddress = address,
                PrimaryPhone = phone
            };

            // Set populated flags if data exists
            if (address != null)
            {
                address.SetPopulated();
            }

            if (phone != null)
            {
                phone.SetPopulated();
            }

            return client;
        }

        // Helper method to add an attorney
        public void AddAttorney(Attorney attorney)
        {
            if (attorney != null && !Attorneys.Contains(attorney))
            {
                Attorneys.Add(attorney);
                if (!attorney.Clients.Contains(this))
                {
                    attorney.Clients.Add(this);
                }
            }
        }

        // Helper method to remove an attorney
        public void RemoveAttorney(Attorney attorney)
        {
            if (attorney != null && Attorneys.Contains(attorney))
            {
                Attorneys.Remove(attorney);
                if (attorney.Clients.Contains(this))
                {
                    attorney.Clients.Remove(this);
                }
            }
        }

        // Helper method to check if client has complete contact info
        public bool HasCompleteContactInfo()
        {
            return !string.IsNullOrWhiteSpace(Email) ||
                   (PrimaryPhone != null && PrimaryPhone.IsPopulated) ||
                   (PrimaryAddress != null && PrimaryAddress.IsPopulated);
        }
    }
}