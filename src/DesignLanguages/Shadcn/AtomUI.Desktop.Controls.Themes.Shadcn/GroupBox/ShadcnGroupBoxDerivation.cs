using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="GroupBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnGroupBoxDerivation : IControlDerivation<ShadcnToken, GroupBoxToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<GroupBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.ContentPadding           = global.PaddingXS;
        token.HeaderContainerMargin = new Thickness(global.UniformlyMargin, global.UniformlyMarginXS,
            global.UniformlyMargin, 0);
        token.HeaderContentPadding = new Thickness(global.UniformlyPaddingXXS, 0);
        token.HeaderIconMargin     = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.HeaderFontWeight     = FontWeight.Normal;
    }
}
