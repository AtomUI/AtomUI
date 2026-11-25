using AtomUI.Theme.Derivation;
using AtomUI.Toolkits.GalleryBase.Models;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public class ColorListControl : TemplatedControl
{
    public static readonly StyledProperty<PaletteMetaItem?> PaletteMetaInfoProperty =
        AvaloniaProperty.Register<ColorListControl, PaletteMetaItem?>(nameof(PaletteMetaInfo));

    public static readonly StyledProperty<bool> IsDarkProperty =
        AvaloniaProperty.Register<ColorListControl, bool>(nameof(IsDark), false);

    public PaletteMetaItem? PaletteMetaInfo
    {
        get => GetValue(PaletteMetaInfoProperty);
        set => SetValue(PaletteMetaInfoProperty, value);
    }

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
