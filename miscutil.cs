using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace tomek_cswpf_notes
{
    public static class miscutil
    {
        public static void ClipboardSetText(string txt)
        {
            var clipboardThread = new Thread(() => clipBoardThreadWorker(txt));
            clipboardThread.SetApartmentState(ApartmentState.STA);
            clipboardThread.IsBackground = false;
            clipboardThread.Start();
        }

        static void clipBoardThreadWorker(string inTextToCopy)
        {
            System.Windows.Clipboard.SetText(inTextToCopy);
        }

        public static void FakeWait(int delayms = 100)
        {
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await Task.Delay(delayms);
                Mouse.OverrideCursor = null;
            });
        }

        public static string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {

                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed && !string.IsNullOrEmpty(tempMac) && tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }


        public static class FastDateTime
        {
            public static TimeSpan LocalUtcOffset { get; private set; }
            public static DateTime Now
            {
                get { return DateTime.UtcNow + LocalUtcOffset; }
            }
            
            static FastDateTime()
            {
                LocalUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            }
        }

    }

    public class IniFile
    {
        Dictionary<string, string> Dat { get; set; }
        char Separator { get; set; }
        public string IniFilePath { get; private set; }
        public bool HasData { get { return Dat != null && Dat.Count > 0; } }
        public Dictionary<string, string> Defaults { get; private set; }

        public IniFile(string path, Dictionary<string, string> defaults = null, char seperator = '|')
        {
            IniFilePath = path;
            Separator = seperator;
            Dat = new Dictionary<string, string>();
            Defaults = defaults;
            if (File.Exists(IniFilePath))
                loadinidat();
            updatefile();
        }

        private void loadinidat()
        {
            var lines = File.ReadAllLines(IniFilePath);
            foreach (var line in lines)
            {
                var kvp = line.Split(Separator);
                if (kvp != null && kvp.Length == 2)
                {
                    if (Dat.ContainsKey(kvp[0]))
                        Dat[kvp[0]] = kvp[1];
                    else
                        Dat.Add(kvp[0], kvp[1]);
                }
            }
        }

        public void AddUpdateDat(string key, string value)
        {
            if (Dat.ContainsKey(key))
                Dat[key] = value;
            else
                Dat.Add(key, value);

            updatefile();
        }

        public string GetDat(string key)
        {
            if (!HasData || !Dat.ContainsKey(key))
                return null;
            return Dat[key];
        }

        public bool? GetDatBool(string key)
        {
            if (!HasData || !HasDat(key))
                return null;
            return Dat[key].ToLower() == "true" || Dat[key] == "1";
        }

        public bool HasDat(string key)
        {
            if (!HasData)
                return false;
            return Dat.ContainsKey(key);
        }

        public bool RemoveDat(string key)
        {
            if (Dat.ContainsKey(key))
                Dat.Remove(key);
            else
                return false;

            updatefile();

            return true;
        }

        void updatefile()
        {
            File.WriteAllText(IniFilePath, "");
            if (Dat.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in Dat)
                {
                    File.AppendAllText(IniFilePath, $"{kvp.Key}{Separator}{kvp.Value}{Environment.NewLine}");
                }
            }
            
            foreach (var kvp in Defaults)
            {
                if (!Dat.ContainsKey(kvp.Key))
                {
                    Dat.Add(kvp.Key, kvp.Value);
                    File.AppendAllText(IniFilePath, $"{kvp.Key}{Separator}{Defaults[kvp.Key]}{Environment.NewLine}");
                }
                    
            }

                
        }


    }
}
