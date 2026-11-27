using AtomUI.Theme.Derivation;
using AtomUI.Toolkits.GalleryBase.Models;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Lists the colors of a preset palette, one item per color of its color sequence.
/// <see cref="PaletteMetaInfo" /> selects the preset color and <see cref="IsDark" /> the light or dark sequence.
/// </summary>
public class ColorListControl : TemplatedControl
{
    /// <summary>
    /// Identifies the <see cref="PaletteMetaInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<PaletteMetaItem?> PaletteMetaInfoProperty =
        AvaloniaProperty.Register<ColorListControl, PaletteMetaItem?>(nameof(PaletteMetaInfo));

    /// <summary>
    /// Identifies the <see cref="IsDark" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDarkProperty =
        AvaloniaProperty.Register<ColorListControl, bool>(nameof(IsDark), false);

    /// <summary>
    /// The preset palette whose color sequence is listed.
    /// The list is regenerated each time the template is applied, and when this value changes while the control is
    /// attached to a visual tree. A <see langword="null" /> value generates nothing and leaves the current list.
    /// </summary>
    public PaletteMetaItem? PaletteMetaInfo
    {
        get => GetValue(PaletteMetaInfoProperty);
        set => SetValue(PaletteMetaInfoProperty, value);
    }

    /// <summary>
    /// Selects the dark (<see langword="true" />) or light (<see langword="false" />) color sequence of the
    /// preset palette. The value is read when the color list is generated, so setting it does not regenerate an
    /// existing list.
    /// </summary>
    public bool IsDark
    {
        get => GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    #region 内部属性定义

    internal static readonly StyledProperty<AvaloniaList<PaletteColorInfo>?> ColorInfosProperty =
        AvaloniaProperty.Register<ColorListControl, AvaloniaList<PaletteColorInfo>?>(nameof(ColorInfos));

    internal AvaloniaList<PaletteColorInfo>? ColorInfos
    {
        get => GetValue(ColorInfosProperty);
        set => SetValue(ColorInfosProperty, value);
    }

    #endregion

    /// <summary>
    /// Called when an Avalonia property of the control changes. Regenerates the color list when
    /// <see cref="PaletteMetaInfo" /> changes while the control is attached to a visual tree. The base method is
    /// called first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PaletteMetaInfoProperty)
        {
            if (VisualRoot is not null)
            {
                GenerateColorInfos();
            }
        }
    }

    /// <summary>
    /// Generates the color list for the applied template, which also covers a <see cref="PaletteMetaInfo" /> value
    /// that was set before the control was attached to a visual tree. The base method is called first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        GenerateColorInfos();
    }

    private void GenerateColorInfos()
    {
        if (PaletteMetaInfo is null)
        {
            return;
        }
        var paletteInfo     = PresetPalettes.GetPresetPalette(PaletteMetaInfo.PresetPrimaryColor, IsDark);
        var presetColorName = PaletteMetaInfo.PresetPrimaryColor.Name();
        var list            = new AvaloniaList<PaletteColorInfo>();
        for (var j = 0; j < paletteInfo.ColorSequence.Count; j++)
        {
            var color = paletteInfo.ColorSequence[j];
            var colorItem = new PaletteColorInfo($"{presetColorName}-{j + 1}",
                new SolidColorBrush(color),
                !IsDark,
                j);
            list.Add(colorItem);
        }

        ColorInfos = list;
    }
}
