using Locana.Controls;
using Locana.DataModel;
using Locana.Utility;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;

namespace Locana.Pages
{
    public sealed partial class ContentsGridPage : IKeyHandlerPage
    {

        private bool IsCtlKeyPressed = false;

        private List<KeyAssignmentData> keyAssignments;

        public IEnumerable<KeyAssignmentData> KeyAssignments
        {
            get
            {
                if (keyAssignments == null)
                {
                    keyAssignments = new List<KeyAssignmentData>();
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + Z", Description = SystemUtil.GetStringResource("KeyDesc_SemanticZoom") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + R", Description = SystemUtil.GetStringResource("KeyDesc_RmMode") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + L", Description = SystemUtil.GetStringResource("KeyDesc_DlMode") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + C", Description = SystemUtil.GetStringResource("KeyDesc_CloseCancel") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + X", Description = SystemUtil.GetStringResource("KeyDesc_Exec") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + P", Description = SystemUtil.GetStringResource("KeyDesc_PlayPause") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + I", Description = SystemUtil.GetStringResource("KeyDesc_ExifInfo") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + Left", Description = SystemUtil.GetStringResource("KeyDesc_RotateLeft") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Ctrl + Right", Description = SystemUtil.GetStringResource("KeyDesc_RotateRight") });
                    keyAssignments.Add(new KeyAssignmentData { AssignedKey = "Space", Description = SystemUtil.GetStringResource("KeyDesc_SelectContent") });
                }

                return keyAssignments;
            }
        }

        private void Global_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.KeyStatus.RepeatCount == 1)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Control:
                        IsCtlKeyPressed = true;
                        break;
                    case VirtualKey.Z:
                        if (IsCtlKeyPressed) // Control+Z (Switch semantic zoom)
                        {
                            if (InnerState == ViewerState.Single || InnerState == ViewerState.Multi)
                            {
                                GridHolder.IsZoomedInViewActive = !GridHolder.IsZoomedInViewActive;
                                args.Handled = true;
                            }
                        }
                        break;
                    case VirtualKey.R:
                        if (IsCtlKeyPressed) // Control+R (Switch multiple removal mode)
                        {
                            if (CommandBarManager.IsEnabled(AppBarItemType.Command, AppBarItem.DeleteMultiple)
                                && Operator?.ContentsCollection.SelectivityFactor != SelectivityFactor.Delete)
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
                            if (CommandBarManager.IsEnabled(AppBarItemType.Command, AppBarItem.DownloadMultiple)
                                && Operator?.ContentsCollection.SelectivityFactor != SelectivityFactor.Download)
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
                            if (CommandBarManager.IsEnabled(AppBarItemType.Command, AppBarItem.Close))
                            {
                                CommandBarCloseSelected();
                                args.Handled = true;
                            }
                        }
                        break;
                    case VirtualKey.X:
                        if (IsCtlKeyPressed) // Control+X (Execute: Equivalent to check button)
                        {
                            if (CommandBarManager.IsEnabled(AppBarItemType.Command, AppBarItem.Ok))
                            {
                                CommandBarOkSelected();
                                args.Handled = true;
                            }
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
                            if (CommandBarManager.IsEnabled(AppBarItemType.Command, AppBarItem.RotateLeft))
                            {
                                CommandBarRotateLeftSelected();
                                args.Handled = true;
                            }
                        }
                        break;
                    case VirtualKey.Right:
                        if (IsCtlKeyPressed) // Control+Right (Turn photo right)
                        {
                            if (CommandBarManager.IsEnabled(AppBarItemType.Command, AppBarItem.RotateRight))
                            {
                                CommandBarRotateRightSelected();
                                args.Handled = true;
                            }
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
                }
            }
        }
    }
}
