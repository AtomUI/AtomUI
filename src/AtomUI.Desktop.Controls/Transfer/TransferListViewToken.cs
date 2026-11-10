using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TransferListViewToken : AbstractListViewToken
{
    public Thickness PaginationMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        PaginationMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS);
    }
}
