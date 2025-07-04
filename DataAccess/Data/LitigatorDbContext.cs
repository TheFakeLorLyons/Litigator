using Microsoft.EntityFrameworkCore;
using Litigator.DataAccess.Entities;

namespace Litigator.DataAccess.Data
{
	public class LitigatorDbContext : DbContext
	{
		public LitigatorDbContext(DbContextOptions<LitigatorDbContext> options) : base(options) { }

        public DbSet<Attorney> Attorneys { get; set; }
        public DbSet<Client> Clients { get; set; }
		public DbSet<Court> Courts { get; set; }
		public DbSet<Case> Cases { get; set; }
		public DbSet<Deadline> Deadlines { get; set; }
		public DbSet<Document> Documents { get; set; }
        public DbSet<Judge> Judges { get; set; }

        public DbSet<Person> People { get; set; }
        public DbSet<LegalProfessional> LegalProfessionals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            base.OnModelCreating(modelBuilder);

            // Configure Table-Per-Hierarchy inheritance instead of TPT
            modelBuilder.Entity<Person>()
                .HasDiscriminator<string>("PersonType")
                .HasValue<Attorney>("Attorney")
                .HasValue<Judge>("Judge")
                .HasValue<Client>("Client");

            // Configure the inheritance hierarchy
            modelBuilder.Entity<LegalProfessional>().HasBaseType<Person>();
            modelBuilder.Entity<Client>().HasBaseType<Person>();
            modelBuilder.Entity<Attorney>().HasBaseType<LegalProfessional>();
            modelBuilder.Entity<Judge>().HasBaseType<LegalProfessional>();


            // Person name configuration (applies to all derived types)
            modelBuilder.Entity<Person>()
                .OwnsOne(p => p.Name, name =>
                {
                    name.Property(n => n.First)
                        .HasColumnName("FirstName")
                        .HasMaxLength(100)
                        .IsRequired();

                    name.Property(n => n.Last)
                        .HasColumnName("LastName")
                        .HasMaxLength(100);

                    name.Property(n => n.Middle)
                        .HasColumnName("MiddleName")
                        .HasMaxLength(100);

                    name.Property(n => n.Title)
                        .HasColumnName("Title")
                        .HasMaxLength(20);

                    name.Property(n => n.Suffix)
                        .HasColumnName("Suffix")
                        .HasMaxLength(20);

                    name.Property(n => n.Preferred)
                        .HasColumnName("PreferredName")
                        .HasMaxLength(100);
                });

            // Person address configuration (applies to all derived types)
            modelBuilder.Entity<Person>()
                .OwnsOne(p => p.PrimaryAddress, address =>
                {
                    address.Property(a => a.Line1)
                        .HasColumnName("AddressLine1")
                        .HasMaxLength(500);

                    address.Property(a => a.Line2)
                        .HasColumnName("AddressLine2")
                        .HasMaxLength(500);

                    address.Property(a => a.City)
                        .HasColumnName("City")
                        .HasMaxLength(100);

                    address.Property(a => a.County)
                        .HasColumnName("County")
                        .HasMaxLength(100);

                    address.Property(a => a.State)
                        .HasColumnName("State")
                        .HasMaxLength(50);

                    address.Property(a => a.PostalCode)
                        .HasColumnName("PostalCode")
                        .HasMaxLength(20);

                    address.Property(a => a.Country)
                        .HasColumnName("Country")
                        .HasMaxLength(100)
                        .HasDefaultValue("United States");

                    address.Property(a => a.IsPopulated)
                        .HasColumnName("HasPrimaryAddress")
                        .IsRequired()
                        .HasDefaultValue(false);
                });

