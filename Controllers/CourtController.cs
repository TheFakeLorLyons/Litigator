using Microsoft.AspNetCore.Mvc;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Court>>> GetAllCourts()
        {
            var courts = await _courtService.GetAllCourtsAsync();
            return Ok(courts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Court>> GetCourt(int id)
        {
            var court = await _courtService.GetCourtByIdAsync(id);
            if (court == null)
                return NotFound($"Court with ID {id} not found.");

            return Ok(court);
        }

        [HttpGet("state/{state}")]
        public async Task<ActionResult<IEnumerable<Court>>> GetCourtsByState(string state)
        {
            var courts = await _courtService.GetCourtsByStateAsync(state);
            return Ok(courts);
        }

        [HttpPost]
        public async Task<ActionResult<Court>> CreateCourt([FromBody] Court court)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdCourt = await _courtService.CreateCourtAsync(court);
                return CreatedAtAction(nameof(GetCourt), new { id = createdCourt.CourtId }, createdCourt);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating court: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Court>> UpdateCourt(int id, [FromBody] Court court)
        {
            if (id != court.CourtId)
                return BadRequest("Court ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedCourt = await _courtService.UpdateCourtAsync(court);
                return Ok(updatedCourt);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating court: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCourt(int id)
        {
            var result = await _courtService.DeleteCourtAsync(id);
            if (!result)
                return NotFound($"Court with ID {id} not found.");

            return NoContent();
        }
    }
}