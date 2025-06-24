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
            var deadline = await _deadlineService.GetDeadlineByIdAsync(id);
            if (deadline == null)
                return NotFound();

            return Ok(deadline);
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetUpcomingDeadlines([FromQuery] int days = 30)
        {
            var deadlines = await _deadlineService.GetUpcomingDeadlinesAsync(days);
            return Ok(deadlines);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetOverdueDeadlines()
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetAllDeadlines()
        {
            var deadlines = await _deadlineService.GetAllDeadlinesAsync();
            return Ok(deadlines);
        }

        [HttpGet("case/{caseId}")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetDeadlinesByCase(int caseId)
        {
            var deadlines = await _deadlineService.GetDeadlinesByCaseAsync(caseId);
            return Ok(deadlines);
        }

        [HttpPost]
        public async Task<ActionResult<DeadlineDTO>> CreateDeadline(DeadlineCreateDTO createDto)
        {
            var deadline = await _deadlineService.CreateDeadlineAsync(createDto);
            return CreatedAtAction(nameof(GetDeadline), new { id = deadline.DeadlineId }, deadline);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeadlineDTO>> UpdateDeadline(int id, DeadlineUpdateDTO updateDto)
        {
            var deadline = await _deadlineService.UpdateDeadlineAsync(id, updateDto);
            if (deadline == null)
                return NotFound();
            return Ok(deadline);
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