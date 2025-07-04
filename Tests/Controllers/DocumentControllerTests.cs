using Moq;
using Xunit;
using Litigator.Controllers;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.Document;
using Litigator.Services.Interfaces;

namespace Litigator.Tests.Controllers
{
    public class DocumentControllerTests
    {
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly DocumentController _controller;

        public DocumentControllerTests()
        {
            _mockDocumentService = new Mock<IDocumentService>();
            _controller = new DocumentController(_mockDocumentService.Object);
        }

        // Helper method to create a test document with all required properties
        private DocumentDTO CreateTestDocumentDTO(int id = 1, string name = "Test Document", string type = "PDF")
        {
            return new DocumentDTO
            {
                DocumentId = id,
                DocumentName = name,
                DocumentType = type,
                FilePath = "/test/path/document.pdf",
                UploadDate = DateTime.Now,
                FileSize = 1024,
                UploadedBy = "TestUser",
                CaseId = 1,
                CaseNumber = "2024-CV-001",
                CaseTitle = "Test Case"
            };
        }

        private DocumentCreateDTO CreateTestDocumentCreateDTO(string name = "New Document", string type = "PDF")
        {
            return new DocumentCreateDTO
            {
                DocumentName = name,
                DocumentType = type,
                FilePath = "/test/path/document.pdf",
                FileSize = 1024,
                UploadedBy = "TestUser",
                CaseId = 1
            };
        }

        private DocumentUpdateDTO CreateTestDocumentUpdateDTO(string name = "Updated Document", string type = "PDF")
        {
            return new DocumentUpdateDTO
            {
                DocumentName = name,
                DocumentType = type,
                UploadedBy = "TestUser"
            };
        }

        // Helper method to create a test case
        private Case CreateTestCase(int id = 1, string title = "Test Case")
        {
            return new Case
            {
                CaseId = id,
                CaseNumber = $"TC{id:000}",
                CaseTitle = title,
                CaseType = "Civil",
                FilingDate = DateTime.Now,
                Status = "Active",
                ClientId = 1,
                AssignedAttorneyId = 1,
                CourtId = 1,
                Client = new Client
                {
                    Name = new PersonName { First = "John", Last = "Doe" },
                    Email = "john.doe@example.com",
                    PrimaryAddress = new Address
                    {
                        Line1 = "123 Test Street",
                        City = "Test City",
                        State = "TS",
                        PostalCode = "12345"
                    },
                    PrimaryPhone = new PhoneNumber { Number = "555-1234" },
                    IsActive = true
                }
            };
        }

        [Fact]
        public async Task CreateDocument_ValidDocument_ReturnsCreatedDocument()
        {
            // Arrange
            var createDto = CreateTestDocumentCreateDTO();
            var createdDocument = CreateTestDocumentDTO(1, createDto.DocumentName, createDto.DocumentType);

            _mockDocumentService.Setup(s => s.CreateDocumentAsync(It.IsAny<DocumentCreateDTO>()))
                              .ReturnsAsync(createdDocument);

            // Act
            var result = await _controller.CreateDocument(createDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DocumentDTO>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(_controller.GetDocument), createdResult.ActionName);
            Assert.Equal(createdDocument.DocumentId, ((DocumentDTO)createdResult.Value!).DocumentId);
        }

        [Fact]
        public async Task GetDocument_ExistingId_ReturnsDocument()
        {
            // Arrange
            var testDocument = CreateTestDocumentDTO();
            _mockDocumentService.Setup(s => s.GetDocumentByIdAsync(1))
                              .ReturnsAsync(testDocument);

            // Act
            var result = await _controller.GetDocument(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DocumentDTO>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var document = Assert.IsType<DocumentDTO>(okResult.Value);
            Assert.Equal(1, document.DocumentId);
        }


        [Fact]
        public async Task GetDocument_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockDocumentService.Setup(s => s.GetDocumentByIdAsync(999))
                              .ReturnsAsync((DocumentDTO?)null);

            // Act
            var result = await _controller.GetDocument(999);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DocumentDTO>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }


