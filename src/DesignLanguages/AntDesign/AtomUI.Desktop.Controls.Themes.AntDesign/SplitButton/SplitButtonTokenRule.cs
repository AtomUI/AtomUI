using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SplitButtonToken"/>.</summary>
[ControlTokenRule]
internal sealed class SplitButtonTokenRule : IControlTokenRule<AntDesignToken, SplitButtonToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SplitButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.InsetSeparator         = false;
        token.GutterToFlyout         = global.UniformlyMarginXXS;
        token.SolidSeparatorColor    = global.ColorPrimaryHover;
        token.DangerSeparatorColor   = global.ColorErrorHover;
    }
}
