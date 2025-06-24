using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Litigator.DataAccess.Entities
{
    public class Court
    {
        public int CourtId { get; set; }
        [Required, MaxLength(200)]
        public required string CourtName { get; set; }
        [MaxLength(100)]
        public required string Address { get; set; }
        [MaxLength(100)]
        public string? County { get; set; }
        [MaxLength(50)]
        public required string State { get; set; }
        [MaxLength(20)]
        public required string CourtType { get; set; } // Federal, State, Municipal

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}