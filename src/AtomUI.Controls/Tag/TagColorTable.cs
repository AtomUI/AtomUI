using System.Runtime.CompilerServices;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

internal struct TagCalcColor
{
    public Color LightColor { get; set; } // Palette color 1
    public Color LightBorderColor { get; set; } // Palette color 3
    public Color DarkColor { get; set; } // Palette color 6
    public Color TextColor { get; set; } // Palette color 7
}

internal struct TagStatusCalcColor
{
    public Color Color { get; set; }
    public Color Background { get; set; }
    public Color BorderColor { get; set; }
}

/// <summary>
/// The preset and status colors of one theme snapshot. A snapshot is immutable, so the table is built once
/// per snapshot and shared by every tag that resolves against it.
/// </summary>
internal sealed class TagColorTable
{
    private enum StatusSlot
    {
        Success,
        Info,
        Warning,
        Error,
        Primary
    }

    private static readonly ConditionalWeakTable<ThemeSnapshot, TagColorTable> s_tables = new();
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    private static readonly (string Name, PresetPrimaryColor Color)[] PresetColorEntries =
    [
        (nameof(PresetColorType.Red), PresetPrimaryColor.Red),
        (nameof(PresetColorType.Volcano), PresetPrimaryColor.Volcano),
        (nameof(PresetColorType.Orange), PresetPrimaryColor.Orange),
        (nameof(PresetColorType.Gold), PresetPrimaryColor.Gold),
        (nameof(PresetColorType.Yellow), PresetPrimaryColor.Yellow),
        (nameof(PresetColorType.Lime), PresetPrimaryColor.Lime),
        (nameof(PresetColorType.Green), PresetPrimaryColor.Green),
        (nameof(PresetColorType.Cyan), PresetPrimaryColor.Cyan),
        (nameof(PresetColorType.Blue), PresetPrimaryColor.Blue),
        (nameof(PresetColorType.GeekBlue), PresetPrimaryColor.GeekBlue),
        (nameof(PresetColorType.Purple), PresetPrimaryColor.Purple),
        (nameof(PresetColorType.Pink), PresetPrimaryColor.Pink),
        (nameof(PresetColorType.Magenta), PresetPrimaryColor.Magenta),
        (nameof(PresetColorType.Grey), PresetPrimaryColor.Grey)
    ];

    // "processing" aliases info, "danger" aliases error.
    private static readonly (string Name, StatusSlot Slot)[] StatusColorEntries =
    [
        (nameof(TagStatus.Success), StatusSlot.Success),
        (nameof(TagStatus.Info), StatusSlot.Info),
        ("processing", StatusSlot.Info),
        (nameof(TagStatus.Warning), StatusSlot.Warning),
        (nameof(TagStatus.Error), StatusSlot.Error),
        ("danger", StatusSlot.Error),
        ("primary", StatusSlot.Primary)
    ];

    private readonly TagCalcColor?[] _presets;
    private readonly TagStatusCalcColor[] _statuses;

    internal Color SolidTextColor { get; }

    private TagColorTable(ThemeSnapshot snapshot)
    {
        _presets = new TagCalcColor?[PresetColorEntries.Length];
        for (var i = 0; i < PresetColorEntries.Length; i++)
        {
            if (!snapshot.PresetColorPalettes.TryGetValue(PresetColorEntries[i].Color, out var palette))
            {
                continue;
            }

            _presets[i] = new TagCalcColor
            {
                LightColor       = palette.ColorSequence[0],
                LightBorderColor = palette.ColorSequence[2],
                DarkColor        = palette.ColorSequence[5],
                TextColor        = palette.ColorSequence[6]
            };
        }

        _statuses =
        [
            Status(snapshot, SharedTokenKind.ColorSuccess, SharedTokenKind.ColorSuccessBg, SharedTokenKind.ColorSuccessBorder),
            Status(snapshot, SharedTokenKind.ColorInfo, SharedTokenKind.ColorInfoBg, SharedTokenKind.ColorInfoBorder),
            Status(snapshot, SharedTokenKind.ColorWarning, SharedTokenKind.ColorWarningBg, SharedTokenKind.ColorWarningBorder),
            Status(snapshot, SharedTokenKind.ColorError, SharedTokenKind.ColorErrorBg, SharedTokenKind.ColorErrorBorder),
            Status(snapshot, SharedTokenKind.ColorPrimary, SharedTokenKind.ColorPrimaryBg, SharedTokenKind.ColorPrimaryBorder)
        ];
        SolidTextColor = Global(snapshot, SharedTokenKind.ColorTextLightSolid);
    }

    internal static TagColorTable For(ThemeSnapshot snapshot)
    {
        return s_tables.GetValue(snapshot, static key => new TagColorTable(key));
    }

    internal bool TryGetPreset(ReadOnlySpan<char> name, out TagCalcColor color)
    {
        for (var i = 0; i < PresetColorEntries.Length; i++)
        {
            if (PresetColorEntries[i].Name.AsSpan().Equals(name, StringComparison.OrdinalIgnoreCase) &&
                _presets[i] is { } preset)
            {
                color = preset;
                return true;
            }
        }

        color = default;
        return false;
    }

    internal bool TryGetStatus(ReadOnlySpan<char> name, out TagStatusCalcColor color)
    {
        foreach (var entry in StatusColorEntries)
        {
            if (entry.Name.AsSpan().Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                color = _statuses[(int)entry.Slot];
                return true;
            }
        }

        color = default;
        return false;
    }

    private static TagStatusCalcColor Status(
        ThemeSnapshot snapshot, SharedTokenKind color, SharedTokenKind background, SharedTokenKind border)
    {
        return new TagStatusCalcColor
        {
            Color       = Global(snapshot, color),
            Background  = Global(snapshot, background),
            BorderColor = Global(snapshot, border)
        };
    }

    private static Color Global(ThemeSnapshot snapshot, SharedTokenKind kind)
    {
        return s_themeTokenResolver.GetGlobal<Color>(snapshot, (int)kind);
    }
}
