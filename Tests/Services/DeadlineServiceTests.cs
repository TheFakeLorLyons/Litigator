using Microsoft.EntityFrameworkCore;
using Bogus;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Services.Implementations;

public class DeadlineServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly DeadlineService _deadlineService;
    private readonly Faker<Deadline> _deadlineFaker;

    public DeadlineServiceTests()
    {
        var options = new DbContextOptionsBuilder<LitigatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LitigatorDbContext(options);
        _deadlineService = new DeadlineService(_context);

        SeedTestData();

        // Setup Bogus faker for Deadline
        _deadlineFaker = new Faker<Deadline>()
            .RuleFor(d => d.DeadlineType, f => f.PickRandom("Filing", "Discovery", "Motion", "Trial", "Appeal"))
            .RuleFor(d => d.Description, f => f.Lorem.Sentence())
            .RuleFor(d => d.DeadlineDate, f => f.Date.Future(60))
            .RuleFor(d => d.IsCritical, f => f.Random.Bool(0.3f))
            .RuleFor(d => d.IsCompleted, f => false)
            .RuleFor(d => d.CaseId, f => 1)
            .RuleFor(d => d.Notes, f => f.Lorem.Paragraph());
    }

    private void SeedTestData()
    {
        var attorney = new Attorney
        {
            AttorneyId = 1,
            FirstName = "Jane",
            LastName = "Smith",
            BarNumber = "67890",
            Email = "jane.smith@law.com",
            Phone = "617-232-1234"
        };

        var client = new Client
        {
            ClientId = 1,
            ClientName = "Deadline Test Client",
            Address = "456 Test Ave",
            Phone = "(555) 987-6543",
            Email = "deadlineclient@test.com"
        };

        var court = new Court
        {
            CourtId = 1,
            CourtName = "Deadline Test Court",
            Address = "123 Fake Street",
            County = "Test County",
            State = "NY",
            CourtType = "State"
        };

        var testCase = new Case
        {
            CaseId = 1,
            CaseNumber = "2024-DEADLINE-001",
            CaseTitle = "Deadline Test Case",
            CaseType = "Civil",
            FilingDate = DateTime.Now.AddDays(-30),
            Client = client,
            AssignedAttorneyId = 1,
            CourtId = 1
        };

        _context.Attorneys.Add(attorney);
        _context.Clients.Add(client);
        _context.Courts.Add(court);
        _context.Cases.Add(testCase);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateDeadlineAsync_ShouldCreateDeadline_WhenValidDataProvided()
    {
        // Arrange
        var newDeadline = _deadlineFaker.Generate();

        // Act
        var result = await _deadlineService.CreateDeadlineAsync(newDeadline);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.DeadlineId > 0);
        Assert.Equal(newDeadline.DeadlineType, result.DeadlineType);
        Assert.Equal(newDeadline.Description, result.Description);
    }

    [Fact]
    public async Task GetUpcomingDeadlinesAsync_ShouldReturnUpcomingDeadlines()
    {
        // Arrange
        var testCase = _context.Cases.First();

        var deadlines = new List<Deadline>
            {
                new Deadline { DeadlineType = "Filing", Description = "Upcoming 1", DeadlineDate = DateTime.Now.AddDays(5), Case = testCase, IsCompleted = false },
                new Deadline { DeadlineType = "Discovery", Description = "Upcoming 2", DeadlineDate = DateTime.Now.AddDays(15), Case = testCase, IsCompleted = false },
                new Deadline { DeadlineType = "Motion", Description = "Too Far", DeadlineDate = DateTime.Now.AddDays(45), Case = testCase, IsCompleted = false }
            };

        _context.Deadlines.AddRange(deadlines);
        await _context.SaveChangesAsync();

        // Act
        var result = await _deadlineService.GetUpcomingDeadlinesAsync(30);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, d => Assert.True(d.DeadlineDate <= DateTime.Now.AddDays(30)));
    }

    [Fact]
    public async Task GetDeadlinesByCaseAsync_ShouldReturnDeadlinesForSpecificCase()
    {
        // Arrange
        var deadlines = _deadlineFaker.Generate(4);
        _context.Deadlines.AddRange(deadlines);
        await _context.SaveChangesAsync();

        // Act
        var result = await _deadlineService.GetDeadlinesByCaseAsync(1);

        // Assert
        Assert.Equal(4, result.Count());
        Assert.All(result, d => Assert.Equal(1, d.CaseId));
    }

    [Fact]
    public async Task UpdateDeadlineAsync_ShouldUpdateDeadline_WhenValidDataProvided()
    {
        // Arrange
        var deadline = _deadlineFaker.Generate();
        _context.Deadlines.Add(deadline);
        await _context.SaveChangesAsync();

        deadline.Description = "Updated Description";
        deadline.DeadlineDate = DateTime.Now.AddDays(10);

        // Act
        var result = await _deadlineService.UpdateDeadlineAsync(deadline);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Description", result.Description);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
