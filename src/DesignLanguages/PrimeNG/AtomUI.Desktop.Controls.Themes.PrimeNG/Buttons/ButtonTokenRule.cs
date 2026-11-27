using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ButtonTokenRule : IControlTokenRule<PrimeNGToken, ButtonToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (ColorBgSolid by the formulas below; BorderRadiusSM, BorderRadiusLG,
        // OpacityLoading by the Button theme (CornerRadius, Opacity); the other six by neither).
        var colors = PrimeNGButtonFamilyTokens<ButtonToken>.EvaluateGlobal(in context, global);
        StandardButtonTokenCalculator.Calculate(in context, global, token);
        token.GroupBorderColor = global.ColorPrimaryHover;
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGButtonFamilyTokens<ButtonToken>.EvaluateOwn(in context, global, token, colors);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.IconOnyPaddingSM = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
    }
}
