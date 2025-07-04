using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.ValueObjects;

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
            return new Attorney //line 37
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
            return new Judge //line 49
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
                EstimatedValue = 50000.00m,
                ClientId = 1,
                ClientFirstName = "John",
                ClientLastName = "Doe",
                ClientEmail = "test@example.com",
                ClientPhone = "555-0101",
                AssignedAttorneyId = 1,
                AttorneyFirstName = "Jane",
                AttorneyLastName = "Smith",
                AttorneyEmail = "attorney@example.com",
                CourtId = 1,
                CourtName = "Test Court",
                CourtAddress = "123 Court St, Court City, Court State, 12345",
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
            // Arrange
            var cases = new List<CaseDTO>
            {
                new CaseDTO
                {
                    CaseId = 1,
                    CaseNumber = "2024-DEADLINE-001",
                    CaseTitle = "Deadline Test Case",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    EstimatedValue = 50000.00m,
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Smith",
                    CourtName = "Test Court",
                    OpenDeadlines = 3
                },
                new CaseDTO
                {
                    CaseId = 2,
                    CaseNumber = "2024-DEADLINE-002",
                    CaseTitle = "Deadline Test Case 2",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    EstimatedValue = 75000.00m,
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Bob",
                    AttorneyLastName = "Wilson",
                    CourtName = "Test Court 2",
                    OpenDeadlines = 1
                }
            };
            _mockCaseService.Setup(s => s.GetAllCasesAsync()).ReturnsAsync(cases);

            // Act
            var result = await _controller.GetAllCases();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDTO>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task GetCase_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var caseId = 1;
            var testCaseDetail = CreateTestCaseDetailDTO(caseId);
            _mockCaseService.Setup(s => s.GetCaseByIdAsync(caseId)).ReturnsAsync(testCaseDetail);

            // Act
            var result = await _controller.GetCase(caseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(okResult.Value);
            Assert.Equal(caseId, returnedCase.CaseId);
        }

        [Fact]
        public async Task GetCase_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var caseId = 999;
            _mockCaseService.Setup(s => s.GetCaseByIdAsync(caseId)).ReturnsAsync((CaseDetailDTO?)null);

            // Act
            var result = await _controller.GetCase(caseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"Case with ID {caseId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCaseByNumber_WithValidNumber_ReturnsOkResult()
        {
            // Arrange
            var caseNumber = "2024-CV-001";
            var testCaseDetail = CreateTestCaseDetailDTO(1, caseNumber);
            _mockCaseService.Setup(s => s.GetCaseByNumberAsync(caseNumber)).ReturnsAsync(testCaseDetail);

            // Act
            var result = await _controller.GetCaseByNumber(caseNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(okResult.Value);
            Assert.Equal(caseNumber, returnedCase.CaseNumber);
        }

        [Fact]
        public async Task GetCaseByNumber_WithInvalidNumber_ReturnsNotFound()
        {
            // Arrange
            var caseNumber = "INVALID-001";
            _mockCaseService.Setup(s => s.GetCaseByNumberAsync(caseNumber)).ReturnsAsync((CaseDetailDTO?)null);

            // Act
            var result = await _controller.GetCaseByNumber(caseNumber);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"Case with number {caseNumber} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCasesByClient_WithValidClientId_ReturnsOkResult()
        {
            // Arrange
            var clientId = 1;
            var caseDTOs = new List<CaseDTO>
            {
                new CaseDTO
                {
                    CaseId = 1,
                    CaseNumber = "2024-CV-001",
                    CaseTitle = "Test Case 1",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Smith",
                    CourtName = "Test Court",
                    OpenDeadlines = 3
                },
                new CaseDTO
                {
                    CaseId = 2,
                    CaseNumber = "2024-CV-002",
                    CaseTitle = "Test Case 2",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Smith",
                    CourtName = "Test Court",
                    OpenDeadlines = 1
                }
            };
            _mockCaseService.Setup(s => s.GetCasesByClientAsync(clientId)).ReturnsAsync(caseDTOs);

            // Act
            var result = await _controller.GetCasesByClient(clientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDTO>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task GetCasesByAttorney_WithValidAttorneyId_ReturnsOkResult()
        {
            // Arrange
            var attorneyId = 1;
            var caseDTOs = new List<CaseDTO>
            {
                new CaseDTO
                {
                    CaseId = 1,
                    CaseNumber = "2024-CV-001",
                    CaseTitle = "Test Case",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Smith",
                    CourtName = "Test Court",
                    OpenDeadlines = 2
                }
            };
            _mockCaseService.Setup(s => s.GetCasesByAttorneyAsync(attorneyId)).ReturnsAsync(caseDTOs);

            // Act
            var result = await _controller.GetCasesByAttorney(attorneyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDTO>>(okResult.Value);
            Assert.Single(returnedCases);
        }

        [Fact]
        public async Task GetActiveCases_ReturnsOkResult_WithActiveCases()
        {
            // Arrange
            var activeCaseDTOs = new List<CaseDTO>
            {
                new CaseDTO
                {
                    CaseId = 1,
                    CaseNumber = "2024-CV-001",
                    CaseTitle = "Test Case 1",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Smith",
                    CourtName = "Test Court",
                    OpenDeadlines = 3
                },
                new CaseDTO
                {
                    CaseId = 2,
                    CaseNumber = "2024-CV-002",
                    CaseTitle = "Test Case 2",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Smith",
                    CourtName = "Test Court",
                    OpenDeadlines = 1
                }
            };
            _mockCaseService.Setup(s => s.GetActiveCasesAsync()).ReturnsAsync(activeCaseDTOs);

            // Act
            var result = await _controller.GetActiveCases();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDTO>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task SearchCases_WithValidSearchTerm_ReturnsOkResult()
        {
            // Arrange
            var searchTerm = "contract";
            var caseDTOs = new List<CaseDTO>
            {
                new CaseDTO
                {
                    CaseId = 1,
                    CaseTitle = "Contract Dispute Case",
                    CaseNumber = "2024-CV-001",
                    CaseType = "Civil",
                    FilingDate = DateTime.Now.AddDays(-30),
                    Status = "Active",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Smith",
                    CourtName = "Test Court",
                    OpenDeadlines = 2
                }
            };
            _mockCaseService.Setup(s => s.SearchCasesAsync(searchTerm)).ReturnsAsync(caseDTOs);

            // Act
            var result = await _controller.SearchCases(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<CaseDTO>>(okResult.Value);
            Assert.Single(returnedCases);
        }

        [Fact]
        public async Task SearchCases_WithEmptySearchTerm_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchCases("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateCase_WithValidCase_ReturnsCreatedAtAction()
        {
            // Arrange
            var client = CreateTestClient();
            var attorney = CreateTestAttorney();
            var judge = CreateTestJudge();
            var court = CreateTestCourt();

            var newCase = new Case
            {
                CaseNumber = "2024-CV-003",
                CaseTitle = "New Case",
                CaseType = "Civil",
                FilingDate = DateTime.Now.AddDays(-30),
                Status = "Active",
                CourtId = court.CourtId
            };

            var createdCaseDetail = CreateTestCaseDetailDTO(3, "2024-CV-003");
            _mockCaseService.Setup(s => s.CreateCaseAsync(newCase)).ReturnsAsync(createdCaseDetail);

            // Act
            var result = await _controller.CreateCase(newCase);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(createdAtActionResult.Value);
            Assert.Equal(createdCaseDetail.CaseId, returnedCase.CaseId);
        }

        [Fact]
        public async Task UpdateCase_WithValidCase_ReturnsOkResult()
        {
            // Arrange
            var caseId = 1;
            var caseToUpdate = new Case
            {
                CaseId = caseId,
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
            _mockCaseService.Setup(s => s.UpdateCaseAsync(caseToUpdate)).ReturnsAsync(updatedCaseDetail);

            // Act
            var result = await _controller.UpdateCase(caseId, caseToUpdate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<CaseDetailDTO>(okResult.Value);
            Assert.Equal(caseId, returnedCase.CaseId);
            Assert.Equal("Updated Case", returnedCase.CaseTitle);
        }


        [Fact]
        public async Task UpdateCase_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var caseId = 1;
            var caseToUpdate = new Case
            {
                CaseId = 2, // Different ID to cause mismatch
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

            // Act
            var result = await _controller.UpdateCase(caseId, caseToUpdate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Case ID mismatch.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteCase_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var caseId = 1;
            _mockCaseService.Setup(s => s.DeleteCaseAsync(caseId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCase(caseId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCase_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var caseId = 99;
            _mockCaseService.Setup(s => s.DeleteCaseAsync(caseId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCase(caseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result); // Remove .Result
            Assert.Equal($"Case with ID {caseId} not found.", notFoundResult.Value);
        }
    }
}