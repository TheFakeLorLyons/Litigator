using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Deadline;
using Litigator.Services.Interfaces;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeadlineController : ControllerBase
    {
        private readonly IDeadlineService _deadlineService;
        private readonly ILogger<DeadlineController> _logger;

        public DeadlineController(IDeadlineService deadlineService, ILogger<DeadlineController> logger)
        {
            _deadlineService = deadlineService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeadlineDTO>> GetDeadline(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid deadline ID.");

            var deadline = await _deadlineService.GetDeadlineByIdAsync(id);
            if (deadline == null)
                return NotFound();
            return Ok(deadline);
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetUpcomingDeadlines([FromQuery] int days = 30)
        {
            try
            {
                if (days < 0)
                    return BadRequest("Days parameter cannot be negative.");

                var deadlines = await _deadlineService.GetUpcomingDeadlinesAsync(days);
                return Ok(deadlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming deadlines");
                return StatusCode(500, "An error occurred while retrieving upcoming deadlines.");
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetOverdueDeadlines()
        {
            var deadlines = await _deadlineService.GetOverdueDeadlinesAsync();
            return Ok(deadlines);
        }

        [HttpGet("critical")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetCriticalDeadlines()
        {
            var deadlines = await _deadlineService.GetCriticalDeadlinesAsync();
            return Ok(deadlines);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetAllDeadlines()
        {
            var deadlines = await _deadlineService.GetAllDeadlinesAsync();
            return Ok(deadlines);
        }

        [HttpGet("case/{caseId}")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetDeadlinesByCase(int caseId)
        {
            if (caseId <= 0)
                return BadRequest("Invalid case ID.");

            var deadlines = await _deadlineService.GetDeadlinesByCaseAsync(caseId);
            return Ok(deadlines);
        }

        [HttpPost]
        public async Task<ActionResult<DeadlineDTO>> CreateDeadline(DeadlineCreateDTO createDto)
        {
            if (createDto == null)
                return BadRequest("Deadline data is required.");

            if (createDto.CaseId <= 0)
                return BadRequest("Valid case ID is required.");

            if (string.IsNullOrWhiteSpace(createDto.DeadlineType))
                return BadRequest("Deadline type is required.");

            if (createDto.DeadlineDate == default(DateTime))
                return BadRequest("Valid deadline date is required.");

            try
            {
                var deadline = await _deadlineService.CreateDeadlineAsync(createDto);
                return CreatedAtAction(nameof(GetDeadline), new { id = deadline.DeadlineId }, deadline);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeadlineDTO>> UpdateDeadline(int id, DeadlineUpdateDTO updateDto)
        {
            if (id <= 0)
                return BadRequest("Invalid deadline ID.");

            // If DeadlineUpdateDTO has an ID property, check if it matches
            if (updateDto.DeadlineId.HasValue && updateDto.DeadlineId != id)
                return BadRequest("ID in URL does not match ID in request body.");

            var deadline = await _deadlineService.UpdateDeadlineAsync(id, updateDto);
            if (deadline == null)
                return NotFound();
            return Ok(deadline);
        }


        [HttpPut("{id}/complete")]
        public async Task<ActionResult> MarkDeadlineComplete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid deadline ID.");

            var result = await _deadlineService.MarkDeadlineCompleteAsync(id);
            if (!result)
                return NotFound($"Deadline with ID {id} not found.");

            return Ok(new { message = "Deadline marked as complete." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDeadline(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid deadline ID.");

            var result = await _deadlineService.DeleteDeadlineAsync(id);
            if (!result)
                return NotFound($"Deadline with ID {id} not found.");

            return NoContent();
        }

    }
}