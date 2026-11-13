using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTimeViewToken : AbstractControlDesignToken
{
    public partial double ItemHeight { get; set; }
    public partial double ItemWidth { get; set; }
    public partial double PeriodHostWidth { get; set; }
    public partial Thickness ItemPadding { get; set; }
    public partial Thickness ItemMargin { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0);
    }

    protected static Thickness CalculateTimeItemPadding(double itemWidth, double itemHeight) =>
        new((itemWidth - itemHeight) / 2, 0, 0, 0);
}
