using System;
using StarPointApi.Repository.Models;

namespace StarPointApi.DTOs
{
    public class SourceDTO
    {
        public SourceDTO()
        {
        }

        public SourceDTO(StarPointDTO starPoint)
        {
            Source = starPoint.Source;
            StarPoints = starPoint.StarPoints;
            CreatedDate = starPoint.CreatedDate;
            DatabaseId = starPoint.ID;
        }

        public string DatabaseId { get; set; }
        public string Source { get; set; }
        public int StarPoints { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}