using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.Models
{
    public class TransportEvent
    {
        public string Type { get; set; }
        public JRaw Payload { get; set; }
    }
}
