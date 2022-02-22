using Azure.DigitalTwins.Core;
using Azure.Identity;
using Betabit.ADT.Simulator.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Betabit.ADT.Simulator
{
    public static class GeneratorTimerFunction
    {
        private static readonly Random random = new Random();

        [FunctionName(nameof(GeneratorTimerFunction))]
        public static async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            var adtInstanceUrl = Environment.GetEnvironmentVariable("DigitalTwinUrl"); ;
            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            //await UpdateSensorsForMetric(client, "temperature", 18.0, 24.0);
            await UpdateSensorsForMetric(client, "humidity", 18.0, 24.0);
            await UpdateSensorsForMetric(client, "illuminance", 0, 250);
        }

        private static async Task UpdateSensorsForMetric(DigitalTwinsClient client, string metricName, double minValue, double maxValue)
        {
            var temperatureSensors = new List<BasicDigitalTwin>();
            var response = client.QueryAsync<BasicDigitalTwin>($"SELECT * FROM digitaltwins WHERE IS_OF_MODEL('dtmi:ramon:sensor:{metricName};1')");
            await foreach (var twin in response)
            {
                temperatureSensors.Add(twin);
                var randomValue = RandomNumberBetween(minValue, maxValue);
                var message = new TelemetryMessage
                {
                    Value = randomValue
                };

                await client.PublishTelemetryAsync(twin.Id, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(message));
            }
        }

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = random.NextDouble();
            return minValue + (next * (maxValue - minValue));
        }
    }
}
