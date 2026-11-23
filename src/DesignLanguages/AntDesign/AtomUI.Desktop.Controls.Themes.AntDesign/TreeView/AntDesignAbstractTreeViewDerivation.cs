using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractTreeViewToken"/> layer.</summary>
public static class AntDesignAbstractTreeViewDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractTreeViewToken token)
    {
        AntDesignAbstractTreePresentationDerivation.DeriveShared(in context, global, token);


        token.DragIndicatorLineWidth = global.LineWidthBold;
    }
}
