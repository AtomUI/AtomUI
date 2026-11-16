using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// NavMenu under the PrimeNG presets: the selected, hovered and keyboard-active items follow PrimeUIX 3.1.1
/// <c>navigation.item</c> (menubar, tieredmenu, menu, panelmenu), which never uses the highlight: an active item (an
/// open submenu's path) has the neutral <c>navigation.item.active.background</c> (Aura and Lara surface 100 / 800,
/// Nora surface 200 / 700) with <c>navigation.item.active.color</c> (<c>text.hover.color</c>; Nora dark
/// <c>text.color</c>), and Aura and Lara focus (hover) on the same pair. PrimeNG menus have no persistent selection;
/// the selected item takes the active pair, so a NavMenu looks like PrimeNG's own navigation in every preset and
/// under any primary (noir included). The selected-path headers keep that text colour on the menu background, where
/// it stays readable.
/// <para>
/// Nora focuses on the solid primary with the contrast colour (<c>navigation.item.focus.background</c>
/// <c>primary.color</c>). AtomUI keeps a hovered selected-path header in the selected text colour, which would vanish
/// on the solid primary (noir light: surface 950 on surface 950), so Nora hovers on its neutral
/// <c>content.hover.background</c> / <c>content.hover.color</c> (surface 200 / 700, the active pair) instead. The
/// horizontal menu (PrimeUIX menubar) shows the active pair with the content radius (<c>menubar.base.item.border.radius</c>)
/// and without Ant Design's indicator bar.
/// </para>
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGNavMenuTokens : IControlTokenAlgorithm<NavMenuToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, NavMenuToken token)
    {
        var rules      = PrimeNGPresets.Resolve(context).For(context.Appearance);
        var surface    = rules.Surface.AsSpan();
        var background = rules.NavigationItemActive.Resolve(surface);
        var color      = rules.NavigationItemActiveColor.Resolve(surface);

        token.ItemSelectedBg    = background;
        token.ItemSelectedColor = color;
        token.ItemHoverBg       = background;
        token.ItemHoverColor    = color;
        token.ItemActiveBg      = background;

        token.HorizontalItemSelectedBg    = background;
        token.HorizontalItemSelectedColor = color;
        token.HorizontalItemHoverBg       = background;
        token.HorizontalItemBorderRadius  = effectiveGlobal.BorderRadius;
        token.ActiveBarHeight             = 0;

        // The submenu popups of the horizontal and vertical menu take the menu frame (PrimeUIX menubar submenu).
        token.MenuPopupShadows = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);
    }
}
