using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls;

public class Carousel : AbstractCarousel
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsShowNavButtonsProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowNavButtons), false);
    
    public new static readonly StyledProperty<bool> IsAutoPlayProperty =
        AbstractCarousel.IsAutoPlayProperty.AddOwner<Carousel>();
    
    public new static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty =
        AbstractCarousel.AutoPlaySpeedProperty.AddOwner<Carousel>();
    
    public static readonly StyledProperty<CarouselPaginationPosition> PaginationPositionProperty = 
        AvaloniaProperty.Register<Carousel, CarouselPaginationPosition>(nameof(PaginationPosition), CarouselPaginationPosition.Bottom);
    
    public static readonly StyledProperty<bool> IsShowPaginationProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowPagination), true);
    
    public static readonly StyledProperty<bool> IsShowTransitionProgressProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowTransitionProgress), false);
    
    public new static readonly StyledProperty<bool> IsInfiniteProperty =
        AbstractCarousel.IsInfiniteProperty.AddOwner<Carousel>();
    
    public static readonly StyledProperty<TimeSpan> PageTransitionDurationProperty = 
        AvaloniaProperty.Register<Carousel, TimeSpan>(nameof(PageTransitionDuration));
    
    public static readonly StyledProperty<Easing> PageInEasingProperty = 
        AvaloniaProperty.Register<Carousel, Easing>(nameof(PageInEasing));
    
    public static readonly StyledProperty<Easing> PageOutEasingProperty = 
        AvaloniaProperty.Register<Carousel, Easing>(nameof(PageOutEasing));
    
    public static readonly StyledProperty<CarouselTransitionEffect> TransitionEffectProperty = 
        AvaloniaProperty.Register<Carousel, CarouselTransitionEffect>(nameof(TransitionEffect));
    
    public new static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AbstractCarousel.IsMotionEnabledProperty.AddOwner<Carousel>();

    public new static readonly StyledProperty<bool> IsSwipeEnabledProperty =
        AbstractCarousel.IsSwipeEnabledProperty.AddOwner<Carousel>();
    
    public bool IsShowNavButtons
    {
        get => GetValue(IsShowNavButtonsProperty);
        set => SetValue(IsShowNavButtonsProperty, value);
    }
    
    public new bool IsAutoPlay
    {
        get => GetValue(IsAutoPlayProperty);
        set => SetValue(IsAutoPlayProperty, value);
    }
    
    public new TimeSpan AutoPlaySpeed
    {
        get => GetValue(AutoPlaySpeedProperty);
        set => SetValue(AutoPlaySpeedProperty, value);
    }
    
    public CarouselPaginationPosition PaginationPosition
    {
        get => GetValue(PaginationPositionProperty);
        set => SetValue(PaginationPositionProperty, value);
    }
    
    public bool IsShowPagination
    {
        get => GetValue(IsShowPaginationProperty);
        set => SetValue(IsShowPaginationProperty, value);
    }
    
    public bool IsShowTransitionProgress
    {
        get => GetValue(IsShowTransitionProgressProperty);
        set => SetValue(IsShowTransitionProgressProperty, value);
    }
    
    public new bool IsInfinite
    {
        get => GetValue(IsInfiniteProperty);
        set => SetValue(IsInfiniteProperty, value);
    }
    
    public TimeSpan PageTransitionDuration
    {
        get => GetValue(PageTransitionDurationProperty);
        set => SetValue(PageTransitionDurationProperty, value);
    }

    public Easing PageInEasing
    {
        get => GetValue(PageInEasingProperty);
        set => SetValue(PageInEasingProperty, value);
    }

    public Easing PageOutEasing
    {
        get => GetValue(PageOutEasingProperty);
        set => SetValue(PageOutEasingProperty, value);
    }

    public CarouselTransitionEffect TransitionEffect
    {
        get => GetValue(TransitionEffectProperty);
        set => SetValue(TransitionEffectProperty, value);
    }
    
    public new bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public new bool IsSwipeEnabled
    {
        get => GetValue(IsSwipeEnabledProperty);
        set => SetValue(IsSwipeEnabledProperty, value);
    }
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<IPageTransition?> PageTransitionProperty =
        AvaloniaProperty.Register<Carousel, IPageTransition?>(nameof(PageTransition));
    
    internal static readonly DirectProperty<Carousel, bool> PreviousNavButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<Carousel, bool>(
            nameof(PreviousNavButtonVisible),
            o => o.PreviousNavButtonVisible,
            (o, v) => o.PreviousNavButtonVisible = v);
    
    internal static readonly DirectProperty<Carousel, bool> NextNavButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<Carousel, bool>(
            nameof(NextNavButtonVisible),
            o => o.NextNavButtonVisible,
            (o, v) => o.NextNavButtonVisible = v);
    
    internal static readonly DirectProperty<Carousel, Thickness> EffectivePaginationMarginProperty =
        AvaloniaProperty.RegisterDirect<Carousel, Thickness>(
            nameof(EffectivePaginationMargin),
            o => o.EffectivePaginationMargin,
            (o, v) => o.EffectivePaginationMargin = v);
    
    internal static readonly DirectProperty<Carousel, Thickness> EffectivePreviousButtonMarginProperty =
        AvaloniaProperty.RegisterDirect<Carousel, Thickness>(
            nameof(EffectivePreviousButtonMargin),
            o => o.EffectivePreviousButtonMargin,
            (o, v) => o.EffectivePreviousButtonMargin = v);
    
    internal static readonly DirectProperty<Carousel, Thickness> EffectiveNextButtonMarginProperty =
        AvaloniaProperty.RegisterDirect<Carousel, Thickness>(
            nameof(EffectiveNextButtonMargin),
            o => o.EffectiveNextButtonMargin,
            (o, v) => o.EffectiveNextButtonMargin = v);
    
    internal static readonly StyledProperty<double> PaginationOffsetProperty =
        AvaloniaProperty.Register<Carousel, double>(nameof(PaginationOffset));
    
    internal static readonly DirectProperty<Carousel, bool> IsEffectiveShowTransitionProgressProperty =
        AvaloniaProperty.RegisterDirect<Carousel, bool>(
            nameof(IsEffectiveShowTransitionProgress),
            o => o.IsEffectiveShowTransitionProgress,
            (o, v) => o.IsEffectiveShowTransitionProgress = v);
    
    internal static readonly DirectProperty<Carousel, IList<object>?> IndicatorItemsProperty =
        AvaloniaProperty.RegisterDirect<Carousel, IList<object>?>(
            nameof(IndicatorItems),
            o => o.IndicatorItems,
            (o, v) => o.IndicatorItems = v);
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingCarouselPanel());
    
    public IPageTransition? PageTransition
    {
        get => GetValue(PageTransitionProperty);
        set => SetValue(PageTransitionProperty, value);
    }

    private bool _previousNavButtonVisible;

    internal bool PreviousNavButtonVisible
    {
        get => _previousNavButtonVisible;
        set => SetAndRaise(PreviousNavButtonVisibleProperty, ref _previousNavButtonVisible, value);
    }
    
    private bool _nextNavButtonVisible;

    internal bool NextNavButtonVisible
    {
        get => _nextNavButtonVisible;
        set => SetAndRaise(NextNavButtonVisibleProperty, ref _nextNavButtonVisible, value);
    }
    
    private Thickness _effectivePaginationMargin;

    internal Thickness EffectivePaginationMargin
    {
        get => _effectivePaginationMargin;
        set => SetAndRaise(EffectivePaginationMarginProperty, ref _effectivePaginationMargin, value);
    }
    
    private Thickness _effectivePreviousButtonMargin;

    internal Thickness EffectivePreviousButtonMargin
    {
        get => _effectivePreviousButtonMargin;
        set => SetAndRaise(EffectivePaginationMarginProperty, ref _effectivePreviousButtonMargin, value);
    }
    
    private Thickness _effectiveNextButtonMargin;

    internal Thickness EffectiveNextButtonMargin
    {
        get => _effectiveNextButtonMargin;
        set => SetAndRaise(EffectivePaginationMarginProperty, ref _effectiveNextButtonMargin, value);
    }
    
    private bool _isEffectiveShowTransitionProgress;

    internal bool IsEffectiveShowTransitionProgress
    {
        get => _isEffectiveShowTransitionProgress;
        set => SetAndRaise(IsEffectiveShowTransitionProgressProperty, ref _isEffectiveShowTransitionProgress, value);
    }
    
    internal double PaginationOffset
    {
        get => GetValue(PaginationOffsetProperty);
        set => SetValue(PaginationOffsetProperty, value);
    }
    
    private IList<object>? _indicatorItems;

    internal IList<object>? IndicatorItems
    {
        get => _indicatorItems;
        set => SetAndRaise(IndicatorItemsProperty, ref _indicatorItems, value);
    }

    #endregion
    
    private IScrollable? _scroller;
    private CarouselPagination? _pagination;
    private AvaloniaWindow? _window;
    private IconButton? _previousButton;
    private IconButton? _nextButton;
    private IPointer? _swipePointer;
    private Point _pointerPressPoint;
    private const double SwipeGestureThreshold = 30;
    private bool _isSwipeCursorActive;
    
    static Carousel()
    {
        ItemsPanelProperty.OverrideDefaultValue<Carousel>(DefaultPanel);
        AffectsArrange<Carousel>(SelectedIndexProperty);
        ItemCountProperty.Changed.AddClassHandler<Carousel>((carousel, args) => carousel.HandleItemCountChanged(args.GetNewValue<int>()));
    }

    public Carousel()
    {
    }
    
    private void HandleItemCountChanged(int count)
    {
        var items = count == 0 ? Array.Empty<object>() : new object[count];
        for (var i = 0; i < count; i++)
        {
            items[i] = new object();
        }
        SetCurrentValue(IndicatorItemsProperty, items);
    }
    
    public new void Next() => base.Next();

    public new void Previous() => base.Previous();

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);

        if (_scroller is not null)
        {
            _scroller.Offset = new(SelectedIndex, 0);
        }

        return result;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CancelSwipe();
        if (_pagination is not null)
        {
            _pagination.Owner = null;
        }
        if (_previousButton is not null)
        {
            _previousButton.Click -= HandlePreviousButtonClick;
        }
        if (_nextButton is not null)
        {
            _nextButton.Click -= HandleNextButtonClick;
        }
        base.OnApplyTemplate(e);
        _scroller       = e.NameScope.Find<IScrollable>("PART_ScrollViewer");
        _pagination     = e.NameScope.Find<CarouselPagination>("PART_Pagination");
        if (_pagination is not null)
        {
            _pagination.Owner = this;
        }
        _previousButton = e.NameScope.Find<IconButton>("PART_PreviousButton");
        _nextButton     = e.NameScope.Find<IconButton>("PART_NextButton");
        BuildEffectivePageTransition(false);
        ConfigureNavButtons();
        if (_previousButton != null)
        {
            _previousButton.Click += HandlePreviousButtonClick;
        }

        if (_nextButton != null)
        {
            _nextButton.Click += HandleNextButtonClick;
        }
    }

    private void HandlePreviousButtonClick(object? sender, RoutedEventArgs args)
    {
        MoveBy(-1, CarouselChangeSource.User);
    }

    private void HandleNextButtonClick(object? sender, RoutedEventArgs args)
    {
        MoveBy(1, CarouselChangeSource.User);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!IsSwipeEnabled || _swipePointer is not null)
        {
            return;
        }

        var point = e.GetCurrentPoint(this);
        var isPrimaryPointer = e.Pointer.Type != PointerType.Mouse || point.Properties.IsLeftButtonPressed;

        if (!isPrimaryPointer)
        {
            return;
        }

        if (ShouldIgnorePointerSource(e.Source))
        {
            return;
        }

        _swipePointer = e.Pointer;
        _pointerPressPoint = point.Position;
        e.Pointer.Capture(this);
        SetAutoPlayInteractionPaused(true);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!ReferenceEquals(_swipePointer, e.Pointer))
        {
            return;
        }

        var handled = IsSwipeEnabled && TryHandleSwipe(e.GetPosition(this));
        CancelSwipe();
        if (handled)
        {
            e.Handled = true;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (ReferenceEquals(_swipePointer, e.Pointer))
        {
            CancelSwipe();
        }
    }

    private void CancelSwipe()
    {
        var pointer = _swipePointer;
        _swipePointer = null;
        if (pointer?.Captured == this)
        {
            pointer.Capture(null);
        }
        SetAutoPlayInteractionPaused(false);
    }

    internal void SelectPageFromPagination(int index) => RequestPageChange(index, CarouselChangeSource.User);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedIndexProperty)
        {
            if (_scroller is not null)
            {
                var value = change.GetNewValue<int>();
                _scroller.Offset = new(value, 0);
            }
            ConfigureNavButtons();
        }
        else if (change.Property == IsInfiniteProperty ||
                 change.Property == IsShowNavButtonsProperty ||
                 change.Property == ItemCountProperty)
        {
            ConfigureNavButtons();
        }
        else if (change.Property == PaginationPositionProperty ||
                 change.Property == PaginationOffsetProperty)
        {
            ConfigurePaginationMargin();
            ConfigureNavButtonsMargin();
        }
        else if (change.Property == TransitionEffectProperty)
        {
            BuildEffectivePageTransition(true);
        }

        if (change.Property == PaginationPositionProperty)
        {
            ConfigureEffectivePageTransition();
            if (IsSwipeEnabled)
            {
                UpdateSwipeCursor();
            }
        }
        else if (change.Property == IsSwipeEnabledProperty)
        {
            UpdateSwipeCursor();
            if (!IsSwipeEnabled)
            {
                CancelSwipe();
            }
        }

        if (change.Property == IsShowTransitionProgressProperty)
        {
            UpdateTransitionProgressVisibility();
        }
    }

    protected override void OnAutoPlayRunningChanged(bool isRunning) => UpdateTransitionProgressVisibility();

    private void UpdateTransitionProgressVisibility() =>
        SetCurrentValue(IsEffectiveShowTransitionProgressProperty, IsAutoPlayTimerRunning && IsShowTransitionProgress);

    private bool TryHandleSwipe(Point releasePoint)
    {
        var delta       = releasePoint - _pointerPressPoint;
        var orientation = GetSwipeOrientation();

        if (orientation == Orientation.Horizontal)
        {
            if (Math.Abs(delta.X) >= SwipeGestureThreshold &&
                Math.Abs(delta.X) > Math.Abs(delta.Y))
            {
                if (delta.X < 0)
                {
                    MoveBy(1, CarouselChangeSource.User);
                }
                else
                {
                    MoveBy(-1, CarouselChangeSource.User);
                }

                return true;
            }
        }
        else
        {
            if (Math.Abs(delta.Y) >= SwipeGestureThreshold &&
                Math.Abs(delta.Y) > Math.Abs(delta.X))
            {
                if (delta.Y < 0)
                {
                    MoveBy(1, CarouselChangeSource.User);
                }
                else
                {
                    MoveBy(-1, CarouselChangeSource.User);
                }

                return true;
            }
        }

        return false;
    }

    private Orientation GetSwipeOrientation()
    {
        if (PaginationPosition == CarouselPaginationPosition.Left ||
            PaginationPosition == CarouselPaginationPosition.Right)
        {
            return Orientation.Vertical;
        }

        return Orientation.Horizontal;
    }

    private bool ShouldIgnorePointerSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        if (_previousButton != null &&
            (_previousButton == visual || _previousButton.IsVisualAncestorOf(visual)))
        {
            return true;
        }

        if (_nextButton != null &&
            (_nextButton == visual || _nextButton.IsVisualAncestorOf(visual)))
        {
            return true;
        }

        if (_pagination != null &&
            (_pagination == visual || _pagination.IsVisualAncestorOf(visual)))
        {
            return true;
        }

        return false;
    }

    private void UpdateSwipeCursor()
    {
        if (IsSwipeEnabled)
        {
            SetCurrentValue(CursorProperty, new Cursor(StandardCursorType.DragMove));
            _isSwipeCursorActive = true;
        }
        else if (_isSwipeCursorActive)
        {
            SetCurrentValue(CursorProperty, Cursor.Default);
            _isSwipeCursorActive = false;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _window = TopLevel.GetTopLevel(this) as AvaloniaWindow;
        if (_window is not null)
        {
            _window.PropertyChanged += HandleHostWindowPropertyChanged;
        }
        UpdateHostWindowAvailability();
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_window is not null)
        {
            _window.PropertyChanged -= HandleHostWindowPropertyChanged;
            _window = null;
        }
        base.OnDetachedFromVisualTree(e);
        CancelSwipe();
    }

    private void HandleHostWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AvaloniaWindow.WindowStateProperty)
        {
            UpdateHostWindowAvailability();
        }
    }

    private void UpdateHostWindowAvailability() => SetAutoPlayHostAvailable(_window?.WindowState != WindowState.Minimized);

    private void ConfigureNavButtons()
    {
        if (IsShowNavButtons)
        {
            if (IsInfinite)
            {
                SetCurrentValue(PreviousNavButtonVisibleProperty, true);
                SetCurrentValue(NextNavButtonVisibleProperty, true);
            }
            else
            {
                SetCurrentValue(PreviousNavButtonVisibleProperty, SelectedIndex != 0);
                SetCurrentValue(NextNavButtonVisibleProperty, SelectedIndex != ItemCount - 1);
            }
        }
        else
        {
            SetCurrentValue(PreviousNavButtonVisibleProperty, false);
            SetCurrentValue(NextNavButtonVisibleProperty, false);
        }
    }

    private void ConfigurePaginationMargin()
    {
        if (PaginationPosition == CarouselPaginationPosition.Bottom)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(0, 0, 0, PaginationOffset));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Top)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(0, PaginationOffset, 0, 0));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Left)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(PaginationOffset, 0, 0, 0));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Right)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(0, 0, PaginationOffset, 0));
        }
    }

    private void ConfigureNavButtonsMargin()
    {
        if (PaginationPosition == CarouselPaginationPosition.Bottom ||
            PaginationPosition == CarouselPaginationPosition.Top)
        {
            SetCurrentValue(EffectivePreviousButtonMarginProperty, new Thickness(PaginationOffset, 0, 0, 0));
            SetCurrentValue(EffectiveNextButtonMarginProperty, new Thickness(0, 0, PaginationOffset, 0));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Left ||
                 PaginationPosition == CarouselPaginationPosition.Right)
        {
            SetCurrentValue(EffectivePreviousButtonMarginProperty, new Thickness(0, PaginationOffset, 0, 0));
            SetCurrentValue(EffectiveNextButtonMarginProperty, new Thickness(0, 0, 0, PaginationOffset));
        }
    }

    private void BuildEffectivePageTransition(bool force)
    {
        if (PageTransition == null || force)
        {
            if (TransitionEffect == CarouselTransitionEffect.Fade)
            {
                SetCurrentValue(PageTransitionProperty, new CrossFade());
            }
            else
            {
                SetCurrentValue(PageTransitionProperty, new PageSlide());
            }
        }
        ConfigureEffectivePageTransition();
    }

    private void ConfigureEffectivePageTransition()
    {
        if (PageTransition is CrossFade crossFade)
        {
            crossFade.FadeInEasing  = PageInEasing;
            crossFade.FadeOutEasing = PageOutEasing;
            crossFade.Duration      = PageTransitionDuration;
        }
        else if (PageTransition is PageSlide pageSlide)
        {
            pageSlide.SlideInEasing  = PageInEasing;
            pageSlide.SlideOutEasing = PageOutEasing;
            pageSlide.Duration      = PageTransitionDuration;
            if (PaginationPosition == CarouselPaginationPosition.Bottom ||
                PaginationPosition == CarouselPaginationPosition.Top)
            {
                pageSlide.Orientation = PageSlide.SlideAxis.Horizontal;
            }
            else if (PaginationPosition == CarouselPaginationPosition.Left ||
                     PaginationPosition == CarouselPaginationPosition.Right)
            {
                pageSlide.Orientation = PageSlide.SlideAxis.Vertical;
            }
        }
    }
}
