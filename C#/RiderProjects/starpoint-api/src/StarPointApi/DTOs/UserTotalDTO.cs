using System;

namespace StarPointApi.DTOs
{
    public class UserTotalDTO
    {
        public string UserID { get; set; }
        public int TotalStarPoints { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}