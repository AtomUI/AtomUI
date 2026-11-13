using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DatePickerToken : AbstractDatePickerToken
{

    public partial Thickness HeaderBorderThickness { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderBorderThickness = new Thickness(0, 0, 0, EffectiveGlobalToken.LineWidth);
        if (!IsExplicit(DatePickerTokenKind.ItemWidth))
        {
            ItemWidth = EffectiveGlobalToken.ControlHeightLG * 1.4;
        }
        if (!IsExplicit(DatePickerTokenKind.ItemHeight))
        {
            ItemHeight = EffectiveGlobalToken.ControlHeight - 4;
        }
        PeriodHostWidth = ItemWidth;
        ItemPadding = CalculateTimeItemPadding(ItemWidth, ItemHeight);
    }
}
