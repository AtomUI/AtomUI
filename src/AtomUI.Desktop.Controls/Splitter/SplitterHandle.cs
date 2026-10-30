using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[TemplatePart("PART_HandleLine", typeof(Border))]
[TemplatePart("PART_DragBar", typeof(SplitterDragBar))]
[TemplatePart("PART_CollapsePrevButton", typeof(IconButton))]
[TemplatePart("PART_CollapseNextButton", typeof(IconButton))]
internal class SplitterHandle : TemplatedControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SplitterHandle, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<IBrush?> LineBrushProperty =
        AvaloniaProperty.Register<SplitterHandle, IBrush?>(nameof(LineBrush));

    public static readonly StyledProperty<double> LineThicknessProperty =
        Splitter.LineThicknessProperty.AddOwner<SplitterHandle>();

    public static readonly StyledProperty<CornerRadius> LineCornerRadiusProperty =
        Splitter.LineCornerRadiusProperty.AddOwner<SplitterHandle>();

    public static readonly StyledProperty<bool> IsDragEnabledProperty =
        AvaloniaProperty.Register<SplitterHandle, bool>(nameof(IsDragEnabled), true);

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public IBrush? LineBrush
    {
        get => GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    public double LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public CornerRadius LineCornerRadius
    {
        get => GetValue(LineCornerRadiusProperty);
        set => SetValue(LineCornerRadiusProperty, value);
    }

    public bool IsDragEnabled
    {
        get => GetValue(IsDragEnabledProperty);
        set => SetValue(IsDragEnabledProperty, value);
    }

    public int HandleIndex { get; set; }

    internal bool IsPreviousCollapsible { get; set; }
    internal bool IsNextCollapsible { get; set; }
    internal bool IsPreviousCollapsed { get; set; }
    internal bool IsNextCollapsed { get; set; }
    internal bool ShowPreviousButton { get; set; } = true;
    internal bool ShowNextButton { get; set; } = true;
    internal bool ShowBothButtonsOnHover { get; set; }
    internal bool PreviousButtonControlsNext { get; set; }
    internal bool NextButtonControlsPrevious { get; set; }
    internal IIconTemplate? PreviousIconTemplate { get; set; }
    internal IIconTemplate? NextIconTemplate { get; set; }

    internal SplitterCollapsibleIconDisplayMode PreviousShowMode { get; set; } =
        SplitterCollapsibleIconDisplayMode.Hover;

    internal SplitterCollapsibleIconDisplayMode NextShowMode { get; set; } =
        SplitterCollapsibleIconDisplayMode.Hover;

    internal bool IsPointerInDragZone => _dragBar?.IsPointerOver == true;

    public event EventHandler<VectorEventArgs>? DragStarted;
    public event EventHandler<VectorEventArgs>? DragDelta;
    public event EventHandler<VectorEventArgs>? DragCompleted;
    public event EventHandler? DraggerDoubleClicked;
    public event EventHandler? CollapsePreviousRequested;
    public event EventHandler? CollapseNextRequested;

    private SplitterDragBar? _dragBar;
    private IconButton? _collapsePrevButton;
    private IconButton? _collapseNextButton;
    private HoverSide _hoverSide = HoverSide.None;

    private enum HoverSide
    {
        None,
        Previous,
        Next
    }

    static SplitterHandle()
    {
        OrientationProperty.Changed.AddClassHandler<SplitterHandle>((x, _) => x.OnOrientationChanged());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachDragBar();
        DetachCollapseButton(ref _collapsePrevButton, HandleCollapsePrevClick);
        DetachCollapseButton(ref _collapseNextButton, HandleCollapseNextClick);

        _dragBar            = e.NameScope.Find<SplitterDragBar>("PART_DragBar");
        _collapsePrevButton = e.NameScope.Find<IconButton>("PART_CollapsePrevButton");
        _collapseNextButton = e.NameScope.Find<IconButton>("PART_CollapseNextButton");

        AttachDragBar();
        AttachCollapseButton(_collapsePrevButton, HandleCollapsePrevClick);
        AttachCollapseButton(_collapseNextButton, HandleCollapseNextClick);

        UpdateCollapseButtons();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateHoverSide(e);
        UpdateCollapseButtons();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        UpdateHoverSide(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _hoverSide = HoverSide.None;
        UpdateCollapseButtons();
    }

    internal void SetDragging(bool isDragging)
    {
        PseudoClasses.Set(StdPseudoClass.Dragging, isDragging);
        _dragBar?.SetDragging(isDragging);
    }

    internal void AdjustDrag(Vector vector)
    {
        _dragBar?.AdjustDrag(vector);
    }

    internal void UpdateCollapseButtons()
    {
        var hasPrevious  = ShowPreviousButton && IsPreviousCollapsible;
        var hasNext      = ShowNextButton && IsNextCollapsible;
        var onlyPrevious = hasPrevious && !hasNext;
        var onlyNext     = hasNext && !hasPrevious;

        if (_collapsePrevButton != null)
        {
            UpdateCollapseButton(
                _collapsePrevButton,
                hasPrevious && ShouldShowIcon(PreviousShowMode, true, onlyPrevious),
                PreviousIconTemplate != null,
                isPrevious: true);
        }

        if (_collapseNextButton != null)
        {
            UpdateCollapseButton(
                _collapseNextButton,
                hasNext && ShouldShowIcon(NextShowMode, false, onlyNext),
                NextIconTemplate != null,
                isPrevious: false);
        }
    }

    private void OnOrientationChanged()
    {
        UpdateCollapseButtons();
    }

    private void AttachDragBar()
    {
        if (_dragBar == null)
        {
            return;
        }

        _dragBar.DragStarted   += HandleDragBarStarted;
        _dragBar.DragDelta     += HandleDragBarDelta;
        _dragBar.DragCompleted += HandleDragBarCompleted;
        _dragBar.DoubleClicked += HandleDragBarDoubleClicked;
    }

    private void DetachDragBar()
    {
        if (_dragBar == null)
        {
            return;
        }

        _dragBar.DragStarted   -= HandleDragBarStarted;
        _dragBar.DragDelta     -= HandleDragBarDelta;
        _dragBar.DragCompleted -= HandleDragBarCompleted;
        _dragBar.DoubleClicked -= HandleDragBarDoubleClicked;
    }

    private void HandleDragBarStarted(object? sender, VectorEventArgs e)
    {
        DragStarted?.Invoke(this, e);
    }

    private void HandleDragBarDelta(object? sender, VectorEventArgs e)
    {
        DragDelta?.Invoke(this, e);
    }

    private void HandleDragBarCompleted(object? sender, VectorEventArgs e)
    {
        DragCompleted?.Invoke(this, e);
    }

    private void HandleDragBarDoubleClicked(object? sender, EventArgs e)
    {
        DraggerDoubleClicked?.Invoke(this, EventArgs.Empty);
    }

    private static void AttachCollapseButton(IconButton? button, EventHandler<RoutedEventArgs> handler)
    {
        if (button != null)
        {
            button.Click += handler;
        }
    }

    private static void DetachCollapseButton(ref IconButton? button, EventHandler<RoutedEventArgs> handler)
    {
        if (button != null)
        {
            button.Click -= handler;
        }

        button = null;
    }

    private void HandleCollapsePrevClick(object? sender, RoutedEventArgs e)
    {
        var target = PreviousButtonControlsNext ? CollapseNextRequested : CollapsePreviousRequested;
        target?.Invoke(this, EventArgs.Empty);
    }

    private void HandleCollapseNextClick(object? sender, RoutedEventArgs e)
    {
        var target = NextButtonControlsPrevious ? CollapsePreviousRequested : CollapseNextRequested;
        target?.Invoke(this, EventArgs.Empty);
    }

    private static void ApplyCustomIconBackground(IconButton button, bool isCustom)
    {
        if (isCustom)
        {
            button.SetCurrentValue(BackgroundProperty, Brushes.Transparent);
        }
        else
        {
            button.ClearValue(BackgroundProperty);
        }
    }

    private void UpdateCollapseButton(IconButton button,
                                      bool isVisible,
                                      bool isCustomIcon,
                                      bool isPrevious)
    {
        ApplyCustomIconBackground(button, isCustomIcon);
        button.IsVisible = isVisible;
        if (isVisible)
        {
            button.Icon = isPrevious ? CreatePrevIcon() : CreateNextIcon();
        }
        else if (button.Icon != null)
        {
            button.Icon = null;
        }
    }

    private bool ShouldShowIcon(SplitterCollapsibleIconDisplayMode mode, bool isPrevious, bool isOnly)
    {
        return mode switch
        {
            SplitterCollapsibleIconDisplayMode.Always => true,
            SplitterCollapsibleIconDisplayMode.Hidden => false,
            SplitterCollapsibleIconDisplayMode.Hover => IsPointerOver &&
                                                        (ShowBothButtonsOnHover ||
                                                         isOnly ||
                                                         _hoverSide == (isPrevious ? HoverSide.Previous : HoverSide.Next)),
            _ => false
        };
    }

    private void UpdateHoverSide(PointerEventArgs e)
    {
        if (!IsPointerOver)
        {
            return;
        }

        var position = e.GetPosition(this);
        var isVertical = Orientation == Orientation.Vertical;
        var compare    = isVertical ? position.X : position.Y;
        var center     = isVertical ? Bounds.Width * 0.5 : Bounds.Height * 0.5;
        var newSide    = compare < center ? HoverSide.Previous : HoverSide.Next;

        if (_hoverSide == newSide)
        {
            return;
        }

        _hoverSide = newSide;
        if (PreviousShowMode == SplitterCollapsibleIconDisplayMode.Hover ||
            NextShowMode == SplitterCollapsibleIconDisplayMode.Hover)
        {
            UpdateCollapseButtons();
        }
    }

    private PathIcon? CreatePrevIcon()
    {
        if (PreviousIconTemplate != null)
        {
            return PreviousIconTemplate.Build();
        }

        return Orientation switch
        {
            Orientation.Vertical   => new LeftOutlined(),
            Orientation.Horizontal => new UpOutlined(),
            _                      => new LeftOutlined()
        };
    }

    private PathIcon? CreateNextIcon()
    {
        if (NextIconTemplate != null)
        {
            return NextIconTemplate.Build();
        }

        return Orientation switch
        {
            Orientation.Vertical   => new RightOutlined(),
            Orientation.Horizontal => new DownOutlined(),
            _                      => new RightOutlined()
        };
    }
}
