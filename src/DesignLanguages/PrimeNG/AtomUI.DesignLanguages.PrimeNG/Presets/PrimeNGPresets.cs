using System.Collections.Immutable;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG.Presets;

/// <summary>Which of the three preset families a style belongs to.</summary>
internal enum PrimeNGPresetKind
{
    Aura,
    Lara,
    Nora
}

/// <summary>
/// Finds the rule table of the current style for the global and control derivations, with the surface scale taken
/// from the language tokens (<see cref="PrimeNGToken"/>), so a theme that overrides a surface stop changes every rule
/// that resolves against the scale.
/// </summary>
internal static class PrimeNGPresets
{
    /// <summary>The rule table of <paramref name="style"/>; an undeclared style fails instead of falling back.</summary>
    internal static PrimeNGPreset Of(DesignStyle style) => KindOf(style) switch
    {
        PrimeNGPresetKind.Lara => PrimeNGLaraPreset.Table,
        PrimeNGPresetKind.Nora => PrimeNGNoraPreset.Table,
        _                      => PrimeNGAuraPreset.Table
    };

    /// <summary>The preset family of <paramref name="style"/>.</summary>
    internal static PrimeNGPresetKind KindOf(DesignStyle style)
    {
        if (style == PrimeNGLanguage.Aura)
        {
            return PrimeNGPresetKind.Aura;
        }
        if (style == PrimeNGLanguage.Lara)
        {
            return PrimeNGPresetKind.Lara;
        }
        if (style == PrimeNGLanguage.Nora)
        {
            return PrimeNGPresetKind.Nora;
        }
        throw new InvalidOperationException($"'{style}' is not a PrimeNG style.");
    }

    /// <summary>The rule table of the derivation's style.</summary>
    internal static PrimeNGPreset Resolve(in DerivationContext<PrimeNGToken> context) => Of(context.Variant.Style);

    /// <summary>The preset family of the derivation's style.</summary>
    internal static PrimeNGPresetKind KindOf(in DerivationContext<PrimeNGToken> context) => KindOf(context.Variant.Style);

    /// <summary>
    /// The rules of <paramref name="appearance"/> with the language's surface scale. Allocates only when a theme
    /// overrode a surface stop.
    /// </summary>
    internal static PrimeNGAppearanceRules For(PrimeNGPreset preset, ThemeAppearance appearance, PrimeNGToken language)
    {
        var rules = preset.For(appearance);
        var scale = language.SurfaceScale;
        return SameScale(rules.Surface, scale) ? rules : rules with { Surface = scale };
    }

    /// <summary>The rules of the derivation's appearance, with the language's surface scale.</summary>
    internal static PrimeNGAppearanceRules For(in DerivationContext<PrimeNGToken> context) =>
        For(Resolve(context), context.Variant.Appearance, context.Language);

    /// <summary>
    /// The rules a global or control derivation applies: the table of the style for the appearance, with the noir rows
    /// when <paramref name="token"/>'s primary colour is noir (see <see cref="PrimeNGNoir"/>).
    /// </summary>
    internal static PrimeNGAppearanceRules Rules(PrimeNGPreset preset, ThemeAppearance appearance, PrimeNGToken language,
                                                 DesignToken token) =>
        PrimeNGNoir.Resolve(For(preset, appearance, language), appearance, token);

    /// <summary>The rules a control derivation applies, with the noir rows when the control's primary colour is noir.</summary>
    internal static PrimeNGAppearanceRules Rules(in DerivationContext<PrimeNGToken> context, DesignToken token) =>
        Rules(Resolve(context), context.Variant.Appearance, context.Language, token);

    /// <summary>A surface stop (slate in light, zinc in dark) of the active appearance; 0 is white.</summary>
    internal static Color Surface(in DerivationContext<PrimeNGToken> context, int stop) =>
        new PrimeNGShade(stop).Resolve(context.Language.SurfaceScale.AsSpan());

    /// <summary>PrimeUIX <c>content.border.color</c>: Aura and Lara surface 200 / 700, Nora surface 400 / 500.</summary>
    internal static Color ContentBorder(in DerivationContext<PrimeNGToken> context)
    {
        var rules = For(context);
        return rules.MenuBorder.Resolve(rules.Surface.AsSpan());
    }

    /// <summary>
    /// Whether the selected background (<c>ControlItemBgActive</c>) is the solid primary colour, as in the Nora
    /// presets (PrimeUIX <c>highlight.background</c>).
    /// </summary>
    internal static bool HasSolidHighlight(DesignToken token) => token.ControlItemBgActive == token.ColorPrimary;

    /// <summary>Whether the control's primary colour is noir.</summary>
    internal static bool IsNoirActive(in DerivationContext<PrimeNGToken> context, DesignToken token)
    {
        var appearance = context.Variant.Appearance;
        return PrimeNGNoir.IsNoirPrimary(For(context), appearance, token.ColorPrimary);
    }

    private static bool SameScale(ImmutableArray<Color> left, ImmutableArray<Color> right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }
        for (var index = 0; index < left.Length; index++)
        {
            if (left[index] != right[index])
            {
                return false;
            }
        }
        return true;
    }
}
