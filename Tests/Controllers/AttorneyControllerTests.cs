using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.Enums;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.DTOs.Shared;
using Litigator.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

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

        // Helper method to create a test attorney entity (for internal use)
        private Attorney CreateTestAttorney(int id = 1, string firstName = "John", string lastName = "Doe",
            string barNumber = "12345", string email = "john.doe@law.com", string phoneNumber = "5555550123")
        {
            return new Attorney
            {
                SystemId = id,
                Name = new PersonName { First = firstName, Last = lastName },
                BarNumber = barNumber,
                Email = email,
                PrimaryPhone = new PhoneNumber { Number = phoneNumber }, // This will format to (555) 555-0123
                PrimaryAddress = Address.Create("456 Lawyer St", "Law City", "NY", "10001"),
                Specialization = LegalSpecialization.GeneralPractice,
                IsActive = true
            };
        }


        // Helper method to create a test attorney DTO
        private AttorneyDTO CreateTestAttorneyDTO(int id = 1, string name = "John Doe",
            string barNumber = "12345", string email = "john.doe@law.com", string phone = "(555) 555-0123")
        {
            return new AttorneyDTO
            {
                AttorneyId = id,
                Name = name,
                BarNumber = barNumber,
                Email = email,
                PrimaryPhone = phone,
                Specialization = LegalSpecialization.GeneralPractice,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                TotalCases = 0,
                ActiveCases = 0
            };
        }

        // Helper method to create a test attorney detail DTO
        private AttorneyDetailDTO CreateTestAttorneyDetailDTO(int id = 1, string firstName = "John", string lastName = "Doe",
            string barNumber = "12345", string email = "john.doe@law.com", string phone = "(555) 555-0123")
        {
            return new AttorneyDetailDTO
            {
                AttorneyId = id,
                DisplayName = $"{firstName} {lastName}",
                ProfessionalName = $"{firstName} {lastName}",
                FullName = $"{firstName} {lastName}",
                FirstName = firstName,
                LastName = lastName,
                BarNumber = barNumber,
                Email = email,
                PrimaryPhone = phone,
                Specialization = LegalSpecialization.GeneralPractice,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                TotalCases = 0,
                ActiveCases = 0,
                ClosedCases = 0
            };
        }

        [Fact]
        public async Task GetAllAttorneys_ReturnsOkResult_WithAttorneyDTOList()
        {
            // Arrange
            var attorneyDTOs = new List<AttorneyDTO>
            {
                CreateTestAttorneyDTO(1, "John Doe", "12345", "john.doe@law.com", "(555) 555-0123"),
                CreateTestAttorneyDTO(2, "Jane Smith", "67890", "jane.smith@law.com", "(555) 555-0124")
            };

            _mockAttorneyService.Setup(s => s.GetAllAttorneysAsync()).ReturnsAsync(attorneyDTOs);

            // Act
            var result = await _controller.GetAllAttorneys();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorneys = Assert.IsAssignableFrom<IEnumerable<AttorneyDTO>>(okResult.Value);
            Assert.Equal(2, returnedAttorneys.Count());
        }

        [Fact]
        public async Task GetAttorneyById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var attorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(617) 323-1234");

            _mockAttorneyService.Setup(s => s.GetAttorneyByIdAsync(1)).ReturnsAsync(attorneyDTO);

            // Act
            var result = await _controller.GetAttorneyById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorney = Assert.IsType<AttorneyDetailDTO>(okResult.Value);
            Assert.Equal(1, returnedAttorney.AttorneyId);
        }

        [Fact]
        public async Task GetAttorneyById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockAttorneyService.Setup(s => s.GetAttorneyByIdAsync(999)).ReturnsAsync((AttorneyDetailDTO?)null);

            // Act
            var result = await _controller.GetAttorneyById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAttorneyByBarNumber_WithValidBarNumber_ReturnsOkResult()
        {
            // Arrange
            var attorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");

            _mockAttorneyService.Setup(s => s.GetAttorneyByBarNumberAsync("12345")).ReturnsAsync(attorneyDTO);

            // Act
            var result = await _controller.GetAttorneyByBarNumber("12345");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorney = Assert.IsType<AttorneyDetailDTO>(okResult.Value);
            Assert.Equal("12345", returnedAttorney.BarNumber);
        }

        [Fact]
        public async Task GetAttorneyByBarNumber_WithInvalidBarNumber_ReturnsNotFound()
        {
            // Arrange
            _mockAttorneyService.Setup(s => s.GetAttorneyByBarNumberAsync("99999")).ReturnsAsync((AttorneyDetailDTO?)null);

            // Act
            var result = await _controller.GetAttorneyByBarNumber("99999");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateAttorney_WithValidAttorney_ReturnsCreatedAtAction()
        {
            // Arrange
            var attorneyDTO = CreateTestAttorneyDetailDTO(0, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");
            var createdAttorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");

            _mockAttorneyService.Setup(s => s.CreateAttorneyAsync(attorneyDTO)).ReturnsAsync(createdAttorneyDTO);

            // Act
            var result = await _controller.CreateAttorney(attorneyDTO);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetAttorneyById", createdAtActionResult.ActionName);
            Assert.Equal(1, createdAtActionResult.RouteValues?["id"]);
            var returnedAttorney = Assert.IsType<AttorneyDetailDTO>(createdAtActionResult.Value);
            Assert.Equal(1, returnedAttorney.AttorneyId);
        }

        [Fact]
        public async Task CreateAttorney_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");
            var attorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");

            // Act
            var result = await _controller.CreateAttorney(attorneyDTO);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAttorney_WithValidIdAndAttorney_ReturnsOkResult()
        {
            // Arrange
            var attorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");
            var updatedAttorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");

            _mockAttorneyService.Setup(s => s.UpdateAttorneyAsync(attorneyDTO)).ReturnsAsync(updatedAttorneyDTO);

            // Act
            var result = await _controller.UpdateAttorney(1, attorneyDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorney = Assert.IsType<AttorneyDetailDTO>(okResult.Value);
            Assert.Equal(1, returnedAttorney.AttorneyId);
        }

        [Fact]
        public async Task UpdateAttorney_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var attorneyDTO = CreateTestAttorneyDetailDTO(2, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");

            // Act
            var result = await _controller.UpdateAttorney(1, attorneyDTO);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAttorney_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");
            var attorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");

            // Act
            var result = await _controller.UpdateAttorney(1, attorneyDTO);

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
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetActiveAttorneys_ReturnsOkResult_WithActiveAttorneyList()
        {
            // Arrange
            var activeAttorneyDTOs = new List<AttorneyDTO>
            {
                CreateTestAttorneyDTO(1, "John Doe", "12345", "john.doe@law.com", "(555) 555-0123")
            };

            _mockAttorneyService.Setup(s => s.GetActiveAttorneysAsync()).ReturnsAsync(activeAttorneyDTOs);

            // Act
            var result = await _controller.GetActiveAttorneys();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttorneys = Assert.IsAssignableFrom<IEnumerable<AttorneyDTO>>(okResult.Value);
            Assert.Single(returnedAttorneys);
        }

        [Fact]
        public async Task CreateAttorney_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var attorneyDTO = CreateTestAttorneyDetailDTO(1, "John", "Doe", "12345", "john.doe@law.com", "(555) 555-0123");
            _mockAttorneyService.Setup(s => s.CreateAttorneyAsync(attorneyDTO)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateAttorney(attorneyDTO);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public void Attorney_PropertiesAccessible_ThroughValueObjects()
        {
            // Arrange & Act
            var attorney = CreateTestAttorney(1, "John", "Doe", "12345", "john.doe@law.com", "5555550123");

            // Assert - Test that we can access the nested properties correctly
            Assert.Equal("John", attorney.Name.First);
            Assert.Equal("Doe", attorney.Name.Last);
            Assert.Equal("John Doe", attorney.Name.Display);
            Assert.Equal("5555550123", attorney.PrimaryPhone?.Number); // Updated assertion
            Assert.Equal("(555) 555-0123", attorney.PrimaryPhone?.Formatted); // This should now match
            Assert.Equal("12345", attorney.BarNumber);
            Assert.Equal("john.doe@law.com", attorney.Email);
            Assert.True(attorney.IsActive);
            Assert.Equal(LegalSpecialization.GeneralPractice, attorney.Specialization);
        }

        [Fact]
        public void Attorney_WithAddress_PropertiesAccessible()
        {
            // Arrange
            var attorney = new Attorney
            {
                Name = new PersonName { First = "John", Last = "Doe" },
                BarNumber = "12345",
                Email = "john.doe@law.com",
                PrimaryPhone = new PhoneNumber { Number = "555-0123" },
                PrimaryAddress = new Address
                {
                    Line1 = "123 Main St",
                    City = "Boston",
                    State = "MA",
                    PostalCode = "02101"
                },
                Specialization = LegalSpecialization.GeneralPractice,
                IsActive = true
            };

            // Act & Assert
            Assert.Equal("123 Main St", attorney.PrimaryAddress?.Line1);
            Assert.Equal("Boston", attorney.PrimaryAddress?.City);
            Assert.Equal("MA", attorney.PrimaryAddress?.State);
            Assert.Equal("02101", attorney.PrimaryAddress?.PostalCode);
            Assert.Equal("123 Main St, Boston, MA, 02101", attorney.PrimaryAddress?.SingleLine);
        }
    }
}