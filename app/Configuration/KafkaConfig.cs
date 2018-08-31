using System.Collections.Generic;

namespace MidnightLizard.Schemes.Screenshots.Configuration
{
    public class KafkaConfig
    {
        public Dictionary<string, object> KAFKA_EVENTS_CONSUMER_CONFIG { get; set; }
        public string SCHEMES_EVENTS_TOPIC { get; set; }
    }
}