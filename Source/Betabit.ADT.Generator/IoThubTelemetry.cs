// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Betabit.ADT.Simulator.Models;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Betabit.ADT.Simulator
{
    public static class IoThubTelemetry
    {
        [FunctionName("IoThubTelemetry")]
        public static async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            try
            {
                var adtInstanceUrl = Environment.GetEnvironmentVariable("DigitalTwinUrl");
                var credential = new DefaultAzureCredential();
                var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    JObject deviceMessage = JsonConvert.DeserializeObject<JObject>(eventGridEvent.Data.ToString());
                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    var temperature = deviceMessage["body"]["Temperature"];

                    log.LogInformation($"Device:{deviceId} Temperature is:{temperature}");

                    var message = new TelemetryMessage
                    {
                        Value = temperature.Value<double>()
                    };

                    await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(message));
                }
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, ex.Message);
            }
        }
    }
}
