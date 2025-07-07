using Microsoft.AspNetCore.Mvc;
using Litigator.Services.Interfaces;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttorneyController : ControllerBase
    {
        private readonly IAttorneyService _attorneyService;
        private readonly ILogger<AttorneyController> _logger;

        public AttorneyController(IAttorneyService attorneyService, ILogger<AttorneyController> logger)
        {
            _attorneyService = attorneyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all attorneys
        /// </summary>
        /// <returns>List of attorneys</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttorneyDTO>>> GetAllAttorneys()
        {
            try
            {
                var attorneys = await _attorneyService.GetAllAttorneysAsync();
                return Ok(attorneys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all attorneys");
                return StatusCode(500, "An error occurred while retrieving attorneys");
            }
        }

        /// <summary>
        /// Get active attorneys only
        /// </summary>
        /// <returns>List of active attorneys</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<AttorneyDTO>>> GetActiveAttorneys()
        {
            try
            {
                var attorneys = await _attorneyService.GetActiveAttorneysAsync();
                return Ok(attorneys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active attorneys");
                return StatusCode(500, "An error occurred while retrieving active attorneys");
            }
        }

        /// <summary>
        /// Get attorney by ID
        /// </summary>
        /// <param name="id">Attorney ID</param>
        /// <returns>Attorney details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AttorneyDetailDTO>> GetAttorneyById(int id)
        {
            try
            {
                var attorney = await _attorneyService.GetAttorneyByIdAsync(id);
                if (attorney == null)
                {
                    return NotFound($"Attorney with ID {id} not found");
                }
                return Ok(attorney);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attorney with ID {AttorneyId}", id);
                return StatusCode(500, "An error occurred while retrieving the attorney");
            }
        }

        /// <summary>
        /// Get attorney by bar number
        /// </summary>
        /// <param name="barNumber">Bar number</param>
        /// <returns>Attorney details</returns>
        [HttpGet("bar/{barNumber}")]
        public async Task<ActionResult<AttorneyDetailDTO>> GetAttorneyByBarNumber(string barNumber)
        {
            try
            {
                var attorney = await _attorneyService.GetAttorneyByBarNumberAsync(barNumber);
                if (attorney == null)
                {
                    return NotFound($"Attorney with bar number {barNumber} not found");
                }
                return Ok(attorney);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attorney with bar number {BarNumber}", barNumber);
                return StatusCode(500, "An error occurred while retrieving the attorney");
            }
        }

        /// <summary>
        /// Create a new attorney
        /// </summary>
        /// <param name="attorneyDto">Attorney data</param>
        /// <returns>Created attorney</returns>
        [HttpPost]
        public async Task<ActionResult<AttorneyDetailDTO>> CreateAttorney([FromBody] AttorneyDetailDTO attorneyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdAttorney = await _attorneyService.CreateAttorneyAsync(attorneyDto);
                return CreatedAtAction(
                    nameof(GetAttorneyById),
                    new { id = createdAttorney.AttorneyId },
                    createdAttorney);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating attorney");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attorney");
                return StatusCode(500, "An error occurred while creating the attorney");
            }
        }

        /// <summary>
        /// Update an existing attorney
        /// </summary>
        /// <param name="id">Attorney ID</param>
        /// <param name="attorneyDto">Updated attorney data</param>
        /// <returns>Updated attorney</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<AttorneyDetailDTO>> UpdateAttorney(int id, [FromBody] AttorneyDetailDTO attorneyDto)
        {
            if (id != attorneyDto.AttorneyId)
            {
                return BadRequest("Attorney ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedAttorney = await _attorneyService.UpdateAttorneyAsync(attorneyDto);
                return Ok(updatedAttorney);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating attorney with ID {AttorneyId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attorney with ID {AttorneyId}", id);
                return StatusCode(500, "An error occurred while updating the attorney");
            }
        }

        /// <summary>
        /// Delete an attorney
        /// </summary>
        /// <param name="id">Attorney ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAttorney(int id)
        {
            try
            {
                var result = await _attorneyService.DeleteAttorneyAsync(id);
                if (!result)
                {
                    return NotFound($"Attorney with ID {id} not found");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting attorney with ID {AttorneyId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attorney with ID {AttorneyId}", id);
                return StatusCode(500, "An error occurred while deleting the attorney");
            }
        }
    }
}