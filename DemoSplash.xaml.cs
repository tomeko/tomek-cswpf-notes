using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using tomek_cswpf_notes.extensions;

namespace tomek_cswpf_notes
{
    /// <summary>
    /// Interaction logic for DemoSplash.xaml
    /// </summary>
    public partial class DemoSplash : Window, INotifyPropertyChanged
    {

        private string _SplashText;
        public string SplashText { get { return _SplashText; } set { _SplashText = value; NotifyPropertyChanged(); } }

        private double _Progress = 0;
        public double Progress { get { return _Progress; } set { _Progress = value; NotifyPropertyChanged(); } }



        public DemoSplash(string splashtext = "DemoSplash")
        {
            InitializeComponent();
            DataContext = this;
            SplashText = splashtext;
        }

        public async Task UpdateProgressSmooth(int progress)
        {
            await prgInit.SetPercent(progress);
        }

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
