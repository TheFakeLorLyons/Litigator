using Litigator.Services.Interfaces;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Litigator.Services.Implementations
{
    public class CourtService : ICourtService
    {
        private readonly LitigatorDbContext _context;

        public CourtService(LitigatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Court>> GetAllCourtsAsync()
        {
            return await _context.Courts
                .Include(c => c.Cases)
                .OrderBy(c => c.State)
                .ThenBy(c => c.County)
                .ThenBy(c => c.CourtName)
                .ToListAsync();
        }

        public async Task<Court?> GetCourtByIdAsync(int id)
        {
            return await _context.Courts
                .Include(c => c.Cases)
                    .ThenInclude(case_ => case_.Client)
                .FirstOrDefaultAsync(c => c.CourtId == id);
        }

        public async Task<Court> CreateCourtAsync(Court court)
        {
            // Validate court name uniqueness within the same county/state
            var existingCourt = await _context.Courts
                .FirstOrDefaultAsync(c =>
                    c.CourtName == court.CourtName &&
                    c.County == court.County &&
                    c.State == court.State);

            if (existingCourt != null)
            {
                throw new InvalidOperationException($"Court '{court.CourtName}' already exists in {court.County} County, {court.State}.");
            }

            court.CreatedDate = DateTime.Now;
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            return await GetCourtByIdAsync(court.CourtId) ?? court;
        }

        public async Task<Court> UpdateCourtAsync(Court court)
        {
            var existingCourt = await _context.Courts.FindAsync(court.CourtId);
            if (existingCourt == null)
            {
                throw new InvalidOperationException($"Court with ID {court.CourtId} not found.");
            }

            // Check court name uniqueness if being changed
            if (existingCourt.CourtName != court.CourtName ||
                existingCourt.County != court.County ||
                existingCourt.State != court.State)
            {
                var duplicateCourt = await _context.Courts
                    .FirstOrDefaultAsync(c =>
                        c.CourtName == court.CourtName &&
                        c.County == court.County &&
                        c.State == court.State &&
                        c.CourtId != court.CourtId);

                if (duplicateCourt != null)
                {
                    throw new InvalidOperationException($"Court '{court.CourtName}' already exists in {court.County} County, {court.State}.");
                }
            }

            _context.Entry(existingCourt).CurrentValues.SetValues(court);
            await _context.SaveChangesAsync();

            return await GetCourtByIdAsync(court.CourtId) ?? court;
        }

        public async Task<bool> DeleteCourtAsync(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null)
            {
                return false;
            }

            // Check if court has active cases
            var activeCases = await _context.Cases
                .Where(c => c.CourtId == id && c.Status == "Active")
                .CountAsync();

            if (activeCases > 0)
            {
                throw new InvalidOperationException($"Cannot delete court with {activeCases} active cases. Please reassign cases first.");
            }

            _context.Courts.Remove(court);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Court>> GetCourtsByStateAsync(string state)
        {
            return await _context.Courts
                .Include(c => c.Cases)
                .Where(c => c.State.ToLower() == state.ToLower())
                .OrderBy(c => c.County)
                .ThenBy(c => c.CourtName)
                .ToListAsync();
        }
    }
}