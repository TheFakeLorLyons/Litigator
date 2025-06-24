using Microsoft.EntityFrameworkCore;
using Bogus;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Services.Implementations;

public class DocumentServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly DocumentService _documentService;
    private readonly Faker<Document> _documentFaker;

    public DocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<LitigatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LitigatorDbContext(options);
        _documentService = new DocumentService(_context);

        SeedTestData();

        // Setup Bogus faker for Document
        _documentFaker = new Faker<Document>()
            .RuleFor(d => d.DocumentName, f => f.System.FileName())
            .RuleFor(d => d.DocumentType, f => f.PickRandom("Pleading", "Motion", "Brief", "Evidence", "Contract"))
            .RuleFor(d => d.FilePath, f => f.System.FilePath())
            .RuleFor(d => d.FileSize, f => f.Random.Long(1000, 10000000))
            .RuleFor(d => d.UploadDate, f => f.Date.Recent(30))
            .RuleFor(d => d.CaseId, f => 1) // Use seeded case
            .RuleFor(d => d.Description, f => f.Lorem.Sentence())
            .RuleFor(d => d.UploadedBy, f => f.Person.FullName);
    }

    private void SeedTestData()
    {
        var attorney = new Attorney
        {
            AttorneyId = 1,
            FirstName = "Test Attorney",
            LastName = "Attorney",
            BarNumber = "TEST001",
            Email = "test@law.com",
            Phone = "617-555-1234"
        };

        var client = new Client
        {
            ClientId = 1,
            ClientName = "Test Client",
            Email = "client@test.com",
            Address = "123 Test St",
            Phone = "413-555-1234"
        };

        var court = new Court
        {
            CourtId = 1,
            CourtName = "Test Court",
            Address = "123 Fake Street",
            County = "Test County",
            State = "NY",
            CourtType = "State"
        };

        _context.Attorneys.Add(attorney);
        _context.Clients.Add(client);
        _context.Courts.Add(court);
        _context.SaveChanges();

        // Fixed: Remove duplicate testCase declaration and properly set required properties
        var testCase = new Case
        {
            CaseId = 1,
            CaseNumber = "TEST-001",
            CaseTitle = "Test Case",
            CaseType = "Civil",
            FilingDate = DateTime.Now,
            ClientId = 1,
            AssignedAttorneyId = 1,
            CourtId = 1,
            Client = client // Set the required Client navigation property
        };

        _context.Cases.Add(testCase);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateDocumentAsync_ShouldCreateDocument_WhenValidDataProvided()
    {
        // Arrange
        var newDocument = _documentFaker.Generate();

        // Act
        var result = await _documentService.CreateDocumentAsync(newDocument);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.DocumentId > 0);
        Assert.Equal(newDocument.DocumentName, result.DocumentName);
        Assert.Equal(newDocument.DocumentType, result.DocumentType);
    }

    [Fact]
    public async Task GetDocumentsByCaseAsync_ShouldReturnDocumentsForSpecificCase()
    {
        // Arrange
        var documents = _documentFaker.Generate(3);
        _context.Documents.AddRange(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetDocumentsByCaseAsync(1);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, d => Assert.Equal(1, d.CaseId));
    }

    [Fact]
    public async Task GetDocumentsByTypeAsync_ShouldReturnDocumentsOfSpecificType()
    {
        // Arrange
        var testCase = _context.Cases.First();

        var documents = new List<Document>
        {
            new Document
            {
                DocumentName = "Motion1.pdf",
                DocumentType = "Motion",
                Case = testCase,
                FilePath = "/docs/motion1.pdf",
                UploadedBy = "John Doe"
            },
            new Document
            {
                DocumentName = "Brief1.pdf",
                DocumentType = "Brief",
                Case = testCase,
                FilePath = "/docs/brief1.pdf",
                UploadedBy = "Jane Smith"
            },
            new Document
            {
                DocumentName = "Motion2.pdf",
                DocumentType = "Motion",
                Case = testCase,
                FilePath = "/docs/motion2.pdf",
                UploadedBy = "Bob Johnson"
            }
        };

        _context.Documents.AddRange(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetDocumentsByTypeAsync("Motion");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, d => Assert.Equal("Motion", d.DocumentType));
    }

    [Fact]
    public async Task SearchDocumentsAsync_ShouldReturnMatchingDocuments()
    {
        // Arrange - Fixed: Create documents that actually contain "Agreement" in their names
        var testCase = _context.Cases.First();

        var documents = new List<Document>
        {
            new Document
            {
                DocumentName = "Service_Agreement.pdf",
                DocumentType = "Contract",
                Case = testCase,
                FilePath = "/docs/service_agreement.pdf",
                UploadedBy = "John Doe"
            },
            new Document
            {
                DocumentName = "Brief1.pdf",
                DocumentType = "Brief",
                Case = testCase,
                FilePath = "/docs/brief1.pdf",
                UploadedBy = "Jane Smith"
            },
            new Document
            {
                DocumentName = "Lease_Agreement.pdf",
                DocumentType = "Contract",
                Case = testCase,
                FilePath = "/docs/lease_agreement.pdf",
                UploadedBy = "Bob Johnson"
            }
        };

        _context.Documents.AddRange(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.SearchDocumentsAsync("Agreement");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, d => Assert.Contains("Agreement", d.DocumentName));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}