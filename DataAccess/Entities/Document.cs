using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Litigator.DataAccess.Entities
{
	public class Document
	{
		public int DocumentId { get; set; }
		[Required, MaxLength(200)]
		public required string DocumentName { get; set; }
		[MaxLength(50)]
		public required string DocumentType { get; set; } // Pleading, Motion, Contract, etc.
		[MaxLength(500)]
		public required string FilePath { get; set; }
		public string? Description { get; set; }
		public DateTime UploadDate { get; set; } = DateTime.Now;
		public long FileSize { get; set; }
		[MaxLength(100)]
		public required string UploadedBy { get; set; }

		// Foreign Key
		public int CaseId { get; set; }

		// Navigation property
		[System.Text.Json.Serialization.JsonIgnore]
		public virtual required Case Case { get; set; }
	}
}