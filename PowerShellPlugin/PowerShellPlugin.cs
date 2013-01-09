using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
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
            StringWriter output = new StringWriter();
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();

            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();

            runspace.SessionStateProxy.SetVariable("Output", output);

            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            scriptInvoker.Invoke("Set-ExecutionPolicy Bypass");

            Pipeline pipeline = runspace.CreatePipeline();

            //Here's how you add a new script with arguments
            Command command = new Command(scriptPath);
            foreach (string argument in arguments)
            {
                CommandParameter parameter = new CommandParameter(argument);
                command.Parameters.Add(parameter);
            }

            pipeline.Commands.Add(command);

            // Execute PowerShell script
            var results = pipeline.Invoke();

            return output.ToString();
        }
    }
}
