using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="SeparatorToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SeparatorTokenRule : IControlTokenRule<ShadcnToken, SeparatorToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SeparatorToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: no registry value is merged into the separator tokens.
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
    }
}
