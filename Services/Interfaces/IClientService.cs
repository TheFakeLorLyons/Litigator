using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Services.Interfaces
{
    public interface IClientService
    {
        // Read operations - return DTOs
        Task<IEnumerable<ClientDTO>> GetAllClientsAsync();
        Task<ClientDetailDTO?> GetClientByIdAsync(int id);
        Task<IEnumerable<ClientDTO>> SearchClientsAsync(string searchTerm);
        Task<IEnumerable<ClientDTO>> GetActiveClientsAsync();

        // Write operations - accept and return DTOs, but work with entities internally
        Task<ClientDetailDTO> CreateClientAsync(ClientDetailDTO clientDto);
        Task<ClientDetailDTO> UpdateClientAsync(ClientDetailDTO clientDto);
        Task<bool> DeleteClientAsync(int id);

        // Entity access for business logic
        Task<Client?> GetClientEntityByIdAsync(int id);
    }
}