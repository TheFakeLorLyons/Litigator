using AutoMapper;
using Bogus;
using Xunit;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.Deadline;
using Litigator.Models.Mapping;
using Litigator.Services.Implementations;

public class DeadlineServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly DeadlineService _deadlineService;
    private readonly IMapper _mapper;
    private readonly Faker<DeadlineCreateDTO> _deadlineCreateFaker;
    private readonly Faker<DeadlineUpdateDTO> _deadlineUpdateFaker;

    public DeadlineServiceTests()
    {
        var options = new DbContextOptionsBuilder<LitigatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LitigatorDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<LitigatorMappingProfile>());
        _mapper = config.CreateMapper();
        _deadlineService = new DeadlineService(_context, _mapper);

        SeedTestData();

        // Setup Bogus faker for DeadlineCreateDTO
        _deadlineCreateFaker = new Faker<DeadlineCreateDTO>()
            .RuleFor(d => d.DeadlineType, f => f.PickRandom("Filing", "Discovery", "Motion", "Trial", "Appeal"))
            .RuleFor(d => d.Description, f => f.Lorem.Sentence())
            .RuleFor(d => d.DeadlineDate, f => f.Date.Future(60))
            .RuleFor(d => d.IsCritical, f => f.Random.Bool(0.3f))
            .RuleFor(d => d.CaseId, f => 1);

        // Setup Bogus faker for DeadlineUpdateDTO
        _deadlineUpdateFaker = new Faker<DeadlineUpdateDTO>()
            .RuleFor(d => d.DeadlineType, f => f.PickRandom("Filing", "Discovery", "Motion", "Trial", "Appeal"))
            .RuleFor(d => d.Description, f => f.Lorem.Sentence())
            .RuleFor(d => d.DeadlineDate, f => f.Date.Future(60))
            .RuleFor(d => d.IsCritical, f => f.Random.Bool(0.3f))
            .RuleFor(d => d.IsCompleted, f => f.Random.Bool(0.2f))
            .RuleFor(d => d.CompletedDate, f => f.Random.Bool(0.5f) ? f.Date.Recent(10) : null);
    }

    private void SeedTestData()
    {
        var attorney = new Attorney
        {
            SystemId = 1,
            Name = PersonName.Create("Jane", "Smith"),
            BarNumber = "67890",
            Email = "jane.smith@law.com",
            PrimaryPhone = PhoneNumber.Create("617-232-1234"),
            PrimaryAddress = Address.Create("456 Lawyer Street", "Law City", "NY", "10001"),
            Specialization = Litigator.DataAccess.Enums.LegalSpecialization.GeneralPractice,
            IsActive = true
        };

        var client = new Client
        {
            SystemId = 1,
            Name = PersonName.Create("John", "Doe"),
            PrimaryAddress = Address.Create("456 Test Ave", "Test City", "NY", "12345"),
            PrimaryPhone = PhoneNumber.Create("555-987-6543"),
            Email = "deadlineclient@test.com",
            IsActive = true
        };

        var court = new Court
        {
            CourtId = 1,
            CourtName = "Deadline Test Court",
            Address = Address.Create("123 Fake Street", "Test City", "NY", "12345"),
            Phone = PhoneNumber.Create("(123) 456-7890"),
            Email = "BronxFamilyCourt@nycourts.gov",
            Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
            CourtType = "State"
        };

        var testCase = new Case
        {
            CaseId = 1,
            CaseNumber = "2024-DEADLINE-001",
            CaseTitle = "Deadline Test Case",
            CaseType = "Civil",
            Status = "Active",
            Client = client,
            ClientId = 1,
            AssignedAttorneyId = 1,
            CourtId = 1,
            FilingDate = DateTime.Now
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
        var createDto = _deadlineCreateFaker.Generate();

        // Act
        var result = await _deadlineService.CreateDeadlineAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.DeadlineId > 0);
        Assert.Equal(createDto.DeadlineType, result.DeadlineType);
        Assert.Equal(createDto.Description, result.Description);
        Assert.Equal(createDto.DeadlineDate, result.DeadlineDate);
        Assert.Equal(createDto.IsCritical, result.IsCritical);
        Assert.Equal(createDto.CaseId, result.CaseId);
        Assert.False(result.IsCompleted);
        Assert.Null(result.CompletedDate);
    }

    [Fact]
    public async Task CreateDeadlineAsync_InvalidCaseId_ThrowsException()
    {
        // Arrange
        var createDto = new DeadlineCreateDTO
        {
            DeadlineType = "Discovery",
            Description = "Complete discovery",
            DeadlineDate = DateTime.Now.AddDays(30),
            IsCritical = true,
            CaseId = 999 // Non-existent case
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _deadlineService.CreateDeadlineAsync(createDto));
    }

    [Fact]
    public async Task GetAllDeadlinesAsync_ShouldReturnAllDeadlines()
    {
        // Arrange
        var testCase = _context.Cases.First();

        var deadlines = new List<Deadline>
        {
            new Deadline
            {
                DeadlineType = "Filing",
                Description = "Test Filing 1",
                DeadlineDate = DateTime.Now.AddDays(5),
                Case = testCase,
                CaseId = testCase.CaseId,
                IsCompleted = false,
                IsCritical = true
            },
            new Deadline
            {
                DeadlineType = "Motion",
                Description = "Test Motion",
                DeadlineDate = DateTime.Now.AddDays(10),
                Case = testCase,
                CaseId = testCase.CaseId,
                IsCompleted = true,
                IsCritical = false,
                CompletedDate = DateTime.Now.AddDays(-1)
            }
        };

        _context.Deadlines.AddRange(deadlines);
        await _context.SaveChangesAsync();

        // Act
        var result = await _deadlineService.GetAllDeadlinesAsync();

        // Assert
        Assert.Equal(2, result.Count());

        var filingDeadline = result.First(d => d.DeadlineType == "Filing");
        Assert.False(filingDeadline.IsCompleted);
        Assert.True(filingDeadline.IsCritical);
        Assert.Null(filingDeadline.CompletedDate);

        var motionDeadline = result.First(d => d.DeadlineType == "Motion");
        Assert.True(motionDeadline.IsCompleted);
        Assert.False(motionDeadline.IsCritical);
        Assert.NotNull(motionDeadline.CompletedDate);
    }

    [Fact]
    public async Task GetDeadlineByIdAsync_ShouldReturnDeadline_WhenExists()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var deadline = new Deadline
        {
            DeadlineType = "Trial",
            Description = "Trial Date",
            DeadlineDate = DateTime.Now.AddDays(30),
            Case = testCase,
            CaseId = testCase.CaseId,
            IsCompleted = false,
            IsCritical = true
        };

        _context.Deadlines.Add(deadline);
        await _context.SaveChangesAsync();

        // Act
        var result = await _deadlineService.GetDeadlineByIdAsync(deadline.DeadlineId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deadline.DeadlineId, result.DeadlineId);
        Assert.Equal("Trial", result.DeadlineType);
        Assert.Equal("Trial Date", result.Description);
        Assert.Equal(testCase.CaseId, result.CaseId);
        Assert.Equal(testCase.CaseNumber, result.CaseNumber);
        Assert.Equal(testCase.CaseTitle, result.CaseTitle);
    }

    [Fact]
    public async Task GetDeadlineByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _deadlineService.GetDeadlineByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUpcomingDeadlinesAsync_ShouldReturnUpcomingDeadlines()
    {
        // Arrange
        var testCase = _context.Cases.First();

        var deadlines = new List<Deadline>
            {
                new Deadline
                {
                    DeadlineType = "Upcoming1",
                    Description = "Within 30 days",
                    DeadlineDate = DateTime.Now.AddDays(15),
                    IsCritical = false,
                    IsCompleted = false,
                    Case = testCase,
                    CaseId = testCase.CaseId,
                    CreatedDate = DateTime.Now
                },
                new Deadline
                {
                    DeadlineType = "Upcoming2",
                    Description = "Also within 30 days",
                    DeadlineDate = DateTime.Now.AddDays(25),
                    IsCritical = true,
                    IsCompleted = false,
                    Case = testCase,
                    CaseId = testCase.CaseId,
                    CreatedDate = DateTime.Now
                },
                new Deadline
                {
                    DeadlineType = "TooFar",
                    Description = "Beyond 30 days",
                    DeadlineDate = DateTime.Now.AddDays(35),
                    IsCritical = false,
                    IsCompleted = false,
                    Case = testCase,
                    CaseId = testCase.CaseId,
                    CreatedDate = DateTime.Now
                },
                new Deadline
                {
                    DeadlineType = "Completed",
                    Description = "Already completed",
                    DeadlineDate = DateTime.Now.AddDays(10),
                    IsCritical = false,
                    IsCompleted = true,
                    CompletedDate = DateTime.Now.AddDays(-1),
                    Case = testCase,
                    CaseId = testCase.CaseId,
                    CreatedDate = DateTime.Now
                }
            };

        _context.Deadlines.AddRange(deadlines);
        await _context.SaveChangesAsync();

        // Act
        var result = await _deadlineService.GetUpcomingDeadlinesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, d => Assert.False(d.IsCompleted));
        Assert.All(result, d => Assert.True(d.DeadlineDate >= DateTime.Now));
        Assert.All(result, d => Assert.True(d.DeadlineDate <= DateTime.Now.AddDays(30)));

        // Verify ordering (earliest first)
        var orderedResult = result.ToList();
        Assert.True(orderedResult[0].DeadlineDate <= orderedResult[1].DeadlineDate);
    }

    [Fact]
    public async Task GetDeadlinesByCaseAsync_ShouldReturnDeadlinesForSpecificCase()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var deadlines = new List<Deadline>
        {
            new Deadline
            {
                DeadlineType = "Filing",
                Description = "Test Filing",
                DeadlineDate = DateTime.Now.AddDays(5),
                Case = testCase,
                CaseId = testCase.CaseId,
                IsCompleted = false,
                IsCritical = true
            },
            new Deadline
            {
                DeadlineType = "Discovery",
                Description = "Test Discovery",
                DeadlineDate = DateTime.Now.AddDays(15),
                Case = testCase,
                CaseId = testCase.CaseId,
                IsCompleted = false,
                IsCritical = false
            }
        };

        _context.Deadlines.AddRange(deadlines);
        await _context.SaveChangesAsync();

        // Act
        var result = await _deadlineService.GetDeadlinesByCaseAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, d => Assert.Equal(1, d.CaseId));
        Assert.All(result, d => Assert.Equal("2024-DEADLINE-001", d.CaseNumber));
        Assert.All(result, d => Assert.Equal("Deadline Test Case", d.CaseTitle));
    }

    [Fact]
    public async Task UpdateDeadlineAsync_ShouldUpdateDeadline_WhenValidDataProvided()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var deadline = new Deadline
        {
            DeadlineType = "Original Type",
            Description = "Original Description",
            DeadlineDate = DateTime.Now.AddDays(10),
            Case = testCase,
            CaseId = testCase.CaseId,
            IsCompleted = false,
            IsCritical = false
        };

        _context.Deadlines.Add(deadline);
        await _context.SaveChangesAsync();

        var updateDto = new DeadlineUpdateDTO
        {
            DeadlineType = "Updated Type",
            Description = "Updated Description",
            DeadlineDate = DateTime.Now.AddDays(20),
            IsCompleted = true,
            CompletedDate = DateTime.Now,
            IsCritical = true
        };

        // Act
        var result = await _deadlineService.UpdateDeadlineAsync(deadline.DeadlineId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Type", result.DeadlineType);
        Assert.Equal("Updated Description", result.Description);
        Assert.True(result.IsCompleted);
        Assert.True(result.IsCritical);
        Assert.NotNull(result.CompletedDate);
    }

    [Fact]
    public async Task UpdateDeadlineAsync_ShouldReturnNull_WhenDeadlineNotExists()
    {
        // Arrange
        var updateDto = _deadlineUpdateFaker.Generate();

        // Act
        var result = await _deadlineService.UpdateDeadlineAsync(999, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDeadlineAsync_ShouldReturnTrue_WhenDeadlineExists()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var deadline = new Deadline
        {
            DeadlineType = "To Delete",
            Description = "This will be deleted",
            DeadlineDate = DateTime.Now.AddDays(5),
            Case = testCase,
            CaseId = testCase.CaseId,
            IsCompleted = false,
            IsCritical = false
        };

        _context.Deadlines.Add(deadline);
        await _context.SaveChangesAsync();

        // Act
        var result = await _deadlineService.DeleteDeadlineAsync(deadline.DeadlineId);

        // Assert
        Assert.True(result);

        // Verify it's actually deleted
        var deletedDeadline = await _context.Deadlines.FindAsync(deadline.DeadlineId);
        Assert.Null(deletedDeadline);
    }

    [Fact]
    public async Task DeleteDeadlineAsync_ShouldReturnFalse_WhenDeadlineNotExists()
    {
        // Act
        var result = await _deadlineService.DeleteDeadlineAsync(999);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}