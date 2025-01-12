using System;
namespace LeaderboardApi.Models
{
	public class CompetitionResponseModel
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

