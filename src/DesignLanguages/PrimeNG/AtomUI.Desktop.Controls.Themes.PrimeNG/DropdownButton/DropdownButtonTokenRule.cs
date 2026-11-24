using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DropdownButtonToken"/>.</summary>
[ControlTokenRule]
internal sealed class DropdownButtonTokenRule : IControlTokenRule<PrimeNGToken, DropdownButtonToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<DropdownButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        var colors = PrimeNGButtonFamilyTokens<DropdownButtonToken>.EvaluateGlobal(in context, global);
        ButtonGeometryTokenCalculator.Calculate(in context, global, token);
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ExtraContentMargin = new Thickness(token.ContentPadding.Left / 2, 0, 0, 0);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ExtraContentMarginSM = new Thickness(token.ContentPaddingSM.Left / 2, 0, 0, 0);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.ExtraContentMarginLG = new Thickness(token.ContentPaddingLG.Left / 2, 0, 0, 0);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
        token.MarginToAnchor = global.UniformlyMarginXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGButtonFamilyTokens<DropdownButtonToken>.EvaluateOwn(in context, global, token, colors);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.ExtraContentMargin = new Thickness(token.ContentPadding.Left / 2, 0, 0, 0);
        token.IconOnyPaddingSM = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ExtraContentMarginSM = new Thickness(token.ContentPaddingSM.Left / 2, 0, 0, 0);
        token.ExtraContentMarginLG = new Thickness(token.ContentPaddingLG.Left / 2, 0, 0, 0);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
    }
}
