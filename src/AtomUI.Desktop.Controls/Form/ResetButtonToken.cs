using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ResetButtonToken : AbstractStandardButtonToken
{

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(ResetButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(ResetButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculateContentLineHeight(ContentFontSize);
        }
        if (!IsExplicit(ResetButtonTokenKind.ContentPadding))
        {
            ContentPadding = CalculateContentPadding(ContentLineHeight, CustomizableSizeType.Middle);
        }
        IconOnyPadding = CalculateIconPadding(ContentLineHeight, CustomizableSizeType.Middle);
        if (!IsExplicit(ResetButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(ResetButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculateContentLineHeight(ContentFontSizeSM);
        }
        if (!IsExplicit(ResetButtonTokenKind.ContentPaddingSM))
        {
            ContentPaddingSM = CalculateContentPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        }
        IconOnyPaddingSM = CalculateIconPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        if (!IsExplicit(ResetButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        }
        if (!IsExplicit(ResetButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculateContentLineHeight(ContentFontSizeLG);
        }
        if (!IsExplicit(ResetButtonTokenKind.ContentPaddingLG))
        {
            ContentPaddingLG = CalculateContentPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        }
        IconOnyPaddingLG = CalculateIconPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        CirclePadding = new Thickness(ContentPaddingSM.Left / 2);
    }
}
