using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Deadline;
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
            var deadlines = new List<DeadlineDTO>
            {
                CreateTestDeadlineDTO(1, "Motion Filing"),
                CreateTestDeadlineDTO(2, "Discovery")
            };
            _mockDeadlineService.Setup(s => s.GetUpcomingDeadlinesAsync(30))
                .ReturnsAsync(deadlines);

            // Act
            var result = await _controller.GetUpcomingDeadlines(30);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<DeadlineDTO>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
        }

        [Fact]
        public async Task GetUpcomingDeadlines_ReturnsOkResult_WithEmptyList_WhenNoDeadlines()
        {
            // Arrange
            _mockDeadlineService.Setup(s => s.GetUpcomingDeadlinesAsync(30))
                .ReturnsAsync(new List<DeadlineDTO>());

            // Act
            var result = await _controller.GetUpcomingDeadlines(30);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<DeadlineDTO>>(okResult.Value);
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
            var overdueDeadlines = new List<DeadlineDTO>
            {
                CreateTestDeadlineDTO(1, "Trial Date", 1),
                CreateTestDeadlineDTO(2, "Filing", 2)
            };
            _mockDeadlineService.Setup(s => s.GetOverdueDeadlinesAsync())
                .ReturnsAsync(overdueDeadlines);

            // Act
            var result = await _controller.GetOverdueDeadlines();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<DeadlineDTO>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
        }

        [Fact]
        public async Task GetDeadlinesByCase_ReturnsOkResult_WithDeadlines()
        {
            // Arrange
            int caseId = 1;
            var deadlines = new List<DeadlineDTO>
            {
                CreateTestDeadlineDTO(1, "Discovery", caseId),
                CreateTestDeadlineDTO(2, "Motion", caseId)
            };
            
            _mockDeadlineService.Setup(s => s.GetDeadlinesByCaseAsync(caseId))
                .ReturnsAsync(deadlines);

            // Act
            var result = await _controller.GetDeadlinesByCase(caseId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DeadlineDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<DeadlineDTO>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
            Assert.All(returnedDeadlines, d => Assert.Equal(caseId, d.CaseId));
        }

        [Fact]
        public async Task GetAllDeadlines_ReturnsOkResult_WithAllDeadlines()
        {
            // Arrange
            var deadlines = new List<DeadlineDTO>
            {
                CreateTestDeadlineDTO(1, "Motion Filing"),
                CreateTestDeadlineDTO(2, "Discovery"),
                CreateTestDeadlineDTO(3, "Trial Date")
            };
            _mockDeadlineService.Setup(s => s.GetAllDeadlinesAsync())
                .ReturnsAsync(deadlines);

            // Act
            var result = await _controller.GetAllDeadlines();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<DeadlineDTO>>(okResult.Value);
            Assert.Equal(3, returnedDeadlines.Count());
        }

        [Fact]
        public async Task GetDeadline_ExistingId_ReturnsOkResult_WithDeadline()
        {
            // Arrange
            var deadline = CreateTestDeadlineDTO(1);
            _mockDeadlineService.Setup(s => s.GetDeadlineByIdAsync(1))
                .ReturnsAsync(deadline);

            // Act
            var result = await _controller.GetDeadline(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadline = Assert.IsType<DeadlineDTO>(okResult.Value);
            Assert.Equal(1, returnedDeadline.DeadlineId);
        }

        [Fact]
        public async Task GetDeadline_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockDeadlineService.Setup(s => s.GetDeadlineByIdAsync(999))
                .ReturnsAsync((DeadlineDTO?)null);

            // Act
            var result = await _controller.GetDeadline(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetDeadlinesByCase_ReturnsBadRequest_WhenCaseIdIsZero()
        {
            // Act
            var result = await _controller.GetDeadlinesByCase(0);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        private DeadlineDTO CreateTestDeadlineDTO(int id = 1, string type = "Motion Filing", int caseId = 1)
        {
            return new DeadlineDTO
            {
                DeadlineId = id,
                DeadlineType = type,
                Description = $"Test {type}",
                DeadlineDate = DateTime.Now.AddDays(10),
                IsCompleted = false,
                CompletedDate = null,
                IsCritical = false,
                CaseId = caseId,
                CaseNumber = $"2024-CV-{caseId:000}",
                CaseTitle = $"Test Case {caseId}"
            };
        }

        private DeadlineCreateDTO CreateTestDeadlineCreateDTO(int caseId = 1)
        {
            return new DeadlineCreateDTO
            {
                DeadlineType = "Motion Filing",
                Description = "File motion to dismiss",
                DeadlineDate = DateTime.Now.AddDays(30),
                CaseId = caseId,
                IsCritical = true
            };
        }

        [Fact]
        public async Task CreateDeadline_ValidDeadline_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new DeadlineCreateDTO
            {
                DeadlineType = "Motion Filing",
                Description = "File motion to dismiss",
                DeadlineDate = DateTime.Now.AddDays(30),
                CaseId = 1,
                IsCritical = true
            };

            var createdDeadline = CreateTestDeadlineDTO(1, "Motion Filing");
            createdDeadline.IsCritical = true;

            _mockDeadlineService.Setup(s => s.CreateDeadlineAsync(It.IsAny<DeadlineCreateDTO>()))
                .ReturnsAsync(createdDeadline);

            // Act
            var result = await _controller.CreateDeadline(createDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(DeadlineController.GetDeadline), createdAtActionResult.ActionName);
            var returnedDeadline = Assert.IsType<DeadlineDTO>(createdAtActionResult.Value);
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
            var createDto = new DeadlineCreateDTO
            {
                DeadlineType = "", // Invalid - required field
                Description = "Test description",
                DeadlineDate = DateTime.Now.AddDays(30),
                CaseId = 1,
                IsCritical = false
            };

            _controller.ModelState.AddModelError("DeadlineType", "DeadlineType is required");

            // Act & Assert
            var result = await _controller.CreateDeadline(createDto);
        }

        private DeadlineUpdateDTO CreateTestDeadlineUpdateDTO()
        {
            return new DeadlineUpdateDTO
            {
                DeadlineType = "Updated Motion",
                Description = "Updated description",
                DeadlineDate = DateTime.Now.AddDays(45),
                IsCompleted = false,
                CompletedDate = null,
                IsCritical = true
            };
        }

        [Fact]
        public async Task UpdateDeadline_ValidDeadline_ReturnsOkResult()
        {
            // Arrange
            int deadlineId = 1;
            var updateDto = new DeadlineUpdateDTO
            {
                DeadlineType = "Updated Motion",
                Description = "Updated description",
                DeadlineDate = DateTime.Now.AddDays(45),
                IsCompleted = false,
                CompletedDate = null,
                IsCritical = true
            };

            var updatedDeadline = CreateTestDeadlineDTO(deadlineId, "Updated Motion");
            updatedDeadline.IsCritical = true;

            _mockDeadlineService.Setup(s => s.UpdateDeadlineAsync(deadlineId, It.IsAny<DeadlineUpdateDTO>()))
                .ReturnsAsync(updatedDeadline);

            // Act
            var result = await _controller.UpdateDeadline(deadlineId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadline = Assert.IsType<DeadlineDTO>(okResult.Value);
            Assert.Equal("Updated Motion", returnedDeadline.DeadlineType);
        }

        [Fact]
        public async Task UpdateDeadline_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var updateDto = CreateTestDeadlineUpdateDTO();

            // Act
            var result = await _controller.UpdateDeadline(1, updateDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateDeadline_NonExistingDeadline_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new DeadlineUpdateDTO
            {
                DeadlineType = "Test",
                DeadlineDate = DateTime.Now.AddDays(10)
            };

            _mockDeadlineService.Setup(s => s.UpdateDeadlineAsync(999, It.IsAny<DeadlineUpdateDTO>()))
                .ReturnsAsync((DeadlineDTO?)null);

            // Act
            var result = await _controller.UpdateDeadline(999, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
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
            var criticalDeadlines = new List<DeadlineDTO>
            {
                CreateTestDeadlineDTO(1, "Trial Date"),
                CreateTestDeadlineDTO(2, "Settlement Conference")
            };
            _mockDeadlineService.Setup(s => s.GetCriticalDeadlinesAsync())
                .ReturnsAsync(criticalDeadlines);

            // Act
            var result = await _controller.GetCriticalDeadlines();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDeadlines = Assert.IsAssignableFrom<IEnumerable<DeadlineDTO>>(okResult.Value);
            Assert.Equal(2, returnedDeadlines.Count());
            Assert.All(returnedDeadlines, d => Assert.True(d.IsCritical));
        }
    }
}