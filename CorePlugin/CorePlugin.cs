using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using S2.Munin.Plugin;

namespace S2.Munin.Plugins.Core
{
    public delegate string GraphDelegate(string graphName);

    public class CorePlugin : IMuninPlugin
    {
        private Dictionary<string, GraphDelegate> valueDelegates = new Dictionary<string, GraphDelegate>();
        private Dictionary<string, GraphDelegate> configurationDelegates = new Dictionary<string, GraphDelegate>();
        private List<string> enabledGraphs = new List<string>();

        public string PluginName { get { return "Core"; } }

        public bool Initialize(IDictionary<string, string> configuration)
        {
            // configuration values
            Settings settings = new Settings(configuration);

            // single graph delegates
            if (!settings.DisableCpu)
            {
                Cpu cpu = new Cpu();
                configurationDelegates.Add("cpu", cpu.GetConfiguration);
                valueDelegates.Add("cpu", cpu.GetValues);
                enabledGraphs.Add("cpu");
            }
            if (!settings.DisableDiskIO || !settings.DisableDiskSpace)
            {
                Disk disk = new Disk(settings);
                if (!settings.DisableDiskSpace)
                {
                    configurationDelegates.Add("disk_space", disk.GetFreeSpaceConfiguration);
                    valueDelegates.Add("disk_space", disk.GetFreeSpaceValues);
                    enabledGraphs.Add("disk_space");
                }
                if (!settings.DisableDiskIO)
                {
                    configurationDelegates.Add("disk_io", disk.GetIOConfiguration);
                    valueDelegates.Add("disk_io", disk.GetIOValues);
                    enabledGraphs.Add("disk_io");
                }
            }
            if (!settings.DisableMemory)
            {
                Memory memory = new Memory();
                configurationDelegates.Add("memory", memory.GetConfiguration);
                valueDelegates.Add("memory", memory.GetValues);
                enabledGraphs.Add("memory");
            }
            if (!settings.DisableProcesses)
            {
                Processes processes = new Processes(settings);
                configurationDelegates.Add("processes", processes.GetConfiguration);
                valueDelegates.Add("processes", processes.GetValues);
                enabledGraphs.Add("processes");
            }
            if (!settings.DisableNetstat)
            {
                Netstat netstat = new Netstat(settings);
                configurationDelegates.Add("netstat", netstat.GetConfiguration);
                valueDelegates.Add("netstat", netstat.GetValues);
                enabledGraphs.Add("netstat");
            }

            // multi graph delegates
            if (!settings.DisableNetworkIO || !settings.DisableNetworkErrors)
            {
                Network network = new Network(settings);
                foreach (string graph in network.Graphs)
                {
                    configurationDelegates.Add(graph, network.GetConfiguration);
                    valueDelegates.Add(graph, network.GetValues);
                    enabledGraphs.Add(graph);
                }
            }
            return true;
        }

        public IList<string> GetGraphs()
        {
            return enabledGraphs.AsReadOnly();
        }

        public string GetConfiguration(string graph)
        {
            return configurationDelegates[graph].Invoke(graph);
        }

        public string GetValues(string graph)
        {
            return valueDelegates[graph].Invoke(graph);
        }

        public void StopPlugin()
        {
            // nothing to do
        }

    }
}
