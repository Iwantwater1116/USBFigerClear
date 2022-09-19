using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Diagnostics;

namespace USBAllClearTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private class DiskInfo
        {
            public string DiskSystemName { get; set; }
            public string DiskCaption { get; set; }
            public UInt64 Size { get; set; }
            public string diskindex { get; set; }
            public string disktype { get; set; }
        }

        //System Parameters
        private diskpartfun DiskPart = new diskpartfun();
        List<DiskInfo> disks = new List<DiskInfo>();
        List<string> disklist = new List<string>();

        //System Data
        string[] Disk_Type = { "MBR", "GPT" };
        string[] Format_Type = { "NTFS", "FAT32", "exFAT" };

        //Runtime Parameter
        int Select_DiskIndex;
        string FormatType = "ntfs";
        string Disktype = "mbr";
        bool Need_QuickFormat = false;
        bool Need_AllClean = false;

        public void GetDiskDevices()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"select * from Win32_DiskDrive"))
                collection = searcher.Get();

            foreach (var disk in collection)
            {
                DiskInfo adisk = new DiskInfo();
                adisk.DiskSystemName = disk.GetPropertyValue("Name").ToString();
                adisk.DiskCaption = disk.GetPropertyValue("Caption").ToString();
                adisk.Size = (UInt64)disk.GetPropertyValue("Size");
                adisk.diskindex = disk.GetPropertyValue("Index").ToString(); ;
                adisk.disktype = disk.GetPropertyValue("MediaType").ToString();
                disks.Add(adisk);
                disklist.Add(adisk.DiskCaption);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetDiskDevices();
            Cmb_Devices.ItemsSource = disklist;
            Cmb_Disk_Type.ItemsSource = Disk_Type;
            Cmb_Format_Type.ItemsSource = Format_Type;

            if(Rdb_JustFormat.IsChecked == true)
            {
                Cmb_Disk_Type.IsEnabled = false;
            }
            Cmb_Devices.SelectedIndex = 0;
            Cmb_Format_Type.SelectedIndex = 0;

            if (Ckb_QuickFormat.IsChecked == true)
            {
                Need_QuickFormat = true;
            }
            else
            {
                Need_QuickFormat = false;
            }
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            //Find USB Index
            foreach(DiskInfo adisk in disks)
            {
                if(adisk.DiskCaption == Cmb_Devices.SelectedItem.ToString())
                {
                    Select_DiskIndex = Convert.ToInt32(adisk.diskindex);
                }
            }

            //Get Other Parameter

            Disktype = Cmb_Disk_Type.SelectedItem.ToString().ToLower();
            FormatType = Cmb_Format_Type.SelectedItem.ToString().ToLower();

            //run diskpart script
            DiskPart.F_Reset_Disk(Select_DiskIndex, FormatType, Disktype, Need_QuickFormat);
        }

        private void Rdb_AllClear_Checked(object sender, RoutedEventArgs e)
        {
            if(Rdb_AllClear.IsChecked == true)
            {
                Cmb_Disk_Type.IsEnabled = true;
                Cmb_Disk_Type.SelectedIndex = 0;
                Need_AllClean = true;
            }
            else
            {
                Cmb_Disk_Type.IsEnabled = false;
                Cmb_Disk_Type.SelectedIndex = -1;
                Need_AllClean = false;
            }
        }

        private void Ckb_QuickFormat_Checked(object sender, RoutedEventArgs e)
        {
            if(Ckb_QuickFormat.IsChecked == true)
            {
                Need_QuickFormat = true;
            }
            else
            {
                Need_QuickFormat = false;
            }
        }
    }
}
