using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractTimeViewToken : AbstractControlDesignToken
{
    public double ItemHeight { get; set; }
    public double ItemWidth { get; set; }
    public double PeriodHostWidth { get; set; }
    public Thickness ItemPadding { get; set; }
    public Thickness ItemMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0);
    }

    protected static Thickness CalculateTimeItemPadding(double itemWidth, double itemHeight) =>
        new((itemWidth - itemHeight) / 2, 0, 0, 0);
}
