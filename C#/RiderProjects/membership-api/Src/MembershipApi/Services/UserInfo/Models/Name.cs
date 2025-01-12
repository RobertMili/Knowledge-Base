using Newtonsoft.Json;

namespace MembershipAPI.Services.UserInfo.Models
{
    public readonly struct Name
    {
        public Name(string givenName, string surName, string displayName = null)
        {
            Display = displayName;
            Given = givenName;
            Sur = surName;
        }

        // Usually a combo of givenname, middle initial, and surname
        [JsonProperty(PropertyName = "DisplayName")]
        public string Display { get; }

        // aka FirstName
        [JsonProperty(PropertyName = "GivenName")]
        public string Given { get; }

        // aka LastName
        [JsonProperty(PropertyName = "SurName")]
        public string Sur { get; }

        [JsonIgnore]
        public string DisplayAuto
            => string.IsNullOrEmpty(Display)
            ? (!string.IsNullOrEmpty(Given) ? Given + " " + Sur : Sur)?? string.Empty
            : Display;

        public override string ToString() => DisplayAuto;
    }
}
