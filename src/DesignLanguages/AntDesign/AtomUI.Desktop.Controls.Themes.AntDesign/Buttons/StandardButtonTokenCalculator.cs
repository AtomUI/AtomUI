using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractStandardButtonToken"/> layer.</summary>
public static class StandardButtonTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractStandardButtonToken token)
    {
        ButtonGeometryTokenCalculator.Calculate(in context, global, token);
        token.FontWeight = FontWeight.Normal;
        token.DefaultBorderColorDisabled = global.ColorBorder;
    }
}
