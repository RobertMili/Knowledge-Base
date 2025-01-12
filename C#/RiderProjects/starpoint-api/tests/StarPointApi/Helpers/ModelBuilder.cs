using System;
using System.Collections.Generic;
using System.Text;
using StarPointApi.DTOs;

namespace StarPointApiTests.Helpers
{
    public static class ModelBuilder
    {
        public static PutStarPointsDTO GetPutStarpointsDto(Action<PutStarPointsDTO> options)
        {
            var dto = new PutStarPointsDTO
            {
                CreatedDate = DateTime.Now,
                Source = "Activity",
                StarPoints = 1,
                UserId = "UserId",
                DatabaseId = "DbId",
                SourceID = Guid.NewGuid().ToString(),
            };

            options.Invoke(dto);
            return dto;
        }
    }
}