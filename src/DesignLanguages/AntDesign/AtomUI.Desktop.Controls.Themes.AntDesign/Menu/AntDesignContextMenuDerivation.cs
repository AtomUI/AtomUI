using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ContextMenuToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignContextMenuDerivation : IControlDerivation<AntDesignToken, ContextMenuToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ContextMenuToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractMenuPopupDerivation.DeriveShared(in context, global, token);
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        token.ContextMenuOffsetX = global.SpacingXXS;
        token.ContextMenuOffsetY = global.SpacingXXS;
    }
}
