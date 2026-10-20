using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Icons.AntDesign;
using AtomUI.Native;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class WindowDialogPresenterTests
{
    static WindowDialogPresenterTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Requested_Size_Does_Not_Overwrite_The_Native_Client_Size_Before_Layout()
    {
        RunOnUIThread(() =>
        {
            var window = new DialogWindow { Width = 500, Height = 300 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                var platform = window.PlatformImpl.ShouldNotBeNull();
                var nativeSize = platform.ClientSize;
                nativeSize.ShouldBe(new Size(500, 300));

                window.ApplyRequestedSize(550, 350);

                // Width/Height request layout; only the platform resize callback owns ClientSize.
                // A native backend may constrain or decline the request, so copying the request
                // into ClientSize would let the rendered surface diverge from the native window.
                window.ClientSize.ShouldBe(platform.ClientSize);
                window.ClientSize.ShouldBe(nativeSize);
                window.Width.ShouldBe(550);
                window.Height.ShouldBe(350);

                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                window.ClientSize.ShouldBe(new Size(550, 350));
                window.ClientSize.ShouldBe(platform.ClientSize);
                window.Bounds.Size.ShouldBe(platform.ClientSize);
            }
            finally
            {
                window.CloseFromPresenter();
            }
        });
    }

    [Fact]
    public void ShowAsync_Completes_When_The_Native_Window_Opens()
    {
        RunOnUIThread(ShowAsync_Completes_When_The_Native_Window_Opens_Core);
    }

    private static void ShowAsync_Completes_When_The_Native_Window_Opens_Core()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = CreateAnimatedDialog();
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            var showTask = presenter.ShowAsync(CancellationToken.None).AsTask();
            PumpUntil(() => presenter.HostWindow.IsVisible);

            WaitWithDispatcherPump(showTask);
            presenter.HostWindow.GetVisualDescendants()
                     .OfType<AtomUI.Controls.Primitives.MotionActor>()
                     .ShouldBeEmpty();
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void CloseAsync_Closes_The_Native_Window_Without_Content_Motion()
    {
        RunOnUIThread(CloseAsync_Closes_The_Native_Window_Without_Content_Motion_Core);
    }

    private static void CloseAsync_Closes_The_Native_Window_Without_Content_Motion_Core()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = CreateAnimatedDialog();
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var closeTask = presenter.CloseAsync().AsTask();
            Dispatcher.UIThread.RunJobs();

            WaitWithDispatcherPump(closeTask);
            presenter.HostWindow.IsVisible.ShouldBeFalse();
        }
        finally
        {
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void CloseAsync_After_ShowAsync_Completes_Teardown()
    {
        RunOnUIThread(CloseAsync_After_ShowAsync_Completes_Teardown_Core);
    }

    private static void CloseAsync_After_ShowAsync_Completes_Teardown_Core()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = CreateAnimatedDialog();
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            var showTask = presenter.ShowAsync(CancellationToken.None).AsTask();
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());

            WaitWithDispatcherPump(showTask);
            presenter.HostWindow.IsVisible.ShouldBeFalse();
        }
        finally
        {
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Theory]
    [InlineData(double.NaN, double.NaN)]
    [InlineData(double.NaN, 220)]
    [InlineData(360, double.NaN)]
    [InlineData(360, 220)]
    public void Sizing_Resolves_Natural_Dimensions_Before_First_Render(double width, double height)
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = new Border { Width = 180, Height = 70 },
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = width,
            HostHeight = height
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);

            if (double.IsNaN(width))
            {
                surface.Bounds.Width.ShouldBeGreaterThanOrEqualTo(180);
                surface.Bounds.Width.ShouldBeLessThan(owner.ClientSize.Width);
            }
            else
            {
                surface.Bounds.Width.ShouldBe(width);
            }

            if (double.IsNaN(height))
            {
                surface.Bounds.Height.ShouldBeGreaterThanOrEqualTo(70);
                surface.Bounds.Height.ShouldBeLessThan(owner.ClientSize.Height);
            }
            else
            {
                surface.Bounds.Height.ShouldBe(height);
            }

            presenter.HostWindow.ClientSize.ShouldBe(surface.Bounds.Size + chrome);
            presenter.HostWindow.SizeToContent.ShouldBe(SizeToContent.Manual);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Natural_Size_Is_Final_When_The_Native_Window_Opens()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = new Border { Width = 420, Height = 180 },
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = double.NaN,
            HostHeight = double.NaN
        };
        var presenter = new WindowDialogPresenter(dialog, owner);
        Size? openedClientSize = null;
        double? openedOpacity = null;
        presenter.HostWindow.Opened += (_, _) =>
        {
            openedClientSize = presenter.HostWindow.ClientSize;
            openedOpacity = presenter.HostWindow.Opacity;
        };

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            openedClientSize.ShouldNotBeNull();
            openedClientSize.Value.ShouldBe(presenter.HostWindow.ClientSize);
            openedOpacity.ShouldBe(1);
            presenter.HostWindow.Opacity.ShouldBe(1);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Natural_Window_Dialog_Height_Does_Not_Start_At_Maximum()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Title = "Basic window modal",
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "Some contents..." },
                    new TextBlock { Text = "Some contents..." },
                    new TextBlock { Text = "Some contents..." }
                }
            },
            IsModal = false,
            IsMotionEnabled = false,
            IsResizable = true,
            IsClosable = true,
            IsMaximizable = true,
            StandardButtons = DialogStandardButton.Yes,
            DefaultStandardButton = DialogStandardButton.Yes,
            HostMinWidth = 300,
            HostMaxWidth = 520,
            HostMaxHeight = 360
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);

            surface.Bounds.Height.ShouldBeLessThan(dialog.HostMaxHeight);
            presenter.HostWindow.ClientSize.Height.ShouldBe(surface.Bounds.Height + chrome.Height);
            presenter.HostWindow.ClientSize.Height.ShouldBeLessThan(dialog.HostMaxHeight + chrome.Height);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Natural_Window_Dialog_With_Custom_Footer_Does_Not_Start_At_Working_Area()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Title = "Title",
            Content = new StackPanel
            {
                Spacing = 5,
                Children =
                {
                    new TextBlock { Text = "Some contents..." },
                    new TextBlock { Text = "Some contents..." },
                    new TextBlock { Text = "Some contents..." },
                    new TextBlock { Text = "Some contents..." },
                    new TextBlock { Text = "Some contents..." }
                }
            },
            IsModal = false,
            IsMotionEnabled = false,
            StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel,
            DefaultStandardButton = DialogStandardButton.Ok,
            HostMinWidth = 400
        };
        dialog.CustomButtons.Add(new DialogButton
        {
            Role = DialogButtonRole.ActionRole,
            Content = "Custom button"
        });
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);

            surface.Bounds.Width.ShouldBeGreaterThanOrEqualTo(dialog.HostMinWidth);
            surface.Bounds.Width.ShouldBeLessThanOrEqualTo(520);
            surface.Bounds.Height.ShouldBeLessThanOrEqualTo(320);
            presenter.HostWindow.ClientSize.ShouldBe(surface.Bounds.Size + chrome);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Open_Window_Tracks_Dialog_Size_Constraints()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            dialog.HostWidth = double.NaN;
            dialog.HostHeight = 260;
            dialog.HostMinWidth = 240;
            dialog.HostMinHeight = 180;
            dialog.HostMaxWidth = 480;
            dialog.HostMaxHeight = 320;
            Dispatcher.UIThread.RunJobs();

            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var structuralMinimum = surface.MeasureStructuralMinimum();
            surface.Bounds.Height.ShouldBe(260);
            presenter.HostWindow.MinWidth.ShouldBe(
                Math.Max(240, structuralMinimum.Width) + chrome.Width);
            presenter.HostWindow.MinHeight.ShouldBe(
                Math.Max(180, structuralMinimum.Height) + chrome.Height);
            presenter.HostWindow.MaxWidth.ShouldBe(480 + chrome.Width);
            presenter.HostWindow.MaxHeight.ShouldBe(320 + chrome.Height);
            presenter.HostWindow.SizeToContent.ShouldBe(SizeToContent.Manual);

            dialog.HostWidth = 420;
            Dispatcher.UIThread.RunJobs();

            surface.Bounds.Width.ShouldBe(420);
            presenter.HostWindow.SizeToContent.ShouldBe(SizeToContent.Manual);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Finite_Host_Maximum_Disables_Native_Maximize_Until_Both_Axes_Are_Unbounded()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            IsResizable = true,
            IsMaximizable = true,
            HostWidth = 360,
            HostHeight = 220,
            HostMaxWidth = 500
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            presenter.HostWindow.CanMaximize.ShouldBeFalse();

            dialog.HostMaxWidth = double.PositiveInfinity;
            Dispatcher.UIThread.RunJobs();
            dialog.IsMaximizable.ShouldBeTrue();
            dialog.HostMaxWidth.ShouldBe(double.PositiveInfinity);
            dialog.HostMaxHeight.ShouldBe(double.PositiveInfinity);
            presenter.HostWindow.CanMaximize.ShouldBeTrue();

            dialog.HostMaxHeight = 320;
            Dispatcher.UIThread.RunJobs();
            presenter.HostWindow.CanMaximize.ShouldBeFalse();

            dialog.HostMaxHeight = double.PositiveInfinity;
            Dispatcher.UIThread.RunJobs();
            presenter.HostWindow.CanMaximize.ShouldBeTrue();
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Default_Window_Constraints_Protect_The_Surface_Structure()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220,
            StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var structuralMinimum = surface.MeasureStructuralMinimum();

            presenter.HostWindow.MinWidth.ShouldBeGreaterThanOrEqualTo(
                structuralMinimum.Width + chrome.Width);
            presenter.HostWindow.MinHeight.ShouldBe(structuralMinimum.Height + chrome.Height);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Runtime_NaN_And_Valid_Constraint_Changes_Preserve_Actual_Surface_Size()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);

            dialog.HostWidth = 420;
            dialog.HostHeight = 260;
            Dispatcher.UIThread.RunJobs();
            surface.Bounds.Size.ShouldBe(new Size(420, 260));

            dialog.HostWidth = double.NaN;
            dialog.HostHeight = double.NaN;
            dialog.HostMinWidth = 300;
            dialog.HostMinHeight = 180;
            dialog.HostMaxWidth = 500;
            dialog.HostMaxHeight = 320;
            Dispatcher.UIThread.RunJobs();

            surface.Bounds.Size.ShouldBe(new Size(420, 260));
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Runtime_Constraint_Changes_Clamp_Only_Out_Of_Range_Surface_Axes()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);

            var resizedWindowSize = new Size(420, 260) + chrome;
            ResizeHostWindowAsNativeUser(presenter.HostWindow, resizedWindowSize);
            surface.Bounds.Size.ShouldBe(new Size(420, 260));

            dialog.HostMinWidth = 300;
            dialog.HostMinHeight = 180;
            dialog.HostMaxWidth = 500;
            dialog.HostMaxHeight = 320;
            Dispatcher.UIThread.RunJobs();
            surface.Bounds.Size.ShouldBe(new Size(420, 260));

            dialog.HostMaxWidth = 390;
            dialog.HostMinHeight = 280;
            Dispatcher.UIThread.RunJobs();
            surface.Bounds.Size.ShouldBe(new Size(390, 280));
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Maximize_Does_Not_Overwrite_The_Normal_Surface_Size_Used_By_Restore()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            IsMaximizable = true,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var workingArea = GetLogicalWorkingAreaSize(presenter.HostWindow, owner);

            var resizedWindowSize = new Size(420, 260) + chrome;
            ResizeHostWindowAsNativeUser(presenter.HostWindow, resizedWindowSize);
            surface.Bounds.Size.ShouldBe(new Size(420, 260));

            presenter.HostWindow.WindowState = WindowState.Maximized;
            presenter.HostWindow.ApplyRequestedSize(workingArea.Width, workingArea.Height);
            dialog.HostWidth = 450;
            dialog.HostHeight = double.NaN;
            Dispatcher.UIThread.RunJobs();

            presenter.HostWindow.WindowState = WindowState.Normal;
            Dispatcher.UIThread.RunJobs();

            surface.Bounds.Size.ShouldBe(new Size(450, 260));
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Window_Chrome_Changes_Preserve_Surface_Size_And_Update_Native_Constraints()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);

            presenter.HostWindow.Padding = new Thickness(7, 8, 9, 10);
            Dispatcher.UIThread.RunJobs();

            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var structuralMinimum = surface.MeasureStructuralMinimum();
            surface.Bounds.Size.ShouldBe(new Size(360, 220));
            presenter.HostWindow.ClientSize.ShouldBe(surface.Bounds.Size + chrome);
            presenter.HostWindow.MinWidth.ShouldBeGreaterThanOrEqualTo(
                structuralMinimum.Width + chrome.Width);
            presenter.HostWindow.MinHeight.ShouldBe(
                ResolveExpectedWindowMinHeight(presenter.HostWindow, structuralMinimum, chrome));
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Csd_Frame_Metrics_Preserve_Surface_Size_And_Update_Native_Constraints()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);

            presenter.HostWindow.IsCsdEnabled = true;
            presenter.HostWindow.FrameShadowThickness = new Thickness(11, 12, 13, 14);
            Dispatcher.UIThread.RunJobs();

            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var structuralMinimum = surface.MeasureStructuralMinimum();
            surface.Bounds.Size.ShouldBe(new Size(360, 220));
            presenter.HostWindow.ClientSize.ShouldBe(surface.Bounds.Size + chrome);
            presenter.HostWindow.MinWidth.ShouldBeGreaterThanOrEqualTo(
                structuralMinimum.Width + chrome.Width);
            presenter.HostWindow.MinHeight.ShouldBe(
                ResolveExpectedWindowMinHeight(presenter.HostWindow, structuralMinimum, chrome));
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Windows_Csd_Track_Size_Uses_Actual_Frame_Client_Delta()
    {
        var trackSize = WindowsCsdSizingHook.CalculateTrackSize(
            new Size(500.2, 320.1),
            new Size(15.5, 8.2),
            1.25);

        trackSize.ShouldBe((645, 411));
    }

    [Fact]
    public void Host_Size_Changes_Defer_Client_Resize_During_Native_User_Resize()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var initialClientSize = presenter.HostWindow.ClientSize;
            var chrome = GetWindowChromeSize(presenter.HostWindow);

            presenter.HostWindow.BeginNativeUserResize();
            dialog.HostHeight = 260;
            Dispatcher.UIThread.RunJobs();

            presenter.HostWindow.IsNativeUserResizeInProgress.ShouldBeTrue();
            presenter.HostWindow.ClientSize.ShouldBe(initialClientSize);

            presenter.HostWindow.CompleteNativeUserResize();
            Dispatcher.UIThread.RunJobs();

            presenter.HostWindow.IsNativeUserResizeInProgress.ShouldBeFalse();
            presenter.HostWindow.ClientSize.Height.ShouldBe(260 + chrome.Height);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Native_User_Client_Size_Changes_Update_The_Window_Dialog_Normal_Size()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            IsResizable = true,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var platformSurfaceSize = new Size(500, 280);

            presenter.HostWindow.BeginNativeUserResize();
            presenter.HostWindow.SetPlatformChromeClientSize(platformSurfaceSize + chrome);
            Dispatcher.UIThread.RunJobs();
            presenter.HostWindow.CompleteNativeUserResize();
            Dispatcher.UIThread.RunJobs();

            surface.Bounds.Size.ShouldBe(platformSurfaceSize);

            presenter.HostWindow.Padding = new Thickness(3, 4, 5, 6);
            Dispatcher.UIThread.RunJobs();

            var updatedChrome = GetWindowChromeSize(presenter.HostWindow);
            surface.Bounds.Size.ShouldBe(platformSurfaceSize);
            presenter.HostWindow.ClientSize.ShouldBe(platformSurfaceSize + updatedChrome);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Sizing_Refresh_Does_Not_Treat_Stale_Client_Size_As_User_Resize()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            IsResizable = true,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);
            var normalSurfaceSize = surface.Bounds.Size;
            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var staleSurfaceSize = normalSurfaceSize + new Size(120, 80);

            presenter.HostWindow.SetPlatformChromeClientSize(staleSurfaceSize + chrome);
            Dispatcher.UIThread.RunJobs();

            presenter.HostWindow.Padding = new Thickness(3, 4, 5, 6);
            Dispatcher.UIThread.RunJobs();

            var updatedChrome = GetWindowChromeSize(presenter.HostWindow);
            surface.Bounds.Size.ShouldBe(normalSurfaceSize);
            presenter.HostWindow.ClientSize.ShouldBe(normalSurfaceSize + updatedChrome);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Unspecified_Native_Window_Resize_Preserves_Surface_Size_After_Refresh()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);

            var resizedWindowSize = new Size(420, 260) + chrome;
            ResizeHostWindowFromPlatform(
                presenter.HostWindow,
                resizedWindowSize,
                WindowResizeReason.Unspecified);
            surface.Bounds.Size.ShouldBe(new Size(420, 260));

            dialog.Title = "Updated title";
            Dispatcher.UIThread.RunJobs();

            surface.Bounds.Size.ShouldBe(new Size(420, 260));
            presenter.HostWindow.ClientSize.ShouldBe(resizedWindowSize);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Csd_Constraints_Ignore_The_Managed_TitleBar_Minimum_Width()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Title = "Managed title bar",
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220,
            HostMaxWidth = 600
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            presenter.HostWindow.IsCsdEnabled = true;
            Dispatcher.UIThread.RunJobs();
            presenter.HostWindow.TitleBar.ShouldNotBeNull().MinWidth = 700;
            dialog.Title = "Updated managed title bar";
            Dispatcher.UIThread.RunJobs();

            var surface = GetSurface(presenter);
            var chrome = GetWindowChromeSize(presenter.HostWindow);
            var structuralMinimum = surface.MeasureStructuralMinimum();

            presenter.HostWindow.MinWidth.ShouldBe(structuralMinimum.Width + chrome.Width);
            presenter.HostWindow.MaxWidth.ShouldBe(600 + chrome.Width);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Positive_Infinity_Window_Maximum_Uses_Logical_Screen_Working_Area()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var workingArea = GetLogicalWorkingAreaSize(presenter.HostWindow, owner);
            presenter.HostWindow.MaxWidth.ShouldBe(workingArea.Width);
            presenter.HostWindow.MaxHeight.ShouldBe(workingArea.Height);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Presenter_Applies_Dialog_Startup_Placement_After_Initial_Measure()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600,
            Position = new PixelPoint(100, 80)
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 240,
            HostHeight = 160,
            HorizontalStartupLocation = DialogHorizontalAnchor.Right,
            VerticalStartupLocation = DialogVerticalAnchor.Bottom
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var ownerOrigin = new Point(
                owner.Position.X / owner.DesktopScaling,
                owner.Position.Y / owner.DesktopScaling);
            var offset = dialog.CalculatePlacementOffset(
                presenter.HostWindow.ClientSize,
                owner.ClientSize);
            var expected = new PixelPoint(
                (int)Math.Round((ownerOrigin.X + offset.X) * presenter.HostWindow.DesktopScaling),
                (int)Math.Round((ownerOrigin.Y + offset.Y) * presenter.HostWindow.DesktopScaling));

            presenter.HostWindow.Position.ShouldBe(expected);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Centered_Startup_Placement_Uses_Resolved_Window_Size_When_PreShow_Client_Size_Is_Stale()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600,
            Position = new PixelPoint(100, 80)
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220,
            HorizontalStartupLocation = DialogHorizontalAnchor.Center,
            VerticalStartupLocation = DialogVerticalAnchor.Center
        };
        var presenter = new WindowDialogPresenter(dialog, owner);
        var stalePreShowClientSize = new Size(40, 50);
        PixelPoint? openedPosition = null;
        presenter.HostWindow.Opened += (_, _) => openedPosition ??= presenter.HostWindow.Position;

        try
        {
            presenter.HostWindow.SetPlatformChromeClientSize(stalePreShowClientSize);
            presenter.HostWindow.ClientSize.ShouldBe(stalePreShowClientSize);
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var ownerOrigin = new Point(
                owner.Position.X / owner.DesktopScaling,
                owner.Position.Y / owner.DesktopScaling);
            var offset = dialog.CalculatePlacementOffset(
                presenter.HostWindow.ClientSize,
                owner.ClientSize);
            var expected = new PixelPoint(
                (int)Math.Round((ownerOrigin.X + offset.X) * presenter.HostWindow.DesktopScaling),
                (int)Math.Round((ownerOrigin.Y + offset.Y) * presenter.HostWindow.DesktopScaling));

            presenter.HostWindow.ClientSize.ShouldNotBe(stalePreShowClientSize);
            openedPosition.ShouldNotBeNull();
            openedPosition.Value.ShouldBe(expected);
            presenter.HostWindow.Position.ShouldBe(expected);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Presenter_Uses_Attached_Dialog_Resources_And_Releases_The_Resource_Parent()
    {
        var resourceKey = new object();
        var resourceValue = new object();
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
        dialog.Resources[resourceKey] = resourceValue;
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600,
            Content = dialog
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var surface = presenter.HostWindow.GetVisualDescendants()
                                   .OfType<DialogSurface>()
                                   .Single();
            ((ILogical)presenter.HostWindow).LogicalParent.ShouldBeSameAs(dialog);
            surface.TryFindResource(resourceKey, out var resolved).ShouldBeTrue();
            resolved.ShouldBeSameAs(resourceValue);

            var replacementValue = new object();
            dialog.Resources[resourceKey] = replacementValue;
            Dispatcher.UIThread.RunJobs();
            surface.TryFindResource(resourceKey, out resolved).ShouldBeTrue();
            resolved.ShouldBeSameAs(replacementValue);

            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());

            surface.TryFindResource(resourceKey, out _).ShouldBeFalse();
        }
        finally
        {
            owner.Close();
        }
    }

    [Fact]
    public void Owner_Close_Forces_A_Modal_Window_Dialog_To_Complete()
    {
        var placementTarget = new Border { Width = 100, Height = 40 };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            PlacementTarget = placementTarget,
            DialogHostType = DialogHostType.Window,
            IsModal = true,
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
        var root = new StackPanel
        {
            Children = { placementTarget, dialog }
        };
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600,
            Content = root
        };
        var opened = false;
        dialog.Opened += (_, _) => opened = true;

        try
        {
            owner.Show();
            var sessionTask = dialog.OpenAsync(TestContext.Current.CancellationToken);
            PumpUntil(() => opened);

            owner.Close();

            WaitWithDispatcherPump(sessionTask);
            dialog.IsOpen.ShouldBeFalse();
            owner.IsVisible.ShouldBeFalse();
        }
        finally
        {
            if (owner.IsVisible)
            {
                owner.Close();
            }
        }
    }

    [Fact]
    public void MacOs_Modal_Disabled_Owner_Input_Suppresses_Only_Redundant_Dialog_Activation()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Dialog/WindowHost/WindowDialogPresenter.cs"));

        var showDialogBranchStart = source.IndexOf(
            "HostWindow.ShowDialog(ownerWindow)",
            StringComparison.Ordinal);
        var macOsScopeStart = source.IndexOf(
            "CreateMacOsDisabledOwnerInputActivationScope(ownerWindow)",
            StringComparison.Ordinal);
        showDialogBranchStart.ShouldBeGreaterThanOrEqualTo(0);
        macOsScopeStart.ShouldBeGreaterThanOrEqualTo(0);
        macOsScopeStart.ShouldBeLessThan(showDialogBranchStart);

        source.ShouldContain("OperatingSystem.IsMacOS()");
        source.ShouldContain("GotInputWhenDisabled");
        source.ShouldContain("HostWindow.IsVisible");
        source.ShouldContain("HostWindow.IsActive");
        source.ShouldContain("return;");
        source.ShouldContain("originalCallback?.Invoke();");
        source.ShouldContain("ReleaseMacOsDisabledOwnerInputActivationScope");
    }

    [Fact]
    public void Window_Hosted_Surface_Leaves_Content_Inset_Below_The_Native_TitleBar()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = new Border { Width = 180, Height = 70 },
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var surface = GetSurface(presenter);
            var contentFrame = surface.GetVisualDescendants()
                                      .OfType<Border>()
                                      .Single(border => border.Name == "ContentFrame");
            var verticalPadding = GetThemeResource<double>(SharedTokenKind.PaddingContentVerticalLG);

            contentFrame.Padding.Top.ShouldBe(verticalPadding);
            contentFrame.Padding.Bottom.ShouldBe(verticalPadding);
            contentFrame.Padding.Left.ShouldBeGreaterThan(0);
            contentFrame.Padding.Right.ShouldBeGreaterThan(0);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Presenter_Uses_The_Shared_Surface_Inside_One_Native_Window()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Title = "Window dialog",
            Content = "Dialog content",
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 360,
            HostHeight = 220
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            presenter.HostWindow.IsVisible.ShouldBeTrue();
            var surface = presenter.HostWindow.GetVisualDescendants()
                                   .OfType<DialogSurface>()
                                   .Single();
            surface.Content.ShouldBe("Dialog content");
            surface.IsHeaderVisible.ShouldBeFalse();

            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());

            presenter.HostWindow.IsVisible.ShouldBeFalse();
        }
        finally
        {
            owner.Close();
        }
    }

    [Fact]
    public void Native_Window_Uses_Native_Resize_Without_The_Overlay_Resizer()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            IsResizable = true
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            presenter.HostWindow.CanResize.ShouldBeTrue();
            var surface = presenter.HostWindow.GetVisualDescendants()
                                   .OfType<DialogSurface>()
                                   .Single();
            surface.IsResizable.ShouldBeFalse();
            surface.Resizer.ShouldNotBeNull();
            surface.Resizer.IsVisible.ShouldBeFalse();

            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
        }
        finally
        {
            owner.Close();
        }
    }

    [Fact]
    public void Window_Surface_Does_Not_Render_The_Overlay_Shadow()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var shadowHost = presenter.HostWindow.GetVisualDescendants()
                                      .OfType<ShadowsAwareContainer>()
                                      .Single();

            shadowHost.BoxShadow.Count.ShouldBe(0);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Window_Surface_Leaves_Corner_Radius_To_The_Native_Window()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            var surface = GetSurface(presenter);

            surface.Classes.ShouldContain("window-hosted");
            surface.CornerRadius.ShouldBe(default);
        }
        finally
        {
            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
            owner.Close();
        }
    }

    [Fact]
    public void Native_Window_Is_The_Only_Owner_Of_The_Title_Icon()
    {
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var titleIcon = new InfoCircleOutlined();
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsModal = false,
            IsMotionEnabled = false,
            TitleIcon = titleIcon
        };
        var presenter = new WindowDialogPresenter(dialog, owner);

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

            presenter.HostWindow.Logo.ShouldBeSameAs(titleIcon);
            var surface = presenter.HostWindow.GetVisualDescendants()
                                   .OfType<DialogSurface>()
                                   .Single();
            surface.TitleIcon.ShouldBeNull();
            surface.Header.ShouldNotBeNull().Logo.ShouldBeNull();

            WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
            WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
        }
        finally
        {
            owner.Close();
        }
    }

    [Fact]
    public void Dispose_Releases_Content_When_Close_Fails()
    {
        var expectedException = new InvalidOperationException("window close failed");
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 800,
            Height = 600
        };
        var presenter = new WindowDialogPresenter(
            new AtomUI.Desktop.Controls.Dialog
            {
                IsModal = false,
                IsMotionEnabled = false
            },
            owner);
        EventHandler<Avalonia.Controls.WindowClosingEventArgs> failingHandler =
            (_, _) => throw expectedException;

        try
        {
            owner.Show();
            WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
            presenter.HostWindow.Closing += failingHandler;

            Should.Throw<InvalidOperationException>(() =>
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask())).ShouldBeSameAs(expectedException);
            Should.Throw<InvalidOperationException>(() =>
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask())).ShouldBeSameAs(expectedException);

            presenter.HostWindow.Content.ShouldBeNull();
        }
        finally
        {
            presenter.HostWindow.Closing -= failingHandler;
            if (presenter.HostWindow.IsVisible)
            {
                presenter.HostWindow.CloseFromPresenter();
            }
            owner.Close();
        }
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
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

    private static DialogSurface GetSurface(WindowDialogPresenter presenter)
    {
        return presenter.HostWindow.GetVisualDescendants()
                        .OfType<DialogSurface>()
                        .Single();
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static Size GetWindowChromeSize(DialogWindow window)
    {
        var surface = window.GetVisualDescendants()
                            .OfType<DialogSurface>()
                            .Single();
        return new Size(
            Math.Max(0, window.ClientSize.Width - surface.Bounds.Width),
            Math.Max(0, window.ClientSize.Height - surface.Bounds.Height));
    }

    private static void ResizeHostWindowAsNativeUser(DialogWindow window, Size clientSize)
    {
        window.BeginNativeUserResize();
        window.SetPlatformChromeClientSize(clientSize);
        Dispatcher.UIThread.RunJobs();
        window.CompleteNativeUserResize();
        Dispatcher.UIThread.RunJobs();
    }

    private static void ResizeHostWindowFromPlatform(
        DialogWindow window,
        Size clientSize,
        WindowResizeReason reason)
    {
        var platformImpl = window.PlatformImpl;
        platformImpl.ShouldNotBeNull();
        platformImpl.Resize(clientSize, reason);
        Dispatcher.UIThread.RunJobs();
    }

    private static double ResolveExpectedWindowMinHeight(
        DialogWindow window,
        Size structuralMinimum,
        Size chrome)
    {
        var expectedMinHeight = structuralMinimum.Height + chrome.Height;
        if (OperatingSystem.IsWindows() && window.IsCsdEnabled)
        {
            expectedMinHeight = Math.Max(
                expectedMinHeight,
                AtomUI.Desktop.Controls.Window.CalculateWindowsCsdMinimumHeight(window.TitleBarHeight));
        }

        return expectedMinHeight;
    }

    private static Size GetLogicalWorkingAreaSize(DialogWindow window, WindowBase owner)
    {
        var screen = window.Screens.ScreenFromWindow(window) ??
                     owner.Screens.ScreenFromWindow(owner) ??
                     window.Screens.Primary ??
                     owner.Screens.Primary;
        var resolvedScreen = screen.ShouldNotBeNull();
        return new Size(
            resolvedScreen.WorkingArea.Width / resolvedScreen.Scaling,
            resolvedScreen.WorkingArea.Height / resolvedScreen.Scaling);
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition() && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        condition().ShouldBeTrue();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

    private static AtomUI.Desktop.Controls.Dialog CreateAnimatedDialog()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = new Border { Width = 180, Height = 70 },
            IsModal = false,
            IsMotionEnabled = true,
            HostWidth = 360,
            HostHeight = 220
        };
        dialog.Resources[ControlTokenResourceKey.Global(
            DialogTokens.Identity,
            SharedTokenKind.MotionDurationMid)] = TimeSpan.FromMilliseconds(120);
        return dialog;
    }
}
