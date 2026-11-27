using Avalonia;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A checked/unchecked icon button intended for application-owned actions in a window title bar add-on.
/// </summary>
public class WindowTitleBarToggleButton : ToggleIconButton
{
    internal static readonly AttachedProperty<bool> IsWindowActiveProperty =
        WindowTitleBarHostContext.IsWindowActiveProperty.AddOwner<WindowTitleBarToggleButton>(
            new StyledPropertyMetadata<bool>(true));

    internal static readonly AttachedProperty<bool> HostMotionEnabledProperty =
        WindowTitleBarHostContext.HostMotionEnabledProperty.AddOwner<WindowTitleBarToggleButton>(
            new StyledPropertyMetadata<bool>(true));

    internal static readonly AttachedProperty<OsType> HostOsTypeProperty =
        WindowTitleBarHostContext.HostOsTypeProperty.AddOwner<WindowTitleBarToggleButton>(
            new StyledPropertyMetadata<OsType>(OsType.Unknown));

    internal bool IsWindowActive
    {
        get => GetValue(IsWindowActiveProperty);
        set => SetValue(IsWindowActiveProperty, value);
    }

    internal bool HostMotionEnabled
    {
        get => GetValue(HostMotionEnabledProperty);
        set => SetValue(HostMotionEnabledProperty, value);
    }

    internal OsType HostOsType
    {
        get => GetValue(HostOsTypeProperty);
        set => SetValue(HostOsTypeProperty, value);
    }

    /// <summary>
    /// Measures the content and returns a square desired size whose side is the shorter side of the measured content.
    /// When <see cref="HostOsType" /> is <c>OsType.Windows</c>, the side is the larger of that value and the shorter
    /// side of the available size, with an infinite available width taken as the available height.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (HostOsType == OsType.Windows)
        {
            var measureConstraint = WindowsCaptionButtonLayout.NormalizeMeasureConstraint(availableSize);
            var measuredSize = base.MeasureOverride(measureConstraint);
            return WindowsCaptionButtonLayout.ResolveDesiredSize(measureConstraint, measuredSize);
        }

        var size = base.MeasureOverride(availableSize);
        var side = Math.Min(size.Width, size.Height);
        return new Size(side, side);
    }

    /// <summary>
    /// Processes the press with the base handler and then marks the event as handled, so that the title bar hosting
    /// the button ignores it.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true;
    }

    /// <summary>
    /// Processes the release with the base handler and then marks the event as handled, so that the title bar
    /// hosting the button ignores it.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Handled = true;
    }

    /// <summary>
    /// Processes the pointer movement with the base handler and then marks the event as handled, so that the title bar
    /// hosting the button ignores it.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        e.Handled = true;
    }
}
