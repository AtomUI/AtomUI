using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DropdownButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnDropdownButtonDerivation : IControlDerivation<ShadcnToken, DropdownButtonToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<DropdownButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnButtonFamily.DeriveGlobal(in context, global);
        ShadcnAbstractButtonGeometryDerivation.DeriveShared(in context, global, token);
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = ShadcnAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = ShadcnAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = ShadcnAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ExtraContentMargin = new Thickness(token.ContentPadding.Left / 2, 0, 0, 0);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = ShadcnAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = ShadcnAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = ShadcnAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ExtraContentMarginSM = new Thickness(token.ContentPaddingSM.Left / 2, 0, 0, 0);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = ShadcnAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = ShadcnAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = ShadcnAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.ExtraContentMarginLG = new Thickness(token.ContentPaddingLG.Left / 2, 0, 0, 0);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
        token.MarginToAnchor = global.UniformlyMarginXXS;
        ShadcnButtonFamily.DeriveToken(in context, global, token);
    }
}
