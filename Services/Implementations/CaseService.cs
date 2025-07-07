using AutoMapper;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Litigator.Services.Implementations
{
    public class CaseService : ICaseService
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;

        public CaseService(LitigatorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CaseDetailDTO>> GetAllCasesAsync()
        {
            return await _context.Cases
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
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
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<CaseDetailDTO?> GetCaseByIdAsync(int id)
        {
            return await _context.Cases
                .AsNoTracking()
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
                .AsNoTracking()
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

        public async Task<IEnumerable<CaseDetailDTO>> GetCasesByClientAsync(int clientId)
        {
            return await _context.Cases
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .Where(c => c.ClientId == clientId)
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
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CaseDetailDTO>> GetCasesByAttorneyAsync(int attorneyId)
        {
            return await _context.Cases
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .Where(c => c.AssignedAttorneyId == attorneyId)
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
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CaseDetailDTO>> GetActiveCasesAsync()
        {
            return await _context.Cases
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .Where(c => c.Status == "Active")
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
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CaseDetailDTO>> SearchCasesAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Cases
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .Where(c =>
                    c.CaseNumber.ToLower().Contains(term) ||
                    c.CaseTitle.ToLower().Contains(term) ||
                    (c.Client != null && c.Client.Name.First.ToLower().Contains(term)) ||
                    (c.AssignedAttorney != null &&
                     (c.AssignedAttorney.Name.First + " " + c.AssignedAttorney.Name.Last).ToLower().Contains(term)))
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
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<CaseDetailDTO> CreateCaseAsync(CaseCreateDTO caseCreateDto)
        {
            // Check if case number already exists
            var existingCase = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseNumber == caseCreateDto.CaseNumber);

            if (existingCase != null)
                throw new InvalidOperationException($"Case number {caseCreateDto.CaseNumber} already exists.");

            // Map DTO to entity
            var caseEntity = _mapper.Map<Case>(caseCreateDto);

            // Add to context
            _context.Cases.Add(caseEntity);
            await _context.SaveChangesAsync();

            // Load related entities for mapping
            var createdCase = await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CaseId == caseEntity.CaseId);

            // Map back to DTO
            return _mapper.Map<CaseDetailDTO>(createdCase);
        }

        public async Task<CaseDetailDTO> UpdateCaseAsync(int caseId, CaseUpdateDTO caseUpdateDto)
        {
            var existingCase = await _context.Cases.FindAsync(caseId);
            if (existingCase == null)
                throw new InvalidOperationException($"Case with ID {caseId} not found.");

            // Check if case number already exists for a different case
            var duplicateCase = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseNumber == caseUpdateDto.CaseNumber && c.CaseId != caseId);

            if (duplicateCase != null)
                throw new InvalidOperationException($"Case number {caseUpdateDto.CaseNumber} already exists.");

            // Map DTO to existing entity
            _mapper.Map(caseUpdateDto, existingCase);

            await _context.SaveChangesAsync();

            // Load related entities for mapping
            var updatedCase = await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CaseId == caseId);

            // Map back to DTO
            return _mapper.Map<CaseDetailDTO>(updatedCase);
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