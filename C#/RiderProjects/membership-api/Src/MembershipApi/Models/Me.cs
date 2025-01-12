using MembershipAPI.Services.UserInfo.Models;

namespace MembershipAPI.Models
{
    // For REST:
    public class Me
    {
        public int? TeamId { get; set; }

        public string UserId { get; set; }

        // Usually a combo of given name, middle initial, and surname
        public string DisplayName { get; set; }

        // aka FirstName
        public string GivenName { get; set; }

        // aka LastName
        public string SurName { get; set; }

        public int StarPoints { get; set; }

        public int Ranking { get; set; }
        public bool IsAdmin { get; set; }
    }
}
