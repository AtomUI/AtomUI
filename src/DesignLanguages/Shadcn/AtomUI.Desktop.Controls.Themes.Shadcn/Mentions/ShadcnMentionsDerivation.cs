using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="MentionsToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnMentionsDerivation : IControlDerivation<ShadcnToken, MentionsToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<MentionsToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnInputFamily.DeriveGlobal(in context, global);
        token.OptionHeight        = global.ControlHeight;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
    }
}
