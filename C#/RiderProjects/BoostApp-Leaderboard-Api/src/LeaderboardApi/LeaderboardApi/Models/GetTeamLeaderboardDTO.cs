using Microsoft.AspNetCore.Mvc;

using System;

namespace LeaderboardApi.DTOs
{
    public class TeamLeaderboardDto
    {
        public Guid TeamId { get; set; }
        public int CompetitionId { get; set; }
        public int TotalStarpoints { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
}