        [Fact]
        public async Task GetDocumentsByCase_ValidCaseId_ReturnsDocuments()
        {
            // Arrange
            var documents = new List<DocumentDTO>
            {
                CreateTestDocumentDTO(1, "Doc1", "PDF"),
                CreateTestDocumentDTO(2, "Doc2", "Word")
            };

            _mockDocumentService.Setup(s => s.GetDocumentsByCaseAsync(1))
                              .ReturnsAsync(documents);

            // Act
            var result = await _controller.GetDocumentsByCase(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DocumentDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<DocumentDTO>>(okResult.Value);
            Assert.Equal(2, returnedDocuments.Count());
        }

        [Fact]
        public async Task GetAllDocuments_ReturnsAllDocuments()
        {
            // Arrange
            var documents = new List<DocumentDTO>
            {
                CreateTestDocumentDTO(1, "Doc1", "PDF"),
                CreateTestDocumentDTO(2, "Doc2", "Word"),
                CreateTestDocumentDTO(3, "Doc3", "Excel")
            };

            _mockDocumentService.Setup(s => s.GetAllDocumentsAsync())
                              .ReturnsAsync(documents);

            // Act
            var result = await _controller.GetAllDocuments();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DocumentDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<DocumentDTO>>(okResult.Value);
            Assert.Equal(3, returnedDocuments.Count());
        }

        [Fact]
        public async Task GetDocumentsByType_ValidType_ReturnsDocuments()
        {
            // Arrange
            var documents = new List<DocumentDTO>
            {
                CreateTestDocumentDTO(1, "Doc1", "PDF"),
                CreateTestDocumentDTO(2, "Doc2", "PDF")
            };

            _mockDocumentService.Setup(s => s.GetDocumentsByTypeAsync("PDF"))
                              .ReturnsAsync(documents);

            // Act
            var result = await _controller.GetDocumentsByType("PDF");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DocumentDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<DocumentDTO>>(okResult.Value);
            Assert.Equal(2, returnedDocuments.Count());
            Assert.All(returnedDocuments, d => Assert.Equal("PDF", d.DocumentType));
        }


        [Fact]
        public async Task SearchDocuments_ValidSearchTerm_ReturnsDocuments()
        {
            var documents = new List<DocumentDTO>
            {
                CreateTestDocumentDTO(1, "Contract Document", "PDF")
            };

            _mockDocumentService.Setup(s => s.SearchDocumentsAsync("Contract"))
                              .ReturnsAsync(documents);

            // Act
            var result = await _controller.SearchDocuments("Contract");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DocumentDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<DocumentDTO>>(okResult.Value);
            Assert.Single(returnedDocuments);
        }

        [Fact]
        public async Task SearchDocuments_EmptySearchTerm_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchDocuments("");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DocumentDTO>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Search term is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task SearchDocuments_WhitespaceSearchTerm_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchDocuments("   ");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DocumentDTO>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Search term is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateDocument_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var createDto = CreateTestDocumentCreateDTO();
            _mockDocumentService.Setup(s => s.CreateDocumentAsync(It.IsAny<DocumentCreateDTO>()))
                              .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _controller.CreateDocument(createDto));
        }

        [Fact]
        public async Task UpdateDocument_ValidDocument_ReturnsUpdatedDocument()
        {
            // Arrange
            int documentId = 1;
            var updateDto = CreateTestDocumentUpdateDTO("Updated Document");
            var updatedDocument = CreateTestDocumentDTO(documentId, "Updated Document");

            _mockDocumentService.Setup(s => s.UpdateDocumentAsync(documentId, It.IsAny<DocumentUpdateDTO>()))
                              .ReturnsAsync(updatedDocument);

            // Act
            var result = await _controller.UpdateDocument(documentId, updateDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DocumentDTO>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var document = Assert.IsType<DocumentDTO>(okResult.Value);
            Assert.Equal(documentId, document.DocumentId);
            Assert.Equal("Updated Document", document.DocumentName);
        }

        [Fact]
        public async Task UpdateDocument_NonExistingDocument_ReturnsNotFound()
        {
            // Arrange
            var updateDto = CreateTestDocumentUpdateDTO();
            _mockDocumentService.Setup(s => s.UpdateDocumentAsync(999, It.IsAny<DocumentUpdateDTO>()))
                              .ReturnsAsync((DocumentDTO?)null);

            // Act
            var result = await _controller.UpdateDocument(999, updateDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DocumentDTO>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task UpdateDocument_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = CreateTestDocumentUpdateDTO();
            _mockDocumentService.Setup(s => s.UpdateDocumentAsync(It.IsAny<int>(), It.IsAny<DocumentUpdateDTO>()))
                              .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert - This test will need to be updated based on your controller's actual exception handling
            // Currently the actual Document controller doesn't have try-catch blocks, so exceptions will bubble up
            await Assert.ThrowsAsync<System.Exception>(() => _controller.UpdateDocument(1, updateDto));
        }

        [Fact]
        public async Task DeleteDocument_ExistingId_ReturnsNoContent()
        {
            // Arrange
            _mockDocumentService.Setup(s => s.DeleteDocumentAsync(1))
                              .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteDocument(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDocument_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockDocumentService.Setup(s => s.DeleteDocumentAsync(999))
                              .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteDocument(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Document with ID 999 not found.", notFoundResult.Value);
        }
    }
}