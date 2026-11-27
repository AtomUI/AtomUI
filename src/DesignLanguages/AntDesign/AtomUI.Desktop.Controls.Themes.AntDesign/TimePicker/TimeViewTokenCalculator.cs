using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractTimeViewToken" /> layer.
/// </summary>
public static class TimeViewTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the time view layer to <paramref name="token" />: the horizontal margin
    /// of an item and the visible arrow of the popup.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractTimeViewToken token)
    {
        token.ItemMargin        = new Thickness(global.UniformlyMarginXXS, 0);
        token.PopupArrowVisible = true;
    }


    /// <summary>
    /// The padding of a time item: half of the difference between its width and its height on the left side, and no
    /// padding on the other three sides.
    /// </summary>
    public static Thickness CalculateTimeItemPadding(double itemWidth, double itemHeight) =>
        new((itemWidth - itemHeight) / 2, 0, 0, 0);
}
