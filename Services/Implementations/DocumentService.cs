using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Document;
using Litigator.Services.Interfaces;

namespace Litigator.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;

        public DocumentService(LitigatorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DocumentDTO>> GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.Case)
                .Select(d => new DocumentDTO
                {
                    DocumentId = d.DocumentId,
                    DocumentName = d.DocumentName,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    UploadDate = d.UploadDate,
                    FileSize = d.FileSize,
                    UploadedBy = d.UploadedBy,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentDTO>> GetDocumentsByCaseAsync(int caseId)
        {
            return await _context.Documents
                .Include(d => d.Case)
                .Where(d => d.CaseId == caseId)
                .Select(d => new DocumentDTO
                {
                    DocumentId = d.DocumentId,
                    DocumentName = d.DocumentName,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    UploadDate = d.UploadDate,
                    FileSize = d.FileSize,
                    UploadedBy = d.UploadedBy,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<DocumentDTO?> GetDocumentByIdAsync(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            return document == null ? null : _mapper.Map<DocumentDTO>(document);
        }


        public async Task<DocumentDTO> CreateDocumentAsync(DocumentCreateDTO createDto)
        {
            // Validate case exists
            var caseExists = await _context.Cases.AnyAsync(c => c.CaseId == createDto.CaseId);
            if (!caseExists)
            {
                throw new InvalidOperationException($"Case with ID {createDto.CaseId} not found.");
            }

            // Validate file path doesn't already exist
            if (!string.IsNullOrWhiteSpace(createDto.FilePath))
            {
                var existingDocument = await _context.Documents
                    .FirstOrDefaultAsync(d => d.FilePath == createDto.FilePath);
                if (existingDocument != null)
                {
                    throw new InvalidOperationException($"Document with file path {createDto.FilePath} already exists.");
                }
            }

            var document = _mapper.Map<Document>(createDto);
            document.UploadDate = DateTime.Now;

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            var createdDocument = await _context.Documents
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(d => d.DocumentId == document.DocumentId);

            return _mapper.Map<DocumentDTO>(createdDocument);
        }

        public async Task<DocumentDTO?> UpdateDocumentAsync(int id, DocumentUpdateDTO updateDto)
        {
            var existingDocument = await _context.Documents.FindAsync(id);
            if (existingDocument == null)
            {
                return null;
            }

            // Map the update DTO to the existing entity (preserving fields not in the DTO)
            _mapper.Map(updateDto, existingDocument);

            await _context.SaveChangesAsync();

            var updatedDocument = await _context.Documents
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            return _mapper.Map<DocumentDTO>(updatedDocument);
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return false;
            }

            // In a real application, I'd have to delete the actual file
            // from the file system if one existed, but this deletes it from memory.

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DocumentDTO>> SearchDocumentsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            var documents = await _context.Documents
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

            return _mapper.Map<IEnumerable<DocumentDTO>>(documents);
        }

        public async Task<IEnumerable<DocumentDTO>> GetDocumentsByTypeAsync(string documentType)
        {
            return await _context.Documents
                .Include(d => d.Case)
                .Where(d => d.DocumentType.ToLower() == documentType.ToLower())
                .Select(d => new DocumentDTO
                {
                    DocumentId = d.DocumentId,
                    DocumentName = d.DocumentName,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    UploadDate = d.UploadDate,
                    FileSize = d.FileSize,
                    UploadedBy = d.UploadedBy,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }
    }
}