using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using S2.Munin.Plugin;

namespace S2.Munin.Plugins.PowerShell
{
    public class PowerShellPlugin : IMuninPlugin
    {
        private IDictionary<string, string> graphs = new Dictionary<string, string>();

        public string PluginName { get { return "PowerShell"; } }

        public bool Initialize(IDictionary<string, string> settings)
        {
            foreach (KeyValuePair<string, string> entry in settings)
            {
                if (File.Exists(entry.Value))
                {
                    this.graphs.Add(entry);
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

            return this.RunScript(this.graphs[graphName], "config");
        }

        public string GetValues(string graphName)
        {
            if (!this.graphs.ContainsKey(graphName))
            {
                return "";
            }
            return this.RunScript(this.graphs[graphName]);
        }

        public void StopPlugin()
        {
            // nothing to do
        }

        private string RunScript(string scriptPath, params string[] arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = string.Format("-executionpolicy unrestricted -file \"{0}\" \"{1}\"", scriptPath, string.Join("\" \"", arguments));
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }
}
