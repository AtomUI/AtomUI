using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TextAreaToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TextAreaTokenRule : IControlTokenRule<PrimeNGToken, TextAreaToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TextAreaToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply (form-field values), read by the TextArea
        // theme and its input frame (CornerRadius, Foreground, Background, IconBrush), not the formulas below.
        PrimeNGTextAreaTokens.EvaluateGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.ResizeIndicatorLineColor = global.ColorTextDescription;
        token.ResizeHandleSize         = global.SizeXS;

        var lineWidth    = global.LineWidth;
        // Ant Design formula kept as is: the PrimeNG text area stage (PrimeNGTextAreaTokens.EvaluateOwn) writes
        // no own token, so these add-on paddings stay the Ant Design formula over the PrimeNG global tokens.
        token.RightAddOnPadding   = new Thickness(0, 0, global.UniformlyPaddingSM - lineWidth, 0);
        token.RightAddOnPaddingSM = new Thickness(0, 0, global.ControlPaddingHorizontalSM - lineWidth, 0);
        token.RightAddOnPaddingLG = new Thickness(0, 0, global.ControlPaddingHorizontal - lineWidth, 0);
        token.LeftInnerAddOnMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTextAreaTokens.EvaluateOwn(in context, global, token);
    }
}
