using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string errorMessage = "# error, try again later\n.";
        private const string lineBreak = "\n";
        private static readonly Regex lineBreakPattern = new Regex("\\r?\\n\\r?");

        protected static string Version
        {
            get
            {
                AssemblyFileVersionAttribute versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).FirstOrDefault() as AssemblyFileVersionAttribute;
                string version = (versionAttribute == null) ? "unkown" : versionAttribute.Version;
                return string.Format(CultureInfo.InvariantCulture, versionFormat, nodeName, version);
            }
        }

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

        protected static void WriteMessage(Stream stream, string message)
        {
            if ((stream == null) || (message == null))
            {
                return;
            }
            // normalize message
            string normalizedMessage = lineBreakPattern.Replace(message, lineBreak);
            if (!normalizedMessage.EndsWith(lineBreak, StringComparison.OrdinalIgnoreCase))
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

                WriteMessage(stream, string.Format(CultureInfo.InvariantCulture, greetingFormat, nodeName));

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

                        string response = HandleCommandLine(line);
                        WriteMessage(stream, response);
                    }
                }
            }
            catch (IOException)
            {
                // assume connection closed
            }
            catch (Exception e)
            {
                Logger.Instance.Error("error handling connection", e);
            }

            // Shutdown and end connection
            client.Close();
        }

        protected static string HandleCommandLine(string commandline)
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
            switch (argv[0].ToUpperInvariant())
            {
                case "QUIT":
                    throw new IOException("closing connection");
                case "VERSION":
                    return Version;
                case "FETCH":
                    if (argv.Length < 2)
                    {
                        return unknownService;
                    }
                    try
                    {
                        return FetchData(argv[1]);
                    }
                    catch (Exception)
                    {
                        return errorMessage;
                    }
                case "CONFIG":
                    if (argv.Length < 2)
                    {
                        return unknownService;
                    }
                    try
                    {
                        return ConfigData(argv[1]);
                    }
                    catch (Exception)
                    {
                        return errorMessage;
                    }
                case "NODES":
                    return string.Format(CultureInfo.InvariantCulture, "{0}\n.", nodeName);
                case "LIST":
                    try
                    {
                        return ListGraphs();
                    }
                    catch (Exception)
                    {
                        return errorMessage;
                    }
                default:
                    return "# Unknown command. Try list, nodes, config, fetch, version or quit";
            }
        }

        protected static string FetchData(string graphId)
        {
            if (string.IsNullOrEmpty(graphId))
            {
                return unknownService;
            }
            foreach (IMuninPlugin plugin in PluginHelper.LoadedPlugins)
            {
                if (plugin.GetGraphs().Contains(graphId))
                {
                    return plugin.GetValues(graphId) + ".";
                }
            }
            return unknownService;
        }

        protected static string ConfigData(string graphId)
        {
            if (string.IsNullOrEmpty(graphId))
            {
                return unknownService;
            }
            foreach (IMuninPlugin plugin in PluginHelper.LoadedPlugins)
            {
                if (plugin.GetGraphs().Contains(graphId))
                {
                    return plugin.GetConfiguration(graphId) + ".";
                }
            }
            return unknownService;
        }

        protected static string ListGraphs()
        {
            List<string> graphList = new List<string>();
            foreach (IMuninPlugin plugin in PluginHelper.LoadedPlugins)
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
