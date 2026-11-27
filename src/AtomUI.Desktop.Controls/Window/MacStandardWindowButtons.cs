using System.Runtime.Versioning;
using AtomUI.Native;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Attached properties and a layout helper that position the standard window buttons of a
/// <see cref="Window" /> on macOS.
/// </summary>
[SupportedOSPlatform("macos")]
public class MacStandardWindowButtons : AvaloniaObject
{
    /// <summary>
    /// The Spacing attached property: the horizontal distance in points between the left edges of adjacent standard
    /// window buttons. Set it on the <see cref="Window" /> whose buttons are positioned; only that window reads the
    /// value and the value is not inherited. When no value is set on the window, it uses 20, which is also what the
    /// Window theme sets on macOS.
    /// </summary>
    public static readonly AttachedProperty<double> SpacingProperty =
        AvaloniaProperty.RegisterAttached<MacStandardWindowButtons, Window, double>("Spacing", 0);
    
    /// <summary>
    /// The OffsetX attached property: the X position of the close button, in points, measured from the left edge of
    /// the window. Set it on the <see cref="Window" /> whose buttons are positioned; only that window reads the
    /// value and the value is not inherited. When no value is set on the window, the buttons keep their original X
    /// position; the Window theme sets 15 on macOS.
    /// </summary>
    public static readonly AttachedProperty<double> OffsetXProperty =
        AvaloniaProperty.RegisterAttached<MacStandardWindowButtons, Window, double>("OffsetX", 0);
    
    /// <summary>
    /// Gets the Spacing attached property value of <paramref name="window" />, or 0 when nothing sets it.
    /// </summary>
    public static double GetSpacing(Window window)
    {
        return window.GetValue(SpacingProperty);
    }

    /// <summary>
    /// Sets the Spacing attached property value on <paramref name="window" /> as a local value, which overrides the
    /// value of the Window theme.
    /// </summary>
    public static void SetSpacing(Window window, double value)
    {
        window.SetValue(SpacingProperty, value);
    }
    
    /// <summary>
    /// Gets the OffsetX attached property value of <paramref name="window" />, or 0 when nothing sets it.
    /// </summary>
    public static double GetOffsetX(Window window)
    {
        return window.GetValue(OffsetXProperty);
    }

    /// <summary>
    /// Sets the OffsetX attached property value on <paramref name="window" /> as a local value, which overrides the
    /// value of the Window theme.
    /// </summary>
    public static void SetOffsetX(Window window, double value)
    {
        window.SetValue(OffsetXProperty, value);
    }

    /// <summary>
    /// Positions the three standard window buttons of <paramref name="window" /> in one row. A
    /// <see langword="null" /> argument is replaced as described for each parameter. Nothing is positioned while the
    /// window is in full screen or before the native buttons exist.
    /// </summary>
    /// <param name="window">
    /// The window whose standard window buttons are positioned.
    /// </param>
    /// <param name="height">
    /// The height of the strip in which the buttons are vertically centered when <paramref name="y" /> is
    /// <see langword="null" />. <see langword="null" /> uses the system title bar height, or 0 when it is unknown.
    /// </param>
    /// <param name="x">
    /// The X position of the close button, in points from the left window edge. <see langword="null" /> keeps the
    /// original X position of the buttons.
    /// </param>
    /// <param name="y">
    /// The Y position shared by all buttons, in Cocoa coordinates measured from the bottom of the window.
    /// <see langword="null" /> centers the buttons vertically in a strip of <paramref name="height" />, or keeps the
    /// original Y position when the center cannot be calculated.
    /// </param>
    /// <param name="spacing">
    /// The horizontal distance in points between the left edges of adjacent buttons. The default is 20.
    /// </param>
    public static void SetStandardWindowButtonsLayout(Window window, double? height, double? x, double? y, double spacing = 20)
    {
        var originPosition = window.GetStandardWindowButtonsOriginalPosition();
        var effectX = x ?? originPosition.X;
        var originTitleHeight = window.GetSystemTitleBarHeight();
        var effectHeight = height ?? originTitleHeight ?? 0;
        var effectY      = y ?? window.CalculateVerticalCenter(effectHeight) ?? originPosition.Y;
        window.SetStandardWindowButtonsLayout(effectX, effectY, spacing);
    }
}