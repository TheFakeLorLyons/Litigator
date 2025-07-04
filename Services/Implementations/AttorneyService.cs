using AutoMapper;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;

namespace Litigator.Services.Implementations
{
    public class AttorneyService : IAttorneyService
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;

        public AttorneyService(LitigatorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AttorneyDTO>> GetAllAttorneysAsync()
        {
            var attorneys = await _context.Attorneys
                .Include(a => a.Cases)
                .OrderBy(a => a.Name.Last)
                .ThenBy(a => a.Name.First)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AttorneyDTO>>(attorneys);
        }

        public async Task<AttorneyDetailDTO?> GetAttorneyByIdAsync(int id)
        {
            var attorney = await _context.Attorneys
                .Include(a => a.Cases)
                    .ThenInclude(c => c.Client)
                .Include(a => a.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(a => a.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(a => a.SystemId == id);

            return attorney == null ? null : _mapper.Map<AttorneyDetailDTO>(attorney);
        }

        public async Task<AttorneyDetailDTO?> GetAttorneyByBarNumberAsync(string barNumber)
        {
            var attorney = await _context.Attorneys
                .Include(a => a.Cases)
                .Include(a => a.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(a => a.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(a => a.BarNumber == barNumber);

            return attorney == null ? null : _mapper.Map<AttorneyDetailDTO>(attorney);
        }

        public async Task<AttorneyDetailDTO> CreateAttorneyAsync(AttorneyDetailDTO attorneyDto)
        {
            // Validate bar number uniqueness
            var existingByBarNumber = await _context.Attorneys
                .FirstOrDefaultAsync(a => a.BarNumber == attorneyDto.BarNumber);
            if (existingByBarNumber != null)
            {
                throw new InvalidOperationException($"Attorney with bar number {attorneyDto.BarNumber} already exists.");
            }

            // Validate email uniqueness
            if (!string.IsNullOrWhiteSpace(attorneyDto.Email))
            {
                var existingByEmail = await _context.Attorneys
                    .FirstOrDefaultAsync(a => a.Email == attorneyDto.Email);
                if (existingByEmail != null)
                {
                    throw new InvalidOperationException($"Attorney with email {attorneyDto.Email} already exists.");
                }
            }

            var attorney = _mapper.Map<Attorney>(attorneyDto);
            attorney.CreatedDate = DateTime.UtcNow;
            attorney.IsActive = true;

            _context.Attorneys.Add(attorney);
            await _context.SaveChangesAsync();

            return await GetAttorneyByIdAsync(attorney.SystemId)
                ?? throw new InvalidOperationException("Failed to retrieve created attorney.");
        }

        public async Task<AttorneyDetailDTO> UpdateAttorneyAsync(AttorneyDetailDTO attorneyDto)
        {
            var existingAttorney = await _context.Attorneys.FindAsync(attorneyDto.AttorneyId);
            if (existingAttorney == null)
            {
                throw new InvalidOperationException($"Attorney with ID {attorneyDto.AttorneyId} not found.");
            }

            // Check bar number uniqueness if being changed
            if (existingAttorney.BarNumber != attorneyDto.BarNumber)
            {
                var duplicateBarNumber = await _context.Attorneys
                    .FirstOrDefaultAsync(a => a.BarNumber == attorneyDto.BarNumber);
                if (duplicateBarNumber != null)
                {
                    throw new InvalidOperationException($"Attorney with bar number {attorneyDto.BarNumber} already exists.");
                }
            }

            // Check email uniqueness if being changed
            if (existingAttorney.Email != attorneyDto.Email)
            {
                var duplicateEmail = await _context.Attorneys
                    .FirstOrDefaultAsync(a => a.Email == attorneyDto.Email);
                if (duplicateEmail != null)
                {
                    throw new InvalidOperationException($"Attorney with email {attorneyDto.Email} already exists.");
                }
            }

            // Map the DTO to the existing entity
            _mapper.Map(attorneyDto, existingAttorney);
            existingAttorney.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetAttorneyByIdAsync(attorneyDto.AttorneyId)
                ?? throw new InvalidOperationException("Failed to retrieve updated attorney.");
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

        public async Task<IEnumerable<AttorneyDTO>> GetActiveAttorneysAsync()
        {
            var attorneys = await _context.Attorneys
                .Include(a => a.Cases)
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name.Last)
                .ThenBy(a => a.Name.First)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AttorneyDTO>>(attorneys);
        }

        public async Task<Attorney?> GetAttorneyEntityByIdAsync(int id)
        {
            return await _context.Attorneys
                .Include(a => a.Cases)
                    .ThenInclude(c => c.Client)
                .Include(a => a.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(a => a.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(a => a.SystemId == id);
        }
    }
}