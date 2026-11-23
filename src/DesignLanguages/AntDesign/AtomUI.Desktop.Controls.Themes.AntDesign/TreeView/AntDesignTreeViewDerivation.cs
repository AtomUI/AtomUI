using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TreeViewToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTreeViewDerivation : IControlDerivation<AntDesignToken, TreeViewToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TreeViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTreeViewDerivation.DeriveShared(in context, global, token);
        token.DirectoryNodeSelectedBg = global.ColorPrimary;
        token.DirectoryNodeSelectedColor = global.ColorTextLightSolid;
    }
}
