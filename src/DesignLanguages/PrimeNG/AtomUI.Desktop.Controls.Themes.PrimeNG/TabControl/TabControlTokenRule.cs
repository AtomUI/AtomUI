using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TabControlToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TabControlTokenRule : IControlTokenRule<PrimeNGToken, TabControlToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TabControlToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: writes MotionDurationSlow, read by the TabControl theme (the ink bar transition), and
        // ColorBorderSecondary, which neither the formulas below nor the theme of the control reads.
        PrimeNGTabControlTokens.EvaluateGlobal(in context, global);
        LineTabTokenCalculator.Calculate(in context, global, token);
        token.MenuEdgeThickness = global.ControlHeight;
        token.TabAndContentGutter = global.UniformlyMarginSM;
        token.HorizontalMargin = new Thickness(0, 0, global.UniformlyMargin, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTabControlTokens.EvaluateOwn(in context, global, token);
    }
}
