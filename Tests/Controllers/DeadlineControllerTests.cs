using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.DataAccess.Entities;
using Litigator.Services.Interfaces;

namespace Litigator.Tests.Controllers
{
    public class DeadlineControllerTests
    {
        private readonly Mock<IDeadlineService> _mockDeadlineService;
        private readonly Mock<ILogger<DeadlineController>> _mockLogger;
        private readonly DeadlineController _controller;

        public DeadlineControllerTests()
        {
            _mockDeadlineService = new Mock<IDeadlineService>();
            _mockLogger = new Mock<ILogger<DeadlineController>>();
            _controller = new DeadlineController(_mockDeadlineService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUpcomingDeadlines_ReturnsOkResult_WithDeadlines()
        {
            // Arrange
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var deadlines = new List<Deadline>
            {
                new Deadline { DeadlineId = 1, DeadlineType = "Motion Filing", DeadlineDate = DateTime.Now.AddDays(5), Case = testCase },
                new Deadline { DeadlineId = 2, DeadlineType = "Discovery", DeadlineDate = DateTime.Now.AddDays(10), Case = testCase }
            };
            _mockDeadlineService.Setup(s => s.GetUpcomingDeadlinesAsync(30))
                .ReturnsAsync(deadlines);

            // Act
            var result = await _controller.GetUpcomingDeadlines(30);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<Deadline>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
        }

        [Fact]
        public async Task GetUpcomingDeadlines_ReturnsOkResult_WithEmptyList_WhenNoDeadlines()
        {
            // Arrange
            _mockDeadlineService.Setup(s => s.GetUpcomingDeadlinesAsync(30))
                .ReturnsAsync(new List<Deadline>());

            // Act
            var result = await _controller.GetUpcomingDeadlines(30);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<Deadline>>(okResult.Value);
            Assert.Empty(returnedDeadlines);
        }

        [Fact]
        public async Task GetUpcomingDeadlines_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockDeadlineService.Setup(s => s.GetUpcomingDeadlinesAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUpcomingDeadlines(30);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetOverdueDeadlines_ReturnsOkResult_WithOverdueDeadlines()
        {
            // Arrange
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var overdueDeadlines = new List<Deadline>
            {
                new Deadline { DeadlineId = 1, DeadlineType = "Trial Date", DeadlineDate = DateTime.Now.AddDays(-5), IsCompleted = false, Case = testCase },
                new Deadline { DeadlineId = 2, DeadlineType = "Filing", DeadlineDate = DateTime.Now.AddDays(-2), IsCompleted = false, Case = testCase }
            };
            _mockDeadlineService.Setup(s => s.GetOverdueDeadlinesAsync())
                .ReturnsAsync(overdueDeadlines);

            // Act
            var result = await _controller.GetOverdueDeadlines();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<Deadline>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
        }

        [Fact]
        public async Task GetDeadlinesByCase_ReturnsOkResult_WithDeadlines()
        {
            // Arrange
            int caseId = 1;
            var testCase = new Case
            {
                CaseId = caseId,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var deadlines = new List<Deadline>
            {
                new Deadline { DeadlineId = 1, CaseId = caseId, DeadlineType = "Discovery", DeadlineDate = DateTime.Now.AddDays(15), Case = testCase },
                new Deadline { DeadlineId = 2, CaseId = caseId, DeadlineType = "Motion", DeadlineDate = DateTime.Now.AddDays(20), Case = testCase }
            };
            _mockDeadlineService.Setup(s => s.GetDeadlinesByCaseAsync(caseId))
                .ReturnsAsync(deadlines);

            // Act
            var result = await _controller.GetDeadlinesByCase(caseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<Deadline>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
        }

        [Fact]
        public async Task GetDeadlinesByCase_ReturnsBadRequest_WhenCaseIdIsZero()
        {
            // Act
            var result = await _controller.GetDeadlinesByCase(0);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateDeadline_ReturnsCreatedAtAction_WhenDeadlineIsValid()
        {
            // Arrange
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var newDeadline = new Deadline
            {
                DeadlineType = "Motion Filing",
                Description = "File motion to dismiss",
                DeadlineDate = DateTime.Now.AddDays(30),
                CaseId = 1,
                IsCritical = true,
                Case = testCase
            };

            var createdDeadline = new Deadline
            {
                DeadlineId = 1,
                DeadlineType = newDeadline.DeadlineType,
                Description = newDeadline.Description,
                DeadlineDate = newDeadline.DeadlineDate,
                CaseId = newDeadline.CaseId,
                IsCritical = newDeadline.IsCritical,
                Case = testCase
            };

            _mockDeadlineService.Setup(s => s.CreateDeadlineAsync(It.IsAny<Deadline>()))
                .ReturnsAsync(createdDeadline);

            // Act
            var result = await _controller.CreateDeadline(newDeadline);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(DeadlineController.GetDeadlinesByCase), createdAtActionResult.ActionName);
            var returnedDeadline = Assert.IsType<Deadline>(createdAtActionResult.Value);
            Assert.Equal(1, returnedDeadline.DeadlineId);
        }

        [Fact]
        public async Task CreateDeadline_ReturnsBadRequest_WhenDeadlineIsNull()
        {
            // Act
            var result = await _controller.CreateDeadline(null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateDeadline_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var invalidDeadline = new Deadline
            {
                DeadlineType = "Test",
                Case = testCase
            }; // Still missing some required fields but has the required ones for compilation
            _controller.ModelState.AddModelError("DeadlineType", "DeadlineType is required");

            // Act
            var result = await _controller.CreateDeadline(invalidDeadline);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateDeadline_ReturnsOkResult_WhenDeadlineIsUpdated()
        {
            // Arrange
            int deadlineId = 1;
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var updatedDeadline = new Deadline
            {
                DeadlineId = deadlineId,
                DeadlineType = "Updated Motion",
                Description = "Updated description",
                DeadlineDate = DateTime.Now.AddDays(45),
                CaseId = 1,
                Case = testCase
            };

            _mockDeadlineService.Setup(s => s.UpdateDeadlineAsync(It.IsAny<Deadline>()))
                .ReturnsAsync(updatedDeadline);

            // Act
            var result = await _controller.UpdateDeadline(deadlineId, updatedDeadline);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadline = Assert.IsType<Deadline>(okResult.Value);
            Assert.Equal("Updated Motion", returnedDeadline.DeadlineType);
        }

        [Fact]
        public async Task UpdateDeadline_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var deadline = new Deadline
            {
                DeadlineId = 2,
                DeadlineType = "Test",
                Case = testCase
            };

            // Act
            var result = await _controller.UpdateDeadline(1, deadline);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateDeadline_ReturnsBadRequest_WhenDeadlineIsNull()
        {
            // Act
            var result = await _controller.UpdateDeadline(1, null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteDeadline_ReturnsNoContent_WhenDeadlineIsDeleted()
        {
            // Arrange
            int deadlineId = 1;
            _mockDeadlineService.Setup(s => s.DeleteDeadlineAsync(deadlineId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteDeadline(deadlineId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDeadline_ReturnsNotFound_WhenDeadlineDoesNotExist()
        {
            // Arrange
            int deadlineId = 999;
            _mockDeadlineService.Setup(s => s.DeleteDeadlineAsync(deadlineId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteDeadline(deadlineId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteDeadline_ReturnsBadRequest_WhenIdIsZero()
        {
            // Act
            var result = await _controller.DeleteDeadline(0);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task MarkDeadlineComplete_ReturnsOkResult_WhenDeadlineIsMarkedComplete()
        {
            // Arrange
            int deadlineId = 1;
            _mockDeadlineService.Setup(s => s.MarkDeadlineCompleteAsync(deadlineId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.MarkDeadlineComplete(deadlineId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Deadline marked as complete", okResult.Value);
        }

        [Fact]
        public async Task MarkDeadlineComplete_ReturnsNotFound_WhenDeadlineDoesNotExist()
        {
            // Arrange
            int deadlineId = 999;
            _mockDeadlineService.Setup(s => s.MarkDeadlineCompleteAsync(deadlineId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.MarkDeadlineComplete(deadlineId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MarkDeadlineComplete_ReturnsBadRequest_WhenIdIsZero()
        {
            // Act
            var result = await _controller.MarkDeadlineComplete(0);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCriticalDeadlines_ReturnsOkResult_WithCriticalDeadlines()
        {
            // Arrange
            var testCase = new Case
            {
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case",
                CaseType = "Civil",
                Client = new Client { ClientId = 1, ClientName = "Test Client", Address = "123 Test St", Phone = "555-0101", Email = "test@example.com" }
            };

            var criticalDeadlines = new List<Deadline>
            {
                new Deadline { DeadlineId = 1, DeadlineType = "Trial Date", IsCritical = true, DeadlineDate = DateTime.Now.AddDays(7), Case = testCase },
                new Deadline { DeadlineId = 2, DeadlineType = "Settlement Conference", IsCritical = true, DeadlineDate = DateTime.Now.AddDays(3), Case = testCase }
            };
            _mockDeadlineService.Setup(s => s.GetCriticalDeadlinesAsync())
                .ReturnsAsync(criticalDeadlines);

            // Act
            var result = await _controller.GetCriticalDeadlines();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<Deadline>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
            Assert.All(returnedDeadlines, d => Assert.True(d.IsCritical));
        }
    }
}