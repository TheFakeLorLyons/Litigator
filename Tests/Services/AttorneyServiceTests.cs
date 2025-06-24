using Microsoft.EntityFrameworkCore;
using Bogus;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.Enums;
using Litigator.Services.Implementations;

public class AttorneyServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly AttorneyService _attorneyService;
    private readonly Faker<Attorney> _attorneyFaker;

    public AttorneyServiceTests()
    {
        var options = new DbContextOptionsBuilder<LitigatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LitigatorDbContext(options);
        _attorneyService = new AttorneyService(_context);

        // Setup Bogus faker for Attorney
        _attorneyFaker = new Faker<Attorney>()
            .RuleFor(a => a.FirstName, f => f.Name.FirstName())
            .RuleFor(a => a.LastName, f => f.Name.LastName())
            .RuleFor(a => a.BarNumber, f => f.Random.AlphaNumeric(8).ToUpper())
            .RuleFor(a => a.Email, (f, a) => f.Internet.Email(a.FirstName, a.LastName))
            .RuleFor(a => a.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(a => a.Specialization, f => f.PickRandom<LegalSpecialization>())
            .RuleFor(a => a.IsActive, f => f.Random.Bool(0.9f));
    }

    [Fact]
    public async Task CreateAttorneyAsync_ShouldCreateAttorney_WhenValidDataProvided()
    {
        // Arrange
        var newAttorney = _attorneyFaker.Generate();

        // Act
        var result = await _attorneyService.CreateAttorneyAsync(newAttorney);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AttorneyId > 0);
        Assert.Equal(newAttorney.FirstName, result.FirstName);
        Assert.Equal(newAttorney.LastName, result.LastName);
        Assert.Equal(newAttorney.BarNumber, result.BarNumber);
    }

    [Fact]
    public async Task GetAttorneyByBarNumberAsync_ShouldReturnAttorney_WhenBarNumberExists()
    {
        // Arrange
        var testAttorney = _attorneyFaker.Generate();
        _context.Attorneys.Add(testAttorney);
        await _context.SaveChangesAsync();

        // Act
        var result = await _attorneyService.GetAttorneyByBarNumberAsync(testAttorney.BarNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testAttorney.BarNumber, result.BarNumber);
        Assert.Equal(testAttorney.FirstName, result.FirstName);
    }

    [Fact]
    public async Task GetActiveAttorneysAsync_ShouldReturnOnlyActiveAttorneys()
    {
        // Arrange
        var attorneys = new List<Attorney>
            {
                new Attorney { FirstName = "Active", LastName = "Attorney1", BarNumber = "ACT001", Email = "active1@law.com", Phone = "555-0001", IsActive = true },
                new Attorney { FirstName = "Inactive", LastName = "Attorney2", BarNumber = "INA002", Email = "inactive2@law.com", Phone = "555-0002", IsActive = false },
                new Attorney { FirstName = "Active", LastName = "Attorney3", BarNumber = "ACT003", Email = "active3@law.com", Phone = "555-0003", IsActive = true }
            };

        _context.Attorneys.AddRange(attorneys);
        await _context.SaveChangesAsync();

        // Act
        var result = await _attorneyService.GetActiveAttorneysAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.True(a.IsActive));
    }

    [Fact]
    public async Task UpdateAttorneyAsync_ShouldThrowException_WhenDuplicateBarNumber()
    {
        // Arrange
        var attorney1 = new Attorney { FirstName = "First", LastName = "Attorney", BarNumber = "BAR001", Email = "first@law.com", Phone = "555-0001" };
        var attorney2 = new Attorney { FirstName = "Second", LastName = "Attorney", BarNumber = "BAR002", Email = "second@law.com", Phone = "555-0002" };

        _context.Attorneys.AddRange(attorney1, attorney2);
        await _context.SaveChangesAsync();

        attorney2.BarNumber = "BAR001"; // Duplicate bar number

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attorneyService.UpdateAttorneyAsync(attorney2));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}