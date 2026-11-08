using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// The colours antd's <c>genPresetColor</c> takes from one palette: steps 1 (<c>lightColor</c>), 3
/// (<c>lightBorderColor</c>), 6 (<c>darkColor</c>) and 7 (<c>textColor</c>).
/// </summary>
internal readonly record struct PresetColorSet(Color LightColor, Color LightBorderColor, Color DarkColor, Color TextColor)
{
    internal static PresetColorSet From(IReadOnlyList<Color> palette) =>
        new(palette[0], palette[2], palette[5], palette[6]);
}

/// <summary>
/// Preset colours of the controls that accept a preset colour name (Tag, Badge, RibbonBadge, ToolTip). The colours come
/// from the theme snapshot's preset palettes, which the theme compiler builds once per snapshot with the snapshot's
/// algorithm (dark palettes under a dark theme), so a lookup never rebuilds a palette per control instance and preset
/// colours follow the theme. Controls re-resolve when their theme snapshot changes
/// (<c>ThemeTokenResolver.Subscribe</c>). Custom colours derive the same steps with the palette algorithm of the
/// snapshot's appearance.
/// </summary>
internal static class PresetColorPalette
{
    private static readonly PaletteGenerateOption s_darkPaletteOption = new() { ThemeVariant = ThemeVariant.Dark };

    /// <summary>Matches a preset colour name (<c>red</c>, <c>geekblue</c>, ...) case-insensitively.</summary>
    internal static bool TryParse(ReadOnlySpan<char> name, out PresetPrimaryColor color)
    {
        var trimmed = name.Trim();
        foreach (var candidate in PresetPrimaryColor.AllColorTypes())
        {
            if (candidate.Name().AsSpan().Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            {
                color = candidate;
                return true;
            }
        }

        color = null!;
        return false;
    }

    /// <summary>The genPresetColor colours of a preset in the snapshot's palettes.</summary>
    internal static bool TryResolve(ThemeSnapshot snapshot, PresetPrimaryColor color, out PresetColorSet colors)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.PresetColorPalettes.TryGetValue(color, out var palette))
        {
            colors = PresetColorSet.From(palette.ColorSequence);
            return true;
        }

        colors = default;
        return false;
    }

    /// <summary>The genPresetColor colours of a custom seed colour, generated for the snapshot's appearance.</summary>
    internal static PresetColorSet ResolveCustom(ThemeSnapshot snapshot, Color seed)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var option = snapshot.Appearance == ThemeAppearance.Dark ? s_darkPaletteOption : null;
        return PresetColorSet.From(PaletteGenerator.GeneratePalette(seed, option));
    }
}
