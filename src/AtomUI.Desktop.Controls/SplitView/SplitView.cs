using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

using AvaloniaSplitView = Avalonia.Controls.SplitView;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public class SplitView : AvaloniaSplitView, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<Easing?> PaneMotionEasingProperty =
        AvaloniaProperty.Register<SplitView, Easing?>(nameof(PaneMotionEasing));

    public static readonly StyledProperty<TimeSpan> PaneOpenMotionDurationProperty =
        AvaloniaProperty.Register<SplitView, TimeSpan>(nameof(PaneOpenMotionDuration));

    public static readonly StyledProperty<TimeSpan> PaneCloseMotionDurationProperty =
        AvaloniaProperty.Register<SplitView, TimeSpan>(nameof(PaneCloseMotionDuration));

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

    #endregion

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsPaneOpenProperty && IsLoaded)
        {
            EnsureTransitionsForPaneState(change.GetNewValue<bool>());
        }

        base.OnPropertyChanged(change);

        if (!IsLoaded)
        {
            return;
        }

        if (change.Property == IsMotionEnabledProperty)
        {
            if (IsMotionEnabled)
            {
                RefreshMaterializedTransitions();
            }
            else
            {
                ClearTransitions();
            }
        }
        else if (change.Property == PanePlacementProperty ||
            change.Property == PaneOpenMotionDurationProperty ||
            change.Property == PaneCloseMotionDurationProperty ||
            change.Property == PaneMotionEasingProperty)
        {
            RefreshMaterializedTransitions();
        }
    }

    private void EnsureTransitionsForPaneState(bool isPaneOpen)
    {
        if (!IsMotionEnabled)
        {
            ClearTransitions();
            return;
        }

        if (isPaneOpen)
        {
            PaneOpenTransitions ??= CreatePaneTransitions(PaneOpenMotionDuration);
        }
        else
        {
            PaneCloseTransitions ??= CreatePaneTransitions(PaneCloseMotionDuration);
        }
    }

    private void RefreshMaterializedTransitions()
    {
        if (!IsMotionEnabled)
        {
            ClearTransitions();
            return;
        }

        if (PaneOpenTransitions is not null)
        {
            PaneOpenTransitions = CreatePaneTransitions(PaneOpenMotionDuration);
        }

        if (PaneCloseTransitions is not null)
        {
            PaneCloseTransitions = CreatePaneTransitions(PaneCloseMotionDuration);
        }
    }

    private Transitions CreatePaneTransitions(TimeSpan duration)
    {
        return
        [
            TransitionUtils.CreateTransition<DoubleTransition>(
                GetPaneTransitionProperty(),
                duration,
                PaneMotionEasing)
        ];
    }

    private AvaloniaProperty GetPaneTransitionProperty()
    {
        return PanePlacement is SplitViewPanePlacement.Left or SplitViewPanePlacement.Right
            ? WidthProperty
            : HeightProperty;
    }

    private void ClearTransitions()
    {
        PaneOpenTransitions  = null;
        PaneCloseTransitions = null;
    }
}
