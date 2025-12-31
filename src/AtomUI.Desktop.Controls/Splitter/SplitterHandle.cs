using System;
using AtomUI;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUIThumb = AtomUI.Desktop.Controls.Primitives.Thumb;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[TemplatePart(SplitterThemeConstants.HandleLinePart, typeof(Border))]
[TemplatePart(SplitterThemeConstants.HandleGripPart, typeof(Border))]
[TemplatePart(SplitterThemeConstants.CollapsePrevButtonPart, typeof(IconButton))]
[TemplatePart(SplitterThemeConstants.CollapseNextButtonPart, typeof(IconButton))]
internal class SplitterHandle : AtomUIThumb
{
    public static readonly StyledProperty<SplitterLayout> LayoutProperty =
        AvaloniaProperty.Register<SplitterHandle, SplitterLayout>(nameof(Layout), SplitterLayout.Vertical);

    public static readonly StyledProperty<IBrush?> LineBrushProperty =
        AvaloniaProperty.Register<SplitterHandle, IBrush?>(nameof(LineBrush));

    public static readonly StyledProperty<double> LineThicknessProperty =
        AvaloniaProperty.Register<SplitterHandle, double>(nameof(LineThickness));

    public static readonly StyledProperty<bool> IsDragEnabledProperty =
        AvaloniaProperty.Register<SplitterHandle, bool>(nameof(IsDragEnabled), true);

    public SplitterLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
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

    public bool IsDragEnabled
    {
        get => GetValue(IsDragEnabledProperty);
        set => SetValue(IsDragEnabledProperty, value);
    }


    public int HandleIndex { get; set; }

    private IconButton? _collapsePrevButton;
    private IconButton? _collapseNextButton;
    private bool _isPointerOver;
    private HoverSide _hoverSide = HoverSide.None;
    private Point _lastPointerPosition;
    private bool _hasPointerPosition;
    private bool _isDragging;

    private enum HoverSide
    {
        None,
        Previous,
        Next
    }

    internal bool IsPreviousCollapsible { get; set; }
    internal bool IsNextCollapsible { get; set; }
    internal bool IsPreviousCollapsed { get; set; }
    internal bool IsNextCollapsed { get; set; }
    internal bool ShowPreviousButton { get; set; } = true;
    internal bool ShowNextButton { get; set; } = true;
    internal bool ShowBothButtonsOnHover { get; set; }
    internal bool PreviousButtonControlsNext { get; set; }
    internal bool NextButtonControlsPrevious { get; set; }
    internal SplitterCollapsibleIconDisplayMode PreviousShowMode { get; set; } =
        SplitterCollapsibleIconDisplayMode.Hover;
    internal SplitterCollapsibleIconDisplayMode NextShowMode { get; set; } =
        SplitterCollapsibleIconDisplayMode.Hover;

