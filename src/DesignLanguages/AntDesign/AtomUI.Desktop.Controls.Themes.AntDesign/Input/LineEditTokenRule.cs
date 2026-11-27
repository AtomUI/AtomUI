using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="LineEditToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class LineEditTokenRule : IControlTokenRule<AntDesignToken, LineEditToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<LineEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.InputFontSize = global.FontSize;
        token.InputFontSizeLG = global.FontSizeLG;
        token.InputFontSizeSM = token.InputFontSize;
    }
}
