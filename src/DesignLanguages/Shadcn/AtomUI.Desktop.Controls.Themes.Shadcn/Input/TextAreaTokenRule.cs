using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TextAreaToken"/>.</summary>
[ControlTokenRule]
internal sealed class TextAreaTokenRule : IControlTokenRule<ShadcnToken, TextAreaToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TextAreaToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var frame = InputTokenCalculator.CalculateFieldGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.ResizeIndicatorLineColor = global.ColorTextDescription;
        token.ResizeHandleSize         = global.SizeXS;

        var lineWidth    = global.LineWidth;
        token.RightAddOnPadding   = new Thickness(0, 0, global.UniformlyPaddingSM - lineWidth, 0);
        token.RightAddOnPaddingSM = new Thickness(0, 0, global.ControlPaddingHorizontalSM - lineWidth, 0);
        token.RightAddOnPaddingLG = new Thickness(0, 0, global.ControlPaddingHorizontal - lineWidth, 0);
        token.LeftInnerAddOnMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        (token.TransparentOutlineBorder, token.BottomOutlineBorder) = frame;
    }
}