    public event EventHandler? CollapsePreviousRequested;
    public event EventHandler? CollapseNextRequested;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_collapsePrevButton != null)
        {
            _collapsePrevButton.Click -= HandleCollapsePrevClick;
        }
        if (_collapseNextButton != null)
        {
            _collapseNextButton.Click -= HandleCollapseNextClick;
        }

        _collapsePrevButton = e.NameScope.Find<IconButton>(SplitterThemeConstants.CollapsePrevButtonPart);
        _collapseNextButton = e.NameScope.Find<IconButton>(SplitterThemeConstants.CollapseNextButtonPart);

        if (_collapsePrevButton != null)
        {
            _collapsePrevButton.Click += HandleCollapsePrevClick;
        }
        if (_collapseNextButton != null)
        {
            _collapseNextButton.Click += HandleCollapseNextClick;
        }

        UpdateCollapseButtons();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _isPointerOver = true;
        CachePointerPosition(e);
        UpdateHoverSide(e);
        UpdateCursor(_lastPointerPosition);
        UpdateCollapseButtons();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _isPointerOver = false;
        _hoverSide = HoverSide.None;
        _hasPointerPosition = false;
        if (!_isDragging)
        {
            Cursor = Cursor.Default;
        }
        UpdateCollapseButtons();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsDragEnabled)
        {
            return;
        }
        CachePointerPosition(e);
        UpdateCursor(_lastPointerPosition);
        if (!IsPointerInDragArea(e))
        {
            return;
        }
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        CachePointerPosition(e);
        UpdateHoverSide(e);
        UpdateCursor(_lastPointerPosition);
        if (IsDragEnabled)
        {
            base.OnPointerMoved(e);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!IsDragEnabled)
        {
            return;
        }
        CachePointerPosition(e);
        UpdateCursor(_lastPointerPosition);
        base.OnPointerReleased(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (!IsDragEnabled)
        {
            return;
        }
        _hasPointerPosition = false;
        Cursor = Cursor.Default;
        base.OnPointerCaptureLost(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDragEnabledProperty || change.Property == LayoutProperty)
        {
            if (_isPointerOver && _hasPointerPosition)
            {
                UpdateCursor(_lastPointerPosition);
            }
            else if (!IsDragEnabled)
            {
                Cursor = Cursor.Default;
            }
        }
    }

    private void HandleCollapsePrevClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (PreviousButtonControlsNext)
        {
            CollapseNextRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        CollapsePreviousRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleCollapseNextClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (NextButtonControlsPrevious)
        {
            CollapsePreviousRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        CollapseNextRequested?.Invoke(this, EventArgs.Empty);
    }

    internal void SetDragging(bool isDragging)
    {
        _isDragging = isDragging;
        PseudoClasses.Set(StdPseudoClass.Dragging, isDragging);
        Cursor = isDragging ? GetDragCursor() : Cursor.Default;
    }

    internal void UpdateCollapseButtons()
    {
        var hasPrevious = ShowPreviousButton && IsPreviousCollapsible;
        var hasNext = ShowNextButton && IsNextCollapsible;
        var onlyPrevious = hasPrevious && !hasNext;
        var onlyNext = hasNext && !hasPrevious;

        if (_collapsePrevButton != null)
        {
            _collapsePrevButton.IsVisible = hasPrevious &&
                                            ShouldShowIcon(PreviousShowMode, true, onlyPrevious);
            _collapsePrevButton.Icon = CreatePrevIcon();
        }

        if (_collapseNextButton != null)
        {
            _collapseNextButton.IsVisible = hasNext &&
                                            ShouldShowIcon(NextShowMode, false, onlyNext);
            _collapseNextButton.Icon = CreateNextIcon();
        }
    }

    private bool ShouldShowIcon(SplitterCollapsibleIconDisplayMode mode, bool isPrevious, bool isOnly)
    {
        return mode switch
        {
            SplitterCollapsibleIconDisplayMode.Always => true,
            SplitterCollapsibleIconDisplayMode.Hidden => false,
            SplitterCollapsibleIconDisplayMode.Hover => _isPointerOver &&
                                                        (ShowBothButtonsOnHover ||
                                                         isOnly ||
                                                         _hoverSide == (isPrevious ? HoverSide.Previous : HoverSide.Next)),
            _ => false
        };
    }

    private void UpdateHoverSide(PointerEventArgs e)
    {
        if (!_isPointerOver)
        {
            return;
        }

        var position = e.GetPosition(this);
        var compare = Layout == SplitterLayout.Vertical ? position.X : position.Y;
        var center = Layout == SplitterLayout.Vertical ? Bounds.Width * 0.5 : Bounds.Height * 0.5;
        var newSide = compare < center ? HoverSide.Previous : HoverSide.Next;

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

    private void CachePointerPosition(PointerEventArgs e)
    {
        _lastPointerPosition = e.GetPosition(this);
        _hasPointerPosition = true;
    }

    private void UpdateCursor(Point position)
    {
        if (_isDragging)
        {
            Cursor = GetDragCursor();
            return;
        }

        if (!IsDragEnabled)
        {
            Cursor = Cursor.Default;
            return;
        }

        Cursor = IsPointerInDragArea(position) ? GetDragCursor() : Cursor.Default;
    }

    private Cursor GetDragCursor()
    {
        return Layout == SplitterLayout.Vertical
            ? new Cursor(StandardCursorType.SizeWestEast)
            : new Cursor(StandardCursorType.SizeNorthSouth);
    }

    private PathIcon? CreatePrevIcon()
    {
        return Layout switch
        {
            SplitterLayout.Vertical => new LeftOutlined(),
            SplitterLayout.Horizontal => new UpOutlined(),
            _ => new LeftOutlined()
        };
    }

    private PathIcon? CreateNextIcon()
    {
        return Layout switch
        {
            SplitterLayout.Vertical => new RightOutlined(),
            SplitterLayout.Horizontal => new DownOutlined(),
            _ => new RightOutlined()
        };
    }

    private bool IsPointerInDragArea(PointerEventArgs e)
    {
        return IsPointerInDragArea(e.GetPosition(this));
    }

    private bool IsPointerInDragArea(Point position)
    {
        var triggerSize = GetDragTriggerSize();
        if (triggerSize <= 0)
        {
            return true;
        }

        var half = triggerSize * 0.5;
        if (Layout == SplitterLayout.Vertical)
        {
            return Math.Abs(position.X - Bounds.Width * 0.5) <= half;
        }

        return Math.Abs(position.Y - Bounds.Height * 0.5) <= half;
    }

    private double GetDragTriggerSize()
    {
        var value = TokenResourceUtils.FindTokenResource(this, SplitterTokenKey.SplitTriggerSize);
        if (value is double size && !double.IsNaN(size) && !double.IsInfinity(size))
        {
            return Math.Max(0, size);
        }

        value = TokenResourceUtils.FindTokenResource(this, SplitterTokenKey.SplitBarSize);
        if (value is double fallback && !double.IsNaN(fallback) && !double.IsInfinity(fallback))
        {
            return Math.Max(0, fallback);
        }

        if (!double.IsNaN(LineThickness) && !double.IsInfinity(LineThickness))
        {
            return Math.Max(0, LineThickness);
        }

        return 0;
    }
}
