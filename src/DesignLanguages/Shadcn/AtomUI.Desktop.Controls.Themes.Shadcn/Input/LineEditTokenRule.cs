using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="LineEditToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class LineEditTokenRule : IControlTokenRule<ShadcnToken, LineEditToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<LineEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var frame = InputTokenCalculator.CalculateFieldGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.InputFontSize = global.FontSize;
        token.InputFontSizeLG = global.FontSizeLG;
        token.InputFontSizeSM = token.InputFontSize;
        // The frame of the style replaces the outlined frame of the formula: a translucent fill without a border in
        // Luma and Rhea, a bottom rule only in Sera.
        (token.TransparentOutlineBorder, token.BottomOutlineBorder) = frame;
    }
}
