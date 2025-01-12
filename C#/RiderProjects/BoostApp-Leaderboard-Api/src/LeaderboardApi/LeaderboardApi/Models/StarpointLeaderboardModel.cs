namespace LeaderboardApi.Models
{
    public class StarpointLeaderboardModel
    {
        public Guid TeamMemberId { get; set; }
        public int Starpoints { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid StarpointId { get; set; }

    }
}