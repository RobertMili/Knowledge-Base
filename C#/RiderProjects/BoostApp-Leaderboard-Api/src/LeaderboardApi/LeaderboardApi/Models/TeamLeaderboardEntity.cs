using Azure;
using Azure.Data.Tables;

namespace LeaderboardApi.Models
{
    public class TeamLeaderboardEntity : ITableEntity
    {
        //Gör dto med competitionid och teamid

        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public TeamLeaderboardEntity()
        {
        }


        public TeamLeaderboardEntity(Guid teamId, int competitionId, string name, string imageUrl, DateTime createdDate,
            DateTime endDate)
        {
            RowKey = teamId.ToString();
            PartitionKey = competitionId.ToString();
            Name = name;
            ImageUrl = imageUrl;
            CreatedDate = createdDate;
            EndDate = endDate;
        }
    }
}