using AutoMapper;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Services.Implementations
{
    public class CourtService : ICourtService
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;

        public CourtService(LitigatorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourtDTO>> GetAllCourtsAsync()
        {
            var courts = await _context.Courts
                .Include(c => c.Cases)
                .OrderBy(c => c.State)
                .ThenBy(c => c.County)
                .ThenBy(c => c.CourtName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourtDTO>>(courts);
        }

        public async Task<CourtDetailDTO?> GetCourtByIdAsync(int id)
        {
            var court = await _context.Courts
                .Include(c => c.Cases)
                    .ThenInclude(case_ => case_.Client)
                .Include(c => c.ChiefJudge)
                .FirstOrDefaultAsync(c => c.CourtId == id);

            return court != null ? _mapper.Map<CourtDetailDTO>(court) : null;
        }

        public async Task<IEnumerable<CourtDTO>> GetCourtsByStateAsync(string state)
        {
            var courts = await _context.Courts
                .Include(c => c.Cases)
                .Where(c => c.State.ToLower() == state.ToLower())
                .OrderBy(c => c.County)
                .ThenBy(c => c.CourtName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourtDTO>>(courts);
        }

        public async Task<IEnumerable<CourtDTO>> GetCourtsByCountyAsync(string state, string county)
        {
            var courts = await _context.Courts
                .Include(c => c.Cases)
                .Where(c => c.State.ToLower() == state.ToLower() && c.County.ToLower() == county.ToLower())
                .OrderBy(c => c.CourtName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourtDTO>>(courts);
        }

        public async Task<CourtDetailDTO?> GetCourtByNameAsync(string courtName, string county, string state)
        {
            var court = await _context.Courts
                .Include(c => c.Cases)
                    .ThenInclude(case_ => case_.Client)
                .Include(c => c.ChiefJudge)
                .FirstOrDefaultAsync(c =>
                    c.CourtName.ToLower() == courtName.ToLower() &&
                    c.County.ToLower() == county.ToLower() &&
                    c.State.ToLower() == state.ToLower());

            return court != null ? _mapper.Map<CourtDetailDTO>(court) : null;
        }

        public async Task<IEnumerable<CourtDTO>> GetActiveCourtsAsync()
        {
            var courts = await _context.Courts
                .Include(c => c.Cases)
                .Where(c => c.IsActive)
                .OrderBy(c => c.State)
                .ThenBy(c => c.County)
                .ThenBy(c => c.CourtName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourtDTO>>(courts);
        }

        public async Task<CourtDetailDTO> CreateCourtAsync(CreateCourtDTO createCourtDto)
        {
            // Validate court name uniqueness within the same county/state
            var existingCourt = await _context.Courts
                .FirstOrDefaultAsync(c =>
                    c.CourtName == createCourtDto.CourtName &&
                    c.Address.County == createCourtDto.County &&
                    c.Address.State == createCourtDto.State);

            if (existingCourt != null)
            {
                throw new InvalidOperationException($"Court '{createCourtDto.CourtName}' already exists in {createCourtDto.County} County, {createCourtDto.State}.");
            }

            var court = new Court
            {
                CourtName = createCourtDto.CourtName,
                CourtType = createCourtDto.CourtType,
                Division = createCourtDto.Division,
                Description = createCourtDto.Description,
                Email = createCourtDto.Email,
                Website = createCourtDto.Website,
                Address = createCourtDto.Address != null ? _mapper.Map<Address>(createCourtDto.Address) :
                    new Address
                    {
                        County = createCourtDto.County,
                        State = createCourtDto.State
                    },
                Phone = new DataAccess.ValueObjects.PhoneNumber
                {
                    Number = createCourtDto.Phone ?? string.Empty
                },
                ClerkOfCourt = createCourtDto.ClerkOfCourt,
                BusinessHours = createCourtDto.BusinessHours,
                IsActive = createCourtDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            return await GetCourtByIdAsync(court.CourtId) ?? _mapper.Map<CourtDetailDTO>(court);
        }

        public async Task<CourtDetailDTO> UpdateCourtAsync(UpdateCourtDTO updateCourtDto)
        {
            var existingCourt = await _context.Courts.FindAsync(updateCourtDto.CourtId);
            if (existingCourt == null)
            {
                throw new InvalidOperationException($"Court with ID {updateCourtDto.CourtId} not found.");
            }

            // Check court name uniqueness if being changed
            if (existingCourt.CourtName != updateCourtDto.CourtName ||
                existingCourt.County != updateCourtDto.County ||
                existingCourt.State != updateCourtDto.State)
            {
                var duplicateCourt = await _context.Courts
                    .FirstOrDefaultAsync(c =>
                        c.CourtName == updateCourtDto.CourtName &&
                        c.County == updateCourtDto.County &&
                        c.State == updateCourtDto.State &&
                        c.CourtId != updateCourtDto.CourtId);

                if (duplicateCourt != null)
                {
                    throw new InvalidOperationException($"Court '{updateCourtDto.CourtName}' already exists in {updateCourtDto.County} County, {updateCourtDto.State}.");
                }
            }

            // Update properties
            existingCourt.CourtName = updateCourtDto.CourtName;
            existingCourt.CourtType = updateCourtDto.CourtType;
            existingCourt.Division = updateCourtDto.Division;
            existingCourt.Description = updateCourtDto.Description;
            existingCourt.Email = updateCourtDto.Email;
            existingCourt.Website = updateCourtDto.Website;

            if (updateCourtDto.Address != null)
            {
                existingCourt.Address = _mapper.Map<Address>(updateCourtDto.Address);
                // Ensure County and State are properly set
                if (string.IsNullOrEmpty(existingCourt.Address.County))
                    existingCourt.Address.County = updateCourtDto.County;
                if (string.IsNullOrEmpty(existingCourt.Address.State))
                    existingCourt.Address.State = updateCourtDto.State;
            }
            else
            {
                // If no address provided, update at least County and State
                existingCourt.Address.County = updateCourtDto.County;
                existingCourt.Address.State = updateCourtDto.State;
            }

            existingCourt.Phone = new DataAccess.ValueObjects.PhoneNumber
            {
                Number = updateCourtDto.Phone ?? string.Empty
            };
            existingCourt.ClerkOfCourt = updateCourtDto.ClerkOfCourt;
            existingCourt.BusinessHours = updateCourtDto.BusinessHours;
            existingCourt.IsActive = updateCourtDto.IsActive;
            existingCourt.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCourtByIdAsync(existingCourt.CourtId) ?? _mapper.Map<CourtDetailDTO>(existingCourt);
        }

        public async Task<Court?> GetCourtEntityByIdAsync(int id)
        {
            return await _context.Courts
                .Include(c => c.Cases)
                .Include(c => c.ChiefJudge)
                .FirstOrDefaultAsync(c => c.CourtId == id);
        }

        public async Task<bool> CourtExistsAsync(string courtName, string county, string state, int? excludeCourtId = null)
        {
            var query = _context.Courts.Where(c =>
                c.CourtName.ToLower() == courtName.ToLower() &&
                c.County.ToLower() == county.ToLower() &&
                c.State.ToLower() == state.ToLower());

            if (excludeCourtId.HasValue)
            {
                query = query.Where(c => c.CourtId != excludeCourtId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<string>> GetStatesAsync()
        {
            return await _context.Courts
                .Select(c => c.State)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCountiesByStateAsync(string state)
        {
            return await _context.Courts
                .Where(c => c.State.ToLower() == state.ToLower())
                .Select(c => c.County)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
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
    }
}