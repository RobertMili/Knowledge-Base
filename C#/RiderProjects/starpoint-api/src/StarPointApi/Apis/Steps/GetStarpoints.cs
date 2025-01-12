using System;
using System.Collections.Generic;
using Collector.Common.RestContracts;
using Newtonsoft.Json;

namespace StarPointApi.Apis.Steps
{
    public static class StepApi
    {
        public class GetStarPointsRequest
            : BaseRequest<GetStarPointsResourceIdentifier, IEnumerable<GetStarPointsResponse>>
        {
            public GetStarPointsRequest(DateTime startTime, DateTime endTime)
                : base(new GetStarPointsResourceIdentifier())
            {
                StartTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss");
                EndTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            [JsonProperty("startTime")] public string StartTime { get; set; }

            [JsonProperty("endTime")] public string EndTime { get; set; }

            public override HttpMethod GetHttpMethod() => HttpMethod.POST;
        }

        public class GetStarPointsResourceIdentifier : ResourceIdentifier
        {
            public override string Uri => "starpoints/";
        }

        public class GetStarPointsResponse
        {
            [JsonProperty("userId")] public Guid UserId { get; set; }

            [JsonProperty("starPoints")] public Info StarPoint { get; set; }
        }

        public class Info
        {
            [JsonProperty("source")] public string Source { get; set; }

            [JsonProperty("starPoints")] public int StarPoints { get; set; }

            [JsonProperty("createdDate")] public DateTime CreatedDate { get; set; }
        }
    }
}