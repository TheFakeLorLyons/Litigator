using Litigator.DataAccess.Enums;
using Litigator.DataAccess.ValueObjects;

namespace Litigator.DataAccess.Entities
{
    public class Judge : LegalProfessional
    {
        public string JudgeId => $"J#{SystemId}";

        // Factory method for creating judges
        public static Judge Create(PersonName name, string barNumber,
            Address address, PhoneNumber phone, string? email = null,
            LegalSpecialization specialization = LegalSpecialization.GeneralPractice)
        {
            // Validate required fields
            if (address == null)
                throw new ArgumentNullException(nameof(address), "Address is required for judges");

            if (phone == null)
                throw new ArgumentNullException(nameof(phone), "Phone is required for judges");

            if (string.IsNullOrWhiteSpace(barNumber))
                throw new ArgumentException("Bar number is required for judges", nameof(barNumber));

            // Ensure address and phone are marked as populated
            address.SetPopulated();
            phone.SetPopulated();

            var judge = new Judge
            {
                Name = name,
                BarNumber = barNumber,
                Email = email,
                PrimaryAddress = address,
                PrimaryPhone = phone,
                Specialization = specialization
            };

            return judge;
        }
    }
}