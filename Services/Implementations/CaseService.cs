using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;

namespace Litigator.Services.Implementations
{
    public class CaseService : ICaseService
    {
        private readonly LitigatorDbContext _context;

        public CaseService(LitigatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CaseDTO>> GetAllCasesAsync()
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Select(c => new CaseDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientFirstName = c.Client != null ? c.Client.Name.First : string.Empty,
                    ClientLastName = c.Client != null ? c.Client.Name.Last ?? string.Empty : string.Empty,
                    AttorneyFirstName = c.AssignedAttorney != null ? $"{c.AssignedAttorney.Name.First}" : null,
                    AttorneyLastName = c.AssignedAttorney != null ? $"{c.AssignedAttorney.Name.Last}" : null,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                })
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<CaseDetailDTO?> GetCaseByIdAsync(int id)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .Where(c => c.CaseId == id)
                .Select(c => new CaseDetailDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientId = c.ClientId,
                    ClientFirstName = c.Client != null ? c.Client.Name.First : string.Empty,
                    ClientLastName = c.Client != null ? c.Client.Name.Last ?? string.Empty : string.Empty,
                    ClientEmail = c.Client != null ? c.Client.Email : null,
                    ClientPhone = c.Client != null && c.Client.PrimaryPhone != null ? c.Client.PrimaryPhone.Display : null,
                    AssignedAttorneyId = c.AssignedAttorneyId,
                    AttorneyFirstName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.First : null,
                    AttorneyLastName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.Last : null,
                    AttorneyEmail = c.AssignedAttorney != null ? c.AssignedAttorney.Email : null,
                    CourtId = c.CourtId,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    CourtAddress = c.Court != null && c.Court.Address != null ? c.Court.Address.ToString() : null,
                    TotalDeadlines = c.Deadlines.Count(),
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now),
                    OverdueDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate < DateTime.Now),
                    TotalDocuments = c.Documents.Count(),
                    NextDeadlineDate = c.Deadlines
                        .Where(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                        .OrderBy(d => d.DeadlineDate)
                        .Select(d => d.DeadlineDate)
                        .FirstOrDefault(),
                    NextDeadlineDescription = c.Deadlines
                        .Where(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                        .OrderBy(d => d.DeadlineDate)
                        .Select(d => d.Description)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CaseDetailDTO?> GetCaseByNumberAsync(string caseNumber)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .Where(c => c.CaseNumber == caseNumber)
                .Select(c => new CaseDetailDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientId = c.ClientId,
                    ClientFirstName = c.Client != null ? c.Client.Name.First : "Unknown",
                    ClientLastName = c.Client != null ? c.Client.Name.Last : "Unknown",
                    ClientEmail = c.Client != null ? c.Client.Email : null,
                    ClientPhone = c.Client != null && c.Client.PrimaryPhone != null ? c.Client.PrimaryPhone.Display : null,
                    AssignedAttorneyId = c.AssignedAttorneyId,
                    AttorneyFirstName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.First : null,
                    AttorneyLastName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.Last : null,
                    AttorneyEmail = c.AssignedAttorney != null ? c.AssignedAttorney.Email : null,
                    CourtId = c.CourtId,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    CourtAddress = c.Court != null && c.Court.Address != null ? c.Court.Address.ToString() : null,
                    TotalDeadlines = c.Deadlines.Count(),
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now),
                    OverdueDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate < DateTime.Now),
                    TotalDocuments = c.Documents.Count(),
                    NextDeadlineDate = c.Deadlines
                        .Where(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                        .OrderBy(d => d.DeadlineDate)
                        .Select(d => d.DeadlineDate)
                        .FirstOrDefault(),
                    NextDeadlineDescription = c.Deadlines
                        .Where(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                        .OrderBy(d => d.DeadlineDate)
                        .Select(d => d.Description)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CaseDTO>> GetCasesByClientAsync(int clientId)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Where(c => c.ClientId == clientId)
                .Select(c => new CaseDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientFirstName = c.Client != null ? c.Client.Name.First : "Unknown",
                    ClientLastName = c.Client != null ? c.Client.Name.Last : "Unknown",
                    AttorneyFirstName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.First : null,
                    AttorneyLastName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.Last : null,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                })
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CaseDTO>> GetCasesByAttorneyAsync(int attorneyId)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Where(c => c.AssignedAttorneyId == attorneyId)
                .Select(c => new CaseDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientFirstName = c.Client != null ? c.Client.Name.First : "Unknown",
                    ClientLastName = c.Client != null ? c.Client.Name.Last : "Unknown",
                    AttorneyFirstName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.First : null,
                    AttorneyLastName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.Last : null,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                })
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CaseDTO>> GetActiveCasesAsync()
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Where(c => c.Status == "Active")
                .Select(c => new CaseDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientFirstName = c.Client != null ? c.Client.Name.First : "Unknown",
                    ClientLastName = c.Client != null ? c.Client.Name.Last : "Unknown",
                    AttorneyFirstName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.First : null,
                    AttorneyLastName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.Last : null,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                })
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CaseDTO>> SearchCasesAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Where(c =>
                    c.CaseNumber.ToLower().Contains(term) ||
                    c.CaseTitle.ToLower().Contains(term) ||
                    (c.Client != null && c.Client.Name.First.ToLower().Contains(term)) ||
                    (c.AssignedAttorney != null &&
                     (c.AssignedAttorney.Name.First + " " + c.AssignedAttorney.Name.Last).ToLower().Contains(term)))
                .Select(c => new CaseDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientFirstName = c.Client != null ? c.Client.Name.First : "Unknown",
                    ClientLastName = c.Client != null ? c.Client.Name.Last : "Unknown",
                    AttorneyFirstName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.First : null,
                    AttorneyLastName = c.AssignedAttorney != null ? c.AssignedAttorney.Name.Last : null,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                })
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<CaseDetailDTO> CreateCaseAsync(Case case_)
        {
            // Validate that the case number is unique
            var existingCase = await _context.Cases.FirstOrDefaultAsync(c => c.CaseNumber == case_.CaseNumber);
            if (existingCase != null)
            {
                throw new InvalidOperationException($"Case number {case_.CaseNumber} already exists.");
            }

            // Validate foreign key references
            await ValidateForeignKeysAsync(case_);

            _context.Cases.Add(case_);
            await _context.SaveChangesAsync();

            var result = await GetCaseByIdAsync(case_.CaseId);
            return result ?? throw new InvalidOperationException("Failed to retrieve created case.");
        }

        public async Task<CaseDetailDTO> UpdateCaseAsync(Case case_)
        {
            var existingCase = await _context.Cases.FindAsync(case_.CaseId);
            if (existingCase == null)
            {
                throw new InvalidOperationException($"Case with ID {case_.CaseId} not found.");
            }

            // Check if case number is being changed and if it's unique
            if (existingCase.CaseNumber != case_.CaseNumber)
            {
                var duplicateCase = await _context.Cases.FirstOrDefaultAsync(c => c.CaseNumber == case_.CaseNumber);
                if (duplicateCase != null)
                {
                    throw new InvalidOperationException($"Case number {case_.CaseNumber} already exists.");
                }
            }

            await ValidateForeignKeysAsync(case_);

            _context.Entry(existingCase).CurrentValues.SetValues(case_);
            await _context.SaveChangesAsync();

            var result = await GetCaseByIdAsync(case_.CaseId);
            return result ?? throw new InvalidOperationException("Failed to retrieve updated case.");
        }

        public async Task<bool> DeleteCaseAsync(int id)
        {
            var case_ = await _context.Cases.FindAsync(id);
            if (case_ == null)
            {
                return false;
            }

            _context.Cases.Remove(case_);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Case?> GetCaseEntityByIdAsync(int id)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CaseId == id);
        }

        private async Task ValidateForeignKeysAsync(Case case_)
        {
            // Validate client exists
            var client = await _context.Clients.FindAsync(case_.ClientId);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {case_.ClientId} not found.");
            }

            // Validate attorney exists
            var attorney = await _context.Attorneys.FindAsync(case_.AssignedAttorneyId);
            if (attorney == null)
            {
                throw new InvalidOperationException($"Attorney with ID {case_.AssignedAttorneyId} not found.");
            }

            // Validate court exists
            var court = await _context.Courts.FindAsync(case_.CourtId);
            if (court == null)
            {
                throw new InvalidOperationException($"Court with ID {case_.CourtId} not found.");
            }
        }
    }
}