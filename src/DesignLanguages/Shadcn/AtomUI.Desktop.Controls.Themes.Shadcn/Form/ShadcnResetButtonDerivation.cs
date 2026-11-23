using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ResetButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnResetButtonDerivation : IControlDerivation<ShadcnToken, ResetButtonToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ResetButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnButtonFamily.DeriveGlobal(in context, global);
        ShadcnAbstractStandardButtonDerivation.DeriveShared(in context, global, token);
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = ShadcnAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = ShadcnAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = ShadcnAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = ShadcnAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = ShadcnAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = ShadcnAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = ShadcnAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = ShadcnAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = ShadcnAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
        ShadcnButtonFamily.DeriveToken(in context, global, token);
    }
}
