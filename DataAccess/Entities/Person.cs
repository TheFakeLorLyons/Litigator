using Litigator.DataAccess.ValueObjects;

namespace Litigator.DataAccess.Entities
{
    public abstract class Person
    {
        [Key]
        public int SystemId { get; set; }
        public virtual string PersonId => $"P#{SystemId}";

        [Required]
        public required PersonName Name { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        // These are optional for base Person (allows for clients without addresses/phones)
        public Address? PrimaryAddress { get; set; }
        public PhoneNumber? PrimaryPhone { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<PersonAddress> AdditionalAddresses { get; set; } = new List<PersonAddress>();

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<PersonPhoneNumber> AdditionalPhones { get; set; } = new List<PersonPhoneNumber>();

        // Helper methods
        public IEnumerable<Address> GetAllAddresses()
        {
            var addresses = new List<Address>();
            if (PrimaryAddress != null && PrimaryAddress.IsPopulated)
                addresses.Add(PrimaryAddress);
            addresses.AddRange(AdditionalAddresses.Select(a => a.Address).Where(a => a.IsPopulated));
            return addresses;
        }

        public IEnumerable<PhoneNumber> GetAllPhoneNumbers()
        {
            var phones = new List<PhoneNumber>();
            if (PrimaryPhone != null && PrimaryPhone.IsPopulated)
                phones.Add(PrimaryPhone);
            phones.AddRange(AdditionalPhones.Select(p => p.PhoneNumber).Where(p => p.IsPopulated));
            return phones;
        }

        // Helper methods to add additional contact info
        public void AddAddress(Address address, AddressType type)
        {
            if (address != null)
            {
                address.SetPopulated();
                AdditionalAddresses.Add(new PersonAddress
                {
                    Address = address,
                    Type = type
                });
            }
        }

        public void AddPhoneNumber(PhoneNumber phone, PhoneType type)
        {
            if (phone != null)
            {
                phone.SetPopulated();
                AdditionalPhones.Add(new PersonPhoneNumber
                {
                    PhoneNumber = phone,
                    Type = type
                });
            }
        }

        // Helper method to set primary contact info
        public virtual void SetPrimaryAddress(Address? address)
        {
            PrimaryAddress = address;
            if (address != null)
            {
                address.SetPopulated();
            }
        }

        public virtual void SetPrimaryPhone(PhoneNumber? phone)
        {
            PrimaryPhone = phone;
            if (phone != null)
            {
                phone.SetPopulated();
            }
        }

        public static bool TryParsePrefixedId(string id, char prefix, out int systemId)
        {
            systemId = -1;
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (!id.StartsWith($"{prefix}#")) return false;
            return int.TryParse(id[2..], out systemId);
        }
    }

    public class PersonAddress
    {
        public int Id { get; set; }
        public required Address Address { get; set; }
        public AddressType Type { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Person? Person { get; set; }
    }

    public class PersonPhoneNumber
    {
        public int Id { get; set; }
        public required PhoneNumber PhoneNumber { get; set; }
        public PhoneType Type { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Person? Person { get; set; }
    }

    public enum AddressType
    {
        Primary = 0,
        Home = 1,
        Work = 2,
        Emergency = 3,
        Other = 99
    }

    public enum PhoneType
    {
        Primary = 0,
        Mobile = 1,
        Home = 2,
        Work = 3,
        Other = 99
    }
}