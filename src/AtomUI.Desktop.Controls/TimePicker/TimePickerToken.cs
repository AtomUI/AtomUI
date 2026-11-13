using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TimePickerToken : AbstractTimePickerToken
{

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(TimePickerTokenKind.ItemWidth))
        {
            ItemWidth = EffectiveGlobalToken.ControlHeightLG * 1.4;
        }
        if (!IsExplicit(TimePickerTokenKind.ItemHeight))
        {
            ItemHeight = EffectiveGlobalToken.ControlHeight - 4;
        }
        PeriodHostWidth = ItemWidth;
        ItemPadding = CalculateTimeItemPadding(ItemWidth, ItemHeight);
    }
}