            // Base Person phone configuration (optional)
            modelBuilder.Entity<Person>()
                .OwnsOne(p => p.PrimaryPhone, phone =>
                {
                    phone.Property(p => p.Number)
                        .HasColumnName("PrimaryPhoneNumber")
                        .HasMaxLength(20);

                    phone.Property(p => p.Extension)
                        .HasColumnName("PrimaryPhoneExtension")
                        .HasMaxLength(10);

                    phone.Property(p => p.IsPopulated)
                        .HasColumnName("HasPrimaryPhone")
                        .IsRequired()
                        .HasDefaultValue(false);
                });

            // Legal Professional specific configurations
            // Override for LegalProfessional to make address required
            modelBuilder.Entity<LegalProfessional>()
                .OwnsOne(lp => lp.PrimaryAddress, address =>
                {
                    // Inherit all base configurations
                    address.Property(a => a.Line1)
                        .HasColumnName("AddressLine1")
                        .HasMaxLength(500);

                    address.Property(a => a.Line2)
                        .HasColumnName("AddressLine2")
                        .HasMaxLength(500);

                    address.Property(a => a.City)
                        .HasColumnName("City")
                        .HasMaxLength(100);

                    address.Property(a => a.County)
                        .HasColumnName("County")
                        .HasMaxLength(100);

                    address.Property(a => a.State)
                        .HasColumnName("State")
                        .HasMaxLength(50);

                    address.Property(a => a.PostalCode)
                        .HasColumnName("PostalCode")
                        .HasMaxLength(20);

                    address.Property(a => a.Country)
                        .HasColumnName("Country")
                        .HasMaxLength(100)
                        .HasDefaultValue("United States");

                    // Mark as required for legal professionals with default true
                    address.Property(a => a.IsPopulated)
                        .HasColumnName("HasPrimaryAddress")
                        .IsRequired()
                        .HasDefaultValue(true);
                });

            // Override for LegalProfessional to make phone required
            modelBuilder.Entity<LegalProfessional>()
                .OwnsOne(lp => lp.PrimaryPhone, phone =>
                {
                    // Inherit all base configurations
                    phone.Property(p => p.Number)
                        .HasColumnName("PrimaryPhoneNumber")
                        .HasMaxLength(20);

                    phone.Property(p => p.Extension)
                        .HasColumnName("PrimaryPhoneExtension")
                        .HasMaxLength(10);

                    // Mark as required for legal professionals with default true
                    phone.Property(p => p.IsPopulated)
                        .HasColumnName("HasPrimaryPhone")
                        .IsRequired()
                        .HasDefaultValue(true);
                });

            // PersonAddress configuration
            modelBuilder.Entity<PersonAddress>()
                .HasOne(pa => pa.Person)
                .WithMany(p => p.AdditionalAddresses)
                .HasForeignKey("PersonId")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PersonAddress>()
                .OwnsOne(pa => pa.Address, address =>
                {
                    address.Property(a => a.Line1)
                        .HasColumnName("AddressLine1")
                        .HasMaxLength(500);

                    address.Property(a => a.Line2)
                        .HasColumnName("AddressLine2")
                        .HasMaxLength(500);

                    address.Property(a => a.City)
                        .HasColumnName("City")
                        .HasMaxLength(100);

                    address.Property(a => a.County)
                        .HasColumnName("County")
                        .HasMaxLength(100);

                    address.Property(a => a.State)
                        .HasColumnName("State")
                        .HasMaxLength(50);

                    address.Property(a => a.PostalCode)
                        .HasColumnName("PostalCode")
                        .HasMaxLength(20);

                    address.Property(a => a.Country)
                        .HasColumnName("Country")
                        .HasMaxLength(100)
                        .HasDefaultValue("United States");

                    address.Property(a => a.IsPopulated)
                        .HasColumnName("HasAddress")
                        .IsRequired()
                        .HasDefaultValue(true);
                });

