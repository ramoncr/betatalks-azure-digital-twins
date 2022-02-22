using System;

namespace Betabit.ADT.Simulator.Models
{
    public class TelemetryMessage
    {
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public double Value { get; set; }
    }
}
