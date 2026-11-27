using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractLineTabToken" /> layer.
/// </summary>
public static class LineTabTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractLineTabToken" /> layer of the tab control and the tab strip. It starts with
    /// <see cref="TabTokenCalculator.Calculate" />, which fills the tab layer first.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractLineTabToken token)
    {
        TabTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: no registry value is merged into the line tab tokens.
        token.InkBarColor = global.ColorPrimary;
        token.InkBarThickness = global.LineWidthBold;
        token.HorizontalItemGutter = 32;
        token.HorizontalItemPadding = new Thickness(0, global.UniformlyPaddingSM);
        token.HorizontalItemPaddingLG = new Thickness(0, global.UniformlyPadding);
        token.HorizontalItemPaddingSM = new Thickness(0, global.UniformlyPaddingXS);
        token.VerticalItemGutter = global.SpacingXXS;
    }
}
