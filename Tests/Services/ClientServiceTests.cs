using Microsoft.EntityFrameworkCore;
using Bogus;
using Xunit;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Services.Implementations;

namespace Litigator.Tests.Services
{
    public class ClientServiceTests : IDisposable
    {
        private readonly LitigatorDbContext _context;
        private readonly ClientService _clientService;
        private readonly Faker<Client> _clientFaker;

        public ClientServiceTests()
        {
            var options = new DbContextOptionsBuilder<LitigatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LitigatorDbContext(options);
            _clientService = new ClientService(_context);

            // Setup Bogus faker for Client
            _clientFaker = new Faker<Client>()
                .RuleFor(c => c.ClientName, f => f.Company.CompanyName())
                .RuleFor(c => c.Address, f => f.Address.FullAddress())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.Email, f => f.Internet.Email());
        }

        [Fact]
        public async Task CreateClientAsync_ShouldCreateClient_WhenValidDataProvided()
        {
            // Arrange
            var newClient = _clientFaker.Generate();

            // Act
            var result = await _clientService.CreateClientAsync(newClient);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ClientId > 0);
            Assert.Equal(newClient.ClientName, result.ClientName);
            Assert.Equal(newClient.Email, result.Email);

            var clientInDb = await _context.Clients.FindAsync(result.ClientId);
            Assert.NotNull(clientInDb);
        }

        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenClientExists()
        {
            // Arrange
            var testClient = _clientFaker.Generate();
            _context.Clients.Add(testClient);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.GetClientByIdAsync(testClient.ClientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testClient.ClientId, result.ClientId);
            Assert.Equal(testClient.ClientName, result.ClientName);
        }

        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnAllClients()
        {
            // Arrange
            var clients = _clientFaker.Generate(3);
            _context.Clients.AddRange(clients);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.GetAllClientsAsync();

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task SearchClientsAsync_ShouldReturnMatchingClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                new Client { ClientName = "ABC Corporation", Email = "abc@corp.com", Address = "123 Main St", Phone = "555-1234" },
                new Client { ClientName = "XYZ Industries", Email = "xyz@ind.com", Address = "456 Oak Ave", Phone = "555-5678" },
                new Client { ClientName = "ABC Holdings", Email = "holdings@abc.com", Address = "789 Pine Rd", Phone = "555-9012" }
            };

            _context.Clients.AddRange(clients);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.SearchClientsAsync("ABC");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.Contains("ABC", c.ClientName));
        }

        [Fact]
        public async Task UpdateClientAsync_ShouldUpdateClient_WhenValidDataProvided()
        {
            // Arrange
            var testClient = _clientFaker.Generate();
            _context.Clients.Add(testClient);
            await _context.SaveChangesAsync();

            testClient.ClientName = "Updated Client Name";
            testClient.Email = "updated@email.com";

            // Act
            var result = await _clientService.UpdateClientAsync(testClient);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Client Name", result.ClientName);
            Assert.Equal("updated@email.com", result.Email);
        }

        [Fact]
        public async Task DeleteClientAsync_ShouldDeleteClient_WhenClientExists()
        {
            // Arrange
            var testClient = _clientFaker.Generate();
            _context.Clients.Add(testClient);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.DeleteClientAsync(testClient.ClientId);

            // Assert
            Assert.True(result);
            var deletedClient = await _context.Clients.FindAsync(testClient.ClientId);
            Assert.Null(deletedClient);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}