using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.DTOs.Shared;
using Litigator.DataAccess.ValueObjects;

namespace Tests.Controllers
{
    public class ClientControllerTests
    {
        private readonly Mock<IClientService> _mockClientService;
        private readonly Mock<ILogger<ClientController>> _mockLogger;
        private readonly ClientController _controller;

        public ClientControllerTests()
        {
            _mockClientService = new Mock<IClientService>();
            _mockLogger = new Mock<ILogger<ClientController>>();
            _controller = new ClientController(_mockClientService.Object, _mockLogger.Object);
        }

        private ClientDTO CreateTestClientDTO(int id = 1, string firstName = "John", string lastName = "Doe")
        {
            return new ClientDTO
            {
                ClientId = id,
                Name = $"{firstName} {lastName}",
                Email = "test@example.com",
                PrimaryPhone = "(555) 010-1234",
                PrimaryAddress = "123 Test St, Test City, TS 12345",
                IsActive = true,
                CreatedDate = DateTime.Now,
                TotalCases = 0,
                ActiveCases = 0
            };
        }

        private ClientDetailDTO CreateTestClientDetailDTO(int id = 1, string firstName = "John", string lastName = "Doe")
        {
            return new ClientDetailDTO
            {
                ClientId = id,
                DisplayName = $"{firstName} {lastName}",
                FullName = $"{firstName} {lastName}",
                FirstName = firstName,
                LastName = lastName,
                MiddleName = null,
                Title = null,
                Suffix = null,
                PreferredName = null,
                Email = "test@example.com",
                PrimaryPhone = "(555) 010-1234",
                PrimaryAddress = "123 Fake St, Test City, TS 12345",
                AllAddresses = new List<AddressDTO>(),
                AllPhones = new List<PhoneNumberDTO>(),
                Attorneys = new List<AttorneyDTO>(),
                Notes = "Test notes",
                IsActive = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = null,
                TotalCases = 0,
                ActiveCases = 0,
                ClosedCases = 0,
                LastCaseDate = null,
                MostRecentCaseTitle = null
            };
        }

        [Fact]
        public async Task GetAllClients_ReturnsOkResult_WithListOfClients()
        {
            // Arrange
            var clients = new List<ClientDTO>
            {
                CreateTestClientDTO(1, "John", "Doe"),
                CreateTestClientDTO(2, "Jane", "Smith")
            };
            _mockClientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(clients);

            // Act
            var result = await _controller.GetAllClients();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ClientDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedClients = Assert.IsAssignableFrom<IEnumerable<ClientDTO>>(okResult.Value);
            Assert.Equal(2, returnedClients.Count());
        }

        [Fact]
        public async Task GetClientById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var clientId = 1;
            var client = CreateTestClientDetailDTO(clientId, "John", "Doe");
            _mockClientService.Setup(s => s.GetClientByIdAsync(clientId)).ReturnsAsync(client);

            // Act
            var result = await _controller.GetClientById(clientId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ClientDetailDTO>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedClient = Assert.IsType<ClientDetailDTO>(okResult.Value);
            Assert.Equal(clientId, returnedClient.ClientId);
        }

        [Fact]
        public async Task GetClientById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var clientId = 99;
            _mockClientService.Setup(s => s.GetClientByIdAsync(clientId)).ReturnsAsync((ClientDetailDTO?)null);

