using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractButtonGeometryToken" /> layer.
/// </summary>
public static class ButtonGeometryTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the button geometry layer to <paramref name="token" />: the sizes and
    /// margins of the icon, the disabled border color, and the flags that switch off the recipe variants, the
    /// default shadow of outline-only buttons, dimming by opacity, the press movement and the underline of link
    /// buttons. It first runs <see cref="ButtonVariantTokenCalculator.Calculate" />, so the variant layer of the
    /// same Token is filled as well.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractButtonGeometryToken token)
    {
        ButtonVariantTokenCalculator.Calculate(in context, global, token);
        // Ant Design has no recipe: gray disabled colors, pressed colors instead of the 1px move, link labels
        // without an underline, and a default shadow that is not limited to the outlined variant.
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
    /// Returns the line height of button text of a font size: the line height multiple of
    /// <see cref="CalculatorUtils.CalculateLineHeight" /> times the font size, which is the font size plus 8.
    /// </summary>
    public static double CalculateContentLineHeight(double fontSize) => CalculatorUtils.CalculateLineHeight(fontSize) * fontSize;


    /// <summary>
    /// Returns the padding around the content of a button. Left and right get the horizontal content padding,
    /// which is 8 for the small size instead of the global value, less the border width. Top and bottom get half
    /// of what remains of the control height after the line height, less the border width and never below 0.
    /// </summary>
    /// <param name="token">
    /// Not read.
    /// </param>
    /// <param name="global">
    /// The global tokens the padding, the control heights and the border width come from.
    /// </param>
    /// <param name="lineHeight">
    /// The line height of the content, as <see cref="CalculateContentLineHeight" /> returns it.
    /// </param>
    /// <param name="size">
    /// The size of the button.
    /// </param>
    public static Thickness CalculateContentPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size)
    {
        var horizontal = size == CustomizableSizeType.Small ? 8 : global.PaddingContentHorizontal;
        var height = GetControlHeight(token, global, size);
        return new Thickness(horizontal - global.LineWidth,
            Math.Max((height - lineHeight) / 2 - global.LineWidth, 0));
    }


    /// <summary>
    /// Returns the padding of an icon-only button: the top and bottom values of
    /// <see cref="CalculateContentPadding" /> for the same arguments, on all four sides.
    /// </summary>
    public static Thickness CalculateIconPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size) =>
        new(Math.Max((GetControlHeight(token, global, size) - lineHeight) / 2 - global.LineWidth, 0));


    /// <summary>
    /// Returns the height of a button of a size: <c>ControlHeightSM</c> for the small size, <c>ControlHeightLG</c>
    /// for the large size and <c>ControlHeight</c> otherwise.
    /// </summary>
    /// <param name="token">
    /// Not read; the height comes from the global tokens only.
    /// </param>
    /// <param name="global">
    /// The global tokens the control heights come from.
    /// </param>
    /// <param name="size">
    /// The size of the button.
    /// </param>
    public static double GetControlHeight(AbstractButtonGeometryToken token, DesignToken global, CustomizableSizeType size) => size switch
    {
        CustomizableSizeType.Small => global.ControlHeightSM,
        CustomizableSizeType.Large => global.ControlHeightLG,
        _ => global.ControlHeight
    };
}
