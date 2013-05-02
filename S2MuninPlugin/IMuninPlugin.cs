using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S2.Munin.Plugin
{
    public interface IMuninPlugin
    {
        /// <summary>
        /// Name of Plugin (used for configuration)
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Initialize Plugin with Settings
        /// </summary>
        /// <param name="settings">The Settings from the INI file</param>
        /// <returns>true if initialization was successful</returns>
        bool Initialize(IDictionary<string, string> settings);

        IList<string> GetGraphs();

        string GetConfiguration(string graphName);

        string GetValues(string graphName);

        void StopPlugin();
    }
}
