using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

using BindingFlags = System.Reflection.BindingFlags;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class DialogMaskClosableTests
{
    static DialogMaskClosableTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void MaskClosable_Defaults_To_True_And_MessageBox_Inherits()
    {
        new AtomUI.Desktop.Controls.Dialog().IsMaskClosable.ShouldBeTrue();
        new AtomUI.Desktop.Controls.MessageBox().IsMaskClosable.ShouldBeTrue();
        new DialogOptions().IsMaskClosable.ShouldBeTrue();
        new AtomUI.Desktop.Controls.MessageBoxOptions().IsMaskClosable.ShouldBeTrue();
    }

    [Fact]
    public void DialogOptions_And_MessageBoxOptions_Map_IsMaskClosable()
    {
        var placementTarget = new Border { Width = 100, Height = 40 };

        var dialog = CreateStaticDialog(placementTarget, new DialogOptions { IsMaskClosable = false });
        dialog.IsMaskClosable.ShouldBeFalse();

        var messageBox = CreateStaticMessageBox(placementTarget, new AtomUI.Desktop.Controls.MessageBoxOptions { IsMaskClosable = false });
        messageBox.IsMaskClosable.ShouldBeFalse();
    }

    [Fact]
    public void Mask_Press_Is_Swallowed_Without_Close_When_Not_MaskClosable()
    {
        RunOnUIThread(() =>
        {
            var backgroundPressCount = 0;
            var closeRequestCount    = 0;
            var background = new Border
            {
                Width           = 640,
                Height          = 480,
                Background      = Brushes.Transparent,
                Focusable       = true,
                IsHitTestVisible = true
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { background }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 640,
                Height  = 480,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                IsModal         = true,
                IsMaskClosable  = false,
                IsMotionEnabled = false,
                HostWidth       = 320,
                HostHeight      = 180
            };
            var presenter = new OverlayDialogPresenter(dialog, background);
            background.PointerPressed += (_, _) => backgroundPressCount++;
            presenter.CloseRequested  += (_, _) => closeRequestCount++;

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);

                var mask = presenter.GetVisualDescendants()
                                    .OfType<OverlayDialogMask>()
                                    .Single();
                // Headless Window.MouseDown does not traverse the TopLevel OverlayLayer;
                // raise the routed event at the actual mask target instead.
                RaisePointerPressed(mask);
                Dispatcher.UIThread.RunJobs();

                closeRequestCount.ShouldBe(0, "IsMaskClosable=false 时 mask 外点不得发起关闭请求");
                backgroundPressCount.ShouldBe(0, "modal mask 仍应阻断底层输入");
            }
            finally
            {
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void Mask_Press_Requests_Close_When_MaskClosable()
    {
        RunOnUIThread(() =>
        {
            var closeRequestCount = 0;
            var background = new Border
            {
                Width           = 640,
                Height          = 480,
                Background      = Brushes.Transparent,
                Focusable       = true,
                IsHitTestVisible = true
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { background }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 640,
                Height  = 480,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                IsModal         = true,
                IsMaskClosable  = true,
                IsMotionEnabled = false,
                HostWidth       = 320,
                HostHeight      = 180
            };
            var presenter = new OverlayDialogPresenter(dialog, background);
            presenter.CloseRequested += (_, e) =>
            {
                e.Reason.ShouldBe(DialogCloseReason.HostCloseRequest);
                closeRequestCount++;
            };

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);

                var mask = presenter.GetVisualDescendants()
                                    .OfType<OverlayDialogMask>()
                                    .Single();
                // Headless Window.MouseDown does not traverse the TopLevel OverlayLayer;
                // raise the routed event at the actual mask target instead.
                RaisePointerPressed(mask);
                Dispatcher.UIThread.RunJobs();

                closeRequestCount.ShouldBe(1);
            }
            finally
            {
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    private static AtomUI.Desktop.Controls.Dialog CreateStaticDialog(
        Control placementTarget,
        DialogOptions? options)
    {
        var createDialog = typeof(AtomUI.Desktop.Controls.Dialog).GetMethod(
            "CreateDialog",
            BindingFlags.Static | BindingFlags.NonPublic);
        createDialog.ShouldNotBeNull();
        return (AtomUI.Desktop.Controls.Dialog)createDialog.Invoke(
            null,
            [new AtomUI.Desktop.Controls.TextBlock { Text = "Dialog" }, null, options, placementTarget])!;
    }

    private static AtomUI.Desktop.Controls.MessageBox CreateStaticMessageBox(
        Control placementTarget,
        MessageBoxOptions? options)
    {
        var createMessageBox = typeof(AtomUI.Desktop.Controls.MessageBox).GetMethod(
            "CreateMessageBox",
            BindingFlags.Static | BindingFlags.NonPublic);
        createMessageBox.ShouldNotBeNull();
        return (AtomUI.Desktop.Controls.MessageBox)createMessageBox.Invoke(
            null,
            [new AtomUI.Desktop.Controls.TextBlock { Text = "MessageBox" }, null, options, placementTarget])!;
    }

    private static void WaitWithDispatcherPump(Task task)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!task.IsCompleted && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        task.IsCompleted.ShouldBeTrue();
        task.GetAwaiter().GetResult();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

    private static void RaisePointerPressed(InputElement source)
    {
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
    }
}
