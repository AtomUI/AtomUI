using System.Collections.Frozen;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Theme.Derivation;

public sealed class PaletteInfo
{
    public PaletteInfo(Color primary, IEnumerable<Color> colorSequence)
    {
        ArgumentNullException.ThrowIfNull(colorSequence);
        Primary       = primary;
        ColorSequence = Array.AsReadOnly(colorSequence.ToArray());
    }

    public Color Primary { get; }
    public IReadOnlyList<Color> ColorSequence { get; }
}

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

    public static PaletteInfo GetPresetPalette(PresetPrimaryColor primaryColor, bool isDark = false)
    {
        if (isDark)
        {
            return sm_presetDarkPalettes[primaryColor];
        }

        return sm_presetPalettes[primaryColor];
    }

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
