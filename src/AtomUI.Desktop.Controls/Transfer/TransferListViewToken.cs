using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TransferListViewToken : AbstractListViewToken
{
    public partial Thickness PaginationMargin { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        PaginationMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS);
    }
}
