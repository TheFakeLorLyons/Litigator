using Litigator.Services.Interfaces;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Litigator.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly LitigatorDbContext _context;

        public DocumentService(LitigatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetDocumentsByCaseAsync(int caseId)
        {
            return await _context.Documents
                .Include(d => d.Case)
                .Where(d => d.CaseId == caseId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(d => d.DocumentId == id);
        }

        public async Task<Document> CreateDocumentAsync(Document document)
        {
            // Validate case exists
            var caseExists = await _context.Cases.AnyAsync(c => c.CaseId == document.CaseId);
            if (!caseExists)
            {
                throw new InvalidOperationException($"Case with ID {document.CaseId} not found.");
            }

            // Validate file path doesn't already exist
            if (!string.IsNullOrWhiteSpace(document.FilePath))
            {
                var existingDocument = await _context.Documents
                    .FirstOrDefaultAsync(d => d.FilePath == document.FilePath);
                if (existingDocument != null)
                {
                    throw new InvalidOperationException($"Document with file path {document.FilePath} already exists.");
                }
            }

            document.UploadDate = DateTime.Now;
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return await GetDocumentByIdAsync(document.DocumentId) ?? document;
        }

        public async Task<Document> UpdateDocumentAsync(Document document)
        {
            var existingDocument = await _context.Documents.FindAsync(document.DocumentId);
            if (existingDocument == null)
            {
                throw new InvalidOperationException($"Document with ID {document.DocumentId} not found.");
            }

            // Validate case exists if being changed
            if (existingDocument.CaseId != document.CaseId)
            {
                var caseExists = await _context.Cases.AnyAsync(c => c.CaseId == document.CaseId);
                if (!caseExists)
                {
                    throw new InvalidOperationException($"Case with ID {document.CaseId} not found.");
                }
            }

            // Check file path uniqueness if being changed
            if (!string.IsNullOrWhiteSpace(document.FilePath) &&
                existingDocument.FilePath != document.FilePath)
            {
                var duplicateDocument = await _context.Documents
                    .FirstOrDefaultAsync(d => d.FilePath == document.FilePath);
                if (duplicateDocument != null)
                {
                    throw new InvalidOperationException($"Document with file path {document.FilePath} already exists.");
                }
            }

            _context.Entry(existingDocument).CurrentValues.SetValues(document);
            await _context.SaveChangesAsync();

            return await GetDocumentByIdAsync(document.DocumentId) ?? document;
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return false;
            }

            // Note: In a real application, you'd also want to delete the physical file
            // from the file system here

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Document>> SearchDocumentsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Documents
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Where(d =>
                    d.DocumentName.ToLower().Contains(term) ||
                    d.DocumentType.ToLower().Contains(term) ||
                    (d.UploadedBy != null && d.UploadedBy.ToLower().Contains(term)) ||
                    d.Case.CaseNumber.ToLower().Contains(term) ||
                    d.Case.CaseTitle.ToLower().Contains(term))
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetDocumentsByTypeAsync(string documentType)
        {
            return await _context.Documents
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Where(d => d.DocumentType.ToLower() == documentType.ToLower())
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }
    }
}