using Microsoft.AspNetCore.Mvc;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetAllClients()
        {
            var clients = await _clientService.GetAllClientsAsync();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
                return NotFound($"Client with ID {id} not found.");

            return Ok(client);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Client>>> SearchClients([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required.");

            var clients = await _clientService.SearchClientsAsync(searchTerm);
            return Ok(clients);
        }

        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient([FromBody] Client client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdClient = await _clientService.CreateClientAsync(client);
                return CreatedAtAction(nameof(GetClient), new { id = createdClient.ClientId }, createdClient);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating client: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Client>> UpdateClient(int id, [FromBody] Client client)
        {
            if (id != client.ClientId)
                return BadRequest("Client ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedClient = await _clientService.UpdateClientAsync(client);
                return Ok(updatedClient);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating client: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteClient(int id)
        {
            var result = await _clientService.DeleteClientAsync(id);
            if (!result)
                return NotFound($"Client with ID {id} not found.");

            return NoContent();
        }
    }
}