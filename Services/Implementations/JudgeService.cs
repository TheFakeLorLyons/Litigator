using AutoMapper;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Implementations
{
    public class JudgeService : IJudgeService
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;

        public JudgeService(LitigatorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<JudgeDTO>> GetAllJudgesAsync()
        {
            var judges = await _context.Judges
                .Include(j => j.Cases)
                .OrderBy(j => j.Name.Last)
                .ThenBy(j => j.Name.First)
                .ToListAsync();

            return _mapper.Map<IEnumerable<JudgeDTO>>(judges);
        }

        public async Task<JudgeDetailDTO?> GetJudgeByIdAsync(int id)
        {
            var judge = await _context.Judges
                .Include(j => j.Cases)
                    .ThenInclude(c => c.Client)
                .Include(j => j.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(j => j.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(j => j.SystemId == id);

            return judge == null ? null : _mapper.Map<JudgeDetailDTO>(judge);
        }

        public async Task<JudgeDetailDTO?> GetJudgeByBarNumberAsync(string barNumber)
        {
            var judge = await _context.Judges
                .Include(j => j.Cases)
                .Include(j => j.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(j => j.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(j => j.BarNumber == barNumber);

            return judge == null ? null : _mapper.Map<JudgeDetailDTO>(judge);
        }

        public async Task<JudgeDetailDTO> CreateJudgeAsync(JudgeDetailDTO judgeDto)
        {
            // Validate bar number uniqueness
            var existingByBarNumber = await _context.Judges
                .FirstOrDefaultAsync(j => j.BarNumber == judgeDto.BarNumber);
            if (existingByBarNumber != null)
            {
                throw new InvalidOperationException($"Judge with bar number {judgeDto.BarNumber} already exists.");
            }

            // Validate email uniqueness
            if (!string.IsNullOrWhiteSpace(judgeDto.Email))
            {
                var existingByEmail = await _context.Judges
                    .FirstOrDefaultAsync(j => j.Email == judgeDto.Email);
                if (existingByEmail != null)
                {
                    throw new InvalidOperationException($"Judge with email {judgeDto.Email} already exists.");
                }
            }

            var judge = _mapper.Map<Judge>(judgeDto);
            judge.CreatedDate = DateTime.UtcNow;
            judge.IsActive = true;

            _context.Judges.Add(judge);
            await _context.SaveChangesAsync();

            return await GetJudgeByIdAsync(judge.SystemId)
                ?? throw new InvalidOperationException("Failed to retrieve created judge.");
        }

        public async Task<JudgeDetailDTO> UpdateJudgeAsync(JudgeDetailDTO judgeDto)
        {
            var existingJudge = await _context.Judges.FindAsync(judgeDto.JudgeId);
            if (existingJudge == null)
            {
                throw new InvalidOperationException($"Judge with ID {judgeDto.JudgeId} not found.");
            }

            // Check bar number uniqueness if being changed
            if (existingJudge.BarNumber != judgeDto.BarNumber)
            {
                var duplicateBarNumber = await _context.Judges
                    .FirstOrDefaultAsync(j => j.BarNumber == judgeDto.BarNumber);
                if (duplicateBarNumber != null)
                {
                    throw new InvalidOperationException($"Judge with bar number {judgeDto.BarNumber} already exists.");
                }
            }

            // Check email uniqueness if being changed
            if (existingJudge.Email != judgeDto.Email)
            {
                var duplicateEmail = await _context.Judges
                    .FirstOrDefaultAsync(j => j.Email == judgeDto.Email);
                if (duplicateEmail != null)
                {
                    throw new InvalidOperationException($"Judge with email {judgeDto.Email} already exists.");
                }
            }

            // Map the DTO to the existing entity
            _mapper.Map(judgeDto, existingJudge);
            existingJudge.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetJudgeByIdAsync(judgeDto.JudgeId)
                ?? throw new InvalidOperationException("Failed to retrieve updated judge.");
        }

        public async Task<bool> DeleteJudgeAsync(int id)
        {
            var judge = await _context.Judges.FindAsync(id);
            if (judge == null)
            {
                return false;
            }

            // Check if judge has active cases
            var activeCases = await _context.Cases
                .Where(c => c.AssignedJudgeId == id && c.Status == "Active")
                .CountAsync();

            if (activeCases > 0)
            {
                throw new InvalidOperationException($"Cannot delete judge with {activeCases} active cases. Please reassign cases first.");
            }

            _context.Judges.Remove(judge);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<JudgeDTO>> GetActiveJudgesAsync()
        {
            var judges = await _context.Judges
                .Include(j => j.Cases)
                .Where(j => j.IsActive)
                .OrderBy(j => j.Name.Last)
                .ThenBy(j => j.Name.First)
                .ToListAsync();

            return _mapper.Map<IEnumerable<JudgeDTO>>(judges);
        }

        public async Task<Judge?> GetJudgeEntityByIdAsync(int id)
        {
            return await _context.Judges
                .Include(j => j.Cases)
                    .ThenInclude(c => c.Client)
                .Include(j => j.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(j => j.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(j => j.SystemId == id);
        }
    }
}