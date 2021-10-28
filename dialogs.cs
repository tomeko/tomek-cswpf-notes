using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tomek_cswpf_notes
{
    public class dialogs
    {

        public class MessageDialog
        {
            Window windlg;
            static readonly Thickness btnPadding = new Thickness(10,5,10,5);

            private MessageDialog(string message, string title, bool hascancel)
            {
                windlg = new Window() { Width = 300, Height = 200, WindowStyle = WindowStyle.SingleBorderWindow, ResizeMode = ResizeMode.CanMinimize, WindowStartupLocation = WindowStartupLocation.CenterScreen };
                windlg.Title = title;
                windlg.MouseDown += new MouseButtonEventHandler((s, e) => { windlg.DragMove(); });
                Grid grdmain = new Grid();
                grdmain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grdmain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(60) });
                TextBlock tbmsg = new TextBlock() { Text = message, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center};
                StackPanel spbuttons = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center};
                Button btnok = new Button() { Content = "_OK", Padding = btnPadding};
                if (hascancel)
                    btnok.Margin = new Thickness(0, 0, 10, 0);
                btnok.Click += new RoutedEventHandler((o, s) => { windlg.DialogResult = true; windlg.Close(); });
                Button btncancel = new Button() { Content = "_Cancel", Padding = btnPadding };
                btncancel.Click += new RoutedEventHandler((o, s) => { windlg.DialogResult = false; windlg.Close(); });
                windlg.PreviewKeyDown += Windlg_PreviewKeyDown;
                spbuttons.Children.Add(btnok);
                if (hascancel)
                    spbuttons.Children.Add(btncancel);
                Grid.SetRow(tbmsg, 0);
                grdmain.Children.Add(tbmsg);
                Grid.SetRow(spbuttons, 1);
                grdmain.Children.Add(spbuttons);
                windlg.Content = grdmain;
                
            }

            public static bool Show(string message, string title = "Dialog", bool hascancel = false)
            {
                MessageDialog diag = new MessageDialog(message, title, hascancel);
                return diag.windlg.ShowDialog() == true;
            }

            private void Windlg_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    windlg.DialogResult = false;
                    windlg.Close();
                    e.Handled = true;
                }
            }
        }

    }
}
