using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> GetDocumentsByCaseAsync(int caseId);
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<Document> CreateDocumentAsync(Document document);
        Task<Document> UpdateDocumentAsync(Document document);
        Task<bool> DeleteDocumentAsync(int id);
        Task<IEnumerable<Document>> SearchDocumentsAsync(string searchTerm);
        Task<IEnumerable<Document>> GetDocumentsByTypeAsync(string documentType);
    }
}