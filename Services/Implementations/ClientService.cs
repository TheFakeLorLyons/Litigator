using AutoMapper;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;

namespace Litigator.Services.Implementations
{
    public class ClientService : IClientService
    {
        private readonly LitigatorDbContext _context;
        private readonly IMapper _mapper;

        public ClientService(LitigatorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ClientDTO>> GetAllClientsAsync()
        {
            var clients = await _context.Clients
                .Include(c => c.Cases)
                .OrderBy(c => c.Name.Last)
                .ThenBy(c => c.Name.First)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClientDTO>>(clients);
        }

        public async Task<ClientDetailDTO?> GetClientByIdAsync(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Cases)
                    .ThenInclude(Case => Case.AssignedAttorney)
                .Include(c => c.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(c => c.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(c => c.SystemId == id);

                return client == null ? null : _mapper.Map<ClientDetailDTO>(client);
            }

        public async Task<IEnumerable<ClientDTO>> SearchClientsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllClientsAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            var clients = await _context.Clients
                .Include(c => c.Cases)
                .Where(c =>
                    c.Name.First.ToLower().Contains(lowerSearchTerm) ||
                    (c.Name.Last != null && c.Name.Last.ToLower().Contains(lowerSearchTerm)) ||
                    (c.Email != null && c.Email.ToLower().Contains(lowerSearchTerm)) ||
                    (c.PrimaryPhone != null && c.PrimaryPhone.Number != null && c.PrimaryPhone.Number.Contains(searchTerm)))
                .OrderBy(c => c.Name.Last)
                .ThenBy(c => c.Name.First)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClientDTO>>(clients);
        }

        public async Task<IEnumerable<ClientDTO>> GetActiveClientsAsync()
        {
            var clients = await _context.Clients
                .Include(c => c.Cases)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name.Last)
                .ThenBy(c => c.Name.First)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClientDTO>>(clients);
        }

        public async Task<ClientDetailDTO> CreateClientAsync(ClientDetailDTO clientDto)
        {
            // Validate email uniqueness
            if (!string.IsNullOrWhiteSpace(clientDto.Email))
            {
                var existingByEmail = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == clientDto.Email);
                if (existingByEmail != null)
                {
                    throw new InvalidOperationException($"Client with email {clientDto.Email} already exists.");
                }
            }

            var client = _mapper.Map<Client>(clientDto);
            client.CreatedDate = DateTime.UtcNow;
            client.IsActive = true;

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return await GetClientByIdAsync(client.SystemId)
                ?? throw new InvalidOperationException("Failed to retrieve created client.");
        }

        public async Task<ClientDetailDTO> UpdateClientAsync(ClientDetailDTO clientDto)
        {
            var existingClient = await _context.Clients.FindAsync(clientDto.ClientId);
            if (existingClient == null)
            {
                throw new InvalidOperationException($"Client with ID {clientDto.ClientId} not found.");
            }

            // Check email uniqueness if being changed
            if (existingClient.Email != clientDto.Email && !string.IsNullOrWhiteSpace(clientDto.Email))
            {
                var duplicateEmail = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == clientDto.Email);
                if (duplicateEmail != null)
                {
                    throw new InvalidOperationException($"Client with email {clientDto.Email} already exists.");
                }
            }

            // Map the DTO to the existing entity
            _mapper.Map(clientDto, existingClient);
            existingClient.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetClientByIdAsync(clientDto.ClientId)
                ?? throw new InvalidOperationException("Failed to retrieve updated client.");
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return false;
            }

            // Check if client has active cases
            var activeCases = await _context.Cases
                .Where(c => c.ClientId == id && c.Status == "Active")
                .CountAsync();

            if (activeCases > 0)
            {
                throw new InvalidOperationException($"Cannot delete client with {activeCases} active cases. Please close cases first or reassign them.");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Client?> GetClientEntityByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Cases)
                    .ThenInclude(Case => Case.AssignedAttorney)
                .Include(c => c.AdditionalAddresses)
                    .ThenInclude(aa => aa.Address)
                .Include(c => c.AdditionalPhones)
                    .ThenInclude(ap => ap.PhoneNumber)
                .FirstOrDefaultAsync(c => c.SystemId == id);
            }
        }
    }