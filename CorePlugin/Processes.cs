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
        protected PerformanceCounter processCounter;
        protected PerformanceCounter threadCounter;
        protected bool logarithmicGraph;

        public Processes(Settings settings)
        {
            this.logarithmicGraph = settings.ProcessesLogarithmic;
            PerformanceCounterCategory category = new PerformanceCounterCategory("System");

            this.processCounter = category.GetCounters().Where(pc => pc.CounterName == @"Processes").FirstOrDefault();
            this.processCounter.NextValue();

            this.threadCounter = category.GetCounters().Where(pc => pc.CounterName == @"Threads").FirstOrDefault();
            this.threadCounter.NextValue();
        }

        public string GetConfiguration(string graphName)
        {
            StringBuilder configuration = new StringBuilder();

            configuration.Append("graph_title Processes\n");
            configuration.AppendFormat("graph_args --base 1000{0}\n", this.logarithmicGraph ? " --logarithmic" : "");
            configuration.Append("graph_vlabel Number of processes\n");
            configuration.Append("graph_category processes\n");
            configuration.Append("graph_order processes_processes processes_threads\n");

            configuration.AppendFormat("processes_processes.label processes\n");
            configuration.AppendFormat("processes_processes.draw LINE\n");
            configuration.AppendFormat("processes_processes.info {0}.\n", processCounter.CounterName);

            configuration.AppendFormat("processes_threads.label threads\n");
            configuration.AppendFormat("processes_threads.draw LINE\n");
            configuration.AppendFormat("processes_threads.info {0}.\n", threadCounter.CounterName);

            return configuration.ToString();
        }

        public string GetValues(string graphName)
        {
            StringBuilder values = new StringBuilder();

            values.AppendFormat(CultureInfo.InvariantCulture, "processes_processes.value {0}\n", processCounter.NextValue());
            values.AppendFormat(CultureInfo.InvariantCulture, "processes_threads.value {0}\n", threadCounter.NextValue());

            return values.ToString();
        }

    }
}
