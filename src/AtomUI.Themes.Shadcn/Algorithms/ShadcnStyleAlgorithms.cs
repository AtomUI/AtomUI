using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;

namespace AtomUI.Themes.Shadcn.Algorithms;

/// <summary>
/// The light Nova style as a theme algorithm (<c>Shadcn:Nova</c>): radius 10, 32-high controls, shadowless cards.
/// </summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Nova", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnNovaAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnNovaAlgorithm()
        : base(ShadcnStyles.Nova, dark: false)
    {
    }
}

/// <summary>
/// The dark Nova style as a theme algorithm (<c>Shadcn:Nova.Dark</c>): the same geometry over the dark variables.
/// </summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnNovaDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnNovaDarkAlgorithm()
        : base(ShadcnStyles.Nova, dark: true)
    {
    }
}
