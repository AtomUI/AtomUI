using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractTimeViewToken" /> layer.
/// </summary>
public static class TimeViewTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared <see cref="AbstractTimeViewToken" /> layer: the horizontal margin of a
    /// column item, and the popup arrow, which the picker stages that run after this calculator turn off again.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractTimeViewToken token)
    {
        token.ItemMargin        = new Thickness(global.UniformlyMarginXXS, 0);
        token.PopupArrowVisible = true;
    }


    /// <summary>
    /// Returns <c>ItemPadding</c>, the left padding of a time column item: half of the difference between
    /// <paramref name="itemWidth" /> and <paramref name="itemHeight" />, and nothing on the other sides.
    /// </summary>
    public static Thickness CalculateTimeItemPadding(double itemWidth, double itemHeight) =>
        new((itemWidth - itemHeight) / 2, 0, 0, 0);
}
