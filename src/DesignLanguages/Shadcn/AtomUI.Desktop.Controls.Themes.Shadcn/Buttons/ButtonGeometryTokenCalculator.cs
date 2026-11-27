using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractButtonGeometryToken" /> layer.
/// </summary>
public static class ButtonGeometryTokenCalculator
{
    /// <summary>
    /// Fills the <see cref="AbstractButtonGeometryToken" /> layer with the shadcn values.
    /// <see cref="ButtonVariantTokenCalculator.Calculate" /> fills the variant layer first; then the shared behavior
    /// switches are turned off (recipe variants, default shadow of the outline-only style, dimming of disabled
    /// buttons, movement on press, link underline) and the icon sizes and margins follow the global tokens.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractButtonGeometryToken token)
    {
        ButtonVariantTokenCalculator.Calculate(in context, global, token);
        token.RecipeVariants           = false;
        token.OutlineOnlyDefaultShadow = false;
        token.DisabledByOpacity        = false;
        token.DisabledOpacity          = 1.0;
        token.PressMoves               = false;
        token.UnderlineLinkOnHover     = false;
        token.UnderlineLinkAtRest      = false;
        token.BorderColorDisabled = global.ColorBorder;
        token.OnlyIconSizeSM = global.IconSize;
        token.OnlyIconSize = global.IconSizeLG;
        token.OnlyIconSizeLG = global.IconSizeLG;
        token.IconMargin = new Thickness(0, 0, global.UniformlyPaddingXXS, 0);
        token.IconEndMargin = new Thickness(global.UniformlyPaddingXXS, 0, 0, 0);
    }


    /// <summary>
    /// Provides the line height, in device-independent pixels, of button text of the font size
    /// <paramref name="fontSize" />.
    /// </summary>
    public static double CalculateContentLineHeight(double fontSize) => CalculatorUtils.CalculateLineHeight(fontSize) * fontSize;


    /// <summary>
    /// Provides the content padding of a button of the size <paramref name="size" />. The horizontal padding is the
    /// global <c>PaddingContentHorizontal</c> less the line width, or a fixed 8 less the line width for the small
    /// size. The vertical padding is half of the space the control height leaves around
    /// <paramref name="lineHeight" />, less the line width and never below 0, so pass the content line height of
    /// the same size.
    /// </summary>
    public static Thickness CalculateContentPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size)
    {
        var horizontal = size == CustomizableSizeType.Small ? 8 : global.PaddingContentHorizontal;
        var height = GetControlHeight(token, global, size);
        return new Thickness(horizontal - global.LineWidth,
            Math.Max((height - lineHeight) / 2 - global.LineWidth, 0));
    }


    /// <summary>
    /// Provides the padding of an icon-only button of the size <paramref name="size" />, the same on all four sides:
    /// half of the space the control height leaves around <paramref name="lineHeight" />, less the line width and
    /// never below 0.
    /// </summary>
    public static Thickness CalculateIconPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size) =>
        new(Math.Max((GetControlHeight(token, global, size) - lineHeight) / 2 - global.LineWidth, 0));


    /// <summary>
    /// Provides the global control height of the size: <c>ControlHeightSM</c> for the small size,
    /// <c>ControlHeightLG</c> for the large size and <c>ControlHeight</c> for the middle size. The
    /// <paramref name="token" /> is not read.
    /// </summary>
    public static double GetControlHeight(AbstractButtonGeometryToken token, DesignToken global, CustomizableSizeType size) => size switch
    {
        CustomizableSizeType.Small => global.ControlHeightSM,
        CustomizableSizeType.Large => global.ControlHeightLG,
        _ => global.ControlHeight
    };
}
