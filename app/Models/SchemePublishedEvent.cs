﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Screenshots.Models
{
    public class SchemePublishedEvent
    {
        public string AggregateId { get; set; }
        public ColorScheme ColorScheme { get; set; }
    }
}
