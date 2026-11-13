using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DropdownButtonToken : AbstractButtonGeometryToken
{
    public partial Thickness ExtraContentMargin { get; set; }
    public partial Thickness ExtraContentMarginLG { get; set; }
    public partial Thickness ExtraContentMarginSM { get; set; }
    public partial double MarginToAnchor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(DropdownButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(DropdownButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculateContentLineHeight(ContentFontSize);
        }
        if (!IsExplicit(DropdownButtonTokenKind.ContentPadding))
        {
            ContentPadding = CalculateContentPadding(ContentLineHeight, CustomizableSizeType.Middle);
        }
        IconOnyPadding = CalculateIconPadding(ContentLineHeight, CustomizableSizeType.Middle);
        ExtraContentMargin = new Thickness(ContentPadding.Left / 2, 0, 0, 0);
        if (!IsExplicit(DropdownButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(DropdownButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculateContentLineHeight(ContentFontSizeSM);
        }
        if (!IsExplicit(DropdownButtonTokenKind.ContentPaddingSM))
        {
            ContentPaddingSM = CalculateContentPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        }
        IconOnyPaddingSM = CalculateIconPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        ExtraContentMarginSM = new Thickness(ContentPaddingSM.Left / 2, 0, 0, 0);
        if (!IsExplicit(DropdownButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        }
        if (!IsExplicit(DropdownButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculateContentLineHeight(ContentFontSizeLG);
        }
        if (!IsExplicit(DropdownButtonTokenKind.ContentPaddingLG))
        {
            ContentPaddingLG = CalculateContentPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        }
        IconOnyPaddingLG = CalculateIconPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        ExtraContentMarginLG = new Thickness(ContentPaddingLG.Left / 2, 0, 0, 0);
        CirclePadding = new Thickness(ContentPaddingSM.Left / 2);
        MarginToAnchor = EffectiveGlobalToken.UniformlyMarginXXS;
    }
}
