using Locana.CameraControl;
using Locana.Controls;
using Locana.Utility;
using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;

namespace Locana.Pages
{
    public sealed partial class ShootingPage : IKeyHandlerPage
    {
        private bool IsBarRightOn = false;
        private bool IsBarLeftOn = false;

        private bool IsCtlKeyPressed = false;
        private bool IsShiftKeyPressed = false;
        private bool IsAltKeyPressed = false;

        private List<KeyAssignmentData> keyAssignments;

        public IEnumerable<KeyAssignmentData> KeyAssignments
        {
            get
            {
                if (keyAssignments == null)
                {
                    keyAssignments = new List<KeyAssignmentData>();
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Space", Description = SystemUtil.GetStringResource("KeyDesc_TakePicture") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Alt + Left", Description = SystemUtil.GetStringResource("KeyDesc_ShowCtrlPanel") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Alt + Right", Description = SystemUtil.GetStringResource("KeyDesc_HideCtrlPanel") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Alt + Up", Description = SystemUtil.GetStringResource("KeyDesc_OpenBottomBar") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Alt + Down", Description = SystemUtil.GetStringResource("KeyDesc_CloseBottomBar") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + Home", Description = SystemUtil.GetStringResource("KeyDesc_TickRight") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + End", Description = SystemUtil.GetStringResource("KeyDesc_TickLeft") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + Z", Description = SystemUtil.GetStringResource("KeyDesc_Zoom") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + I", Description = SystemUtil.GetStringResource("KeyDesc_ISO") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + E", Description = SystemUtil.GetStringResource("KeyDesc_EV") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + F", Description = SystemUtil.GetStringResource("KeyDesc_FNum") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + S", Description = SystemUtil.GetStringResource("KeyDesc_SS") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + P", Description = SystemUtil.GetStringResource("KeyDesc_PShift") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + C", Description = SystemUtil.GetStringResource("KeyDesc_CancelTouchAF") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + M", Description = SystemUtil.GetStringResource("KeyDesc_ExposureMode") });
                }

                return keyAssignments;
            }
        }

        private async void Global_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.KeyStatus.RepeatCount == 1)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Control:
                        IsCtlKeyPressed = true;
                        break;
                    case VirtualKey.Shift:
                        IsShiftKeyPressed = true;
                        break;
                    case VirtualKey.Menu:
                        IsAltKeyPressed = true;
                        break;
                    case VirtualKey.Space:
                        ShutterButtonPressed();
                        break;
                    case VirtualKey.Left:
                        if (IsAltKeyPressed && !ControlPanelDisplayed) { ToggleControlPanel(); }
                        break;
                    case VirtualKey.Right:
                        if (IsAltKeyPressed && ControlPanelDisplayed) { ToggleControlPanel(); }
                        break;
                    case VirtualKey.Home:
                        if (IsCtlKeyPressed)
                        {
                            if (ZoomElements.Visibility.IsVisible())
                            {
                                if (!IsBarLeftOn)
                                {
                                    if (IsShiftKeyPressed) { IsBarLeftOn = true; ZoomOutStart(); }
                                    else { ZoomOutTick(); }
                                }
                            }
                            else if (ISOSlider.Visibility.IsVisible()) { ISOSlider.TickSlider(1); }
                            else if (EvSlider.Visibility.IsVisible()) { EvSlider.TickSlider(1); }
                            else if (FnumberSlider.Visibility.IsVisible()) { FnumberSlider.TickSlider(1); }
                            else if (SSSlider.Visibility.IsVisible()) { SSSlider.TickSlider(1); }
                            else if (ProgramShiftSlider.Visibility.IsVisible()) { ProgramShiftSlider.TickSlider(1); }
                        }
                        break;
                    case VirtualKey.End:
                        if (IsCtlKeyPressed)
                        {
                            if (ZoomElements.Visibility.IsVisible())
                            {
                                if (!IsBarRightOn)
                                {
                                    if (IsShiftKeyPressed) { IsBarRightOn = true; ZoomInStart(); }
                                    else { ZoomInTick(); }
                                }
                            }
                            else if (ISOSlider.Visibility.IsVisible()) { ISOSlider.TickSlider(-1); }
                            else if (EvSlider.Visibility.IsVisible()) { EvSlider.TickSlider(-1); }
                            else if (FnumberSlider.Visibility.IsVisible()) { FnumberSlider.TickSlider(-1); }
                            else if (SSSlider.Visibility.IsVisible()) { SSSlider.TickSlider(-1); }
                            else if (ProgramShiftSlider.Visibility.IsVisible()) { ProgramShiftSlider.TickSlider(-1); }
                        }
                        break;
                    case VirtualKey.Up:
                        if (IsAltKeyPressed && !AppBarUnit.IsOpen)
                        {
                            AppBarUnit.IsOpen = true;
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.Down:
                        if (IsAltKeyPressed && AppBarUnit.IsOpen)
                        {
                            AppBarUnit.IsOpen = false;
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.Z:
                        if (IsCtlKeyPressed)
                        {
                            _CommandBarManager.FireTapEvent(AppBarItem.Zoom, this);
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.I:
                        if (IsCtlKeyPressed)
                        {
                            _CommandBarManager.FireTapEvent(AppBarItem.IsoSlider, this);
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.E:
                        if (IsCtlKeyPressed)
                        {
                            _CommandBarManager.FireTapEvent(AppBarItem.EvSlider, this);
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.F:
                        if (IsCtlKeyPressed)
                        {
                            _CommandBarManager.FireTapEvent(AppBarItem.FNumberSlider, this);
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.S:
                        if (IsCtlKeyPressed)
                        {
                            _CommandBarManager.FireTapEvent(AppBarItem.ShutterSpeedSlider, this);
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.P:
                        if (IsCtlKeyPressed)
                        {
                            _CommandBarManager.FireTapEvent(AppBarItem.ProgramShiftSlider, this);
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.C:
                        if (IsCtlKeyPressed)
                        {
                            _CommandBarManager.FireTapEvent(AppBarItem.CancelTouchAF, this);
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.M:
                        if (IsCtlKeyPressed)
                        {
                            await SequentialOperation.ToggleExposureMode(this.target);
                            args.Handled = true;
                        }
                        break;

                }
            }
        }

        private void Global_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.KeyStatus.IsKeyReleased)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Control:
                        IsCtlKeyPressed = false;

                        ISOSlider.FixShootingParam();
                        EvSlider.FixShootingParam();
                        FnumberSlider.FixShootingParam();
                        SSSlider.FixShootingParam();
                        ProgramShiftSlider.ReleaseSlider();
                        break;
                    case VirtualKey.Shift:
                        IsShiftKeyPressed = false;
                        if (IsBarRightOn)
                        {
                            IsBarRightOn = false;
                            if (ZoomElements.Visibility.IsVisible()) { ZoomInStop(); }
                        }
                        else if (IsBarLeftOn)
                        {
                            IsBarLeftOn = false;
                            if (ZoomElements.Visibility.IsVisible()) { ZoomOutStop(); }
                        }
                        break;
                    case VirtualKey.Menu:
                        IsAltKeyPressed = false;
                        break;
                }
            }
        }
    }
}
