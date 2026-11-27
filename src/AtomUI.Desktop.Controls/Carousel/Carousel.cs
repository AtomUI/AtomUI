using System.Globalization;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
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
using Avalonia.VisualTree;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A desktop carousel that shows one item per page and changes the page through autoplay, the previous/next
/// buttons, the pagination indicator or a pointer swipe.
/// <see cref="AbstractCarousel" /> owns the paging and autoplay rules. This class decides which of these input
/// methods are available and where the buttons and the indicator are placed.
/// </summary>
public class Carousel : AbstractCarousel
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsShowNavButtons" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowNavButtonsProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowNavButtons), false);
    
    /// <summary>
    /// Identifies the <see cref="IsAutoPlay" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsAutoPlayProperty =
        AbstractCarousel.IsAutoPlayProperty.AddOwner<Carousel>();
    
    /// <summary>
    /// Identifies the <see cref="AutoPlaySpeed" /> property.
    /// </summary>
    public new static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty =
        AbstractCarousel.AutoPlaySpeedProperty.AddOwner<Carousel>();
    
    /// <summary>
    /// Identifies the <see cref="PaginationPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<CarouselPaginationPosition> PaginationPositionProperty = 
        AvaloniaProperty.Register<Carousel, CarouselPaginationPosition>(nameof(PaginationPosition), CarouselPaginationPosition.Bottom);
    
    /// <summary>
    /// Identifies the <see cref="IsShowPagination" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowPaginationProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowPagination), true);
    
    /// <summary>
    /// Identifies the <see cref="IsShowTransitionProgress" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowTransitionProgressProperty = 
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsShowTransitionProgress), false);
    
    /// <summary>
    /// Identifies the <see cref="IsInfinite" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsInfiniteProperty =
        AbstractCarousel.IsInfiniteProperty.AddOwner<Carousel>();
    
    /// <summary>
    /// Identifies the <see cref="PageTransitionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> PageTransitionDurationProperty = 
        AvaloniaProperty.Register<Carousel, TimeSpan>(nameof(PageTransitionDuration));
    
    /// <summary>
    /// Identifies the <see cref="PageInEasing" /> property.
    /// </summary>
    public static readonly StyledProperty<Easing> PageInEasingProperty = 
        AvaloniaProperty.Register<Carousel, Easing>(nameof(PageInEasing));
    
    /// <summary>
    /// Identifies the <see cref="PageOutEasing" /> property.
    /// </summary>
    public static readonly StyledProperty<Easing> PageOutEasingProperty = 
        AvaloniaProperty.Register<Carousel, Easing>(nameof(PageOutEasing));
    
    /// <summary>
    /// Identifies the <see cref="TransitionEffect" /> property.
    /// </summary>
    public static readonly StyledProperty<CarouselTransitionEffect> TransitionEffectProperty = 
        AvaloniaProperty.Register<Carousel, CarouselTransitionEffect>(nameof(TransitionEffect));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AbstractCarousel.IsMotionEnabledProperty.AddOwner<Carousel>();

    /// <summary>
    /// Identifies the <see cref="IsSwipeEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsSwipeEnabledProperty =
        AbstractCarousel.IsSwipeEnabledProperty.AddOwner<Carousel>();
    
    /// <summary>
    /// Gets or sets whether the previous/next buttons are shown; one click moves one page. The default is
    /// <see langword="false" />.
    /// While <see cref="IsInfinite" /> is <see langword="false" />, the previous button is hidden on the first page
    /// and the next button on the last page; while it is <see langword="true" />, both are shown on every page.
    /// </summary>
    public bool IsShowNavButtons
    {
        get => GetValue(IsShowNavButtonsProperty);
        set => SetValue(IsShowNavButtonsProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the carousel advances to the next page by itself. The value is shared with
    /// <see cref="AbstractCarousel.IsAutoPlay" />. The default is <see langword="false" />.
    /// Autoplay runs only while the control is attached, enabled and visible, <see cref="IsMotionEnabled" /> is
    /// <see langword="true" />, there are at least two pages and nothing pauses it. In this class a tracked swipe
    /// and a minimized host window keep it from running until the swipe ends or the window is restored.
    /// </summary>
    public new bool IsAutoPlay
    {
        get => GetValue(IsAutoPlayProperty);
        set => SetValue(IsAutoPlayProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how long each page stays on screen during autoplay. The value is shared with
    /// <see cref="AbstractCarousel.AutoPlaySpeed" />. The default is 3 seconds.
    /// A value of zero or less keeps autoplay from running. The timer applies a change immediately, also while
    /// autoplay runs. The theme forwards the value to the pagination indicator, and the autoplay progress animation
    /// on the current page takes as long.
    /// </summary>
    public new TimeSpan AutoPlaySpeed
    {
        get => GetValue(AutoPlaySpeedProperty);
        set => SetValue(AutoPlaySpeedProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the edge of the carousel on which the pagination indicator is placed. The default is
    /// <see cref="CarouselPaginationPosition.Bottom" />.
    /// The previous/next buttons sit at the ends of the same axis: left and right for Top and Bottom, top and
    /// bottom for Left and Right. The value also selects the axis of the swipe gesture and of the
    /// <see cref="CarouselTransitionEffect.Scroll" /> transition.
    /// </summary>
    public CarouselPaginationPosition PaginationPosition
    {
        get => GetValue(PaginationPositionProperty);
        set => SetValue(PaginationPositionProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the pagination indicator is shown. The default is <see langword="true" />.
    /// </summary>
    public bool IsShowPagination
    {
        get => GetValue(IsShowPaginationProperty);
        set => SetValue(IsShowPaginationProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the pagination indicator shows the autoplay progress on the current page. The default
    /// is <see langword="false" />.
    /// The progress shows only while the autoplay timer runs, so it also needs <see cref="IsAutoPlay" /> to be
    /// <see langword="true" />. One progress run takes <see cref="AutoPlaySpeed" />.
    /// </summary>
    public bool IsShowTransitionProgress
    {
        get => GetValue(IsShowTransitionProgressProperty);
        set => SetValue(IsShowTransitionProgressProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the carousel loops. The value is shared with <see cref="AbstractCarousel.IsInfinite" />.
    /// The default is <see langword="true" />.
    /// While <see langword="true" />, moving past the last page continues with the first page and moving before the
    /// first page continues with the last page. While <see langword="false" />, <see cref="Next" /> and
    /// <see cref="Previous" /> stop at the last and first page, and so do the buttons and the swipe gesture.
    /// While <see cref="IsShowNavButtons" /> is <see langword="true" />, the buttons are shown on every page.
    /// </summary>
    public new bool IsInfinite
    {
        get => GetValue(IsInfiniteProperty);
        set => SetValue(IsInfiniteProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how long the page transition takes. The theme sets the value from the <c>MotionDurationSlow</c>
    /// token of the carousel tokens; clearing a local value returns to the token.
    /// The carousel writes the value into the transition when it builds or reconfigures the transition: when the
    /// template is applied and when <see cref="TransitionEffect" /> or <see cref="PaginationPosition" /> changes.
    /// </summary>
    public TimeSpan PageTransitionDuration
    {
        get => GetValue(PageTransitionDurationProperty);
        set => SetValue(PageTransitionDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the easing of the incoming page in the page transition. The theme sets the value to
    /// <c>CubicEaseOut</c>; clearing a local value returns to it.
    /// The carousel writes the value into the transition when it builds or reconfigures the transition: when the
    /// template is applied and when <see cref="TransitionEffect" /> or <see cref="PaginationPosition" /> changes.
    /// </summary>
    public Easing PageInEasing
    {
        get => GetValue(PageInEasingProperty);
        set => SetValue(PageInEasingProperty, value);
    }

    /// <summary>
    /// Gets or sets the easing of the outgoing page in the page transition. The theme sets the value to
    /// <c>CubicEaseOut</c>; clearing a local value returns to it.
    /// The carousel writes the value into the transition when it builds or reconfigures the transition: when the
    /// template is applied and when <see cref="TransitionEffect" /> or <see cref="PaginationPosition" /> changes.
    /// </summary>
    public Easing PageOutEasing
    {
        get => GetValue(PageOutEasingProperty);
        set => SetValue(PageOutEasingProperty, value);
    }

    /// <summary>
    /// Gets or sets which page transition the carousel builds when <see cref="PageTransition" /> is
    /// <see langword="null" />. The default is <see cref="CarouselTransitionEffect.Scroll" />.
    /// A change builds a new transition and replaces the current one, also one that was assigned to
    /// <see cref="PageTransition" />.
    /// </summary>
    public CarouselTransitionEffect TransitionEffect
    {
        get => GetValue(TransitionEffectProperty);
        set => SetValue(TransitionEffectProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether motion runs on the carousel. The value is shared with
    /// <see cref="AbstractCarousel.IsMotionEnabled" />.
    /// The theme sets it from the <c>EnableMotion</c> token of the carousel tokens and forwards it to the pagination
    /// indicator. A value set on the carousel overrides the token; clearing it returns to the token.
    /// While <see langword="false" />, autoplay does not run even if <see cref="IsAutoPlay" /> is
    /// <see langword="true" />.
    /// </summary>
    public new bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a drag with the pointer changes the page. The value is shared with
    /// <see cref="AbstractCarousel.IsSwipeEnabled" />. The default is <see langword="false" />.
    /// While <see langword="true" />, a drag that starts outside the previous/next buttons and the pagination
    /// indicator and covers at least 30 device-independent pixels along the page axis, more than along the other
    /// axis, moves one page: dragging left, or up when the axis is vertical, goes to the next page, and the other
    /// way goes to the previous page. The axis follows <see cref="PaginationPosition" />. Autoplay does not run while
    /// the carousel tracks the drag, and the carousel shows the drag-move cursor.
    /// </summary>
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
    
    internal static readonly StyledProperty<bool> UseNavigationOffsetTokenResourcesProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(UseNavigationOffsetTokenResources));
    
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
    
    /// <summary>
    /// Gets or sets the transition that the default items panel plays when the current page changes, or
    /// <see langword="null" /> to use the transition built from <see cref="TransitionEffect" />.
    /// The carousel acts on <see langword="null" /> when the template is applied. Duration, easings and the slide
    /// axis are applied as described for <see cref="PageTransitionDuration" />.
    /// A transition assigned here stays in place until <see cref="TransitionEffect" /> changes.
    /// </summary>
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
        set => SetAndRaise(EffectivePreviousButtonMarginProperty, ref _effectivePreviousButtonMargin, value);
    }
    
    private Thickness _effectiveNextButtonMargin;

    internal Thickness EffectiveNextButtonMargin
    {
        get => _effectiveNextButtonMargin;
        set => SetAndRaise(EffectiveNextButtonMarginProperty, ref _effectiveNextButtonMargin, value);
    }
    
    private bool _isEffectiveShowTransitionProgress;

    internal bool IsEffectiveShowTransitionProgress
    {
        get => _isEffectiveShowTransitionProgress;
        set => SetAndRaise(IsEffectiveShowTransitionProgressProperty, ref _isEffectiveShowTransitionProgress, value);
    }
    
    internal bool UseNavigationOffsetTokenResources
    {
        get => GetValue(UseNavigationOffsetTokenResourcesProperty);
        set => SetValue(UseNavigationOffsetTokenResourcesProperty, value);
    }

    private static readonly object s_paginationOffsetKey = CarouselTokenKind.PaginationOffset;
    private static readonly object s_arrowOffsetKey = CarouselTokenKind.ArrowOffset;
    private OwnerTokenSet? _offsetTokens;
    private double _paginationOffset;
    private double _arrowOffset;
    
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
    
    /// <summary>
    /// Moves to the next page and reports a programmatic change as the origin, in the same way as
    /// <see cref="AbstractCarousel.Next" />.
    /// The carousel wraps to the first page while <see cref="IsInfinite" /> is <see langword="true" />, and
    /// otherwise stays on the last page. Nothing happens while there are no items.
    /// </summary>
    public new void Next() => base.Next();

    /// <summary>
    /// Moves to the previous page and reports a programmatic change as the origin, in the same way as
    /// <see cref="AbstractCarousel.Previous" />.
    /// The carousel wraps to the last page while <see cref="IsInfinite" /> is <see langword="true" />, and otherwise
    /// stays on the first page. Nothing happens while there are no items.
    /// </summary>
    public new void Previous() => base.Previous();

    /// <summary>
    /// Arranges the control and then sets the scroll offset of the template's scroll viewer to the selected page.
    /// Calls the base member first and returns the size it reports.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);

        if (_scroller is not null)
        {
            _scroller.Offset = new(SelectedIndex, 0);
        }

        return result;
    }
    
    /// <summary>
    /// Cancels a swipe in progress, releases the parts of the previous template, calls the base member and connects
    /// the parts of the new one: the pagination indicator gets this carousel as its owner, the previous/next buttons
    /// move one page per click, a page transition is built when none is assigned and the buttons are configured for
    /// the current page.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then starts tracking a swipe when <see cref="IsSwipeEnabled" /> is
    /// <see langword="true" />: the pointer is captured, the press position is stored and autoplay is paused.
    /// Presses on the previous/next buttons or the pagination indicator, presses of mouse buttons other than the
    /// left one and presses that arrive while another pointer is tracked are ignored.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then ends the swipe tracked for the releasing pointer: autoplay is no longer held
    /// back for the swipe and, when the drag covers the swipe distance, the page changes and the event is marked
    /// handled. Releases of other pointers are ignored.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then cancels the swipe when the capture was lost by the tracked pointer, so that
    /// no page change follows and autoplay is no longer held back for the swipe.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then updates the state that depends on the changed property.
    /// The selected page moves the scroll offset. The selected page, the item count, <see cref="IsInfinite" /> and
    /// <see cref="IsShowNavButtons" /> configure the previous/next buttons. <see cref="PaginationPosition" />
    /// updates the margins of the indicator and the buttons, the slide axis and the swipe cursor.
    /// <see cref="TransitionEffect" /> builds a new page transition. <see cref="IsSwipeEnabled" /> updates the swipe
    /// cursor and cancels a swipe when it becomes <see langword="false" />. <see cref="IsShowTransitionProgress" />
    /// updates whether the autoplay progress shows, and <c>UseNavigationOffsetTokenResources</c> updates the offset
    /// token subscription while the control is attached.
    /// </summary>
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
        else if (change.Property == PaginationPositionProperty)
        {
            ConfigurePaginationMargin();
            ConfigureNavButtonsMargin();
        }
        else if (change.Property == UseNavigationOffsetTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateNavigationOffsetTokenSubscription();
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

    /// <summary>
    /// Updates whether the pagination indicator shows the autoplay progress, which it does only while the autoplay
    /// timer runs. The base class calls this method when the autoplay timer starts or stops.
    /// </summary>
    /// <param name="isRunning">
    /// Whether the autoplay timer runs after the change. This override ignores the argument and reads the timer
    /// state instead.
    /// </param>
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

    /// <summary>
    /// Registers the host window before the base member is called, so that autoplay can take a minimized window
    /// into account, and after that call subscribes to the offset tokens of the pagination indicator and the
    /// buttons when <c>UseNavigationOffsetTokenResources</c> is <see langword="true" />.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _window = TopLevel.GetTopLevel(this) as AvaloniaWindow;
        if (_window is not null)
        {
            _window.PropertyChanged += HandleHostWindowPropertyChanged;
        }
        UpdateHostWindowAvailability();
        base.OnAttachedToVisualTree(e);
        UpdateNavigationOffsetTokenSubscription();
    }

    /// <summary>
    /// Disposes the offset token subscription and the host window subscription before the base member is called,
    /// and after that call cancels a swipe in progress.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _offsetTokens?.Dispose();
        _offsetTokens = null;
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

    private void UpdateNavigationOffsetTokenSubscription()
    {
        if (UseNavigationOffsetTokenResources)
        {
            if (_offsetTokens is null)
            {
                _offsetTokens = new OwnerTokenSet(this, 2, RefreshOffsets);
                _offsetTokens.SetResourceKey(0, s_paginationOffsetKey);
                _offsetTokens.SetResourceKey(1, s_arrowOffsetKey);
            }
        }
        else
        {
            _offsetTokens?.Dispose();
            _offsetTokens = null;
        }

        RefreshOffsets();
    }

    private void RefreshOffsets()
    {
        var pagination = ReadOffset(_offsetTokens?.GetValue(0), _paginationOffset);
        var arrow = ReadOffset(_offsetTokens?.GetValue(1), _arrowOffset);
        var paginationChanged = !_paginationOffset.Equals(pagination);
        var arrowChanged = !_arrowOffset.Equals(arrow);
        _paginationOffset = pagination;
        _arrowOffset = arrow;

        if (paginationChanged)
        {
            ConfigurePaginationMargin();
        }
        if (arrowChanged)
        {
            ConfigureNavButtonsMargin();
        }
    }

    private static double ReadOffset(object? value, double previous)
    {
        if (value is double offset)
        {
            return offset;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private void ConfigurePaginationMargin()
    {
        if (PaginationPosition == CarouselPaginationPosition.Bottom)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(0, 0, 0, _paginationOffset));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Top)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(0, _paginationOffset, 0, 0));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Left)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(_paginationOffset, 0, 0, 0));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Right)
        {
            SetCurrentValue(EffectivePaginationMarginProperty, new Thickness(0, 0, _paginationOffset, 0));
        }
    }

    private void ConfigureNavButtonsMargin()
    {
        if (PaginationPosition == CarouselPaginationPosition.Bottom ||
            PaginationPosition == CarouselPaginationPosition.Top)
        {
            SetCurrentValue(EffectivePreviousButtonMarginProperty, new Thickness(_arrowOffset, 0, 0, 0));
            SetCurrentValue(EffectiveNextButtonMarginProperty, new Thickness(0, 0, _arrowOffset, 0));
        }
        else if (PaginationPosition == CarouselPaginationPosition.Left ||
                 PaginationPosition == CarouselPaginationPosition.Right)
        {
            SetCurrentValue(EffectivePreviousButtonMarginProperty, new Thickness(0, _arrowOffset, 0, 0));
            SetCurrentValue(EffectiveNextButtonMarginProperty, new Thickness(0, 0, 0, _arrowOffset));
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
