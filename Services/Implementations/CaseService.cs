using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Case;
using Litigator.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                .Include(c => c.Deadlines) // Include deadlines to count open ones
                .Select(c => new CaseDTO
                {
                    CaseId = c.CaseId,
                    CaseNumber = c.CaseNumber,
                    CaseTitle = c.CaseTitle,
                    CaseType = c.CaseType,
                    FilingDate = c.FilingDate,
                    Status = c.Status,
                    EstimatedValue = c.EstimatedValue,
                    ClientName = c.Client.ClientName,
                    AttorneyFirstName = c.AssignedAttorney != null ? $"{c.AssignedAttorney.FirstName}" : null,
                    AttorneyLastName = c.AssignedAttorney != null ? $"{c.AssignedAttorney.LastName}" : null,
                    CourtName = c.Court != null ? c.Court.CourtName : null,
                    OpenDeadlines = c.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                })
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<Case?> GetCaseByIdAsync(int id)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Include(c => c.Deadlines)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CaseId == id);
        }

        public async Task<Case?> GetCaseByNumberAsync(string caseNumber)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .FirstOrDefaultAsync(c => c.CaseNumber == caseNumber);
        }

        public async Task<Case> CreateCaseAsync(Case case_)
        {
            // Validate that the case number is unique
            var existingCase = await GetCaseByNumberAsync(case_.CaseNumber);
            if (existingCase != null)
            {
                throw new InvalidOperationException($"Case number {case_.CaseNumber} already exists.");
            }

            // Validate foreign key references
            await ValidateForeignKeysAsync(case_);

            _context.Cases.Add(case_);
            await _context.SaveChangesAsync();

            return await GetCaseByIdAsync(case_.CaseId) ?? case_;
        }

        public async Task<Case> UpdateCaseAsync(Case case_)
        {
            var existingCase = await _context.Cases.FindAsync(case_.CaseId);
            if (existingCase == null)
            {
                throw new InvalidOperationException($"Case with ID {case_.CaseId} not found.");
            }

            // Check if case number is being changed and if it's unique
            if (existingCase.CaseNumber != case_.CaseNumber)
            {
                var duplicateCase = await GetCaseByNumberAsync(case_.CaseNumber);
                if (duplicateCase != null)
                {
                    throw new InvalidOperationException($"Case number {case_.CaseNumber} already exists.");
                }
            }

            await ValidateForeignKeysAsync(case_);

            _context.Entry(existingCase).CurrentValues.SetValues(case_);
            await _context.SaveChangesAsync();

            return await GetCaseByIdAsync(case_.CaseId) ?? case_;
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

        public async Task<IEnumerable<Case>> GetCasesByClientAsync(int clientId)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Case>> GetCasesByAttorneyAsync(int attorneyId)
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Where(c => c.AssignedAttorneyId == attorneyId)
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Case>> GetActiveCasesAsync()
        {
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Where(c => c.Status == "Active")
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Case>> SearchCasesAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Cases
                .Include(c => c.Client)
                .Include(c => c.AssignedAttorney)
                .Include(c => c.Court)
                .Where(c =>
                    c.CaseNumber.ToLower().Contains(term) ||
                    c.CaseTitle.ToLower().Contains(term) ||
                    c.Client.ClientName.ToLower().Contains(term) ||
                    (c.AssignedAttorney != null &&
                     (c.AssignedAttorney.FirstName + " " + c.AssignedAttorney.LastName).ToLower().Contains(term)))
                .OrderByDescending(c => c.FilingDate)
                .ToListAsync();
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