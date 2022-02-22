using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Betabit.ADT.TwinCreation
{
    internal class Worker
    {
        private readonly IConfiguration configuration;

        public Worker(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task CreateDemoBuildingInTwin()
        {
            var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
            {
                TenantId = this.configuration.GetValue<string>("Tenant")
            });
            var client = new DigitalTwinsClient(new UriBuilder("https", this.configuration.GetValue<string>("HostName")).Uri, credential);

            var suffix = 1;

            var buildingId = await TopologyActions.AddBuilding(client, suffix);

            for (int i = 0; i < 6; i++)
            {
                var floorId = await TopologyActions.AddFloor(client, suffix, i, buildingId);

                for (int y = 0; y < 10; y++)
                {
                    var spaceId = await TopologyActions.AddSpace(client, suffix, i, floorId);
                }
            }

        }
    }
}
