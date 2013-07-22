using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using S2.Munin.Plugin;

namespace S2.Munin.Plugins.PerformanceCounter
{
    public class PerformanceCounterPlugin : IMuninPlugin
    {
        private IDictionary<string, Graph> graphs = new Dictionary<string, Graph>();

        public string PluginName { get { return "PerformanceCounter"; } }

        public ILogger Logger { private get; set; }

        public bool Initialize(IDictionary<string, string> settings)
        {
            // split settings into single graphs
            Dictionary<string, Dictionary<string, string>> graphSettings = new Dictionary<string, Dictionary<string, string>>();
            foreach (KeyValuePair<string, string> entry in settings)
            {
                Match match = Regex.Match(entry.Key, "^([^\\.]+)\\.(.+)$");
                if (!match.Success)
                {
                    continue;
                }
                string graph = match.Groups[1].Value;
                string subsetting = match.Groups[2].Value;
                Dictionary<string, string> g;
                if (graphSettings.ContainsKey(graph))
                {
                    g = graphSettings[graph];
                }
                else
                {
                    g = new Dictionary<string, string>();
                    graphSettings.Add(graph, g);
                }
                g.Add(subsetting, entry.Value);
            }

            // setup graphs
            foreach (string graphName in graphSettings.Keys)
            {
                Dictionary<string, string> subsettings = graphSettings[graphName];

                Graph graph = new Graph();
                graph.Arguments = new Dictionary<string, string>();
                graph.Counter = new Dictionary<string, GraphCounter>();

                foreach (KeyValuePair<string, string> entry in subsettings)
                {
                    Match match = Regex.Match(entry.Key, "^([^\\.]+)\\.(.+)$");
                    if (!match.Success)
                    {
                        // no "." => graph argument
                        graph.Arguments.Add(entry.Key, entry.Value);
                        continue;
                    }
                    string counterName = match.Groups[1].Value;
                    string counterArgument = match.Groups[2].Value;

                    GraphCounter counter;
                    if (graph.Counter.ContainsKey(counterName))
                    {
                        counter = graph.Counter[counterName];
                    }
                    else
                    {
                        counter = new GraphCounter();
                        counter.Arguments = new Dictionary<string, string>();
                        counter.FloatValue = true;
                        graph.Counter.Add(counterName, counter);
                    }
                    if (counterArgument == "counter")
                    {
                        counter.PerformanceCounter = GetPerformanceCounter(entry.Value);
                        continue;
                    }
                    counter.Arguments.Add(counterArgument, entry.Value);
                    if (counterArgument == "type")
                    {
                        counter.FloatValue = (entry.Value.ToUpperInvariant() == "GAUGE");
                    }
                   
                }
                // cleanup
                List<string> missing = new List<string>();
                foreach (KeyValuePair<string, GraphCounter> entry in graph.Counter)
                {
                    if (entry.Value.PerformanceCounter == null)
                    {
                        missing.Add(entry.Key);
                    }
                }
                foreach (string counterName in missing)
                {
                    graph.Counter.Remove(counterName);
                }
                if (graph.Counter.Count > 0)
                {
                    this.graphs.Add(graphName, graph);
                }
            }
            return this.graphs.Count > 0;
        }

        public IList<string> GetGraphs()
        {
            return new List<string>(this.graphs.Keys);
        }
        public string GetConfiguration(string graphName)
        {
            if (!this.graphs.ContainsKey(graphName))
            {
                return "";
            }
            Graph graph = this.graphs[graphName];
            StringBuilder configuration = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in graph.Arguments)
            {
                configuration.AppendFormat(CultureInfo.InvariantCulture, "graph_{0} {1}\n", entry.Key, entry.Value);
            }
            foreach (KeyValuePair<string, GraphCounter> counter in graph.Counter)
            {
                foreach (KeyValuePair<string, string> entry in counter.Value.Arguments)
                {
                    configuration.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1} {2}\n", counter.Key, entry.Key, entry.Value);
                }
            }
            return configuration.ToString();
        }

        public string GetValues(string graphName)
        {
            if (!this.graphs.ContainsKey(graphName))
            {
                return "";
            }
            Graph graph = this.graphs[graphName];
            StringBuilder values = new StringBuilder();
            foreach (KeyValuePair<string, GraphCounter> counter in graph.Counter)
            {
                if (counter.Value.FloatValue)
                {
                    values.AppendFormat(CultureInfo.InvariantCulture, "{0}.value {1}\n", counter.Key, counter.Value.PerformanceCounter.NextValue());
                }
                else
                {
                    ulong value = (ulong)counter.Value.PerformanceCounter.NextValue();
                    values.AppendFormat(CultureInfo.InvariantCulture, "{0}.value {1}\n", counter.Key, value);
                }
            }
            return values.ToString();
        }

        public void StopPlugin()
        {
            // nothing to do
        }


        private static System.Diagnostics.PerformanceCounter GetPerformanceCounter(string path)
        {
            // check if performance counter path exists
            string host = null;
            if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                int index = path.IndexOf('\\', 2);
                host = path.Substring(2, index - 2);
                path = path.Substring(index);

            }
            if (!path.StartsWith(@"\", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            int categoryIndex = path.IndexOf('\\', 1);
            if (categoryIndex < 1)
            {
                return null;
            }
            string categoryPath = path.Substring(1, categoryIndex - 1);
            string counterPath = path.Substring(categoryIndex + 1);

            string instancePath = "";
            Match instanceMatch = Regex.Match(categoryPath, @"^(.+)\((.+)\)$");
            if (instanceMatch.Success)
            {
                categoryPath = instanceMatch.Groups[1].Value;
                instancePath = instanceMatch.Groups[2].Value;
            }

            PerformanceCounterCategory category;
            if (string.IsNullOrEmpty(host))
            {
                if (!PerformanceCounterCategory.Exists(categoryPath))
                {
                    return null;
                }
                category = new PerformanceCounterCategory(categoryPath);
            }
            else
            {
                if (!PerformanceCounterCategory.Exists(categoryPath, host))
                {
                    return null;
                }
                category = new PerformanceCounterCategory(categoryPath, host);
            }
            if (category.CategoryType == PerformanceCounterCategoryType.Unknown)
            {
                return null;
            }
            if (string.IsNullOrEmpty(instancePath) != (category.CategoryType == PerformanceCounterCategoryType.SingleInstance))
            {
                return null;
            }
            if (!category.CounterExists(counterPath))
            {
                return null;
            }
            System.Diagnostics.PerformanceCounter performanceCounter;
            if (!string.IsNullOrEmpty(instancePath))
            {
                if (!category.InstanceExists(instancePath))
                {
                    return null;
                }
                performanceCounter = category.GetCounters(instancePath).Where(pc => pc.CounterName == counterPath).FirstOrDefault();
            }
            else
            {
                performanceCounter = category.GetCounters().Where(pc => pc.CounterName == counterPath).FirstOrDefault();
            }
            performanceCounter.NextValue();
            return performanceCounter;
        }
    }
}
