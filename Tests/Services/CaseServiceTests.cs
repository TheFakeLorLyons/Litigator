using AutoMapper;
using Bogus;
using Xunit;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Enums;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.Mapping;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Tests.Services
{
    public class CaseServiceTests : IDisposable
    {
        private readonly LitigatorDbContext _context;
        private readonly CaseService _caseService;
        private readonly IMapper _mapper;
        private readonly Client _testClient;
        private readonly Attorney _testAttorney;
        private readonly Court _testCourt;

        public CaseServiceTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<LitigatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LitigatorDbContext(options);

            var config = new MapperConfiguration(cfg => cfg.AddProfile<LitigatorMappingProfile>());
            _mapper = config.CreateMapper();
            _caseService = new CaseService(_context, _mapper);

            // Test entities that will be reused
            _testAttorney = new Attorney
            {
                SystemId = 1,
                Name = PersonName.Create("Jane", "Smith"),
                BarNumber = "12345",
                Email = "john.doe@caselaw.com",
                PrimaryPhone = PhoneNumber.Create("617-232-1234"),
                PrimaryAddress = Address.Create("456 Lawyer Blvd", null, "Law City", "Test County", "NY", "10001"),
                Specialization = Litigator.DataAccess.Enums.LegalSpecialization.GeneralPractice,
                IsActive = true
            };

            _testClient = new Client
            {
                SystemId = 2,
                Name = PersonName.Create("Jane", "Smith"),
                PrimaryAddress = Address.Create("123 Fake Street", null, "Test City", "Test County", "NY", "12345"),
                PrimaryPhone = PhoneNumber.Create("(555) 123-4567"),
                Email = "client@test.com",
                IsActive = true
            };

            _testCourt = new Court
            {
                CourtId = 1,
                CourtName = "Case Test Court",
                Address = Address.Create("123 Fake Ave", null, "Test City", "Test County", "NY", "12345"),
                CourtType = "State",
                Phone = PhoneNumber.Create("(123) 456-7890"),
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml"
            };

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Attorneys.Add(_testAttorney);
            _context.Clients.Add(_testClient);
            _context.Courts.Add(_testCourt);
            _context.SaveChanges();

            var newCase = new Case
            {
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test v. Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1, // You'll need to create a test judge or use a valid ID
                CourtId = _testCourt.CourtId
            };

            _context.Cases.Add(newCase);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateCaseAsync_ShouldCreateCase_WhenValidDataProvided()
        {
            // Arrange
            var newCaseDto = new CaseCreateDTO
            {
                CaseNumber = "2024-CV-NEW",
                CaseTitle = "New Test v. Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1, // You'll need to create a test judge or use a valid ID
                CourtId = _testCourt.CourtId
            };

            // Act
            var result = await _caseService.CreateCaseAsync(newCaseDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CaseId > 0);
            Assert.Equal("2024-CV-NEW", result.CaseNumber);

            var caseInDb = await _context.Cases.FindAsync(result.CaseId);
            Assert.NotNull(caseInDb);
        }

        [Fact]
        public async Task CreateCaseAsync_ShouldThrowException_WhenDuplicateCaseNumber()
        {
            // Arrange
            var duplicateCaseDto = new CaseCreateDTO
            {
                CaseNumber = "2024-CV-001", // This already exists from seed data
                CaseTitle = "Duplicate Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _caseService.CreateCaseAsync(duplicateCaseDto));

            Assert.Contains("Case number 2024-CV-001 already exists", exception.Message);
        }

        [Fact]
        public async Task GetCaseByIdAsync_ShouldReturnCase_WhenCaseExists()
        {
            // Arrange
            var testCase = new Case
            {
                CaseNumber = "2024-CV-002",
                CaseTitle = "Another Test Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            _context.Cases.Add(testCase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.GetCaseByIdAsync(testCase.CaseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testCase.CaseId, result.CaseId);
            Assert.Equal("2024-CV-002", result.CaseNumber);
            Assert.Equal("Jane", result.ClientFirstName);
            Assert.Equal("Smith", result.ClientLastName);
            Assert.Equal("Jane", result.AttorneyFirstName);
            Assert.Equal("Smith", result.AttorneyLastName);
            Assert.Equal("Case Test Court", result.CourtName);
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
            var case1 = new Case
            {
                CaseNumber = "2024-CV-003",
                CaseTitle = "Attorney Test 1",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            var case2 = new Case
            {
                CaseNumber = "2024-CV-004",
                CaseTitle = "Attorney Test 2",
                CaseType = "Criminal",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            _context.Cases.AddRange(case1, case2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.GetCasesByAttorneyAsync(_testAttorney.SystemId);

            // Assert
            var caseList = result.ToList();
            Assert.True(caseList.Count >= 2); // At least 2 (plus the seed data case)
            Assert.All(caseList, c => Assert.Equal("Jane", c.AttorneyFirstName));
            Assert.All(caseList, c => Assert.Equal("Smith", c.AttorneyLastName));
        }

        [Fact]
        public async Task GetCasesByClientAsync_ShouldReturnCorrectCases()
        {
            // Arrange
            var case1 = new Case
            {
                CaseNumber = "2024-CV-005",
                CaseTitle = "Client Test 1",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            _context.Cases.Add(case1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.GetCasesByClientAsync(_testClient.SystemId);

            // Assert
            var caseList = result.ToList();
            Assert.True(caseList.Count >= 1);
            Assert.All(caseList, c => Assert.Equal("Jane", c.ClientFirstName));
            Assert.All(caseList, c => Assert.Equal("Smith", c.ClientLastName));
        }

        [Fact]
        public async Task UpdateCaseAsync_ShouldUpdateCase_WhenValidDataProvided()
        {
            // Arrange
            var originalCase = new Case
            {
                CaseNumber = "2024-CV-UPDATE",
                CaseTitle = "Original Title",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            _context.Cases.Add(originalCase);
            await _context.SaveChangesAsync();

            // Create the update DTO
            var updateDto = new CaseUpdateDTO
            {
                CaseNumber = "2024-CV-UPDATE",
                CaseTitle = "Updated Title",
                CaseType = "Civil",
                FilingDate = originalCase.FilingDate,
                Status = "Closed",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            // Act
            var result = await _caseService.UpdateCaseAsync(originalCase.CaseId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.CaseTitle);
            Assert.Equal("Closed", result.Status);
        }

        [Fact]
        public async Task DeleteCaseAsync_ShouldDeleteCase_WhenCaseExists()
        {
            // Arrange
            var testCase = new Case
            {
                CaseNumber = "2024-CV-DELETE",
                CaseTitle = "Case to Delete",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
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

        [Fact]
        public async Task SearchCasesAsync_ShouldReturnMatchingCases()
        {
            // Arrange
            var searchableCase = new Case
            {
                CaseNumber = "2024-CV-SEARCH",
                CaseTitle = "Searchable Test Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            _context.Cases.Add(searchableCase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.SearchCasesAsync("Searchable");

            // Assert
            var caseList = result.ToList();
            Assert.True(caseList.Count >= 1);
            Assert.Contains(caseList, c => c.CaseTitle.Contains("Searchable"));
        }

        [Fact]
        public async Task GetActiveCasesAsync_ShouldReturnOnlyActiveCases()
        {
            // Arrange
            var activeCase = new Case
            {
                CaseNumber = "2024-CV-ACTIVE",
                CaseTitle = "Active Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            var closedCase = new Case
            {
                CaseNumber = "2024-CV-CLOSED",
                CaseTitle = "Closed Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Closed",
                ClientId = _testClient.SystemId,
                AssignedAttorneyId = _testAttorney.SystemId,
                AssignedJudgeId = 1,
                CourtId = _testCourt.CourtId
            };

            _context.Cases.AddRange(activeCase, closedCase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _caseService.GetActiveCasesAsync();

            // Assert
            var caseList = result.ToList();
            Assert.All(caseList, c => Assert.Equal("Active", c.Status));
            Assert.DoesNotContain(caseList, c => c.Status == "Closed");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}