using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S2.Munin.Service
{
    public abstract class Constants
    {
        public const string IniFileName = "S2Munin.ini";
        public const string IniGlobalSection = "S2Munin";
        public const string IniPortKey = "port";
        public const string IniBindKey = "bind-address";
        public const string PluginDirectory = "Plugins";
        public const string DllPattern = "*.dll";
    }
}
