using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RangeDatePickerToken : AbstractDatePickerToken
{
    public partial Thickness RangePickerArrowMargin { get; set; }
    public partial double RangePickerIndicatorThickness { get; set; }
    public partial double RangePanelSpacing { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(RangeDatePickerTokenKind.ItemWidth))
        {
            ItemWidth = EffectiveGlobalToken.ControlHeightLG * 1.4;
        }
        if (!IsExplicit(RangeDatePickerTokenKind.ItemHeight))
        {
            ItemHeight = EffectiveGlobalToken.ControlHeight - 4;
        }
        PeriodHostWidth = ItemWidth;
        ItemPadding = CalculateTimeItemPadding(ItemWidth, ItemHeight);
        var range = RangePickerInputTokenValues.Calculate(EffectiveGlobalToken);
        RangePickerArrowMargin = range.ArrowMargin;
        RangePickerIndicatorThickness = range.IndicatorThickness;
        RangePanelSpacing = 20;
    }
}
