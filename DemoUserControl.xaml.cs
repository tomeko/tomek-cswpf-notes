using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace tomek_cswpf_notes
{
    /// <summary>
    /// Interaction logic for DemoUserControl.xaml
    /// </summary>
    public partial class DemoUserControl : UserControl
    {

        static readonly SolidColorBrush DEFAULT_CIRCLE_COLOR = new SolidColorBrush(Colors.Gray);

        public SolidColorBrush ColorCircle
        {
            get { return (SolidColorBrush)GetValue(ColorCircleProperty); }
            set { SetValue(ColorCircleProperty, value); }
        }

        public static readonly DependencyProperty ColorCircleProperty =
            DependencyProperty.Register("ColorCircle", typeof(SolidColorBrush), typeof(DemoUserControl), new PropertyMetadata(DEFAULT_CIRCLE_COLOR));

        public string TextDemoLabel
        {
            get { return (string)GetValue(TextDemoLabelProperty); }
            set { SetValue(TextDemoLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextDemoLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextDemoLabelProperty =
            DependencyProperty.Register("TextDemoLabel", typeof(string), typeof(DemoUserControl), new PropertyMetadata("Default Label"));


        public DemoUserControl()
        {
            InitializeComponent();
        }
    }
}
