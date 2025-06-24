using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Litigator.DataAccess.Entities
{
	public class Client
	{
		public int ClientId { get; set; }
		[Required, MaxLength(200)]
		public required string ClientName { get; set; }
		[MaxLength(500)]
		public required string Address { get; set; }
		[MaxLength(20)]
		public required string Phone { get; set; }
		[MaxLength(100)]
		public string? Email { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.Now;

		// Navigation properties
		[System.Text.Json.Serialization.JsonIgnore]
		public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
	}
}