using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal class DrawerContainer : ContentControl
{
    #region 内部属性定义

    internal static readonly DirectProperty<DrawerContainer, DrawerPlacement> PlacementProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, DrawerPlacement>(nameof(Placement),
            o => o.Placement,
            (o, v) => o.Placement = v);

    internal static readonly DirectProperty<DrawerContainer, bool> IsShowCloseButtonProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, bool>(nameof(IsShowCloseButton),
            o => o.IsShowCloseButton,
            (o, v) => o.IsShowCloseButton = v);

    internal static readonly DirectProperty<DrawerContainer, bool> IsShowMaskProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, bool>(nameof(IsShowMask),
            o => o.IsShowMask,
            (o, v) => o.IsShowMask = v);

    internal static readonly DirectProperty<DrawerContainer, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, string>(nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);

    internal static readonly DirectProperty<DrawerContainer, object?> FooterProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, object?>(nameof(Footer),
            o => o.Footer,
            (o, v) => o.Footer = v);

    internal static readonly DirectProperty<DrawerContainer, IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, IDataTemplate?>(nameof(FooterTemplate),
            o => o.FooterTemplate,
            (o, v) => o.FooterTemplate = v);

    internal static readonly DirectProperty<DrawerContainer, object?> ExtraProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, object?>(nameof(Extra),
            o => o.Extra,
            (o, v) => o.Extra = v);

    internal static readonly DirectProperty<DrawerContainer, IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, IDataTemplate?>(nameof(ExtraTemplate),
            o => o.ExtraTemplate,
            (o, v) => o.ExtraTemplate = v);

    internal static readonly DirectProperty<DrawerContainer, double> DialogSizeProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, double>(nameof(DialogSize),
            o => o.DialogSize,
            (o, v) => o.DialogSize = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DrawerContainer>();

    internal static readonly DirectProperty<DrawerContainer, bool> CloseWhenClickOnMaskProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, bool>(nameof(CloseWhenClickOnMask),
            o => o.CloseWhenClickOnMask,
            (o, v) => o.CloseWhenClickOnMask = v);
    
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<DrawerContainer>();
    
    internal static readonly DirectProperty<DrawerContainer, double> PushOffsetPercentProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, double>(nameof(PushOffsetPercent),
            o => o.PushOffsetPercent,
            (o, v) => o.PushOffsetPercent = v);
    
    internal static readonly DirectProperty<DrawerContainer, IBrush?> MaskBgColorProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, IBrush?>(nameof(MaskBgColor),
            o => o.MaskBgColor,
            (o, v) => o.MaskBgColor = v);

    private DrawerPlacement _placement = DrawerPlacement.Right;

    internal DrawerPlacement Placement
    {
        get => _placement;
        set => SetAndRaise(PlacementProperty, ref _placement, value);
    }

    private bool _isShowMask;

    internal bool IsShowMask
    {
        get => _isShowMask;
        set => SetAndRaise(IsShowMaskProperty, ref _isShowMask, value);
    }

    private bool _isShowCloseButton = true;

    internal bool IsShowCloseButton
    {
        get => _isShowCloseButton;
        set => SetAndRaise(IsShowCloseButtonProperty, ref _isShowCloseButton, value);
    }

    private string _title = string.Empty;

    internal string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    private object? _footer;

    internal object? Footer
    {
        get => _footer;
        set => SetAndRaise(FooterProperty, ref _footer, value);
    }

    private IDataTemplate? _footerTemplate;

    internal IDataTemplate? FooterTemplate
    {
        get => _footerTemplate;
        set => SetAndRaise(FooterTemplateProperty, ref _footerTemplate, value);
    }

    private object? _extra;

    internal object? Extra
    {
        get => _extra;
        set => SetAndRaise(ExtraProperty, ref _extra, value);
    }

    private IDataTemplate? _extraTemplate;

    internal IDataTemplate? ExtraTemplate
    {
        get => _extraTemplate;
        set => SetAndRaise(ExtraTemplateProperty, ref _extraTemplate, value);
    }

    private double _dialogSize;

    internal double DialogSize
    {
        get => _dialogSize;
        set => SetAndRaise(DialogSizeProperty, ref _dialogSize, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _closeWhenClickOnMask;

    internal bool CloseWhenClickOnMask
    {
        get => _closeWhenClickOnMask;
        set => SetAndRaise(CloseWhenClickOnMaskProperty, ref _closeWhenClickOnMask, value);
    }
    
    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    private double _pushOffsetPercent;

    internal double PushOffsetPercent
    {
        get => _pushOffsetPercent;
        set => SetAndRaise(PushOffsetPercentProperty, ref _pushOffsetPercent, value);
    }
    
    private IBrush? _maskBgColor;

    internal IBrush? MaskBgColor
    {
        get => _maskBgColor;
        set => SetAndRaise(MaskBgColorProperty, ref _maskBgColor, value);
    }
    
    #endregion

    internal WeakReference<Drawer>? Drawer { get; set; }
    
    private BaseMotionActor? _motionActor;
    private DrawerInfoContainer? _infoContainer;
    private ITransform? _originInfoContainerTransform;
    private bool _openAnimating;
    private bool _closeAnimating;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetCurrentValue(BackgroundProperty, MaskBgColor);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        SetCurrentValue(BackgroundProperty, Brushes.Transparent);
    }

    internal void Open(ScopeAwareAdornerLayer layer)
    {
        if (Drawer != null && Drawer.TryGetTarget(out var drawer))
        {
            ScopeAwareAdornerLayer.SetAdornedElement(this, drawer.OpenOn);
            layer.Children.Add(this);
            Dispatcher.UIThread.Post(() =>
            {
                // 让 layer 更新
                if (_motionActor is null || _openAnimating)
                {
                    return;
                }
              
                if (!IsMotionEnabled)
                {
                    _motionActor.Opacity = 1.0;
                    drawer.NotifyOpened();
                    return;
                }

                _openAnimating         = true;
                _motionActor.Opacity = 0.0;
                
                LayoutHelper.MeasureChild(_motionActor, DesiredSize, new Thickness());
                
                var motion = BuildMotionByPlacement(Placement, MotionDuration, true);
           
                motion.Run(_motionActor, null,
                    () =>
                    {
                        _openAnimating = false;
                        drawer.NotifyOpened();
                       
                    });
            });
           
        }
    }

    internal void Close(ScopeAwareAdornerLayer layer)
    {
        if (Drawer != null && Drawer.TryGetTarget(out var drawer))
        {
            if (_motionActor is null || _closeAnimating)
            {
                return;
            }
        
            if (!IsMotionEnabled)
            {
                layer.Children.Remove(this);
                drawer.NotifyClosed();
                return;
            }
        
            _closeAnimating = true;
            var duration = TimeSpan.Zero;
            if (Transitions is not null)
            {
                foreach (var transition in Transitions)
                {
                    if (transition is TransitionBase transitionBase)
                    {
                        if (duration.CompareTo(transitionBase.Duration) < 0)
                        {
                            duration = transitionBase.Duration;
                        }
                    }
                }
            }
        
            SetValue(BackgroundProperty, Brushes.Transparent, BindingPriority.Template);
        
            var maskRunTaskSrc  = new TaskCompletionSource();
            var moveAnimTaskSrc = new TaskCompletionSource();
            var motion          = BuildMotionByPlacement(Placement, MotionDuration, false);
            motion.Run(_motionActor, null,
                () =>
                {
                    _closeAnimating = false;
                    _motionActor.Opacity = 0.0;
                    moveAnimTaskSrc.SetResult();
                });
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(duration);
                maskRunTaskSrc.SetResult();
            });
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.WhenAll(moveAnimTaskSrc.Task, moveAnimTaskSrc.Task);
                layer.Children.Remove(this);
                _motionActor.Opacity = 1.0;
                drawer.NotifyClosed();
            });
        }
    }

    private AbstractMotion BuildMotionByPlacement(DrawerPlacement placement, TimeSpan duration, bool isOpen)
    {
        AbstractMotion? motion = null;
        Debug.Assert(_motionActor != null);
        if (isOpen)
        {
            if (placement == DrawerPlacement.Left)
            {
                motion = new MoveLeftInMotion(_motionActor.DesiredSize.Width, duration, new CubicEaseOut());
            }
            else if (placement == DrawerPlacement.Right)
            {
                motion = new MoveRightInMotion(_motionActor.DesiredSize.Width, duration, new CubicEaseOut());
            }
            else if (placement == DrawerPlacement.Top)
            {
                motion = new MoveUpInMotion(_motionActor.DesiredSize.Height, duration, new CubicEaseOut());
            }
            else
            {
                motion = new MoveDownInMotion(_motionActor.DesiredSize.Height, duration, new CubicEaseOut());
            }
        }
        else
        {
            if (placement == DrawerPlacement.Left)
            {
                motion = new MoveLeftOutMotion(_motionActor.DesiredSize.Width, duration, new CubicEaseOut());
            }
            else if (placement == DrawerPlacement.Right)
            {
                motion = new MoveRightOutMotion(_motionActor.DesiredSize.Width, duration, new CubicEaseOut());
            }
            else if (placement == DrawerPlacement.Top)
            {
                motion = new MoveUpOutMotion(_motionActor.DesiredSize.Height, duration, new CubicEaseOut());
            }
            else
            {
                motion = new MoveDownOutMotion(_motionActor.DesiredSize.Height, duration, new CubicEaseOut());
            }
        }

        Debug.Assert(motion != null);
        return motion;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (CloseWhenClickOnMask)
        {
            if (Drawer != null && Drawer.TryGetTarget(out var drawer))
            {
                drawer.IsOpen = false;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _motionActor   = e.NameScope.Find<BaseMotionActor>(DrawerContainerThemeConstants.InfoContainerMotionActorPart);
        
        if (_infoContainer != null)
        {
            _infoContainer.CloseRequested -= HandleCloseRequested;
        }

        _infoContainer = e.NameScope.Find<DrawerInfoContainer>(DrawerContainerThemeConstants.InfoContainerPart);
        if (_infoContainer != null)
        {
            _infoContainer.CloseRequested += HandleCloseRequested;
        }
    }

    private void HandleCloseRequested(object? sender, EventArgs e)
    {
        if (Drawer != null && Drawer.TryGetTarget(out var drawer))
        {
            drawer.IsOpen = false;
        }
    }

    internal void NotifyChildDrawerAboutToOpen(Drawer childDrawer)
    {
        if (_infoContainer != null)
        {
            var builder = new TransformOperations.Builder(1);
            if (Placement != childDrawer.Placement)
            {
                return;
            }
        
            double offsetX = 0d;
            double offsetY = 0d;
        
            if (Placement == DrawerPlacement.Left)
            {
                offsetX = DesiredSize.Width * PushOffsetPercent;
            }
            else if (Placement == DrawerPlacement.Right)
            {
                offsetX = -DesiredSize.Width * PushOffsetPercent;
            }
            else if (Placement == DrawerPlacement.Top)
            {
                offsetY = DesiredSize.Height * PushOffsetPercent;
            }
            else
            {
                offsetY = -DesiredSize.Height * PushOffsetPercent;
            }
            builder.AppendTranslate(offsetX, offsetY);
            _originInfoContainerTransform  = _infoContainer.RenderTransform;
            _infoContainer.RenderTransform = builder.Build();
        }
    }

    internal void NotifyChildDrawerAboutToClose(Drawer childDrawer)
    {
        if (_infoContainer != null)
        {
            if (Placement != childDrawer.Placement)
            {
                return;
            }
            _infoContainer.RenderTransform = _originInfoContainerTransform;
        }
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

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BackgroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
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
}