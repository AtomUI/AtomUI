using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class ToggleIconButton : ToggleButton,
                                IControlSharedTokenResourcesHost, 
                                IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> CheckedIconProperty =
        AvaloniaProperty.Register<ToggleIconButton, PathIcon?>(nameof(CheckedIcon));

    public static readonly StyledProperty<PathIcon?> UnCheckedIconProperty =
        AvaloniaProperty.Register<ToggleIconButton, PathIcon?>(nameof(UnCheckedIcon));

    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<ToggleIconButton, double>(nameof(IconWidth));

    public static readonly StyledProperty<double> IconHeightProperty =
        AvaloniaProperty.Register<ToggleIconButton, double>(nameof(IconHeight));
        
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<ToggleIconButton, IBrush?>(
            nameof(IconBrush));
    
    public static readonly StyledProperty<IBrush?> NormalIconBrushProperty =
        AvaloniaProperty.Register<ToggleIconButton, IBrush?>(
            nameof(NormalIconBrush));
    
    public static readonly StyledProperty<IBrush?> ActiveIconBrushProperty =
        AvaloniaProperty.Register<ToggleIconButton, IBrush?>(
            nameof(ActiveIconBrush));
    
    public static readonly StyledProperty<IBrush?> SelectedIconBrushProperty =
        AvaloniaProperty.Register<ToggleIconButton, IBrush?>(
            nameof(SelectedIconBrush));
    
    public static readonly StyledProperty<IBrush?> DisabledIconBrushProperty =
        AvaloniaProperty.Register<ToggleIconButton, IBrush?>(
            nameof(DisabledIconBrush));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ToggleIconButton>();

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
    
    #region 内部属性定义
    Control IMotionAwareControl.PropertyBindTarget => this;
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ButtonToken.ID;
    
    #endregion

    static ToggleIconButton()
    {
        AffectsMeasure<ToggleIconButton>(CheckedIconProperty, UnCheckedIconProperty, IsCheckedProperty, IconWidthProperty, IconHeightProperty);
        AffectsRender<ToggleIconButton>(IconBrushProperty);
    }

    public ToggleIconButton()
    {
        this.RegisterResources();
    }
    
    public bool HitTest(Point point)
    {
        return NotifyHistTest(point);
    }

    protected virtual bool NotifyHistTest(Point point)
    {
        return true;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty),
                    TransitionUtils.CreateTransition<TransformOperationsTransition>(RenderTransformProperty)
                ];
                NotifyConfigureTransitions(Transitions);
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected virtual void NotifyConfigureTransitions(Transitions transitions)
    {
    }
}