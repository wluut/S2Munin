using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

using S2.Munin.Plugin;

namespace S2.Munin.Service
{
    public partial class S2MuninService : ServiceBase
    {
        public S2MuninService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            int muninPort = 4949;
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
        }

        protected override void OnStop()
        {
            Listener.Instance.StopSocket();

            foreach (IMuninPlugin plugin in PluginHelper.loadedPlugins)
            {
                plugin.StopPlugin();
            }
        }
    }
}
