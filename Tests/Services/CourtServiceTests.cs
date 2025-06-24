using Microsoft.EntityFrameworkCore;
using Bogus;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Services.Implementations;

public class CourtServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly CourtService _courtService;
    private readonly Faker<Court> _courtFaker;

    public CourtServiceTests()
    {
        var options = new DbContextOptionsBuilder<LitigatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LitigatorDbContext(options);
        _courtService = new CourtService(_context);

        // Setup Bogus faker for Court
        _courtFaker = new Faker<Court>()
            .RuleFor(c => c.CourtName, f => f.Company.CompanyName() + " Court")
            .RuleFor(c => c.County, f => f.Address.County())
            .RuleFor(c => c.State, f => f.Address.StateAbbr())
            .RuleFor(c => c.CourtType, f => f.PickRandom("Federal", "State", "Municipal", "Family"))
            .RuleFor(c => c.Address, f => f.Address.FullAddress());
    }

    [Fact]
    public async Task CreateCourtAsync_ShouldCreateCourt_WhenValidDataProvided()
    {
        // Arrange
        var newCourt = _courtFaker.Generate();

        // Act
        var result = await _courtService.CreateCourtAsync(newCourt);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CourtId > 0);
        Assert.Equal(newCourt.CourtName, result.CourtName);
        Assert.Equal(newCourt.County, result.County);
    }

    [Fact]
    public async Task GetCourtsByStateAsync_ShouldReturnCourtsFromSpecificState()
    {
        // Arrange
        var courts = new List<Court>
            {
                new Court { CourtName = "NY Court 1", County = "Nassau", Address = "123 Fake Stret", State = "NY", CourtType = "State" },
                new Court { CourtName = "CA Court 1", County = "Los Angeles", Address = "123 Fake Stret", State = "CA", CourtType = "State" },
                new Court { CourtName = "NY Court 2", County = "Suffolk", Address = "123 Fake Stret", State = "NY", CourtType = "Municipal" }
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
        var courts = _courtFaker.Generate(5);
        _context.Courts.AddRange(courts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courtService.GetAllCourtsAsync();

        // Assert
        Assert.Equal(5, result.Count());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}