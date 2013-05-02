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

        public bool Initialize(IDictionary<string, string> settings)
        {
            // configuration values
            Settings coreSettings = new Settings(settings);

            // single graph delegates
            if (!coreSettings.DisableCpu)
            {
                Cpu cpu = new Cpu();
                configurationDelegates.Add("cpu", cpu.GetConfiguration);
                valueDelegates.Add("cpu", cpu.GetValues);
                enabledGraphs.Add("cpu");
            }
            if (!coreSettings.DisableDiskIO || !coreSettings.DisableDiskSpace)
            {
                Disk disk = new Disk(coreSettings);
                if (!coreSettings.DisableDiskSpace)
                {
                    configurationDelegates.Add("disk_space", disk.GetFreeSpaceConfiguration);
                    valueDelegates.Add("disk_space", disk.GetFreeSpaceValues);
                    enabledGraphs.Add("disk_space");
                }
                if (!coreSettings.DisableDiskIO)
                {
                    configurationDelegates.Add("disk_io", disk.GetIOConfiguration);
                    valueDelegates.Add("disk_io", disk.GetIOValues);
                    enabledGraphs.Add("disk_io");
                }
            }
            if (!coreSettings.DisableMemory)
            {
                Memory memory = new Memory();
                configurationDelegates.Add("memory", memory.GetConfiguration);
                valueDelegates.Add("memory", memory.GetValues);
                enabledGraphs.Add("memory");
            }
            if (!coreSettings.DisableProcesses)
            {
                Processes processes = new Processes(coreSettings);
                configurationDelegates.Add("processes", processes.GetConfiguration);
                valueDelegates.Add("processes", processes.GetValues);
                enabledGraphs.Add("processes");
            }
            if (!coreSettings.DisableNetstat)
            {
                Netstat netstat = new Netstat(coreSettings);
                configurationDelegates.Add("netstat", netstat.GetConfiguration);
                valueDelegates.Add("netstat", netstat.GetValues);
                enabledGraphs.Add("netstat");
            }

            // multi graph delegates
            if (!coreSettings.DisableNetworkIO || !coreSettings.DisableNetworkErrors)
            {
                Network network = new Network(coreSettings);
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

        public string GetConfiguration(string graphName)
        {
            return configurationDelegates[graphName].Invoke(graphName);
        }

        public string GetValues(string graphName)
        {
            return valueDelegates[graphName].Invoke(graphName);
        }

        public void StopPlugin()
        {
            // nothing to do
        }

    }
}
