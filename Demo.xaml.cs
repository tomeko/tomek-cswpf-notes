using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using tomek_cswpf_notes.extensions;
using tomek_cswpf_notes.memoryops;

namespace tomek_cswpf_notes
{
    public enum E_ENUMBINDING { OPTION1, OPTION2, OPTION3, OPTION4 }

    public partial class Demo : Window, INotifyPropertyChanged
    {
        private E_ENUMBINDING _EnumBindingSelected = E_ENUMBINDING.OPTION1;
        public E_ENUMBINDING EnumBindingSelected { get { return _EnumBindingSelected; } set { _EnumBindingSelected = value; NotifyPropertyChanged(); } }

        private Point _WindowSize = new Point();
        public Point WindowSize { get { return _WindowSize; } set { _WindowSize = value; NotifyPropertyChanged(); } }

        private bool _MemcpyRunning = false;
        public bool MemcpyRunning { get { return _MemcpyRunning; } set { _MemcpyRunning = value; NotifyPropertyChanged(); } }

        public Demo()
        {
            InitializeComponent();
            Loaded += Demo_Loaded;
            MouseDown += Demo_MouseDown;
            SizeChanged += Demo_SizeChanged;
            Closing += Demo_Closing;
        }

        private void Demo_Closing(object sender, CancelEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Demo_Loaded(object sender, RoutedEventArgs e)
        {
            buildDynamicControl();
        }

        private void buildDynamicControl()
        {
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock tb_width = new TextBlock();
            tb_width.SetBinding(TextBlock.TextProperty, new Binding("ctx_this.WindowSize"));
            sp.Children.Add(new TextBlock() { Text = "WindowDim:", Margin = new Thickness(10, 0, 10, 0) });
            sp.Children.Add(tb_width);
            grdDynamicControl.Children.Add(sp);
        }

        private void Demo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }
        }

        private void gbDragDropFiles_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                MessageBox.Show(string.Join(System.Environment.NewLine, files));
            }
        }

        private unsafe void testmemcpy_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                string result = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Cursor = Cursors.Wait;
                    MemcpyRunning = true;
                });

                try
                {
                    int LARGE_SZ_MB = 256;
                    int SMALL_SZ_MB = 8;

                    Random rnd = new Random();
                    Stopwatch sw = new Stopwatch();

                    uint sz_large = (uint)(Math.Pow(1024, 2) * LARGE_SZ_MB);
                    uint sz_small = (uint)(Math.Pow(1024, 2) * SMALL_SZ_MB);
                    byte[] src_large = new byte[sz_large];
                    byte[] dst_large = new byte[sz_large];
                    byte[] src_small = new byte[sz_small];
                    byte[] dst_small = new byte[sz_small];

                    rnd.NextBytes(src_large);
                    rnd.NextBytes(src_small);

                    string sz_smallstr = $"{sz_small.ToSize(FileExtensions.SizeUnits.MB)} MB";
                    string sz_largestr = $"{sz_large.ToSize(FileExtensions.SizeUnits.MB)} MB";
                    StringBuilder res = new StringBuilder($"Memory Copy tests, 64bit: {Environment.Is64BitProcess}{Environment.NewLine}{Environment.NewLine}");

                    fixed (byte* srcp_small = src_small, dstp_small = dst_small, srcp_large = src_large, dstp_large = dst_large)
                    {
                        sw.Start();
                        memops.memcpy((IntPtr)dstp_small, (IntPtr)srcp_small, sz_small);
                        res.Append($"{sz_smallstr} RTL memcpy: {sw.ElapsedMilliseconds}ms{Environment.NewLine}");
                        sw.Restart();
                        memops.memcpy((IntPtr)dstp_small, (IntPtr)srcp_small, sz_small);
                        res.Append($"{sz_smallstr} cpblk memcpy: {sw.ElapsedMilliseconds}ms{Environment.NewLine}{Environment.NewLine}");
                        sw.Restart();
                        memops.memcpy((IntPtr)dstp_large, (IntPtr)srcp_large, sz_large);
                        res.Append($"{sz_largestr} RTL memcpy: {sw.ElapsedMilliseconds}ms{Environment.NewLine}");
                        sw.Restart();
                        memops.memcpyblk((IntPtr)dstp_large, (IntPtr)srcp_large, sz_large);
                        res.Append($"{sz_largestr} cpblk memcpy: {sw.ElapsedMilliseconds}ms{Environment.NewLine}{Environment.NewLine}");
                    }
                    sw.Restart();
                    Buffer.BlockCopy(src_small, 0, dst_small, 0, (int)sz_small);
                    res.Append($"{sz_smallstr} BufferBlockCopy: {sw.ElapsedMilliseconds}ms{Environment.NewLine}");
                    sw.Restart();
                    Buffer.BlockCopy(src_large, 0, dst_large, 0, (int)sz_large);
                    res.Append($"{sz_largestr} BufferBlockCopy: {sw.ElapsedMilliseconds}ms{Environment.NewLine}");
                    sw.Stop();
                    result = res.ToString();
                }
                catch (Exception ex)
                {
                    result = ex.Message;

                }
                finally
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Cursor = null;
                        MemcpyRunning = false;
                        MessageBox.Show(result);
                    });
                }

            }).Start();

        }

        private void Demo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowSizeDisplay();
        }

        private void UpdateWindowSizeDisplay()
        {
            WindowSize = new Point(Width, Height);

            // Alternatively, the below would do the same, by explicitly calling the PropertyChanged event for the WindowSize object
            //_WindowSize.X = Width;
            //_WindowSize.Y = Height;
            //NotifyPropertyChanged("WindowSize");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void CustomDialog_Click(object sender, RoutedEventArgs e)
        {
            var res = dialogs.MessageDialog.Show("Continue?", "Custom Dialog", hascancel: true);
            dialogs.MessageDialog.Show($"Dialog return val: {res}");
        }
    }

    public class DemoUserControlColorConverter : IValueConverter
    {
        readonly Dictionary<E_ENUMBINDING, Color> dictEnumBindingColors = new Dictionary<E_ENUMBINDING, Color>()
        {
            { E_ENUMBINDING.OPTION1, Colors.LightBlue },
            { E_ENUMBINDING.OPTION2, Colors.LightGreen },
            { E_ENUMBINDING.OPTION3, Colors.LightYellow },
            { E_ENUMBINDING.OPTION4, Colors.LightPink },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is E_ENUMBINDING))
                throw new ArgumentException();
            return new SolidColorBrush(dictEnumBindingColors[(E_ENUMBINDING)value]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RandomClass : INotifyPropertyChanged
    {

        private int _Val1;
        public int Val1 { get { return _Val1; } set { _Val1 = value; NotifyPropertyChanged(); } }

        private string _Label1;
        public string Label1 { get { return _Label1; } set { _Label1 = value; NotifyPropertyChanged(); } }
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
