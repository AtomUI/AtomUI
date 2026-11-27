using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="MentionsToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MentionsTokenRule : IControlTokenRule<PrimeNGToken, MentionsToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MentionsToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply (form-field values), read by the Mentions
        // theme and its input frame (CornerRadius, Foreground, Background, IconBrush), not the formulas below.
        PrimeNGMentionsTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is; the PrimeNG mentions stage (PrimeNGMentionsTokens.EvaluateOwn)
        // replaces only OptionColor and OptionSelectedColor, the PrimeUIX list option colors. The option
        // height, the popup padding and the minimum width stay.
        token.OptionHeight        = global.ControlHeight;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMentionsTokens.EvaluateOwn(in context, global, token);
    }
}
