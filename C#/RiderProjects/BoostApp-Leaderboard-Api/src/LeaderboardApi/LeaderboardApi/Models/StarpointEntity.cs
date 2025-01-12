using Azure;
using Azure.Data.Tables;

namespace LeaderboardApi.Models
{
    public class StarpointEntity : ITableEntity
    {
        //UserID
        public string PartitionKey { get ; set ; }
        //ID
        public string RowKey { get; set; }
        //CreatedDate
        public DateTime CreatedDate { get; set; }
        //Starpoint
        public int Starpoints { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public StarpointEntity()
        {
        }

        public StarpointEntity(Guid teamMemberId, Guid starpointId,  int starpoints,
            DateTime createdDate)
        {
            RowKey = starpointId.ToString();
            PartitionKey = teamMemberId.ToString();          
            Starpoints = starpoints;
            CreatedDate = createdDate;
        }

    }
}
