using System;
namespace LeaderboardApi.Models
{
	public class StarpointResponseModel
	{
        public string UserId { get; set; }
        public int TotalStarPoints { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}

