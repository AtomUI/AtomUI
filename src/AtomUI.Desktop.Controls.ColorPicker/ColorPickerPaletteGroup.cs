using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

internal class ColorPickerPaletteGroup : TemplatedControl
{
    public static readonly StyledProperty<List<ColorPickerPalette>?> PaletteGroupProperty =
        AvaloniaProperty.Register<ColorPickerPaletteGroup, List<ColorPickerPalette>?>(
            nameof(PaletteGroup));

    public List<ColorPickerPalette>? PaletteGroup
    {
        get => GetValue(PaletteGroupProperty);
        set => SetValue(PaletteGroupProperty, value);
    }

    public event EventHandler<ColorPickerPaletteColorSelectedEventArgs>? ColorSelected;

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ColorPickerPaletteGroup>();

    internal static readonly DirectProperty<ColorPickerPaletteGroup, string?> GroupNameProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerPaletteGroup, string?>(
            nameof(GroupName),
            o => o.GroupName,
            (o, v) => o.GroupName = v);

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    private string? _groupNameProperty;
    private IDisposable? _colorSelectedDisposable;

    internal string? GroupName
    {
        get => _groupNameProperty;
        set => SetAndRaise(GroupNameProperty, ref _groupNameProperty, value);
    }

    #endregion

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        SetCurrentValue(GroupNameProperty, $"ColorPaletteGroup-{GetHashCode()}");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _colorSelectedDisposable?.Dispose();
        _colorSelectedDisposable = this.AddDisposableHandler(
            PaletteColorItem.IsCheckedChangedEvent,
            HandlePaletteColorItemCheckedChanged,
            RoutingStrategies.Bubble);
    }

    private void HandlePaletteColorItemCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (args.Source is PaletteColorItem { IsChecked: true, Color: { } selectedColor })
        {
            ColorSelected?.Invoke(this, new ColorPickerPaletteColorSelectedEventArgs(selectedColor));
        }
        args.Handled = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _colorSelectedDisposable?.Dispose();
        _colorSelectedDisposable = null;
    }
}
