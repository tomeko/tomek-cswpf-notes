using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace tomek_cswpf_notes
{



    public partial class App : Application
    {
        const bool SHOW_SPLASH = false;
        static readonly string logfile = "bob.log";

        public static Demo winDemo { get; private set; }
        public static DemoSplash winDemoSplash { get; private set; }
        public static App CurrentApp { get; private set; }
        public static IniFile inifile { get; private set; }

        readonly Dictionary<string, string> default_ini = new Dictionary<string, string>()
        {
            { "splash", "true" },
        };


        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            CurrentApp = this;
            winDemo = new Demo();
            initDataContexts();

            inifile = new IniFile("demo.ini", default_ini);

            if (inifile.GetDatBool("splash") == true)
            {
                winDemoSplash = new DemoSplash("DemoSplash1.0");
                winDemoSplash.Show();

                // Below showing two ways of updating the progress bar in DemoSplash window
                for (int i = 0; i < 10; i++)
                {

                    if (i < 5)
                    {
                        // First half: Directly updates the Progress of the progressbar, you'll notice the first half animation is choppy
                        await Task.Delay(100);
                        winDemoSplash.Progress = i * 10;
                    }
                    else
                    {
                        // Second half: Calls a function in winDemoSplash that uses a animation smoothing progress bar extension, notice the smooth animation
                        await winDemoSplash.UpdateProgressSmooth(i * 10);
                    }

                }

                winDemoSplash.Close();
            }

            winDemo.Show();
        }

        private void initDataContexts()
        {
            winDemo.DataContext = new
            {
                ctx_this = winDemo,
            };
        }

        public static void Log(string msg)
        {
            File.AppendAllText(logfile, $"{DateTime.Now}: {msg}{Environment.NewLine}");
        }

        public static void Log(Exception ex, [CallerMemberName] string callername = "")
        {
            File.AppendAllText(logfile, $"{DateTime.Now}: {callername}|{ex.Message}{Environment.NewLine}");
            File.AppendAllText(logfile, $"{ex.StackTrace}{Environment.NewLine}");
        }


    }
}
