using Litigator.DataAccess.Enums;
using Litigator.DataAccess.ValueObjects;

namespace Litigator.DataAccess.Entities
{
    public class Attorney : LegalProfessional
    {
        public string AttorneyId => $"A#{SystemId}";

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

        // Factory method for creating attorneys
        public static Attorney Create(PersonName name, string barNumber,
            Address address, PhoneNumber phone, string? email = null,
            LegalSpecialization specialization = LegalSpecialization.GeneralPractice)
        {
            // Validate required fields
            if (address == null)
                throw new ArgumentNullException(nameof(address), "Address is required for attorneys");

            if (phone == null)
                throw new ArgumentNullException(nameof(phone), "Phone is required for attorneys");

            if (string.IsNullOrWhiteSpace(barNumber))
                throw new ArgumentException("Bar number is required for attorneys", nameof(barNumber));

            // Ensure address and phone are marked as populated
            address.SetPopulated();
            phone.SetPopulated();

            var attorney = new Attorney
            {
                Name = name,
                BarNumber = barNumber,
                Email = email,
                PrimaryAddress = address,
                PrimaryPhone = phone,
                Specialization = specialization
            };

            return attorney;
        }

        // Helper method to add a client
        public void AddClient(Client client)
        {
            if (client != null && !Clients.Contains(client))
            {
                Clients.Add(client);
                if (!client.Attorneys.Contains(this))
                {
                    client.Attorneys.Add(this);
                }
            }
        }

        // Helper method to remove a client
        public void RemoveClient(Client client)
        {
            if (client != null && Clients.Contains(client))
            {
                Clients.Remove(client);
                if (client.Attorneys.Contains(this))
                {
                    client.Attorneys.Remove(this);
                }
            }
        }
    }
}