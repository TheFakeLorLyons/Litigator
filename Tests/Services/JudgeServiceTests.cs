using AutoMapper;
using FluentAssertions;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.Mapping;
using Litigator.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace Litigator.Tests.Services
{
    public class JudgeServiceTests : IDisposable
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;
        private readonly JudgeService _judgeService;

        public JudgeServiceTests()
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

            _judgeService = new JudgeService(_context, _mapper);
        }

        [Fact]
        public async Task GetAllJudgesAsync_ReturnsAllJudges_OrderedByName()
        {
            // Arrange
            var judges = new List<Judge>
            {
                CreateTestJudge(1, "John", "Smith", "BAR001"),
                CreateTestJudge(2, "Jane", "Adams", "BAR002"),
                CreateTestJudge(3, "Bob", "Wilson", "BAR003")
            };

            _context.Judges.AddRange(judges);
            await _context.SaveChangesAsync();

            // Act
            var result = await _judgeService.GetAllJudgesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            var judgeList = result.ToList();

            // Assuming judges are ordered by last name
            judgeList.First().Name.Should().Contain("Adams");
            judgeList.Skip(1).First().Name.Should().Contain("Smith");
            judgeList.Last().Name.Should().Contain("Wilson");
        }

        [Fact]
        public async Task GetJudgeByIdAsync_ExistingId_ReturnsJudgeDetail()
        {
            // Arrange
            var judge = CreateTestJudge(1, "John", "Smith", "BAR001");
            _context.Judges.Add(judge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _judgeService.GetJudgeByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.JudgeId.Should().Be(1);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Smith");
            result.BarNumber.Should().Be("BAR001");
        }

        [Fact]
        public async Task GetJudgeByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _judgeService.GetJudgeByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetJudgeByBarNumberAsync_ExistingBarNumber_ReturnsJudgeDetail()
        {
            // Arrange
            var judge = CreateTestJudge(1, "John", "Smith", "BAR001");
            _context.Judges.Add(judge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _judgeService.GetJudgeByBarNumberAsync("BAR001");

            // Assert
            result.Should().NotBeNull();
            result!.BarNumber.Should().Be("BAR001");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Smith");
        }

        [Fact]
        public async Task GetJudgeByBarNumberAsync_NonExistingBarNumber_ReturnsNull()
        {
            // Act
            var result = await _judgeService.GetJudgeByBarNumberAsync("NONEXISTENT");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveJudgesAsync_ReturnsOnlyActiveJudges()
        {
            // Arrange
            var judges = new List<Judge>
            {
                CreateTestJudge(1, "John", "Smith", "BAR001", isActive: true),
                CreateTestJudge(2, "Jane", "Adams", "BAR002", isActive: false),
                CreateTestJudge(3, "Bob", "Wilson", "BAR003", isActive: true)
            };

            _context.Judges.AddRange(judges);
            await _context.SaveChangesAsync();

            // Act
            var result = await _judgeService.GetActiveJudgesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(j => j.IsActive).Should().BeTrue();
        }

        [Fact]
        public async Task CreateJudgeAsync_ValidJudge_ReturnsCreatedJudge()
        {
            // Arrange
            var judgeDto = new JudgeDetailDTO
            {
                DisplayName = "John Smith",
                ProfessionalName = "Judge John Smith",
                FullName = "John Smith",
                FirstName = "John",
                LastName = "Smith",
                BarNumber = "BAR001",
                Email = "john.smith@court.gov",
                IsActive = true
            };

            // Act
            var result = await _judgeService.CreateJudgeAsync(judgeDto);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Smith");
            result.BarNumber.Should().Be("BAR001");
            result.Email.Should().Be("john.smith@court.gov");
            result.IsActive.Should().BeTrue();
            result.JudgeId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateJudgeAsync_DuplicateBarNumber_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingJudge = CreateTestJudge(1, "John", "Smith", "BAR001");
            _context.Judges.Add(existingJudge);
            await _context.SaveChangesAsync();

            var judgeDto = new JudgeDetailDTO
            {
                DisplayName = "Jane Adams",
                ProfessionalName = "Judge Jane Adams",
                FullName = "Jane Adams",
                FirstName = "Jane",
                LastName = "Adams",
                BarNumber = "BAR001",
                Email = "jane.adams@court.gov"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _judgeService.CreateJudgeAsync(judgeDto));

            exception.Message.Should().Contain("Judge with bar number BAR001 already exists");
        }

        [Fact]
        public async Task CreateJudgeAsync_DuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingJudge = CreateTestJudge(1, "John", "Smith", "BAR001", email: "duplicate@court.gov");
            _context.Judges.Add(existingJudge);
            await _context.SaveChangesAsync();

            var judgeDto = new JudgeDetailDTO
            {
                DisplayName = "Jane Adams",
                ProfessionalName = "Judge Jane Adams",
                FullName = "Jane Adams",
                FirstName = "Jane",
                LastName = "Adams",
                BarNumber = "BAR002",
                Email = "duplicate@court.gov"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _judgeService.CreateJudgeAsync(judgeDto));

            exception.Message.Should().Contain("Judge with email duplicate@court.gov already exists");
        }

        [Fact]
        public async Task UpdateJudgeAsync_ValidUpdate_ReturnsUpdatedJudge()
        {
            // Arrange
            var judge = CreateTestJudge(1, "John", "Smith", "BAR001");
            _context.Judges.Add(judge);
            await _context.SaveChangesAsync();

            var updateDto = new JudgeDetailDTO
            {
                JudgeId = 1,
                DisplayName = "John Johnson",
                ProfessionalName = "Judge John Johnson",
                FullName = "John Johnson",
                FirstName = "John",
                LastName = "Johnson",
                BarNumber = "BAR001",
                Email = "john.johnson@court.gov",
                IsActive = true
            };

            // Act
            var result = await _judgeService.UpdateJudgeAsync(updateDto);

            // Assert
            result.Should().NotBeNull();
            result.JudgeId.Should().Be(1);
            result.LastName.Should().Be("Johnson");
            result.Email.Should().Be("john.johnson@court.gov");
        }

        [Fact]
        public async Task UpdateJudgeAsync_NonExistingJudge_ThrowsInvalidOperationException()
        {
            // Arrange
            var updateDto = new JudgeDetailDTO
            {
                JudgeId = 999,
                DisplayName = "John Smith",
                ProfessionalName = "Judge John Smith",
                FullName = "John Smith",
                FirstName = "John",
                LastName = "Smith",
                BarNumber = "BAR001"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _judgeService.UpdateJudgeAsync(updateDto));

            exception.Message.Should().Contain("Judge with ID 999 not found");
        }

        [Fact]
        public async Task UpdateJudgeAsync_DuplicateBarNumber_ThrowsInvalidOperationException()
        {
            // Arrange
            var judge1 = CreateTestJudge(1, "John", "Smith", "BAR001");
            var judge2 = CreateTestJudge(2, "Jane", "Adams", "BAR002");
            _context.Judges.AddRange(judge1, judge2);
            await _context.SaveChangesAsync();

            var updateDto = new JudgeDetailDTO
            {
                JudgeId = 2,
                DisplayName = "Jane Adams",
                ProfessionalName = "Judge Jane Adams",
                FullName = "Jane Adams",
                FirstName = "Jane",
                LastName = "Adams",
                BarNumber = "BAR001" // Trying to use judge1's bar number
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _judgeService.UpdateJudgeAsync(updateDto));

            exception.Message.Should().Contain("Judge with bar number BAR001 already exists");
        }

        [Fact]
        public async Task DeleteJudgeAsync_ExistingJudgeWithoutActiveCases_ReturnsTrue()
        {
            // Arrange
            var judge = CreateTestJudge(1, "John", "Smith", "BAR001");
            _context.Judges.Add(judge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _judgeService.DeleteJudgeAsync(1);

            // Assert
            result.Should().BeTrue();
            var deletedJudge = await _context.Judges.FindAsync(1);
            deletedJudge.Should().BeNull();
        }

        [Fact]
        public async Task DeleteJudgeAsync_NonExistingJudge_ReturnsFalse()
        {
            // Act
            var result = await _judgeService.DeleteJudgeAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetJudgeEntityByIdAsync_ExistingId_ReturnsJudgeEntity()
        {
            // Arrange
            var judge = CreateTestJudge(1, "John", "Smith", "BAR001");
            _context.Judges.Add(judge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _judgeService.GetJudgeEntityByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Judge>();
            result!.JudgeId.Should().Be(judge.JudgeId);
            result.Name.First.Should().Be("John");
            result.Name.Last.Should().Be("Smith");
        }

        [Fact]
        public async Task GetJudgeEntityByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _judgeService.GetJudgeEntityByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        private static Judge CreateTestJudge(int id, string firstName, string lastName, string barNumber,
            bool isActive = true, string? email = null)
        {
            return new Judge
            {
                SystemId = id,
                Name = PersonName.Create(firstName, lastName), // Now uses the correct 2-parameter Create method
                BarNumber = barNumber,
                PrimaryPhone = PhoneNumber.Create("617-915-3333"),
                PrimaryAddress = Address.Create("456 Judge Blvd", "Law City", "NY", "10001"),
                Email = email,
                IsActive = isActive,
                CreatedDate = DateTime.UtcNow
            };
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}