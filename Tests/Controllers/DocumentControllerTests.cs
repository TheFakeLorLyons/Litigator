using Microsoft.AspNetCore.Mvc;
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
        private Document CreateTestDocument(int id = 1, string name = "Test Document", string type = "PDF")
        {
            return new Document
            {
                DocumentId = id,
                DocumentName = name,
                DocumentType = type,
                FilePath = "/test/path/document.pdf",
                UploadedBy = "TestUser",
                Case = CreateTestCase(1, "Test Case"),
                CaseId = 1,
                UploadDate = DateTime.Now
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
                    ClientId = 1,
                    ClientName = "Test Client",
                    Address = "123 Test Street, Test City, TS 12345",
                    Phone = "555-1234",
                    Email = "test@example.com"
                }
            };
        }

        [Fact]
        public async Task GetDocument_ExistingId_ReturnsDocument()
        {
            // Arrange
            var testDocument = CreateTestDocument();
            _mockDocumentService.Setup(s => s.GetDocumentByIdAsync(1))
                              .ReturnsAsync(testDocument);

            // Act
            var result = await _controller.GetDocument(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Document>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var document = Assert.IsType<Document>(okResult.Value);
            Assert.Equal(1, document.DocumentId);
        }

        [Fact]
        public async Task GetDocument_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockDocumentService.Setup(s => s.GetDocumentByIdAsync(999))
                              .ReturnsAsync((Document?)null);

            // Act
            var result = await _controller.GetDocument(999);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Document>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("Document with ID 999 not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetDocumentsByCase_ValidCaseId_ReturnsDocuments()
        {
            // Arrange
            var testCase = CreateTestCase();
            var documents = new List<Document>
            {
                CreateTestDocument(1, "Doc1", "PDF"),
                CreateTestDocument(2, "Doc2", "Word")
            };

            _mockDocumentService.Setup(s => s.GetDocumentsByCaseAsync(1))
                              .ReturnsAsync(documents);

            // Act
            var result = await _controller.GetDocumentsByCase(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Document>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<Document>>(okResult.Value);
            Assert.Equal(2, returnedDocuments.Count());
        }

        [Fact]
        public async Task GetDocumentsByType_ValidType_ReturnsDocuments()
        {
            // Arrange
            var documents = new List<Document>
            {
                CreateTestDocument(1, "Doc1", "PDF"),
                CreateTestDocument(2, "Doc2", "PDF")
            };

            _mockDocumentService.Setup(s => s.GetDocumentsByTypeAsync("PDF"))
                              .ReturnsAsync(documents);

            // Act
            var result = await _controller.GetDocumentsByType("PDF");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Document>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<Document>>(okResult.Value);
            Assert.Equal(2, returnedDocuments.Count());
        }

        [Fact]
        public async Task SearchDocuments_ValidSearchTerm_ReturnsDocuments()
        {
            // Arrange
            var documents = new List<Document>
            {
                CreateTestDocument(1, "Contract Document", "PDF")
            };

            _mockDocumentService.Setup(s => s.SearchDocumentsAsync("Contract"))
                              .ReturnsAsync(documents);

            // Act
            var result = await _controller.SearchDocuments("Contract");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Document>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<Document>>(okResult.Value);
            Assert.Single(returnedDocuments);
        }

        [Fact]
        public async Task SearchDocuments_EmptySearchTerm_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchDocuments("");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Document>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Search term is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task SearchDocuments_WhitespaceSearchTerm_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchDocuments("   ");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Document>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Search term is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateDocument_ValidDocument_ReturnsCreatedDocument()
        {
            // Arrange
            var newDocument = CreateTestDocument();
            var createdDocument = CreateTestDocument();

            _mockDocumentService.Setup(s => s.CreateDocumentAsync(It.IsAny<Document>()))
                              .ReturnsAsync(createdDocument);

            // Act
            var result = await _controller.CreateDocument(newDocument);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Document>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(_controller.GetDocument), createdResult.ActionName);
            Assert.Equal(createdDocument.DocumentId, ((Document)createdResult.Value!).DocumentId);
        }

        [Fact]
        public async Task CreateDocument_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var newDocument = CreateTestDocument();
            _mockDocumentService.Setup(s => s.CreateDocumentAsync(It.IsAny<Document>()))
                              .ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _controller.CreateDocument(newDocument);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Document>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Contains("Error creating document", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task UpdateDocument_ValidDocument_ReturnsUpdatedDocument()
        {
            // Arrange
            var documentToUpdate = CreateTestDocument();
            var updatedDocument = CreateTestDocument();

            _mockDocumentService.Setup(s => s.UpdateDocumentAsync(It.IsAny<Document>()))
                              .ReturnsAsync(updatedDocument);

            // Act
            var result = await _controller.UpdateDocument(1, documentToUpdate);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Document>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var document = Assert.IsType<Document>(okResult.Value);
            Assert.Equal(1, document.DocumentId);
        }

        [Fact]
        public async Task UpdateDocument_MismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var documentToUpdate = CreateTestDocument(2); // Different ID

            // Act
            var result = await _controller.UpdateDocument(1, documentToUpdate);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Document>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Document ID mismatch.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateDocument_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var documentToUpdate = CreateTestDocument();
            _mockDocumentService.Setup(s => s.UpdateDocumentAsync(It.IsAny<Document>()))
                              .ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _controller.UpdateDocument(1, documentToUpdate);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Document>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Contains("Error updating document", badRequestResult.Value?.ToString());
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