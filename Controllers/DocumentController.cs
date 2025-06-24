using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Document;
using Litigator.Services.Interfaces;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDTO>>> GetAllDocuments()
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDTO>> GetDocument(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();
            return Ok(document);
        }

        [HttpGet("case/{caseId}")]
        public async Task<ActionResult<IEnumerable<DocumentDTO>>> GetDocumentsByCase(int caseId)
        {
            var documents = await _documentService.GetDocumentsByCaseAsync(caseId);
            return Ok(documents);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Document>>> SearchDocuments([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required.");

            var documents = await _documentService.SearchDocumentsAsync(searchTerm);
            return Ok(documents);
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDTO>> CreateDocument(DocumentCreateDTO createDto)
        {
            var document = await _documentService.CreateDocumentAsync(createDto);
            return CreatedAtAction(nameof(GetDocument), new { id = document.DocumentId }, document);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentDTO>> UpdateDocument(int id, DocumentUpdateDTO updateDto)
        {
            var document = await _documentService.UpdateDocumentAsync(id, updateDto);
            if (document == null)
                return NotFound();
            return Ok(document);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(int id)
        {
            var result = await _documentService.DeleteDocumentAsync(id);
            if (!result)
                return NotFound($"Document with ID {id} not found.");

            return NoContent();
        }

        [HttpGet("type/{documentType}")]
        public async Task<ActionResult<IEnumerable<DocumentDTO>>> GetDocumentsByType(string documentType)
        {
            var documents = await _documentService.GetDocumentsByTypeAsync(documentType);
            return Ok(documents);
        }
    }
}