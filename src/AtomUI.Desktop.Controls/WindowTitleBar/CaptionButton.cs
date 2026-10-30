using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

using AvaloniaButton = Avalonia.Controls.Button;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal class CaptionButton : AvaloniaButton
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> NormalIconProperty =
        AvaloniaProperty.Register<CaptionButton, PathIcon?>(nameof(NormalIcon));
    
    public static readonly StyledProperty<PathIcon?> CheckedIconProperty =
        AvaloniaProperty.Register<CaptionButton, PathIcon?>(nameof(CheckedIcon));
    
    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<CaptionButton, double>(nameof(IconWidth));

    public static readonly StyledProperty<double> IconHeightProperty = 
        AvaloniaProperty.Register<CaptionButton, double>(nameof(IconHeight));
    
    public static readonly StyledProperty<bool> IsCheckedProperty = 
        AvaloniaProperty.Register<CaptionButton, bool>(nameof(IsChecked), defaultBindingMode: BindingMode.TwoWay, defaultValue:false);

    internal static readonly DirectProperty<CaptionButton, PathIcon?> EffectiveIconProperty =
        AvaloniaProperty.RegisterDirect<CaptionButton, PathIcon?>(
            nameof(EffectiveIcon),
            o => o.EffectiveIcon);
    
    public PathIcon? NormalIcon
    {
        get => GetValue(NormalIconProperty);
        set => SetValue(NormalIconProperty, value);
    }
    
    public PathIcon? CheckedIcon
    {
        get => GetValue(CheckedIconProperty);
        set => SetValue(CheckedIconProperty, value);
    }

    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    public double IconHeight
    {
        get => GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }
    
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private PathIcon? _effectiveIcon;

    internal PathIcon? EffectiveIcon
    {
        get => _effectiveIcon;
        private set => SetAndRaise(EffectiveIconProperty, ref _effectiveIcon, value);
    }

    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CaptionButton>();
    
    internal static readonly StyledProperty<bool> IsWindowActiveProperty = 
        WindowTitleBar.IsWindowActiveProperty.AddOwner<CaptionButton>();

    internal static readonly StyledProperty<Thickness> BackgroundInsetProperty =
        AvaloniaProperty.Register<CaptionButton, Thickness>(nameof(BackgroundInset));
    
    internal static readonly DirectProperty<CaptionButton, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<CaptionButton, CornerRadius>(
            nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    internal bool IsWindowActive
    {
        get => GetValue(IsWindowActiveProperty);
        set => SetValue(IsWindowActiveProperty, value);
    }

    internal Thickness BackgroundInset
    {
        get => GetValue(BackgroundInsetProperty);
        set => SetValue(BackgroundInsetProperty, value);
    }

    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }
    #endregion

    static CaptionButton()
    {
        AffectsRender<CaptionButton>(EffectiveCornerRadiusProperty);
        NormalIconProperty.Changed.AddClassHandler<CaptionButton>((button, _) => button.UpdateEffectiveIcon());
        CheckedIconProperty.Changed.AddClassHandler<CaptionButton>((button, _) => button.UpdateEffectiveIcon());
        IsCheckedProperty.Changed.AddClassHandler<CaptionButton>((button, _) => button.UpdateEffectiveIcon());
    }
    
    public CaptionButton()
    {
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateEffectiveCornerRadius(e.NewSize);
    }

    protected virtual void UpdateEffectiveCornerRadius(Size size)
    {
        var maxSize = Math.Max(size.Height, size.Width);
        EffectiveCornerRadius = new CornerRadius(maxSize / 2);
    }

    private void UpdateEffectiveIcon()
    {
        EffectiveIcon = IsChecked && CheckedIcon is not null
            ? CheckedIcon
            : NormalIcon;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        e.Handled = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        var minSize = Math.Min(size.Width, size.Height);
        return new Size(minSize, minSize);
    }
}
