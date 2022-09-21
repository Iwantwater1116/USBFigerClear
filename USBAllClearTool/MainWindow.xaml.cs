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
            public string DeviceID { get; set; }
            public string DiskSystemName { get; set; }
            public string DiskCaption { get; set; }
            public UInt64 Size { get; set; }
            public string diskindex { get; set; }
            public string diskmodel { get; set; }
            public string disktype { get; set; }
            public string disklistname { get; set; }
        }

        private class VolumeInfo
        {
            public string DiskIndex { get; set; }
            public string PartitionIndex { get; set; }
            public string DiskSize { get; set; }
            public string DiskModel { get; set; }
            public string DriveLetter { get; set; }
            public string VolumeName { get; set; }
            public string VolumeListName { get; set; }
        }

        //System Parameters
        private diskpartfun DiskPart = new diskpartfun();
        List<DiskInfo> disks = new List<DiskInfo>();
        List<string> disklist = new List<string>();
        List<VolumeInfo> vols = new List<VolumeInfo>();
        List<string> vollist = new List<string>();
        //System Data
        string[] Disk_Type = { "MBR", "GPT" };
        string[] Format_Type = { "NTFS", "FAT32", "exFAT" };

        //Runtime Parameter
        int Select_DiskIndex;
        int Program_Loaded_Flag = 0;
        string FormatType = "ntfs";
        string Disktype = "mbr";
        bool Need_QuickFormat = false;
        bool Need_AllClean = false;

        private void GetDiskDevices()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"select * from Win32_DiskDrive"))
                collection = searcher.Get();

            foreach (var disk in collection)
            {
                DiskInfo adisk = new DiskInfo();
                adisk.DeviceID = disk.GetPropertyValue("DeviceID").ToString();
                adisk.DiskSystemName = disk.GetPropertyValue("Name").ToString();
                adisk.DiskCaption = disk.GetPropertyValue("Caption").ToString();
                adisk.Size = (UInt64)disk.GetPropertyValue("Size");
                adisk.diskindex = disk.GetPropertyValue("Index").ToString(); ;
                adisk.disktype = disk.GetPropertyValue("MediaType").ToString();
                adisk.diskmodel = disk.GetPropertyValue("Model").ToString();
                adisk.disklistname = adisk.DiskCaption + " (" + SizeConverter(Convert.ToDouble(adisk.Size)) + ")";
                disks.Add(adisk);
                disklist.Add(adisk.disklistname);
            }
        }

        private List<VolumeInfo> GetDiskVolumes(DiskInfo dinfo)
        {
            List<VolumeInfo> volumes = new List<VolumeInfo>();
            ManagementObjectCollection collection;
            string query = "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + dinfo.DeviceID + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
            using (var searcher = new ManagementObjectSearcher(query))
                collection = searcher.Get();

            foreach (var partition in collection)
            {
                //get disk indformation
                ManagementObjectCollection disk_collection;
                query = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition.GetPropertyValue("DeviceID") + "'} where AssocClass = Win32_LogicalDiskToPartition";
                using (var searcher = new ManagementObjectSearcher(query))
                    disk_collection = searcher.Get();

                foreach (var final in disk_collection)
                {
                    VolumeInfo avolume = new VolumeInfo();
                    avolume.DiskIndex = dinfo.diskindex;
                    avolume.PartitionIndex = partition.GetPropertyValue("Index").ToString();
                    avolume.DiskSize = dinfo.Size.ToString();
                    avolume.DiskModel = dinfo.diskmodel;
                    avolume.DriveLetter = final.GetPropertyValue("DeviceID").ToString();
                    avolume.VolumeName = final.GetPropertyValue("VolumeName").ToString();
                    string listname = "[" + avolume.DriveLetter + "]" + avolume.VolumeName;
                    avolume.VolumeListName = listname;
                    volumes.Add(avolume);
                }
            }
            return volumes;
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

            if(Rdb_JustFormat.IsChecked == true)
            {
                foreach(DiskInfo adisk in disks)
                {
                    if(adisk.disklistname == Cmb_Devices.SelectedItem.ToString())
                    {
                        var DiskVolume = GetDiskVolumes(adisk);
                        foreach(VolumeInfo volumelist in DiskVolume)
                        {
                            vollist.Add(volumelist.VolumeListName);
                        }
                        vols = DiskVolume;
                    }
                }
            }

            //Cmb_Volumelist.Items.Clear();
            Cmb_Volumelist.ItemsSource = vollist;
            Cmb_Volumelist.SelectedIndex = 0;
            Program_Loaded_Flag = 1;
        }

        private string SizeConverter(double bytes)
        {
            string newsize = "";
            double KB = 0;
            KB = bytes / 1024;
            newsize = KB.ToString("#0.00") + " KB";
            if (KB >= 1024)
            {
                double MB = 0;
                MB = KB / 1024;
                newsize = MB.ToString("#0.00") + " MB";
                if (MB >= 1024)
                {
                    double GB = 0;
                    GB = MB / 1024;
                    newsize = GB.ToString("#0.00") + " GB";
                    if (GB >= 1024)
                    {
                        double TB = 0;
                        TB = GB / 1024;
                        newsize = TB.ToString("#0.00") + " TB";
                    }
                }
            }

            return newsize;
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            //
            bool LargeDeviceFlag = false;
            //Find USB Index
            foreach(DiskInfo adisk in disks)
            {
                if(adisk.disklistname == Cmb_Devices.SelectedItem.ToString())
                {
                    Select_DiskIndex = Convert.ToInt32(adisk.diskindex);
                    if(adisk.Size > 32212254720)
                    {
                        LargeDeviceFlag = true;
                    }
                }
            }

            //Get Other Parameter

            if (Rdb_AllClear.IsChecked == true)
            {
                Disktype = Cmb_Disk_Type.SelectedItem.ToString().ToLower();
            }
            FormatType = Cmb_Format_Type.SelectedItem.ToString().ToLower();

            //run diskpart script

            if(Rdb_AllClear.IsChecked == true)
            {
                DiskPart.F_Reset_Disk(Select_DiskIndex, FormatType, Disktype, Need_QuickFormat,LargeDeviceFlag);
            }
            else
            {
                foreach(VolumeInfo vol in vols)
                {
                    if(vol.VolumeListName == Cmb_Volumelist.SelectedItem.ToString())
                    {
                        string VolumeLetter = vol.DriveLetter.Substring(0, 1);
                        DiskPart.F_Format_Volume(Select_DiskIndex, FormatType, VolumeLetter, Need_QuickFormat, LargeDeviceFlag);
                    }
                }
            }
        }

        private void Rdb_AllClear_Checked(object sender, RoutedEventArgs e)
        {
            if(Rdb_AllClear.IsChecked == true)
            {
                Need_AllClean = true;
                Cmb_Disk_Type.IsEnabled = true;
                Cmb_Disk_Type.SelectedIndex = 0;
                Need_AllClean = true;
                Lbl_Volume.Visibility = Visibility.Hidden;
                Cmb_Volumelist.Visibility = Visibility.Hidden;
                Cmb_Volumelist.SelectedIndex = -1;
                Gbx_ClrOpt.Height = 50;
                Gbx_QuickFormat.Margin = new Thickness(20, 285, 20, 0);
                this.Height = 465;
            }
            else
            {
                Need_AllClean = false;
                Cmb_Disk_Type.IsEnabled = false;
                Cmb_Disk_Type.SelectedIndex = -1;
                Need_AllClean = false;
                Lbl_Volume.Visibility = Visibility.Visible;
                Cmb_Volumelist.Visibility = Visibility.Visible;
                Gbx_ClrOpt.Height = 100;
                Gbx_QuickFormat.Margin = new Thickness(20, 335, 20, 0);
                this.Height = 515;
                if(Program_Loaded_Flag == 1)
                {
                    vollist = new List<string>();
                    vols = new List<VolumeInfo>();
                    foreach (DiskInfo adisk in disks)
                    {
                        if (adisk.disklistname == Cmb_Devices.SelectedItem.ToString())
                        {
                            var DiskVolume = GetDiskVolumes(adisk);
                            foreach (VolumeInfo volumelist in DiskVolume)
                            {
                                vollist.Add(volumelist.VolumeListName);
                            }
                            vols = DiskVolume;
                        }
                    }
                }
                Cmb_Volumelist.ItemsSource = vollist;
                Cmb_Volumelist.SelectedIndex = 0;
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

        private void Cmb_Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Rdb_JustFormat.IsChecked == true)
            {
                if (Program_Loaded_Flag == 1)
                {
                    vollist = new List<string>();
                    vols = new List<VolumeInfo>();
                    foreach (DiskInfo adisk in disks)
                    {
                        if (adisk.disklistname == Cmb_Devices.SelectedItem.ToString())
                        {
                            var DiskVolume = GetDiskVolumes(adisk);
                            foreach (VolumeInfo volumelist in DiskVolume)
                            {
                                vollist.Add(volumelist.VolumeListName);
                            }
                            vols = DiskVolume;
                        }
                    }
                }
                Cmb_Volumelist.ItemsSource = vollist;
                Cmb_Volumelist.SelectedIndex = 0;
            }
        }
    }
}
