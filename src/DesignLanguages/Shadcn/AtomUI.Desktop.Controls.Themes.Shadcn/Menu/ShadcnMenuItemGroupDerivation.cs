using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="MenuItemGroupToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnMenuItemGroupDerivation : IControlDerivation<ShadcnToken, MenuItemGroupToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<MenuItemGroupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.TitleFontWeight = FontWeight.Normal;
    }
}
