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
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Z", Description = SystemUtil.GetStringResource("KeyDesc_Zoom") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "I", Description = SystemUtil.GetStringResource("KeyDesc_ISO") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "E", Description = SystemUtil.GetStringResource("KeyDesc_EV") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "F", Description = SystemUtil.GetStringResource("KeyDesc_FNum") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "S", Description = SystemUtil.GetStringResource("KeyDesc_SS") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "P", Description = SystemUtil.GetStringResource("KeyDesc_PShift") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "C", Description = SystemUtil.GetStringResource("KeyDesc_CancelTouchAF") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "M", Description = SystemUtil.GetStringResource("KeyDesc_ExposureMode") });
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
                        if (IsAltKeyPressed && !AppBarUnit.IsOpen) { AppBarUnit.IsOpen = true; }
                        break;
                    case VirtualKey.Down:
                        if (IsAltKeyPressed && AppBarUnit.IsOpen) { AppBarUnit.IsOpen = false; }
                        break;
                    case VirtualKey.Z:
                        _CommandBarManager.FireTapEvent(AppBarItem.Zoom, this);
                        break;
                    case VirtualKey.I:
                        _CommandBarManager.FireTapEvent(AppBarItem.IsoSlider, this);
                        break;
                    case VirtualKey.E:
                        _CommandBarManager.FireTapEvent(AppBarItem.EvSlider, this);
                        break;
                    case VirtualKey.F:
                        _CommandBarManager.FireTapEvent(AppBarItem.FNumberSlider, this);
                        break;
                    case VirtualKey.S:
                        _CommandBarManager.FireTapEvent(AppBarItem.ShutterSpeedSlider, this);
                        break;
                    case VirtualKey.P:
                        _CommandBarManager.FireTapEvent(AppBarItem.ProgramShiftSlider, this);
                        break;
                    case VirtualKey.C:
                        _CommandBarManager.FireTapEvent(AppBarItem.CancelTouchAF, this);
                        break;
                    case VirtualKey.M:
                        await SequentialOperation.ToggleExposureMode(this.target);
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
