using Newtonsoft.Json;

namespace alltheairgeadApp.DataObjects
{
    public class Account
    {
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
