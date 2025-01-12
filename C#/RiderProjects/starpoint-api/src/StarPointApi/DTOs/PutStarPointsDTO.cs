using System;
using System.ComponentModel.DataAnnotations;
using StarPointApi.Shared;

namespace StarPointApi.DTOs
{
    public class PutStarPointsDTO
    {
        [StringNotNullOrEmptyOrWhiteSpace(ErrorMessage = "UserId can not be null or empty")]
        public string UserId { get; set; }

        [StringNotEmptyOrWhiteSpace(ErrorMessage = "Database Id cannot be empty or all whitespace")]
        public string DatabaseId { get; set; }

        [StringNotNullOrEmptyOrWhiteSpace(ErrorMessage = "Source can not be null or empty")]
        public string Source { get; set; }

        [StringNotNullOrEmptyOrWhiteSpace(ErrorMessage = "Source can not be null or empty")]
        public string SourceID { get; set; }

        [Required(ErrorMessage = "Can not be empty")]
        [Range(1, int.MaxValue)]
        public int StarPoints { get; set; }

        [Required(ErrorMessage = "Can not be empty")]
        public DateTime CreatedDate { get; set; }
    }
}