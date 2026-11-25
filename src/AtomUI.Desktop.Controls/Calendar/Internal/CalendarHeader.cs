using System.Globalization;
using AtomUI.Desktop.Controls.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

/// <summary>
/// Calendar 专用默认 Header。展示 Year Select、条件 Month Select 与 Month/Year 模式切换，
/// 只报告用户操作，不拥有 Value/Mode/选择状态。
/// </summary>
[TemplatePart(YearSelectPart, typeof(ComboBox))]
[TemplatePart(MonthSelectPart, typeof(ComboBox))]
[TemplatePart(ModeSwitchPart, typeof(OptionButtonGroup))]
internal sealed partial class CalendarHeader : TemplatedControl
{
    internal const string YearSelectPart = "PART_YearSelect";
    internal const string MonthSelectPart = "PART_MonthSelect";
    internal const string ModeSwitchPart = "PART_ModeSwitch";

    public static readonly StyledProperty<DateTime> ValueProperty =
        AvaloniaProperty.Register<CalendarHeader, DateTime>(nameof(Value));

    public static readonly StyledProperty<CalendarMode> ModeProperty =
        AvaloniaProperty.Register<CalendarHeader, CalendarMode>(nameof(Mode));

    public static readonly StyledProperty<CalendarDateRange?> ValidRangeProperty =
        AvaloniaProperty.Register<CalendarHeader, CalendarDateRange?>(nameof(ValidRange));

    public static readonly StyledProperty<CultureInfo?> CultureProperty =
        AvaloniaProperty.Register<CalendarHeader, CultureInfo?>(nameof(Culture));

    public static readonly StyledProperty<bool> FullscreenProperty =
        AvaloniaProperty.Register<CalendarHeader, bool>(nameof(Fullscreen), true);

    public DateTime Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public CalendarMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public CalendarDateRange? ValidRange
    {
        get => GetValue(ValidRangeProperty);
        set => SetValue(ValidRangeProperty, value);
    }

    public CultureInfo? Culture
    {
        get => GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    public bool Fullscreen
    {
        get => GetValue(FullscreenProperty);
        set => SetValue(FullscreenProperty, value);
    }

    // Minimum widths of the year and month Selects. The hosting Calendar or LunarCalendar theme assigns its own
    // Token to these plain properties, so the values reach only the header instead of inheriting through the tree.
    internal static readonly StyledProperty<double> YearControlWidthProperty =
        AvaloniaProperty.Register<CalendarHeader, double>(nameof(YearControlWidth));

    internal static readonly StyledProperty<double> MonthControlWidthProperty =
        AvaloniaProperty.Register<CalendarHeader, double>(nameof(MonthControlWidth));

    internal double YearControlWidth
    {
        get => GetValue(YearControlWidthProperty);
        set => SetValue(YearControlWidthProperty, value);
    }

    internal double MonthControlWidth
    {
        get => GetValue(MonthControlWidthProperty);
        set => SetValue(MonthControlWidthProperty, value);
    }

    /// <summary>用户在 Header 选择了年份。参数是完整目标日期（保留月/日，收敛到合法月）。</summary>
    public event EventHandler<DateTime>? YearSelected;

    /// <summary>用户在 Header 选择了月份。参数是完整目标日期。</summary>
    public event EventHandler<DateTime>? MonthSelected;

    /// <summary>用户切换了 Month/Year 模式。</summary>
    public event EventHandler<CalendarMode>? ModeSwitched;

    private AtomUI.Desktop.Controls.ComboBox? _yearSelect;
    private AtomUI.Desktop.Controls.ComboBox? _monthSelect;
    private OptionButtonGroup? _modeSwitch;
    private bool _suppress;
    private bool _yearDropDownActive;
    private bool _monthDropDownActive;
    private ICalendarPresentationAdapter _presentationAdapter = DefaultCalendarPresentationAdapter.Instance;

    internal ICalendarPresentationAdapter PresentationAdapter
    {
        get => _presentationAdapter;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_presentationAdapter, value))
            {
                return;
            }

            _presentationAdapter = value;
            SyncFromState();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Detach();

        _yearSelect = e.NameScope.Find<AtomUI.Desktop.Controls.ComboBox>(YearSelectPart);
        _monthSelect = e.NameScope.Find<AtomUI.Desktop.Controls.ComboBox>(MonthSelectPart);
        _modeSwitch = e.NameScope.Find<OptionButtonGroup>(ModeSwitchPart);

