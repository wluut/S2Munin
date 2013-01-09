using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S2.Munin.Plugins.PerformanceCounter
{
    internal class Graph
    {
        public IDictionary<string, string> Arguments { get; set; }
        public IDictionary<string, GraphCounter> Counter { get; set; }
    }
}
