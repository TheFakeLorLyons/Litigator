namespace Litigator.Models.DTOs.ClassDTOs
{
    public class DeadlineDTO
    {
        public int DeadlineId { get; set; }
        public required string DeadlineType { get; set; }
        public string? Description { get; set; }
        public DateTime DeadlineDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCritical { get; set; }
        public int CaseId { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseTitle { get; set; }
    }

    public class DeadlineCreateDTO
    {
        public required string DeadlineType { get; set; }
        public string? Description { get; set; }
        public DateTime DeadlineDate { get; set; }
        public bool IsCritical { get; set; }
        public int CaseId { get; set; }
    }

    public class DeadlineUpdateDTO
    {
        public int? DeadlineId { get; set; }
        public required string DeadlineType { get; set; }
        public string? Description { get; set; }
        public DateTime DeadlineDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCritical { get; set; }
    }
}