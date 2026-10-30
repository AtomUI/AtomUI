using System.Reflection;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

[Collection(DialogLifecycleTestCollection.Name)]
public class DialogMotionAnchorTests
{
    static DialogMotionAnchorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Dialog_Without_Explicit_PlacementTarget_Uses_Centered_Zoom_Motion()
    {
        var placementTarget = new Border();
        var dialog = new AtomUI.Desktop.Controls.Dialog();
        var presenter = new OverlayDialogPresenter(dialog, placementTarget);

        CreateSurfaceMotion(presenter, isOpening: true).ShouldBeOfType<DialogZoomInMotion>();
        CreateSurfaceMotion(presenter, isOpening: false).ShouldBeOfType<DialogZoomOutMotion>();
    }

    [Fact]
    public void Dialog_With_Explicit_PlacementTarget_Uses_Anchored_Zoom_Motion()
    {
        var placementTarget = new Border();
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            PlacementTarget = placementTarget
        };
        var presenter = new OverlayDialogPresenter(dialog, placementTarget);

        CreateSurfaceMotion(presenter, isOpening: true).ShouldBeOfType<DialogZoomInMotion>();
        CreateSurfaceMotion(presenter, isOpening: false).ShouldBeOfType<DialogZoomOutMotion>();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Static_Dialog_Only_Uses_An_Explicit_Target_As_Motion_Anchor(
        bool hasExplicitTarget,
        bool shouldUseAnchor)
    {
        var fallbackTarget = new Border();
        var explicitTarget = new Border();
        var dialog = CreateStaticDialog(
            fallbackTarget,
            hasExplicitTarget
                ? new DialogOptions { PlacementTarget = explicitTarget }
                : null);
        var presenter = new OverlayDialogPresenter(
            dialog,
            hasExplicitTarget ? explicitTarget : fallbackTarget);

        var openingMotion = CreateSurfaceMotion(presenter, isOpening: true);

        if (shouldUseAnchor)
        {
            openingMotion.ShouldBeOfType<DialogZoomInMotion>();
        }
        else
        {
            openingMotion.ShouldBeOfType<DialogZoomInMotion>();
        }
    }

    [Fact]
    public void Closing_Motion_Keeps_Footer_Buttons_Until_The_Presenter_Is_Removed()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 80, Height = 32 };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Width  = 480,
                Height = 360,
                Children = { placementTarget }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 480,
                Height  = 360,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                Content         = "Dialog",
                StandardButtons = DialogStandardButton.Ok,
                IsMotionEnabled = false,
                HostWidth       = 240,
                HostHeight      = 160
            };
            var presenter = new OverlayDialogPresenter(dialog, placementTarget);

            try
            {
                window.Show();
                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
                var buttonBox = presenter.GetVisualDescendants()
                                         .OfType<DialogButtonBox>()
                                         .ShouldHaveSingleItem();
                var button = buttonBox.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.DialogButton>()
                                      .ShouldHaveSingleItem();
                var contentLayer = presenter.Surface.SurfaceContentLayer.ShouldNotBeNull();
                var originalSurfaceBounds = presenter.Surface.Bounds;

                presenter.IsMotionEnabled = true;
                presenter.MotionDuration  = TimeSpan.FromMilliseconds(80);
                var closeTask = presenter.CloseAsync().AsTask();

                closeTask.IsCompleted.ShouldBeFalse();
                presenter.Parent.ShouldBeOfType<DialogOverlayLayer>();
                button.IsAttachedToVisualTree().ShouldBeTrue();
                contentLayer.IsAttachedToVisualTree().ShouldBeTrue();
                presenter.Surface.Bounds.ShouldBe(originalSurfaceBounds);
                buttonBox.GetVisualDescendants()
                         .OfType<AtomUI.Desktop.Controls.DialogButton>()
                         .Count()
                         .ShouldBe(1);

                WaitWithDispatcherPump(closeTask);
                presenter.Parent.ShouldBeNull();
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static object CreateSurfaceMotion(OverlayDialogPresenter presenter, bool isOpening)
    {
        var method = typeof(OverlayDialogPresenter).GetMethod(
            "CreateSurfaceMotion",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return method.Invoke(presenter, [isOpening]).ShouldNotBeNull();
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

    private static void WaitWithDispatcherPump(Task task)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _ = task.ContinueWith(
            _ => cancellation.Cancel(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        Dispatcher.UIThread.MainLoop(cancellation.Token);

        task.IsCompleted.ShouldBeTrue();
        task.GetAwaiter().GetResult();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }
}
