using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDTO>> GetDocumentsByCaseAsync(int caseId);
        Task<IEnumerable<DocumentDTO>> GetAllDocumentsAsync();
        Task<DocumentDTO?> GetDocumentByIdAsync(int id);
        Task<DocumentDTO> CreateDocumentAsync(DocumentCreateDTO createDto);
        Task<DocumentDTO?> UpdateDocumentAsync(int id, DocumentUpdateDTO updateDto);
        Task<bool> DeleteDocumentAsync(int id);

        Task<IEnumerable<DocumentDTO>> GetDocumentsByTypeAsync(string documentType);
        Task<IEnumerable<DocumentDTO>> SearchDocumentsAsync(string searchTerm);
    }
}