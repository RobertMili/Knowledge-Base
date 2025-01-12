namespace LeaderboardApi.Models
{
    public class GetTeamMemberLeaderboardDTO
    {
        public string TeamMemberId { get; set; }
        public string TeamId { get; set; }
        public string Name { get; set; }
        public int Starpoints { get; set; }
    }
}
