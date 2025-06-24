using Litigator.Services.Interfaces;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Litigator.Services.Implementations
{
    public class DeadlineService : IDeadlineService
    {
        private readonly LitigatorDbContext _context;

        public DeadlineService(LitigatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Deadline>> GetUpcomingDeadlinesAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(days);

            return await _context.Deadlines
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Include(d => d.Case)
                    .ThenInclude(c => c.AssignedAttorney)
                .Where(d => !d.IsCompleted &&
                           d.DeadlineDate >= DateTime.Now &&
                           d.DeadlineDate <= cutoffDate)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Deadline>> GetOverdueDeadlinesAsync()
        {
            return await _context.Deadlines
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Include(d => d.Case)
                    .ThenInclude(c => c.AssignedAttorney)
                .Where(d => !d.IsCompleted && d.DeadlineDate < DateTime.Now)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Deadline>> GetDeadlinesByCaseAsync(int caseId)
        {
            return await _context.Deadlines
                .Include(d => d.Case)
                .Where(d => d.CaseId == caseId)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }

        public async Task<Deadline> CreateDeadlineAsync(Deadline deadline)
        {
            // Validate case exists
            var caseExists = await _context.Cases.AnyAsync(c => c.CaseId == deadline.CaseId);
            if (!caseExists)
            {
                throw new InvalidOperationException($"Case with ID {deadline.CaseId} not found.");
            }

            // Validate deadline date is not in the past (unless explicitly allowed)
            if (deadline.DeadlineDate < DateTime.Now.Date)
            {
                throw new InvalidOperationException("Deadline date cannot be in the past.");
            }

            deadline.CreatedDate = DateTime.Now;
            _context.Deadlines.Add(deadline);
            await _context.SaveChangesAsync();

            return await _context.Deadlines
                .Include(d => d.Case)
                .FirstOrDefaultAsync(d => d.DeadlineId == deadline.DeadlineId) ?? deadline;
        }

        public async Task<Deadline> UpdateDeadlineAsync(Deadline deadline)
        {
            var existingDeadline = await _context.Deadlines.FindAsync(deadline.DeadlineId);
            if (existingDeadline == null)
            {
                throw new InvalidOperationException($"Deadline with ID {deadline.DeadlineId} not found.");
            }

            // Validate case exists if being changed
            if (existingDeadline.CaseId != deadline.CaseId)
            {
                var caseExists = await _context.Cases.AnyAsync(c => c.CaseId == deadline.CaseId);
                if (!caseExists)
                {
                    throw new InvalidOperationException($"Case with ID {deadline.CaseId} not found.");
                }
            }

            _context.Entry(existingDeadline).CurrentValues.SetValues(deadline);
            await _context.SaveChangesAsync();

            return await _context.Deadlines
                .Include(d => d.Case)
                .FirstOrDefaultAsync(d => d.DeadlineId == deadline.DeadlineId) ?? deadline;
        }

        public async Task<bool> DeleteDeadlineAsync(int id)
        {
            var deadline = await _context.Deadlines.FindAsync(id);
            if (deadline == null)
            {
                return false;
            }

            _context.Deadlines.Remove(deadline);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkDeadlineCompleteAsync(int id)
        {
            var deadline = await _context.Deadlines.FindAsync(id);
            if (deadline == null)
            {
                return false;
            }

            deadline.IsCompleted = true;
            deadline.CompletedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Deadline>> GetCriticalDeadlinesAsync()
        {
            return await _context.Deadlines
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Include(d => d.Case)
                    .ThenInclude(c => c.AssignedAttorney)
                .Where(d => d.IsCritical && !d.IsCompleted)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }
    }
}