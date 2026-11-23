using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DropdownButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDropdownButtonDerivation : IControlDerivation<PrimeNGToken, DropdownButtonToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DropdownButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        var colors = PrimeNGButtonFamilyTokens<DropdownButtonToken>.EvaluateGlobal(in context, global);
        PrimeNGAbstractButtonGeometryDerivation.DeriveShared(in context, global, token);
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = PrimeNGAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = PrimeNGAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = PrimeNGAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ExtraContentMargin = new Thickness(token.ContentPadding.Left / 2, 0, 0, 0);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = PrimeNGAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = PrimeNGAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = PrimeNGAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ExtraContentMarginSM = new Thickness(token.ContentPaddingSM.Left / 2, 0, 0, 0);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = PrimeNGAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = PrimeNGAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = PrimeNGAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.ExtraContentMarginLG = new Thickness(token.ContentPaddingLG.Left / 2, 0, 0, 0);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
        token.MarginToAnchor = global.UniformlyMarginXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGButtonFamilyTokens<DropdownButtonToken>.EvaluateOwn(in context, global, token, colors);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.ExtraContentMargin = new Thickness(token.ContentPadding.Left / 2, 0, 0, 0);
        token.IconOnyPaddingSM = PrimeNGAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ExtraContentMarginSM = new Thickness(token.ContentPaddingSM.Left / 2, 0, 0, 0);
        token.ExtraContentMarginLG = new Thickness(token.ContentPaddingLG.Left / 2, 0, 0, 0);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
    }
}
