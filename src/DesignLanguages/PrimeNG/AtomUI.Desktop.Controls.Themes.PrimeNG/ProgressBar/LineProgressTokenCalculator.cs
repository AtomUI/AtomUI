using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractLineProgressToken" /> layer.
/// </summary>
public static class LineProgressTokenCalculator
{
    /// <summary>
    /// Calls <see cref="ProgressTokenCalculator.Calculate" /> and then writes the PrimeNG values into the shared
    /// line progress layer: the info icon sizes and the gap between the bar and the extra information beside it.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractLineProgressToken token)
    {
        ProgressTokenCalculator.Calculate(in context, global, token);
        token.LineInfoIconSize    = global.IconSize;
        token.LineInfoIconSizeSM  = global.IconSizeSM;
        token.LineExtraInfoMargin = global.ControlPaddingHorizontalSM;
    }
}
