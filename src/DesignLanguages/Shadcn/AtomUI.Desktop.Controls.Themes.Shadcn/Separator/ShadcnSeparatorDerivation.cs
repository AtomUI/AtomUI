using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SeparatorToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnSeparatorDerivation : IControlDerivation<ShadcnToken, SeparatorToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SeparatorToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.TextPaddingInline              = 1.0;
        token.OrientationMarginPercent       = 0.05;
        token.VerticalMarginInline           = global.UniformlyMarginXS;
        token.HorizontalMarginBlockSM        = new Thickness(0, global.UniformlyMarginXS);
        token.HorizontalMarginBlock          = new Thickness(0, global.UniformlyMargin);
        token.HorizontalMarginBlockLG        = new Thickness(0, global.UniformlyMarginLG);
        token.HorizontalWithTextGutterMargin = new Thickness(0, global.UniformlyMargin);
        // antd Divider renders the title of a divider with text at weight 500 (a fixed value, not fontWeightStrong).
        token.TitleFontWeight                = FontWeight.Medium;
    }
}
