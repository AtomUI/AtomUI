using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Presentation values from the terminal's Own Tokens. Each terminal theme (Button, SubmitButton, ResetButton) assigns
// them on the button; the button's own theme reads them there and in its template, so they do not inherit through
// the button's content.
public partial class Button
{
    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> ThemeBorderColorDisabledProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(ThemeBorderColorDisabled));

    internal IBrush? ThemeBorderColorDisabled
    {
        get => GetValue(ThemeBorderColorDisabledProperty);
        set => SetValue(ThemeBorderColorDisabledProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ThemeCirclePaddingProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(ThemeCirclePadding));

    internal Thickness ThemeCirclePadding
    {
        get => GetValue(ThemeCirclePaddingProperty);
        set => SetValue(ThemeCirclePaddingProperty, value);
    }

    internal static readonly StyledProperty<double> ThemeContentFontSizeProperty =
        AvaloniaProperty.Register<Button, double>(nameof(ThemeContentFontSize));

    internal double ThemeContentFontSize
    {
        get => GetValue(ThemeContentFontSizeProperty);
        set => SetValue(ThemeContentFontSizeProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ThemeContentPaddingProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(ThemeContentPadding));

    internal Thickness ThemeContentPadding
    {
        get => GetValue(ThemeContentPaddingProperty);
        set => SetValue(ThemeContentPaddingProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ThemeDefaultBorderColorDisabledProperty =
        AvaloniaProperty.Register<Button, IBrush?>(nameof(ThemeDefaultBorderColorDisabled));

    internal IBrush? ThemeDefaultBorderColorDisabled
    {
        get => GetValue(ThemeDefaultBorderColorDisabledProperty);
        set => SetValue(ThemeDefaultBorderColorDisabledProperty, value);
    }

    internal static readonly StyledProperty<FontWeight> ThemeFontWeightProperty =
        AvaloniaProperty.Register<Button, FontWeight>(nameof(ThemeFontWeight));

    internal FontWeight ThemeFontWeight
    {
        get => GetValue(ThemeFontWeightProperty);
        set => SetValue(ThemeFontWeightProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ThemeIconEndMarginProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(ThemeIconEndMargin));

    internal Thickness ThemeIconEndMargin
    {
        get => GetValue(ThemeIconEndMarginProperty);
        set => SetValue(ThemeIconEndMarginProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ThemeIconMarginProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(ThemeIconMargin));

    internal Thickness ThemeIconMargin
    {
        get => GetValue(ThemeIconMarginProperty);
        set => SetValue(ThemeIconMarginProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ThemeIconOnyPaddingProperty =
        AvaloniaProperty.Register<Button, Thickness>(nameof(ThemeIconOnyPadding));

    internal Thickness ThemeIconOnyPadding
    {
        get => GetValue(ThemeIconOnyPaddingProperty);
        set => SetValue(ThemeIconOnyPaddingProperty, value);
    }

    internal static readonly StyledProperty<double> ThemeOnlyIconSizeProperty =
        AvaloniaProperty.Register<Button, double>(nameof(ThemeOnlyIconSize));

    internal double ThemeOnlyIconSize
    {
        get => GetValue(ThemeOnlyIconSizeProperty);
        set => SetValue(ThemeOnlyIconSizeProperty, value);
    }

    #endregion
}
