using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

using AvaloniaSplitView = Avalonia.Controls.SplitView;

public class SplitView : AvaloniaSplitView, IMotionAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<Easing?> PaneMotionEasingProperty =
        AvaloniaProperty.Register<SplitView, Easing?>(nameof(PaneMotionEasing));
    
    public static readonly StyledProperty<TimeSpan> PaneOpenMotionDurationProperty =
        AvaloniaProperty.Register<StyledElement, TimeSpan>(nameof(PaneOpenMotionDuration));
    
    public static readonly StyledProperty<TimeSpan> PaneCloseMotionDurationProperty =
        AvaloniaProperty.Register<StyledElement, TimeSpan>(nameof(PaneCloseMotionDuration));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SplitView>();
    
    public Easing? PaneMotionEasing
    {
        get => GetValue(PaneMotionEasingProperty);
        set => SetValue(PaneMotionEasingProperty, value);
    }
    
    public TimeSpan PaneOpenMotionDuration
    {
        get => GetValue(PaneOpenMotionDurationProperty);
        set => SetValue(PaneOpenMotionDurationProperty, value);
    }
    
    public TimeSpan PaneCloseMotionDuration
    {
        get => GetValue(PaneCloseMotionDurationProperty);
        set => SetValue(PaneCloseMotionDurationProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<SplitView, Transitions?> PaneOpenTransitionsProperty =
        AvaloniaProperty.RegisterDirect<SplitView, Transitions?>(
            nameof(PaneOpenTransitions),
            o => o.PaneOpenTransitions,
            (o, v) => o.PaneOpenTransitions = v);
    
    internal static readonly DirectProperty<SplitView, Transitions?> PaneCloseTransitionsProperty =
        AvaloniaProperty.RegisterDirect<SplitView, Transitions?>(
            nameof(PaneCloseTransitions),
            o => o.PaneCloseTransitions,
            (o, v) => o.PaneCloseTransitions = v);

    private Transitions? _paneOpenTransitions;

    internal Transitions? PaneOpenTransitions
    {
        get => _paneOpenTransitions;
        set => SetAndRaise(PaneOpenTransitionsProperty, ref _paneOpenTransitions, value);
    }
    
    private Transitions? _paneCloseTransitions;

    internal Transitions? PaneCloseTransitions
    {
        get => _paneCloseTransitions;
        set => SetAndRaise(PaneCloseTransitionsProperty, ref _paneCloseTransitions, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SplitViewToken.ID;
    
    #endregion
    
    public SplitView()
    {
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PanePlacementProperty)
        {
            ConfigureTransitions(true);
        }
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
        PaneOpenTransitions  = null;
        PaneCloseTransitions = null;
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || PaneOpenTransitions == null)
            {
                if (PanePlacement == SplitViewPanePlacement.Left || PanePlacement == SplitViewPanePlacement.Right)
                {
                    PaneOpenTransitions = [
                        TransitionUtils.CreateTransition<DoubleTransition>(WidthProperty, PaneOpenMotionDuration, PaneMotionEasing)
                    ];
                    PaneCloseTransitions = [
                        TransitionUtils.CreateTransition<DoubleTransition>(WidthProperty, PaneCloseMotionDuration, PaneMotionEasing)
                    ];
                }
                else
                {
                    PaneOpenTransitions = [
                        TransitionUtils.CreateTransition<DoubleTransition>(HeightProperty, PaneOpenMotionDuration, PaneMotionEasing)
                    ];
                    PaneCloseTransitions = [
                        TransitionUtils.CreateTransition<DoubleTransition>(HeightProperty, PaneCloseMotionDuration, PaneMotionEasing)
                    ];
                }
            }
        }
        else
        {
            PaneOpenTransitions  = null;
            PaneCloseTransitions = null;
        }
    }
}