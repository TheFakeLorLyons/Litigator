using AutoMapper;
using Bogus;
using Xunit;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.DTOs.Shared;
using Litigator.Models.Mapping;
using Litigator.Services.Implementations;

namespace Litigator.Tests.Services
{
    public class ClientServiceTests : IDisposable
    {
        private readonly LitigatorDbContext _context;
        private readonly ClientService _clientService;
        private readonly IMapper _mapper;
        private readonly Faker<ClientDetailDTO> _clientDtoFaker;

        public ClientServiceTests()
        {
            var options = new DbContextOptionsBuilder<LitigatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new LitigatorDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LitigatorMappingProfile>();
            });
            _mapper = config.CreateMapper();
            _clientService = new ClientService(_context, _mapper);

            // Setup Bogus faker for Client
            _clientDtoFaker = new Faker<ClientDetailDTO>()
                .RuleFor(c => c.ClientId, f => 0) // Will be set by database
                .RuleFor(c => c.FirstName, f => f.Name.FirstName())
                .RuleFor(c => c.LastName, f => f.Name.LastName())
                .RuleFor(c => c.MiddleName, f => f.Random.Bool(0.3f) ? f.Name.FirstName() : null)
                .RuleFor(c => c.Title, f => f.Random.Bool(0.2f) ? f.PickRandom("Mr.", "Ms.", "Dr.") : null)
                .RuleFor(c => c.Suffix, f => f.Random.Bool(0.1f) ? f.PickRandom("Jr.", "Sr.", "III") : null)
                .RuleFor(c => c.PreferredName, f => f.Random.Bool(0.2f) ? f.Name.FirstName() : null)
                .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FirstName, c.LastName))
                .RuleFor(c => c.PrimaryPhone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.PrimaryAddress, f => f.Address.FullAddress())
                .RuleFor(c => c.IsActive, f => f.Random.Bool(0.9f))
                .RuleFor(c => c.CreatedDate, f => f.Date.Past())
                .RuleFor(c => c.ModifiedDate, f => f.Date.Recent())
                .RuleFor(c => c.Notes, f => f.Lorem.Paragraph())
                // Set computed properties
                .RuleFor(c => c.DisplayName, (f, c) => $"{c.FirstName} {c.LastName}".Trim())
                .RuleFor(c => c.FullName, (f, c) => $"{c.FirstName} {c.LastName}")
                .RuleFor(c => c.AllAddresses, f => new List<AddressDTO>())
                .RuleFor(c => c.AllPhones, f => new List<PhoneNumberDTO>())
                .RuleFor(c => c.Attorneys, f => new List<AttorneyDTO>())
                .RuleFor(c => c.TotalCases, f => f.Random.Int(0, 50))
                .RuleFor(c => c.ActiveCases, f => f.Random.Int(0, 25))
                .RuleFor(c => c.ClosedCases, f => f.Random.Int(0, 25))
                .RuleFor(c => c.LastCaseDate, f => f.Date.Recent())
                .RuleFor(c => c.MostRecentCaseTitle, f => f.Lorem.Sentence());
        }

        // Helper method to create Client entity for direct database testing
        private Client CreateClientEntity(string firstName, string lastName, string email, bool isActive = true)
        {
            return new Client
            {
                Name = new PersonName
                {
                    First = firstName,
                    Last = lastName
                },
                Email = email,
                IsActive = isActive,
                PrimaryPhone = new PhoneNumber { Number = "555-1234" },
                PrimaryAddress = Address.Create("123 Main St", "Test City", "TX", "12345")
            };
        }

        [Fact]
        public async Task CreateClientAsync_ShouldCreateClient_WhenValidDataProvided()
        {
            // Arrange
            var newClientDto = _clientDtoFaker.Generate();

            // Act
            var result = await _clientService.CreateClientAsync(newClientDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ClientId > 0);
            Assert.Equal(newClientDto.FirstName, result.FirstName);
            Assert.Equal(newClientDto.LastName, result.LastName);
            Assert.Equal(newClientDto.Email, result.Email);

            var clientInDb = await _context.Clients.FindAsync(result.ClientId);
            Assert.NotNull(clientInDb);
        }

        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenClientExists()
        {
            // Arrange
            var testClient = CreateClientEntity("John", "Doe", "john.doe@email.com");
            _context.Clients.Add(testClient);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.GetClientByIdAsync(testClient.SystemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testClient.SystemId, result.ClientId);
            Assert.Equal(testClient.Name.First, result.FirstName);
            Assert.Equal(testClient.Name.Last, result.LastName);
        }

        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnAllClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                CreateClientEntity("Client", "One", "client1@email.com"),
                CreateClientEntity("Client", "Two", "client2@email.com"),
                CreateClientEntity("Client", "Three", "client3@email.com")
            };
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
                CreateClientEntity("ABC", "Corporation", "abc@corp.com"),
                CreateClientEntity("XYZ", "Industries", "xyz@ind.com"),
                CreateClientEntity("ABC", "Holdings", "holdings@abc.com")
            };

            _context.Clients.AddRange(clients);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.SearchClientsAsync("ABC");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.Contains("ABC", c.Name));
        }

        [Fact]
        public async Task UpdateClientAsync_ShouldUpdateClient_WhenValidDataProvided()
        {
            // Arrange
            var testClient = CreateClientEntity("Original", "Name", "original@email.com");
            _context.Clients.Add(testClient);
            await _context.SaveChangesAsync();

            var updateDto = new ClientDetailDTO
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@email.com",
                DisplayName = "Updated Name",
                FullName = "Updated Name",
                IsActive = testClient.IsActive,
                CreatedDate = testClient.CreatedDate,
                AllAddresses = new List<AddressDTO>(),
                AllPhones = new List<PhoneNumberDTO>(),
                Attorneys = new List<AttorneyDTO>()
            };

            // Act
            var result = await _clientService.UpdateClientAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result.FirstName);
            Assert.Equal("updated@email.com", result.Email);
        }

        [Fact]
        public async Task DeleteClientAsync_ShouldDeleteClient_WhenClientExists()
        {
            // Arrange
            var testClient = CreateClientEntity("Delete", "Me", "delete@email.com");
            _context.Clients.Add(testClient);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.DeleteClientAsync(testClient.SystemId);

            // Assert
            Assert.True(result);
            var deletedClient = await _context.Clients.FindAsync(testClient.SystemId);
            Assert.Null(deletedClient);
        }

        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnNull_WhenClientNotExists()
        {
            // Act
            var result = await _clientService.GetClientByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveClientsAsync_ShouldReturnOnlyActiveClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                CreateClientEntity("Active", "Client1", "active1@email.com", true),
                CreateClientEntity("Inactive", "Client2", "inactive2@email.com", false),
                CreateClientEntity("Active", "Client3", "active3@email.com", true)
            };

            _context.Clients.AddRange(clients);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clientService.GetActiveClientsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.True(c.IsActive));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}