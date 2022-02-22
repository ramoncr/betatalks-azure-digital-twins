using System.Text.Json.Serialization;

namespace Betabit.ADT.TwinCreation
{
    public class Address
    {
        [JsonPropertyName("country")]
        public string Country { get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; }
        [JsonPropertyName("state")]
        public string State { get; set; }
        [JsonPropertyName("address")]
        public string AddressString { get; set; }
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }
    }
}
