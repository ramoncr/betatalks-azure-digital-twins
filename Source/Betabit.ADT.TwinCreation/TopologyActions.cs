using Azure.DigitalTwins.Core;

namespace Betabit.ADT.TwinCreation
{
    public static class TopologyActions
    {
        internal static async Task<string> AddBuilding(DigitalTwinsClient client, int suffix)
        {
            var twin = new BasicDigitalTwin
            {
                Id = $"DEMO-{suffix}"
            };
            twin.Metadata.ModelId = "dtmi:ramon:building;1";
            twin.Contents.Add("name", $"Demo Building {suffix}");
            twin.Contents.Add("address", new Address
            {
                Country = "Netherlands",
                City = "Amsterdam",
                State = "Noord-Holland",
                AddressString = "Rainbowroad 125",
                PostalCode = "1234AB"
            });

            twin.Contents.Add("type", "office");
            twin.Contents.Add("surfaceArea", 100000);

            var geolocation = new BasicDigitalTwinComponent();
            geolocation.Contents.Add("alt", 100);
            geolocation.Contents.Add("lat", 90);
            geolocation.Contents.Add("long", 0);

            twin.Contents.Add("geolocation", geolocation);

            await client.CreateOrReplaceDigitalTwinAsync(twin.Id, twin);

            return twin.Id;
        }

        internal static async Task<string> AddFloor(DigitalTwinsClient client, int suffix, int floorNumber, string buildingId)
        {
            var twinId = $"DEMO-{suffix}-F{floorNumber}";
            var floor = new Floor
            {
                Metadata = { ModelId = "dtmi:ramon:floor;1" },
                Id = $"DEMO-{suffix}-F{floorNumber}",
                Name = $"Floor {suffix}",
                SurfaceArea = 2540
            };

            await client.CreateOrReplaceDigitalTwinAsync(twinId, floor);

            var relationsipToBuilding = new BasicRelationship
            {
                Id = $"{buildingId}_{twinId}_consistsOf",
                Name = "consistsOf",
                SourceId = buildingId,
                TargetId = twinId
            };

            await client.CreateOrReplaceRelationshipAsync(buildingId, relationsipToBuilding.Id, relationsipToBuilding);

            return twinId;
        }

        internal static async Task<string> AddSpace(DigitalTwinsClient client, int suffix, int spaceNumber, string floorId)
        {
            var twin = new BasicDigitalTwin
            {
                Id = $"DEMO-{suffix}-F{floorId}-S{spaceNumber}"
            };
            twin.Metadata.ModelId = "dtmi:ramon:space;1";
            twin.Contents.Add("name", $"Room {spaceNumber}");
            twin.Contents.Add("type", $"Room");
            twin.Contents.Add("subType", $"Meeting Room");
            twin.Contents.Add("surfaceArea", 14);

            if (spaceNumber == 8)
            {
                twin.Contents.Add("temperature", 35);
                twin.Contents.Add("humidity", 62);
            }

            await client.CreateOrReplaceDigitalTwinAsync(twin.Id, twin);


            var relationsipToBuilding = new BasicRelationship
            {
                Id = $"{floorId}_{twin.Id}_contains",
                Name = "contains",
                SourceId = floorId,
                TargetId = twin.Id
            };

            await client.CreateOrReplaceRelationshipAsync(floorId, relationsipToBuilding.Id, relationsipToBuilding);

            return twin.Id;
        }
    }
}
