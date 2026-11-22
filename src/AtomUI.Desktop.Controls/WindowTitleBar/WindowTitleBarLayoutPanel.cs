using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal enum WindowTitleBarLayoutRole
{
    None,
    Leading,
    Title,
    Trailing
}

internal sealed class WindowTitleBarLayoutPanel : Panel
{
    public static readonly StyledProperty<WindowTitleBarTitleAlignment> TitleAlignmentProperty =
        WindowTitleBar.TitleAlignmentProperty.AddOwner<WindowTitleBarLayoutPanel>();

    public static readonly StyledProperty<OsType> OsTypeProperty =
        OperationSystemAwareControlProperty.OsTypeProperty.AddOwner<WindowTitleBarLayoutPanel>();

    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<WindowTitleBarLayoutPanel, Thickness>(nameof(Padding));

    public static readonly StyledProperty<Thickness> NativeChromeInsetsProperty =
        AvaloniaProperty.Register<WindowTitleBarLayoutPanel, Thickness>(nameof(NativeChromeInsets));

    public static readonly StyledProperty<bool> IsCsdEnabledProperty =
        AvaloniaProperty.Register<WindowTitleBarLayoutPanel, bool>(nameof(IsCsdEnabled));

    public static readonly StyledProperty<WindowState> WindowStateProperty =
        AvaloniaProperty.Register<WindowTitleBarLayoutPanel, WindowState>(nameof(WindowState));

    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        AvaloniaProperty.Register<WindowTitleBarLayoutPanel, double>(nameof(HorizontalSpacing));

    public static readonly AttachedProperty<WindowTitleBarLayoutRole> RoleProperty =
        AvaloniaProperty.RegisterAttached<WindowTitleBarLayoutPanel, Control, WindowTitleBarLayoutRole>("Role");

