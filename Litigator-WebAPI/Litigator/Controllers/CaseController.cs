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
    public class CaseController : ControllerBase
    {
        private readonly ICaseService _caseService;

        public CaseController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CaseDetailDTO>>> GetAllCases()
        {
            var cases = await _caseService.GetAllCasesAsync();
            return Ok(cases);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CaseDetailDTO>> GetCase(int id)
        {
            var case_ = await _caseService.GetCaseByIdAsync(id);
            if (case_ == null)
                return NotFound($"Case with ID {id} not found.");
            return Ok(case_);
        }

        [HttpGet("number/{caseNumber}")]
        public async Task<ActionResult<CaseDetailDTO>> GetCaseByNumber(string caseNumber)
        {
            var case_ = await _caseService.GetCaseByNumberAsync(caseNumber);
            if (case_ == null)
                return NotFound($"Case with number {caseNumber} not found.");
            return Ok(case_);
        }

        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<CaseDetailDTO>>> GetCasesByClient(int clientId)
        {
            var cases = await _caseService.GetCasesByClientAsync(clientId);
            return Ok(cases);
        }

        [HttpGet("attorney/{attorneyId}")]
        public async Task<ActionResult<IEnumerable<CaseDetailDTO>>> GetCasesByAttorney(int attorneyId)
        {
            var cases = await _caseService.GetCasesByAttorneyAsync(attorneyId);
            return Ok(cases);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CaseDetailDTO>>> GetActiveCases()
        {
            var cases = await _caseService.GetActiveCasesAsync();
            return Ok(cases);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CaseDetailDTO>>> SearchCases([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required.");
            var cases = await _caseService.SearchCasesAsync(searchTerm);
            return Ok(cases);
        }

        [HttpPost]
        public async Task<ActionResult<CaseDetailDTO>> CreateCase([FromBody] CaseCreateDTO caseCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var createdCase = await _caseService.CreateCaseAsync(caseCreateDto);
                return CreatedAtAction(nameof(GetCase), new { id = createdCase.CaseId }, createdCase);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating case: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CaseDetailDTO>> UpdateCase(int id, [FromBody] CaseUpdateDTO caseUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var updatedCase = await _caseService.UpdateCaseAsync(id, caseUpdateDto);
                return Ok(updatedCase);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating case: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCase(int id)
        {
            var result = await _caseService.DeleteCaseAsync(id);
            if (!result)
                return NotFound($"Case with ID {id} not found.");
            return NoContent();
        }
    }
}