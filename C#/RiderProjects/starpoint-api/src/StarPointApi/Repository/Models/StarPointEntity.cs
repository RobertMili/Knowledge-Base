using System;
using Microsoft.Azure.Cosmos.Table;

namespace StarPointApi.Repository.Models
{
    public class StarPointEntity : TableEntity
    {
        public StarPointEntity()
        {
        }

        public StarPointEntity(string userID, string source, DateTime createdDate, int starPoints, string sourceID, string databaseID = null)
        {
            RowKey = databaseID ?? Guid.NewGuid().ToString();
            PartitionKey = userID;
            StarPoints = starPoints;
            CreatedDate = createdDate;
            Source = source;
            SourceID = sourceID;
        }

        public string ID => RowKey;
        public string UserID => PartitionKey;
        public int StarPoints { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Source { get; set; } // This is the previous "Activity"
        public string SourceID { get; set; } // Id is added for easier handling of source

    }
}