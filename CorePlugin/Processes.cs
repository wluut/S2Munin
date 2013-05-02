using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace S2.Munin.Plugins.Core
{
    public class Processes
    {
        protected PerformanceCounter ProcessCounter { get; set; }
        protected PerformanceCounter ThreadCounter { get; set; }
        protected bool LogarithmicGraph { get; set; }

        public Processes(Settings settings)
        {
            this.LogarithmicGraph = settings.ProcessesLogarithmic;
            PerformanceCounterCategory category = new PerformanceCounterCategory("System");

            this.ProcessCounter = category.GetCounters().Where(pc => pc.CounterName == @"Processes").FirstOrDefault();
            this.ProcessCounter.NextValue();

            this.ThreadCounter = category.GetCounters().Where(pc => pc.CounterName == @"Threads").FirstOrDefault();
            this.ThreadCounter.NextValue();
        }

        public string GetConfiguration(string graphName)
        {
            StringBuilder configuration = new StringBuilder();

            configuration.Append("graph_title Processes\n");
            configuration.AppendFormat(CultureInfo.InvariantCulture, "graph_args --base 1000{0}\n", this.LogarithmicGraph ? " --logarithmic" : "");
            configuration.Append("graph_vlabel Number of processes\n");
            configuration.Append("graph_category processes\n");
            configuration.Append("graph_order processes_processes processes_threads\n");

            configuration.Append("processes_processes.label processes\n");
            configuration.Append("processes_processes.draw LINE\n");
            configuration.AppendFormat(CultureInfo.InvariantCulture, "processes_processes.info {0}.\n", this.ProcessCounter.CounterName);

            configuration.Append("processes_threads.label threads\n");
            configuration.Append("processes_threads.draw LINE\n");
            configuration.AppendFormat(CultureInfo.InvariantCulture, "processes_threads.info {0}.\n", this.ThreadCounter.CounterName);

            return configuration.ToString();
        }

        public string GetValues(string graphName)
        {
            StringBuilder values = new StringBuilder();

            values.AppendFormat(CultureInfo.InvariantCulture, "processes_processes.value {0}\n", this.ProcessCounter.NextValue());
            values.AppendFormat(CultureInfo.InvariantCulture, "processes_threads.value {0}\n", this.ThreadCounter.NextValue());

            return values.ToString();
        }

    }
}
