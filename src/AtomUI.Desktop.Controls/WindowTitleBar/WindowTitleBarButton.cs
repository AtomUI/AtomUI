using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An icon button intended for application-owned actions in a window title bar add-on.
/// </summary>
public class WindowTitleBarButton : IconButton
{
    internal static readonly AttachedProperty<bool> IsWindowActiveProperty =
        WindowTitleBarHostContext.IsWindowActiveProperty.AddOwner<WindowTitleBarButton>(
            new StyledPropertyMetadata<bool>(true));

    internal static readonly AttachedProperty<bool> HostMotionEnabledProperty =
        WindowTitleBarHostContext.HostMotionEnabledProperty.AddOwner<WindowTitleBarButton>(
            new StyledPropertyMetadata<bool>(true));

    internal static readonly AttachedProperty<OsType> HostOsTypeProperty =
        WindowTitleBarHostContext.HostOsTypeProperty.AddOwner<WindowTitleBarButton>(
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
}
