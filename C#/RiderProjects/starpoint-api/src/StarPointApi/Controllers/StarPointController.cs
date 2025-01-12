using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StarPointApi.DTOs;
using StarPointApi.Services.StarPointService;
using StarPointApi.Shared;
using BoostApp.Shared.Security;

namespace StarPointApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class StarPointController : ControllerBase
    {
        private readonly IStarPointService _starPointService;

        public StarPointController(IStarPointService starPointService)
        {
            _starPointService = starPointService;
        }

        [HttpGet("Me/TimeSeries/{startDate}")]
        public async Task<ActionResult<TimeSeriesDTO>> GetTimeLine(DateTime startDate, [BindRequired] DateTime endDate,
            [BindRequired] TimeFrame timeFrame, string source)
            => await GetTimeLine(User.GetUniqueUserId(), startDate, endDate, timeFrame, source);

        [HttpGet("User/TimeSeries/{startDate}")]
        public async Task<ActionResult<TimeSeriesDTO>> GetTimeLine([BindRequired]string userId, [BindRequired] DateTime startDate, [BindRequired]DateTime endDate, [BindRequired] TimeFrame timeFrame, string source)
        {
            if (endDate.IsBefore(startDate))
                return BadRequest($"{nameof(endDate)} must be greater than {nameof(startDate)}");
                
            return await _starPointService.GetTimeSeriesByUserIdAsync(userId, startDate, endDate, timeFrame, source);
        }


        [HttpGet("Me/Total/{startDate}")]
        public async Task<ActionResult<UserTotalDTO>> GetMyStarPoints(DateTime startDate, DateTime? endDate, string source)
        {
            return await GetTotalStarPointsByUser(startDate, endDate, User.GetUniqueUserId(), source);
        }

        [HttpGet("Me/History/{startDate}")]
        public async Task<ActionResult<UserHistoryDTO>> GetMyHistory(DateTime startDate, DateTime? endDate, string source)
        {
            return await GetUserHistory(startDate, endDate, User.GetUniqueUserId(), source);
        }

        [HttpGet("User/Total/{startDate}")]
        public async Task<ActionResult<UserTotalDTO>> GetTotalStarPointsByUser(DateTime startDate, DateTime? endDate,
            [BindRequired] string userID, string source)
        {
            var _endDate = endDate ?? DateTime.Now.AddDays(1);

            if (_endDate.IsBefore(startDate))
                return BadRequest($"{nameof(endDate)} must be greater than {nameof(startDate)}");

            var result = await _starPointService.GetTotalStarPointsByUserIdAsync(userID, startDate, _endDate, source);

            return result == null
                   || string.IsNullOrEmpty(result.UserID)
                ? NotFound("No user found")
                : new OkObjectResult(result);
        }

        [HttpGet("User/History/{startDate}")]
        public async Task<ActionResult<UserHistoryDTO>> GetUserHistory(DateTime startDate, DateTime? endDate,
            [BindRequired] string userID, string source)
        {
            var _endDate = endDate ?? DateTime.Now.AddDays(1);

            if (_endDate.IsBefore(startDate))
                return BadRequest($"{nameof(endDate)} must be greater than {nameof(startDate)}");

            var result = await _starPointService.GetStarPointHistoryByUserIdAsync(userID, startDate, _endDate, source);
            return result.UserID != null
                ? new OkObjectResult(result)
                : NotFound("No user found");
        }
    }
}