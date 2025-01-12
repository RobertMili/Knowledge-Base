using System.Collections.Generic;

namespace StarPointApi.DTOs
{
    public class UserHistoryDTO
    {
        public string UserID { get; set; }
        public IEnumerable<SourceDTO> Activities { get; set; }
    }
}