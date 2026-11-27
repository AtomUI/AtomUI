using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractLineTabToken" /> layer.
/// </summary>
public static class LineTabTokenCalculator
{
    /// <summary>
    /// Calls <see cref="TabTokenCalculator.Calculate" /> and then writes the PrimeNG values into the shared line tab
    /// layer: the ink bar color and thickness and the gutter and padding of the items in the horizontal and the
    /// vertical layout.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractLineTabToken token)
    {
        TabTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the tab stage (PrimeNGTabStage.ApplyLine), run by the callers after this
        // calculator, replaces the horizontal item padding and gutter and the ink bar thickness; InkBarColor and
        // VerticalItemGutter keep the Ant formula over the PrimeNG global tokens.
        token.InkBarColor = global.ColorPrimary;
        token.InkBarThickness = global.LineWidthBold;
        token.HorizontalItemGutter = 32;
        token.HorizontalItemPadding = new Thickness(0, global.UniformlyPaddingSM);
        token.HorizontalItemPaddingLG = new Thickness(0, global.UniformlyPadding);
        token.HorizontalItemPaddingSM = new Thickness(0, global.UniformlyPaddingXS);
        token.VerticalItemGutter = global.SpacingXXS;
    }
}
