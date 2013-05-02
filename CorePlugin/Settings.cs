using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace S2.Munin.Plugins.Core
{
    public class Settings
    {
        public enum DiskSpaceDisplayType { FREE, USED }
        public enum CpuDisplayType { BOTH, TOTAL, SINGLE }

        public bool NetstatLogarithmic { get; private set; }
        public bool ProcessesLogarithmic { get; private set; }
        public IList<string> NetstatCategories { get; private set; }
        public DiskSpaceDisplayType DiskSpaceDisplay { get; private set; }
        public CpuDisplayType CpuDisplay { get; private set; }
        public IList<string> DisabledNetworkInterfaces { get; private set; }

        public bool DisableCpu { get; private set; }
        public bool DisableDiskIO { get; private set; }
        public bool DisableDiskSpace { get; private set; }
        public bool DisableMemory { get; private set; }
        public bool DisableNetstat { get; private set; }
        public bool DisableNetworkIO { get; private set; }
        public bool DisableNetworkErrors { get; private set; }
        public bool DisableProcesses { get; private set; }

        public Settings(IDictionary<string, string> settings)
        {
            this.NetstatLogarithmic = true;
            this.ProcessesLogarithmic = true;
            this.NetstatCategories = null;
            this.DiskSpaceDisplay = DiskSpaceDisplayType.USED;
            this.CpuDisplay = CpuDisplayType.BOTH;
            this.DisabledNetworkInterfaces = new List<string>();

            this.DisableCpu = false;
            this.DisableDiskIO = false;
            this.DisableDiskSpace = false;
            this.DisableMemory = false;
            this.DisableNetstat = false;
            this.DisableNetworkIO = false;
            this.DisableNetworkErrors = false;
            this.DisableProcesses = false;

            if (settings == null)
            {
                return;
            }

            // graph settings
            #region Netstat
            if (settings.ContainsKey("netstat-logarithmic"))
            {
                bool netstatLogarithmic;
                if (bool.TryParse(settings["netstat-logarithmic"], out netstatLogarithmic))
                {
                    this.NetstatLogarithmic = netstatLogarithmic;
                }
            }
            if (settings.ContainsKey("netstat-categories"))
            {
                foreach (string category in Regex.Split(settings["netstat-categories"], "\\s*,\\s*"))
                {
                    if (!string.IsNullOrEmpty(category))
                    {
                        this.DisabledNetworkInterfaces.Add(category);
                    }
                }
            }
            #endregion
            #region Processes
            if (settings.ContainsKey("processes-logarithmic"))
            {
                bool processesLogarithmic;
                if (bool.TryParse(settings["processes-logarithmic"], out processesLogarithmic))
                {
                    this.ProcessesLogarithmic = processesLogarithmic;
                }
            }
            #endregion
            #region Disk
            if (settings.ContainsKey("display-space") && (string.Compare("free", settings["display-space"], StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.DiskSpaceDisplay = DiskSpaceDisplayType.FREE;
            }
            #endregion
            #region Cpu
            if (settings.ContainsKey("display-cpu"))
            {
                if (string.Compare("total", settings["display-cpu"], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.CpuDisplay = CpuDisplayType.TOTAL;
                }
                else if (string.Compare("single", settings["display-cpu"], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.CpuDisplay = CpuDisplayType.SINGLE;
                }
            }
            #endregion
            #region Network
            if (settings.ContainsKey("network-inferfaces-disabled"))
            {
                List<string> disabledNetworkInterfaces = new List<string>();
                foreach (string category in Regex.Split(settings["network-inferfaces-disabled"], "\\s*,\\s*"))
                {
                    if (!string.IsNullOrEmpty(category))
                    {
                        this.DisabledNetworkInterfaces.Add(category.ToLowerInvariant());
                    }
                }
                this.DisabledNetworkInterfaces = disabledNetworkInterfaces;
            }
            #endregion


            #region Disable
            // disable flags for single graphs
            if (settings.ContainsKey("disable-cpu"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-cpu"], out disable))
                {
                    this.DisableCpu = disable;
                }
            }
            if (settings.ContainsKey("disable-disk-io"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-disk-io"], out disable))
                {
                    this.DisableDiskIO = disable;
                }
            }
            if (settings.ContainsKey("disable-disk-space"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-disk-space"], out disable))
                {
                    this.DisableDiskSpace = disable;
                }
            }
            if (settings.ContainsKey("disable-memory"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-memory"], out disable))
                {
                    this.DisableMemory = disable;
                }
            }
            if (settings.ContainsKey("disable-netstat"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-netstat"], out disable))
                {
                    this.DisableNetstat = disable;
                }
            }
            if (settings.ContainsKey("disable-network-io"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-network-io"], out disable))
                {
                    this.DisableNetworkIO = disable;
                }
            }
            if (settings.ContainsKey("disable-network-errors"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-network-errors"], out disable))
                {
                    this.DisableNetworkErrors = disable;
                }
            }
            if (settings.ContainsKey("disable-processes"))
            {
                bool disable;
                if (bool.TryParse(settings["disable-processes"], out disable))
                {
                    this.DisableProcesses = disable;
                }
            }
            #endregion

        }
    }
}
