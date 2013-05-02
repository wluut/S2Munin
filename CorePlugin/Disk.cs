using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace S2.Munin.Plugins.Core
{
    public class Disk
    {
        private Dictionary<string, PerformanceCounter> freeSpaceCounterMap = new Dictionary<string, PerformanceCounter>();
        private Dictionary<string, PerformanceCounter> readCounterMap = new Dictionary<string, PerformanceCounter>();
        private Dictionary<string, PerformanceCounter> writeCounterMap = new Dictionary<string, PerformanceCounter>();
        private Settings.DiskSpaceDisplayType diskSpaceDisplay;

        public Disk(Settings settings)
        {
            this.diskSpaceDisplay = settings.DiskSpaceDisplay;
            PerformanceCounterCategory category = new PerformanceCounterCategory("LogicalDisk");
            foreach (string instanceName in category.GetInstanceNames())
            {
                // names
                string deviceName = instanceName;
                if (Regex.IsMatch(instanceName, "^[a-zA-Z]:$"))
                {
                    deviceName = instanceName.Substring(0, 1).ToLowerInvariant();
                }
                else if (instanceName == "_Total")
                {
                    deviceName = "total";
                }
                else
                {
                    continue;
                }


                // Free Space
                PerformanceCounter counter = category.GetCounters(instanceName).Where(pc => pc.CounterName == @"% Free Space").FirstOrDefault();
                string freeSpaceCounterName = string.Format(CultureInfo.InvariantCulture, "disk_space_{0}_{1}", this.diskSpaceDisplay.ToString().ToLowerInvariant(), deviceName);
                counter.NextValue();
                this.freeSpaceCounterMap.Add(freeSpaceCounterName, counter);

                // Disk IO
                var counters = category.GetCounters(instanceName);
                PerformanceCounter readCounter = category.GetCounters(instanceName).Where(pc => pc.CounterName == @"Disk Read Bytes/sec").FirstOrDefault();
                PerformanceCounter writeCounter = category.GetCounters(instanceName).Where(pc => pc.CounterName == @"Disk Write Bytes/sec").FirstOrDefault();
                string ioCounterName = string.Format(CultureInfo.InvariantCulture, "disk_io_{0}", deviceName);
                readCounter.NextValue();
                writeCounter.NextValue();
                this.readCounterMap.Add(ioCounterName, readCounter);
                this.writeCounterMap.Add(ioCounterName, writeCounter);
            }
        }

        public string GetFreeSpaceConfiguration(string graphName)
        {
            StringBuilder configuration = new StringBuilder();

            List<string> graphOrder = new List<string>(this.freeSpaceCounterMap.Keys);
            graphOrder.Sort();

            if (this.diskSpaceDisplay == Settings.DiskSpaceDisplayType.FREE)
            {
                configuration.Append("graph_title Free disk space\n");
            }
            else
            {
                configuration.Append("graph_title Used disk space\n");
            }
            configuration.Append("graph_args -l 0 --upper-limit 100\n");
            configuration.Append("graph_vlabel percent\n");
            configuration.Append("graph_category disk\n");
            configuration.AppendFormat("graph_order {0}\n", string.Join(" ", graphOrder.ToArray()));
            foreach (string counterName in graphOrder)
            {
                PerformanceCounter counter = this.freeSpaceCounterMap[counterName];

                configuration.AppendFormat("{0}.label Disk {1}\n", counterName, counter.InstanceName);
                configuration.AppendFormat("{0}.draw LINE\n", counterName);
                if (this.diskSpaceDisplay == Settings.DiskSpaceDisplayType.FREE)
                {
                    configuration.AppendFormat("{0}.info Disk {1} free space in percent.\n", counterName, counter.InstanceName);
                }
                else
                {
                    configuration.AppendFormat("{0}.info Disk {1} used space in percent.\n", counterName, counter.InstanceName);
                }
            }
            return configuration.ToString();
        }

        public string GetFreeSpaceValues(string graphName)
        {
            StringBuilder values = new StringBuilder();
            foreach (KeyValuePair<string, PerformanceCounter> counter in this.freeSpaceCounterMap)
            {
                float value = counter.Value.NextValue();
                if (this.diskSpaceDisplay == Settings.DiskSpaceDisplayType.USED)
                {
                    value = 100.0f - value;
                }
                values.AppendFormat(CultureInfo.InvariantCulture, "{0}.value {1}\n", counter.Key, value);
            }
            return values.ToString();
        }

        public string GetIOConfiguration(string graphName)
        {
            StringBuilder configuration = new StringBuilder();

            List<string> graphOrder = new List<string>(this.readCounterMap.Keys);
            graphOrder.Sort();

            configuration.Append("graph_title Disk I/O\n");
            configuration.Append("graph_args --base 1024 -l 0\n");
            configuration.Append("graph_vlabel bytes/second read (-) / written (+)\n");
            configuration.Append("graph_category disk\n");
            configuration.Append("graph_order");
            foreach (string counterName in graphOrder)
            {
                configuration.AppendFormat(CultureInfo.InvariantCulture, " {0}_read {0}_write", counterName);
            }
            configuration.Append("\n");
            foreach (string counterName in graphOrder)
            {
                PerformanceCounter readCounter = this.readCounterMap[counterName];

                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_read.label {1}\n", counterName, readCounter.InstanceName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_read.type DERIVE\n", counterName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_read.min 0\n", counterName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_read.graph no\n", counterName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_write.label {1}\n", counterName, readCounter.InstanceName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_write.info I/O on disk {1}\n", counterName, readCounter.InstanceName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_write.type DERIVE\n", counterName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_write.min 0\n", counterName);
                configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}_write.negative {0}_read\n", counterName);
            }
            return configuration.ToString();
        }

        public string GetIOValues(string graphName)
        {
            StringBuilder values = new StringBuilder();
            foreach (KeyValuePair<string, PerformanceCounter> counter in this.readCounterMap)
            {
                PerformanceCounter writeCounter = this.writeCounterMap[counter.Key];
                long readBytes = (long)counter.Value.NextValue();
                long writeBytes = (long)writeCounter.NextValue();
                values.AppendFormat(CultureInfo.InvariantCulture, "{0}_read.value {1}\n", counter.Key, readBytes);
                values.AppendFormat(CultureInfo.InvariantCulture, "{0}_write.value {1}\n", counter.Key, writeBytes);
            }
            return values.ToString();
        }

    }
}
