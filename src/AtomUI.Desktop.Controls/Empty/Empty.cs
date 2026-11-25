using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public partial class Empty : AbstractEmpty
{
    private static readonly object[] s_presetPaletteKeys =
    [
        ControlTokenResourceKey.Global(EmptyTokens.Identity, SharedTokenKind.ColorFill),
        ControlTokenResourceKey.Global(EmptyTokens.Identity, SharedTokenKind.ColorBorderSecondary),
        ControlTokenResourceKey.Global(EmptyTokens.Identity, SharedTokenKind.ColorFillTertiary),
        ControlTokenResourceKey.Global(EmptyTokens.Identity, SharedTokenKind.ColorFillQuaternary),
        ControlTokenResourceKey.Global(EmptyTokens.Identity, SharedTokenKind.ColorBgElevated)
    ];

    // Description is object-typed and shown by one content presenter, as Ant Design wraps any description in one
    // node. A null description is presented as empty text so the node keeps the layout of an empty text line.
    internal static readonly DirectProperty<Empty, object?> EffectiveDescriptionProperty =
        AvaloniaProperty.RegisterDirect<Empty, object?>(nameof(EffectiveDescription), o => o.EffectiveDescription);

    internal static readonly StyledProperty<bool> UsePresetPaletteTokenResourcesProperty =
        AvaloniaProperty.Register<Empty, bool>(nameof(UsePresetPaletteTokenResources));

    internal bool UsePresetPaletteTokenResources
    {
        get => GetValue(UsePresetPaletteTokenResourcesProperty);
        set => SetValue(UsePresetPaletteTokenResourcesProperty, value);
    }

    private object? _effectiveDescription = string.Empty;
    private OwnerTokenSet? _presetPalette;

    internal object? EffectiveDescription => _effectiveDescription;

    public Empty()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DescriptionProperty)
        {
            SetAndRaise(EffectiveDescriptionProperty, ref _effectiveDescription, Description ?? string.Empty);
        }
        else if (change.Property == UsePresetPaletteTokenResourcesProperty)
        {
            if (!UsePresetPaletteTokenResources)
            {
                ReleasePresetPaletteTokenSubscription();
            }
            RefreshPresetImage();
        }
        else if (change.Property == PresetImageProperty && PresetImage is null)
        {
            ReleasePresetPaletteTokenSubscription();
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ReleasePresetPaletteTokenSubscription();
        base.OnDetachedFromLogicalTree(e);
    }

    private void ReleasePresetPaletteTokenSubscription()
    {
        _presetPalette?.Dispose();
        _presetPalette = null;
    }

    private protected override bool TryResolvePresetColors(out PresetImageColors colors)
    {
        if (!UsePresetPaletteTokenResources)
        {
            return base.TryResolvePresetColors(out colors);
        }

        // Custom image sources never enter this branch or subscribe to the preset palette.
        _presetPalette ??= CreatePresetPaletteTokenSet();
        var border = ReadPaletteColor(0, BorderColorProperty);
        var secondary = ReadPaletteColor(1, BorderColorSecondaryProperty);
        var shadow = ReadPaletteColor(2, ShadowColorProperty);
        var content = ReadPaletteColor(3, ContentColorProperty);
        var background = ReadPaletteColor(4, BgColorProperty);
        if (border.HasValue && secondary.HasValue && shadow.HasValue && content.HasValue && background.HasValue)
        {
            colors = new PresetImageColors(border.Value, secondary.Value, shadow.Value, content.Value, background.Value);
            return true;
        }

        colors = default;
        return false;
    }

    private OwnerTokenSet CreatePresetPaletteTokenSet()
    {
        var palette = new OwnerTokenSet(this, s_presetPaletteKeys.Length, RefreshPresetImage);
        for (var index = 0; index < s_presetPaletteKeys.Length; ++index)
        {
            palette.SetResourceKey(index, s_presetPaletteKeys[index]);
        }
        return palette;
    }

    private Color? ReadPaletteColor(int index, StyledProperty<IBrush?> overrideProperty)
    {
        var value = IsSet(overrideProperty) ? GetValue(overrideProperty) : _presetPalette!.GetValue(index);
        return value switch
        {
            Color color => color,
            ISolidColorBrush brush => brush.Color,
            string text when Color.TryParse(text, out var color) => color,
            _ => null
        };
    }
}
