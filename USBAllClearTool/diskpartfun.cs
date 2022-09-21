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

        public void F_Reset_Disk(int DiskIndex, string Format_Type, string DiskType, bool quick_format, bool IsBigSize)
        {
            //bigsize is large divce (>32GB)
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

        public void F_Format_Volume(int DiskIndex, string Format_Type, string VolumeLetter, bool quick_format,bool IsBigSize)
        {
            //bigsize is large divce (>32GB)
            if(IsBigSize == true && Format_Type == "fat32")
            {
                F_Format_FAT32_Large(VolumeLetter);
                return;
            }
            dcmd.Start();
            dcmd.StandardInput.WriteLine("select disk {0}", DiskIndex);
            dcmd.StandardInput.WriteLine("select vol {0}",VolumeLetter);
            if (quick_format)
            {
                dcmd.StandardInput.WriteLine("format fs={0} quick", Format_Type);
            }
            else
            {
                dcmd.StandardInput.WriteLine("format fs={0}", Format_Type);
            }
            dcmd.StandardInput.WriteLine("exit");
            string output = dcmd.StandardOutput.ReadToEnd();
            MessageBox.Show(output);
            dcmd.WaitForExit();
        }

        private void F_Format_FAT32_Large(string VolumeLetter)
        {
            Process formatfat32 = new Process();
            formatfat32.StartInfo.UseShellExecute = false;
            formatfat32.StartInfo.RedirectStandardOutput = true;
            formatfat32.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            formatfat32.StartInfo.CreateNoWindow = true;
            formatfat32.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            formatfat32.StartInfo.RedirectStandardInput = true;
            formatfat32.Start();

            //start format
            //string startpath = System.AppDomain.CurrentDomain.BaseDirectory + "fat32format.exe ";
            formatfat32.StandardInput.WriteLine("fat32format.exe " + VolumeLetter + ":");
            formatfat32.StandardInput.WriteLine("y");
            formatfat32.StandardInput.Close();
            string output = formatfat32.StandardOutput.ReadToEnd();
            MessageBox.Show(output);
            formatfat32.Dispose();
            formatfat32.Close();
        }
    }
}
