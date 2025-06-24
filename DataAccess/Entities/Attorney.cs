using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using Litigator.DataAccess.Enums;

namespace Litigator.DataAccess.Entities
{
    public class Attorney
    {
        public int AttorneyId { get; set; }
        [Required, MaxLength(100)]
        public required string FirstName { get; set; }
        [Required, MaxLength(100)]
        public required string LastName { get; set; }
        [MaxLength(50)]
        public required string BarNumber { get; set; }
        [MaxLength(100)]
        public required string Email { get; set; }
        [MaxLength(20)]
        public required string Phone { get; set; }

        public LegalSpecialization Specialization { get; set; } = LegalSpecialization.GeneralPractice;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}