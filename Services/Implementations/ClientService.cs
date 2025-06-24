using Litigator.Services.Interfaces;
using Litigator.DataAccess.Data;
using Litigator.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Litigator.Services.Implementations
{
    public class ClientService : IClientService
    {
        private readonly LitigatorDbContext _context;

        public ClientService(LitigatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _context.Clients
                .Include(c => c.Cases)
                .OrderBy(c => c.ClientName)
                .ToListAsync();
        }

        public async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Cases)
                    .ThenInclude(case_ => case_.AssignedAttorney)
                .FirstOrDefaultAsync(c => c.ClientId == id);
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            // Validate email uniqueness if provided
            if (!string.IsNullOrWhiteSpace(client.Email))
            {
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == client.Email);
                if (existingClient != null)
                {
                    throw new InvalidOperationException($"Client with email {client.Email} already exists.");
                }
            }

            client.CreatedDate = DateTime.Now;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return await GetClientByIdAsync(client.ClientId) ?? client;
        }

        public async Task<Client> UpdateClientAsync(Client client)
        {
            var existingClient = await _context.Clients.FindAsync(client.ClientId);
            if (existingClient == null)
            {
                throw new InvalidOperationException($"Client with ID {client.ClientId} not found.");
            }

            // Check email uniqueness if being changed
            if (!string.IsNullOrWhiteSpace(client.Email) &&
                existingClient.Email != client.Email)
            {
                var duplicateClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == client.Email);
                if (duplicateClient != null)
                {
                    throw new InvalidOperationException($"Client with email {client.Email} already exists.");
                }
            }

            _context.Entry(existingClient).CurrentValues.SetValues(client);
            await _context.SaveChangesAsync();

            return await GetClientByIdAsync(client.ClientId) ?? client;
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
                throw new InvalidOperationException($"Cannot delete client with {activeCases} active cases. Please close or reassign cases first.");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Clients
                .Include(c => c.Cases)
                .Where(c =>
                    c.ClientName.ToLower().Contains(term) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)) ||
                    (c.Phone != null && c.Phone.Contains(searchTerm)) ||
                    (c.Address != null && c.Address.ToLower().Contains(term)))
                .OrderBy(c => c.ClientName)
                .ToListAsync();
        }
    }
}