        if (_yearSelect is not null)
        {
            _yearSelect.SelectionChanged += OnYearChanged;
            _yearSelect.DropDownOpened += OnSelectDropDownOpened;
            _yearSelect.DropDownClosed += OnSelectDropDownClosed;
        }

        if (_monthSelect is not null)
        {
            _monthSelect.SelectionChanged += OnMonthChanged;
            _monthSelect.DropDownOpened += OnSelectDropDownOpened;
            _monthSelect.DropDownClosed += OnSelectDropDownClosed;
        }

        if (_modeSwitch is not null)
        {
            _modeSwitch.SelectionChanged += OnModeChanged;
        }

        UpdateControlSizes();
        SyncFromState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FullscreenProperty)
        {
            UpdateControlSizes();
        }
        else if (change.Property == ValueProperty ||
                 change.Property == ModeProperty ||
                 change.Property == ValidRangeProperty ||
                 change.Property == CultureProperty)
        {
            SyncFromState();
        }
    }

    private void Detach()
    {
        if (_yearSelect is not null)
        {
            _yearSelect.SelectionChanged -= OnYearChanged;
            _yearSelect.DropDownOpened -= OnSelectDropDownOpened;
            _yearSelect.DropDownClosed -= OnSelectDropDownClosed;
        }

        if (_monthSelect is not null)
        {
            _monthSelect.SelectionChanged -= OnMonthChanged;
            _monthSelect.DropDownOpened -= OnSelectDropDownOpened;
            _monthSelect.DropDownClosed -= OnSelectDropDownClosed;
        }

        if (_modeSwitch is not null)
        {
            _modeSwitch.SelectionChanged -= OnModeChanged;
        }

        _yearDropDownActive = false;
        _monthDropDownActive = false;
    }

    private bool IsDropDownActive =>
        _yearSelect?.IsDropDownOpen == true ||
        _monthSelect?.IsDropDownOpen == true ||
        _yearDropDownActive ||
        _monthDropDownActive;

    private void SyncFromState()
    {
        if (_yearSelect is null || _monthSelect is null)
        {
            return;
        }

        var culture = Culture ?? CultureInfo.CurrentCulture;
        var value = Value.Date;
        var effectiveRange = PresentationAdapter.GetEffectiveRange(ValidRange);

        var years = effectiveRange.IsEmpty
            ? Array.Empty<int>()
            : CalendarHeaderOptions.BuildYearOptions(
                value.Year, effectiveRange.Start?.Year, effectiveRange.End?.Year);
        var yearItems = years
            .Select(y => new CalendarHeaderItem(y, PresentationAdapter.FormatYearOption(y, culture)))
            .ToList();

        // Month Select 仅 Month 模式显示
        var showMonth = Mode == CalendarMode.Month;
        var monthItems = showMonth
            ? (effectiveRange.IsEmpty
                ? Array.Empty<int>()
                : CalendarHeaderOptions.BuildMonthOptions(value.Year, effectiveRange.Start, effectiveRange.End))
                .Select(m => new CalendarHeaderItem(m, PresentationAdapter.FormatMonthOption(value.Year, m, culture)))
                .ToList()
            : null;

        var yearItemsChanged = !HasSameItems(_yearSelect.ItemsSource, yearItems);
        var monthItemsChanged = monthItems is not null && !HasSameItems(_monthSelect.ItemsSource, monthItems);

        // Replacing ItemsSource creates/removes popup item visuals. Keep the current
        // source intact until the popup has completed its close traversal.
        if (IsDropDownActive && (yearItemsChanged || monthItemsChanged))
        {
            return;
        }

        _suppress = true;
        try
        {
            if (yearItemsChanged)
            {
                _yearSelect.ItemsSource = yearItems;
            }

            if (monthItemsChanged && monthItems is not null)
            {
                _monthSelect.ItemsSource = monthItems;
            }

            SetSelectedItem(_yearSelect, value.Year);
            if (showMonth)
            {
                SetSelectedItem(_monthSelect, value.Month);
            }

            if (_monthSelect.IsVisible != showMonth)
            {
                _monthSelect.IsVisible = showMonth;
            }

            if (_modeSwitch is not null)
            {
                if (_modeSwitch.Items.Count >= 2)
                {
                    var localizer = Application.Current is { } application
                        ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)
                        : null;
                    var monthLabel = localizer?.Get(CalendarControlLangResourceKind.Month)
                                     ?? CalendarControlLangResourceKind.Month.ToString();
                    var yearLabel = localizer?.Get(CalendarControlLangResourceKind.Year)
                                    ?? CalendarControlLangResourceKind.Year.ToString();

                    if (_modeSwitch.Items[0] is OptionButton monthItem && !Equals(monthItem.Content, monthLabel))
                    {
                        monthItem.Content = monthLabel;
                    }

                    if (_modeSwitch.Items[1] is OptionButton yearItem && !Equals(yearItem.Content, yearLabel))
                    {
                        yearItem.Content = yearLabel;
                    }
                }

                var selectedIndex = Mode == CalendarMode.Year ? 1 : 0;
                if (_modeSwitch.SelectedIndex != selectedIndex)
                {
                    _modeSwitch.SelectedIndex = selectedIndex;
                }
            }
        }
        finally
        {
            _suppress = false;
        }
    }

    internal void RefreshPresentation() => SyncFromState();

    private static bool HasSameItems(
        System.Collections.IEnumerable? source,
        IReadOnlyList<CalendarHeaderItem> expected)
    {
        return source is System.Collections.Generic.IEnumerable<CalendarHeaderItem> current &&
               current.SequenceEqual(expected);
    }

    private static void SetSelectedItem(AtomUI.Desktop.Controls.ComboBox select, int key)
    {
        if (select.SelectedItem is CalendarHeaderItem current && current.Key == key)
        {
            return;
        }

        if (select.ItemsSource is System.Collections.Generic.IEnumerable<CalendarHeaderItem> items)
        {
            select.SelectedItem = items.FirstOrDefault(item => item.Key == key);
        }
    }

    private void OnSelectDropDownOpened(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, _yearSelect))
        {
            _yearDropDownActive = true;
        }
        else if (ReferenceEquals(sender, _monthSelect))
        {
            _monthDropDownActive = true;
        }
    }

    private void OnSelectDropDownClosed(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, _yearSelect))
        {
            _yearDropDownActive = false;
        }
        else if (ReferenceEquals(sender, _monthSelect))
        {
            _monthDropDownActive = false;
        }

        SyncFromState();
    }

    private void UpdateControlSizes()
    {
        var sizeType = Fullscreen ? CustomizableSizeType.Middle : CustomizableSizeType.Small;
        if (_yearSelect is not null)
        {
            _yearSelect.SizeType = sizeType;
        }

        if (_monthSelect is not null)
        {
            _monthSelect.SizeType = sizeType;
        }

        if (_modeSwitch is not null)
        {
            _modeSwitch.SizeType = sizeType;
        }
    }

    private void OnYearChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppress || _yearSelect?.SelectedItem is not CalendarHeaderItem item)
        {
            return;
        }

        var value = Value.Date;
        var year = item.Key;
        var effectiveRange = PresentationAdapter.GetEffectiveRange(ValidRange);
        var month = CalendarHeaderOptions.ClampMonthToYear(
            value.Month, year, effectiveRange.Start, effectiveRange.End);
        var day = Math.Min(value.Day, DateTime.DaysInMonth(year, month));
        YearSelected?.Invoke(this, new DateTime(year, month, day));
    }

    private void OnMonthChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppress || _monthSelect?.SelectedItem is not CalendarHeaderItem item)
        {
            return;
        }

        var value = Value.Date;
        var month = item.Key;
        var day = Math.Min(value.Day, DateTime.DaysInMonth(value.Year, month));
        MonthSelected?.Invoke(this, new DateTime(value.Year, month, day));
    }

    private void OnModeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppress || _modeSwitch is null)
        {
            return;
        }

        var mode = _modeSwitch.SelectedIndex == 1 ? CalendarMode.Year : CalendarMode.Month;
        if (mode != Mode)
        {
            ModeSwitched?.Invoke(this, mode);
        }
    }
}

/// <summary>Header 下拉项：业务键 + 显示文本。</summary>
internal sealed record CalendarHeaderItem(int Key, string Display)
{
    public override string ToString() => Display;
}
