using Bogus;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using System;
using System.Collections.Generic;

namespace Litigator.DataAccess.Data
{
    public static class DbSeeder
    {
        public static void SeedData(LitigatorDbContext context, bool includeTestData = true)
        {
            // Ensure database is created
            context.Database.EnsureCreated();
            
            // Essential seed data (always create)
            SeedEssentialData(context);

            // Test data (only in development)
            if (includeTestData)
            {
                SeedTestDataWithBogus(context);
            }
        }

        private static void SeedEssentialData(LitigatorDbContext context)
        {
            // Seed initial Courts
            if (!context.Courts.Any())
            {
                var courts = new[]
                {
                    new Court { CourtName = "New York Supreme Court", County = "New York", Address = "123 Fake Stret", State = "NY", CourtType = "State" },
                    new Court { CourtName = "U.S. District Court SDNY", County = "New York",  Address = "123 Fake Stret", State = "NY", CourtType = "Federal" },
                    new Court { CourtName = "Nassau County Court", County = "Nassau",  Address = "123 Fake Stret", State = "NY", CourtType = "State" },
                    new Court { CourtName = "Kings County Supreme Court", County = "Kings",  Address = "123 Fake Stret", State = "NY", CourtType = "State" },
                    new Court { CourtName = "U.S. District Court EDNY", County = "Kings", Address = "123 Fake Stret", State = "NY", CourtType = "Federal" }
                };
                context.Courts.AddRange(courts);
                context.SaveChanges();
            }

            // Seed initial Attorneys
            if (!context.Attorneys.Any())
            {
                var attorneys = new[]
                {
                    new Attorney { FirstName = "Sarah", LastName = "Johnson", BarNumber = "12345", Phone = "(617) 123-4567", Email = "sjohnson@firm.com" },
                    new Attorney { FirstName = "Michael", LastName = "Davis", BarNumber = "67890", Phone = "(718) 922-6543", Email = "mdavis@firm.com" },
                    new Attorney { FirstName = "Emily", LastName = "Brown", BarNumber = "54321", Phone = "(720) 666-1854", Email = "ebrown@firm.com" },
                    new Attorney { FirstName = "Robert", LastName = "Wilson", BarNumber = "RW11111", Phone = "(413) 645-6435", Email = "rwilson@lawfirm.com" },
                    new Attorney { FirstName = "Jennifer", LastName = "Martinez", BarNumber = "JM22222", Phone = "(910) 882-4436", Email = "jmartinez@lawfirm.com" }
                };
                context.Attorneys.AddRange(attorneys);
                context.SaveChanges();
            }

            // Seed initial Clients
            if (!context.Clients.Any())
            {
                var clients = new[]
                {
                    new Client { ClientName = "Acme Corporation", Address = "123 Business Ave, New York, NY 10001", Phone = "(555) 123-4567", Email = "legal@acmecorp.com" },
                    new Client { ClientName = "Global Industries Inc.", Address = "456 Corporate Blvd, New York, NY 10002", Phone = "(555) 234-5678", Email = "contracts@globalind.com" },
                    new Client { ClientName = "Smith Family Trust", Address = "789 Residential St, Brooklyn, NY 11201", Phone = "(555) 345-6789", Email = "trustee@smithfamily.com" }
                };
                context.Clients.AddRange(clients);
                context.SaveChanges();
            }
        }

        private static void SeedTestDataWithBogus(LitigatorDbContext context)
        {
            var random = new Random();

            // Generate additional clients
            if (context.Clients.Count() < 50)
            {
                var clientFaker = new Faker<Client>()
                    .RuleFor(c => c.ClientName, f => f.Random.Bool(0.7f) ? f.Company.CompanyName() : f.Name.FullName())
                    .RuleFor(c => c.Address, f => f.Address.FullAddress())
                    .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("(###) ###-####"))
                    .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.ClientName.Replace(" ", "").Replace(",", "").Replace(".", "").ToLower()))
                    .RuleFor(c => c.CreatedDate, f => f.Date.Past(2));

