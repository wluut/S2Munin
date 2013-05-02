using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace S2.Munin.Plugins.Core
{
    public class Cpu
    {
        private Dictionary<string, PerformanceCounter> counterMap = new Dictionary<string, PerformanceCounter>();

        public Cpu(Settings settings)
        {
            PerformanceCounterCategory category = new PerformanceCounterCategory("Processor");
            foreach (string instanceName in category.GetInstanceNames())
            {
                PerformanceCounter counter = category.GetCounters(instanceName).Where(pc => pc.CounterName == @"% Processor Time").FirstOrDefault();
                string counterName = instanceName;
                if (Regex.IsMatch(instanceName, "^\\d+$"))
                {
                    if (settings.CpuDisplay == Settings.CpuDisplayType.TOTAL)
                    {
                        continue;
                    }
                    counterName = string.Format(CultureInfo.InvariantCulture, "cpu_{0}", instanceName);
                }
                else
                {
                    if (settings.CpuDisplay == Settings.CpuDisplayType.SINGLE)
                    {
                        continue;
                    }
                    counterName = Regex.Replace(instanceName.ToLowerInvariant(), "[^a-z0-9]", "");
                }
                counter.NextValue();
                this.counterMap.Add(counterName, counter);
            }
        }

        public string GetConfiguration(string graphName)
        {
            StringBuilder configuration = new StringBuilder();

            List<string> graphOrder = new List<string>(this.counterMap.Keys);
            graphOrder.Sort();

            configuration.Append("graph_title CPU usage\n");
            configuration.Append("graph_args -l 0 --vertical-label percent --upper-limit 100\n");
            configuration.Append("graph_category system\n");
            configuration.AppendFormat("graph_order {0}\n", string.Join(" ", graphOrder.ToArray()));
            foreach (string counterName in graphOrder)
            {
                PerformanceCounter counter = this.counterMap[counterName];

                configuration.AppendFormat("{0}.label CPU {1}\n", counterName, counter.InstanceName);
                configuration.AppendFormat("{0}.draw LINE\n", counterName);
                configuration.AppendFormat("{0}.info CPU {1} {2}.\n", counterName, counter.InstanceName, counter.CounterName);
            }
            return configuration.ToString();
        }

        public string GetValues(string graphName)
        {
            StringBuilder values = new StringBuilder();
            foreach (KeyValuePair<string, PerformanceCounter> counter in this.counterMap)
            {
                values.AppendFormat(CultureInfo.InvariantCulture, "{0}.value {1}\n", counter.Key, counter.Value.NextValue());
            }
            return values.ToString();
        }
    }
}
