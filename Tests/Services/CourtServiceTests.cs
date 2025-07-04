using AutoMapper;
using Bogus;
using Xunit;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.DTOs.Shared;
using Litigator.Models.Mapping;
using Litigator.Services.Implementations;

public class CourtServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly CourtService _courtService;
    private readonly IMapper _mapper;
    private readonly Faker<Court> _courtEntityFaker;
    private readonly Faker<CreateCourtDTO> _createCourtFaker;

    public CourtServiceTests()
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
        _courtService = new CourtService(_context, _mapper);

        // Setup Bogus faker for CreateCourtDTO
        _createCourtFaker = new Faker<CreateCourtDTO>()
            .RuleFor(c => c.CourtName, f => f.Company.CompanyName() + " Court")
            .RuleFor(c => c.County, f => f.Address.County())
            .RuleFor(c => c.State, f => f.Address.StateAbbr())
            .RuleFor(c => c.CourtType, f => f.PickRandom("Federal", "State", "Municipal", "Family"))
            .RuleFor(c => c.Division, f => f.PickRandom("Civil", "Criminal", "Family", "Probate"))
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Website, f => f.Internet.Url())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Address, f => new AddressDTO
            {
                Line1 = f.Address.StreetAddress(),
                City = f.Address.City(),
                State = f.Address.StateAbbr(),
                PostalCode = f.Address.ZipCode(),
                Country = "USA"
            })
            .RuleFor(c => c.ChiefJudge, f => f.Name.FullName())
            .RuleFor(c => c.ClerkOfCourt, f => f.Name.FullName())
            .RuleFor(c => c.BusinessHours, f => "9:00 AM - 5:00 PM")
            .RuleFor(c => c.IsActive, f => true);

        // Setup Bogus faker for Court entity (for direct database seeding)
        _courtEntityFaker = new Faker<Court>()
            .RuleFor(c => c.CourtName, f => f.Company.CompanyName() + " Court")
            .RuleFor(c => c.County, f => f.Address.County())
            .RuleFor(c => c.State, f => f.Address.StateAbbr())
            .RuleFor(c => c.CourtType, f => f.PickRandom("Federal", "State", "Municipal", "Family"))
            .RuleFor(c => c.Division, f => f.PickRandom("Civil", "Criminal", "Family", "Probate"))
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Website, f => f.Internet.Url())
            .RuleFor(c => c.Phone, f => new PhoneNumber { Number = f.Phone.PhoneNumber() })
            .RuleFor(c => c.Address, f => new Address
            {
                Line1 = f.Address.StreetAddress(),
                City = f.Address.City(),
                State = f.Address.StateAbbr(),
                PostalCode = f.Address.ZipCode(),
                Country = "USA"
            })
            .RuleFor(c => c.ChiefJudge, f => (Judge?)null) // Set to null for simplicity
            .RuleFor(c => c.ClerkOfCourt, f => f.Name.FullName())
            .RuleFor(c => c.BusinessHours, f => "9:00 AM - 5:00 PM")
            .RuleFor(c => c.IsActive, f => true)
            .RuleFor(c => c.CreatedDate, f => f.Date.Recent());
    }

    [Fact]
    public async Task CreateCourtAsync_ShouldCreateCourt_WhenValidDataProvided()
    {
        // Arrange
        var createCourtDto = _createCourtFaker.Generate();

        // Act
        var result = await _courtService.CreateCourtAsync(createCourtDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CourtId > 0);
        Assert.Equal(createCourtDto.CourtName, result.CourtName);
        Assert.Equal(createCourtDto.County, result.County);
        Assert.Equal(createCourtDto.State, result.State);
    }

    [Fact]
    public async Task GetCourtsByStateAsync_ShouldReturnCourtsFromSpecificState()
    {
        // Arrange
        var courts = new List<Court>
        {
            new Court {
                CourtName = "NY Court 1",
                Address = new Address
                {
                    Line1 = "123 Fake Street",
                    City = "Test City",
                    State = "NY",
                    PostalCode = "12345",
                    Country = "USA"
                },
                Phone = new PhoneNumber { Number = "718-618-2098" },
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Court {
                CourtName = "CA Court 1",
                Address = new Address
                {
                    Line1 = "456 Fake Street",
                    City = "Test City",
                    State = "CA",
                    PostalCode = "12345",
                    Country = "USA"
                },
                Phone = new PhoneNumber { Number = "347-401-9610" },
                Email = "kingsfamilycourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Court {
                CourtName = "NY Court 2",
                Address = new Address
                {
                    Line1 = "789 Fake Street",
                    City = "Test City",
                    State = "NY",
                    PostalCode = "12345",
                    Country = "USA"
                },
                Phone = new PhoneNumber { Number = "646-386-5223" },
                Email = "manhattanfamilycourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "Municipal",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };

        _context.Courts.AddRange(courts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courtService.GetCourtsByStateAsync("NY");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal("NY", c.State));
    }

    [Fact]
    public async Task GetAllCourtsAsync_ShouldReturnAllCourts()
    {
        // Arrange
        var courts = _courtEntityFaker.Generate(5);
        _context.Courts.AddRange(courts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courtService.GetAllCourtsAsync();

        // Assert
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task GetCourtByIdAsync_ShouldReturnCourt_WhenCourtExists()
    {
        // Arrange
        var court = _courtEntityFaker.Generate();
        _context.Courts.Add(court);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courtService.GetCourtByIdAsync(court.CourtId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(court.CourtId, result.CourtId);
        Assert.Equal(court.CourtName, result.CourtName);
    }

    [Fact]
    public async Task GetCourtByIdAsync_ShouldReturnNull_WhenCourtDoesNotExist()
    {
        // Act
        var result = await _courtService.GetCourtByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveCourtsAsync_ShouldReturnOnlyActiveCourts()
    {
        // Arrange
        var activeCourts = _courtEntityFaker.Generate(3);
        var inactiveCourts = _courtEntityFaker.Generate(2);
        inactiveCourts.ForEach(c => c.IsActive = false);

        _context.Courts.AddRange(activeCourts);
        _context.Courts.AddRange(inactiveCourts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courtService.GetActiveCourtsAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, c => Assert.True(c.IsActive));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}