            // PersonPhoneNumber configuration
            modelBuilder.Entity<PersonPhoneNumber>()
                .HasOne(ppn => ppn.Person)
                .WithMany(p => p.AdditionalPhones)
                .HasForeignKey("PersonId")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PersonPhoneNumber>()
                .OwnsOne(ppn => ppn.PhoneNumber, phone =>
                {
                    phone.Property(p => p.Number)
                        .HasColumnName("PhoneNumber")
                        .HasMaxLength(20);

                    phone.Property(p => p.Extension)
                        .HasColumnName("PhoneExtension")
                        .HasMaxLength(10);

                    phone.Property(p => p.IsPopulated)
                        .HasColumnName("HasPhone")
                        .IsRequired()
                        .HasDefaultValue(true);
                });

            // Case context
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
                .HasOne(c => c.AssignedJudge)
                .WithMany(j => j.Cases)
                .HasForeignKey(c => c.AssignedJudgeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Case>()
                .HasOne(c => c.Court)
                .WithMany(co => co.Cases)
                .HasForeignKey(c => c.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Case>()
                .Property(c => c.CurrentRealCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Case>()
                .Property(c => c.EstimatedValue)
                .HasPrecision(18, 2);

            // Client-Attorney many-to-many relationship
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Attorneys)
                .WithMany(a => a.Clients)
                .UsingEntity(j => j.ToTable("ClientAttorney"));

            // Court configuration
            modelBuilder.Entity<Court>()
                .HasOne(c => c.ChiefJudge)
                .WithMany()
                .HasForeignKey(c => c.ChiefJudgeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Court>()
                .HasMany(c => c.LegalProfessionals)
                .WithMany(lp => lp.Courts)
                .UsingEntity(j => j.ToTable("CourtLegalProfessional"));

            modelBuilder.Entity<Court>()
                .OwnsOne(c => c.Address, address =>
                {
                    address.Property(a => a.Line1)
                        .HasColumnName("AddressLine1")
                        .HasMaxLength(500);

                    address.Property(a => a.Line2)
                        .HasColumnName("AddressLine2")
                        .HasMaxLength(500);

                    address.Property(a => a.City)
                        .HasColumnName("City")
                        .HasMaxLength(100);

                    address.Property(a => a.County)
                        .HasColumnName("County")
                        .HasMaxLength(100)
                        .IsRequired();

                    address.Property(a => a.State)
                        .HasColumnName("State")
                        .HasMaxLength(50)
                        .IsRequired();

                    address.Property(a => a.PostalCode)
                        .HasColumnName("PostalCode")
                        .HasMaxLength(20);

                    address.Property(a => a.Country)
                        .HasColumnName("Country")
                        .HasMaxLength(100)
                        .HasDefaultValue("United States");
                });

            modelBuilder.Entity<Court>()
                .OwnsOne(c => c.Phone, phone =>
                {
                    phone.Property(p => p.Number)
                        .HasColumnName("PhoneNumber")
                        .HasMaxLength(20);

                    phone.Property(p => p.Extension)
                        .HasColumnName("PhoneExtension")
                        .HasMaxLength(10);
                });

            // Deadline context
            modelBuilder.Entity<Deadline>()
                .HasOne(d => d.Case)
                .WithMany(c => c.Deadlines)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document context
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Case)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Basic indexing
            modelBuilder.Entity<Attorney>()
                .HasIndex(a => a.BarNumber)
                .IsUnique();

            modelBuilder.Entity<Attorney>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Judge>()
                .HasIndex(j => j.BarNumber)
                .IsUnique();

            modelBuilder.Entity<Judge>()
                .HasIndex(j => j.Email)
                .IsUnique();

            modelBuilder.Entity<Case>()
                .HasIndex(c => c.CaseNumber)
                .IsUnique();

            modelBuilder.Entity<Case>()
                .Property(c => c.EstimatedValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Case>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<Case>()
                .HasIndex(c => c.FilingDate);

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email);

            modelBuilder.Entity<Deadline>()
                .HasIndex(d => d.DeadlineDate);

            modelBuilder.Entity<Deadline>()
                .HasIndex(d => d.IsCompleted);
        }
    }
}