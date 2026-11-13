using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MessageBoxToken : AbstractDialogToken
{
    public partial double StyleIconSize { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
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
