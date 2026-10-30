using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class TimeSelectedEventArgs : EventArgs
{
    public TimeSpan? Time { get; }

    public TimeSelectedEventArgs(TimeSpan? value)
    {
        Time = value;
    }
}

[TemplatePart("PART_HourSelector", typeof(DateTimePickerPanel), IsRequired = true)]
[TemplatePart("PART_MinuteSelector", typeof(DateTimePickerPanel), IsRequired = true)]
[TemplatePart("PART_SecondSelector", typeof(DateTimePickerPanel), IsRequired = true)]
[TemplatePart("PART_PeriodHost", typeof(Panel), IsRequired = true)]
[TemplatePart("PART_PeriodSelector", typeof(DateTimePickerPanel), IsRequired = true)]
[TemplatePart("PART_PickerContainer", typeof(Grid), IsRequired = true)]
[TemplatePart("PART_ThirdSpacer", typeof(Rectangle), IsRequired = true)]
internal class TimeView : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<int> MinuteIncrementProperty =
        TimePicker.MinuteIncrementProperty.AddOwner<TimeView>();

    public static readonly StyledProperty<int> SecondIncrementProperty =
        TimePicker.SecondIncrementProperty.AddOwner<TimeView>();

    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        TimePicker.ClockIdentifierProperty.AddOwner<TimeView>();

    public static readonly StyledProperty<TimeSpan?> SelectedTimeProperty =
        AvaloniaProperty.Register<TimeView, TimeSpan?>(nameof(SelectedTime));

    public static readonly StyledProperty<bool> IsShowHeaderProperty =
        AvaloniaProperty.Register<TimeView, bool>(nameof(IsShowHeader), true);

    public static readonly StyledProperty<int> SelectorRowCountProperty =
        AvaloniaProperty.Register<TimeView, int>(nameof(SelectorRowCount), 7);

    /// <summary>
    /// Gets or sets the minute increment in the selector
    /// </summary>
    public int MinuteIncrement
    {
        get => GetValue(MinuteIncrementProperty);
        set => SetValue(MinuteIncrementProperty, value);
    }

    public int SecondIncrement
    {
        get => GetValue(SecondIncrementProperty);
        set => SetValue(SecondIncrementProperty, value);
    }

    /// <summary>
    /// Gets or sets the current clock identifier, either 12HourClock or 24HourClock
    /// </summary>
    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }

    public TimeSpan? SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public bool IsShowHeader
    {
        get => GetValue(IsShowHeaderProperty);
        set => SetValue(IsShowHeaderProperty, value);
    }

    public int SelectorRowCount
    {
        get => GetValue(SelectorRowCountProperty);
        set => SetValue(SelectorRowCountProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<TimeSelectedEventArgs>? TimeSelected;
    public event EventHandler<TimeSelectedEventArgs>? TempTimeSelected;
    public event EventHandler<TimeSelectedEventArgs>? HoverTimeChanged;

    #endregion
    
    #region 内部属性定义

    internal static readonly DirectProperty<TimeView, double> SpacerWidthProperty =
        AvaloniaProperty.RegisterDirect<TimeView, double>(nameof(SpacerWidth),
            o => o.SpacerWidth,
            (o, v) => o.SpacerWidth = v);

    internal static readonly DirectProperty<TimeView, double> ItemHeightProperty =
        AvaloniaProperty.RegisterDirect<TimeView, double>(nameof(ItemHeight),
            o => o.ItemHeight,
            (o, v) => o.ItemHeight = v);

    internal static readonly StyledProperty<bool> IsPointerInSelectorProperty =
        AvaloniaProperty.Register<TimeView, bool>(nameof(IsPointerInSelector), false);

    internal static readonly StyledProperty<bool> IsChangeOnScrollProperty =
        TimePicker.IsChangeOnScrollProperty.AddOwner<TimeView>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty
        = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TimeView>();
    
    internal static readonly DirectProperty<TimeView, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<TimeView, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);
    
    internal static readonly DirectProperty<TimeView, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<TimeView, string?>(nameof(PmText),
            o => o.PmText,
            (o, v) => o.PmText = v);
    
    private double _spacerWidth;

    internal double SpacerWidth
    {
        get => _spacerWidth;
        set => SetAndRaise(SpacerWidthProperty, ref _spacerWidth, value);
    }

    private double _itemHeight;

    internal double ItemHeight
    {
        get => _itemHeight;
        set => SetAndRaise(ItemHeightProperty, ref _itemHeight, value);
    }

    internal bool IsPointerInSelector
    {
        get => GetValue(IsPointerInSelectorProperty);
        set => SetValue(IsPointerInSelectorProperty, value);
    }

    internal bool IsChangeOnScroll
    {
        get => GetValue(IsChangeOnScrollProperty);
        set => SetValue(IsChangeOnScrollProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private string? _amText;

    internal string? AmText
    {
        get => _amText;
        set => SetAndRaise(AmTextProperty, ref _amText, value);
    }
    
    private string? _pmText;

    internal string? PmText
    {
        get => _pmText;
        set => SetAndRaise(PmTextProperty, ref _pmText, value);
    }


    #endregion
    
    // TemplateItems
    private Grid? _pickerSelectorContainer;
    private Rectangle? _spacer3;
    private Panel? _periodHost;
    private TextBlock? _headerText;
    private DateTimePickerPanel? _hourSelector;
    private DateTimePickerPanel? _minuteSelector;
    private DateTimePickerPanel? _secondSelector;
    private DateTimePickerPanel? _periodSelector;
    private IDisposable? _spacerWidthDisposable;
    private IDisposable? _pointerPositionDisposable;
    private TimeSpan? _pendingDisplayTime;
    private bool _isSyncingPanelValue;

    static TimeView()
    {
        KeyboardNavigation.TabNavigationProperty
                          .OverrideDefaultValue<TimeView>(KeyboardNavigationMode.Cycle);
    }

    public TimeView()
    {
        // Each column handles its own scrolling first. Unconsumed focus requests
        // must not continue through the popup's logical owner into the page.
        AddHandler(RequestBringIntoViewEvent, (_, e) =>
        {
            if (e.TargetObject != this)
            {
                e.Handled = true;
            }
        });
    }

    private void DetectPointerPosition(RawInputEventArgs args)
    {
        if (args is RawPointerEventArgs pointerEventArgs && this.IsAttachedToVisualTree() &&
            TopLevel.GetTopLevel(this) is { } topLevel &&
            topLevel.TryGetInputPosition(pointerEventArgs, out var position))
        {
            IsPointerInSelector = pointerEventArgs.Type is not (RawPointerEventType.LeaveWindow or RawPointerEventType.TouchCancel)
                                  && CheckPointerInSelectors(position);
        }
    }

    protected virtual bool CheckPointerInSelectors(Point position)
    {
        if (ClockIdentifier == ClockIdentifierType.HourClock12)
        {
            return CheckPointerInSelector(_hourSelector, position) ||
                   CheckPointerInSelector(_minuteSelector, position) ||
                   CheckPointerInSelector(_secondSelector, position) ||
                   CheckPointerInSelector(_periodSelector, position);
        }

        return CheckPointerInSelector(_hourSelector, position) ||
               CheckPointerInSelector(_minuteSelector, position) ||
               CheckPointerInSelector(_secondSelector, position);
    }

    private bool CheckPointerInSelector(DateTimePickerPanel? selector, Point position)
    {
        if (selector is null)
        {
            return false;
        }

        var globalRect = GetSelectorGlobalRect(selector);
        return globalRect.Width > 0 && globalRect.Height > 0 && globalRect.Contains(position);
    }

    private Rect GetSelectorGlobalRect(DateTimePickerPanel selector)
    {
        // The physical content can be taller than its viewport. Pointer geometry
        // belongs to the clipped scroll presenter, not the whole item extent.
        var viewport = selector.GetVisualAncestors().OfType<Avalonia.Controls.Presenters.ScrollContentPresenter>().FirstOrDefault();
        var target = (Control?)viewport ?? selector;
        return TopLevel.GetTopLevel(target) is { } topLevel && target.TranslatePoint(default, topLevel) is { } position
            ? new Rect(position, target.Bounds.Size) : default;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _spacerWidthDisposable = TokenResourceBinder.CreateControlTokenBinding(
            typeof(TimePicker),
            this,
            this,
            SpacerWidthProperty,
            SharedTokenKind.LineWidth,
            BindingPriority.Template,
            new RenderScaleAwareDoubleConfigure(this));
        var inputManager = AvaloniaLocator.Current.GetService(typeof(IInputManager)) as IInputManager;
        Debug.Assert(inputManager != null);
        _pointerPositionDisposable = inputManager.Process.Subscribe(DetectPointerPosition);
        SyncTimeValueToPanel(SelectedTime ?? TimeSpan.Zero);
        ApplyPendingDisplayTime();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _pointerPositionDisposable?.Dispose();
        _spacerWidthDisposable?.Dispose();
        _pointerPositionDisposable = null;
        _spacerWidthDisposable     = null;
        IsPointerInSelector = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachSelectorHandlers();
        base.OnApplyTemplate(e);

        _pickerSelectorContainer = e.NameScope.Get<Grid>("PART_PickerContainer");
        _periodHost              = e.NameScope.Get<Panel>("PART_PeriodHost");
        _headerText              = e.NameScope.Get<TextBlock>("PART_HeaderText");

        _hourSelector   = e.NameScope.Get<DateTimePickerPanel>("PART_HourSelector");
        _minuteSelector = e.NameScope.Get<DateTimePickerPanel>("PART_MinuteSelector");
        _secondSelector = e.NameScope.Get<DateTimePickerPanel>("PART_SecondSelector");
        _periodSelector = e.NameScope.Get<DateTimePickerPanel>("PART_PeriodSelector");
        SetupPickerSelectorContainerHeight();

        _spacer3 = e.NameScope.Get<Rectangle>("PART_ThirdSpacer");
        InitPicker();

        if (_hourSelector is not null)
        {
            _hourSelector.SelectionChanged += HandleSelectionChanged;
            _hourSelector.CellHovered      += HandleSelectorCellHovered;
            _hourSelector.CellDbClicked    += HandleSelectorCellDbClicked;
        }

        if (_minuteSelector is not null)
        {
            _minuteSelector.SelectionChanged += HandleSelectionChanged;
            _minuteSelector.CellHovered      += HandleSelectorCellHovered;
            _minuteSelector.CellDbClicked    += HandleSelectorCellDbClicked;
        }

        if (_secondSelector is not null)
        {
            _secondSelector.SelectionChanged += HandleSelectionChanged;
            _secondSelector.CellHovered      += HandleSelectorCellHovered;
            _secondSelector.CellDbClicked    += HandleSelectorCellDbClicked;
        }

        if (_periodSelector is not null)
        {
            _periodSelector.SelectionChanged += HandleSelectionChanged;
            _periodSelector.CellHovered      += HandleSelectorCellHovered;
            _periodSelector.CellDbClicked    += HandleSelectorCellDbClicked;
            // Keep LocalValue priority for the internal locale relay. ControlTheme TemplateBinding
            // would lower the target priority and let style setters replace the locale text.
            _periodSelector[!DateTimePickerPanel.AmTextProperty] = this[!AmTextProperty];
            _periodSelector[!DateTimePickerPanel.PmTextProperty] = this[!PmTextProperty];
        }
        ApplyPendingDisplayTime();
    }

    private void HandleSelectorCellDbClicked(object? sender, CellDbClickedEventArgs args)
    {
        if (args.IsSelected)
        {
            SelectedTime = CollectValue();
            TimeSelected?.Invoke(this, new TimeSelectedEventArgs(SelectedTime));
        }
    }

    private void HandleSelectorCellHovered(object? sender, CellHoverEventArgs args)
    {
        if (args.CellHoverInfo == null)
        {
            HoverTimeChanged?.Invoke(this, new TimeSelectedEventArgs(null));
            return;
        }
        var selectedTime  = CollectValue(false);
        var hour          = selectedTime.Hours;
        var minute        = selectedTime.Minutes;
        var second        = selectedTime.Seconds;
        var period        = _periodSelector?.SelectedValue ?? default;
        var cellHoverInfo = args.CellHoverInfo;

        if (cellHoverInfo.HasValue)
        {
            var panelType = cellHoverInfo.Value.PanelType;
            var cellValue = cellHoverInfo.Value.CellValue;
            if (panelType == DateTimePickerPanelType.Hour)
            {
                hour = cellValue;
            }
            else if (panelType == DateTimePickerPanelType.Minute)
            {
                minute = cellValue;
            }
            else if (panelType == DateTimePickerPanelType.Second)
            {
                second = cellValue;
            }
            else if (panelType == DateTimePickerPanelType.TimePeriod)
            {
                period = cellValue;
            }

            if (ClockIdentifier == ClockIdentifierType.HourClock12)
            {
                if (period == 0 && hour == 12)
                {
                    // AM
                    hour = 0;
                }

                if (period == 1 && hour != 12)
                {
                    hour += 12;
                }
            }

            var hoverTime = new TimeSpan(hour, minute, second);
            HoverTimeChanged?.Invoke(this, new TimeSelectedEventArgs(hoverTime));
        }
    }

    private void HandleSelectionChanged(object? sender, EventArgs args)
    {
        if (_isSyncingPanelValue)
        {
            return;
        }

        var selectedValue = CollectValue();
        if (IsShowHeader)
        {
            if (_headerText is not null)
            {
                _headerText.Text =
                    DateTimeUtils.FormatTimeSpan(selectedValue, ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
            }
        }

        TempTimeSelected?.Invoke(this, new TimeSelectedEventArgs(selectedValue));
    }

    private TimeSpan CollectValue(bool translate = true)
    {
        var hour   = _hourSelector!.SelectedValue;
        var minute = _minuteSelector!.SelectedValue;
        var second = _secondSelector!.SelectedValue;
        var period = _periodSelector!.SelectedValue;

        if (translate)
        {
            if (ClockIdentifier == ClockIdentifierType.HourClock12)
            {
                hour = period == 1 ? hour == 12 ? 12 : hour + 12 : period == 0 && hour == 12 ? 0 : hour;
            }
        }

        return new TimeSpan(hour, minute, second);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MinuteIncrementProperty ||
            change.Property == SecondIncrementProperty ||
            change.Property == ClockIdentifierProperty)
        {
            InitPicker();
        }

        if (change.Property == SelectedTimeProperty)
        {
            if (this.IsAttachedToVisualTree())
            {
                SyncTimeValueToPanel(SelectedTime ?? TimeSpan.Zero);
            }
        }

        if (change.Property == ItemHeightProperty ||
            change.Property == SelectorRowCountProperty)
        {
            SetupPickerSelectorContainerHeight();
        }

        if (change.Property == AmTextProperty || change.Property == PmTextProperty)
        {
            RefreshHeaderText();
        }
    }

    private void RefreshHeaderText()
    {
        if (!IsShowHeader || _headerText is null ||
            _hourSelector is null || _minuteSelector is null ||
            _secondSelector is null || _periodSelector is null)
        {
            return;
        }

        _headerText.Text = DateTimeUtils.FormatTimeSpan(CollectValue(),
            ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
    }

    internal void SyncDisplayTimeToPanel(TimeSpan time)
    {
        _pendingDisplayTime = time;
        ApplyPendingDisplayTime();
    }

    private void SyncTimeValueToPanel(TimeSpan time)
    {
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null || _periodSelector == null)
        {
            return;
        }
        _isSyncingPanelValue = true;
        try
        {
            var clock12 = ClockIdentifier == ClockIdentifierType.HourClock12;
            _hourSelector!.MaximumValue = clock12 ? 12 : 23;
            _hourSelector.MinimumValue = clock12 ? 1 : 0;
            _hourSelector.ItemFormat = clock12 ? "%h" : "hh";
            _minuteSelector!.MaximumValue = 59;
            _minuteSelector.MinimumValue = 0;
            _minuteSelector.Increment = MinuteIncrement;
            _minuteSelector.ItemFormat = "mm";
            _secondSelector!.MaximumValue = 59;
            _secondSelector.MinimumValue = 0;
            _secondSelector.Increment = SecondIncrement;
            _secondSelector.ItemFormat = "ss";
            _periodSelector!.MaximumValue = 1;
            _periodSelector.MinimumValue = 0;
            if (_spacer3 != null)
            {
                _spacer3.IsVisible = clock12;
            }
            if (_periodHost != null)
            {
                _periodHost.IsVisible = clock12;
            }
            var hour = time.Hours;
            if (_hourSelector is not null)
            {
                _hourSelector.SelectedValue = !clock12 ? hour :
                    hour > 12 ? hour - 12 :
                    hour == 0 ? 12 : hour;
            }

            if (_minuteSelector is not null)
            {
                _minuteSelector.SelectedValue = time.Minutes;
            }

            if (_secondSelector is not null)
            {
                _secondSelector.SelectedValue = time.Seconds;
            }

            if (_periodSelector is not null)
            {
                _periodSelector.SelectedValue = hour >= 12 ? 1 : 0;
            }
        }
        finally
        {
            _isSyncingPanelValue = false;
        }

        RefreshHeaderText();
    }

    private void InitPicker()
    {
        if (_pickerSelectorContainer == null)
        {
            return;
        }
        SyncTimeValueToPanel(SelectedTime ?? TimeSpan.Zero);
    }

    private void DetachSelectorHandlers()
    {
        foreach (var selector in new[] { _hourSelector, _minuteSelector, _secondSelector, _periodSelector })
        {
            if (selector != null)
            {
                selector.SelectionChanged -= HandleSelectionChanged;
                selector.CellHovered -= HandleSelectorCellHovered;
                selector.CellDbClicked -= HandleSelectorCellDbClicked;
            }
        }
    }

    private void ApplyPendingDisplayTime()
    {
        if (_pendingDisplayTime is null ||
            !this.IsAttachedToVisualTree() ||
            _hourSelector is null ||
            _minuteSelector is null ||
            _secondSelector is null ||
            _periodSelector is null)
        {
            return;
        }

        var displayTime = _pendingDisplayTime.Value;
        _pendingDisplayTime = null;
        SyncTimeValueToPanel(displayTime);
    }

    private void SetupPickerSelectorContainerHeight()
    {
        if (_pickerSelectorContainer is null)
        {
            return;
        }

        _pickerSelectorContainer.Height = ItemHeight * SelectorRowCount;
    }
}
