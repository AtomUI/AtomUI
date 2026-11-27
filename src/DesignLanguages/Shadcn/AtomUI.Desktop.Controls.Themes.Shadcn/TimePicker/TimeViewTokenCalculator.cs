using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractTimeViewToken" /> layer.
/// </summary>
public static class TimeViewTokenCalculator
{
    /// <summary>
    /// Fills the <see cref="AbstractTimeViewToken" /> layer with the shadcn values: the horizontal margin around a
    /// time item and the popup arrow, which is visible.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTimeViewToken token)
    {
        // Ant Design formula kept as is: no registry value is merged into the time view layer.
        token.ItemMargin        = new Thickness(global.UniformlyMarginXXS, 0);
        token.PopupArrowVisible = true;
    }


    /// <summary>
    /// Provides the padding of a time item: the left side takes half of the difference between the item width and
    /// the item height, the other three sides take 0.
    /// </summary>
    public static Thickness CalculateTimeItemPadding(double itemWidth, double itemHeight) =>
        new((itemWidth - itemHeight) / 2, 0, 0, 0);
}
