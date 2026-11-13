using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ListTransferToken : AbstractTransferToken
{
    public partial double ListWidthLG { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ListWidthLG = 250;
    }
}
