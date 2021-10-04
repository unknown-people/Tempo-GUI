using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TempoWithGUI
{
    class Tokens
    {
        [JsonProperty("token")]
        public string token { get; set; }
        [JsonProperty("email")]
        public string email { get; set; }
        [JsonProperty("password")]
        public string password { get; set; }
        [JsonProperty("creation")]
        public string creation { get; set; }
    }
    class DiscordToken
    {
        public bool _active;
        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        public string _token;
        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public string _type;
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public DiscordToken(bool active, string token, string type)
        {
            _active = active;
            _token = token;
            _type = type;
        }
        public DiscordToken()
        {

        }
        public override string ToString()
        {
            return _token.ToString();
        }
    }
}
