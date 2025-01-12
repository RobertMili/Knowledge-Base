using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StarPointApi.DTOs;
using StarPointApi.Services.StarPointService;

namespace StarPointApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class StarPointBackEndController : ControllerBase
    {
        private readonly IStarPointService _starPointService;

        public StarPointBackEndController(IStarPointService starPointService)
        {
            _starPointService = starPointService;
        }

        [HttpPut]
        public async Task<ActionResult<AddUserResponseDTO>> Put([BindRequired] PutStarPointsDTO putStarPoints)
        {
            if (ModelState.IsValid)
            {
                return await _starPointService.PostOrEditUserActivity(putStarPoints);
            }

            return BadRequest("ModelBinding is not valid");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id, [BindRequired] string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId)
                && !string.IsNullOrWhiteSpace(id)
                && await _starPointService.DeleteUserActivity(id, userId))
                return new OkObjectResult("Activity Deleted");
            return BadRequest("Activity could not be found");
        }

        [HttpDelete("deleteIds")] // With this endpoint you can delete several starpointEntities from the same user
        public async Task<ActionResult> DeleteMultiple([FromQuery] string userId, [FromQuery] string[] databaseIds)
        {
            if (string.IsNullOrWhiteSpace(userId) || databaseIds is null || databaseIds.Length == 0)
                return BadRequest();

            var requests = databaseIds.Select(id => Delete(id, userId));
            var actionResults = await Task.WhenAll(requests);

            int success = actionResults.Count(result => result.GetType() == typeof(OkObjectResult));

            return new OkObjectResult($"Activities Deleted. {success}/{actionResults.Length}");
        }

        [HttpPost("starpoints")]
        public async Task<ActionResult> PostStarPoints([BindRequired] PostStarpointsDTO starpointsDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("ModelBinding is not valid");
            }

            var response = await _starPointService.PostStarpoints(starpointsDTO);
            if (string.IsNullOrEmpty(response.DatabaseID))
            {
                return BadRequest("Saved failed");
            }

            return new OkObjectResult($"Starpoints has been saved with ID: {response.DatabaseID}");
        }
    }
}