using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SeparatorToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGSeparatorDerivation : IControlDerivation<PrimeNGToken, SeparatorToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<SeparatorToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSeparatorTokens.EvaluateGlobal(in context, global);
        token.TextPaddingInline              = 1.0;
        token.OrientationMarginPercent       = 0.05;
        token.VerticalMarginInline           = global.UniformlyMarginXS;
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
