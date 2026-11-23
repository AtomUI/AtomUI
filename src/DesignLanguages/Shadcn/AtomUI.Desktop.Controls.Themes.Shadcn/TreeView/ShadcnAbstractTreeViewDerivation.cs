using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractTreeViewToken"/> layer.</summary>
public static class ShadcnAbstractTreeViewDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractTreeViewToken token)
    {
        ShadcnAbstractTreePresentationDerivation.DeriveShared(in context, global, token);


        token.DragIndicatorLineWidth = global.LineWidthBold;
    }
}
