using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace USBAllClearTool
{
    public class diskpartfun
    {
        private static Process dcmd = new Process();

        public diskpartfun()
        {
            dcmd.StartInfo.UseShellExecute = false;
            dcmd.StartInfo.RedirectStandardOutput = true;
            dcmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            dcmd.StartInfo.CreateNoWindow = true;
            dcmd.StartInfo.FileName = @"C:\Windows\System32\diskpart.exe";
            dcmd.StartInfo.RedirectStandardInput = true;
        }
        public void F_Test()
        {
            dcmd.Start();
            dcmd.StandardInput.WriteLine("list disk");
            dcmd.StandardInput.WriteLine("list vol");
            dcmd.StandardInput.WriteLine("exit");
            string output = dcmd.StandardOutput.ReadToEnd();
            dcmd.WaitForExit();
            MessageBox.Show(output);
        }

        public void F_Reset_Disk(int DiskIndex, string Format_Type, string DiskType, bool quick_format)
        {
            dcmd.Start();
            dcmd.StandardInput.WriteLine("select disk {0}", DiskIndex);
            dcmd.StandardInput.WriteLine("clean");
            dcmd.StandardInput.WriteLine("convert {0}", DiskType);
            dcmd.StandardInput.WriteLine("create partition primary");
            if(quick_format)
            {
                dcmd.StandardInput.WriteLine("format fs={0} quick",Format_Type);
            }
            else
            {
                dcmd.StandardInput.WriteLine("format fs={0}",Format_Type);
            }
            dcmd.StandardInput.WriteLine("exit");
            string output = dcmd.StandardOutput.ReadToEnd();
            MessageBox.Show(output);
            dcmd.WaitForExit();
        }
    }
}
