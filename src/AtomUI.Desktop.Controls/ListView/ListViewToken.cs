using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ListViewToken : AbstractListViewToken
{
    public Thickness GroupHeaderPadding { get; set; }
    public Thickness SelectedIndicatorMargin { get; set; }
    public Thickness PaginationMargin { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GroupHeaderPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, EffectiveGlobalToken.UniformlyPaddingXS);
        SelectedIndicatorMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
        PaginationMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS);
    }
}
