using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class RangeTimePickerToken : AbstractTimePickerToken
{
    public Thickness RangePickerArrowMargin { get; set; }
    public double RangePickerIndicatorThickness { get; set; }
    public double RangeMarginToAnchor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
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
