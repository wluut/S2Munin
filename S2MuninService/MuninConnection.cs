using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using S2.Munin.Plugin;

namespace S2.Munin.Service
{
    public class MuninConnection
    {
        private static readonly string nodeName = System.Net.Dns.GetHostName();
        private const string greetingFormat = "# s2 munin node at {0}";
        private const string versionFormat = "# s2 munin on host {0} version: {1}";
        private const string unknownService = "# unknown service\n.";
        private const string lineBreak = "\n";
        private static readonly Regex lineBreakPattern = new Regex("\\r?\\n\\r?");

        private TcpClient client;

        public MuninConnection(TcpClient client)
        {
            this.client = client;
        }

        public void Start()
        {
            Thread thread = new Thread(ConnectionLoop);
            thread.Start();
        }

        protected void WriteMessage(NetworkStream stream, string message)
        {
            if ((stream == null) || (message == null))
            {
                return;
            }
            // normalize message
            string normalizedMessage = lineBreakPattern.Replace(message, lineBreak);
            if (!normalizedMessage.EndsWith(lineBreak))
            {
                normalizedMessage += lineBreak;
            }
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(normalizedMessage);
            stream.Write(msg, 0, msg.Length);
        }

        public void ConnectionLoop()
        {
            byte[] bytes = new byte[1024];

            try
            {
                NetworkStream stream = client.GetStream();

                this.WriteMessage(stream, string.Format(greetingFormat, nodeName));

                String unprocessedInput = "";

                // Loop to receive all the data sent by the client.
                while (client.Connected)
                {
                    int i = stream.Read(bytes, 0, bytes.Length);
                    if (i == 0)
                    {
                        break;
                    }
                    // Translate data bytes to a ASCII string.
                    unprocessedInput += System.Text.Encoding.UTF8.GetString(bytes, 0, i);

                    Match lineBreakMatch = lineBreakPattern.Match(unprocessedInput);
                    while (lineBreakMatch.Success)
                    {
                        String line = unprocessedInput.Substring(0, lineBreakMatch.Index);
                        unprocessedInput = unprocessedInput.Substring(lineBreakMatch.Index + lineBreakMatch.Length);
                        lineBreakMatch = lineBreakPattern.Match(unprocessedInput);

                        string response = this.HandleCommandLine(line);
                        this.WriteMessage(stream, response);
                    }
                }
            }
            catch (IOException)
            {
                // assume connection closed
            }
            catch (Exception e)
            {
                Logger.Error("error handling connection", e);
            }

            // Shutdown and end connection
            client.Close();
        }

        protected string HandleCommandLine(string commandline)
        {
            if (commandline == null)
            {
                return "";
            }
            string[] argv = commandline.Split(new string[] { " ", "\t", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (argv.Length < 1)
            {
                return "";
            }
            // commands: list, nodes, config, fetch, version or quit
            switch (argv[0].ToLowerInvariant())
            {
                case "quit":
                    throw new IOException("closing connection");
                case "version":
                    return this.GetVersion();
                case "fetch":
                    if (argv.Length < 2)
                    {
                        return unknownService;
                    }
                    return this.FetchData(argv[1]);
                case "config":
                    if (argv.Length < 2)
                    {
                        return unknownService;
                    }
                    return this.ConfigData(argv[1]);
                case "nodes":
                    return string.Format("{0}\n.", nodeName);
                case "list":
                    return this.ListGraphs();
                default:
                    return "# Unknown command. Try list, nodes, config, fetch, version or quit";
            }
        }

        protected string GetVersion()
        {
            AssemblyFileVersionAttribute versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).FirstOrDefault() as AssemblyFileVersionAttribute;
            string version = (versionAttribute == null) ? "unkown" : versionAttribute.Version;
            return string.Format(versionFormat, nodeName, version);
        }

        protected string FetchData(string graphId)
        {
            if (string.IsNullOrEmpty(graphId))
            {
                return unknownService;
            }
            foreach (IMuninPlugin plugin in PluginHelper.loadedPlugins)
            {
                if (plugin.GetGraphs().Contains(graphId))
                {
                    return plugin.GetValues(graphId) + ".";
                }
            }
            return unknownService;
        }

        protected string ConfigData(string graphId)
        {
            if (string.IsNullOrEmpty(graphId))
            {
                return unknownService;
            }
            foreach (IMuninPlugin plugin in PluginHelper.loadedPlugins)
            {
                if (plugin.GetGraphs().Contains(graphId))
                {
                    return plugin.GetConfiguration(graphId) + ".";
                }
            }
            return unknownService;
        }

        protected string ListGraphs()
        {
            List<string> graphList = new List<string>();
            foreach (IMuninPlugin plugin in PluginHelper.loadedPlugins)
            {
                foreach (string graph in plugin.GetGraphs())
                {
                    if (!graphList.Contains(graph))
                    {
                        graphList.Add(graph);
                    }
                }
            }
            graphList.Sort();

            return string.Join(" ", graphList.ToArray());
        }
    }
}
