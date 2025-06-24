using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Litigator.DataAccess.Entities
{
    public class Deadline
    {
        public int DeadlineId { get; set; }
        [Required, MaxLength(100)]
        public required string DeadlineType { get; set; } // Discovery, Filing, Hearing, etc.
        [MaxLength(500)]
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public DateTime DeadlineDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public bool IsCritical { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        // Foreign Key
        public int CaseId { get; set; }

        // Navigation property
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual required Case Case { get; set; }
    }
}