using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord
{
    public class GuildVerificationFormField
    {
        [JsonProperty("field_type")]
        public string FieldType { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; } = null;
        [JsonProperty("automations")]
        public string Automations { get; set; } = null;

        [JsonProperty("required")]
        public bool Required { get; set; } = true;

        [JsonProperty("values")]
        public IReadOnlyList<string> Values { get; set; }

        [JsonProperty("response")]
        public object Response { get; set; }
    }
}
