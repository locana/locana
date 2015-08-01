using Kazyx.Uwpmm.DataModel;
using System;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class CheckBoxSetting : UserControl
    {
        public CheckBoxSetting(AppSettingData<bool> data)
        {
            this.InitializeComponent();
            if (data == null)
            {
                throw new ArgumentNullException("AppSettingData must not be null");
            }
            this.DataContext = data;
        }
    }
}
