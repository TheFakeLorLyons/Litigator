using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Litigator.Tests.Controllers
{
    public class AttorneyControllerTests
    {
        private readonly Mock<IAttorneyService> _mockAttorneyService;
        private readonly Mock<ILogger<AttorneyController>> _mockLogger;
        private readonly AttorneyController _controller;

        public AttorneyControllerTests()
        {
            _mockAttorneyService = new Mock<IAttorneyService>();
            _mockLogger = new Mock<ILogger<AttorneyController>>();
            _controller = new AttorneyController(_mockAttorneyService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllAttorneys_ReturnsOkResult_WithAttorneyList()
        {
            // Arrange
            var attorneys = new List<Attorney>
            {
                new Attorney { AttorneyId = 1, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" },
                new Attorney { AttorneyId = 2, FirstName = "Jane", LastName = "Smith", BarNumber = "67890", Email = "jane.smith@law.com", Phone = "555-0124" }
            };
            _mockAttorneyService.Setup(s => s.GetAllAttorneysAsync()).ReturnsAsync(attorneys);

            // Act
            var result = await _controller.GetAllAttorneys();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorneys = Assert.IsAssignableFrom<IEnumerable<Attorney>>(okResult.Value);
            Assert.Equal(2, returnedAttorneys.Count());
        }

        [Fact]
        public async Task GetAttorney_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var attorney = new Attorney { AttorneyId = 1, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "617-323-1234"};
            _mockAttorneyService.Setup(s => s.GetAttorneyByIdAsync(1)).ReturnsAsync(attorney);

            // Act
            var result = await _controller.GetAttorney(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorney = Assert.IsType<Attorney>(okResult.Value);
            Assert.Equal(1, returnedAttorney.AttorneyId);
        }

        [Fact]
        public async Task GetAttorney_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockAttorneyService.Setup(s => s.GetAttorneyByIdAsync(999)).ReturnsAsync((Attorney?)null);

            // Act
            var result = await _controller.GetAttorney(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAttorneyByBarNumber_WithValidBarNumber_ReturnsOkResult()
        {
            // Arrange
            var attorney = new Attorney { AttorneyId = 1, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" };
            _mockAttorneyService.Setup(s => s.GetAttorneyByBarNumberAsync("12345")).ReturnsAsync(attorney);

            // Act
            var result = await _controller.GetAttorneyByBarNumber("12345");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorney = Assert.IsType<Attorney>(okResult.Value);
            Assert.Equal("12345", returnedAttorney.BarNumber);
        }

        [Fact]
        public async Task GetAttorneyByBarNumber_WithInvalidBarNumber_ReturnsNotFound()
        {
            // Arrange
            _mockAttorneyService.Setup(s => s.GetAttorneyByBarNumberAsync("99999")).ReturnsAsync((Attorney?)null);

            // Act
            var result = await _controller.GetAttorneyByBarNumber("99999");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateAttorney_WithValidAttorney_ReturnsCreatedAtAction()
        {
            // Arrange
            var attorney = new Attorney { AttorneyId = 1, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" };
            var createdAttorney = new Attorney { AttorneyId = 1, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "666-0123" };
            _mockAttorneyService.Setup(s => s.CreateAttorneyAsync(attorney)).ReturnsAsync(createdAttorney);

            // Act
            var result = await _controller.CreateAttorney(attorney);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetAttorney", createdAtActionResult.ActionName);
            Assert.Equal(1, createdAtActionResult.RouteValues?["id"]);
            var returnedAttorney = Assert.IsType<Attorney>(createdAtActionResult.Value);
            Assert.Equal(1, returnedAttorney.AttorneyId);
        }

        [Fact]
        public async Task CreateAttorney_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var attorney = new Attorney { FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" };

            // Assert
            var result = await _controller.CreateAttorney(attorney);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAttorney_WithValidIdAndAttorney_ReturnsOkResult()
        {
            // Arrange
            var attorney = new Attorney { AttorneyId = 1, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" };
            _mockAttorneyService.Setup(s => s.UpdateAttorneyAsync(attorney)).ReturnsAsync(attorney);

            // Act
            var result = await _controller.UpdateAttorney(1, attorney);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorney = Assert.IsType<Attorney>(okResult.Value);
            Assert.Equal(1, returnedAttorney.AttorneyId);
        }

        [Fact]
        public async Task UpdateAttorney_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var attorney = new Attorney { AttorneyId = 2, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" };

            // Act
            var result = await _controller.UpdateAttorney(1, attorney);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAttorney_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");
            var attorney = new Attorney 
            { 
                AttorneyId = 1,
                FirstName = "John",
                LastName = "Doe",
                BarNumber = "12345",
                Email = "john.doe@law.com",
                Phone = "555-0123"
            };

            // Act
            var result = await _controller.UpdateAttorney(1, attorney);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteAttorney_WithValidId_ReturnsNoContent()
        {
            // Arrange
            _mockAttorneyService.Setup(s => s.DeleteAttorneyAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAttorney(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAttorney_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockAttorneyService.Setup(s => s.DeleteAttorneyAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAttorney(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetActiveAttorneys_ReturnsOkResult_WithActiveAttorneyList()
        {
            // Arrange
            var activeAttorneys = new List<Attorney>
            {
                new Attorney { AttorneyId = 1, FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" }
            };
            _mockAttorneyService.Setup(s => s.GetActiveAttorneysAsync()).ReturnsAsync(activeAttorneys);

            // Act
            var result = await _controller.GetActiveAttorneys();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorneys = Assert.IsAssignableFrom<IEnumerable<Attorney>>(okResult.Value);
            Assert.Single(returnedAttorneys);
        }

        [Fact]
        public async Task CreateAttorney_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var attorney = new Attorney { FirstName = "John", LastName = "Doe", BarNumber = "12345", Email = "john.doe@law.com", Phone = "555-0123" };
            _mockAttorneyService.Setup(s => s.CreateAttorneyAsync(attorney)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateAttorney(attorney));
        }
    }
}