using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

[Collection(DialogLifecycleTestCollection.Name)]
public sealed class OverlayDialogOwnerResizeTests
{
    static OverlayDialogOwnerResizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Non_draggable_natural_dialog_restores_its_viewport_and_center_through_owner_resize(bool dark)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var placementTarget = new Border { Width = 100, Height = 40 };
            var owner = new AtomUI.Desktop.Controls.Window
            {
                Width = 1240,
                Height = 820,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
                Content = new ScopeAwareOverlayLayerPanel
                {
                    Children = { placementTarget }
                }
            };
            var editor = new AtomUI.Desktop.Controls.LineEdit
            {
                Text = "unchanged draft",
                Height = 32
            };
            var scrollViewer = new Avalonia.Controls.ScrollViewer
            {
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = new StackPanel
                {
                    Children = { editor, new Border { Height = 1200 } }
                }
            };
            var footer = new Border { Height = 32 };
            var content = new Avalonia.Controls.Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto"),
                Margin = new Thickness(8),
                Children = { scrollViewer, footer }
            };
            Avalonia.Controls.Grid.SetRow(footer, 1);
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                Title = "Natural-size owner resize",
                Content = content,
                IsModal = true,
                IsMaskClosable = false,
                IsMotionEnabled = false,
                IsDragMovable = false,
                IsResizable = false,
                IsFooterVisible = false,
                HostWidth = 760,
                HostMaxWidth = 760,
                HostHeight = double.NaN,
                HostMaxHeight = 796
            };
            var presenter = new OverlayDialogPresenter(dialog, placementTarget);

            try
            {
                owner.Show();
                Pump(owner);
                Wait(presenter.ShowAsync(CancellationToken.None).AsTask());
                Pump(owner);

                content.Bounds.Width.ShouldBeGreaterThan(0);
                var originalViewportHeight = scrollViewer.Viewport.Height;
                originalViewportHeight.ShouldBeGreaterThan(0);
                scrollViewer.Extent.Height.ShouldBeGreaterThan(originalViewportHeight);
                editor.Focus().ShouldBeTrue();

                foreach (var size in new[]
                         {
                             new Size(1240, 820),
                             new Size(940, 640),
                             new Size(1240, 820),
                             new Size(940, 640)
                         })
                {
                    owner.Width = size.Width;
                    owner.Height = size.Height;
                    Pump(owner);

                    owner.ClientSize.ShouldBe(size);
                    presenter.Surface.Height.ShouldBe(double.NaN);
                    footer.Bounds.Height.ShouldBe(32);
                    if (size.Height == 820)
                    {
                        scrollViewer.Viewport.Height.ShouldBe(originalViewportHeight, 1);
                    }
                    else
                    {
                        scrollViewer.Viewport.Height.ShouldBeLessThan(originalViewportHeight);
                    }
                    var center = content.TranslatePoint(
                        new Point(content.Bounds.Width / 2, 0), owner).ShouldNotBeNull().X;
                    center.ShouldBe(owner.ClientSize.Width / 2, 1);
                    var surface = presenter.Surface;
                    var surfaceCenter = surface.TranslatePoint(
                        new Point(surface.Bounds.Width / 2, surface.Bounds.Height / 2), owner)
                        .ShouldNotBeNull();
                    surfaceCenter.Y.ShouldBe(owner.ClientSize.Height / 2, 1);
                    dialog.OffsetX.ShouldBe(0);
                    dialog.OffsetY.ShouldBe(0);
                    editor.Text.ShouldBe("unchanged draft");
                    editor.IsKeyboardFocusWithin.ShouldBeTrue();
                }
            }
            finally
            {
                Wait(presenter.CloseAsync().AsTask());
                Wait(presenter.DisposeAsync().AsTask());
                owner.Close();
            }
        });
    }

    [Fact]
    public void Owner_resize_temporarily_clamps_surface_without_rewriting_user_offsets()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var placementTarget = new Border { Width = 100, Height = 40 };
            var owner = new AtomUI.Desktop.Controls.Window
            {
                Width = 1240,
                Height = 820,
                Content = new ScopeAwareOverlayLayerPanel
                {
                    Children = { placementTarget }
                }
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                IsModal = true,
                IsMaskClosable = false,
                IsMotionEnabled = false,
                IsDragMovable = true,
                IsResizable = false,
                IsFooterVisible = false,
                HostWidth = 760,
                HostHeight = 500,
                OffsetX = 160,
                OffsetY = 120
            };
            var presenter = new OverlayDialogPresenter(dialog, placementTarget);

            try
            {
                owner.Show();
                Pump(owner);
                Wait(presenter.ShowAsync(CancellationToken.None).AsTask());
                Pump(owner);

                AssertSurfaceCenter(owner, presenter.Surface, new Point(780, 530));

                owner.Width = 940;
                owner.Height = 640;
                Pump(owner);

                var constrainedPosition = presenter.Surface.TranslatePoint(default, owner).ShouldNotBeNull();
                constrainedPosition.X.ShouldBe(180, 1);
                constrainedPosition.Y.ShouldBe(140, 1);
                dialog.OffsetX.ShouldBe(160);
                dialog.OffsetY.ShouldBe(120);

                owner.Width = 1240;
                owner.Height = 820;
                Pump(owner);

                AssertSurfaceCenter(owner, presenter.Surface, new Point(780, 530));
                dialog.OffsetX.ShouldBe(160);
                dialog.OffsetY.ShouldBe(120);
            }
            finally
            {
                Wait(presenter.CloseAsync().AsTask());
                Wait(presenter.DisposeAsync().AsTask());
                owner.Close();
            }
        });
    }

    private static void AssertSurfaceCenter(
        TopLevel owner,
        Control surface,
        Point expectedCenter)
    {
        var center = surface.TranslatePoint(
            new Point(surface.Bounds.Width / 2, surface.Bounds.Height / 2),
            owner).ShouldNotBeNull();
        center.X.ShouldBe(expectedCenter.X, 1);
        center.Y.ShouldBe(expectedCenter.Y, 1);
    }

    private static void Pump(TopLevel owner)
    {
        Dispatcher.UIThread.RunJobs();
        owner.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
        owner.UpdateLayout();
    }

    private static void Wait(Task task)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _ = task.ContinueWith(
            _ => cancellation.Cancel(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        Dispatcher.UIThread.MainLoop(cancellation.Token);
        task.GetAwaiter().GetResult();
    }
}
