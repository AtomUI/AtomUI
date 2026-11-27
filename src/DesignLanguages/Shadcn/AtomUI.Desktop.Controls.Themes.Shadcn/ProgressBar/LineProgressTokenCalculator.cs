using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractLineProgressToken" /> layer.
/// </summary>
public static class LineProgressTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractLineProgressToken" /> layer of the progress bar and the steps progress bar. It starts
    /// with <see cref="ProgressTokenCalculator.Calculate" />, which fills the progress layer first.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractLineProgressToken token)
    {
        ProgressTokenCalculator.Calculate(in context, global, token);
        token.LineInfoIconSize    = global.IconSize;
        token.LineInfoIconSizeSM  = global.IconSizeSM;
        token.LineExtraInfoMargin = global.ControlPaddingHorizontalSM;
    }
}
