﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using S2.Munin.Plugin;

namespace S2.Munin.Service
{
    public static class PluginHelper
    {
        public static readonly IList<IMuninPlugin> loadedPlugins = new List<IMuninPlugin>();

        public static void LoadPlugins()
        {
            FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);

            string pluginDirectoryPath = Path.Combine(file.Directory.FullName, "Plugins");

#if DEBUG
            var plugins = file.Directory.GetFiles("*.dll", SearchOption.AllDirectories);
#else
            FileInfo[] plugins =
                           Directory.Exists(pluginDirectoryPath)
                               ? new DirectoryInfo(pluginDirectoryPath).GetFiles("*.dll")
                               : new FileInfo[] { };
#endif

            foreach (FileInfo pluginFile in plugins)
            {
                Type[] types = Assembly.LoadFile(pluginFile.FullName).GetTypes();

                foreach (Type type in types)
                {
                    if (!typeof(IMuninPlugin).IsAssignableFrom(type) || type.IsInterface)
                    {
                        // type not matching or sub-interface
                        continue;
                    }

                    IMuninPlugin plugin = Activator.CreateInstance(type) as IMuninPlugin;

                    IDictionary<string, string> pluginSettings = GetSettings(plugin.PluginName);
                    if (plugin.Initialize(pluginSettings))
                    {
                        loadedPlugins.Add(plugin);
                    }
                }
            }
        }

        static IDictionary<string, string> GetSettings(string pluginName)
        {
            IDictionary<string, string> settings = new Dictionary<string, string>();

            FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string settingsPath = Path.Combine(file.Directory.FullName, "S2Munin.ini");

            if (!File.Exists(settingsPath))
            {
                return settings;
            }
            INI.IniFileName ini = new INI.IniFileName(settingsPath);

            string[] entries = ini.GetEntryNames(pluginName);
            if (entries != null)
            {
                foreach (string entry in entries)
                {
                    string value = ini.GetEntryValue(pluginName, entry) as string;
                    settings.Add(entry, value);
                }
            }
            return settings;
        }
    }
}