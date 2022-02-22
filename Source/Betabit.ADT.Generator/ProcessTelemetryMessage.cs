using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Betabit.ADT.Simulator.Models;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betabit.ADT.Simulator
{
    public static class ProcessTelemetryMessage
    {
        [FunctionName("ProcessTelemetryMessage")]
        public static async Task Run([EventHubTrigger("telemetry", Connection = "TelemetryEventhubConnectionString")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    var tm = JsonConvert.DeserializeObject<TelemetryMessage>(messageBody);

                    var adtInstanceUrl = Environment.GetEnvironmentVariable("DigitalTwinUrl");
                    var credential = new DefaultAzureCredential();
                    var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

                    // The properties will contain a full url to the twin, including the digital twin host.
                    // Therefor we have to split it and only take the last segment.
                    var source = eventData.Properties["cloudEvents:source"].ToString();
                    var twinId = source.Split('/').LastOrDefault();

                    if (string.IsNullOrEmpty(twinId)) continue;

                    var parentId = await GetParentId(client, twinId);

                    if (string.IsNullOrEmpty(parentId)) continue;

                    if (eventData.Properties.TryGetValue("cloudEvents:dataschema", out var rawSchema))
                    {
                        var twinSchema = rawSchema.ToString();

                        var value = Math.Round(tm.Value, 2);

                        // Updating the twin uses json patch. This is a way of patching instructions on a object.
                        // In our case we want to make sure the latest value is set, this would translate to:
                        // [
                        //  { "op": "add", "path": "/latestValue", "value": <XXX> }
                        // ]
                        var sensorPatchDocument = new JsonPatchDocument();
                        sensorPatchDocument.AppendAdd("/latestValue", value);
                        await client.UpdateDigitalTwinAsync(twinId, sensorPatchDocument);

                        var parentPatchDocument = GetParentPatch(twinSchema, value);
                        await client.UpdateDigitalTwinAsync(parentId, parentPatchDocument);
                    }
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.
            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private static JsonPatchDocument GetParentPatch(string twinSchema, double value)
        {
            var parentPatchDocument = new JsonPatchDocument();

            // A twin schema is a full twin interface id like: 'dtmi:ramon:sensor:temperature;1'
            if (twinSchema.Contains("temperature"))
                parentPatchDocument.AppendAdd("/temperature", value);


            if (twinSchema.Contains("humidity"))
                parentPatchDocument.AppendAdd("/humidity", value);


            if (twinSchema.Contains("illuminance"))
                parentPatchDocument.AppendAdd("/illuminance", value);

            return parentPatchDocument;
        }

        private static async Task<string> GetParentId(DigitalTwinsClient client, string childTwinId)
        {
            var foundRelationships = new List<BasicRelationship>();
            var response = client.QueryAsync<BasicRelationship>($"SELECT * FROM relationships WHERE $targetId = '{childTwinId}' AND $relationshipName = 'measuredBy'");
            await foreach (var result in response)
            {
                foundRelationships.Add(result);
            }

            return foundRelationships.FirstOrDefault()?.SourceId;
        }
    }
}
