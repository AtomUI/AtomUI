using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// An explicitly set global Alias Token drives the Alias Tokens derived from it: <c>ControlOutlineWidth</c> is the spread
/// of the focused input's outline (<c>InputActiveShadow</c>) when it is set on the root. (A control-level Alias override
/// replaces only that value and does not re-derive the aliases computed from it, spec 7.5.) The outline is painted
/// outside the input frame, so the comparison is the ring around the frame.
/// </summary>
public partial class InputFamilyEffectTests
{
    private const int DefaultOutlineWidth = 2;

    [Theory]
    [InlineData("Root", 0)]
    [InlineData("Root", 6)]
    public void ControlOutlineWidth_Drives_The_Focused_Input_Outline(string scope, int width)
    {
        var lineEdit = new LineEdit { Width = 260, Text = SampleText };
        WithControls([lineEdit], (provider, capture) =>
        {
            var frame = FrameOf(lineEdit);
            using var unfocused = capture();
            frame.IsInputFocusWithin = true;
            using var focused = capture();

            provider.Config = OutlineWidthConfig(scope, width);
            using var after = capture();

            var ring = Math.Max(DefaultOutlineWidth, width) + 1;
            ChangedRingPixels(focused, after, frame, ring).ShouldBeGreaterThan(0,
                $"{scope} ControlOutlineWidth = {width} did not change the focused outline.");
            if (width == 0)
            {
                ChangedRingPixels(unfocused, after, frame, ring).ShouldBe(0,
                    $"{scope} ControlOutlineWidth = 0 still paints a focused outline.");
            }
        });
    }

    private static ThemeConfig OutlineWidthConfig(string scope, int width)
    {
        var value   = width.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var builder = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        return scope switch
        {
            "Root" => builder.WithToken(nameof(SharedTokenKind.ControlOutlineWidth), value).Build(),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        };
    }

    /// <summary>Pixels that differ in the band of <paramref name="ring"/> pixels around the control's bounds.</summary>
    private static int ChangedRingPixels(SKBitmap before, SKBitmap after, Control control, int ring)
    {
        var window = (Visual)TopLevel.GetTopLevel(control)!;
        var origin = control.TranslatePoint(new Point(), window).ShouldNotBeNull();
        var inner = new SKRectI(
            (int)Math.Floor(origin.X),
            (int)Math.Floor(origin.Y),
            (int)Math.Ceiling(origin.X + control.Bounds.Width),
            (int)Math.Ceiling(origin.Y + control.Bounds.Height));
        var outer = SKRectI.Inflate(inner, ring, ring);
        var tolerance = TokenEffectHarness.DefaultChannelTolerance;
        return FrameCapture.CountDifferentPixels(before, after, outer, tolerance) -
               FrameCapture.CountDifferentPixels(before, after, inner, tolerance);
    }
}
