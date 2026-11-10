using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractPopupCornerToken : AbstractControlDesignToken
{
    public CornerRadius PopupCornerRadius { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PopupCornerRadius = EffectiveGlobalToken.BorderRadiusLG;
    }
}
