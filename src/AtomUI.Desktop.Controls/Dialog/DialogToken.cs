using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DialogToken : AbstractDialogToken
{
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(DialogTokenKind.HeaderPadding))
        {
            HeaderPadding = CalculateHeaderPadding();
        }
        MinHeight = CalculateMinimumHeight(HeaderPadding);
        MinWidth = 200;
    }
}
