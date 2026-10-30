using AtomUI.Controls;
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

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<PaletteColorItem>();

    internal static readonly StyledProperty<ITransform?> CheckedMarkRenderTransformProperty =
        AvaloniaProperty.Register<PaletteColorItem, ITransform?>(nameof(CheckedMarkRenderTransform));

    internal static readonly DirectProperty<PaletteColorItem, bool> IsLightColorProperty =
        AvaloniaProperty.RegisterDirect<PaletteColorItem, bool>(
            nameof(IsLightColor),
            o => o.IsLightColor,
            (o, v) => o.IsLightColor = v);

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

    static PaletteColorItem()
    {
        AffectsRender<PaletteColorItem>(ColorProperty, IsLightColorProperty);
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
