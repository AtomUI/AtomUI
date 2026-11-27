using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractButtonGeometryToken" /> layer.
/// </summary>
public static class ButtonGeometryTokenCalculator
{
    /// <summary>
    /// Calls <c>ButtonVariantTokenCalculator.Calculate</c>, which fills the variant layer as well, and then writes
    /// Ant Design's look with the recipe off into the shared <see cref="AbstractButtonGeometryToken" /> layer: no
    /// recipe variants, no shadow on the outline-only default button, no dimming by opacity when disabled, no
    /// movement on press and no underline on link buttons; the disabled border takes <c>ColorBorder</c>, and the
    /// icon-only sizes and the gaps around a leading and a trailing icon are set. For the Button family
    /// <c>PrimeNGButtonFamilyTokens</c> then writes the PrimeUIX recipe over three of the flags.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractButtonGeometryToken token)
    {
        ButtonVariantTokenCalculator.Calculate(in context, global, token);
        // Ant Design's look with the recipe off: gray disabled colors, pressed colors instead of the 1px move,
        // link labels without an underline. The PrimeNG button stage (PrimeNGButtonFamilyTokens) then writes the
        // PrimeUIX recipe over three of the flags for the Button family: dimming to 0.6 when disabled and the
        // underline of link labels on hover. The rest already match PrimeUIX: no recipe variants, no press
        // movement, no resting shadow.
        token.RecipeVariants           = false;
        token.OutlineOnlyDefaultShadow = false;
        token.DisabledByOpacity        = false;
        token.DisabledOpacity          = 1.0;
        token.PressMoves               = false;
        token.UnderlineLinkOnHover     = false;
        token.UnderlineLinkAtRest      = false;
        // Ant Design formula kept as is: the button stage replaces the two icon gaps (8) but not the icon-only
        // sizes or the disabled border, which keep the Ant Design formula over the PrimeNG global tokens.
        token.BorderColorDisabled = global.ColorBorder;
        token.OnlyIconSizeSM = global.IconSize;
        token.OnlyIconSize = global.IconSizeLG;
        token.OnlyIconSizeLG = global.IconSizeLG;
        token.IconMargin = new Thickness(0, 0, global.UniformlyPaddingXXS, 0);
        token.IconEndMargin = new Thickness(global.UniformlyPaddingXXS, 0, 0, 0);
    }


    /// <summary>
    /// Returns the height of one line of button text, in DIPs: <paramref name="fontSize" /> times the relative line
    /// height of that size (<c>CalculatorUtils.CalculateLineHeight</c>).
    /// </summary>
    public static double CalculateContentLineHeight(double fontSize) => CalculatorUtils.CalculateLineHeight(fontSize) * fontSize;


    /// <summary>
    /// Returns the padding of a button's content for <paramref name="size" />: the horizontal padding minus one border
    /// width on the left and right, and vertically half of what the control height leaves after the text line and the
    /// borders, but never less than 0. The small size takes a fixed horizontal padding of 8 instead of
    /// <c>PaddingContentHorizontal</c>.
    /// </summary>
    public static Thickness CalculateContentPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size)
    {
        var horizontal = size == CustomizableSizeType.Small ? 8 : global.PaddingContentHorizontal;
        var height = GetControlHeight(token, global, size);
        return new Thickness(horizontal - global.LineWidth,
            Math.Max((height - lineHeight) / 2 - global.LineWidth, 0));
    }


    /// <summary>
    /// Returns the padding of an icon-only button for <paramref name="size" />: the vertical padding of
    /// <see cref="CalculateContentPadding" /> on all four sides.
    /// </summary>
    public static Thickness CalculateIconPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size) =>
        new(Math.Max((GetControlHeight(token, global, size) - lineHeight) / 2 - global.LineWidth, 0));


    /// <summary>
    /// Returns the control height of a button size: <c>ControlHeightSM</c> for the small,
    /// <c>ControlHeightLG</c> for the large and <c>ControlHeight</c> for the middle size.
    /// </summary>
    public static double GetControlHeight(AbstractButtonGeometryToken token, DesignToken global, CustomizableSizeType size) => size switch
    {
        CustomizableSizeType.Small => global.ControlHeightSM,
        CustomizableSizeType.Large => global.ControlHeightLG,
        _ => global.ControlHeight
    };
}
