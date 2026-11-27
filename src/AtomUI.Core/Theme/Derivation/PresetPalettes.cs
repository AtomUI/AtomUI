using System.Collections.Frozen;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Theme.Derivation;

/// <summary>
/// The palette of one preset primary color.
/// </summary>
public sealed class PaletteInfo
{
    /// <summary>
    /// Creates the palette of a preset color.
    /// </summary>
    /// <param name="primary">
    /// The color that stands for the preset.
    /// </param>
    /// <param name="colorSequence">
    /// The palette colors. The sequence is copied into a read-only list, so later changes to it are not visible.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="colorSequence" /> is <see langword="null" />.
    /// </exception>
    public PaletteInfo(Color primary, IEnumerable<Color> colorSequence)
    {
        ArgumentNullException.ThrowIfNull(colorSequence);
        Primary       = primary;
        ColorSequence = Array.AsReadOnly(colorSequence.ToArray());
    }

    /// <summary>
    /// The color that stands for the preset: the sixth color of the palette.
    /// </summary>
    public Color Primary { get; }
    /// <summary>
    /// The palette colors, at palette index 1 to 10. Only an unblended palette is ordered from the lightest to the
    /// darkest color.
    /// </summary>
    public IReadOnlyList<Color> ColorSequence { get; }
}

/// <summary>
/// The palettes generated for the preset primary colors, in an unblended and a dark-blended version.
/// </summary>
public static class PresetPalettes
{
    private const int PresetColorCount = 14;
    private static readonly FrozenDictionary<PresetPrimaryColor, PaletteInfo> sm_presetPalettes;
    private static readonly FrozenDictionary<PresetPrimaryColor, PaletteInfo> sm_presetDarkPalettes;
    private static readonly PaletteGenerateOption sm_darkPaletteOption = new()
    {
        ThemeVariant = ThemeVariant.Dark
    };

    static PresetPalettes()
    {
        var light = new Dictionary<PresetPrimaryColor, PaletteInfo>(PresetColorCount);
        var dark  = new Dictionary<PresetPrimaryColor, PaletteInfo>(PresetColorCount);
        InitPalettes(light, false);
        InitPalettes(dark, true);
        sm_presetPalettes     = light.ToFrozenDictionary();
        sm_presetDarkPalettes = dark.ToFrozenDictionary();
    }

    /// <summary>
    /// Returns the palette of a preset primary color.
    /// </summary>
    /// <param name="primaryColor">
    /// The preset color. Only the static instances of <see cref="PresetPrimaryColor" /> exist, so every key is
    /// found.
    /// </param>
    /// <param name="isDark">
    /// <see langword="true" /> returns the palette blended for the dark theme variant; the default returns the
    /// unblended palette.
    /// </param>
    /// <returns>
    /// The palette of <paramref name="primaryColor" />.
    /// </returns>
    public static PaletteInfo GetPresetPalette(PresetPrimaryColor primaryColor, bool isDark = false)
    {
        if (isDark)
        {
            return sm_presetDarkPalettes[primaryColor];
        }

        return sm_presetPalettes[primaryColor];
    }

    /// <summary>
    /// Returns every preset palette, keyed by its preset primary color.
    /// </summary>
    /// <param name="isDark">
    /// <see langword="true" /> returns the palettes blended for the dark theme variant; the default returns the
    /// unblended palettes.
    /// </param>
    /// <returns>
    /// The fourteen preset palettes.
    /// </returns>
    public static IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> GetPresetPalettes(bool isDark = false)
    {
        if (isDark)
        {
            return sm_presetDarkPalettes;
        }

        return sm_presetPalettes;
    }

    private static void InitPalettes(Dictionary<PresetPrimaryColor, PaletteInfo> target, bool isDark)
    {
        var allColors = PresetPrimaryColor.AllColorTypes();
        foreach (var presetColor in allColors)
        {
            if (isDark)
            {
                var colorSequence = PaletteGenerator.GeneratePalette(presetColor.Color(), sm_darkPaletteOption);
                target[presetColor] = new PaletteInfo(colorSequence[5], colorSequence);
            }
            else
            {
                var colorSequence = PaletteGenerator.GeneratePalette(presetColor.Color());
                target[presetColor] = new PaletteInfo(colorSequence[5], colorSequence);
            }
        }
    }
}
