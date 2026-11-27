using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="RibbonBadgeToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class RibbonBadgeTokenRule : IControlTokenRule<PrimeNGToken, RibbonBadgeToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<RibbonBadgeToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        // Ant Design formula kept as is: PrimeUIX defines no values for this control, so the rule takes the Ant
        // Design formula over the PrimeNG global tokens.
        token.Offset             = new Point(global.UniformlyMarginXS, global.UniformlyMarginXS);
        token.CornerTransform    = new ScaleTransform(1, 0.75).ToImmutable();
        token.CornerDarkenAmount = 15;
        token.TextPadding        = new Thickness(global.UniformlyPaddingXS, 0);
    }
}
