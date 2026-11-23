using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MentionsToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignMentionsDerivation : IControlDerivation<AntDesignToken, MentionsToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<MentionsToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.OptionHeight        = global.ControlHeight;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
    }
}
