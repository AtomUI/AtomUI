using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ButtonToken : AbstractStandardButtonToken
{

    [ReservedToken("Compact-space primary buttons do not draw a group separator yet.")]
    public Color GroupBorderColor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GroupBorderColor = EffectiveGlobalToken.ColorPrimaryHover;
        if (!IsExplicit(ButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(ButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculateContentLineHeight(ContentFontSize);
        }
        if (!IsExplicit(ButtonTokenKind.ContentPadding))
        {
            ContentPadding = CalculateContentPadding(ContentLineHeight, CustomizableSizeType.Middle);
        }
        IconOnyPadding = CalculateIconPadding(ContentLineHeight, CustomizableSizeType.Middle);
        if (!IsExplicit(ButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(ButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculateContentLineHeight(ContentFontSizeSM);
        }
        if (!IsExplicit(ButtonTokenKind.ContentPaddingSM))
        {
            ContentPaddingSM = CalculateContentPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        }
        IconOnyPaddingSM = CalculateIconPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        if (!IsExplicit(ButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        }
        if (!IsExplicit(ButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculateContentLineHeight(ContentFontSizeLG);
        }
        if (!IsExplicit(ButtonTokenKind.ContentPaddingLG))
        {
            ContentPaddingLG = CalculateContentPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        }
        IconOnyPaddingLG = CalculateIconPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        CirclePadding = new Thickness(ContentPaddingSM.Left / 2);
    }
}
