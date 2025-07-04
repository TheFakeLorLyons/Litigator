using Litigator.Models.DTOs.Judge;
using Litigator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JudgeController : ControllerBase
    {
        private readonly IJudgeService _judgeService;
        private readonly ILogger<JudgeController> _logger;

        public JudgeController(IJudgeService judgeService, ILogger<JudgeController> logger)
        {
            _judgeService = judgeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all judges
        /// </summary>
        /// <returns>List of judges</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JudgeDTO>>> GetAllJudges()
        {
            try
            {
                var judges = await _judgeService.GetAllJudgesAsync();
                return Ok(judges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all judges");
                return StatusCode(500, "An error occurred while retrieving judges");
            }
        }

        /// <summary>
        /// Get active judges only
        /// </summary>
        /// <returns>List of active judges</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<JudgeDTO>>> GetActiveJudges()
        {
            try
            {
                var judges = await _judgeService.GetActiveJudgesAsync();
                return Ok(judges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active judges");
                return StatusCode(500, "An error occurred while retrieving active judges");
            }
        }

        /// <summary>
        /// Get judge by ID
        /// </summary>
        /// <param name="id">Judge ID</param>
        /// <returns>Judge details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<JudgeDetailDTO>> GetJudgeById(int id)
        {
            try
            {
                var judge = await _judgeService.GetJudgeByIdAsync(id);
                if (judge == null)
                {
                    return NotFound($"Judge with ID {id} not found");
                }
                return Ok(judge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving judge with ID {JudgeId}", id);
                return StatusCode(500, "An error occurred while retrieving the judge");
            }
        }

        /// <summary>
        /// Get judge by bar number
        /// </summary>
        /// <param name="barNumber">Bar number</param>
        /// <returns>Judge details</returns>
        [HttpGet("bar/{barNumber}")]
        public async Task<ActionResult<JudgeDetailDTO>> GetJudgeByBarNumber(string barNumber)
        {
            try
            {
                var judge = await _judgeService.GetJudgeByBarNumberAsync(barNumber);
                if (judge == null)
                {
                    return NotFound($"Judge with bar number {barNumber} not found");
                }
                return Ok(judge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving judge with bar number {BarNumber}", barNumber);
                return StatusCode(500, "An error occurred while retrieving the judge");
            }
        }

        /// <summary>
        /// Create a new judge
        /// </summary>
        /// <param name="judgeDto">Judge data</param>
        /// <returns>Created judge</returns>
        [HttpPost]
        public async Task<ActionResult<JudgeDetailDTO>> CreateJudge([FromBody] JudgeDetailDTO judgeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdJudge = await _judgeService.CreateJudgeAsync(judgeDto);
                return CreatedAtAction(
                    nameof(GetJudgeById),
                    new { id = createdJudge.JudgeId },
                    createdJudge);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating judge");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating judge");
                return StatusCode(500, "An error occurred while creating the judge");
            }
        }

        /// <summary>
        /// Update an existing judge
        /// </summary>
        /// <param name="id">Judge ID</param>
        /// <param name="judgeDto">Updated judge data</param>
        /// <returns>Updated judge</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<JudgeDetailDTO>> UpdateJudge(int id, [FromBody] JudgeDetailDTO judgeDto)
        {
            if (id != judgeDto.JudgeId)
            {
                return BadRequest("Judge ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedJudge = await _judgeService.UpdateJudgeAsync(judgeDto);
                return Ok(updatedJudge);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating judge with ID {JudgeId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating judge with ID {JudgeId}", id);
                return StatusCode(500, "An error occurred while updating the judge");
            }
        }

        /// <summary>
        /// Delete a judge
        /// </summary>
        /// <param name="id">Judge ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJudge(int id)
        {
            try
            {
                var result = await _judgeService.DeleteJudgeAsync(id);
                if (!result)
                {
                    return NotFound($"Judge with ID {id} not found");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting judge with ID {JudgeId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting judge with ID {JudgeId}", id);
                return StatusCode(500, "An error occurred while deleting the judge");
            }
        }
    }
}