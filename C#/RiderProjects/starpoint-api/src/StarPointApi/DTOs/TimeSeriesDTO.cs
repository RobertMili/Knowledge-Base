using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StarPointApi.DTOs
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeFrame
    {
        [EnumMember(Value = "Yearly")] Yearly,
        [EnumMember(Value = "Monthly")] Monthly,
        [EnumMember(Value = "Weekly")] Weekly,
        [EnumMember(Value = "Daily")] Daily
    }

    public class TimeSeriesDTO
    {
        public string UserId { get; set; }
        public TimeFrame TimeFrame { get; set; }
        public IEnumerable<StarPoint> StarPoints { get; set; }
    }

    public class StarPoint
    {
        public DateTime Date { get; set; }
        public int StarPoints { get; set; }
    }
}