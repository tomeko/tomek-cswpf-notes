using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace tomek_cswpf_notes.extensions
{
    public static class GenericExtensions
    {
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }

    public static class FileExtensions
    {
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static long ToSize(this long value, SizeUnits unit)
        {
            return (long)(value / (double)Math.Pow(1024, (int)unit));
        }

        public static int ToSize(this int value, SizeUnits unit)
        {
            return (int)(value / (double)Math.Pow(1024, (int)unit));
        }

        public static int ToSize(this uint value, SizeUnits unit)
        {
            return (int)(value / (double)Math.Pow(1024, (int)unit));
        }

        public static void DirectoryCopy(string sourceDirPath, string destDirName, bool copysubdirs)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirPath);
            DirectoryInfo[] directories = directoryInfo.GetDirectories();
            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: "
                    + sourceDirPath);
            }
            DirectoryInfo parentDirectory = Directory.GetParent(directoryInfo.FullName);
            destDirName = System.IO.Path.Combine(parentDirectory.FullName, destDirName);

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            FileInfo[] files = directoryInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                string tempPath = System.IO.Path.Combine(destDirName, file.Name);
                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                file.CopyTo(tempPath, false);
            }
            if (copysubdirs)
            {
                foreach (DirectoryInfo item in directories)
                {
                    string tempPath = System.IO.Path.Combine(destDirName, item.Name);
                    DirectoryCopy(item.FullName, tempPath, copysubdirs);
                }
            }
        }
    }

    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (!string.IsNullOrEmpty(file.Name))
                    file.ExtractToFile(completeFileName, true);
            }
        }
    }

    public static class ColorExtensions
    {
        public static SolidColorBrush ToBrush(this Color color)
        {
            return new SolidColorBrush(color);
        }

        public static System.Windows.Media.Color WavelengthToColor(double Wavelength)
        {
            double factor;
            double Red, Green, Blue;
            double Gamma = 0.8;
            int IntensityMax = 255;

            if ((Wavelength >= 380) && (Wavelength < 440))
            {
                Red = -(Wavelength - 440) / (440 - 380);
                Green = 0.0;
                Blue = 1.0;
            }
            else if ((Wavelength >= 440) && (Wavelength < 490))
            {
                Red = 0.0;
                Green = (Wavelength - 440) / (490 - 440);
                Blue = 1.0;
            }
            else if ((Wavelength >= 490) && (Wavelength < 510))
            {
                Red = 0.0;
                Green = 1.0;
                Blue = -(Wavelength - 510) / (510 - 490);
            }
            else if ((Wavelength >= 510) && (Wavelength < 580))
            {
                Red = (Wavelength - 510) / (580 - 510);
                Green = 1.0;
                Blue = 0.0;
            }
            else if ((Wavelength >= 580) && (Wavelength < 645))
            {
                Red = 1.0;
                Green = -(Wavelength - 645) / (645 - 580);
                Blue = 0.0;
            }
            else if ((Wavelength >= 645) && (Wavelength < 781))
            {
                Red = 1.0;
                Green = 0.0;
                Blue = 0.0;
            }
            else
            {
                Red = 0.0;
                Green = 0.0;
                Blue = 0.0;
            };

            if ((Wavelength >= 380) && (Wavelength < 420))
                factor = 0.3 + 0.7 * (Wavelength - 380) / (420 - 380);
            else if ((Wavelength >= 420) && (Wavelength < 701))
                factor = 1.0;
            else if ((Wavelength >= 701) && (Wavelength < 781))
                factor = 0.3 + 0.7 * (780 - Wavelength) / (780 - 700);
            else
                factor = 0.0;

            int[] rgb = new int[3];

            // Don't want 0^x = 1 for x <> 0
            rgb[0] = Red == 0.0 ? 0 : (int)Math.Round(IntensityMax * Math.Pow(Red * factor, Gamma));
            rgb[1] = Green == 0.0 ? 0 : (int)Math.Round(IntensityMax * Math.Pow(Green * factor, Gamma));
            rgb[2] = Blue == 0.0 ? 0 : (int)Math.Round(IntensityMax * Math.Pow(Blue * factor, Gamma));

            System.Windows.Media.Color colres = System.Windows.Media.Color.FromRgb((byte)rgb[0], (byte)rgb[1], (byte)rgb[2]);
            //System.Drawing.Color colres = System.Drawing.Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            return colres;
        }
    }

    public static class ArrayExtensions
    {
        public static unsafe double[] Flatten(this double[,] arr)
        {
            int w = arr.GetLength(1), h = arr.GetLength(0);
            uint tot = (uint)(w * h);
            double[] ret = new double[tot];
            fixed (double* arrp = arr, retp = ret)
            {
                for (int i = 0; i < tot; i++)
                    retp[i] = arrp[i];
            }
            return ret;
        }

        public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0)).Select(x => matrix[x, columnNumber]).ToArray();
        }

        public static T[] GetRow<T>(this T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[rowNumber, x]).ToArray();
        }

        public static List<T> GetRowList<T>(this T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[rowNumber, x]).ToList();
        }

        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }
    }

    public static class ProgressBarExtensions
    {
        private static TimeSpan _Duration = TimeSpan.FromMilliseconds(500);
        public static TimeSpan Duration { get { return _Duration; } set { _Duration = value; } }

        public static async Task SetPercent(this ProgressBar progressBar, double percentage)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, _Duration);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
            await Task.Delay(_Duration);

        }
    }

    public static class ImageExtensions
    {
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            using (var handle = new SafeHBitmapHandle(source))
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle.DangerousGetHandle(),
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private sealed class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            public SafeHBitmapHandle(System.Drawing.Bitmap bitmap)
                : base(true)
            {
                SetHandle(bitmap.GetHbitmap());
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            protected override bool ReleaseHandle()
            {
                return DeleteObject(handle) > 0;
            }
        }

        public static BitmapImage ResizeImageJpg(string inputPath, string outputPath, int width, int height, bool preserve_aspect = true, bool freeze = true)
        {
            var bitmap = new BitmapImage();

            using (var stream = new FileStream(inputPath, FileMode.Open))
            {
                bitmap.BeginInit();
                bitmap.DecodePixelWidth = width;
                if (!preserve_aspect)
                    bitmap.DecodePixelHeight = height;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream(outputPath, FileMode.Create))
            {
                encoder.Save(stream);
            }
            if (freeze)
                bitmap.Freeze();
            return bitmap;

        }
    }

    public static class MiscExtensions
    {

        

        public static string Sha256_hash(this string value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(value)).Select(item => item.ToString("x2")));
            }
        }

        public static string Sha256_hash(this byte[] bytes)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Concat(hash.ComputeHash(bytes).Select(item => item.ToString("x2")));
            }
        }

        public static string[] RegexMatches(this string str, Regex regex)
        {
            try
            {
                return regex.Matches(str).Cast<Match>().Select(m => m.Value).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string[] RegexMatches(this string str, string pattern)
        {
            try
            {
                return Regex.Matches(str, pattern).Cast<Match>().Select(m => m.Value).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Valid(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }
    }

    public static class WindowsNETExtensions
    {
        public static bool CheckDriver(string drivername)
        {
            //System.Management.SelectQuery query = new System.Management.SelectQuery("Win32_SystemDriver");
            //query.Condition = $"Name = '{drivername}'";
            //ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            //var drivers = searcher.Get();
            //bool found = drivers.Count > 0;
            //return found;
            return false;
        }

        public static string GetPCModel()
        {
            //string pc = "";
            //ManagementClass mc = new ManagementClass("Win32_ComputerSystemProduct");
            //ManagementObjectCollection moc = mc.GetInstances();
            //foreach (ManagementObject mo in moc)
            //{
            //    pc = mo.Properties["Caption"].Value.ToString();
            //    break;
            //}
            //return pc;
            return "";
        }

        public static string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {

                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }
    }
}
