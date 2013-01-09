using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace S2.Munin.Plugins.Core
{
    public class Netstat
    {
        // is TCP used in old systems?
        internal static readonly string[] categories = { "TCP", "TCPv4", "TCPv6" };

        private bool logarithmicGraph = true;

        List<PerformanceCounter> activeConnectionCounterList = new List<PerformanceCounter>();
        List<PerformanceCounter> establishedConnectionCounterList = new List<PerformanceCounter>();
        List<PerformanceCounter> failedConnectionCounterList = new List<PerformanceCounter>();
        List<PerformanceCounter> passiveConnectionCounterList = new List<PerformanceCounter>();
        List<PerformanceCounter> resetConnectionCounterList = new List<PerformanceCounter>();

        public Netstat(bool logarithmic, string categoriesToCheck)
        {
            this.logarithmicGraph = logarithmic;

            string [] categoryArray;
            if (string.IsNullOrEmpty(categoriesToCheck))
            {
                categoryArray = categories;
            }
            else
            {
                categoryArray = Regex.Split(categoriesToCheck, "\\s*,\\s*");
            }

            foreach (string categoryName in categoryArray)
            {
                if (!PerformanceCounterCategory.Exists(categoryName))
                {
                    continue;
                }
                PerformanceCounterCategory category = new PerformanceCounterCategory(categoryName);

                PerformanceCounter counter = category.GetCounters().Where(pc => pc.CounterName == @"Connections Active").FirstOrDefault();
                if (counter != null)
                {
                    counter.NextValue();
                    this.activeConnectionCounterList.Add(counter);
                }
                counter = category.GetCounters().Where(pc => pc.CounterName == @"Connections Established").FirstOrDefault();
                if (counter != null)
                {
                    counter.NextValue();
                    this.establishedConnectionCounterList.Add(counter);
                }
                counter = category.GetCounters().Where(pc => pc.CounterName == @"Connection Failures").FirstOrDefault();
                if (counter != null)
                {
                    counter.NextValue();
                    this.failedConnectionCounterList.Add(counter);
                }
                counter = category.GetCounters().Where(pc => pc.CounterName == @"Connections Passive").FirstOrDefault();
                if (counter != null)
                {
                    counter.NextValue();
                    this.passiveConnectionCounterList.Add(counter);
                }
                counter = category.GetCounters().Where(pc => pc.CounterName == @"Connections Reset").FirstOrDefault();
                if (counter != null)
                {
                    counter.NextValue();
                    this.resetConnectionCounterList.Add(counter);
                }
            }
        }

        public string GetConfiguration(string graphName)
        {
            StringBuilder configuration = new StringBuilder();

            configuration.Append("graph_title Netstat\n");
            configuration.AppendFormat("graph_args --base 1000{0}\n", this.logarithmicGraph ? " --logarithmic" : "");
            configuration.Append("graph_vlabel active connections\n");
            configuration.Append("graph_category network\n");
            configuration.Append("graph_info This graph shows the TCP activity of all the network interfaces combined\n");
            if (this.activeConnectionCounterList.Count > 0)
            {
                configuration.Append("active.label active\n");
                configuration.Append("active.type DERIVE\n");
                configuration.Append("active.min 0\n");
                configuration.Append("active.info The number of active TCP connection openings\n");
            }
            if (this.establishedConnectionCounterList.Count > 0)
            {
                configuration.Append("established.label established\n");
                configuration.Append("established.type GAUGE\n");
                configuration.Append("established.min 0\n");
                configuration.Append("established.info The number of currently open connections\n");
            }
            if (this.failedConnectionCounterList.Count > 0)
            {
                configuration.Append("failed.label failed\n");
                configuration.Append("failed.type DERIVE\n");
                configuration.Append("failed.min 0\n");
                configuration.Append("failed.info The number of failed TCP connection attempts\n");
            }
            if (this.passiveConnectionCounterList.Count > 0)
            {
                configuration.Append("passive.label passive\n");
                configuration.Append("passive.type DERIVE\n");
                configuration.Append("passive.min 0\n");
                configuration.Append("passive.info The number of passive TCP connection openings\n");
            }
            if (this.resetConnectionCounterList.Count > 0)
            {
                configuration.Append("resets.label resets\n");
                configuration.Append("resets.type DERIVE\n");
                configuration.Append("resets.min 0\n");
                configuration.Append("resets.info The number of TCP connection resets\n");
            }

            return configuration.ToString();
        }

        public string GetValues(string graphName)
        {
            StringBuilder values = new StringBuilder();

            if (this.activeConnectionCounterList.Count > 0)
            {
                long count = 0;
                foreach (PerformanceCounter counter in this.activeConnectionCounterList)
                {
                    count += (long)counter.NextValue();
                }
                values.AppendFormat(CultureInfo.InvariantCulture, "active.value {0}\n", count);
            }
            if (this.establishedConnectionCounterList.Count > 0)
            {
                long count = 0;
                foreach (PerformanceCounter counter in this.establishedConnectionCounterList)
                {
                    count += (long)counter.NextValue();
                }
                values.AppendFormat(CultureInfo.InvariantCulture, "established.value {0}\n", count);
            }
            if (this.failedConnectionCounterList.Count > 0)
            {
                long count = 0;
                foreach (PerformanceCounter counter in this.failedConnectionCounterList)
                {
                    count += (long)counter.NextValue();
                }
                values.AppendFormat(CultureInfo.InvariantCulture, "failed.value {0}\n", count);
            }
            if (this.passiveConnectionCounterList.Count > 0)
            {
                long count = 0;
                foreach (PerformanceCounter counter in this.passiveConnectionCounterList)
                {
                    count += (long)counter.NextValue();
                }
                values.AppendFormat(CultureInfo.InvariantCulture, "passive.value {0}\n", count);
            }
            if (this.resetConnectionCounterList.Count > 0)
            {
                long count = 0;
                foreach (PerformanceCounter counter in this.resetConnectionCounterList)
                {
                    count += (long)counter.NextValue();
                }
                values.AppendFormat(CultureInfo.InvariantCulture, "resets.value {0}\n", count);
            }

            return values.ToString();
        }
    }
}
