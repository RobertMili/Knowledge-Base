using StarPointApi.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StarPointApi.DTOs
{
    public class PostStarpointsDTO
    {
        [StringNotNullOrEmptyOrWhiteSpace(ErrorMessage = "UserId can not be null or empty")]
        public string UserId { get; set; }
        [StringNotNullOrEmptyOrWhiteSpace(ErrorMessage = "Source can not be null or empty")]
        public string Source { get; set; }
        [StringNotNullOrEmptyOrWhiteSpace(ErrorMessage = "SourceID can not be null or empty")]
        public string SourceID { get; set; }
        [Required(ErrorMessage = "Starpoints must have a value higher then 0")]
        [Range(1, 30000)]
        public int Starpoints { get; set; }
        
    }
}
