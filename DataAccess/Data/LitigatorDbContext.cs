using Microsoft.EntityFrameworkCore;
using Litigator.DataAccess.Entities;

namespace Litigator.DataAccess.Data
{
	public class LitigatorDbContext : DbContext
	{
		public LitigatorDbContext(DbContextOptions<LitigatorDbContext> options) : base(options) { }

		public DbSet<Client> Clients { get; set; }
		public DbSet<Attorney> Attorneys { get; set; }
		public DbSet<Court> Courts { get; set; }
		public DbSet<Case> Cases { get; set; }
		public DbSet<Deadline> Deadlines { get; set; }
		public DbSet<Document> Documents { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Case>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Cases)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Case>()
                .HasOne(c => c.AssignedAttorney)
                .WithMany(a => a.Cases)
                .HasForeignKey(c => c.AssignedAttorneyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Case>()
                .HasOne(c => c.Court)
                .WithMany(co => co.Cases)
                .HasForeignKey(c => c.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Deadline>()
                .HasOne(d => d.Case)
                .WithMany(c => c.Deadlines)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Case)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Simple indexing
            modelBuilder.Entity<Attorney>()
                .HasIndex(a => a.BarNumber)
                .IsUnique();

            modelBuilder.Entity<Attorney>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Case>()
                .HasIndex(c => c.CaseNumber)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email);

            modelBuilder.Entity<Case>()
                .Property(c => c.EstimatedValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Case>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<Case>()
                .HasIndex(c => c.FilingDate);

            modelBuilder.Entity<Deadline>()
                .HasIndex(d => d.DeadlineDate);

            modelBuilder.Entity<Deadline>()
                .HasIndex(d => d.IsCompleted);
        }
	}
}