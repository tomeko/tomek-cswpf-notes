using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace tomek_cswpf_notes
{
    public static class fileops
    {
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

        public static string Zip(string zippedname, List<string> files, string toplevel_dir, string _dst_dir = null, bool delete_existing = true)
        {
            string zippedpath = null;
            try
            {
                string fdir = _dst_dir == null ? Path.GetDirectoryName(files.First()) : _dst_dir;

                string ext = zippedname.Contains(".zip") ? "" : ".zip";
                zippedpath = $"{fdir}\\{zippedname}{ext}";
                if (delete_existing && File.Exists(zippedpath))
                    File.Delete(zippedpath);
                var zip = ZipFile.Open(zippedpath, ZipArchiveMode.Create);
                foreach (var file in files)
                {
                    string fname = file.Replace(toplevel_dir, "");
                    fname = fname.TrimStart('\\');
                    zip.CreateEntryFromFile(file, fname, CompressionLevel.Optimal);
                }
                zip.Dispose();
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }

            return zippedpath;
        }

        public static List<string> OpenFiles(string filter = "(*.*)|*.*", string startdir = null)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Multiselect = true, Filter = filter };
            if (startdir != null && Directory.Exists(startdir))
                ofd.InitialDirectory = startdir;
            if (ofd.ShowDialog() == true)
                return ofd.FileNames.ToList();
            return null;
        }

        public static string OpenDirectoryNoWinforms(string startdir = null)
        {
            // no winform dependency workaround
            // taken from https://stackoverflow.com/a/50261723/843859

            // Create a "Save As" dialog for selecting a directory (HACK)
            var dialog = new Microsoft.Win32.SaveFileDialog();
            if (startdir != null && Directory.Exists(startdir))
                dialog.InitialDirectory = startdir; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                return path;
            }
            return null;
        }
    }

    
}
