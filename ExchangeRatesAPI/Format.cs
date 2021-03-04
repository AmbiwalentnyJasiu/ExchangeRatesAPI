using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExchangeRatesAPI
{
    class Format
    {
        [JsonPropertyName("rates")]
        public SortedDictionary<string,double> Rates { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}
