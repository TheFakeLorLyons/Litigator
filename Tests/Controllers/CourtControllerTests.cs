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
    public class CourtControllerTests
    {
        private readonly Mock<ICourtService> _mockCourtService;
        private readonly Mock<ILogger<CourtController>> _mockLogger;
        private readonly CourtController _controller;

        public CourtControllerTests()
        {
            _mockCourtService = new Mock<ICourtService>();
            _mockLogger = new Mock<ILogger<CourtController>>();
            _controller = new CourtController(_mockCourtService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllCourts_ReturnsOkResult_WithListOfCourts()
        {
            // Arrange
            var courts = new List<Court>
            {
                new Court { CourtId = 1, CourtName = "New York Supreme Court", Address = "123 Fake Street", County = "New York", State = "NY", CourtType = "State" },
                new Court { CourtId = 2, CourtName = "U.S. District Court SDNY", Address = "123 Fake Street", County = "New York", State = "NY", CourtType = "Federal" }
            };
            _mockCourtService.Setup(s => s.GetAllCourtsAsync()).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetAllCourts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourts = Assert.IsAssignableFrom<IEnumerable<Court>>(okResult.Value);
            Assert.Equal(2, returnedCourts.Count());
        }

        [Fact]
        public async Task GetCourt_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var court = new Court { CourtId = 1, CourtName = "New York Supreme Court", Address = "123 Fake Street", County = "New York", State = "NY", CourtType = "State" };
            _mockCourtService.Setup(s => s.GetCourtByIdAsync(1)).ReturnsAsync(court);

            // Act
            var result = await _controller.GetCourt(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourt = Assert.IsType<Court>(okResult.Value);
            Assert.Equal(1, returnedCourt.CourtId);
            Assert.Equal("New York Supreme Court", returnedCourt.CourtName);
        }

        [Fact]
        public async Task GetCourt_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockCourtService.Setup(s => s.GetCourtByIdAsync(999)).ReturnsAsync((Court?)null);

            // Act
            var result = await _controller.GetCourt(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateCourt_ValidCourt_ReturnsCreatedAtAction()
        {
            // Arrange
            var newCourt = new Court { CourtName = "Test Court", Address = "123 Fake Street", County = "Test County", State = "NY", CourtType = "State" };
            var createdCourt = new Court { CourtId = 1, CourtName = "Test Court", Address = "123 Fake Street", County = "Test County", State = "NY", CourtType = "State" };
            _mockCourtService.Setup(s => s.CreateCourtAsync(It.IsAny<Court>())).ReturnsAsync(createdCourt);

            // Act
            var result = await _controller.CreateCourt(newCourt);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetCourt", createdAtActionResult.ActionName);
            Assert.Equal(1, createdAtActionResult.RouteValues?["id"]);

            var returnedCourt = Assert.IsType<Court>(createdAtActionResult.Value);
            Assert.Equal("Test Court", returnedCourt.CourtName);
        }

        [Fact]
        public async Task CreateCourt_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("CourtName", "Court name is required");
            var newCourt = new Court { CourtId = 1, CourtName = "Test Court", Address = "123 Fake Street", County = "Test County", State = "NY", CourtType = "State" };

            // Act
            var result = await _controller.CreateCourt(newCourt);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateCourt_ValidCourt_ReturnsOkResult()
        {
            // Arrange
            var updatedCourt = new Court { CourtId = 1, CourtName = "Test Court", Address = "123 Fake Street", County = "Test County", State = "NY", CourtType = "State" };
            _mockCourtService.Setup(s => s.UpdateCourtAsync(It.IsAny<Court>())).ReturnsAsync(updatedCourt);

            // Act
            var result = await _controller.UpdateCourt(1, updatedCourt);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourt = Assert.IsType<Court>(okResult.Value);
            Assert.Equal("Updated Court", returnedCourt.CourtName);
        }

        [Fact]
        public async Task UpdateCourt_MismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var court = new Court { CourtId = 2, CourtName = "Test Court", Address = "123 Fake Street", County = "Test County", State = "NY", CourtType = "State" };

            // Act
            var result = await _controller.UpdateCourt(1, court);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateCourt_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("CourtName", "Court name is required");
            var court = new Court { CourtId = 1, CourtName = "Updated Court", Address = "123 Fake Street", County = "Updated County", State = "NY", CourtType = "State" };

            // Act
            var result = await _controller.UpdateCourt(1, court);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCourt_ExistingId_ReturnsNoContent()
        {
            // Arrange
            _mockCourtService.Setup(s => s.DeleteCourtAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCourt(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCourt_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockCourtService.Setup(s => s.DeleteCourtAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCourt(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCourtsByState_ValidState_ReturnsOkResult()
        {
            // Arrange
            var courts = new List<Court>
            {
                new Court { CourtId = 1, CourtName = "NY Court 1", Address = "123 Fake Street", County = "County1", State = "NY", CourtType = "State" },
                new Court { CourtId = 2, CourtName = "NY Court 2", Address = "123 Fake Street", County = "County2", State = "NY", CourtType = "Federal" }
            };
            _mockCourtService.Setup(s => s.GetCourtsByStateAsync("NY")).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetCourtsByState("NY");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourts = Assert.IsAssignableFrom<IEnumerable<Court>>(okResult.Value);
            Assert.Equal(2, returnedCourts.Count());
            Assert.All(returnedCourts, c => Assert.Equal("NY", c.State));
        }

        [Fact]
        public async Task GetCourtsByState_EmptyState_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetCourtsByState("");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateCourt_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var newCourt = new Court { CourtName = "Test Court", Address = "123 Fake Street", County = "Test County", State = "NY", CourtType = "State" };
            _mockCourtService.Setup(s => s.CreateCourtAsync(It.IsAny<Court>()))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateCourt(newCourt);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}