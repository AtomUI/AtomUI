using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

public abstract class AbstractToggleIconButton : ToggleButton, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> CheckedIconProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, PathIcon?>(nameof(CheckedIcon));

    public static readonly StyledProperty<PathIcon?> UnCheckedIconProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, PathIcon?>(nameof(UnCheckedIcon));

    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, double>(nameof(IconWidth));

    public static readonly StyledProperty<double> IconHeightProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, double>(nameof(IconHeight));
        
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(IconBrush));
    
    public static readonly StyledProperty<IBrush?> NormalIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(NormalIconBrush));
    
    public static readonly StyledProperty<IBrush?> ActiveIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(ActiveIconBrush));
    
    public static readonly StyledProperty<IBrush?> SelectedIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(SelectedIconBrush));
    
    public static readonly StyledProperty<IBrush?> DisabledIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(DisabledIconBrush));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractToggleIconButton>();

    public PathIcon? CheckedIcon
    {
        get => GetValue(CheckedIconProperty);
        set => SetValue(CheckedIconProperty, value);
    }

    public PathIcon? UnCheckedIcon
    {
        get => GetValue(UnCheckedIconProperty);
        set => SetValue(UnCheckedIconProperty, value);
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

    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }
    
    public IBrush? NormalIconBrush
    {
        get => GetValue(NormalIconBrushProperty);
        set => SetValue(NormalIconBrushProperty, value);
    }

    public IBrush? ActiveIconBrush
    {
        get => GetValue(ActiveIconBrushProperty);
        set => SetValue(ActiveIconBrushProperty, value);
    }

    public IBrush? SelectedIconBrush
    {
        get => GetValue(SelectedIconBrushProperty);
        set => SetValue(SelectedIconBrushProperty, value);
    }

    public IBrush? DisabledIconBrush
    {
        get => GetValue(DisabledIconBrushProperty);
        set => SetValue(DisabledIconBrushProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    static AbstractToggleIconButton()
    {
        AffectsMeasure<AbstractToggleIconButton>(CheckedIconProperty, UnCheckedIconProperty, IsCheckedProperty, IconWidthProperty, IconHeightProperty);
        AffectsRender<AbstractToggleIconButton>(IconBrushProperty);
    }
    
    public bool HitTest(Point point)
    {
        return NotifyHistTest(point);
    }

    protected virtual bool NotifyHistTest(Point point)
    {
        return true;
    }

}