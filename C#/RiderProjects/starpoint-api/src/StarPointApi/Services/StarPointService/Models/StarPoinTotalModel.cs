using System;

namespace StarPointApi.Services.StarPointService.Models
{
    public class StarPoinTotalModel
    {
        public string ID { get; set; }
        public string UserID { get; set; }
        public int TotalStarPoints { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}