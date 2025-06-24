using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;

namespace Litigator.Services.Implementations
{
    public class AttorneyService : IAttorneyService
    {
        private readonly LitigatorDbContext _context;

        public AttorneyService(LitigatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attorney>> GetAllAttorneysAsync()
        {
            return await _context.Attorneys
                .Include(a => a.Cases)
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }

        public async Task<Attorney?> GetAttorneyByIdAsync(int id)
        {
            return await _context.Attorneys
                .Include(a => a.Cases)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(a => a.AttorneyId == id);
        }

        public async Task<Attorney?> GetAttorneyByBarNumberAsync(string barNumber)
        {
            return await _context.Attorneys
                .Include(a => a.Cases)
                .FirstOrDefaultAsync(a => a.BarNumber == barNumber);
        }

        public async Task<Attorney> CreateAttorneyAsync(Attorney attorney)
        {
            // Validate bar number uniqueness
            var existingByBarNumber = await GetAttorneyByBarNumberAsync(attorney.BarNumber);
            if (existingByBarNumber != null)
            {
                throw new InvalidOperationException($"Attorney with bar number {attorney.BarNumber} already exists.");
            }

            // Validate email uniqueness
            var existingByEmail = await _context.Attorneys
                .FirstOrDefaultAsync(a => a.Email == attorney.Email);
            if (existingByEmail != null)
            {
                throw new InvalidOperationException($"Attorney with email {attorney.Email} already exists.");
            }

            attorney.CreatedDate = DateTime.Now;
            attorney.IsActive = true; // Default to active
            _context.Attorneys.Add(attorney);
            await _context.SaveChangesAsync();

            return await GetAttorneyByIdAsync(attorney.AttorneyId) ?? attorney;
        }

        public async Task<Attorney> UpdateAttorneyAsync(Attorney attorney)
        {
            var existingAttorney = await _context.Attorneys.FindAsync(attorney.AttorneyId);
            if (existingAttorney == null)
            {
                throw new InvalidOperationException($"Attorney with ID {attorney.AttorneyId} not found.");
            }

            // Check bar number uniqueness if being changed
            if (existingAttorney.BarNumber != attorney.BarNumber)
            {
                var duplicateBarNumber = await GetAttorneyByBarNumberAsync(attorney.BarNumber);
                if (duplicateBarNumber != null)
                {
                    throw new InvalidOperationException($"Attorney with bar number {attorney.BarNumber} already exists.");
                }
            }

            // Check email uniqueness if being changed
            if (existingAttorney.Email != attorney.Email)
            {
                var duplicateEmail = await _context.Attorneys
                    .FirstOrDefaultAsync(a => a.Email == attorney.Email);
                if (duplicateEmail != null)
                {
                    throw new InvalidOperationException($"Attorney with email {attorney.Email} already exists.");
                }
            }

            _context.Entry(existingAttorney).CurrentValues.SetValues(attorney);
            await _context.SaveChangesAsync();

            return await GetAttorneyByIdAsync(attorney.AttorneyId) ?? attorney;
        }

        public async Task<bool> DeleteAttorneyAsync(int id)
        {
            var attorney = await _context.Attorneys.FindAsync(id);
            if (attorney == null)
            {
                return false;
            }

            // Check if attorney has active cases
            var activeCases = await _context.Cases
                .Where(c => c.AssignedAttorneyId == id && c.Status == "Active")
                .CountAsync();

            if (activeCases > 0)
            {
                throw new InvalidOperationException($"Cannot delete attorney with {activeCases} active cases. Please reassign cases first.");
            }

            _context.Attorneys.Remove(attorney);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Attorney>> GetActiveAttorneysAsync()
        {
            return await _context.Attorneys
                .Include(a => a.Cases)
                .Where(a => a.IsActive)
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }
    }
}