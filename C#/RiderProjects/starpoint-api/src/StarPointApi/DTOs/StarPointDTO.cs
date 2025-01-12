using System;
using Microsoft.Azure.Cosmos.Table;

namespace StarPointApi.DTOs
{
	public class StarPointDTO : TableEntity
    {
        public string ID { get; set; }
        public string UserID { get; set; }
        public int StarPoints { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Source { get; set; } // This is the previous "Activity"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public StarPointDTO()
        {
        }
    }
}

