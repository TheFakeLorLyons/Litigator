using Litigator.Services.Interfaces;
using Litigator.Models.DTOs.ClassDTOs;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtController : ControllerBase
    {
        private readonly ICourtService _courtService;
        private readonly ILogger<CourtController> _logger;

        public CourtController(ICourtService courtService, ILogger<CourtController> logger)
        {
            _courtService = courtService;
            _logger = logger;
        }

        /// <summary>
        /// Get all courts
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourtDTO>>> GetAllCourts()
        {
            try
            {
                var courts = await _courtService.GetAllCourtsAsync();
                return Ok(courts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all courts");
                return StatusCode(500, "An error occurred while retrieving courts");
            }
        }

        /// <summary>
        /// Get court by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CourtDetailDTO>> GetCourtById(int id)
        {
            try
            {
                var court = await _courtService.GetCourtByIdAsync(id);
                if (court == null)
                {
                    return NotFound($"Court with ID {id} not found");
                }
                return Ok(court);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving court with ID {CourtId}", id);
                return StatusCode(500, "An error occurred while retrieving the court");
            }
        }

        /// <summary>
        /// Get courts by state
        /// </summary>
        [HttpGet("state/{state}")]
        public async Task<ActionResult<IEnumerable<CourtDTO>>> GetCourtsByState(string state)
        {
            try
            {
                var courts = await _courtService.GetCourtsByStateAsync(state);
                return Ok(courts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courts for state {State}", state);
                return StatusCode(500, "An error occurred while retrieving courts");
            }
        }

        /// <summary>
        /// Get courts by state and county
        /// </summary>
        [HttpGet("state/{state}/county/{county}")]
        public async Task<ActionResult<IEnumerable<CourtDTO>>> GetCourtsByCounty(string state, string county)
        {
            try
            {
                var courts = await _courtService.GetCourtsByCountyAsync(state, county);
                return Ok(courts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courts for {County} County, {State}", county, state);
                return StatusCode(500, "An error occurred while retrieving courts");
            }
        }

        /// <summary>
        /// Get active courts only
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CourtDTO>>> GetActiveCourts()
        {
            try
            {
                var courts = await _courtService.GetActiveCourtsAsync();
                return Ok(courts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active courts");
                return StatusCode(500, "An error occurred while retrieving active courts");
            }
        }

        /// <summary>
        /// Get court by name, county, and state
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<CourtDetailDTO>> GetCourtByName([FromQuery] string courtName, [FromQuery] string county, [FromQuery] string state)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(courtName) || string.IsNullOrWhiteSpace(county) || string.IsNullOrWhiteSpace(state))
                {
                    return BadRequest("Court name, county, and state are required");
                }

                var court = await _courtService.GetCourtByNameAsync(courtName, county, state);
                if (court == null)
                {
                    return NotFound($"Court '{courtName}' not found in {county} County, {state}");
                }
                return Ok(court);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving court {CourtName} in {County}, {State}", courtName, county, state);
                return StatusCode(500, "An error occurred while retrieving the court");
            }
        }

        /// <summary>
        /// Create a new court
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CourtDetailDTO>> CreateCourt([FromBody] CreateCourtDTO createCourtDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var court = await _courtService.CreateCourtAsync(createCourtDto);
                return CreatedAtAction(nameof(GetCourtById), new { id = court.CourtId }, court);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating court");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating court");
                return StatusCode(500, "An error occurred while creating the court");
            }
        }

        /// <summary>
        /// Update an existing court
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CourtDetailDTO>> UpdateCourt(int id, [FromBody] UpdateCourtDTO updateCourtDto)
        {
            try
            {
                if (id != updateCourtDto.CourtId)
                {
                    return BadRequest("Court ID in URL does not match Court ID in request body");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var court = await _courtService.UpdateCourtAsync(updateCourtDto);
                return Ok(court);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating court {CourtId}", id);
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating court {CourtId}", id);
                return StatusCode(500, "An error occurred while updating the court");
            }
        }

        /// <summary>
        /// Delete a court
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCourt(int id)
        {
            try
            {
                var result = await _courtService.DeleteCourtAsync(id);
                if (!result)
                {
                    return NotFound($"Court with ID {id} not found");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting court {CourtId}", id);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting court {CourtId}", id);
                return StatusCode(500, "An error occurred while deleting the court");
            }
        }

        /// <summary>
        /// Get all unique states
        /// </summary>
        [HttpGet("states")]
        public async Task<ActionResult<IEnumerable<string>>> GetStates()
        {
            try
            {
                var states = await _courtService.GetStatesAsync();
                return Ok(states);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving states");
                return StatusCode(500, "An error occurred while retrieving states");
            }
        }

        /// <summary>
        /// Get counties for a specific state
        /// </summary>
        [HttpGet("state/{state}/counties")]
        public async Task<ActionResult<IEnumerable<string>>> GetCountiesByState(string state)
        {
            try
            {
                var counties = await _courtService.GetCountiesByStateAsync(state);
                return Ok(counties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving counties for state {State}", state);
                return StatusCode(500, "An error occurred while retrieving counties");
            }
        }

        /// <summary>
        /// Check if a court exists
        /// </summary>
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> CourtExists([FromQuery] string courtName, [FromQuery] string county, [FromQuery] string state, [FromQuery] int? excludeCourtId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(courtName) || string.IsNullOrWhiteSpace(county) || string.IsNullOrWhiteSpace(state))
                {
                    return BadRequest("Court name, county, and state are required");
                }

                var exists = await _courtService.CourtExistsAsync(courtName, county, state, excludeCourtId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if court exists");
                return StatusCode(500, "An error occurred while checking court existence");
            }
        }
    }
}