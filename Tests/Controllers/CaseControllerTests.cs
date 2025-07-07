using Litigator.Controllers;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Litigator.Tests.Controllers
{
    public class CaseControllerTests
    {
        private readonly Mock<ICaseService> _mockCaseService;
        private readonly CaseController _controller;

        public CaseControllerTests()
        {
            _mockCaseService = new Mock<ICaseService>();
            _controller = new CaseController(_mockCaseService.Object);
        }

        private Client CreateTestClient(int clientId = 1, string firstName = "John", string lastName = "Doe")
        {
            return new Client
            {
                SystemId = clientId,
                Name = PersonName.Create(firstName, lastName),
                Email = "test@example.com",
                PrimaryPhone = PhoneNumber.Create("555-0101"),
                PrimaryAddress = Address.Create("123 Test St", "Test City", "Test State", "12345"),
                CreatedDate = DateTime.UtcNow
            };
        }

        private Attorney CreateTestAttorney(int attorneyId = 1, string firstName = "Jane", string lastName = "Smith")
        {
            return new Attorney
            {
                SystemId = attorneyId,
                Name = PersonName.Create(firstName, lastName),
                Email = "attorney@example.com",
                BarNumber = "BAR12345",
                PrimaryAddress = Address.Create("456 Lawyer St", "Law City", "NY", "10001"),
                PrimaryPhone = PhoneNumber.Create("555-222-3333"),
                CreatedDate = DateTime.UtcNow
            };
        }

        private Judge CreateTestJudge(int judgeId = 1, string firstName = "Robert", string lastName = "Johnson")
        {
            return new Judge
            {
                SystemId = judgeId,
                Name = PersonName.Create(firstName, lastName),
                Email = "judge@example.com",
                BarNumber = "JUDGE12345",
                PrimaryAddress = Address.Create("789 Judge Blvd", "Justice City", "NY", "10002"),
                PrimaryPhone = PhoneNumber.Create("555-444-5555"),
                CreatedDate = DateTime.UtcNow
            };
        }

        private Court CreateTestCourt(int courtId = 1, string name = "Test Court")
        {
            return new Court
            {
                CourtId = courtId,
                CourtName = name,
                Address = Address.Create("123 Court St", "Court City", "Court State", "12345"),
                Phone = new PhoneNumber { Number = "718-618-2098" },
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CreatedDate = DateTime.UtcNow
            };
        }

        private CaseDetailDTO CreateTestCaseDetailDTO(int caseId = 1, string caseNumber = "2024-CV-001")
        {
            return new CaseDetailDTO
            {
                CaseId = caseId,
                CaseNumber = caseNumber,
                CaseTitle = "Test Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now.AddDays(-30),
                Status = "Active",
                EstimatedValue = 50000,
                ClientId = 1,
                ClientFirstName = "John",
                ClientLastName = "Doe",
                ClientEmail = "john.doe@example.com",
                ClientPhone = "555-0101",
                AssignedAttorneyId = 1,
                AttorneyFirstName = "Jane",
                AttorneyLastName = "Smith",
                AttorneyEmail = "jane.smith@example.com",
                CourtId = 1,
                CourtName = "Test Court",
                CourtAddress = "123 Court St, Court City, Court State 12345",
                TotalDeadlines = 5,
                OpenDeadlines = 3,
                OverdueDeadlines = 1,
                TotalDocuments = 10,
                NextDeadlineDate = DateTime.Now.AddDays(7),
                NextDeadlineDescription = "File motion"
            };
        }

        [Fact]
        public async Task GetAllCases_ReturnsOkResult_WithListOfCases()
        {
            var cases = new List<CaseDetailDTO>
            {
                CreateTestCaseDetailDTO(1, "2024-DEADLINE-001"),
                CreateTestCaseDetailDTO(2, "2024-DEADLINE-002")
            };
            _mockCaseService.Setup(s => s.GetAllCasesAsync()).ReturnsAsync(cases);

            var result = await _controller.GetAllCases();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDetailDTO>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task GetCase_WithValidId_ReturnsOkResult()
        {
            var caseId = 1;
            var testCaseDetail = CreateTestCaseDetailDTO(caseId);
            _mockCaseService.Setup(s => s.GetCaseByIdAsync(caseId)).ReturnsAsync(testCaseDetail);

            var result = await _controller.GetCase(caseId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(okResult.Value);
            Assert.Equal(caseId, returnedCase.CaseId);
        }

        [Fact]
        public async Task GetCase_WithInvalidId_ReturnsNotFound()
        {
            var caseId = 999;
            _mockCaseService.Setup(s => s.GetCaseByIdAsync(caseId)).ReturnsAsync((CaseDetailDTO?)null);

            var result = await _controller.GetCase(caseId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"Case with ID {caseId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCaseByNumber_WithValidNumber_ReturnsOkResult()
        {
            var caseNumber = "2024-CV-001";
            var testCaseDetail = CreateTestCaseDetailDTO(1, caseNumber);
            _mockCaseService.Setup(s => s.GetCaseByNumberAsync(caseNumber)).ReturnsAsync(testCaseDetail);

            var result = await _controller.GetCaseByNumber(caseNumber);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(okResult.Value);
            Assert.Equal(caseNumber, returnedCase.CaseNumber);
        }

        [Fact]
        public async Task GetCaseByNumber_WithInvalidNumber_ReturnsNotFound()
        {
            var caseNumber = "INVALID-001";
            _mockCaseService.Setup(s => s.GetCaseByNumberAsync(caseNumber)).ReturnsAsync((CaseDetailDTO?)null);

            var result = await _controller.GetCaseByNumber(caseNumber);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"Case with number {caseNumber} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCasesByClient_WithValidClientId_ReturnsOkResult()
        {
            var clientId = 1;
            var caseDetailDTOs = new List<CaseDetailDTO>
            {
                CreateTestCaseDetailDTO(1, "2024-CV-001"),
                CreateTestCaseDetailDTO(2, "2024-CV-002")
            };
            _mockCaseService.Setup(s => s.GetCasesByClientAsync(clientId)).ReturnsAsync(caseDetailDTOs);

            var result = await _controller.GetCasesByClient(clientId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDetailDTO>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task GetCasesByAttorney_WithValidAttorneyId_ReturnsOkResult()
        {
            var attorneyId = 1;
            var caseDetailDTOs = new List<CaseDetailDTO>
            {
                CreateTestCaseDetailDTO(1, "2024-CV-001")
            };
            _mockCaseService.Setup(s => s.GetCasesByAttorneyAsync(attorneyId)).ReturnsAsync(caseDetailDTOs);

            var result = await _controller.GetCasesByAttorney(attorneyId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDetailDTO>>(okResult.Value);
            Assert.Single(returnedCases);
        }

        [Fact]
        public async Task GetActiveCases_ReturnsOkResult_WithActiveCases()
        {
            var activeCaseDetailDTOs = new List<CaseDetailDTO>
            {
                CreateTestCaseDetailDTO(1, "2024-CV-001"),
                CreateTestCaseDetailDTO(2, "2024-CV-002")
            };
            _mockCaseService.Setup(s => s.GetActiveCasesAsync()).ReturnsAsync(activeCaseDetailDTOs);

            var result = await _controller.GetActiveCases();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDetailDTO>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task SearchCases_WithValidSearchTerm_ReturnsOkResult()
        {
            var searchTerm = "contract";
            var caseDetailDTO = CreateTestCaseDetailDTO(1, "2024-CV-001");
            caseDetailDTO.CaseTitle = "Contract Dispute Case";
            var caseDetailDTOs = new List<CaseDetailDTO> { caseDetailDTO };
            _mockCaseService.Setup(s => s.SearchCasesAsync(searchTerm)).ReturnsAsync(caseDetailDTOs);

            var result = await _controller.SearchCases(searchTerm);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDetailDTO>>(okResult.Value);
            Assert.Single(returnedCases);
        }

        [Fact]
        public async Task SearchCases_WithEmptySearchTerm_ReturnsBadRequest()
        {
            var result = await _controller.SearchCases("");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateCase_WithValidCase_ReturnsCreatedAtAction()
        {
            var caseCreateDto = new CaseCreateDTO
            {
                CaseNumber = "2024-CV-003",
                CaseTitle = "New Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now.AddDays(-30),
                Status = "Active",
                ClientId = 1,
                AssignedAttorneyId = 1,
                AssignedJudgeId = 1,
                CourtId = 1
            };

            var createdCaseDetail = CreateTestCaseDetailDTO(3, "2024-CV-003");
            _mockCaseService.Setup(s => s.CreateCaseAsync(caseCreateDto)).ReturnsAsync(createdCaseDetail);

            var result = await _controller.CreateCase(caseCreateDto);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(createdAtActionResult.Value);
            Assert.Equal(createdCaseDetail.CaseId, returnedCase.CaseId);
        }

        [Fact]
        public async Task UpdateCase_WithValidCase_ReturnsOkResult()
        {
            var caseId = 1;
            var caseUpdateDto = new CaseUpdateDTO
            {
                CaseNumber = "2024-CV-001",
                CaseTitle = "Updated Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now.AddDays(-30),
                Status = "Active",
                ClientId = 1,
                AssignedAttorneyId = 1,
                AssignedJudgeId = 1,
                CourtId = 1
            };

            var updatedCaseDetail = CreateTestCaseDetailDTO(caseId);
            updatedCaseDetail.CaseTitle = "Updated Case";
            _mockCaseService.Setup(s => s.UpdateCaseAsync(caseId, caseUpdateDto)).ReturnsAsync(updatedCaseDetail);

            var result = await _controller.UpdateCase(caseId, caseUpdateDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(okResult.Value);
            Assert.Equal(caseId, returnedCase.CaseId);
            Assert.Equal("Updated Case", returnedCase.CaseTitle);
        }

        [Fact]
        public async Task DeleteCase_WithValidId_ReturnsNoContent()
        {
            var caseId = 1;
            _mockCaseService.Setup(s => s.DeleteCaseAsync(caseId)).ReturnsAsync(true);

            var result = await _controller.DeleteCase(caseId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCase_WithInvalidId_ReturnsNotFound()
        {
            var caseId = 99;
            _mockCaseService.Setup(s => s.DeleteCaseAsync(caseId)).ReturnsAsync(false);

            var result = await _controller.DeleteCase(caseId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Case with ID {caseId} not found.", notFoundResult.Value);
        }
    }
}