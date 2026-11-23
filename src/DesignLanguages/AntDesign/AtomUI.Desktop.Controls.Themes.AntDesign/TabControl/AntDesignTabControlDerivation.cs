using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TabControlToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTabControlDerivation : IControlDerivation<AntDesignToken, TabControlToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TabControlToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractLineTabDerivation.DeriveShared(in context, global, token);
        token.MenuEdgeThickness = global.ControlHeight;
        token.TabAndContentGutter = global.UniformlyMarginSM;
        token.HorizontalMargin = new Thickness(0, 0, global.UniformlyMargin, 0);
    }
}
