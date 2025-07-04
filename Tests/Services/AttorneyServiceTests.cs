using AutoMapper;
using Bogus;
using Xunit;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.Enums;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.DTOs.Shared;
using Litigator.Models.Mapping;
using Litigator.Services.Implementations;

using EntityPerson = Litigator.DataAccess.Entities.Person;

public class AttorneyServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly AttorneyService _attorneyService;
    private readonly IMapper _mapper;
    private readonly Faker<AttorneyDetailDTO> _attorneyDtoFaker;

    public AttorneyServiceTests()
    {
        var options = new DbContextOptionsBuilder<LitigatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new LitigatorDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<LitigatorMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _attorneyService = new AttorneyService(_context, _mapper);

        // Setup Bogus faker for Attorney
        _attorneyDtoFaker = new Faker<AttorneyDetailDTO>()
            .RuleFor(a => a.AttorneyId, f => 0) // Will be set by database
            .RuleFor(a => a.FirstName, f => f.Name.FirstName())
            .RuleFor(a => a.LastName, f => f.Name.LastName())
            .RuleFor(a => a.MiddleName, f => f.Random.Bool(0.3f) ? f.Name.FirstName() : null)
            .RuleFor(a => a.Title, f => f.Random.Bool(0.2f) ? f.PickRandom("Mr.", "Ms.", "Dr.") : null)
            .RuleFor(a => a.Suffix, f => f.Random.Bool(0.1f) ? f.PickRandom("Jr.", "Sr.", "III") : null)
            .RuleFor(a => a.PreferredName, f => f.Random.Bool(0.2f) ? f.Name.FirstName() : null)
            .RuleFor(a => a.BarNumber, f => f.Random.AlphaNumeric(8).ToUpper())
            .RuleFor(a => a.Email, (f, a) => f.Internet.Email(a.FirstName, a.LastName))
            .RuleFor(a => a.PrimaryPhone, f => f.Phone.PhoneNumber())
            .RuleFor(a => a.PrimaryAddress, f => f.Address.FullAddress())
            .RuleFor(a => a.Specialization, f => f.PickRandom<LegalSpecialization>())
            .RuleFor(a => a.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(a => a.CreatedDate, f => f.Date.Past())
            .RuleFor(a => a.ModifiedDate, f => f.Date.Recent())
            // Set computed properties
            .RuleFor(a => a.DisplayName, (f, a) => $"{a.FirstName} {a.LastName}")
            .RuleFor(a => a.ProfessionalName, (f, a) => $"{a.FirstName} {a.LastName}")
            .RuleFor(a => a.FullName, (f, a) => $"{a.FirstName} {a.LastName}")
            .RuleFor(a => a.AllAddresses, f => new List<AddressDTO>())
            .RuleFor(a => a.AllPhones, f => new List<PhoneNumberDTO>())
            .RuleFor(a => a.Clients, f => new List<ClientDTO>())
            .RuleFor(a => a.TotalCases, f => f.Random.Int(0, 50))
            .RuleFor(a => a.ActiveCases, f => f.Random.Int(0, 25))
            .RuleFor(a => a.ClosedCases, f => f.Random.Int(0, 25))
            .RuleFor(a => a.LastCaseDate, f => f.Date.Recent())
            .RuleFor(a => a.MostRecentCaseTitle, f => f.Lorem.Sentence())
            .RuleFor(a => a.TotalCaseValue, f => f.Random.Decimal(0, 1000000));
    }

    // Helper method to create Attorney entity for direct database testing
    private Attorney CreateAttorneyEntity(string firstName, string lastName, string barNumber, string email, bool isActive = true)
    {
        return new Attorney
        {
            Name = new PersonName
            {
                First = firstName,
                Last = lastName
            },
            BarNumber = barNumber,
            Email = email,
            IsActive = isActive,
            Specialization = LegalSpecialization.GeneralPractice,
            PrimaryPhone = new PhoneNumber { Number = "555-1234" },
            PrimaryAddress = Address.Create("123 Main St", "Test City", "TX", "12345")
        };
    }

    [Fact]
    public async Task CreateAttorneyAsync_ShouldCreateAttorney_WhenValidDataProvided()
    {
        // Arrange
        var newAttorneyDto = _attorneyDtoFaker.Generate();

        // Act
        var result = await _attorneyService.CreateAttorneyAsync(newAttorneyDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AttorneyId > 0);
        Assert.Equal(newAttorneyDto.FirstName, result.FirstName);
        Assert.Equal(newAttorneyDto.LastName, result.LastName);
        Assert.Equal(newAttorneyDto.BarNumber, result.BarNumber);
    }

    [Fact]
    public async Task GetAttorneyByBarNumberAsync_ShouldReturnAttorney_WhenBarNumberExists()
    {
        // Arrange
        var testAttorney = CreateAttorneyEntity("John", "Doe", "BAR12345", "john.doe@law.com");
        _context.Attorneys.Add(testAttorney);
        await _context.SaveChangesAsync();

        // Act
        var result = await _attorneyService.GetAttorneyByBarNumberAsync(testAttorney.BarNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testAttorney.BarNumber, result.BarNumber);
        Assert.Equal(testAttorney.Name.First, result.FirstName);
    }

    [Fact]
    public async Task GetAttorneyByIdAsync_ShouldReturnNull_WhenAttorneyNotExists()
    {
        // Act
        var result = await _attorneyService.GetAttorneyByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAttorneyByIdAsync_ShouldReturnAttorney_WhenAttorneyExists()
    {
        // Arrange
        var testAttorney = CreateAttorneyEntity("Jane", "Smith", "JS123456", "jane.smith@law.com");
        _context.Attorneys.Add(testAttorney);
        await _context.SaveChangesAsync();

        var systemId = testAttorney.SystemId;

        // Act
        var result = await _attorneyService.GetAttorneyByIdAsync(systemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(systemId, result.AttorneyId);
        Assert.Equal(testAttorney.Name.First, result.FirstName);
        Assert.Equal(testAttorney.Name.Last, result.LastName);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}