            // Act
            var result = await _controller.GetClientById(clientId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ClientDetailDTO>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal($"Client with ID {clientId} not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetActiveClients_ReturnsOkResult_WithActiveClients()
        {
            // Arrange
            var clients = new List<ClientDTO>
            {
                CreateTestClientDTO(1, "John", "Doe"),
                CreateTestClientDTO(2, "Jane", "Smith")
            };
            _mockClientService.Setup(s => s.GetActiveClientsAsync()).ReturnsAsync(clients);

            // Act
            var result = await _controller.GetActiveClients();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ClientDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedClients = Assert.IsAssignableFrom<IEnumerable<ClientDTO>>(okResult.Value);
            Assert.Equal(2, returnedClients.Count());
        }

        [Fact]
        public async Task SearchClients_WithValidSearchTerm_ReturnsOkResult()
        {
            // Arrange
            var searchTerm = "John";
            var clients = new List<ClientDTO>
            {
                CreateTestClientDTO(1, "John", "Doe")
            };
            _mockClientService.Setup(s => s.SearchClientsAsync(searchTerm)).ReturnsAsync(clients);

            // Act
            var result = await _controller.SearchClients(searchTerm);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ClientDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedClients = Assert.IsAssignableFrom<IEnumerable<ClientDTO>>(okResult.Value);
            Assert.Single(returnedClients);
        }

        [Fact]
        public async Task SearchClients_WithEmptySearchTerm_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchClients("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateClient_WithValidClient_ReturnsCreatedAtAction()
        {
            // Arrange
            var newClient = CreateTestClientDetailDTO(0, "New", "Client");
            var createdClient = CreateTestClientDetailDTO(1, "New", "Client");
            _mockClientService.Setup(s => s.CreateClientAsync(It.IsAny<ClientDetailDTO>())).ReturnsAsync(createdClient);

            // Act
            var result = await _controller.CreateClient(newClient);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ClientDetailDTO>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedClient = Assert.IsType<ClientDetailDTO>(createdAtActionResult.Value);
            Assert.Equal(createdClient.ClientId, returnedClient.ClientId);
            Assert.Equal(nameof(_controller.GetClientById), createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task CreateClient_ServiceThrowsInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            var newClient = CreateTestClientDetailDTO(0, "New", "Client");
            _mockClientService.Setup(s => s.CreateClientAsync(It.IsAny<ClientDetailDTO>()))
                .ThrowsAsync(new InvalidOperationException("Email already exists"));

            // Act
            var result = await _controller.CreateClient(newClient);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ClientDetailDTO>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Email already exists", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateClient_WithValidClient_ReturnsOkResult()
        {
            // Arrange
            var clientId = 1;
            var client = CreateTestClientDetailDTO(clientId, "Updated", "Client");
            _mockClientService.Setup(s => s.UpdateClientAsync(It.IsAny<ClientDetailDTO>())).ReturnsAsync(client);

            // Act
            var result = await _controller.UpdateClient(clientId, client);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ClientDetailDTO>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedClient = Assert.IsType<ClientDetailDTO>(okResult.Value);
            Assert.Equal(clientId, returnedClient.ClientId);
        }

        [Fact]
        public async Task UpdateClient_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var clientId = 1;
            var client = CreateTestClientDetailDTO(2, "Updated", "Client");

            // Act
            var result = await _controller.UpdateClient(clientId, client);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ClientDetailDTO>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Client ID mismatch", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateClient_ServiceThrowsInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            var clientId = 1;
            var client = CreateTestClientDetailDTO(clientId, "Updated", "Client");
            _mockClientService.Setup(s => s.UpdateClientAsync(It.IsAny<ClientDetailDTO>()))
                .ThrowsAsync(new InvalidOperationException("Client not found"));

            // Act
            var result = await _controller.UpdateClient(clientId, client);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ClientDetailDTO>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Client not found", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteClient_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var clientId = 1;
            _mockClientService.Setup(s => s.DeleteClientAsync(clientId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteClient(clientId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteClient_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var clientId = 99;
            _mockClientService.Setup(s => s.DeleteClientAsync(clientId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteClient(clientId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Client with ID {clientId} not found", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteClient_ServiceThrowsInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            var clientId = 1;
            _mockClientService.Setup(s => s.DeleteClientAsync(clientId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete client with active cases"));

            // Act
            var result = await _controller.DeleteClient(clientId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cannot delete client with active cases", badRequestResult.Value);
        }
    }
}