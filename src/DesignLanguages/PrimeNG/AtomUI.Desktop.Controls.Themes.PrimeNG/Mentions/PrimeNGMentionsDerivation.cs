using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MentionsToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGMentionsDerivation : IControlDerivation<PrimeNGToken, MentionsToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<MentionsToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGMentionsTokens.EvaluateGlobal(in context, global);
        token.OptionHeight        = global.ControlHeight;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMentionsTokens.EvaluateOwn(in context, global, token);
    }
}
