using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;

namespace PanicCall
{
    class ProcessorManager
    {
        public static void Killer()
        {
            Process[] ProcessList = Process.GetProcesses();

            foreach (Process pro in ProcessList)
            {
                string ProcessName = pro.ProcessName;
                if (ProcessName == "iexplore" || ProcessName == "taskmgr")
                {
                    try
                    {
                        //pro.Kill();
                    }
                    catch
                    { }
                }

               
            }
        }

        public static void ProcessorKill(string _ProcessorName)
        {
            Process[] ProcessList = Process.GetProcesses();

            foreach (Process pro in ProcessList)
            {
                if(pro.ProcessName.Equals(_ProcessorName))
                {
                    pro.Kill();
                }
            }
        }
    }
}
