﻿using System;
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

        public Network()
        {
            // find network devices
            // http://weblogs.sqlteam.com/mladenp/archive/2010/11/04/find-only-physical-network-adapters-with-wmi-win32_networkadapter-class.aspx
            ManagementObjectSearcher mos = new ManagementObjectSearcher(@"SELECT * 
                                     FROM   Win32_NetworkAdapter
                                     WHERE  Manufacturer != 'Microsoft' 
                                            AND NOT PNPDeviceID LIKE 'ROOT\\%'");

            IList<string> interfaces = mos.Get()
                                                  .Cast<ManagementObject>()
                                                  .OrderBy(p => Convert.ToUInt32(p.Properties["Index"].Value)).Select(p => p.Properties["Description"].Value as string)
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
                string trafficGraphName = "if_" + interfaceName;
                string errorGraphName = "if_err_" + interfaceName;
                PerformanceCounter[] counter = category.GetCounters(instance);

                PerformanceCounter inputCounter = counter.Where(pc => pc.CounterName == @"Bytes Received/sec").FirstOrDefault();
                PerformanceCounter outputCounter = counter.Where(pc => pc.CounterName == @"Bytes Sent/sec").FirstOrDefault();
                this.inputCounter.Add(trafficGraphName, inputCounter);
                this.outputCounter.Add(trafficGraphName, outputCounter);
                inputCounter.NextValue();
                outputCounter.NextValue();
                graphNames.Add(trafficGraphName);

                inputCounter = counter.Where(pc => pc.CounterName == @"Packets Received Errors").FirstOrDefault();
                outputCounter = counter.Where(pc => pc.CounterName == @"Packets Outbound Errors").FirstOrDefault();
                this.inputCounter.Add(errorGraphName, inputCounter);
                this.outputCounter.Add(errorGraphName, outputCounter);
                inputCounter.NextValue();
                outputCounter.NextValue();
                graphNames.Add(errorGraphName);
            }
            this.Graphs = graphNames;
        }

        public string GetConfiguration(string graph)
        {
            StringBuilder configuration = new StringBuilder();

            PerformanceCounter counter = this.inputCounter[graph];

            if (graph.StartsWith("if_err_"))
            {
                configuration.AppendFormat(CultureInfo.InvariantCulture, "graph_title {0} errors\n", counter.InstanceName);
                configuration.Append("graph_args --base 1000\n");
                configuration.Append("graph_vlabel packets in (-) / out (+)\n");
                configuration.Append("graph_category network\n");
                configuration.Append("graph_order rcvd trans\n");

                configuration.AppendFormat("rcvd.label received\n");
                configuration.AppendFormat("rcvd.type COUNTER\n");
                configuration.AppendFormat("rcvd.graph no\n");
                configuration.AppendFormat("rcvd.min 0\n");

                configuration.AppendFormat("trans.label packets\n");
                configuration.AppendFormat("trans.type COUNTER\n");
                configuration.AppendFormat("trans.negative rcvd\n");
                configuration.AppendFormat("trans.min 0\n");
            }
            else
            {
                configuration.AppendFormat(CultureInfo.InvariantCulture, "graph_title {0} traffic\n", counter.InstanceName);
                configuration.Append("graph_args --base 1000\n");
                configuration.Append("graph_vlabel bit in (-) / out (+)\n");
                configuration.Append("graph_category network\n");
                configuration.Append("graph_order rcvd trans\n");

                configuration.AppendFormat("rcvd.label received\n");
                configuration.AppendFormat("rcvd.type DERIVE\n");
                configuration.AppendFormat("rcvd.graph no\n");
                configuration.AppendFormat("rcvd.cdef rcvd,8,*\n");
                configuration.AppendFormat("rcvd.min 0\n");

                configuration.AppendFormat("trans.label bps\n");
                configuration.AppendFormat("trans.type DERIVE\n");
                configuration.AppendFormat("trans.negative rcvd\n");
                configuration.AppendFormat("trans.cdef trans,8,*\n");
                configuration.AppendFormat("trans.min 0\n");
                configuration.AppendFormat("trans.info traffic of the {0} interface.\n", counter.InstanceName);
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