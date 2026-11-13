using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ListViewToken : AbstractListViewToken
{
    public partial Thickness GroupHeaderPadding { get; set; }
    public partial Thickness SelectedIndicatorMargin { get; set; }
    public partial Thickness PaginationMargin { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GroupHeaderPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, EffectiveGlobalToken.UniformlyPaddingXS);
        SelectedIndicatorMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
        PaginationMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS);
    }
}
