using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="DotBadgeToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DotBadgeTokenRule : IControlTokenRule<AntDesignToken, DotBadgeToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<DotBadgeToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        token.DotSize     = global.FontSizeSM / 2;
        token.DotColor    = global.ColorError;
        token.ShadowSize  = global.LineWidth;
        token.ShadowColor = global.ColorBorderBg;
        token.LabelMargin = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
    }
}
