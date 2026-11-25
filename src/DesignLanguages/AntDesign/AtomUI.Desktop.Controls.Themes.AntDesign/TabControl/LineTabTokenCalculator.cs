using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractLineTabToken"/> layer.</summary>
public static class LineTabTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractLineTabToken token)
    {
        TabTokenCalculator.Calculate(in context, global, token);
        token.InkBarColor = global.ColorPrimary;
        token.InkBarThickness = global.LineWidthBold;
        token.HorizontalItemGutter = 32;
        token.HorizontalItemPadding = new Thickness(0, global.UniformlyPaddingSM);
        token.HorizontalItemPaddingLG = new Thickness(0, global.UniformlyPadding);
        token.HorizontalItemPaddingSM = new Thickness(0, global.UniformlyPaddingXS);
        token.VerticalItemGutter = global.SpacingXXS;
    }
}
