using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;

namespace S2.Munin.Plugins.Core
{
    public class Memory
    {
        public Memory()
        {
        }

        public string GetConfiguration(string graphName)
        {
            StringBuilder configuration = new StringBuilder();

            configuration.Append("graph_title Memory usage\n");
            configuration.Append("graph_args -l 0 --base 1024\n");
            configuration.Append("graph_vlabel Byte\n");
            configuration.Append("graph_category system\n");
            configuration.Append("graph_order memory_used memory_free memory_swap\n");

            configuration.Append("memory_used.label used\n");
            configuration.Append("memory_used.draw AREA\n");
            configuration.Append("memory_used.info memory used\n");

            configuration.Append("memory_free.label free\n");
            configuration.Append("memory_free.draw STACK\n");
            configuration.Append("memory_free.info free memory\n");

            configuration.Append("memory_swap.label swap\n");
            configuration.Append("memory_swap.draw STACK\n");
            configuration.Append("memory_swap.info swap used\n");

            return configuration.ToString();
        }

        public string GetValues(string graphName)
        {
            ObjectQuery winQuery = new ObjectQuery("SELECT * FROM CIM_OperatingSystem");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(winQuery);

            StringBuilder values = new StringBuilder();

            foreach (ManagementObject result in searcher.Get())
            {
                object total = result["TotalVisibleMemorySize"];
                object free = result["FreePhysicalMemory"];
                object swap = result["TotalSwapSpaceSize"];

                object totalvirt = result["TotalVirtualMemorySize"];
                object freevirt = result["FreeVirtualMemory"];

                ulong totalK = 0;
                ulong freeK = 0;
                ulong swapK = 0;
                ulong totalVirtualK = 0;
                ulong freeVirtualK = 0;

                if (total != null)
                {
                    totalK = (ulong)total;
                }
                if (free != null)
                {
                    freeK = (ulong)free;
                }
                if (swap != null)
                {
                    swapK = (ulong)swap;
                }
                if (totalvirt != null)
                {
                    totalVirtualK = (ulong)totalvirt;
                }
                if (freevirt != null)
                {
                    freeVirtualK = (ulong)freevirt;
                }

                ulong used = totalK - freeK;
                ulong usedVirtual = (totalVirtualK > freeVirtualK) ? totalVirtualK - freeVirtualK : 0;
                ulong usedSwap = (usedVirtual > used) ? usedVirtual - used : 0;

                values.AppendFormat(CultureInfo.InvariantCulture, "memory_used.value {0}\n", used * 1024);
                values.AppendFormat(CultureInfo.InvariantCulture, "memory_free.value {0}\n", freeK * 1024);
                values.AppendFormat(CultureInfo.InvariantCulture, "memory_swap.value {0}\n", usedSwap * 1024);
            }

            return values.ToString();
        }


    }
}
