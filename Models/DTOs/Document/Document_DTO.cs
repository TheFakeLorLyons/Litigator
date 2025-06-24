namespace Litigator.Models.DTOs.Document
{
    public class DocumentDTO
    {
        public int DocumentId { get; set; }
        public required string DocumentName { get; set; }
        public required string DocumentType { get; set; }
        public required string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public long FileSize { get; set; }
        public string? UploadedBy { get; set; }
        public int CaseId { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseTitle { get; set; }
    }

    public class DocumentCreateDTO
    {
        public required string DocumentName { get; set; }
        public required string DocumentType { get; set; }
        public required string FilePath { get; set; }
        public long FileSize { get; set; }
        public string? UploadedBy { get; set; }
        public int CaseId { get; set; }
    }

    public class DocumentUpdateDTO
    {
        public required string DocumentName { get; set; }
        public required string DocumentType { get; set; }
        public string? UploadedBy { get; set; }
    }
}