using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ListViewToken : AbstractListViewToken
{
    public partial Thickness GroupHeaderPadding { get; set; }
    public partial Thickness SelectedIndicatorMargin { get; set; }

    /// <summary>
    /// Colour of the check mark on a selected item.
    /// </summary>
    public partial Color ItemSelectedIndicatorColor { get; set; }
    public partial Thickness PaginationMargin { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GroupHeaderPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, EffectiveGlobalToken.UniformlyPaddingXS);
        SelectedIndicatorMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
        ItemSelectedIndicatorColor = EffectiveGlobalToken.ColorPrimary;
        PaginationMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS);
    }
}
