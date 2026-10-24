using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Primitives;

public class DashedBorderClipContentTests
{
    static DashedBorderClipContentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Clip_Content_To_Corner_Radius_Clips_The_Child_To_The_Inner_Border_Edge()
    {
        var border = new GeometryHitTestCapableBorder
        {
            Width                     = 200,
            Height                    = 100,
            BorderThickness           = new Thickness(1),
            CornerRadius              = new CornerRadius(8),
            ClipContentToCornerRadius = true,
            Child                     = new Border { Background = Brushes.Red }
        };

        ShowInWindow(border, () =>
        {
            var clip = border.Child!.Clip;
            clip.ShouldNotBeNull();

            // The child is inset by the border thickness, so the ring's inner edge in
            // child space is the child's own bounds — the clip must not shrink them.
            AssertRectClose(clip!.Bounds, new Rect(0, 0, 198, 98));
        });
    }

    [Fact]
    public void Clip_Is_Not_Applied_When_The_Platform_Cannot_Hit_Test_The_Rounded_Figure()
    {
        var border = new GeometryHitTestIncapableBorder
        {
            Width                     = 200,
            Height                    = 100,
            BorderThickness           = new Thickness(1),
            CornerRadius              = new CornerRadius(8),
            ClipContentToCornerRadius = true,
            Child                     = new Border { Background = Brushes.Red }
        };

        ShowInWindow(border, () =>
        {
            // Exercise the capability fallback independently of the active renderer.
            // Rounded geometry must not block input when containment is unsupported.
            border.Child!.Clip.ShouldBeNull();
        });
    }

    [Fact]
    public void Disabling_Clip_Content_To_Corner_Radius_Clears_The_Managed_Clip()
    {
        var border = new GeometryHitTestCapableBorder
        {
            Width                     = 200,
            Height                    = 100,
            BorderThickness           = new Thickness(1),
            CornerRadius              = new CornerRadius(8),
            ClipContentToCornerRadius = true,
            Child                     = new Border { Background = Brushes.Red }
        };

        ShowInWindow(border, () =>
        {
            border.Child!.Clip.ShouldNotBeNull();

            border.ClipContentToCornerRadius = false;

            border.Child.Clip.ShouldBeNull();
        });
    }

    [Fact]
    public void Disabled_Clip_Content_To_Corner_Radius_Leaves_The_Child_Clip_Untouched()
    {
        var border = new DashedBorder
        {
            Width           = 200,
            Height          = 100,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(8),
            Child           = new Border { Background = Brushes.Red }
        };
        var originalClip = new RectangleGeometry(new Rect(4, 4, 64, 64));
        border.Child.Clip = originalClip;

        ShowInWindow(border, () =>
        {
            border.Child.Clip.ShouldBeSameAs(originalClip);
        });
    }

    [Fact]
    public void Removing_Child_Releases_The_Managed_Clip()
    {
        var child = new Border { Background = Brushes.Red };
        var border = new GeometryHitTestCapableBorder
        {
            Width = 200,
            Height = 100,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipContentToCornerRadius = true,
            Child = child
        };

        ShowInWindow(border, () =>
        {
            child.Clip.ShouldNotBeNull();
            border.Child = null;
            child.Clip.ShouldBeNull();
        });
    }

    [Fact]
    public void Replacing_Child_Releases_The_Previous_Managed_Clip()
    {
        var oldChild = new Border { Background = Brushes.Red };
        var newChild = new Border { Background = Brushes.Blue };
        var border = new GeometryHitTestCapableBorder
        {
            Width = 200,
            Height = 100,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipContentToCornerRadius = true,
            Child = oldChild
        };

        ShowInWindow(border, () =>
        {
            oldChild.Clip.ShouldNotBeNull();
            border.Child = newChild;
            Dispatcher.UIThread.RunJobs();
            newChild.Clip.ShouldNotBeNull();
            oldChild.Clip.ShouldBeNull();
        });
    }

    [Fact]
    public void Disabling_Clip_Does_Not_Clear_A_Clip_Replaced_After_Management()
    {
        var child = new Border { Background = Brushes.Red };
        var border = new GeometryHitTestCapableBorder
        {
            Width = 200,
            Height = 100,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipContentToCornerRadius = true,
            Child = child
        };
        var externalClip = new RectangleGeometry(new Rect(2, 3, 40, 50));

        ShowInWindow(border, () =>
        {
            child.Clip.ShouldNotBeNull();
            child.Clip = externalClip;
            border.ClipContentToCornerRadius = false;
            child.Clip.ShouldBeSameAs(externalClip);
        });
    }

    private sealed class GeometryHitTestCapableBorder : DashedBorder
    {
        protected override bool SupportsGeometryClipHitTesting(Geometry figure) => true;
    }

    private sealed class GeometryHitTestIncapableBorder : DashedBorder
    {
        protected override bool SupportsGeometryClipHitTesting(Geometry figure) => false;
    }

    private static void AssertRectClose(Rect actual, Rect expected)
    {
        const double tolerance = 0.01;
        Math.Abs(actual.X - expected.X).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Y - expected.Y).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Width - expected.Width).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Height - expected.Height).ShouldBeLessThan(tolerance);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 300,
            Height  = 200,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
