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

/// <summary>The light Vega style (<c>Shadcn:Vega</c>): radius 8, xs shadowed cards.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Vega", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnVegaAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnVegaAlgorithm()
        : base(ShadcnStyles.Vega, dark: false)
    {
    }
}

/// <summary>The dark Vega style (<c>Shadcn:Vega.Dark</c>).</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnVegaDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnVegaDarkAlgorithm()
        : base(ShadcnStyles.Vega, dark: true)
    {
    }
}

/// <summary>The light Maia style (<c>Shadcn:Maia</c>): pill buttons, 2xl overlays, a 5% light ring.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Maia", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnMaiaAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnMaiaAlgorithm()
        : base(ShadcnStyles.Maia, dark: false)
    {
    }
}

/// <summary>The dark Maia style (<c>Shadcn:Maia.Dark</c>): a 10% dark ring.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnMaiaDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnMaiaDarkAlgorithm()
        : base(ShadcnStyles.Maia, dark: true)
    {
    }
}

/// <summary>The light Lyra style (<c>Shadcn:Lyra</c>): square everything, 12px type.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Lyra", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnLyraAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnLyraAlgorithm()
        : base(ShadcnStyles.Lyra, dark: false)
    {
    }
}

/// <summary>The dark Lyra style (<c>Shadcn:Lyra.Dark</c>).</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnLyraDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnLyraDarkAlgorithm()
        : base(ShadcnStyles.Lyra, dark: true)
    {
    }
}

/// <summary>The light Mira style (<c>Shadcn:Mira</c>): compact 28-high controls.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Mira", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnMiraAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnMiraAlgorithm()
        : base(ShadcnStyles.Mira, dark: false)
    {
    }
}

/// <summary>The dark Mira style (<c>Shadcn:Mira.Dark</c>).</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnMiraDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnMiraDarkAlgorithm()
        : base(ShadcnStyles.Mira, dark: true)
    {
    }
}

/// <summary>The light Luma style (<c>Shadcn:Luma</c>): lg overlays, xl dialogs and drawers.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Luma", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnLumaAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnLumaAlgorithm()
        : base(ShadcnStyles.Luma, dark: false)
    {
    }
}

/// <summary>The dark Luma style (<c>Shadcn:Luma.Dark</c>): a 10% dark ring.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnLumaDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnLumaDarkAlgorithm()
        : base(ShadcnStyles.Luma, dark: true)
    {
    }
}

/// <summary>The light Sera style (<c>Shadcn:Sera</c>): editorial, square, semibold.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Sera", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnSeraAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnSeraAlgorithm()
        : base(ShadcnStyles.Sera, dark: false)
    {
    }
}

/// <summary>The dark Sera style (<c>Shadcn:Sera.Dark</c>).</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnSeraDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnSeraDarkAlgorithm()
        : base(ShadcnStyles.Sera, dark: true)
    {
    }
}

/// <summary>The light Rhea style (<c>Shadcn:Rhea</c>): rounded 18, sm cards, xl dialogs.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Rhea", 1, ThemeAppearanceEffect.Light)]
public sealed class ShadcnRheaAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnRheaAlgorithm()
        : base(ShadcnStyles.Rhea, dark: false)
    {
    }
}

/// <summary>The dark Rhea style (<c>Shadcn:Rhea.Dark</c>): a 10% dark ring.</summary>
[ThemeAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class ShadcnRheaDarkAlgorithm : ShadcnStyleAlgorithm
{
    public ShadcnRheaDarkAlgorithm()
        : base(ShadcnStyles.Rhea, dark: true)
    {
    }
}
