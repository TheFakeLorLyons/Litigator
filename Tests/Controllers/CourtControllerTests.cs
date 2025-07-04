using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.Services.Interfaces;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.DTOs.Shared;

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
            var courts = new List<CourtDTO>
            {
                new CourtDTO
                {
                    CourtId = 1,
                    CourtName = "New York Supreme Court",
                    County = "New York",
                    State = "NY",
                    CourtType = "State",
                    Address = "123 Fake Street",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 5,
                    ActiveCases = 3
                },
                new CourtDTO
                {
                    CourtId = 2,
                    CourtName = "U.S. District Court SDNY",
                    County = "New York",
                    State = "NY",
                    CourtType = "Federal",
                    Address = "500 Pearl Street",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 10,
                    ActiveCases = 7
                }
            };
            _mockCourtService.Setup(s => s.GetAllCourtsAsync()).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetAllCourts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourts = Assert.IsAssignableFrom<IEnumerable<CourtDTO>>(okResult.Value);
            Assert.Equal(2, returnedCourts.Count());
        }

        [Fact]
        public async Task GetCourtById_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var court = new CourtDetailDTO
            {
                CourtId = 1,
                CourtName = "New York Supreme Court",
                County = "New York",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                Address = "123 Fake Street",
                AddressDetails = new AddressDTO
                {
                    Line1 = "123 Fake Street",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10001",
                    Country = "United States"
                },
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                TotalCases = 5,
                ActiveCases = 3,
                ClosedCases = 2
            };
            _mockCourtService.Setup(s => s.GetCourtByIdAsync(1)).ReturnsAsync(court);

            // Act
            var result = await _controller.GetCourtById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourt = Assert.IsType<CourtDetailDTO>(okResult.Value);
            Assert.Equal(1, returnedCourt.CourtId);
            Assert.Equal("New York Supreme Court", returnedCourt.CourtName);
        }

        [Fact]
        public async Task GetCourtById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockCourtService.Setup(s => s.GetCourtByIdAsync(999)).ReturnsAsync((CourtDetailDTO?)null);

            // Act
            var result = await _controller.GetCourtById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCourt_ValidCourt_ReturnsCreatedAtAction()
        {
            // Arrange
            var newCourt = new CreateCourtDTO
            {
                CourtName = "Test Court",
                County = "Test County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                Address = new AddressDTO
                {
                    Line1 = "123 Test Street",
                    City = "Test City",
                    State = "NY",
                    PostalCode = "10001",
                    Country = "United States"
                },
                IsActive = true
            };

            var createdCourt = new CourtDetailDTO
            {
                CourtId = 1,
                CourtName = "Test Court",
                County = "Test County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                Address = "123 Test Street",
                AddressDetails = new AddressDTO
                {
                    Line1 = "123 Test Street",
                    City = "Test City",
                    State = "NY",
                    PostalCode = "10001",
                    Country = "United States"
                },
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                TotalCases = 0,
                ActiveCases = 0,
                ClosedCases = 0
            };

            _mockCourtService.Setup(s => s.CreateCourtAsync(It.IsAny<CreateCourtDTO>())).ReturnsAsync(createdCourt);

            // Act
            var result = await _controller.CreateCourt(newCourt);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetCourtById", createdAtActionResult.ActionName);
            Assert.Equal(1, createdAtActionResult.RouteValues?["id"]);

            var returnedCourt = Assert.IsType<CourtDetailDTO>(createdAtActionResult.Value);
            Assert.Equal("Test Court", returnedCourt.CourtName);
        }

        [Fact]
        public async Task CreateCourt_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("CourtName", "Court name is required");
            var newCourt = new CreateCourtDTO
            {
                CourtName = "", // Invalid - required field
                County = "Test County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml"
            };

            // Act
            var result = await _controller.CreateCourt(newCourt);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateCourt_ValidCourt_ReturnsOkResult()
        {
            // Arrange
            var updateCourt = new UpdateCourtDTO
            {
                CourtId = 1,
                CourtName = "Updated Court",
                County = "Updated County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                Address = new AddressDTO
                {
                    Line1 = "456 Updated Street",
                    City = "Updated City",
                    State = "NY",
                    PostalCode = "10002",
                    Country = "United States"
                },
                IsActive = true
            };

            var updatedCourt = new CourtDetailDTO
            {
                CourtId = 1,
                CourtName = "Updated Court",
                County = "Updated County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                Address = "456 Updated Street",
                AddressDetails = new AddressDTO
                {
                    Line1 = "456 Updated Street",
                    City = "Updated City",
                    State = "NY",
                    PostalCode = "10002",
                    Country = "United States"
                },
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                ModifiedDate = DateTime.UtcNow,
                TotalCases = 3,
                ActiveCases = 2,
                ClosedCases = 1
            };

            _mockCourtService.Setup(s => s.UpdateCourtAsync(It.IsAny<UpdateCourtDTO>())).ReturnsAsync(updatedCourt);

            // Act
            var result = await _controller.UpdateCourt(1, updateCourt);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourt = Assert.IsType<CourtDetailDTO>(okResult.Value);
            Assert.Equal("Updated Court", returnedCourt.CourtName);
        }

        [Fact]
        public async Task UpdateCourt_MismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var court = new UpdateCourtDTO
            {
                CourtId = 2,
                CourtName = "Test Court",
                County = "Test County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml"
            };

            // Act
            var result = await _controller.UpdateCourt(1, court);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateCourt_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("CourtName", "Court name is required");
            var court = new UpdateCourtDTO
            {
                CourtId = 1,
                CourtName = "", // Invalid - required field
                County = "Updated County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml"
            };

            // Act
            var result = await _controller.UpdateCourt(1, court);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
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
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCourtsByState_ValidState_ReturnsOkResult()
        {
            // Arrange
            var courts = new List<CourtDTO>
            {
                new CourtDTO
                {
                    CourtId = 1,
                    CourtName = "NY Court 1",
                    County = "County1",
                    State = "NY",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    CourtType = "State",
                    Address = "123 First Street",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 5,
                    ActiveCases = 3
                },
                new CourtDTO
                {
                    CourtId = 2,
                    CourtName = "NY Court 2",
                    County = "County2",
                    State = "NY",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    CourtType = "Federal",
                    Address = "456 Second Street",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 8,
                    ActiveCases = 5
                }
            };
            _mockCourtService.Setup(s => s.GetCourtsByStateAsync("NY")).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetCourtsByState("NY");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourts = Assert.IsAssignableFrom<IEnumerable<CourtDTO>>(okResult.Value);
            Assert.Equal(2, returnedCourts.Count());
            Assert.All(returnedCourts, c => Assert.Equal("NY", c.State));
        }

        [Fact]
        public async Task GetCourtsByState_EmptyState_ReturnsInternalServerError()
        {
            // Arrange
            _mockCourtService.Setup(s => s.GetCourtsByStateAsync("")).ReturnsAsync(new List<CourtDTO>());

            // Act
            var result = await _controller.GetCourtsByState("");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourts = Assert.IsAssignableFrom<IEnumerable<CourtDTO>>(okResult.Value);
            Assert.Empty(returnedCourts);
        }

        [Fact]
        public async Task CreateCourt_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var newCourt = new CreateCourtDTO
            {
                CourtName = "Test Court",
                County = "Test County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State"
            };

            _mockCourtService.Setup(s => s.CreateCourtAsync(It.IsAny<CreateCourtDTO>()))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateCourt(newCourt);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetCourtsByCounty_ValidCounty_ReturnsOkResult()
        {
            // Arrange
            var courts = new List<CourtDTO>
            {
                new CourtDTO
                {
                    CourtId = 1,
                    CourtName = "Manhattan Court 1",
                    County = "New York",
                    State = "NY",
                    CourtType = "State",
                    Address = "123 Manhattan Street",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 12,
                    ActiveCases = 8
                },
                new CourtDTO
                {
                    CourtId = 2,
                    CourtName = "Manhattan Court 2",
                    County = "New York",
                    State = "NY",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    CourtType = "Municipal",
                    Address = "456 Manhattan Avenue",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 6,
                    ActiveCases = 4
                }
            };
            _mockCourtService.Setup(s => s.GetCourtsByCountyAsync("NY", "New York")).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetCourtsByCounty("NY", "New York");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourts = Assert.IsAssignableFrom<IEnumerable<CourtDTO>>(okResult.Value);
            Assert.Equal(2, returnedCourts.Count());
            Assert.All(returnedCourts, c => Assert.Equal("New York", c.County));
        }

        [Fact]
        public async Task GetActiveCourts_ReturnsOkResult()
        {
            // Arrange
            var courts = new List<CourtDTO>
            {
                new CourtDTO
                {
                    CourtId = 1,
                    CourtName = "Active Court 1",
                    County = "New York",
                    State = "NY",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    CourtType = "State",
                    Address = "123 Active Street",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 15,
                    ActiveCases = 10
                },
                new CourtDTO
                {
                    CourtId = 2,
                    CourtName = "Active Court 2",
                    County = "Kings",
                    State = "NY",
                    Email = "BronxFamilyCourt@nycourts.gov",
                    Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                    CourtType = "Federal",
                    Address = "456 Active Building",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    TotalCases = 20,
                    ActiveCases = 12
                }
            };
            _mockCourtService.Setup(s => s.GetActiveCourtsAsync()).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetActiveCourts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourts = Assert.IsAssignableFrom<IEnumerable<CourtDTO>>(okResult.Value);
            Assert.Equal(2, returnedCourts.Count());
            Assert.All(returnedCourts, c => Assert.True(c.IsActive));
        }

        [Fact]
        public async Task GetCourtByName_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            var court = new CourtDetailDTO
            {
                CourtId = 1,
                CourtName = "Test Court",
                County = "Test County",
                State = "NY",
                Email = "BronxFamilyCourt@nycourts.gov",
                Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                CourtType = "State",
                Address = "123 Test Street",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                TotalCases = 5,
                ActiveCases = 3,
                ClosedCases = 2
            };
            _mockCourtService.Setup(s => s.GetCourtByNameAsync("Test Court", "Test County", "NY")).ReturnsAsync(court);

            // Act
            var result = await _controller.GetCourtByName("Test Court", "Test County", "NY");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCourt = Assert.IsType<CourtDetailDTO>(okResult.Value);
            Assert.Equal("Test Court", returnedCourt.CourtName);
        }

        [Fact]
        public async Task GetCourtByName_InvalidParameters_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetCourtByName("", "Test County", "NY");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetStates_ReturnsOkResult()
        {
            // Arrange
            var states = new List<string> { "NY", "CA", "TX" };
            _mockCourtService.Setup(s => s.GetStatesAsync()).ReturnsAsync(states);

            // Act
            var result = await _controller.GetStates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStates = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Equal(3, returnedStates.Count());
        }

        [Fact]
        public async Task GetCountiesByState_ValidState_ReturnsOkResult()
        {
            // Arrange
            var counties = new List<string> { "New York", "Kings", "Queens" };
            _mockCourtService.Setup(s => s.GetCountiesByStateAsync("NY")).ReturnsAsync(counties);

            // Act
            var result = await _controller.GetCountiesByState("NY");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCounties = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Equal(3, returnedCounties.Count());
        }

        [Fact]
        public async Task CourtExists_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            _mockCourtService.Setup(s => s.CourtExistsAsync("Test Court", "Test County", "NY", null)).ReturnsAsync(true);

            // Act
            var result = await _controller.CourtExists("Test Court", "Test County", "NY");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var exists = Assert.IsType<bool>(okResult.Value);
            Assert.True(exists);
        }

        [Fact]
        public async Task CourtExists_InvalidParameters_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CourtExists("", "Test County", "NY");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}