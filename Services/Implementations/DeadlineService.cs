using AutoMapper;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.Mapping;
using Litigator.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Implementations
{
    public class DeadlineService : IDeadlineService
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;

        public DeadlineService(LitigatorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DeadlineDTO>> GetUpcomingDeadlinesAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(days);
            return await _context.Deadlines
                .Include(d => d.Case)
                .Where(d => !d.IsCompleted &&
                           d.DeadlineDate >= DateTime.Now &&
                           d.DeadlineDate <= cutoffDate)
                .Select(d => new DeadlineDTO
                {
                    DeadlineId = d.DeadlineId,
                    DeadlineType = d.DeadlineType,
                    Description = d.Description,
                    DeadlineDate = d.DeadlineDate,
                    IsCompleted = d.IsCompleted,
                    CompletedDate = d.CompletedDate,
                    IsCritical = d.IsCritical,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeadlineDTO>> GetOverdueDeadlinesAsync()
        {
            return await _context.Deadlines
                .Include(d => d.Case)
                .Where(d => !d.IsCompleted && d.DeadlineDate < DateTime.Now)
                .Select(d => new DeadlineDTO
                {
                    DeadlineId = d.DeadlineId,
                    DeadlineType = d.DeadlineType,
                    Description = d.Description,
                    DeadlineDate = d.DeadlineDate,
                    IsCompleted = d.IsCompleted,
                    CompletedDate = d.CompletedDate,
                    IsCritical = d.IsCritical,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }


        public async Task<IEnumerable<DeadlineDTO>> GetDeadlinesByCaseAsync(int caseId)
        {
            return await _context.Deadlines
                .Include(d => d.Case)
                .Where(d => d.CaseId == caseId)
                .Select(d => new DeadlineDTO
                {
                    DeadlineId = d.DeadlineId,
                    DeadlineType = d.DeadlineType,
                    Description = d.Description,
                    DeadlineDate = d.DeadlineDate,
                    IsCompleted = d.IsCompleted,
                    CompletedDate = d.CompletedDate,
                    IsCritical = d.IsCritical,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeadlineDTO>> GetAllDeadlinesAsync()
        {
            return await _context.Deadlines
                .Include(d => d.Case)
                .Select(d => new DeadlineDTO
                {
                    DeadlineId = d.DeadlineId,
                    DeadlineType = d.DeadlineType,
                    Description = d.Description,
                    DeadlineDate = d.DeadlineDate,
                    IsCompleted = d.IsCompleted,
                    CompletedDate = d.CompletedDate,
                    IsCritical = d.IsCritical,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();
        }

        public async Task<DeadlineDTO?> GetDeadlineByIdAsync(int id)
        {
            return await _context.Deadlines
                .Include(d => d.Case)
                .Where(d => d.DeadlineId == id)
                .Select(d => new DeadlineDTO
                {
                    DeadlineId = d.DeadlineId,
                    DeadlineType = d.DeadlineType,
                    Description = d.Description,
                    DeadlineDate = d.DeadlineDate,
                    IsCompleted = d.IsCompleted,
                    CompletedDate = d.CompletedDate,
                    IsCritical = d.IsCritical,
                    CaseId = d.CaseId,
                    CaseNumber = d.Case != null ? d.Case.CaseNumber : null,
                    CaseTitle = d.Case != null ? d.Case.CaseTitle : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DeadlineDTO> CreateDeadlineAsync(DeadlineCreateDTO createDto)
        {
            // Validate case exists
            var caseExists = await _context.Cases.AnyAsync(c => c.CaseId == createDto.CaseId);
            if (!caseExists)
            {
                throw new InvalidOperationException($"Case with ID {createDto.CaseId} not found.");
            }

            // Validate deadline date is not in the past (unless explicitly allowed)
            if (createDto.DeadlineDate < DateTime.Now.Date)
            {
                throw new InvalidOperationException("Deadline date cannot be in the past.");
            }

            var deadline = _mapper.Map<Deadline>(createDto);
            deadline.CreatedDate = DateTime.Now;

            _context.Deadlines.Add(deadline);
            await _context.SaveChangesAsync();

            var createdDeadline = await _context.Deadlines
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Include(d => d.Case)
                    .ThenInclude(c => c.AssignedAttorney)
                .FirstOrDefaultAsync(d => d.DeadlineId == deadline.DeadlineId);

            return _mapper.Map<DeadlineDTO>(createdDeadline);
        }

        public async Task<DeadlineDTO?> UpdateDeadlineAsync(int id, DeadlineUpdateDTO updateDto)
        {
            var existingDeadline = await _context.Deadlines.FindAsync(id);
            if (existingDeadline == null)
            {
                return null;
            }

            // Map the update DTO to the existing entity
            _mapper.Map(updateDto, existingDeadline);

            await _context.SaveChangesAsync();

            var updatedDeadline = await _context.Deadlines
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Include(d => d.Case)
                    .ThenInclude(c => c.AssignedAttorney)
                .FirstOrDefaultAsync(d => d.DeadlineId == id);

            return _mapper.Map<DeadlineDTO>(updatedDeadline);
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

        public async Task<IEnumerable<DeadlineDTO>> GetCriticalDeadlinesAsync()
        {
            var deadlines = await _context.Deadlines
                .Include(d => d.Case)
                    .ThenInclude(c => c.Client)
                .Include(d => d.Case)
                    .ThenInclude(c => c.AssignedAttorney)
                .Where(d => d.IsCritical && !d.IsCompleted)
                .OrderBy(d => d.DeadlineDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DeadlineDTO>>(deadlines);
        }
    }
}