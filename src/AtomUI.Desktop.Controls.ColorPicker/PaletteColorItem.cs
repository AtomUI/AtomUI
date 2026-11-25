using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia.VisualTree;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaRadioButton = Avalonia.Controls.RadioButton;

internal class PaletteColorItem : AvaloniaRadioButton
{
    #region 公共属性定义

    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<PaletteColorItem, Color?>(nameof(Color));

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> CheckedMarkForegroundProperty =
        AvaloniaProperty.Register<PaletteColorItem, IBrush?>(nameof(CheckedMarkForeground));


    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<PaletteColorItem>();

    internal static readonly StyledProperty<ITransform?> CheckedMarkRenderTransformProperty =
        AvaloniaProperty.Register<PaletteColorItem, ITransform?>(nameof(CheckedMarkRenderTransform));

    internal static readonly DirectProperty<PaletteColorItem, bool> IsLightColorProperty =
        AvaloniaProperty.RegisterDirect<PaletteColorItem, bool>(
            nameof(IsLightColor),
            o => o.IsLightColor,
            (o, v) => o.IsLightColor = v);

    internal IBrush? CheckedMarkForeground
    {
        get => GetValue(CheckedMarkForegroundProperty);
        set => SetValue(CheckedMarkForegroundProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal ITransform? CheckedMarkRenderTransform
    {
        get => GetValue(CheckedMarkRenderTransformProperty);
        set => SetValue(CheckedMarkRenderTransformProperty, value);
    }

    private bool _isLightColor;

    internal bool IsLightColor
    {
        get => _isLightColor;
        set => SetAndRaise(IsLightColorProperty, ref _isLightColor, value);
    }
    #endregion

    #region 运行时字段

    private IDisposable? _tokenOwnerForwarding;

    #endregion

    static PaletteColorItem()
    {
        AffectsRender<PaletteColorItem>(ColorProperty, IsLightColorProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tokenOwnerForwarding?.Dispose();
        // ControlTheme cannot set ThemeScope.TokenOwner (internal attached setter); the registered Collapse
        // between this swatch and its palette must keep its own identity. Forward only the host-owned swatch.
        if (this.FindAncestorOfType<ColorPickerPaletteGroup>() is { } palette)
        {
            _tokenOwnerForwarding = TokenOwnerForwarding.Forward(palette, this);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _tokenOwnerForwarding?.Dispose();
        _tokenOwnerForwarding = null;
        base.OnDetachedFromVisualTree(e);
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColorProperty)
        {
            if (Color != null)
            {
                SetCurrentValue(BackgroundProperty, new SolidColorBrush(Color.Value));
                var luminance = ColorHelper.GetRelativeLuminance(Color.Value);
                SetCurrentValue(IsLightColorProperty, luminance > 0.5);
            }
        }
    }

}
