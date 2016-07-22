using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class ProgressDialog : UserControl
    {
        public ProgressDialog()
        {
            this.InitializeComponent();
        }

        public string ProgressMessage
        {
            set { SetValue(ProgressMessageProperty, value); }
            get { return GetValue(ProgressMessageProperty) as string; }
        }

        public static readonly DependencyProperty ProgressMessageProperty = DependencyProperty.Register(
            nameof(ProgressMessage),
            typeof(string),
            typeof(ProgressDialog),
            new PropertyMetadata("", new PropertyChangedCallback(OnProgressMessageUpdated)));

        private static void OnProgressMessageUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Utility.DebugUtil.Log(() => "OnProgressMessageUpdated: " + e.NewValue as string);
            var dialog = d as ProgressDialog;
            dialog.ProgressText.Text = e.NewValue as string;
        }

        public new Visibility Visibility
        {
            set
            {
                if (value == Visibility.Visible)
                {
                    ProgressCircle.IsActive = true;
                }
                else
                {
                    ProgressCircle.IsActive = false;
                }
                base.Visibility = value;
            }
            get { return base.Visibility; }
        }
    }
}
