using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class MessageBoxToken : AbstractDialogToken
{
    public double StyleIconSize { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(MessageBoxTokenKind.HeaderPadding))
        {
            HeaderPadding = CalculateHeaderPadding();
        }
        MinHeight = CalculateMinimumHeight(HeaderPadding);
        MinWidth = 410;
        StyleIconSize = EffectiveGlobalToken.SizeLG * 1.2;
    }
}
