using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;

namespace Betabit.ADT.Simulator.Helpers
{
    public static class DigitalTwinClientHelper
    {
        public static DigitalTwinsClient Build()
        {
            var adtInstanceUrl = Environment.GetEnvironmentVariable("DigitalTwinUrl"); ;
            var credential = new DefaultAzureCredential();
            return new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);
        }
    }
}
