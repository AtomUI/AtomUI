using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ResetButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignResetButtonDerivation : IControlDerivation<AntDesignToken, ResetButtonToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ResetButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractStandardButtonDerivation.DeriveShared(in context, global, token);
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = AntDesignAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = AntDesignAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = AntDesignAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = AntDesignAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = AntDesignAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = AntDesignAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = AntDesignAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = AntDesignAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = AntDesignAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
    }
}
