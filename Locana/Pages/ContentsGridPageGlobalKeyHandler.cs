using Locana.Controls;
using Locana.DataModel;
using Windows.System;
using Windows.UI.Core;

namespace Locana.Pages
{
    public sealed partial class ContentsGridPage
    {

        private bool IsCtlKeyPressed = false;
        private bool IsShiftKeyPressed = false;
        private bool IsAltKeyPressed = false;

        private void Global_KeyDown(CoreWindow sender, KeyEventArgs args)
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
                    case VirtualKey.Z:
                        if (IsCtlKeyPressed) // Control+Z (Switch semantic zoom)
                        {
                            GridHolder.IsZoomedInViewActive = !GridHolder.IsZoomedInViewActive;
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.R:
                        if (IsCtlKeyPressed) // Control+R (Switch multiple removal mode)
                        {
                            if (Operator?.ContentsCollection.SelectivityFactor != SelectivityFactor.Delete)
                            {
                                CommandBarDeleteMultipleSelected();
                            }
                            else
                            {
                                UpdateInnerState(ViewerState.Single);
                            }
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.L:
                        if (IsCtlKeyPressed) // Control+L (Switch multiple download mode)
                        {
                            if (Operator?.ContentsCollection.SelectivityFactor != SelectivityFactor.Download)
                            {
                                CommandBarDownloadMultipleSelected();
                            }
                            else
                            {
                                UpdateInnerState(ViewerState.Single);
                            }
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.C:
                        if (IsCtlKeyPressed) // Control+C (Close: Equivalent to cross button)
                        {
                            CommandBarCloseSelected();
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.X:
                        if (IsCtlKeyPressed) // Control+X (Execute: Equivalent to check button)
                        {
                            CommandBarOkSelected();
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.P:
                        if (IsCtlKeyPressed) // Control+P (Switch movie play/pause)
                        {
                            if (MoviePlayer.PlaybackState == MoviePlaybackScreen.PlayerState.PausedOrStopped)
                            {
                                CommandBarResumeMovieSelected();
                            }
                            else if (MoviePlayer.PlaybackState == MoviePlaybackScreen.PlayerState.Playing)
                            {
                                CommandBarPauseMovieSelected();
                            }
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.I:
                        if (IsCtlKeyPressed) // Control+I (Switch photo detail info display visibility)
                        {
                            if (PhotoScreen.DetailInfoDisplayed)
                            {
                                CommandBarHideDetailInfo();
                            }
                            else
                            {
                                CommandBarShowDetailInfo();
                            }
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.Left:
                        if (IsCtlKeyPressed) // Control+Left (Turn photo left)
                        {
                            CommandBarRotateLeftSelected();
                            args.Handled = true;
                        }
                        break;
                    case VirtualKey.Right:
                        if (IsCtlKeyPressed) // Control+Right (Turn photo right)
                        {
                            CommandBarRotateRightSelected();
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
                        break;
                    case VirtualKey.Shift:
                        IsShiftKeyPressed = false;
                        break;
                    case VirtualKey.Menu:
                        IsAltKeyPressed = false;
                        break;
                }
            }
        }
    }
}
