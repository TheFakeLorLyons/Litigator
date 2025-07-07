using FluentAssertions;
using Litigator.Controllers;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Litigator.Tests.Controllers
{
    public class JudgeControllerTests
    {
        private readonly Mock<IJudgeService> _mockJudgeService;
        private readonly Mock<ILogger<JudgeController>> _mockLogger;
        private readonly JudgeController _controller;

        public JudgeControllerTests()
        {
            _mockJudgeService = new Mock<IJudgeService>();
            _mockLogger = new Mock<ILogger<JudgeController>>();
            _controller = new JudgeController(_mockJudgeService.Object, _mockLogger.Object);
        }

        #region GetAllJudges Tests

        [Fact]
        public async Task GetAllJudges_ReturnsOkResultWithJudges()
        {
            // Arrange
            var judges = new List<JudgeDTO>
            {
                CreateTestJudgeDTO(1, "John Smith", "BAR001"),
                CreateTestJudgeDTO(2, "Jane Adams", "BAR002")
            };

            _mockJudgeService.Setup(x => x.GetAllJudgesAsync())
                .ReturnsAsync(judges);

            // Act
            var result = await _controller.GetAllJudges();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedJudges = okResult!.Value as IEnumerable<JudgeDTO>;
            returnedJudges.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllJudges_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.GetAllJudgesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllJudges();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var errorResult = result.Result as ObjectResult;
            errorResult.Should().NotBeNull();
            errorResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetActiveJudges Tests

        [Fact]
        public async Task GetActiveJudges_ReturnsOkResultWithActiveJudges()
        {
            // Arrange
            var activeJudges = new List<JudgeDTO>
            {
                CreateTestJudgeDTO(1, "John Smith", "BAR001", isActive: true)
            };

            _mockJudgeService.Setup(x => x.GetActiveJudgesAsync())
                .ReturnsAsync(activeJudges);

            // Act
            var result = await _controller.GetActiveJudges();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedJudges = okResult!.Value as IEnumerable<JudgeDTO>;
            returnedJudges.Should().HaveCount(1);
            returnedJudges!.First().IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetActiveJudges_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.GetActiveJudgesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetActiveJudges();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var errorResult = result.Result as ObjectResult;
            errorResult.Should().NotBeNull();
            errorResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetJudgeById Tests

        [Fact]
        public async Task GetJudgeById_ExistingId_ReturnsOkResultWithJudge()
        {
            // Arrange
            var judge = CreateTestJudgeDetailDTO(1, "John Smith", "BAR001");
            _mockJudgeService.Setup(x => x.GetJudgeByIdAsync(1))
                .ReturnsAsync(judge);

            // Act
            var result = await _controller.GetJudgeById(1);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedJudge = okResult!.Value as JudgeDetailDTO;
            returnedJudge.Should().NotBeNull();
            returnedJudge!.JudgeId.Should().Be(1);
        }

        [Fact]
        public async Task GetJudgeById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.GetJudgeByIdAsync(999))
                .ReturnsAsync((JudgeDetailDTO?)null);

            // Act
            var result = await _controller.GetJudgeById(999);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.Value.Should().Be("Judge with ID 999 not found");
        }

        [Fact]
        public async Task GetJudgeById_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.GetJudgeByIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetJudgeById(1);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var errorResult = result.Result as ObjectResult;
            errorResult.Should().NotBeNull();
            errorResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetJudgeByBarNumber Tests

        [Fact]
        public async Task GetJudgeByBarNumber_ExistingBarNumber_ReturnsOkResultWithJudge()
        {
            // Arrange
            var judge = CreateTestJudgeDetailDTO(1, "John Smith", "BAR001");
            _mockJudgeService.Setup(x => x.GetJudgeByBarNumberAsync("BAR001"))
                .ReturnsAsync(judge);

            // Act
            var result = await _controller.GetJudgeByBarNumber("BAR001");

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedJudge = okResult!.Value as JudgeDetailDTO;
            returnedJudge.Should().NotBeNull();
            returnedJudge!.BarNumber.Should().Be("BAR001");
        }

        [Fact]
        public async Task GetJudgeByBarNumber_NonExistingBarNumber_ReturnsNotFound()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.GetJudgeByBarNumberAsync("NONEXISTENT"))
                .ReturnsAsync((JudgeDetailDTO?)null);

            // Act
            var result = await _controller.GetJudgeByBarNumber("NONEXISTENT");

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.Value.Should().Be("Judge with bar number NONEXISTENT not found");
        }

        [Fact]
        public async Task GetJudgeByBarNumber_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.GetJudgeByBarNumberAsync("BAR001"))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetJudgeByBarNumber("BAR001");

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var errorResult = result.Result as ObjectResult;
            errorResult.Should().NotBeNull();
            errorResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region CreateJudge Tests

        [Fact]
        public async Task CreateJudge_ValidJudge_ReturnsCreatedResult()
        {
            // Arrange
            var inputJudge = CreateTestJudgeDetailDTO(0, "John Smith", "BAR001");
            var createdJudge = CreateTestJudgeDetailDTO(1, "John Smith", "BAR001");

            _mockJudgeService.Setup(x => x.CreateJudgeAsync(inputJudge))
                .ReturnsAsync(createdJudge);

            // Act
            var result = await _controller.CreateJudge(inputJudge);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.ActionName.Should().Be(nameof(JudgeController.GetJudgeById));
            createdResult.RouteValues!["id"].Should().Be(1);
            var returnedJudge = createdResult.Value as JudgeDetailDTO;
            returnedJudge.Should().NotBeNull();
            returnedJudge!.JudgeId.Should().Be(1);
        }

        [Fact]
        public async Task CreateJudge_InvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            var inputJudge = CreateTestJudgeDetailDTO(0, "John Smith", "BAR001");

            _mockJudgeService.Setup(x => x.CreateJudgeAsync(inputJudge))
                .ThrowsAsync(new InvalidOperationException("Judge with bar number BAR001 already exists"));

            // Act
            var result = await _controller.CreateJudge(inputJudge);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().Be("Judge with bar number BAR001 already exists");
        }

        [Fact]
        public async Task CreateJudge_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var inputJudge = CreateTestJudgeDetailDTO(0, "John Smith", "BAR001");

            _mockJudgeService.Setup(x => x.CreateJudgeAsync(inputJudge))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateJudge(inputJudge);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var errorResult = result.Result as ObjectResult;
            errorResult.Should().NotBeNull();
            errorResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region UpdateJudge Tests

        [Fact]
        public async Task UpdateJudge_ValidUpdate_ReturnsOkResult()
        {
            // Arrange
            var updateJudge = CreateTestJudgeDetailDTO(1, "John Johnson", "BAR001");

            _mockJudgeService.Setup(x => x.UpdateJudgeAsync(updateJudge))
                .ReturnsAsync(updateJudge);

            // Act
            var result = await _controller.UpdateJudge(1, updateJudge);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var returnedJudge = okResult!.Value as JudgeDetailDTO;
            returnedJudge.Should().NotBeNull();
            returnedJudge!.LastName.Should().Be("Johnson");
        }

        [Fact]
        public async Task UpdateJudge_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var updateJudge = CreateTestJudgeDetailDTO(2, "John Smith", "BAR001");

            // Act
            var result = await _controller.UpdateJudge(1, updateJudge);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().Be("Judge ID mismatch");
        }

        [Fact]
        public async Task UpdateJudge_InvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            var updateJudge = CreateTestJudgeDetailDTO(1, "John Smith", "BAR001");

            _mockJudgeService.Setup(x => x.UpdateJudgeAsync(updateJudge))
                .ThrowsAsync(new InvalidOperationException("Judge with ID 1 not found"));

            // Act
            var result = await _controller.UpdateJudge(1, updateJudge);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().Be("Judge with ID 1 not found");
        }

        [Fact]
        public async Task UpdateJudge_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var updateJudge = CreateTestJudgeDetailDTO(1, "John Smith", "BAR001");

            _mockJudgeService.Setup(x => x.UpdateJudgeAsync(updateJudge))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateJudge(1, updateJudge);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var errorResult = result.Result as ObjectResult;
            errorResult.Should().NotBeNull();
            errorResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region DeleteJudge Tests

        [Fact]
        public async Task DeleteJudge_ExistingJudge_ReturnsNoContent()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.DeleteJudgeAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteJudge(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteJudge_NonExistingJudge_ReturnsNotFound()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.DeleteJudgeAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteJudge(999);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.Value.Should().Be("Judge with ID 999 not found");
        }

        [Fact]
        public async Task DeleteJudge_InvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.DeleteJudgeAsync(1))
                .ThrowsAsync(new InvalidOperationException("Cannot delete judge with 2 active cases"));

            // Act
            var result = await _controller.DeleteJudge(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().Be("Cannot delete judge with 2 active cases");
        }

        [Fact]
        public async Task DeleteJudge_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockJudgeService.Setup(x => x.DeleteJudgeAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteJudge(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            var errorResult = result as ObjectResult;
            errorResult.Should().NotBeNull();
            errorResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region Helper Methods

        private static JudgeDTO CreateTestJudgeDTO(int id, string name, string barNumber, bool isActive = true)
        {
            return new JudgeDTO
            {
                JudgeId = id,
                Name = name,
                BarNumber = barNumber,
                Email = $"{name.Replace(" ", ".").ToLower()}@court.gov",
                IsActive = isActive,
                CreatedDate = DateTime.UtcNow,
                TotalCases = 0,
                ActiveCases = 0
            };
        }

        private static JudgeDetailDTO CreateTestJudgeDetailDTO(int id, string fullName, string barNumber, bool isActive = true)
        {
            var nameParts = fullName.Split(' ');
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            return new JudgeDetailDTO
            {
                JudgeId = id,
                DisplayName = $"Judge {fullName}",
                ProfessionalName = $"The Honorable {fullName}",
                FullName = fullName,
                FirstName = firstName,
                LastName = lastName,
                BarNumber = barNumber,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@court.gov",
                IsActive = isActive,
                CreatedDate = DateTime.UtcNow,
                TotalCases = 0,
                ActiveCases = 0,
                ClosedCases = 0
            };
        }

        #endregion
    }
}