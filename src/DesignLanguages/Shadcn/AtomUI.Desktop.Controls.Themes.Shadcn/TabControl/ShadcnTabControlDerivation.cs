using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TabControlToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTabControlDerivation : IControlDerivation<ShadcnToken, TabControlToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TabControlToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractLineTabDerivation.DeriveShared(in context, global, token);
        token.MenuEdgeThickness = global.ControlHeight;
        token.TabAndContentGutter = global.UniformlyMarginSM;
        token.HorizontalMargin = new Thickness(0, 0, global.UniformlyMargin, 0);
    }
}
