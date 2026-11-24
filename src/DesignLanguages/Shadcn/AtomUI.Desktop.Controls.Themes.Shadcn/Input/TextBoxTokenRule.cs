using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TextBoxToken"/>.</summary>
[ControlTokenRule]
internal sealed class TextBoxTokenRule : IControlTokenRule<ShadcnToken, TextBoxToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TextBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        var fontSize     = global.FontSize;
        var fontSizeSM   = global.FontSizeSM;
        var fontSizeLG   = global.FontSizeLG;
        var lineHeight   = global.RelativeLineHeight;
        var lineHeightSM = global.RelativeLineHeightSM;
        var lineHeightLG = global.RelativeLineHeightLG;
        var lineWidth    = global.LineWidth;

        token.ContentPadding = new Thickness(global.UniformlyPaddingSM - lineWidth,
            Math.Round((global.ControlHeight - fontSize * lineHeight) / 2 * 10) / 10 - lineWidth);
        token.ContentPaddingSM = new Thickness(global.ControlPaddingHorizontalSM - lineWidth,
            Math.Round((global.ControlHeightSM - fontSizeSM * lineHeightSM) / 2 * 10) / 10 -
            lineWidth);
        token.ContentPaddingLG = new Thickness(global.ControlPaddingHorizontal - lineWidth,
            Math.Ceiling((global.ControlHeightLG - fontSizeLG * lineHeightLG) / 2 * 10) / 10 -
            lineWidth);
    }
}
