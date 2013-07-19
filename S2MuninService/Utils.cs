using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using S2.Munin.Plugin;

namespace S2.Munin.Service
{
    public abstract class Utils
    {
        public static void CheckIniFile()
        {
            try
            {
                FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
                string settingsPath = Path.Combine(file.Directory.FullName, Constants.IniFileName);
                string defaultSettingsPath = Path.Combine(file.Directory.FullName, Constants.IniDefaultFileName);

                if (!File.Exists(settingsPath) && File.Exists(defaultSettingsPath))
                {
                    File.Copy(defaultSettingsPath, settingsPath);
                }
            }
            catch (Exception e)
            {
                Logger.Error("error checking settings", e);
            }
        }
    }
}
