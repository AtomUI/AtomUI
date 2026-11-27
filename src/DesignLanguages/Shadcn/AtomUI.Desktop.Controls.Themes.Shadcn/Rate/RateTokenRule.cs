using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="RateToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class RateTokenRule : IControlTokenRule<ShadcnToken, RateToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<RateToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: shadcn has no rate, so the language has no values of its own.
        token.StarColor      = global.ColorPalettes[PresetPrimaryColor.Yellow].Color6;
        token.StarSize       = global.ControlHeight * 0.625;
        token.StarSizeSM     = global.ControlHeightSM * 0.625;
        token.StarSizeLG     = global.ControlHeightLG * 0.625;
        token.StarHoverScale = 1.2;
        token.StarBg         = global.ColorFillContent;
    }
}
