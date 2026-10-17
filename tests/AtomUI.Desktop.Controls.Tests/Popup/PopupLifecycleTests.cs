using Avalonia.Headless;
using Avalonia;
using Avalonia.Input;
using AtomUI.MotionScene;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIPopup = AtomUI.Desktop.Controls.Popup;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Popups;

public class PopupLifecycleTests
{
    static PopupLifecycleTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pinned_Open_Request_Opens_Valid_Popup()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Open_Popup_Starts_Open_Motion_When_Actor_Becomes_Ready_After_Opened()
    {
        var (window, _, _, popup) = CreatePopupWindow();
        popup.OpenMotion     = new FadeInMotion();
        popup.MotionDuration = TimeSpan.Zero;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            // A lazily materialized popup can raise Opened before its host template
            // attaches PopupMotionActor. The actor pre-hides itself to prevent a
            // flash, so the late-ready path must still complete the opening motion.
            var lateActor = new PopupMotionActor { Opacity = 0.0d };
            popup.NotifyMotionActorReady(lateActor);
            Dispatcher.UIThread.RunJobs();

            lateActor.Opacity.ShouldBe(1.0d);
        }
        finally
        {
            CloseWindow(window, popup);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pinning_An_Open_Popup_Updates_Only_Its_Mask_Without_Reopening(bool initiallyPinned)
    {
        var (window, _, popup) = CreateDismissPopupWindow();
        popup.IsMotionEnabled = false;
        popup.IsLightDismissEnabled = true;
        popup.IsPopupPinnedOpen = initiallyPinned;
        var opened = 0;
        var closed = 0;
        popup.Opened += (_, _) => opened++;
        popup.Closed += (_, _) => closed++;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);
            var host = window.GetVisualDescendants().OfType<OverlayPopupHost>().Single();
            var child = popup.Child;
            for (var i = 0; i < 3; i++)
            {
                popup.IsPopupPinnedOpen = true;
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                popup.IsLightDismissEnabled.ShouldBeFalse();
                window.InputHitTest(new Point(340, 290))?.GetType().Name.ShouldNotBe("LightDismissOverlayLayer");

                popup.IsPopupPinnedOpen = false;
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                popup.IsLightDismissEnabled.ShouldBeTrue();
                window.InputHitTest(new Point(340, 290))?.GetType().Name.ShouldBe("LightDismissOverlayLayer");
                window.GetVisualDescendants().OfType<OverlayPopupHost>().Single().ShouldBeSameAs(host);
                popup.Child.ShouldBeSameAs(child);
                popup.IsOpen.ShouldBeTrue();
                opened.ShouldBe(1);
                closed.ShouldBe(0);
            }
            // The restored registration must route real outside input to light-dismiss.
            window.MouseDown(new Point(340, 290), MouseButton.Left);
            window.MouseUp(new Point(340, 290), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();
            closed.ShouldBe(1);
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinning_One_Popup_Does_Not_Remove_Another_Popups_Mask_And_Restores_Latest_Config()
    {
        var (window, target, popup) = CreateDismissPopupWindow();
        var other = CreatePopup(target);
        popup.IsMotionEnabled = other.IsMotionEnabled = false;
        popup.IsLightDismissEnabled = other.IsLightDismissEnabled = true;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);
            OpenPopup(other);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var dismissLayer = window.GetVisualDescendants()
                                     .Single(layer => layer.GetType().Name == "LightDismissOverlayLayer");
            popup.IsLightDismissEnabled.ShouldBeFalse();
            other.IsLightDismissEnabled.ShouldBeTrue();
            dismissLayer.IsVisible.ShouldBeTrue();

            window.MouseDown(new Point(340, 290), MouseButton.Left);
            window.MouseUp(new Point(340, 290), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            other.IsOpen.ShouldBeFalse();
            dismissLayer.IsVisible.ShouldBeFalse();

            popup.IsLightDismissEnabled = false;
            popup.IsPopupPinnedOpen = false;
            popup.IsLightDismissEnabled.ShouldBeFalse();
            dismissLayer.IsVisible.ShouldBeFalse();
        }
        finally
        {
            other.CloseForLifecycle();
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinned_Popup_Rejects_Normal_Close_Without_Starting_Close_Motion()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();
        var closeMotion = new CountingFadeOutMotion();
        popup.CloseMotion = closeMotion;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            popup.Close();
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            closeMotion.ConfigurationCount.ShouldBe(0);
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinned_Popup_Closes_Immediately_When_PlacementTarget_Detaches()
    {
        var (window, panel, target, popup) = CreateAnimatedPopupWindow();
        var closedCount = 0;
        popup.Closed += (_, _) => closedCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            panel.Children.Remove(target);
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeFalse();
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            closedCount.ShouldBe(1);
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinned_Open_Request_Waits_For_PlacementTarget_To_Attach()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();

        try
        {
            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeFalse();

            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinned_Open_Request_Waits_For_PlacementTarget_To_Become_Visible()
    {
        var (window, _, target, popup) = CreateAnimatedPopupWindow();
        target.IsVisible = false;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();

            target.IsVisible = true;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinned_Open_Request_Waits_For_PlacementTarget_To_Become_Enabled()
    {
        var (window, _, target, popup) = CreateAnimatedPopupWindow();
        target.IsEnabled = false;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();

            target.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinned_Explicit_Open_Waits_For_PlacementTarget_To_Become_Enabled()
    {
        var (window, _, target, popup) = CreateAnimatedPopupWindow();
        target.IsEnabled = false;
        var openedCount = 0;
        popup.Opened += (_, _) => openedCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen = true;
            popup.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();
            openedCount.ShouldBe(0);

            target.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            openedCount.ShouldBe(1);
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Unpinning_A_Pending_Explicit_Open_Clears_The_Open_Request()
    {
        var (window, _, target, popup) = CreateAnimatedPopupWindow();
        target.IsEnabled = false;
        var openedCount = 0;
        popup.Opened += (_, _) => openedCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen = true;
            popup.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();
            openedCount.ShouldBe(0);

            popup.IsPopupPinnedOpen = false;
            target.IsEnabled = true;
            popup.CoerceValue(Popup.IsOpenProperty);
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeFalse();
            openedCount.ShouldBe(0);
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinned_Open_Request_Waits_For_Content_To_Become_Available()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();
        var child = popup.Child;
        popup.Child = null;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();

            popup.Child = child;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Lifecycle_Close_Bypasses_Pin_And_Close_Motion()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            popup.CloseForLifecycle();
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeFalse();
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Pinning_During_Close_Motion_Cancels_The_Pending_Close()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            popup.Close();
            Dispatcher.UIThread.RunJobs();
            popup.IsPlayingCloseMotion.ShouldBeTrue();

            popup.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            popup.IsPopupPinnedOpen = false;
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void PlacementTarget_Detach_ForceCloses_Animated_Popup()
    {
        var (window, panel, target, popup) = CreateAnimatedPopupWindow();
        var closedCount = 0;
        popup.Closed += (_, _) => closedCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            panel.Children.Remove(target);
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeFalse();
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            closedCount.ShouldBe(1);
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();

            popup.MarginToAnchor     = 8;
            popup.RequestedPlacement = PlacementMode.Top;
            popup.Placement          = PlacementMode.Bottom;
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void LogicalOwner_Detach_ForceCloses_Animated_Popup()
    {
        var (window, panel, _, popup) = CreateAnimatedPopupWindow();
        var closedCount = 0;
        popup.Closed += (_, _) => closedCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            panel.Children.Remove(popup);
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeFalse();
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            closedCount.ShouldBe(1);
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();
        }
        finally
        {
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void PlacementTarget_Change_To_Another_TopLevel_ForceCloses_Animated_Popup()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();
        var otherTarget = new Border
        {
            Width  = 80,
            Height = 24
        };
        var otherWindow = new AtomUIWindow
        {
            Width   = 240,
            Height  = 160,
            Content = otherTarget
        };

        try
        {
            window.Show();
            otherWindow.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            popup.PlacementTarget = otherTarget;
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeFalse();
            popup.IsPlayingCloseMotion.ShouldBeFalse();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();
        }
        finally
        {
            CloseWindow(window, popup);
            otherWindow.Close();
        }
    }

    [Fact]
    public void Normal_Close_Still_Uses_Close_Motion()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();
        var closedCount = 0;
        popup.Closed += (_, _) => closedCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            popup.Close();
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            popup.IsPlayingCloseMotion.ShouldBeTrue();
            closedCount.ShouldBe(0);
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Direct_Popup_Without_Logical_Owner_Still_Uses_Close_Motion()
    {
        var target = new Border
        {
            Width  = 100,
            Height = 30
        };
        var popup = CreateAnimatedPopup(target);
        var window = new AtomUIWindow
        {
            Width   = 360,
            Height  = 320,
            Content = target
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            popup.Close();
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue();
            popup.IsPlayingCloseMotion.ShouldBeTrue();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            CloseWindow(window, popup);
        }
    }

    [Fact]
    public void Repeated_Close_Coalesces_Into_One_Close_Motion()
    {
        var (window, _, _, popup) = CreateAnimatedPopupWindow();
        var closeMotion = new CountingFadeOutMotion();
        popup.CloseMotion = closeMotion;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            OpenPopup(popup);

            popup.Close();
            popup.Close();
            Dispatcher.UIThread.RunJobs();

            popup.IsPlayingCloseMotion.ShouldBeTrue();
            closeMotion.ConfigurationCount.ShouldBe(1);
        }
        finally
        {
            CloseWindow(window, popup);
        }
    }

    private static (Avalonia.Controls.Window Window, Control Target, AtomUIPopup Popup) CreateDismissPopupWindow()
    {
        var target = new Border { Width = 100, Height = 30 };
        var popup = CreatePopup(target);
        var canvas = new Canvas { Children = { target, popup } };
        var panel = new AtomUI.Controls.Primitives.ScopeAwareOverlayLayerPanel
        {
            Width = 360, Height = 320, Children = { canvas }
        };
        var layers = new VisualLayerManager { Child = panel };
        typeof(VisualLayerManager).GetProperty("EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(layers, true);
        return (new Avalonia.Controls.Window { Width = 360, Height = 320, Content = layers }, target, popup);
    }

    private static (AtomUIWindow Window, Panel Panel, Control Target, AtomUIPopup Popup)
        CreateAnimatedPopupWindow()
    {
        var result = CreatePopupWindow();
        result.Popup.NotifyMotionActorReady(new PopupMotionActor());
        return result;
    }

    private static (AtomUIWindow Window, Panel Panel, Control Target, AtomUIPopup Popup)
        CreatePopupWindow()
    {
        var target = new Border
        {
            Width  = 100,
            Height = 30
        };
        var popup = CreatePopup(target);

        var panel = new Canvas();
        panel.Children.Add(target);
        panel.Children.Add(popup);

        var window = new AtomUIWindow
        {
            Width   = 360,
            Height  = 320,
            Content = panel
        };
        return (window, panel, target, popup);
    }

    private static AtomUIPopup CreateAnimatedPopup(Control target)
    {
        var popup = CreatePopup(target);
        popup.NotifyMotionActorReady(new PopupMotionActor());
        return popup;
    }

    private static AtomUIPopup CreatePopup(Control target)
    {
        var popup = new AtomUIPopup
        {
            PlacementTarget       = target,
            RequestedPlacement    = PlacementMode.Bottom,
            ShouldUseOverlayLayer = true,
            IsMotionEnabled       = true,
            CloseMotion           = new FadeOutMotion(),
            MotionDuration        = TimeSpan.FromSeconds(2),
            Child = new Border
            {
                Width  = 120,
                Height = 40
            }
        };
        return popup;
    }

    private static void OpenPopup(AtomUIPopup popup)
    {
        popup.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        popup.IsOpen.ShouldBeTrue();
    }

    private static void CloseWindow(Avalonia.Controls.Window window, AtomUIPopup popup)
    {
        popup.CancelCloseAnimation();
        popup.IsMotionEnabled = false;
        popup.IsOpen          = false;
        window.Close();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class CountingFadeOutMotion : FadeOutMotion
    {
        public int ConfigurationCount { get; private set; }

        protected override void ConfigureTransitions()
        {
            ConfigurationCount++;
            base.ConfigureTransitions();
        }
    }
}
