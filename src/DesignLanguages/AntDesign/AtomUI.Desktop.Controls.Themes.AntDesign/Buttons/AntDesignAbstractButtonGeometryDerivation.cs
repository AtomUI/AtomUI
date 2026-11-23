using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractButtonGeometryToken"/> layer.</summary>
public static class AntDesignAbstractButtonGeometryDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractButtonGeometryToken token)
    {
        AntDesignAbstractButtonVariantDerivation.DeriveShared(in context, global, token);
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


    public static double CalculateContentLineHeight(double fontSize) => CalculatorUtils.CalculateLineHeight(fontSize) * fontSize;


    public static Thickness CalculateContentPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size)
    {
        var horizontal = size == CustomizableSizeType.Small ? 8 : global.PaddingContentHorizontal;
        var height = GetControlHeight(token, global, size);
        return new Thickness(horizontal - global.LineWidth,
            Math.Max((height - lineHeight) / 2 - global.LineWidth, 0));
    }


    public static Thickness CalculateIconPadding(AbstractButtonGeometryToken token, DesignToken global, double lineHeight, CustomizableSizeType size) =>
        new(Math.Max((GetControlHeight(token, global, size) - lineHeight) / 2 - global.LineWidth, 0));


    public static double GetControlHeight(AbstractButtonGeometryToken token, DesignToken global, CustomizableSizeType size) => size switch
    {
        CustomizableSizeType.Small => global.ControlHeightSM,
        CustomizableSizeType.Large => global.ControlHeightLG,
        _ => global.ControlHeight
    };
}
