using Locana.DataModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Locana.Utility
{
    public enum LocanaVisualState
    {
        Narrow,
        Wide,
        ExtraWide,
    }

    public class LocanaStateTrigger : StateTriggerBase
    {
        const double NARROW_STATE_MAX_WIDTH = 720;
        const double WIDE_STATE_MAX_WIDTH = 1024;

        public LocanaStateTrigger()
        {
            Window.Current.SizeChanged += Current_SizeChanged;
            ApplicationSettings.GetInstance().ForcePhoneViewChanged += ForcePhoneViewSettingChanged;
        }

        private void ForcePhoneViewSettingChanged(bool force)
        {
            SetActive(ShouldTriggerBeActive(LocanaState));
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            SetActive(ShouldTriggerBeActive(LocanaState));
        }

        private static bool ShouldTriggerBeActive(LocanaVisualState state)
        {
            var currentView = ApplicationView.GetForCurrentView();
            bool active = false;
            var width = currentView.VisibleBounds.Width;

            if (ApplicationSettings.GetInstance().ForcePhoneView)
            {
                active = ShouldTriggerBeActiveOnPhoneView(state);
            }
            else
            {
                active = ShouldTriggerBeActive(state, width);
            }

            return active;
        }

        private static bool ShouldTriggerBeActive(LocanaVisualState state, double width)
        {
            bool active;
            switch (state)
            {
                case LocanaVisualState.Narrow:
                    active = (width <= NARROW_STATE_MAX_WIDTH);
                    break;
                case LocanaVisualState.Wide:
                    active = (width > NARROW_STATE_MAX_WIDTH && width <= WIDE_STATE_MAX_WIDTH);
                    break;
                case LocanaVisualState.ExtraWide:
                    active = (width > WIDE_STATE_MAX_WIDTH);
                    break;
                default:
                    active = false;
                    break;
            }

            return active;
        }

        private static bool ShouldTriggerBeActiveOnPhoneView(LocanaVisualState state)
        {
            bool active;
            switch (state)
            {
                case LocanaVisualState.Narrow:
                    active = true;
                    break;
                default:
                    active = false;
                    break;
            }

            return active;
        }

        public LocanaVisualState LocanaState
        {
            get
            {
                return (LocanaVisualState)GetValue(LocanaStateProperty);
            }
            set
            {
                SetValue(LocanaStateProperty, value);
            }
        }

        public DependencyProperty LocanaStateProperty = DependencyProperty.Register(
            nameof(LocanaState),
            typeof(LocanaVisualState),
            typeof(LocanaStateTrigger),
            new PropertyMetadata(LocanaVisualState.Narrow, OnOrientationSizeChanged));

        private static void OnOrientationSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var newVal = (LocanaVisualState)args.NewValue;
            (sender as LocanaStateTrigger).SetActive(ShouldTriggerBeActive(newVal));
        }
    }
}
