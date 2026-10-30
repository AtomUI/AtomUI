using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal enum DateTimePickerPanelType
{
    Year,
    Month,
    Day,
    Hour,
    Minute,
    Second,
    TimePeriod //AM or PM
}

internal struct CellHoverInfo
{
    public DateTimePickerPanelType PanelType { get; }
    public int CellValue { get; }

    public CellHoverInfo(DateTimePickerPanelType panelType, int cellValue)
    {
        PanelType = panelType;
        CellValue = cellValue;
    }
}

internal class CellHoverEventArgs : EventArgs
{
    public CellHoverInfo? CellHoverInfo { get; }

    public CellHoverEventArgs(CellHoverInfo? hoverInfo)
    {
        CellHoverInfo = hoverInfo;
    }
}

internal class CellDbClickedEventArgs : EventArgs
{
    public bool IsSelected { get; }

    public CellDbClickedEventArgs(bool isSelected)
    {
        IsSelected = isSelected;
    }
}

internal class DateTimePickerPanel : Panel
{
    #region 公共属性定义

    public static readonly StyledProperty<double> ItemHeightProperty = AvaloniaProperty.Register<DateTimePickerPanel, double>(nameof(ItemHeight), 28);
    public static readonly StyledProperty<DateTimePickerPanelType> PanelTypeProperty = AvaloniaProperty.Register<DateTimePickerPanel, DateTimePickerPanelType>(nameof(PanelType));
    public static readonly StyledProperty<string> ItemFormatProperty = AvaloniaProperty.Register<DateTimePickerPanel, string>(nameof(ItemFormat), "hh");
    public static readonly StyledProperty<bool> ShouldLoopProperty = AvaloniaProperty.Register<DateTimePickerPanel, bool>(nameof(ShouldLoop));

