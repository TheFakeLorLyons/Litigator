using Microsoft.EntityFrameworkCore;
using Bogus;
using Xunit;
using System.Threading.Tasks;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Services.Implementations;

namespace Tests.Services  
{
    public class CaseServiceTests : IDisposable
    {
        private readonly LitigatorDbContext _context;
        private readonly CaseService _caseService;

        public CaseServiceTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<LitigatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LitigatorDbContext(options);
            _caseService = new CaseService(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var attorney = new Attorney
            {
                AttorneyId = 1,
                FirstName = "John",
                LastName = "Doe",
                BarNumber = "12345",
                Email = "john.doe@caselaw.com",
                Phone = "617-232-1234"
            };
            var client = new Client
            {
                ClientId = 1,
                ClientName = "Case Test Client",
                Address = "123 Test St",
                Phone = "(555) 123-4567",
                Email = "client@test.com"
            };

            var court = new Court
            {
                CourtId = 1,
                CourtName = "Case Test Court",
                Address = "123 Fake Street",
                County = "Test County",
                State = "NY",
                CourtType = "State"
            };

            _context.Attorneys.Add(attorney);
            _context.Clients.Add(client);
            _context.Courts.Add(court);
            _context.SaveChanges();
        
            var newCase = new Case
            {
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test v. Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Client = client,
                AssignedAttorneyId = 1,
                CourtId = 1
            };

            _context.Cases.Add(newCase);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateCaseAsync_ShouldCreateCase_WhenValidDataProvided()
        {
            // Arrange
            var client = new Client
            {
                ClientId = 1,
                ClientName = "Case Test Client",
                Address = "123 Test St",
                Phone = "(555) 123-4567",
                Email = "client@test.com"
            };

            var newCase = new Case
            {
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test v. Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Client = client,
                AssignedAttorneyId = 1,
                CourtId = 1
            };

            // Act
            var result = await _caseService.CreateCaseAsync(newCase);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CaseId > 0);
            Assert.Equal("2024-CV-001", result.CaseNumber);

            var caseInDb = await _context.Cases.FindAsync(result.CaseId);
            Assert.NotNull(caseInDb);
        }

        [Fact]
        public async Task GetCaseByIdAsync_ShouldReturnCase_WhenCaseExists()
        {
            // Arrange
            var client = new Client
            {
                ClientId = 1,
                ClientName = "Case Test Client",
                Address = "123 Test St",
                Phone = "(555) 123-4567",
                Email = "client@test.com"
            };

            var testCase = new Case
            {
                CaseNumber = "2024-CV-002",
                CaseTitle = "Another Test Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Client = client,
                AssignedAttorneyId = 1,
                CourtId = 1
            };

            _context.Cases.Add(testCase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.GetCaseByIdAsync(testCase.CaseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testCase.CaseId, result.CaseId);
            Assert.Equal("2024-CV-002", result.CaseNumber);
            Assert.NotNull(result.Client);
            Assert.NotNull(result.AssignedAttorney);
            Assert.NotNull(result.Court);
        }

        [Fact]
        public async Task GetCaseByIdAsync_ShouldReturnNull_WhenCaseDoesNotExist()
        {
            // Act
            var result = await _caseService.GetCaseByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCasesByAttorneyAsync_ShouldReturnCorrectCases()
        {
            // Arrange
            var client = new Client
            {
                ClientId = 1,
                ClientName = "Case Test Client",
                Address = "123 Test St",
                Phone = "(555) 123-4567",
                Email = "client@test.com"
            };

            var case1 = new Case
            {
                CaseNumber = "2024-CV-003",
                CaseTitle = "Attorney Test 1",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Client = client,
                AssignedAttorneyId = 1,
                CourtId = 1
            };

            var case2 = new Case
            {
                CaseNumber = "2024-CV-004",
                CaseTitle = "Attorney Test 2",
                CaseType = "Criminal",
                FilingDate = DateTime.Now,
                Client = client,
                AssignedAttorneyId = 1,
                CourtId = 1
            };

            _context.Cases.AddRange(case1, case2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.GetCasesByAttorneyAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.Equal(1, c.AssignedAttorneyId));
        }

        [Fact]
        public async Task DeleteCaseAsync_ShouldDeleteCase_WhenCaseExists()
        {
            // Arrange
            var testClient = _context.Clients.First();

            var testCase = new Case
            {
                CaseNumber = "2024-CV-DELETE",
                CaseTitle = "Case to Delete",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Client = testClient,
                AssignedAttorneyId = 1,
                CourtId = 1
            };

            _context.Cases.Add(testCase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.DeleteCaseAsync(testCase.CaseId);

            // Assert
            Assert.True(result);
            var deletedCase = await _context.Cases.FindAsync(testCase.CaseId);
            Assert.Null(deletedCase);
        }

        [Fact]
        public async Task DeleteCaseAsync_ShouldReturnFalse_WhenCaseDoesNotExist()
        {
            // Act
            var result = await _caseService.DeleteCaseAsync(999);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}