    public WindowTitleBarTitleAlignment TitleAlignment
    {
        get => GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public OsType OsType
    {
        get => GetValue(OsTypeProperty);
        set => SetValue(OsTypeProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public Thickness NativeChromeInsets
    {
        get => GetValue(NativeChromeInsetsProperty);
        set => SetValue(NativeChromeInsetsProperty, value);
    }

    public bool IsCsdEnabled
    {
        get => GetValue(IsCsdEnabledProperty);
        set => SetValue(IsCsdEnabledProperty, value);
    }

    public WindowState WindowState
    {
        get => GetValue(WindowStateProperty);
        set => SetValue(WindowStateProperty, value);
    }

    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    static WindowTitleBarLayoutPanel()
    {
        AffectsMeasure<WindowTitleBarLayoutPanel>(
            TitleAlignmentProperty,
            OsTypeProperty,
            PaddingProperty,
            NativeChromeInsetsProperty,
            IsCsdEnabledProperty,
            WindowStateProperty,
            HorizontalSpacingProperty);
        AffectsArrange<WindowTitleBarLayoutPanel>(
            TitleAlignmentProperty,
            OsTypeProperty,
            PaddingProperty,
            NativeChromeInsetsProperty,
            IsCsdEnabledProperty,
            WindowStateProperty,
            HorizontalSpacingProperty);
        AffectsParentMeasure<WindowTitleBarLayoutPanel>(RoleProperty);
        AffectsParentArrange<WindowTitleBarLayoutPanel>(RoleProperty);
    }

    public static void SetRole(Control element, WindowTitleBarLayoutRole value)
    {
        element.SetValue(RoleProperty, value);
    }

    public static WindowTitleBarLayoutRole GetRole(Control element)
    {
        return element.GetValue(RoleProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var leading  = FindChild(WindowTitleBarLayoutRole.Leading);
        var title    = FindChild(WindowTitleBarLayoutRole.Title);
        var trailing = FindChild(WindowTitleBarLayoutRole.Trailing);
        var verticalPadding = Normalize(Padding.Top) + Normalize(Padding.Bottom);
        var contentHeight = double.IsFinite(availableSize.Height)
            ? Math.Max(0, availableSize.Height - verticalPadding)
            : double.PositiveInfinity;
        var operationConstraint = new Size(double.PositiveInfinity, contentHeight);

        leading?.Measure(operationConstraint);
        trailing?.Measure(operationConstraint);

        var leadingWidth  = GetDesiredWidth(leading);
        var trailingWidth = GetDesiredWidth(trailing);
        var titleAvailableWidth = double.IsFinite(availableSize.Width)
            ? CalculateTitleAvailableWidth(availableSize.Width, leadingWidth, trailingWidth)
            : double.PositiveInfinity;
        title?.Measure(new Size(titleAvailableWidth, contentHeight));

        var titleWidth = GetDesiredWidth(title);
        var spacing = Normalize(HorizontalSpacing);
        var leadingOccupation = leadingWidth > 0 ? leadingWidth + spacing : 0;
        var trailingOccupation = trailingWidth > 0 ? trailingWidth + spacing : 0;
        var nativeChromeInsets = ResolveNativeChromeInsets(availableSize.Width);
        var desiredWidth = Normalize(nativeChromeInsets.Left) + Normalize(Padding.Left) +
                           leadingOccupation + titleWidth + trailingOccupation +
                           Normalize(Padding.Right) + Normalize(nativeChromeInsets.Right);
        var desiredHeight = Math.Max(
            GetDesiredHeight(title),
            Math.Max(GetDesiredHeight(leading), GetDesiredHeight(trailing))) + verticalPadding;

        return new Size(
            double.IsFinite(availableSize.Width) ? Math.Min(availableSize.Width, desiredWidth) : desiredWidth,
            double.IsFinite(availableSize.Height) ? Math.Min(availableSize.Height, desiredHeight) : desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var leading  = FindChild(WindowTitleBarLayoutRole.Leading);
        var title    = FindChild(WindowTitleBarLayoutRole.Title);
        var trailing = FindChild(WindowTitleBarLayoutRole.Trailing);
        var width = Normalize(finalSize.Width);
        var top = Math.Min(Normalize(Padding.Top), Normalize(finalSize.Height));
        var bottom = Math.Min(Normalize(Padding.Bottom), Math.Max(0, Normalize(finalSize.Height) - top));
        var contentHeight = Math.Max(0, Normalize(finalSize.Height) - top - bottom);
        var nativeChromeInsets = ResolveNativeChromeInsets(width);
        var leftBoundary = Math.Clamp(
            Normalize(nativeChromeInsets.Left) + Normalize(Padding.Left),
            0,
            width);
        var rightBoundary = Math.Clamp(
            width - Normalize(nativeChromeInsets.Right) - Normalize(Padding.Right),
            0,
            width);
        var leadingWidth = GetDesiredWidth(leading);
        var trailingWidth = GetDesiredWidth(trailing);

        leading?.Arrange(new Rect(leftBoundary, top, leadingWidth, contentHeight));
        trailing?.Arrange(new Rect(rightBoundary - trailingWidth, top, trailingWidth, contentHeight));

        if (title is not null)
        {
            var titleRect = CalculateTitleRect(width, leadingWidth, trailingWidth, GetDesiredWidth(title));
            title.Arrange(new Rect(titleRect.X, top, titleRect.Width, contentHeight));
        }

        return finalSize;
    }

    private double CalculateTitleAvailableWidth(double width, double leadingWidth, double trailingWidth)
    {
        var bounds = CalculateTitleBounds(width, leadingWidth, trailingWidth);
        if (ResolveAlignment() != WindowTitleBarTitleAlignment.WindowCenter)
        {
            return bounds.AvailableWidth;
        }

        var center = Normalize(width) / 2;
        return 2 * Math.Max(0, Math.Min(center - bounds.Left, bounds.Right - center));
    }

    private Rect CalculateTitleRect(double width, double leadingWidth, double trailingWidth, double titleWidth)
    {
        var bounds = CalculateTitleBounds(width, leadingWidth, trailingWidth);
        var requestedWidth = Normalize(titleWidth);
        var alignment = ResolveAlignment();

        if (alignment == WindowTitleBarTitleAlignment.WindowCenter)
        {
            var center = Normalize(width) / 2;
            var halfWidth = Math.Max(0, Math.Min(center - bounds.Left, bounds.Right - center));
            var arrangedWidth = Math.Min(requestedWidth, 2 * halfWidth);
            return new Rect(center - arrangedWidth / 2, 0, arrangedWidth, 0);
        }

        var effectiveWidth = Math.Min(requestedWidth, bounds.AvailableWidth);
        var x = alignment switch
        {
            WindowTitleBarTitleAlignment.Center => bounds.Left + (bounds.AvailableWidth - effectiveWidth) / 2,
            WindowTitleBarTitleAlignment.Right => bounds.Right - effectiveWidth,
            _ => bounds.Left
        };
        return new Rect(x, 0, effectiveWidth, 0);
    }

    private (double Left, double Right, double AvailableWidth) CalculateTitleBounds(
        double width,
        double leadingWidth,
        double trailingWidth)
    {
        var normalizedWidth = Normalize(width);
        var spacing = Normalize(HorizontalSpacing);
        var nativeChromeInsets = ResolveNativeChromeInsets(normalizedWidth);
        var leftBoundary = Math.Clamp(
            Normalize(nativeChromeInsets.Left) + Normalize(Padding.Left),
            0,
            normalizedWidth);
        var rightBoundary = Math.Clamp(
            normalizedWidth - Normalize(nativeChromeInsets.Right) - Normalize(Padding.Right),
            0,
            normalizedWidth);
        var leadingOccupation = leadingWidth > 0 ? leadingWidth + spacing : 0;
        var trailingOccupation = trailingWidth > 0 ? trailingWidth + spacing : 0;
        var left = Math.Clamp(leftBoundary + leadingOccupation, 0, normalizedWidth);
        var right = Math.Clamp(rightBoundary - trailingOccupation, 0, normalizedWidth);
        return (left, right, Math.Max(0, right - left));
    }

    private WindowTitleBarTitleAlignment ResolveAlignment()
    {
        return TitleAlignment == WindowTitleBarTitleAlignment.Auto
            ? WindowTitleBarLayoutStrategies.Get(OsType).AutoAlignment
            : TitleAlignment;
    }

    private Thickness ResolveNativeChromeInsets(double width)
    {
        return WindowTitleBarLayoutStrategies.Get(OsType)
                                             .ResolveNativeChromeInsets(
                                                 width,
                                                 NativeChromeInsets,
                                                 IsCsdEnabled);
    }

    private Control? FindChild(WindowTitleBarLayoutRole role)
    {
        return Children.FirstOrDefault(child => child.IsVisible && GetRole(child) == role);
    }

    private static double GetDesiredWidth(Control? control)
    {
        return Normalize(control?.DesiredSize.Width ?? 0);
    }

    private static double GetDesiredHeight(Control? control)
    {
        return Normalize(control?.DesiredSize.Height ?? 0);
    }

    private static double Normalize(double value)
    {
        return double.IsFinite(value) && value > 0 ? value : 0;
    }
}
