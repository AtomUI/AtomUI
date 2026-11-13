using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RangeTimePickerToken : AbstractTimePickerToken
{
    public partial Thickness RangePickerArrowMargin { get; set; }
    public partial double RangePickerIndicatorThickness { get; set; }
    public partial double RangeMarginToAnchor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(RangeTimePickerTokenKind.ItemWidth))
        {
            ItemWidth = EffectiveGlobalToken.ControlHeightLG * 1.4;
        }
        if (!IsExplicit(RangeTimePickerTokenKind.ItemHeight))
        {
            ItemHeight = EffectiveGlobalToken.ControlHeight - 4;
        }
        PeriodHostWidth = ItemWidth;
        ItemPadding = CalculateTimeItemPadding(ItemWidth, ItemHeight);
        var range = RangePickerInputTokenValues.Calculate(EffectiveGlobalToken);
        RangePickerArrowMargin = range.ArrowMargin;
        RangePickerIndicatorThickness = range.IndicatorThickness;
        RangeMarginToAnchor = EffectiveGlobalToken.UniformlyMarginXS;
    }
}
