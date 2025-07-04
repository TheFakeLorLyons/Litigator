using Litigator.DataAccess.Entities;
using Litigator.DataAccess.Enums;
using Litigator.DataAccess.ValueObjects;

public abstract class LegalProfessional : Person
{
    [MaxLength(50)]
    [Required]
    public required string BarNumber { get; set; }

    // Override to make required for legal professionals
    [Required]
    public new required Address PrimaryAddress { get; set; }

    // Make phone required for legal professionals too
    [Required]
    public new required PhoneNumber PrimaryPhone { get; set; }

    public LegalSpecialization Specialization { get; set; } = LegalSpecialization.GeneralPractice;
    public bool IsActive { get; set; } = true;

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Court> Courts { get; set; } = new List<Court>();

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

    // Override to ensure legal professionals always have populated contact info
    public override void SetPrimaryAddress(Address? address)
    {
        if (address == null)
            throw new ArgumentNullException(nameof(address), "Address is required for legal professionals");

        PrimaryAddress = address;
        address.SetPopulated();
    }

    public override void SetPrimaryPhone(PhoneNumber? phone)
    {
        if (phone == null)
            throw new ArgumentNullException(nameof(phone), "Phone is required for legal professionals");

        PrimaryPhone = phone;
        phone.SetPopulated();
    }
}