using Microsoft.AspNetCore.Mvc;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attorney>>> GetAllAttorneys()
        {
            var attorneys = await _attorneyService.GetAllAttorneysAsync();
            return Ok(attorneys);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Attorney>> GetAttorney(int id)
        {
            var attorney = await _attorneyService.GetAttorneyByIdAsync(id);
            if (attorney == null)
                return NotFound($"Attorney with ID {id} not found.");

            return Ok(attorney);
        }

        [HttpGet("bar/{barNumber}")]
        public async Task<ActionResult<Attorney>> GetAttorneyByBarNumber(string barNumber)
        {
            var attorney = await _attorneyService.GetAttorneyByBarNumberAsync(barNumber);
            if (attorney == null)
                return NotFound($"Attorney with bar number {barNumber} not found.");

            return Ok(attorney);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Attorney>>> GetActiveAttorneys()
        {
            var attorneys = await _attorneyService.GetActiveAttorneysAsync();
            return Ok(attorneys);
        }

        [HttpPost]
        public async Task<ActionResult<Attorney>> CreateAttorney([FromBody] Attorney attorney)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdAttorney = await _attorneyService.CreateAttorneyAsync(attorney);
                return CreatedAtAction(nameof(GetAttorney), new { id = createdAttorney.AttorneyId }, createdAttorney);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating attorney: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Attorney>> UpdateAttorney(int id, [FromBody] Attorney attorney)
        {
            if (id != attorney.AttorneyId)
                return BadRequest("Attorney ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedAttorney = await _attorneyService.UpdateAttorneyAsync(attorney);
                return Ok(updatedAttorney);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating attorney: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAttorney(int id)
        {
            var result = await _attorneyService.DeleteAttorneyAsync(id);
            if (!result)
                return NotFound($"Attorney with ID {id} not found.");

            return NoContent();
        }
    }
}