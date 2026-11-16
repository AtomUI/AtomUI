using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The Drawer (Sheet) under the shadcn styles: the drawer shadow level of the overlay table on all four sides
/// (design 5.6.1). The anchored-side border is a Task-9 control token; until then the Ant Design rendering stays.
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnDrawerTokens : IControlTokenAlgorithm<DrawerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DrawerToken token)
    {
        var shadows = ShadcnStyles.Shadow(ShadcnStyles.Resolve(context).Overlay.DrawerShadow);
        token.BoxShadowDrawerLeft  = shadows;
        token.BoxShadowDrawerRight = shadows;
        token.BoxShadowDrawerUp    = shadows;
        token.BoxShadowDrawerDown  = shadows;
    }
}
