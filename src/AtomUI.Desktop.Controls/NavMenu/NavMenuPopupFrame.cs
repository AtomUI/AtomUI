using System.Globalization;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

internal class NavMenuPopupFrame : Border
{
    public static readonly StyledProperty<bool> UsePopupMaxSizeTokenResourcesProperty =
        AvaloniaProperty.Register<NavMenuPopupFrame, bool>(nameof(UsePopupMaxSizeTokenResources));

    public bool UsePopupMaxSizeTokenResources
    {
        get => GetValue(UsePopupMaxSizeTokenResourcesProperty);
        set => SetValue(UsePopupMaxSizeTokenResourcesProperty, value);
    }

    private OwnerTokenSet? _sizeTokens;
    private double _popupMaxWidth = double.PositiveInfinity;
    private double _popupMaxHeight = double.PositiveInfinity;

    static NavMenuPopupFrame()
    {
        AffectsMeasure<NavMenuPopupFrame>(
            PaddingProperty,
            BorderThicknessProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != UsePopupMaxSizeTokenResourcesProperty)
        {
            return;
        }

        if (UsePopupMaxSizeTokenResources)
        {
            _sizeTokens = new OwnerTokenSet(this, 2, RefreshPopupSizeLimits);
            _sizeTokens.SetResourceKey(0, NavMenuTokenKind.MenuPopupMaxWidth);
            _sizeTokens.SetResourceKey(1, NavMenuTokenKind.MenuPopupMaxHeight);
        }
        else
        {
            _sizeTokens?.Dispose();
            _sizeTokens = null;
        }
        RefreshPopupSizeLimits();
    }

    private void RefreshPopupSizeLimits()
    {
        var width = ReadLengthResource(_sizeTokens?.GetValue(0), _popupMaxWidth, double.PositiveInfinity);
        var height = ReadLengthResource(_sizeTokens?.GetValue(1), _popupMaxHeight, double.PositiveInfinity);
        if (!_popupMaxWidth.Equals(width) || !_popupMaxHeight.Equals(height))
        {
            _popupMaxWidth = width;
            _popupMaxHeight = height;
            InvalidateMeasure();
        }
    }

    internal static double ReadLengthResource(object? value, double previous, double fallback)
    {
        if (value is double length)
        {
            return length;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : fallback;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return fallback;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child is not { } child)
        {
            return ResolveFrameSize(default, availableSize);
        }

        var chrome = GetChromeThickness();
        child.Measure(Deflate(new Size(double.PositiveInfinity, double.PositiveInfinity), chrome));

        var naturalSize = Inflate(child.DesiredSize, chrome);
        var frameSize   = ResolveFrameSize(naturalSize, availableSize);
        var childSize   = Deflate(frameSize, chrome);

        if (ShouldRemeasureChild(naturalSize, frameSize))
        {
            child.Measure(childSize);
            frameSize = ResolveFrameSize(Inflate(child.DesiredSize, chrome), availableSize);
        }

        return frameSize;
    }

    private Size ResolveFrameSize(Size desiredSize, Size availableSize)
    {
        var width = ResolveLength(
            desiredSize.Width,
            MinWidth,
            _popupMaxWidth,
            availableSize.Width);
        var height = ResolveLength(
            desiredSize.Height,
            MinHeight,
            _popupMaxHeight,
            availableSize.Height);

        return new Size(width, height);
    }

    private Thickness GetChromeThickness()
    {
        var padding         = Padding;
        var borderThickness = BorderThickness;

        return new Thickness(
            padding.Left + borderThickness.Left,
            padding.Top + borderThickness.Top,
            padding.Right + borderThickness.Right,
            padding.Bottom + borderThickness.Bottom);
    }

    private static bool ShouldRemeasureChild(Size naturalSize, Size frameSize)
    {
        return frameSize.Width < naturalSize.Width ||
               frameSize.Height < naturalSize.Height ||
               frameSize.Width > naturalSize.Width ||
               frameSize.Height > naturalSize.Height;
    }

    private static double ResolveLength(double desired, double min, double max, double available)
    {
        var value = NormalizeLength(desired, 0);
        value = Math.Max(value, NormalizeLength(min, 0));

        var effectiveMax = NormalizeLength(max, double.PositiveInfinity);
        if (double.IsFinite(available))
        {
            effectiveMax = Math.Min(effectiveMax, Math.Max(available, 0));
        }

        return Math.Min(value, effectiveMax);
    }

    private static Size Deflate(Size size, Thickness thickness)
    {
        return new Size(
            DeflateLength(size.Width, thickness.Left + thickness.Right),
            DeflateLength(size.Height, thickness.Top + thickness.Bottom));
    }

    private static Size Inflate(Size size, Thickness thickness)
    {
        return new Size(
            InflateLength(size.Width, thickness.Left + thickness.Right),
            InflateLength(size.Height, thickness.Top + thickness.Bottom));
    }

    private static double DeflateLength(double length, double amount)
    {
        return double.IsFinite(length) ? Math.Max(0, length - amount) : double.PositiveInfinity;
    }

    private static double InflateLength(double length, double amount)
    {
        return double.IsFinite(length) ? length + amount : double.PositiveInfinity;
    }

    private static double NormalizeLength(double value, double fallback)
    {
        return double.IsNaN(value) ? fallback : value;
    }
}
