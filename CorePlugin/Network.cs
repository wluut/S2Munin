using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace S2.Munin.Plugins.Core
{
    public class Network
    {
        private Dictionary<string, PerformanceCounter> inputCounter = new Dictionary<string, PerformanceCounter>();
        private Dictionary<string, PerformanceCounter> outputCounter = new Dictionary<string, PerformanceCounter>();

        public IList<string> Graphs { get; private set; }

        public Network(Settings settings)
        {
            // find network devices
            // http://weblogs.sqlteam.com/mladenp/archive/2010/11/04/find-only-physical-network-adapters-with-wmi-win32_networkadapter-class.aspx
            ManagementObjectSearcher mos = new ManagementObjectSearcher(@"SELECT * 
                                     FROM   Win32_NetworkAdapter
                                     WHERE  Manufacturer != 'Microsoft' 
                                            AND NOT PNPDeviceID LIKE 'ROOT\\%'");

            IList<string> interfaces = mos.Get()
                                                  .Cast<ManagementObject>()
                                                  .OrderBy(p => Convert.ToUInt32(p.Properties["Index"].Value, CultureInfo.InvariantCulture))
                                                  .Select(p => p.Properties["Description"].Value as string)
                                                  .Distinct()
                                                  .ToList();

            List<string> graphNames = new List<string>();
            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
            foreach (string instance in interfaces)
            {
                if (!category.InstanceExists(instance))
                {
                    continue;
                }

                string interfaceName = Regex.Replace(instance.ToLowerInvariant(), "[^a-z0-9]", "");
                if ((settings.DisabledNetworkInterfaces != null) && settings.DisabledNetworkInterfaces.Contains(interfaceName))
                {
                    continue;
                }
                string trafficGraphName = "if_" + interfaceName;
                string errorGraphName = "if_err_" + interfaceName;
                PerformanceCounter[] counter = category.GetCounters(instance);

                if (!settings.DisableNetworkIO)
                {
                    PerformanceCounter inputCounter = counter.Where(pc => pc.CounterName == @"Bytes Received/sec").FirstOrDefault();
                    PerformanceCounter outputCounter = counter.Where(pc => pc.CounterName == @"Bytes Sent/sec").FirstOrDefault();
                    this.inputCounter.Add(trafficGraphName, inputCounter);
                    this.outputCounter.Add(trafficGraphName, outputCounter);
                    inputCounter.NextValue();
                    outputCounter.NextValue();
                    graphNames.Add(trafficGraphName);
                }

                if (!settings.DisableNetworkErrors)
                {
                    PerformanceCounter inputCounter = counter.Where(pc => pc.CounterName == @"Packets Received Errors").FirstOrDefault();
                    PerformanceCounter outputCounter = counter.Where(pc => pc.CounterName == @"Packets Outbound Errors").FirstOrDefault();
                    this.inputCounter.Add(errorGraphName, inputCounter);
                    this.outputCounter.Add(errorGraphName, outputCounter);
                    inputCounter.NextValue();
                    outputCounter.NextValue();
                    graphNames.Add(errorGraphName);
                }
            }
            this.Graphs = graphNames;
        }

        public string GetConfiguration(string graph)
        {
            StringBuilder configuration = new StringBuilder();

            PerformanceCounter counter = this.inputCounter[graph];

            if (graph.StartsWith("if_err_", StringComparison.OrdinalIgnoreCase))
            {
                configuration.AppendFormat(CultureInfo.InvariantCulture, "graph_title {0} errors\n", counter.InstanceName);
                configuration.Append("graph_args --base 1000\n");
                configuration.Append("graph_vlabel packets in (-) / out (+)\n");
                configuration.Append("graph_category network\n");
                configuration.Append("graph_order rcvd trans\n");

                configuration.Append("rcvd.label received\n");
                configuration.Append("rcvd.type COUNTER\n");
                configuration.Append("rcvd.graph no\n");
                configuration.Append("rcvd.min 0\n");

                configuration.Append("trans.label packets\n");
                configuration.Append("trans.type COUNTER\n");
                configuration.Append("trans.negative rcvd\n");
                configuration.Append("trans.min 0\n");
            }
            else
            {
                configuration.AppendFormat(CultureInfo.InvariantCulture, "graph_title {0} traffic\n", counter.InstanceName);
                configuration.Append("graph_args --base 1000\n");
                configuration.Append("graph_vlabel bit in (-) / out (+)\n");
                configuration.Append("graph_category network\n");
                configuration.Append("graph_order rcvd trans\n");

                configuration.Append("rcvd.label received\n");
                configuration.Append("rcvd.type DERIVE\n");
                configuration.Append("rcvd.graph no\n");
                configuration.Append("rcvd.cdef rcvd,8,*\n");
                configuration.Append("rcvd.min 0\n");

                configuration.Append("trans.label bps\n");
                configuration.Append("trans.type DERIVE\n");
                configuration.Append("trans.negative rcvd\n");
                configuration.Append("trans.cdef trans,8,*\n");
                configuration.Append("trans.min 0\n");
                configuration.AppendFormat(CultureInfo.InvariantCulture, "trans.info traffic of the {0} interface.\n", counter.InstanceName);
            }
            return configuration.ToString();
        }

        public string GetValues(string graph)
        {
            StringBuilder values = new StringBuilder();

            PerformanceCounter inCounter = this.inputCounter[graph];
            PerformanceCounter outCounter = this.outputCounter[graph];

            values.AppendFormat(CultureInfo.InvariantCulture, "rcvd.value {0}\n", (ulong)inCounter.NextValue());
            values.AppendFormat(CultureInfo.InvariantCulture, "trans.value {0}\n", (ulong)outCounter.NextValue());

            return values.ToString();
        }

    }
}
