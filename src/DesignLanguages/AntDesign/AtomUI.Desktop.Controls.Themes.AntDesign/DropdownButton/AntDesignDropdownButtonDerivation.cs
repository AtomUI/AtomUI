using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="DropdownButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignDropdownButtonDerivation : IControlDerivation<AntDesignToken, DropdownButtonToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<DropdownButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractButtonGeometryDerivation.DeriveShared(in context, global, token);
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = AntDesignAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = AntDesignAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = AntDesignAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ExtraContentMargin = new Thickness(token.ContentPadding.Left / 2, 0, 0, 0);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = AntDesignAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = AntDesignAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = AntDesignAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ExtraContentMarginSM = new Thickness(token.ContentPaddingSM.Left / 2, 0, 0, 0);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = AntDesignAbstractButtonGeometryDerivation.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = AntDesignAbstractButtonGeometryDerivation.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = AntDesignAbstractButtonGeometryDerivation.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.ExtraContentMarginLG = new Thickness(token.ContentPaddingLG.Left / 2, 0, 0, 0);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
        token.MarginToAnchor = global.UniformlyMarginXXS;
    }
}
