using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="MentionsToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MentionsTokenRule : IControlTokenRule<AntDesignToken, MentionsToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<MentionsToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.OptionHeight        = global.ControlHeight;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
    }
}
