using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ExchangeRatesAPI
{
    class Format
    {
        [JsonPropertyName("rates")]
        public Dictionary<string,double> Rates { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}
