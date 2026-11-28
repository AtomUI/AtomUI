using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SeparatorToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SeparatorTokenRule : IControlTokenRule<PrimeNGToken, SeparatorToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SeparatorToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (ColorSplit), read by the Separator theme (LineColor) and not by
        // the formulas below.
        PrimeNGSeparatorTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is; the PrimeNG divider stage at the end of the method replaces the block
        // margins (HorizontalMarginBlock, VerticalMarginInline) and the title weight.
        token.TextPaddingInline              = 1.0;
        token.OrientationMarginPercent       = 0.05;
        token.VerticalMarginInline           = global.UniformlyMarginXS;
        token.VerticalMarginBlock            = new Thickness(0);
        token.HorizontalMarginBlockSM        = new Thickness(0, global.UniformlyMarginXS);
        token.HorizontalMarginBlock          = new Thickness(0, global.UniformlyMargin);
        token.HorizontalMarginBlockLG        = new Thickness(0, global.UniformlyMarginLG);
        token.HorizontalWithTextGutterMargin = new Thickness(0, global.UniformlyMargin);
        // antd Divider renders the title of a divider with text at weight 500 (a fixed value, not fontWeightStrong).
        token.TitleFontWeight                = FontWeight.Medium;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSeparatorTokens.EvaluateOwn(in context, global, token);
    }
}
