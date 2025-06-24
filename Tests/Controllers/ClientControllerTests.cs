using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Tests.Controllers
{
    public class ClientControllerTests
    {
        private readonly Mock<IClientService> _mockClientService;
        private readonly ClientController _controller;

        public ClientControllerTests()
        {
            _mockClientService = new Mock<IClientService>();
            _controller = new ClientController(_mockClientService.Object);
        }

        [Fact]
        public async Task GetAllClients_ReturnsOkResult_WithListOfClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                new Client {
                    ClientId = 1,
                    ClientName = "Test Client 1",
                    Email = "test1@test.com",
                    Address = "123 Test St",
                    Phone = "555-0101"
                },
                new Client {
                    ClientId = 2,
                    ClientName = "Test Client 2",
                    Email = "test2@test.com",
                    Address = "456 Test Ave",
                    Phone = "555-0102"
                }
            };
            _mockClientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(clients);

            // Act
            var result = await _controller.GetAllClients();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClients = Assert.IsAssignableFrom<IEnumerable<Client>>(okResult.Value);
            Assert.Equal(2, returnedClients.Count());
        }

        [Fact]
        public async Task GetClient_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var clientId = 1;
            var client = new Client
            {
                ClientId = clientId,
                ClientName = "Test Client",
                Email = "test@test.com",
                Address = "123 Test St",
                Phone = "555-0101"
            };
            _mockClientService.Setup(s => s.GetClientByIdAsync(clientId)).ReturnsAsync(client);

            // Act
            var result = await _controller.GetClient(clientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClient = Assert.IsType<Client>(okResult.Value);
            Assert.Equal(clientId, returnedClient.ClientId);
        }

        [Fact]
        public async Task GetClient_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var clientId = 99;
            _mockClientService.Setup(s => s.GetClientByIdAsync(clientId)).ReturnsAsync((Client?)null);

            // Act
            var result = await _controller.GetClient(clientId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"Client with ID {clientId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task SearchClients_WithValidSearchTerm_ReturnsOkResult()
        {
            // Arrange
            var searchTerm = "Test";
            var clients = new List<Client>
            {
                new Client {
                    ClientId = 1,
                    ClientName = "Test Client",
                    Email = "test@test.com",
                    Address = "123 Test St",
                    Phone = "555-0101"
                }
            };
            _mockClientService.Setup(s => s.SearchClientsAsync(searchTerm)).ReturnsAsync(clients);

            // Act
            var result = await _controller.SearchClients(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClients = Assert.IsAssignableFrom<IEnumerable<Client>>(okResult.Value);
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
            var newClient = new Client
            {
                ClientName = "New Client",
                Email = "new@test.com",
                Address = "789 New St",
                Phone = "555-0103"
            };
            var createdClient = new Client
            {
                ClientId = 1,
                ClientName = "New Client",
                Email = "new@test.com",
                Address = "789 New St",
                Phone = "555-0103"
            };
            _mockClientService.Setup(s => s.CreateClientAsync(newClient)).ReturnsAsync(createdClient);

            // Act
            var result = await _controller.CreateClient(newClient);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedClient = Assert.IsType<Client>(createdAtActionResult.Value);
            Assert.Equal(createdClient.ClientId, returnedClient.ClientId);
            Assert.Equal("GetClient", createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task CreateClient_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var newClient = new Client
            {
                ClientName = "New Client",
                Email = "new@test.com",
                Address = "789 New St",
                Phone = "555-0103"
            };
            _mockClientService.Setup(s => s.CreateClientAsync(newClient))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateClient(newClient);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Error creating client", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task UpdateClient_WithValidClient_ReturnsOkResult()
        {
            // Arrange
            var clientId = 1;
            var client = new Client
            {
                ClientId = clientId,
                ClientName = "Updated Client",
                Email = "updated@test.com",
                Address = "321 Updated St",
                Phone = "555-0104"
            };
            _mockClientService.Setup(s => s.UpdateClientAsync(client)).ReturnsAsync(client);

            // Act
            var result = await _controller.UpdateClient(clientId, client);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClient = Assert.IsType<Client>(okResult.Value);
            Assert.Equal(clientId, returnedClient.ClientId);
        }

        [Fact]
        public async Task UpdateClient_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var clientId = 1;
            var client = new Client
            {
                ClientId = 2,
                ClientName = "Updated Client",
                Email = "updated@test.com",
                Address = "321 Updated St",
                Phone = "555-0104"
            };

            // Act
            var result = await _controller.UpdateClient(clientId, client);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Client ID mismatch.", badRequestResult.Value);
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
            Assert.Equal($"Client with ID {clientId} not found.", notFoundResult.Value);
        }
    }
}