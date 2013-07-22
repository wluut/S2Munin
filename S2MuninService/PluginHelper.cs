using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using S2.Munin.Plugin;

namespace S2.Munin.Service
{
    public static class PluginHelper
    {
        private static List<IMuninPlugin> loadedPlugins = new List<IMuninPlugin>();
        public static IList<IMuninPlugin> LoadedPlugins { get { return loadedPlugins.AsReadOnly(); } }

        public static void LoadPlugins()
        {
            FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);

            string pluginDirectoryPath = Path.Combine(file.Directory.FullName, Constants.PluginDirectory);

#if DEBUG
            var plugins = file.Directory.GetFiles(Constants.DllPattern, SearchOption.AllDirectories);
#else
            FileInfo[] plugins =
                           Directory.Exists(pluginDirectoryPath)
                               ? new DirectoryInfo(pluginDirectoryPath).GetFiles(Constants.DllPattern)
                               : new FileInfo[] { };
#endif

            foreach (FileInfo pluginFile in plugins)
            {
                Type[] types;
                try
                {
                    types = Assembly.LoadFile(pluginFile.FullName).GetTypes();
                }
                catch (IOException ioe)
                {
                    Logger.Instance.Error("could not load plugin " + pluginFile.Name, ioe);
                    continue;
                }
                catch (BadImageFormatException bife)
                {
                    Logger.Instance.Error("could not load plugin " + pluginFile.Name, bife);
                    continue;
                }

                foreach (Type type in types)
                {
                    if (!typeof(IMuninPlugin).IsAssignableFrom(type) || type.IsInterface)
                    {
                        // type not matching or sub-interface
                        continue;
                    }

                    try
                    {
                        IMuninPlugin plugin = Activator.CreateInstance(type) as IMuninPlugin;

                        plugin.Logger = new Logger(plugin.PluginName);

                        IDictionary<string, string> pluginSettings = GetSettings(plugin.PluginName);
                        if (plugin.Initialize(pluginSettings))
                        {
                            loadedPlugins.Add(plugin);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Error("error loading plugin " + type.FullName + " from " + pluginFile.Name, e);
                    }
                }
            }
        }

        static IDictionary<string, string> GetSettings(string pluginName)
        {
            IDictionary<string, string> settings = new Dictionary<string, string>();

            try
            {
                FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
                string settingsPath = Path.Combine(file.Directory.FullName, Constants.IniFileName);

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
            }
            catch (Exception e)
            {
                Logger.Instance.Error("error reading settings", e);
            }
            return settings;
        }
    }
}
