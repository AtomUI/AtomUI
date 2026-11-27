using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

using AvaloniaSplitView = Avalonia.Controls.SplitView;

/// <summary>
/// A split view that animates the pane between its open and closed position, with a separate duration for
/// opening and for closing.
/// </summary>
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public class SplitView : AvaloniaSplitView, IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="PaneMotionEasing" /> property.
    /// </summary>
    public static readonly StyledProperty<Easing?> PaneMotionEasingProperty =
        AvaloniaProperty.Register<SplitView, Easing?>(nameof(PaneMotionEasing));

    /// <summary>
    /// Identifies the <see cref="PaneOpenMotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> PaneOpenMotionDurationProperty =
        AvaloniaProperty.Register<SplitView, TimeSpan>(nameof(PaneOpenMotionDuration));

    /// <summary>
    /// Identifies the <see cref="PaneCloseMotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> PaneCloseMotionDurationProperty =
        AvaloniaProperty.Register<SplitView, TimeSpan>(nameof(PaneCloseMotionDuration));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SplitView>();

    /// <summary>
    /// Easing curve of the pane open and close animations. It is used only while <see cref="IsMotionEnabled" />
    /// is <see langword="true" />. The theme sets it from <see cref="SplitViewToken.PaneMotionEasing" />, so an
    /// explicit value overrides the token and clearing it returns to the token value.
    /// </summary>
    public Easing? PaneMotionEasing
    {
        get => GetValue(PaneMotionEasingProperty);
        set => SetValue(PaneMotionEasingProperty, value);
    }

    /// <summary>
    /// Duration of the animation that opens the pane. It is used only while <see cref="IsMotionEnabled" /> is
    /// <see langword="true" />. The theme sets it from <see cref="SplitViewToken.PaneOpenMotionDuration" />, so
    /// an explicit value overrides the token and clearing it returns to the token value.
    /// </summary>
    public TimeSpan PaneOpenMotionDuration
    {
        get => GetValue(PaneOpenMotionDurationProperty);
        set => SetValue(PaneOpenMotionDurationProperty, value);
    }

    /// <summary>
    /// Duration of the animation that closes the pane. It is used only while <see cref="IsMotionEnabled" /> is
    /// <see langword="true" />. The theme sets it from <see cref="SplitViewToken.PaneCloseMotionDuration" />, so
    /// an explicit value overrides the token and clearing it returns to the token value.
    /// </summary>
    public TimeSpan PaneCloseMotionDuration
    {
        get => GetValue(PaneCloseMotionDurationProperty);
        set => SetValue(PaneCloseMotionDurationProperty, value);
    }

    /// <summary>
    /// Enables the animation of the pane between its open and closed position. When it is
    /// <see langword="false" /> the pane jumps to its new position. The theme sets it from the token value
    /// <c>EnableMotion</c>, so an explicit value overrides that value and clearing it returns to it.
    /// </summary>
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

    /// <summary>
    /// When <c>IsPaneOpen</c> changes on a loaded control, installs the transition set of the pane state being
    /// changed to before the base member is called. After the base call it rebuilds or removes the transitions
    /// when <see cref="IsMotionEnabled" />, <c>PanePlacement</c> or a motion setting changes; this also acts only
    /// once the control is loaded. An override must call the base member.
    /// </summary>
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
