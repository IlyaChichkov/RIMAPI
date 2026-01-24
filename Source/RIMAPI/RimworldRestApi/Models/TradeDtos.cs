using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;

namespace RIMAPI.Models
{
    public class TraderKindDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }

        // Using short boolean flags (0/1 or omit if false) could optimize further, 
        // but standard bools are fine for readability.
        public bool Orbital { get; set; }
        public bool Visitor { get; set; }
        public float Commonality { get; set; }

        // -- BUCKETS --
        // We only initialize these if they have data to keep JSON clean
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<StockRuleDto> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<StockRuleDto> Categories { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<StockRuleDto> Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<StockRuleDto> Special { get; set; }
    }

    public class StockRuleDto
    {
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Count { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Price { get; set; }

        // FIX: Explicitly tell serializer that 'true' is the default
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool Buys { get; set; } = true;
    }
}