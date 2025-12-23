using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
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
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class Carousel : SelectingItemsControl, 
                        IControlSharedTokenResourcesHost,
                        IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsShowNavButtonsProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowNavButtons), false);
    
    public static readonly StyledProperty<bool> IsAutoPlayProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsAutoPlay));
    
    public static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty = 
        AvaloniaProperty.Register<Carousel, TimeSpan>(nameof(AutoPlaySpeed), TimeSpan.FromMilliseconds(3000));
    
    public static readonly StyledProperty<CarouselPaginationPosition> PaginationPositionProperty = 
        AvaloniaProperty.Register<Carousel, CarouselPaginationPosition>(nameof(PaginationPosition), CarouselPaginationPosition.Bottom);
    
    public static readonly StyledProperty<bool> IsShowPaginationProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowPagination), true);
    
    public static readonly StyledProperty<bool> IsShowTransitionProgressProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowTransitionProgress), false);
    
    public static readonly StyledProperty<bool> IsInfiniteProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsInfinite), true);
    
    public static readonly StyledProperty<TimeSpan> PageTransitionDurationProperty = 
        AvaloniaProperty.Register<Carousel, TimeSpan>(nameof(PageTransitionDuration));
    
    public static readonly StyledProperty<Easing> PageInEasingProperty = 
        AvaloniaProperty.Register<Carousel, Easing>(nameof(PageInEasing));
    
    public static readonly StyledProperty<Easing> PageOutEasingProperty = 
        AvaloniaProperty.Register<Carousel, Easing>(nameof(PageOutEasing));
    
    public static readonly StyledProperty<CarouselTransitionEffect> TransitionEffectProperty = 
        AvaloniaProperty.Register<Carousel, CarouselTransitionEffect>(nameof(TransitionEffect));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Carousel>();

    public static readonly StyledProperty<bool> IsSwipeEnabledProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsSwipeEnabled));
    
    public bool IsShowNavButtons
    {
        get => GetValue(IsShowNavButtonsProperty);
        set => SetValue(IsShowNavButtonsProperty, value);
    }
    
    public bool IsAutoPlay
    {
        get => GetValue(IsAutoPlayProperty);
        set => SetValue(IsAutoPlayProperty, value);
    }
    
    public TimeSpan AutoPlaySpeed
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
    
    public bool IsInfinite
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
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsSwipeEnabled
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CarouselToken.ID;

    #endregion
    
    private IScrollable? _scroller;
    private CarouselPagination? _pagination;
    private DispatcherTimer? _autoPlayTimer;
    private IconButton? _previousButton;
    private IconButton? _nextButton;
    private bool _isPointerGestureActive;
    private Point _pointerPressPoint;
    private const double SwipeGestureThreshold = 30;
    private bool _isSwipeCursorActive;
    
    static Carousel()
    {
        SelectionModeProperty.OverrideDefaultValue<Carousel>(SelectionMode.AlwaysSelected);
        ItemsPanelProperty.OverrideDefaultValue<Carousel>(DefaultPanel);
        AffectsArrange<Carousel>(SelectedIndexProperty);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<Carousel>(false);
    }

    public Carousel()
    {
        this.RegisterResources();
        SelectionChanged += HandleSelectionChanged;
    }
    
    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        SyncPagination();
    }
    
    public void Next()
    {
        if (!IsInfinite)
        {
            if (SelectedIndex < ItemCount - 1)
            {
                ++SelectedIndex;
            }
        }
        else
        {
            SelectedIndex = (SelectedIndex + 1) % ItemCount;
        }
    }
    
    public void Previous()
    {
        if (SelectedIndex > 0)
        {
            --SelectedIndex;
        }
        else if (IsInfinite)
        {
            SelectedIndex = ItemCount - 1;
        }
    }
    
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
        base.OnApplyTemplate(e);
        _scroller       = e.NameScope.Find<IScrollable>(CarouselThemeConstants.ScrollViewerPart);
        _pagination     = e.NameScope.Find<CarouselPagination>(CarouselThemeConstants.PaginationPart);
        _previousButton = e.NameScope.Find<IconButton>(CarouselThemeConstants.PreviousButtonPart);
        _nextButton     = e.NameScope.Find<IconButton>(CarouselThemeConstants.NextButtonPart);
        SyncPagination();
        if (_pagination != null)
        {
            BindUtils.RelayBind(this, SelectedIndexProperty, _pagination, SelectedIndexProperty, BindingMode.TwoWay);
        }
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
        Previous();
    }

    private void HandleNextButtonClick(object? sender, RoutedEventArgs args)
    {
        Next();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!IsSwipeEnabled)
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

        _isPointerGestureActive = true;
        _pointerPressPoint      = point.Position;
        e.Pointer.Capture(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!IsSwipeEnabled || !_isPointerGestureActive)
        {
            return;
        }

        var handled = TryHandleSwipe(e.GetPosition(this));
        _isPointerGestureActive = false;
        e.Pointer.Capture(null);
        if (handled)
        {
            e.Handled = true;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _isPointerGestureActive = false;
    }

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
        else if (change.Property == IsAutoPlayProperty)
        {
            BuildAutoPlayTimer();
        }
        else if (change.Property == AutoPlaySpeedProperty)
        {
            ConfigureAutoPlayTimer();
        }
        else if (change.Property == IsSwipeEnabledProperty)
        {
            UpdateSwipeCursor();
        }
        else if (change.Property == SelectedIndexProperty)
        {
            if (_autoPlayTimer != null && _autoPlayTimer.IsEnabled)
            {
                _autoPlayTimer?.Stop();
                _autoPlayTimer?.Start();
            }
        }

        if (change.Property == IsAutoPlayProperty ||
            change.Property == IsShowTransitionProgressProperty)
        {
            if (IsAutoPlay)
            {
                SetCurrentValue(IsEffectiveShowTransitionProgressProperty, IsShowTransitionProgress);
            }
            else
            {
                SetCurrentValue(IsEffectiveShowTransitionProgressProperty, false);
            }
        }
    }

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
                    Next();
                }
                else
                {
                    Previous();
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
                    Next();
                }
                else
                {
                    Previous();
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
        base.OnAttachedToVisualTree(e);
        if (IsAutoPlay)
        {
            _autoPlayTimer?.Start();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (IsAutoPlay)
        {
            _autoPlayTimer?.Stop();
        }
    }

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

    private void SyncPagination()
    {
        if (_pagination != null)
        {
            var paginationCount = _pagination.ItemCount;
            var pageCount       = ItemCount;
            if (paginationCount != pageCount)
            {
                if (pageCount < paginationCount)
                {
                    var delta = paginationCount - pageCount;
                    for (var i = 0; i < delta; ++i)
                    {
                        _pagination.Items.RemoveAt(_pagination.Items.Count - 1);
                    }
                }
                else if (pageCount > paginationCount)
                {
                    var delta = pageCount - paginationCount;
                    for (var i = 0; i < delta; ++i)
                    {
                        _pagination.Items.Add(new CarouselPageIndicator());
                    }
                }
            }
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

    private void BuildAutoPlayTimer()
    {
        if (IsAutoPlay)
        {
            _autoPlayTimer      =  new DispatcherTimer();
            _autoPlayTimer.Tick += HandleAutoPlayTick;
            ConfigureAutoPlayTimer();
        }
        else
        {
            _autoPlayTimer?.Stop();
            _autoPlayTimer = null;
        }
    }

    private void HandleAutoPlayTick(object? sender, EventArgs e)
    {
        Next();
    }
    
    private void ConfigureAutoPlayTimer()
    {
        if (_autoPlayTimer != null)
        {
            _autoPlayTimer.Interval = AutoPlaySpeed;
        }
    }
}
