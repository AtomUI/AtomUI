using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="MentionsToken"/>.</summary>
[ControlTokenRule]
internal sealed class MentionsTokenRule : IControlTokenRule<ShadcnToken, MentionsToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<MentionsToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        InputTokenCalculator.CalculateGlobal(in context, global);
        token.OptionHeight        = global.ControlHeight;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
    }
}
