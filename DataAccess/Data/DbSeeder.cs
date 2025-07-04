using Bogus;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using System.CodeDom;

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

        public class FakerContext
        {
            public Faker<Address> AddressFaker { get; }
            public Faker<PhoneNumber> PhoneFaker { get; }
            public Faker<PersonName> PersonNameFaker { get; }

            public FakerContext()
            {
                AddressFaker = FakerFactory.CreateAddressFaker();
                PhoneFaker = FakerFactory.CreatePhoneFaker();
                PersonNameFaker = FakerFactory.CreatePersonNameFaker();
            }
        }

        public static class FakerFactory
        {
            public static Faker<Address> CreateAddressFaker() =>
                new Faker<Address>()
                    .RuleFor(a => a.Line1, f => f.Address.StreetAddress())
                    .RuleFor(a => a.Line2, f => f.Random.Bool(0.3f) ? f.Address.SecondaryAddress() : null)
                    .RuleFor(a => a.City, f => f.Address.City())
                    .RuleFor(a => a.State, f => f.Address.StateAbbr())
                    .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
                    .RuleFor(a => a.Country, _ => "United States");

            public static Faker<PhoneNumber> CreatePhoneFaker() =>
                new Faker<PhoneNumber>()
                    .RuleFor(p => p.Number, f => f.Phone.PhoneNumber("(###) ###-####"))
                    .RuleFor(p => p.Extension, f => f.Random.Bool(0.2f) ? f.Random.Number(100, 9999).ToString() : null);

            public static Faker<PersonName> CreatePersonNameFaker() =>
                  new Faker<PersonName>()
                      .RuleFor(n => n.First, f => f.Name.FirstName())
                      .RuleFor(n => n.Last, f => f.Name.LastName())
                      .RuleFor(n => n.Middle, f => f.Random.Bool(0.3f) ? f.Name.FirstName() : null)
                      .RuleFor(n => n.Title, f => f.Random.Bool(0.1f) ? f.PickRandom("Mr.", "Ms.", "Mrs.", "Dr.", "Esq.", "Attorney at Law", "Counsel") : null)
                      .RuleFor(n => n.Suffix, f => f.Random.Bool(0.05f) ? f.PickRandom("Jr.", "Sr.", "III") : null)
                      .RuleFor(n => n.Preferred, f => f.Random.Bool(0.2f) ? f.Name.FirstName() : null);
        }

        private static void SeedEssentialData(LitigatorDbContext context)
        {
            var addressFaker = FakerFactory.CreateAddressFaker();
            var phoneFaker = FakerFactory.CreatePhoneFaker();
            var personNameFaker = FakerFactory.CreatePersonNameFaker();
            //Seed Judges first
            if (!context.Judges.Any())
            {
                var judges = new[]
                {
                    new Judge {
                        Name = new PersonName { First = "Alice", Last = "Franklin" },
                        BarNumber = "J12345",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "afranklin@court.gov"
                    },
                    new Judge {
                        Name = new PersonName { First = "David", Last = "Nguyen" },
                        BarNumber = "J67890",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "dnguyen@court.gov"
                    },
                    new Judge {
                        Name = new PersonName { First = "Grace", Last = "Kim" },
                        BarNumber = "J11122",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "gkim@court.gov"
                    },
                    new Judge {
                        Name = new PersonName { First = "Henry", Last = "Lopez" },
                        BarNumber = "J33344",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "hlopez@court.gov"
                    },
                    new Judge {
                        Name = new PersonName { First = "Irene", Last = "Woods" },
                        BarNumber = "J55566",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "iwoods@court.gov"
                    }
                };
                context.Judges.AddRange(judges);
                context.SaveChanges();
            }

            var judgesFromDb = context.Judges.ToList();

            // Seed initial Courts
            if (!context.Courts.Any())
            {
                var judges = context.Judges.ToList();
                var courts = new[]
                {
                    new Court {
                        CourtName = "New York Supreme Court",
                        CourtType = "State",
                        Division = "Civil",
                        Address = Address.Create("60 Centre Street", "New York", "NY", "10007"),
                        Phone = PhoneNumber.Create("(212) 374-8875"),
                        Email = "BronxFamilyCourt@nycourts.gov",
                        Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                        ClerkOfCourt = "Maria Rodriguez",
                        BusinessHours = "9:00 AM - 5:00 PM, Monday - Friday",
                        IsActive = true
                    },
                    new Court {
                        CourtName = "U.S. District Court SDNY",
                        CourtType = "Federal",
                        Division = "Civil",
                        Address = Address.Create("40 Foley Square", "New York", "NY", "10007"),
                        Phone = PhoneNumber.Create("(212) 805-0136"),
                        Email = "kingsfamilycourt@nycourts.gov",
                        Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                        ClerkOfCourt = "Ruby J. Krajick",
                        BusinessHours = "8:30 AM - 5:00 PM, Monday - Friday",
                        IsActive = true
                    },
                    new Court {
                        CourtName = "Nassau County Court",
                        CourtType = "State",
                        Division = "Criminal",
                        Address = Address.Create("262 Old Country Road", "Mineola", "NY", "11501"),
                        Phone = PhoneNumber.Create("(516) 571-2905"),
                        Email = "manhattanfamilycourt@nycourts.gov",
                        Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                        ClerkOfCourt = "Maureen C. Bartlett",
                        BusinessHours = "9:00 AM - 4:30 PM, Monday - Friday",
                        IsActive = true
                    },
                    new Court {
                        CourtName = "Kings County Supreme Court",
                        CourtType = "State",
                        Division = "Civil",
                        Address = Address.Create("360 Adams Street", "Brooklyn", "NY", "11201"),
                        Phone = PhoneNumber.Create("(347) 401-9000"),
                        Email = "queensfamilycourt@nycourts.gov",
                        Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                        ClerkOfCourt = "Nancy T. Sunshine",
                        BusinessHours = "9:00 AM - 5:00 PM, Monday - Friday",
                        IsActive = true
                    },
                    new Court {
                        CourtName = "U.S. District Court EDNY",
                        CourtType = "Federal",
                        Division = "Civil",
                        Address = Address.Create("225 Cadman Plaza East", "Brooklyn", "NY", "11201"),
                        Phone = PhoneNumber.Create("(718) 613-2600"),
                        Email = "richmondfamilycourt@nycourts.gov",
                        Website = "https://ww2.nycourts.gov/courts/nyc/family/contactus.shtml",
                        ClerkOfCourt = "Cheryl L. Pollak",
                        BusinessHours = "8:30 AM - 5:00 PM, Monday - Friday",
                        IsActive = true
                    }
                };
                context.Courts.AddRange(courts);
                context.SaveChanges();
            }

            // Seed initial Attorneys
            if (!context.Attorneys.Any())
            {
                var attorneys = new[]
                {
                    new Attorney {
                        Name = new PersonName { First = "Sarah", Last = "Johnson" },
                        BarNumber = "12345",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "sjohnson@firm.com"
                    },
                    new Attorney {
                        Name = new PersonName { First = "Michael", Last = "Davis" },
                        BarNumber = "67890",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "mdavis@firm.com"
                    },
                    new Attorney {
                        Name = new PersonName { First = "Emily", Last = "Brown" },
                        BarNumber = "54321",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "ebrown@firm.com"
                    },
                    new Attorney {
                        Name = new PersonName { First = "Robert", Last = "Wilson" },
                        BarNumber = "RW11111",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "rwilson@lawfirm.com"
                    },
                    new Attorney {
                        Name = new PersonName { First = "Jennifer", Last = "Martinez" },
                        BarNumber = "JM22222",
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "jmartinez@lawfirm.com"
                    }
                };
                context.Attorneys.AddRange(attorneys);
                context.SaveChanges();
            }

            // Seed initial Clients
            if (!context.Clients.Any())
            {
                var clients = new[]
                {
                    new Client {
                        Name = new PersonName { First = "John", Last = "Smith" },
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "johnsmith@acmecorp.com"
                    },
                    new Client {
                        Name = new PersonName { First = "Linda", Last= "Taylor" },
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "ltaylor@globalind.com"
                    },
                    new Client {
                        Name = new PersonName { First = "James", Last = "Smith" },
                        PrimaryAddress = addressFaker.Generate(),
                        PrimaryPhone = phoneFaker.Generate(),
                        Email = "jsmith@smithtrust.com"
                    }
                };
                context.Clients.AddRange(clients);
                context.SaveChanges();
            }
        }

        private static void SeedTestDataWithBogus(LitigatorDbContext context)
        {
            var addressFaker = FakerFactory.CreateAddressFaker();
            var phoneFaker = FakerFactory.CreatePhoneFaker();
            var personNameFaker = FakerFactory.CreatePersonNameFaker();
            var random = new Random();

            // Load existing data
            var courts = context.Courts.Include(c => c.LegalProfessionals).ToList();
            var judges = context.Judges.ToList();
            var clients = context.Clients.ToList();
            var attorneys = context.Attorneys.ToList();
            var legalProfessionals = attorneys.Cast<LegalProfessional>().ToList();
            legalProfessionals.AddRange(judges);

            // Assign attorneys to clients
            foreach (var client in clients)
            {
                // Assign 1 to 3 random attorneys per client
                var assignedAttorneys = attorneys.OrderBy(_ => Guid.NewGuid()).Take(random.Next(1, 4)).ToList();
                foreach (var attorney in assignedAttorneys)
                {
                    client.Attorneys.Add(attorney);
                }
            }

            // Assign legal professionals to courts
            foreach (var court in courts)
            {
                court.LegalProfessionals = legalProfessionals.OrderBy(_ => Guid.NewGuid()).Take(5).ToList();
            }
            context.SaveChanges();

            // Generate additional clients
            if (context.Clients.Count() < 50)
            {
                var clientFaker = new Faker<Client>()
                    .RuleFor(c => c.Name, f => personNameFaker.Generate())
                    .RuleFor(c => c.PrimaryAddress, f => addressFaker.Generate())
                    .RuleFor(c => c.PrimaryPhone, f => phoneFaker.Generate())
                    .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name.First, c.Name.Last))
                    .RuleFor(c => c.CreatedDate, f => f.Date.Past(2));

                var additionalClients = clientFaker.GenerateBetween(30, 50);
                context.Clients.AddRange(additionalClients);
                context.SaveChanges();
                clients = context.Clients.ToList();
            }

            // Generate cases with realistic data
            if (context.Cases.Count() < 100)
            {
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
                    .RuleFor(c => c.CourtId, f => f.PickRandom(courts).CourtId);

                var cases = caseFaker.GenerateBetween(80, 100);
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