using Microsoft.AspNetCore.Mvc;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<Deadline>>> GetUpcomingDeadlines([FromQuery] int days = 30)
        {
            var deadlines = await _deadlineService.GetUpcomingDeadlinesAsync(days);
            return Ok(deadlines);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<Deadline>>> GetOverdueDeadlines()
        {
            var deadlines = await _deadlineService.GetOverdueDeadlinesAsync();
            return Ok(deadlines);
        }

        [HttpGet("critical")]
        public async Task<ActionResult<IEnumerable<Deadline>>> GetCriticalDeadlines()
        {
            var deadlines = await _deadlineService.GetCriticalDeadlinesAsync();
            return Ok(deadlines);
        }

        [HttpGet("case/{caseId}")]
        public async Task<ActionResult<IEnumerable<Deadline>>> GetDeadlinesByCase(int caseId)
        {
            var deadlines = await _deadlineService.GetDeadlinesByCaseAsync(caseId);
            return Ok(deadlines);
        }

        [HttpPost]
        public async Task<ActionResult<Deadline>> CreateDeadline([FromBody] Deadline deadline)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdDeadline = await _deadlineService.CreateDeadlineAsync(deadline);
                return CreatedAtAction(nameof(GetDeadlinesByCase), new { caseId = createdDeadline.CaseId }, createdDeadline);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating deadline: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Deadline>> UpdateDeadline(int id, [FromBody] Deadline deadline)
        {
            if (id != deadline.DeadlineId)
                return BadRequest("Deadline ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedDeadline = await _deadlineService.UpdateDeadlineAsync(deadline);
                return Ok(updatedDeadline);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating deadline: {ex.Message}");
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult> MarkDeadlineComplete(int id)
        {
            var result = await _deadlineService.MarkDeadlineCompleteAsync(id);
            if (!result)
                return NotFound($"Deadline with ID {id} not found.");

            return Ok(new { message = "Deadline marked as complete." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDeadline(int id)
        {
            var result = await _deadlineService.DeleteDeadlineAsync(id);
            if (!result)
                return NotFound($"Deadline with ID {id} not found.");

            return NoContent();
        }
    }
}