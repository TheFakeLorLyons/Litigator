using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.Controllers;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Case;
using Litigator.Services.Interfaces;

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

        [Fact]
        public async Task GetAllCases_ReturnsOkResult_WithListOfCases()
        {
            // Arrange
            var client = new Client
            {
                ClientId = 1,
                ClientName = "Deadline Test Client",
                Address = "456 Test Ave",
                Phone = "(555) 987-6543",
                Email = "deadlineclient@test.com"
            };

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
                    ClientName = "Deadline Test Client",
                    AttorneyFirstName = "John",
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
                    ClientName = "Deadline Test Client 2",
                    AttorneyFirstName = "Jane",
                    AttorneyLastName = "Doe",
                    CourtName = "Test Court 2",
                    OpenDeadlines = 1
                }
            };
            _mockCaseService.Setup(s => s.GetAllCasesAsync()).ReturnsAsync(cases);

            // Act
            var result = await _controller.GetAllCases();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<Case>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task GetCase_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var caseId = 1;
            var client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" };
            var testCase = new Case
            {
                CaseId = caseId,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = client
            };
            _mockCaseService.Setup(s => s.GetCaseByIdAsync(caseId)).ReturnsAsync(testCase);

            // Act
            var result = await _controller.GetCase(caseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<Case>(okResult.Value);
            Assert.Equal(caseId, returnedCase.CaseId);
        }

        [Fact]
        public async Task GetCaseByNumber_WithValidNumber_ReturnsOkResult()
        {
            // Arrange
            var caseNumber = "2024-CV-001";
            var client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" };
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = caseNumber,
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = client
            };
            _mockCaseService.Setup(s => s.GetCaseByNumberAsync(caseNumber)).ReturnsAsync(testCase);

            // Act
            var result = await _controller.GetCaseByNumber(caseNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<Case>(okResult.Value);
            Assert.Equal(caseNumber, returnedCase.CaseNumber);
        }

        [Fact]
        public async Task GetCasesByClient_WithValidClientId_ReturnsOkResult()
        {
            // Arrange
            var clientId = 1;

            // Create a client object to use for both cases
            var testClient = new Client
            {
                ClientId = clientId,
                ClientName = "Test Client",
                Address = "123 Test St",
                Phone = "555-0101",
                Email = "test@example.com"
            };

            var cases = new List<Case>
            {
                new Case
                {
                    CaseId = 1,
                    ClientId = clientId,
                    CaseNumber = "2024-CV-001",
                    CaseTitle = "Test Case 1",
                    CaseType = "Civil",
                    Client = testClient
                },
                new Case
                {
                    CaseId = 2,
                    ClientId = clientId,
                    CaseNumber = "2024-CV-002",
                    CaseTitle = "Test Case 2",
                    CaseType = "Civil",
                    Client = testClient
                }
            };
            _mockCaseService.Setup(s => s.GetCasesByClientAsync(clientId)).ReturnsAsync(cases);

            // Act
            var result = await _controller.GetCasesByClient(clientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<Case>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
            Assert.All(returnedCases, c => Assert.Equal(clientId, c.ClientId));
        }

        [Fact]
        public async Task GetCasesByAttorney_WithValidAttorneyId_ReturnsOkResult()
        {
            // Arrange
            var attorneyId = 1;
            var client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" };
            var cases = new List<Case>
            {
                new Case
                {
                    CaseId = 1,
                    AssignedAttorneyId = attorneyId,
                    CaseNumber = "2024-CV-001",
                    CaseTitle = "Test Case",
                    CaseType = "Civil",
                    Client = client
                }
            };
            _mockCaseService.Setup(s => s.GetCasesByAttorneyAsync(attorneyId)).ReturnsAsync(cases);

            // Act
            var result = await _controller.GetCasesByAttorney(attorneyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<Case>>(okResult.Value);
            Assert.Single(returnedCases);
        }

        [Fact]
        public async Task GetActiveCases_ReturnsOkResult_WithActiveCases()
        {
            // Arrange
            // Fixed: Define testClient variable that was missing
            var testClient = new Client
            {
                ClientId = 1,
                ClientName = "Test Client",
                Address = "123 Test St",
                Phone = "555-0101",
                Email = "test@example.com"
            };

            var activeCases = new List<Case>
            {
                new Case
                {
                    CaseId = 1,
                    ClientId = 1,
                    CaseNumber = "2024-CV-001",
                    CaseTitle = "Test Case 1",
                    CaseType = "Civil",
                    Client = testClient
                },
                new Case
                {
                    CaseId = 2,
                    ClientId = 1, // Fixed: Use consistent ClientId
                    CaseNumber = "2024-CV-002",
                    CaseTitle = "Test Case 2",
                    CaseType = "Civil",
                    Client = testClient
                }
            };
            _mockCaseService.Setup(s => s.GetActiveCasesAsync()).ReturnsAsync(activeCases);

            // Act
            var result = await _controller.GetActiveCases();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<Case>>(okResult.Value);
            Assert.Equal(2, returnedCases.Count());
        }

        [Fact]
        public async Task SearchCases_WithValidSearchTerm_ReturnsOkResult()
        {
            // Arrange
            var searchTerm = "contract";
            var client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" };
            var cases = new List<Case>
            {
                new Case
                {
                    CaseId = 1,
                    CaseTitle = "Contract Dispute Case",
                    CaseNumber = "2024-CV-001",
                    CaseType = "Civil",
                    Client = client
                }
            };
            _mockCaseService.Setup(s => s.SearchCasesAsync(searchTerm)).ReturnsAsync(cases);

            // Act
            var result = await _controller.SearchCases(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCases = Assert.IsAssignableFrom<IEnumerable<Case>>(okResult.Value);
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
            var client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" };
            var newCase = new Case
            {
                CaseNumber = "2024-CV-003",
                CaseTitle = "New Case",
                CaseType = "Civil",
                Client = client
            };
            var createdCase = new Case
            {
                CaseId = 3,
                CaseNumber = "2024-CV-003",
                CaseTitle = "New Case",
                CaseType = "Civil",
                Client = client
            };
            _mockCaseService.Setup(s => s.CreateCaseAsync(newCase)).ReturnsAsync(createdCase);

            // Act
            var result = await _controller.CreateCase(newCase);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedCase = Assert.IsType<Case>(createdAtActionResult.Value);
            Assert.Equal(createdCase.CaseId, returnedCase.CaseId);
        }

        [Fact]
        public async Task UpdateCase_WithValidCase_ReturnsOkResult()
        {
            // Arrange
            var caseId = 1;
            var client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" };
            var caseToUpdate = new Case
            {
                CaseId = caseId,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Updated Case",
                CaseType = "Civil",
                Client = client
            };
            _mockCaseService.Setup(s => s.UpdateCaseAsync(caseToUpdate)).ReturnsAsync(caseToUpdate);

            // Act
            var result = await _controller.UpdateCase(caseId, caseToUpdate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCase = Assert.IsType<Case>(okResult.Value);
            Assert.Equal(caseId, returnedCase.CaseId);
        }

        [Fact]
        public async Task UpdateCase_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var caseId = 1;
            var client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" };
            var caseToUpdate = new Case
            {
                CaseId = 2,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Updated Case",
                CaseType = "Civil",
                Client = client
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
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Case with ID {caseId} not found.", notFoundResult.Value);
        }
    }
}