using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractPopupCornerToken : AbstractControlDesignToken
{
    public partial CornerRadius PopupCornerRadius { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PopupCornerRadius = EffectiveGlobalToken.BorderRadiusLG;
    }
}
