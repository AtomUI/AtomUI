using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SubmitButtonToken : AbstractStandardButtonToken
{

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(SubmitButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(SubmitButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculateContentLineHeight(ContentFontSize);
        }
        if (!IsExplicit(SubmitButtonTokenKind.ContentPadding))
        {
            ContentPadding = CalculateContentPadding(ContentLineHeight, CustomizableSizeType.Middle);
        }
        IconOnyPadding = CalculateIconPadding(ContentLineHeight, CustomizableSizeType.Middle);
        if (!IsExplicit(SubmitButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = EffectiveGlobalToken.FontSize;
        }
        if (!IsExplicit(SubmitButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculateContentLineHeight(ContentFontSizeSM);
        }
        if (!IsExplicit(SubmitButtonTokenKind.ContentPaddingSM))
        {
            ContentPaddingSM = CalculateContentPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        }
        IconOnyPaddingSM = CalculateIconPadding(ContentLineHeightSM, CustomizableSizeType.Small);
        if (!IsExplicit(SubmitButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        }
        if (!IsExplicit(SubmitButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculateContentLineHeight(ContentFontSizeLG);
        }
        if (!IsExplicit(SubmitButtonTokenKind.ContentPaddingLG))
        {
            ContentPaddingLG = CalculateContentPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        }
        IconOnyPaddingLG = CalculateIconPadding(ContentLineHeightLG, CustomizableSizeType.Large);
        CirclePadding = new Thickness(ContentPaddingSM.Left / 2);
    }
}
