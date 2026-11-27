using AtomUI.Theme;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

internal static class BadgeColorUtils
{
    // Preset entries keep a fixed light-theme color for callers that have no theme snapshot (ribbon badge,
    // detached controls); with a snapshot the preset resolves from the palette so it follows dark mode.
#pragma warning disable ATOMUIGEN044 // Ant Design preset seed colors: reference data for callers without a theme snapshot.
    private static readonly (string Name, Color Color, PresetColorType Type)[] PresetColorEntries =
    {
        (nameof(PresetColorType.Red), Color.FromRgb(0xF5, 0x22, 0x2D), PresetColorType.Red),
        (nameof(PresetColorType.Volcano), Color.FromRgb(0xFA, 0x54, 0x1C), PresetColorType.Volcano),
        (nameof(PresetColorType.Orange), Color.FromRgb(0xFA, 0x8C, 0x16), PresetColorType.Orange),
        (nameof(PresetColorType.Gold), Color.FromRgb(0xFA, 0xAD, 0x14), PresetColorType.Gold),
        (nameof(PresetColorType.Yellow), Color.FromRgb(0xFA, 0xDB, 0x14), PresetColorType.Yellow),
        (nameof(PresetColorType.Lime), Color.FromRgb(0xA0, 0xD9, 0x11), PresetColorType.Lime),
        (nameof(PresetColorType.Green), Color.FromRgb(0x52, 0xC4, 0x1A), PresetColorType.Green),
        (nameof(PresetColorType.Cyan), Color.FromRgb(0x13, 0xC2, 0xC2), PresetColorType.Cyan),
        (nameof(PresetColorType.Blue), Color.FromRgb(0x16, 0x77, 0xFF), PresetColorType.Blue),
        (nameof(PresetColorType.GeekBlue), Color.FromRgb(0x2F, 0x54, 0xEB), PresetColorType.GeekBlue),
        (nameof(PresetColorType.Purple), Color.FromRgb(0x72, 0x2E, 0xD1), PresetColorType.Purple),
        (nameof(PresetColorType.Pink), Color.FromRgb(0xEB, 0x2F, 0x96), PresetColorType.Pink),
        (nameof(PresetColorType.Magenta), Color.FromRgb(0xEB, 0x2F, 0x96), PresetColorType.Magenta),
        (nameof(PresetColorType.Grey), Color.FromRgb(0x66, 0x66, 0x66), PresetColorType.Grey)
    };
#pragma warning restore ATOMUIGEN044

    // "danger" is an alias of "error".
    private static readonly (string Name, SharedTokenKind Token)[] StatusColorEntries =
    {
        ("primary", SharedTokenKind.ColorPrimary),
        ("success", SharedTokenKind.ColorSuccess),
        ("warning", SharedTokenKind.ColorWarning),
        ("error", SharedTokenKind.ColorError),
        ("danger", SharedTokenKind.ColorError)
    };

    // The 6th palette entry is the preset's base color.
    private const int PresetBaseColorIndex = 5;

    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    /// <summary>
    /// Resolves against the theme snapshot that applies to <paramref name="owner" />.
    /// </summary>
    /// <returns>
    /// A solid brush, or <see langword="null" /> when <paramref name="colorStr" /> is empty or unknown, or when it is
    /// a status name and no theme snapshot applies to <paramref name="owner" />.
    /// </returns>
    internal static IBrush? CalculateColor(StyledElement owner, string? colorStr)
    {
        return CalculateColor(colorStr, ThemeScope.ResolveContext(owner)?.Snapshot);
    }

    /// <summary>
    /// Resolves without a theme: presets use the fixed light palette, status names do not resolve.
    /// </summary>
    /// <returns>
    /// A solid brush, or <see langword="null" /> when <paramref name="colorStr" /> is empty, a status name or unknown.
    /// </returns>
    internal static IBrush? CalculateColor(string? colorStr)
    {
        return CalculateColor(colorStr, null);
    }

    internal static IBrush? CalculateColor(string? colorStr, ThemeSnapshot? snapshot)
    {
        if (string.IsNullOrEmpty(colorStr))
        {
            return null;
        }
        var colorSpan = colorStr.AsSpan().Trim();

        if (TryGetPresetColor(colorSpan, snapshot, out var presetColor))
        {
            return new SolidColorBrush(presetColor);
        }

        if (snapshot is not null && TryGetStatusColor(colorSpan, snapshot, out var statusColor))
        {
            return new SolidColorBrush(statusColor);
        }

        if (Color.TryParse(colorSpan, out var color))
        {
            return new SolidColorBrush(color);
        }
        return null;
    }

    /// <summary>
    /// True when the value is a preset or status name, whose result depends on the theme.
    /// </summary>
    internal static bool IsThemeDependent(string? colorStr)
    {
        if (string.IsNullOrEmpty(colorStr))
        {
            return false;
        }

        var colorSpan = colorStr.AsSpan().Trim();
        return TryGetPresetColor(colorSpan, out _) || TryGetStatusToken(colorSpan, out _);
    }

    internal static bool TryGetPresetColor(ReadOnlySpan<char> colorSpan, out Color color)
    {
        foreach (var entry in PresetColorEntries)
        {
            if (entry.Name.AsSpan().Equals(colorSpan, StringComparison.OrdinalIgnoreCase))
            {
                color = entry.Color;
                return true;
            }
        }

        color = default;
        return false;
    }

    private static bool TryGetPresetColor(ReadOnlySpan<char> colorSpan, ThemeSnapshot? snapshot, out Color color)
    {
        foreach (var entry in PresetColorEntries)
        {
            if (!entry.Name.AsSpan().Equals(colorSpan, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            color = entry.Color;
            if (snapshot is not null &&
                snapshot.PresetColorPalettes.TryGetValue(PresetPrimaryColor.GetColor(entry.Type), out var palette) &&
                palette.ColorSequence.Count > PresetBaseColorIndex)
            {
                color = palette.ColorSequence[PresetBaseColorIndex];
            }
            return true;
        }

        color = default;
        return false;
    }

    private static bool TryGetStatusToken(ReadOnlySpan<char> colorSpan, out SharedTokenKind token)
    {
        foreach (var entry in StatusColorEntries)
        {
            if (entry.Name.AsSpan().Equals(colorSpan, StringComparison.OrdinalIgnoreCase))
            {
                token = entry.Token;
                return true;
            }
        }

        token = default;
        return false;
    }

    private static bool TryGetStatusColor(ReadOnlySpan<char> colorSpan, ThemeSnapshot snapshot, out Color color)
    {
        if (TryGetStatusToken(colorSpan, out var token))
        {
            color = s_themeTokenResolver.GetGlobal<Color>(snapshot, (int)token);
            return true;
        }

        color = default;
        return false;
    }
}
