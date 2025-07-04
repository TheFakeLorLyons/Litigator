using Litigator.Services.Interfaces;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Get all clients
        /// </summary>
        /// <returns>List of clients</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> GetAllClients()
        {
            try
            {
                var clients = await _clientService.GetAllClientsAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clients");
                return StatusCode(500, "An error occurred while retrieving clients");
            }
        }

        /// <summary>
        /// Get active clients only
        /// </summary>
        /// <returns>List of active clients</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> GetActiveClients()
        {
            try
            {
                var clients = await _clientService.GetActiveClientsAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active clients");
                return StatusCode(500, "An error occurred while retrieving active clients");
            }
        }

        /// <summary>
        /// Search clients by name, email, or phone
        /// </summary>
        /// <param name="q">Search term</param>
        /// <returns>List of matching clients</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> SearchClients([FromQuery] string q)
        {
            try
            {
                var clients = await _clientService.SearchClientsAsync(q);
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching clients with term {SearchTerm}", q);
                return StatusCode(500, "An error occurred while searching clients");
            }
        }

        /// <summary>
        /// Get client by ID
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <returns>Client details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDetailDTO>> GetClientById(int id)
        {
            try
            {
                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound($"Client with ID {id} not found");
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client with ID {ClientId}", id);
                return StatusCode(500, "An error occurred while retrieving the client");
            }
        }

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="clientDto">Client data</param>
        /// <returns>Created client</returns>
        [HttpPost]
        public async Task<ActionResult<ClientDetailDTO>> CreateClient([FromBody] ClientDetailDTO clientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdClient = await _clientService.CreateClientAsync(clientDto);
                return CreatedAtAction(
                    nameof(GetClientById),
                    new { id = createdClient.ClientId },
                    createdClient);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating client");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, "An error occurred while creating the client");
            }
        }

        /// <summary>
        /// Update an existing client
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="clientDto">Updated client data</param>
        /// <returns>Updated client</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ClientDetailDTO>> UpdateClient(int id, [FromBody] ClientDetailDTO clientDto)
        {
            if (id != clientDto.ClientId)
            {
                return BadRequest("Client ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedClient = await _clientService.UpdateClientAsync(clientDto);
                return Ok(updatedClient);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating client with ID {ClientId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client with ID {ClientId}", id);
                return StatusCode(500, "An error occurred while updating the client");
            }
        }

        /// <summary>
        /// Delete a client
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteClient(int id)
        {
            try
            {
                var result = await _clientService.DeleteClientAsync(id);
                if (!result)
                {
                    return NotFound($"Client with ID {id} not found");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting client with ID {ClientId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client with ID {ClientId}", id);
                return StatusCode(500, "An error occurred while deleting the client");
            }
        }
    }
}