                var additionalClients = clientFaker.Generate(47);
                context.Clients.AddRange(additionalClients);
                context.SaveChanges();
            }

            // Generate cases with realistic data
            if (context.Cases.Count() < 100)
            {
                var clients = context.Clients.ToList();
                var attorneys = context.Attorneys.ToList();
                var courts = context.Courts.ToList();

                var caseTypes = new[] { "Civil", "Criminal", "Family", "Corporate", "Personal Injury", "Contract Dispute", "Employment", "Real Estate", "Intellectual Property" };
                var statuses = new[] { "Active", "Pending", "Closed", "On Hold" };

                var caseFaker = new Faker<Case>()
                    .RuleFor(c => c.CaseNumber, f => f.Random.Replace("####-CV-####"))
                    .RuleFor(c => c.CaseTitle, f =>
                    {
                        var plaintiffName = f.Random.Bool(0.6f) ? f.Name.LastName() : f.Company.CompanyName();
                        var defendantName = f.Random.Bool(0.6f) ? f.Name.LastName() : f.Company.CompanyName();
                        return $"{plaintiffName} v. {defendantName}";
                    })
                    .RuleFor(c => c.CaseType, f => f.PickRandom(caseTypes))
                    .RuleFor(c => c.FilingDate, f => f.Date.Past(2))
                    .RuleFor(c => c.Status, f => f.PickRandom(statuses))
                    .RuleFor(c => c.EstimatedValue, f => f.Random.Bool(0.8f) ? f.Random.Decimal(5000, 2000000) : null)
                    .RuleFor(c => c.ClientId, f => f.PickRandom(clients).ClientId)
                    .RuleFor(c => c.AssignedAttorneyId, f => f.PickRandom(attorneys).AttorneyId)
                    .RuleFor(c => c.CourtId, f => f.PickRandom(courts).CourtId);

                var cases = caseFaker.Generate(97); // 97 + 3 existing = 100
                context.Cases.AddRange(cases);
                context.SaveChanges();

                // Generate deadlines for cases
                var allCases = context.Cases.ToList();
                var deadlineTypes = new[] { "Discovery Deadline", "Motion Filing", "Hearing Date", "Deposition", "Trial Date", "Settlement Conference", "Mediation", "Expert Witness Disclosure" };

                var deadlines = new List<Deadline>();
                foreach (var caseItem in allCases)
                {
                    var numDeadlines = random.Next(1, 6); // 1-5 deadlines per case
                    for (int i = 0; i < numDeadlines; i++)
                    {
                        var deadlineFaker = new Faker<Deadline>()
                            .RuleFor(d => d.DeadlineType, f => f.PickRandom(deadlineTypes))
                            .RuleFor(d => d.Description, (f, d) => $"{d.DeadlineType} for {caseItem.CaseTitle}")
                            .RuleFor(d => d.DeadlineDate, f => f.Date.Between(caseItem.FilingDate.AddDays(30), DateTime.Now.AddMonths(12)))
                            .RuleFor(d => d.IsCompleted, f => f.Random.Bool(0.3f))
                            .RuleFor(d => d.IsCritical, f => f.Random.Bool(0.25f))
                            .RuleFor(d => d.CaseId, _ => caseItem.CaseId)
                            .RuleFor(d => d.CompletedDate, (f, d) => d.IsCompleted && d.DeadlineDate < DateTime.Now ? f.Date.Between(d.DeadlineDate.AddDays(-5), DateTime.Now) : null);

                        deadlines.Add(deadlineFaker.Generate());
                    }
                }

                context.Deadlines.AddRange(deadlines);
                context.SaveChanges();

                var documentTypes = new[] { "Complaint", "Answer", "Motion", "Brief", "Affidavit", "Contract", "Correspondence", "Discovery Request", "Exhibit" };
                var documents = new List<Document>();

                foreach (var caseItem in allCases.Take(75)) // Documents for 75% of cases
                {
                    var numDocs = random.Next(1, 8); // 1-7 documents per case
                    for (int i = 0; i < numDocs; i++)
                    {
                        var docFaker = new Faker<Document>()
                            .RuleFor(d => d.DocumentName, (f, d) =>
                            {
                                var docType = f.PickRandom(documentTypes);
                                return $"{docType} - {caseItem.CaseNumber}";
                            })
                            .RuleFor(d => d.DocumentType, f => f.PickRandom(documentTypes))
                            .RuleFor(d => d.FilePath, (f, d) => $"/documents/{caseItem.CaseId}/{d.DocumentName.Replace(" ", "_")}.pdf")
                            .RuleFor(d => d.UploadDate, f => f.Date.Between(caseItem.FilingDate, DateTime.Now))
                            .RuleFor(d => d.FileSize, f => f.Random.Long(50000, 5000000)) // 50KB to 5MB
                            .RuleFor(d => d.UploadedBy, f => f.PickRandom(attorneys).Email)
                            .RuleFor(d => d.CaseId, _ => caseItem.CaseId);

                        documents.Add(docFaker.Generate());
                    }
                }

                context.Documents.AddRange(documents);
                context.SaveChanges();
            }
        }
    }
}