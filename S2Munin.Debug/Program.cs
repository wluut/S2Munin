using System;
using System.Collections.Generic;
using System.Linq;
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

            PluginHelper.LoadPlugins();

            Listener.Instance.SetupSocket(bindAddress, muninPort);

            bool keepRunning = true;

            while (keepRunning)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Listener.Instance.StopSocket();

            foreach (IMuninPlugin plugin in PluginHelper.loadedPlugins)
            {
                plugin.StopPlugin();
            }
        }
    }
}
