using Azure.DigitalTwins.Core;
using System.Text.Json.Serialization;

namespace Betabit.ADT.TwinCreation
{
    public class Floor
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string Id { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; } = new DigitalTwinMetadata();

        [JsonPropertyName("surfaceArea")]
        public int SurfaceArea { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
