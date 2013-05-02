using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using S2.Munin.Plugin;
using S2.Munin.Service;

namespace S2.Munin.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            int muninPort = 4950;
            string bindAddress = "";

            FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string settingsPath = Path.Combine(file.Directory.FullName, Constants.IniFileName);

            if (File.Exists(settingsPath))
            {
                INI.IniFileName ini = new INI.IniFileName(settingsPath);

                string iniPort = ini.GetEntryValue(Constants.IniGlobalSection, Constants.IniPortKey) as string;
                if (!string.IsNullOrEmpty(iniPort))
                {
                    if (!int.TryParse(iniPort, out muninPort))
                    {
                        Logger.ErrorFormat("could not parse port \"{0}\", defaulting to {1}", iniPort, muninPort);
                    }
                }
                string iniBindAdress = ini.GetEntryValue(Constants.IniGlobalSection, Constants.IniBindKey) as string;
                if (!string.IsNullOrEmpty(iniBindAdress))
                {
                    bindAddress = iniBindAdress;
                }
            }

            PluginHelper.LoadPlugins();

            Listener.Instance.SetupSocket(bindAddress, muninPort);

            bool keepRunning = true;

            while (keepRunning)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Listener.Instance.StopSocket();

            foreach (IMuninPlugin plugin in PluginHelper.LoadedPlugins)
            {
                plugin.StopPlugin();
            }
        }
    }
}
