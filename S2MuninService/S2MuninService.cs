using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
