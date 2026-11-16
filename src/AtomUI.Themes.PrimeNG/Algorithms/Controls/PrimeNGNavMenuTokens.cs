using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// NavMenu under the Nora presets. The selected item draws <c>ItemSelectedColor</c> (the primary colour) on
/// <c>ItemSelectedBg</c>, which Ant Design takes from <c>ControlItemBgActive</c>: the solid primary in Nora, so the
/// text would vanish. <c>ItemSelectedColor</c> cannot take the contrast colour instead, because the same token
/// colours the headers of the selected path, which sit on the menu background. The selected background therefore
/// takes the rule table's primary tint for the menu only (<see cref="PrimeNGAppearanceRules.PrimaryTint"/>: light 6%,
/// dark 16%); the text stays the primary. It writes only while the
/// selected background really is the solid primary (<see cref="PrimeNGPresets.HasSolidHighlight"/>): under a noir
/// primary or a mixed chain whose Map colours come from Ant Design the Ant Design value stays.
/// </summary>
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
        if (!PrimeNGPresets.HasSolidHighlight(effectiveGlobal) ||
            PrimeNGPresets.Resolve(context).For(context.Appearance).PrimaryTint is not { } tint)
        {
            return;
        }
        token.ItemSelectedBg = tint(effectiveGlobal).Bg;
    }
}
