using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<DeadlineDTO>> GetDeadline(int id)
        {
            try
            {
                var deadline = await _deadlineService.GetDeadlineByIdAsync(id);
                if (deadline == null)
                    return NotFound();
                return Ok(deadline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deadline {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the deadline");
            }
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetUpcomingDeadlines([FromQuery] int days = 30)
        {
            try
            {
                var deadlines = await _deadlineService.GetUpcomingDeadlinesAsync(days);
                return Ok(deadlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming deadlines");
                return StatusCode(500, "An error occurred while retrieving upcoming deadlines");
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetOverdueDeadlines()
        {
            try
            {
                var deadlines = await _deadlineService.GetOverdueDeadlinesAsync();
                return Ok(deadlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue deadlines");
                return StatusCode(500, "An error occurred while retrieving overdue deadlines");
            }
        }

        [HttpGet("critical")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetCriticalDeadlines()
        {
            try
            {
                var deadlines = await _deadlineService.GetCriticalDeadlinesAsync();
                return Ok(deadlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting critical deadlines");
                return StatusCode(500, "An error occurred while retrieving critical deadlines");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetAllDeadlines()
        {
            try
            {
                var deadlines = await _deadlineService.GetAllDeadlinesAsync();
                return Ok(deadlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all deadlines");
                return StatusCode(500, "An error occurred while retrieving deadlines");
            }
        }

        [HttpGet("case/{caseId}")]
        public async Task<ActionResult<IEnumerable<DeadlineDTO>>> GetDeadlinesByCase(int caseId)
        {
            if (caseId <= 0)
                return BadRequest("Invalid case ID.");

            try
            {
                var deadlines = await _deadlineService.GetDeadlinesByCaseAsync(caseId);
                return Ok(deadlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deadlines for case {CaseId}", caseId);
                return StatusCode(500, "An error occurred while retrieving deadlines for the case");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DeadlineDTO>> CreateDeadline(DeadlineCreateDTO createDto)
        {
            if (createDto == null)
                return BadRequest("Deadline data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var deadline = await _deadlineService.CreateDeadlineAsync(createDto);
                return CreatedAtAction(nameof(GetDeadline), new { id = deadline.DeadlineId }, deadline);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating deadline");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deadline");
                return StatusCode(500, "An error occurred while creating the deadline");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeadlineDTO>> UpdateDeadline(int id, DeadlineUpdateDTO updateDto)
        {
            if (updateDto == null)
                return BadRequest("Deadline data is required.");

            if (updateDto.DeadlineId.HasValue && updateDto.DeadlineId.Value != id)
                return BadRequest("ID mismatch between URL and request body.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var deadline = await _deadlineService.UpdateDeadlineAsync(id, updateDto);
                if (deadline == null)
                    return NotFound();
                return Ok(deadline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating deadline {Id}", id);
                return StatusCode(500, "An error occurred while updating the deadline");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDeadline(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid deadline ID.");

            try
            {
                var result = await _deadlineService.DeleteDeadlineAsync(id);
                if (!result)
                    return NotFound($"Deadline with ID {id} not found.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting deadline {Id}", id);
                return StatusCode(500, "An error occurred while deleting the deadline");
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult> MarkDeadlineComplete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid deadline ID.");

            try
            {
                var result = await _deadlineService.MarkDeadlineCompleteAsync(id);
                if (!result)
                    return NotFound();
                return Ok("Deadline marked as complete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking deadline {Id} as complete", id);
                return StatusCode(500, "An error occurred while marking the deadline as complete");
            }
        }
    }
}