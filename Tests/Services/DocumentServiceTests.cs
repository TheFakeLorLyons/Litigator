using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Bogus;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Document;
using Litigator.Models.Mapping;
using Litigator.Services.Implementations;

public class DocumentServiceTests : IDisposable
{
    private readonly LitigatorDbContext _context;
    private readonly DocumentService _documentService;
    private readonly IMapper _mapper;
    private readonly Faker<DocumentCreateDTO> _documentCreateFaker;
    private readonly Faker<DocumentUpdateDTO> _documentUpdateFaker;

    public DocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<LitigatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LitigatorDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<LitigatorMappingProfile>());

        _mapper = config.CreateMapper();
        _documentService = new DocumentService(_context, _mapper);

        SeedTestData();

        // Setup Bogus faker for DocumentCreateDTO
        _documentCreateFaker = new Faker<DocumentCreateDTO>()
            .RuleFor(d => d.DocumentName, f => f.System.FileName())
            .RuleFor(d => d.DocumentType, f => f.PickRandom("Pleading", "Motion", "Brief", "Evidence", "Contract"))
            .RuleFor(d => d.FilePath, f => f.System.FilePath())
            .RuleFor(d => d.FileSize, f => f.Random.Long(1000, 10000000))
            .RuleFor(d => d.CaseId, f => 1) // Use seeded case
            .RuleFor(d => d.UploadedBy, f => f.Person.FullName);

        // Setup Bogus faker for DocumentUpdateDTO
        _documentUpdateFaker = new Faker<DocumentUpdateDTO>()
            .RuleFor(d => d.DocumentName, f => f.System.FileName())
            .RuleFor(d => d.DocumentType, f => f.PickRandom("Pleading", "Motion", "Brief", "Evidence", "Contract"))
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

        var testCase = new Case
        {
            CaseId = 1,
            CaseNumber = "TEST-001",
            CaseTitle = "Test Case",
            CaseType = "Civil",
            FilingDate = DateTime.Now,
            Status = "Active",
            ClientId = 1,
            AssignedAttorneyId = 1,
            CourtId = 1,
            Client = client
        };

        _context.Cases.Add(testCase);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateDocumentAsync_ShouldCreateDocument_WhenValidDataProvided()
    {
        // Arrange
        var createDto = _documentCreateFaker.Generate();

        // Act
        var result = await _documentService.CreateDocumentAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.DocumentId > 0);
        Assert.Equal(createDto.DocumentName, result.DocumentName);
        Assert.Equal(createDto.DocumentType, result.DocumentType);
        Assert.Equal(createDto.FilePath, result.FilePath);
        Assert.Equal(createDto.FileSize, result.FileSize);
        Assert.Equal(createDto.UploadedBy, result.UploadedBy);
        Assert.Equal(createDto.CaseId, result.CaseId);
        Assert.Equal("TEST-001", result.CaseNumber);
        Assert.Equal("Test Case", result.CaseTitle);
        Assert.True(result.UploadDate <= DateTime.Now);
    }

    [Fact]
    public async Task GetAllDocumentsAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var documents = new List<Document>
        {
            new Document
            {
                DocumentName = "AllDocs1.pdf",
                DocumentType = "Brief",
                FilePath = "/docs/all1.pdf",
                FileSize = 1500,
                Case = testCase,
                CaseId = testCase.CaseId,
                UploadedBy = "Attorney 1",
                UploadDate = DateTime.Now.AddDays(-2)
            },
            new Document
            {
                DocumentName = "AllDocs2.pdf",
                DocumentType = "Evidence",
                FilePath = "/docs/all2.pdf",
                FileSize = 3000,
                Case = testCase,
                CaseId = testCase.CaseId,
                UploadedBy = "Attorney 2",
                UploadDate = DateTime.Now.AddDays(-1)
            }
        };

        _context.Documents.AddRange(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetAllDocumentsAsync();

        // Assert
        Assert.Equal(2, result.Count());

        var briefDoc = result.First(d => d.DocumentType == "Brief");
        Assert.Equal("AllDocs1.pdf", briefDoc.DocumentName);
        Assert.Equal(1500, briefDoc.FileSize);
        Assert.Equal("Attorney 1", briefDoc.UploadedBy);

        var evidenceDoc = result.First(d => d.DocumentType == "Evidence");
        Assert.Equal("AllDocs2.pdf", evidenceDoc.DocumentName);
        Assert.Equal(3000, evidenceDoc.FileSize);
        Assert.Equal("Attorney 2", evidenceDoc.UploadedBy);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldReturnDocument_WhenExists()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var document = new Document
        {
            DocumentName = "GetById.pdf",
            DocumentType = "Contract",
            FilePath = "/docs/getbyid.pdf",
            FileSize = 4096,
            Case = testCase,
            CaseId = testCase.CaseId,
            UploadedBy = "Test User",
            UploadDate = DateTime.Now
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetDocumentByIdAsync(document.DocumentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(document.DocumentId, result.DocumentId);
        Assert.Equal("GetById.pdf", result.DocumentName);
        Assert.Equal("Contract", result.DocumentType);
        Assert.Equal("/docs/getbyid.pdf", result.FilePath);
        Assert.Equal(4096, result.FileSize);
        Assert.Equal("Test User", result.UploadedBy);
        Assert.Equal(testCase.CaseId, result.CaseId);
        Assert.Equal(testCase.CaseNumber, result.CaseNumber);
        Assert.Equal(testCase.CaseTitle, result.CaseTitle);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _documentService.GetDocumentByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDocumentsByCaseAsync_ShouldReturnDocumentsForSpecificCase()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var documents = new List<Document>
        {
            new Document
            {
                DocumentName = "Document1.pdf",
                DocumentType = "Pleading",
                FilePath = "/docs/doc1.pdf",
                FileSize = 1024,
                Case = testCase,
                CaseId = testCase.CaseId,
                UploadedBy = "John Doe",
                UploadDate = DateTime.Now.AddDays(-1)
            },
            new Document
            {
                DocumentName = "Document2.pdf",
                DocumentType = "Motion",
                FilePath = "/docs/doc2.pdf",
                FileSize = 2048,
                Case = testCase,
                CaseId = testCase.CaseId,
                UploadedBy = "Jane Smith",
                UploadDate = DateTime.Now
            }
        };

        _context.Documents.AddRange(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetDocumentsByCaseAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, d => Assert.Equal(1, d.CaseId));
        Assert.All(result, d => Assert.Equal("TEST-001", d.CaseNumber));
        Assert.All(result, d => Assert.Equal("Test Case", d.CaseTitle));

        // Verify ordering (most recent first)
        var orderedResult = result.ToList();
        Assert.True(orderedResult[0].UploadDate >= orderedResult[1].UploadDate);
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
    public async Task SearchDocumentsAsync_FindsMatchingDocuments()
    {
        // Arrange
        var testCase = _context.Cases.First();

        var document1 = new Document
        {
            DocumentName = "Contract Agreement",
            DocumentType = "PDF",
            FilePath = "/contract.pdf",
            FileSize = 1024,
            UploadDate = DateTime.Now,
            Case = testCase,
            CaseId = 1,
            UploadedBy = "John Doe"
        };

        var document2 = new Document
        {
            DocumentName = "Settlement Letter",
            DocumentType = "PDF",
            FilePath = "/settlement.pdf",
            FileSize = 512,
            UploadDate = DateTime.Now,
            Case = testCase,
            CaseId = 1,
            UploadedBy = "John Doe"
        };

        _context.Documents.AddRange(document1, document2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.SearchDocumentsAsync("contract");

        // Assert
        Assert.Single(result);
        Assert.Equal("Contract Agreement", result.First().DocumentName);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ShouldUpdateDocument_WhenValidDataProvided()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var document = new Document
        {
            DocumentName = "Original.pdf",
            DocumentType = "Original Type",
            FilePath = "/docs/original.pdf",
            FileSize = 1024,
            Case = testCase,
            CaseId = testCase.CaseId,
            UploadedBy = "Original User",
            UploadDate = DateTime.Now.AddDays(-1)
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        var updateDto = new DocumentUpdateDTO
        {
            DocumentName = "Updated.pdf",
            DocumentType = "Updated Type",
            UploadedBy = "Updated User"
        };

        // Act
        var result = await _documentService.UpdateDocumentAsync(document.DocumentId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated.pdf", result.DocumentName);
        Assert.Equal("Updated Type", result.DocumentType);
        Assert.Equal("Updated User", result.UploadedBy);
        // FilePath and FileSize should remain unchanged
        Assert.Equal("/docs/original.pdf", result.FilePath);
        Assert.Equal(1024, result.FileSize);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ShouldReturnNull_WhenDocumentNotExists()
    {
        // Arrange
        var updateDto = _documentUpdateFaker.Generate();

        // Act
        var result = await _documentService.UpdateDocumentAsync(999, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnTrue_WhenDocumentExists()
    {
        // Arrange
        var testCase = _context.Cases.First();
        var document = new Document
        {
            DocumentName = "ToDelete.pdf",
            DocumentType = "Test",
            FilePath = "/docs/todelete.pdf",
            FileSize = 512,
            Case = testCase,
            CaseId = testCase.CaseId,
            UploadedBy = "Test User",
            UploadDate = DateTime.Now
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.DeleteDocumentAsync(document.DocumentId);

        // Assert
        Assert.True(result);

        // Verify it's actually deleted
        var deletedDocument = await _context.Documents.FindAsync(document.DocumentId);
        Assert.Null(deletedDocument);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnFalse_WhenDocumentNotExists()
    {
        // Act
        var result = await _documentService.DeleteDocumentAsync(999);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}