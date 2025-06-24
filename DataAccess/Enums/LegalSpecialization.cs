
using System.ComponentModel;

namespace Litigator.DataAccess.Enums
{
    public enum LegalSpecialization
    {
        [Description("General Practice")]
        GeneralPractice = 0,

        [Description("Criminal Law")]
        CriminalLaw = 1,

        [Description("Civil Litigation")]
        CivilLitigation = 2,

        [Description("Family Law")]
        FamilyLaw = 3,

        [Description("Corporate Law")]
        CorporateLaw = 4,

        [Description("Real Estate Law")]
        RealEstateLaw = 5,

        [Description("Personal Injury")]
        PersonalInjury = 6,

        [Description("Employment Law")]
        EmploymentLaw = 7,

        [Description("Immigration Law")]
        ImmigrationLaw = 8,

        [Description("Bankruptcy Law")]
        BankruptcyLaw = 9,

        [Description("Tax Law")]
        TaxLaw = 10,

        [Description("Intellectual Property")]
        IntellectualProperty = 11
    }
}