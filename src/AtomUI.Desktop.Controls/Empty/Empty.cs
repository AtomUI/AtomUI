using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The empty-state placeholder: a built-in or custom illustration, a description and an optional footer.
/// With the palette tokens enabled, the built-in illustration takes its colors from the shared palette
/// tokens and follows their changes; an explicitly set color property still wins.
/// </summary>
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

    /// <summary>
    /// Calls the base handler, then mirrors a changed description into the effective description, with
    /// <see langword="null" /> becoming an empty string, and keeps the preset illustration in step: switching
    /// UsePresetPaletteTokenResources rebuilds the illustration and, when switched off, releases the palette
    /// token subscription, which is also released when PresetImage is cleared.
    /// </summary>
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

    /// <summary>
    /// Releases the palette token subscription of the preset illustration before the control leaves the logical
    /// tree. The subscription is created again on demand when the illustration colors are next resolved.
    /// </summary>
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

    /// <summary>
    /// Resolves the colors the preset illustration is drawn with. While
    /// <see cref="UsePresetPaletteTokenResources" /> is off, it returns the result of the base implementation.
    /// While it is on, each of <c>BorderColor</c>, <c>BorderColorSecondary</c>, <c>ShadowColor</c>,
    /// <c>ContentColor</c> and <c>BgColor</c> comes from that property when it was set explicitly, and otherwise
    /// from the shared tokens <c>ColorFill</c>, <c>ColorBorderSecondary</c>, <c>ColorFillTertiary</c>,
    /// <c>ColorFillQuaternary</c> and <c>ColorBgElevated</c> respectively. Calling it while
    /// <see cref="UsePresetPaletteTokenResources" /> is on creates the palette token subscription on demand; the
    /// subscription rebuilds the illustration when a token changes.
    /// </summary>
    /// <param name="colors">
    /// The resolved colors when this method returns <see langword="true" />; <see langword="default" /> otherwise.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when all five colors were resolved. While
    /// <see cref="UsePresetPaletteTokenResources" /> is on, a value of one of these properties that is neither a
    /// <c>Color</c>, a solid brush nor a text that parses as a color makes it return
    /// <see langword="false" />, and the illustration that is shown is left unchanged.
    /// </returns>
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
