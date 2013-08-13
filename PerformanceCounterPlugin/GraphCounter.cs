using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S2.Munin.Plugins.PerformanceCounter
{
    internal class GraphCounter
    {
        public IDictionary<string, string> Arguments { get; set; }
        public string CounterPath { get; set; }
        public System.Diagnostics.PerformanceCounter PerformanceCounter { get; set; }
        public bool FloatValue { get; set; }
    }
}
