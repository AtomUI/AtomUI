using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TreeViewToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTreeViewDerivation : IControlDerivation<ShadcnToken, TreeViewToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TreeViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTreeViewDerivation.DeriveShared(in context, global, token);
        token.DirectoryNodeSelectedBg = global.ColorPrimary;
        token.DirectoryNodeSelectedColor = global.ColorTextLightSolid;
        if (context.Variant.IsDark)
        {
            token.NodeSelectedBg = ShadcnRecipes.CompositeOver(global.ColorText, 0.14, token.NodeSelectedBg);
        }
    }
}