    public double ItemHeight { get => GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }
    public DateTimePickerPanelType PanelType { get => GetValue(PanelTypeProperty); set => SetValue(PanelTypeProperty, value); }
    public string ItemFormat { get => GetValue(ItemFormatProperty); set => SetValue(ItemFormatProperty, value); }
    public bool ShouldLoop { get => GetValue(ShouldLoopProperty); set => SetValue(ShouldLoopProperty, value); }
    public DateTime FormatDate { get; set; } = DateTime.Today;
    public int MinimumValue
    {
        get => _minimumValue;
        set
        {
            if (_minimumValue != value)
            {
                _minimumValue = value;
                RefreshItems();
            }
        }
    }
    public int MaximumValue
    {
        get => _maximumValue;
        set
        {
            if (_maximumValue != value)
            {
                _maximumValue = value;
                RefreshItems();
            }
        }
    }
    public int Increment
    {
        get => _increment;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            if (_increment != value)
            {
                _increment = value;
                RefreshItems();
            }
        }
    }
    public int SelectedValue
    {
        get => _selectedValue;
        set
        {
            var selected = Normalize(value);
            var changed = _selectedValue != selected;
            _selectedValue = selected;
            RefreshSelection();
            _scrollTarget = (_selectedValue - MinimumValue) / Increment;
            ApplyScrollTarget();
            if (changed)
            {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public Vector Offset
    {
        get => _scroller?.Offset ?? default;
        set
        {
            if (_scroller != null)
            {
                _scroller.SetCurrentValue(ScrollViewer.OffsetProperty, value);
            }
        }
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsChangeOnScrollProperty = TimePicker.IsChangeOnScrollProperty.AddOwner<DateTimePickerPanel>();
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DateTimePickerPanel>();
    internal static readonly StyledProperty<string?> AmTextProperty = AvaloniaProperty.Register<DateTimePickerPanel, string?>(nameof(AmText));
    internal static readonly StyledProperty<string?> PmTextProperty = AvaloniaProperty.Register<DateTimePickerPanel, string?>(nameof(PmText));

    internal bool IsChangeOnScroll { get => GetValue(IsChangeOnScrollProperty); set => SetValue(IsChangeOnScrollProperty, value); }
    internal bool IsMotionEnabled { get => GetValue(IsMotionEnabledProperty); set => SetValue(IsMotionEnabledProperty, value); }
    internal string? AmText { get => GetValue(AmTextProperty); set => SetValue(AmTextProperty, value); }
    internal string? PmText { get => GetValue(PmTextProperty); set => SetValue(PmTextProperty, value); }

    #endregion

    #region 公共事件定义

    public event EventHandler? SelectionChanged;

    #endregion

    #region 内部事件定义

    internal event EventHandler<CellHoverEventArgs>? CellHovered;
    internal event EventHandler<CellDbClickedEventArgs>? CellDbClicked;

    #endregion

    private int _minimumValue;
    private int _maximumValue;
    private int _increment = 1;
    private int _selectedValue;
    private int? _scrollTarget;
    private ScrollViewer? _scroller;
    private int ValueCount => Math.Max(1, (MaximumValue - MinimumValue) / Increment + 1);
    private double LoopHeight => Children.Count * ItemHeight;

    static DateTimePickerPanel()
    {
        FocusableProperty.OverrideDefaultValue<DateTimePickerPanel>(true);
        BackgroundProperty.OverrideDefaultValue<DateTimePickerPanel>(Avalonia.Media.Brushes.Transparent);
    }

    public DateTimePickerPanel()
    {
        AddHandler(TappedEvent, HandleItemTapped);
        AddHandler(DoubleTappedEvent, HandleItemDoubleTapped);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemHeightProperty || change.Property == PanelTypeProperty ||
            change.Property == ItemFormatProperty || change.Property == AmTextProperty || change.Property == PmTextProperty ||
            change.Property == ShouldLoopProperty)
        {
            RefreshItems();
        }
    }

    private int Normalize(int value)
    {
        var max = Math.Max(MinimumValue, MaximumValue);
        var clamped = Math.Clamp(value, MinimumValue, max);
        return MinimumValue + (clamped - MinimumValue) / Increment * Increment;
    }

    public void RefreshItems()
    {
        var count = ValueCount;
        if (ShouldLoop)
        {
            // Short stepped sequences still need enough real rows to cover the
            // viewport while wrapping. Reuse these containers during scrolling.
            var visibleCount = (int)Math.Ceiling((_scroller?.Viewport.Height ?? 0) / ItemHeight) + 2;
            count *= Math.Max(1, (int)Math.Ceiling((double)visibleCount / count));
        }
        while (Children.Count < count)
        {
            var cell = new TimeViewCell { Content = new TextBlock { IsHitTestVisible = false } };
            // Runtime-generated rows are not template parts. A ControlTheme cannot
            // TemplateBind through this panel to their owner's motion property.
            cell[!TimeViewCell.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            cell.PointerEntered += HandleCellPointerEntered;
            cell.PointerExited += HandleCellPointerExited;
            Children.Add(cell);
        }
        while (Children.Count > count)
        {
            var cell = (TimeViewCell)Children[^1];
            cell.PointerEntered -= HandleCellPointerEntered;
            cell.PointerExited -= HandleCellPointerExited;
            cell.ClearValue(TimeViewCell.IsMotionEnabledProperty);
            Children.RemoveAt(Children.Count - 1);
        }
        _selectedValue = Normalize(_selectedValue);
        for (var i = 0; i < Children.Count; ++i)
        {
            var cell = (TimeViewCell)Children[i];
            var value = MinimumValue + i % ValueCount * Increment;
            cell.Tag = value;
            ((TextBlock)cell.Content!).Text = FormatContent(value);
        }
        RefreshSelection();
        InvalidateMeasure();
    }

    private string? FormatContent(int value)
    {
        return PanelType switch
        {
            DateTimePickerPanelType.TimePeriod => value == 0 ? AmText : PmText,
            DateTimePickerPanelType.Hour => new TimeSpan(value, 0, 0).ToString(ItemFormat),
            DateTimePickerPanelType.Minute => new TimeSpan(0, value, 0).ToString(ItemFormat),
            DateTimePickerPanelType.Second => new TimeSpan(0, 0, value).ToString(ItemFormat),
            _ => value.ToString()
        };
    }

    private void RefreshSelection()
    {
        var center = Offset.Y + (_scroller?.Viewport.Height ?? 0) / 2;
        var selected = Children.OfType<TimeViewCell>()
            .Where(cell => (int)cell.Tag! == SelectedValue)
            .MinBy(cell => Math.Abs(cell.Bounds.Center.Y - center));
        foreach (var cell in Children.OfType<TimeViewCell>())
        {
            cell.IsSelected = cell == selected;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
        {
            RefreshItems();
        }
        var width = double.IsInfinity(availableSize.Width) ? 56 : availableSize.Width;
        foreach (var cell in Children)
        {
            cell.Measure(new Size(width, ItemHeight));
        }
        var centeringSpace = Math.Max(0, (_scroller?.Viewport.Height ?? 0) - ItemHeight);
        // Looping columns arrange real wrapped values above/below the center.
        // Three scroll cycles provide room to browse in either direction.
        return new Size(width, LoopHeight * (ShouldLoop ? 3 : 1) + centeringSpace);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var leadingSpace = Math.Max(0, (_scroller?.Viewport.Height ?? 0) - ItemHeight) / 2;
        for (var i = 0; i < Children.Count; ++i)
        {
            var y = i * ItemHeight;
            if (ShouldLoop)
            {
                y += Math.Round((Offset.Y - y) / LoopHeight) * LoopHeight;
            }
            Children[i].Arrange(new Rect(0, leadingSpace + y, finalSize.Width, ItemHeight));
        }
        RefreshSelection();
        ApplyScrollTarget();
        return finalSize;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _scroller = this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
        if (_scroller != null)
        {
            _scroller.ScrollChanged += HandleScrollChanged;
        }
        _scrollTarget = (SelectedValue - MinimumValue) / Increment;
        ApplyScrollTarget();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_scroller != null)
        {
            _scroller.ScrollChanged -= HandleScrollChanged;
        }
        _scroller = null;
        _scrollTarget = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void ApplyScrollTarget()
    {
        if (_scrollTarget is { } index && _scroller is { } scroller && scroller.Viewport.Height > 0)
        {
            var target = (ShouldLoop ? LoopHeight : 0) + index * ItemHeight;
            if (scroller.Extent.Height - scroller.Viewport.Height >= target)
            {
                _scrollTarget = null;
                scroller.SetCurrentValue(ScrollViewer.OffsetProperty, new Vector(0, target));
            }
        }
    }

    private void HandleScrollChanged(object? sender, ScrollChangedEventArgs args)
    {
        if (args.ViewportDelta.Y != 0)
        {
            RefreshItems();
        }
        if (args.ViewportDelta.Y != 0 || args.ExtentDelta.Y != 0)
        {
            ApplyScrollTarget();
            return;
        }
        if (ShouldLoop && args.OffsetDelta.Y != 0)
        {
            // Rebase by complete cycles, preserving every visible row and the
            // fractional wheel offset without altering the selected value.
            if (Offset.Y < LoopHeight / 2 || Offset.Y > _scroller!.Extent.Height - _scroller.Viewport.Height - LoopHeight / 2)
            {
                Offset = new Vector(0, LoopHeight + Offset.Y % LoopHeight);
            }
            InvalidateArrange();
        }
        if (IsChangeOnScroll && args.OffsetDelta.Y != 0)
        {
            var index = (int)Math.Round(Offset.Y / ItemHeight);
            var value = MinimumValue + (ShouldLoop ? index % ValueCount : Math.Clamp(index, 0, ValueCount - 1)) * Increment;
            if (value != SelectedValue)
            {
                SelectedValue = value;
            }
        }
    }

    public void ScrollUp(int numItems = 1) => Offset = new Vector(0, Math.Max(0, Offset.Y - numItems * ItemHeight));
    public void ScrollDown(int numItems = 1) => Offset = new Vector(0, Offset.Y + numItems * ItemHeight);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var next = e.Key switch
        {
            Key.Up => SelectedValue - Increment,
            Key.Down => SelectedValue + Increment,
            Key.Home => MinimumValue,
            Key.End => MaximumValue,
            Key.PageUp => SelectedValue - Increment * 4,
            Key.PageDown => SelectedValue + Increment * 4,
            _ => (int?)null
        };
        if (next is { } value)
        {
            if (ShouldLoop && e.Key is Key.Up or Key.Down or Key.PageUp or Key.PageDown)
            {
                var index = (value - MinimumValue) / Increment;
                value = MinimumValue + (index % ValueCount + ValueCount) % ValueCount * Increment;
            }
            SelectedValue = value;
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    private TimeViewCell? CellFromSource(object? source)
    {
        return source is Visual visual ? visual.GetSelfAndVisualAncestors().OfType<TimeViewCell>().FirstOrDefault() : null;
    }

    private void HandleItemTapped(object? sender, TappedEventArgs args)
    {
        if (CellFromSource(args.Source) is { Tag: int value })
        {
            Focus();
            if (value == SelectedValue)
            {
                SelectedValue = value;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SelectedValue = value;
            }
            args.Handled = true;
        }
    }

    private void HandleItemDoubleTapped(object? sender, TappedEventArgs args)
    {
        if (CellFromSource(args.Source) is { } cell)
        {
            CellDbClicked?.Invoke(this, new CellDbClickedEventArgs(cell.IsSelected));
            args.Handled = true;
        }
    }

    private void HandleCellPointerEntered(object? sender, PointerEventArgs args)
    {
        if (sender is TimeViewCell { Tag: int value })
        {
            CellHovered?.Invoke(this, new CellHoverEventArgs(new CellHoverInfo(PanelType, value)));
        }
    }

    private void HandleCellPointerExited(object? sender, PointerEventArgs args) => CellHovered?.Invoke(this, new CellHoverEventArgs(null));
}
