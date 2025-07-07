using Microsoft.AspNetCore.Mvc;
using Litigator.Services.Interfaces;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.ClassDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litigator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("attorney-performance")]
        public async Task<ActionResult> GetAttorneyPerformance()
        {
            var result = await _analyticsService.GetAttorneyPerformanceAsync();
            return Ok(result);
        }

        [HttpGet("case-predictions")]
        public async Task<ActionResult> GetCaseOutcomePredictions()
        {
            var result = await _analyticsService.GetCaseOutcomePredictionsAsync();
            return Ok(result);
        }

        [HttpGet("critical-cases")]
        public async Task<ActionResult> GetCriticalCases()
        {
            var result = await _analyticsService.GetCriticalCasesAsync();
            return Ok(result);
        }

        [HttpGet("monthly-trends")]
        public async Task<ActionResult> GetMonthlyTrends()
        {
            var result = await _analyticsService.GetMonthlyTrendsAsync();
            return Ok(result);
        }

        [HttpGet("deadline-performance")]
        public async Task<ActionResult> GetDeadlinePerformance()
        {
            var result = await _analyticsService.GetDeadlinePerformanceAsync();
            return Ok(result);
        }
    }
}