using Microsoft.AspNetCore.Mvc;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound($"Document with ID {id} not found.");

            return Ok(document);
        }

        [HttpGet("case/{caseId}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByCase(int caseId)
        {
            var documents = await _documentService.GetDocumentsByCaseAsync(caseId);
            return Ok(documents);
        }

        [HttpGet("type/{documentType}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByType(string documentType)
        {
            var documents = await _documentService.GetDocumentsByTypeAsync(documentType);
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
        public async Task<ActionResult<Document>> CreateDocument([FromBody] Document document)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdDocument = await _documentService.CreateDocumentAsync(document);
                return CreatedAtAction(nameof(GetDocument), new { id = createdDocument.DocumentId }, createdDocument);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating document: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Document>> UpdateDocument(int id, [FromBody] Document document)
        {
            if (id != document.DocumentId)
                return BadRequest("Document ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedDocument = await _documentService.UpdateDocumentAsync(document);
                return Ok(updatedDocument);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating document: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(int id)
        {
            var result = await _documentService.DeleteDocumentAsync(id);
            if (!result)
                return NotFound($"Document with ID {id} not found.");

            return NoContent();
        }
    }
}