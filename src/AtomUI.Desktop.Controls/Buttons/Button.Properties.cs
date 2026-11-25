using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public partial class Button
{
    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> DisabledBorderBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(DisabledBorderBrush));

    internal IBrush? DisabledBorderBrush
    {
        get => GetValue(DisabledBorderBrushProperty);
        set => SetValue(DisabledBorderBrushProperty, value);
    }

    internal static readonly StyledProperty<Thickness> CirclePaddingProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(CirclePadding));

    internal Thickness CirclePadding
    {
        get => GetValue(CirclePaddingProperty);
        set => SetValue(CirclePaddingProperty, value);
    }

    internal static readonly StyledProperty<double> ContentFontSizeProperty =
        AvaloniaProperty.Register<Button, double>(nameof(ContentFontSize));

    internal double ContentFontSize
    {
        get => GetValue(ContentFontSizeProperty);
        set => SetValue(ContentFontSizeProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> DefaultDisabledBorderBrushProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(DefaultDisabledBorderBrush));

    internal IBrush? DefaultDisabledBorderBrush
    {
        get => GetValue(DefaultDisabledBorderBrushProperty);
        set => SetValue(DefaultDisabledBorderBrushProperty, value);
    }

    internal static readonly StyledProperty<FontWeight> DefaultFontWeightProperty =
        AvaloniaProperty.Register<Button, FontWeight>(nameof(DefaultFontWeight));

    internal FontWeight DefaultFontWeight
    {
        get => GetValue(DefaultFontWeightProperty);
        set => SetValue(DefaultFontWeightProperty, value);
    }

    internal static readonly StyledProperty<Thickness> IconEndMarginProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(IconEndMargin));

    internal Thickness IconEndMargin
    {
        get => GetValue(IconEndMarginProperty);
        set => SetValue(IconEndMarginProperty, value);
    }

    internal static readonly StyledProperty<Thickness> IconMarginProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(IconMargin));

    internal Thickness IconMargin
    {
        get => GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    internal static readonly StyledProperty<Thickness> IconOnlyPaddingProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(IconOnlyPadding));

    internal Thickness IconOnlyPadding
    {
        get => GetValue(IconOnlyPaddingProperty);
        set => SetValue(IconOnlyPaddingProperty, value);
    }

    internal static readonly StyledProperty<double> IconOnlySizeProperty =
        AvaloniaProperty.Register<Button, double>(nameof(IconOnlySize));

    internal double IconOnlySize
    {
        get => GetValue(IconOnlySizeProperty);
        set => SetValue(IconOnlySizeProperty, value);
    }

    #endregion
}
