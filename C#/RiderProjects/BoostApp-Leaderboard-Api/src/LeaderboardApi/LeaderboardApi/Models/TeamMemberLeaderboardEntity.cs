using Azure;
using Azure.Data.Tables;

namespace LeaderboardApi.Models
{
    public class TeamMemberLeaderboardEntity : ITableEntity
    {
        // Lägg till dessa värden i en DTO som vi använder till frontenden istället
        //public Guid TeamMemberId { get => _rowKey; set => _rowKey = value; }
        //public Guid TeamId { get => _partitionKey; set => _partitionKey = value; }

        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        public string Name { get; set; }
        public int Starpoints { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public TeamMemberLeaderboardEntity()
        {
        }

        public TeamMemberLeaderboardEntity(string teamMemberId, string teamId, string name, int starpoints,
            DateTime createdDate)
        {
            RowKey = teamMemberId;
            PartitionKey = teamId;
            Name = name;
            Starpoints = starpoints;
            CreatedDate = createdDate;
        }
    }
}