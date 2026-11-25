using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ContextMenuToken"/>.</summary>
[ControlTokenRule]
internal sealed class ContextMenuTokenRule : IControlTokenRule<PrimeNGToken, ContextMenuToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ContextMenuToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGContextMenuTokens.EvaluateGlobal(in context, global);
        MenuPopupTokenCalculator.Calculate(in context, global, token);
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        token.ContextMenuOffsetX = global.SpacingXXS;
        token.ContextMenuOffsetY = global.SpacingXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGContextMenuTokens.EvaluateOwn(in context, global, token);
    }
}
