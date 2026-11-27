using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using AtomUI.Desktop.Controls.Internal.Calendar;
using AtomUI.Data;
using AtomUI.Localization;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;
using CalendarRangeBarPanelControl = AtomUI.Desktop.Controls.Internal.Calendar.CalendarRangeBarPanel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A desktop calendar, and the single owner of its business state: <see cref="Value" />, <see cref="Mode" /> and
/// the three public events are managed here. The date panel and the headers only submit changes.
/// </summary>
[PseudoClasses(
    CalendarRootPseudoClass.Fullscreen,
    CalendarRootPseudoClass.Mini,
    CalendarRootPseudoClass.Month,
    CalendarRootPseudoClass.Year,
    CalendarRootPseudoClass.ShowWeek)]
[TemplatePart(DateViewerPart, typeof(DateViewer))]
[TemplatePart(RangeBarPanelPart, typeof(CalendarRangeBarPanelControl))]
public partial class Calendar : TemplatedControl
{
    #region Avalonia Properties

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime> ValueProperty =
        AvaloniaProperty.Register<Calendar, DateTime>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="Mode" /> property.
    /// </summary>
    public static readonly StyledProperty<CalendarMode> ModeProperty =
        AvaloniaProperty.Register<Calendar, CalendarMode>(
            nameof(Mode), CalendarMode.Month, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="Fullscreen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> FullscreenProperty =
        AvaloniaProperty.Register<Calendar, bool>(nameof(Fullscreen), true);

    /// <summary>
    /// Identifies the <see cref="ShowWeek" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowWeekProperty =
        AvaloniaProperty.Register<Calendar, bool>(nameof(ShowWeek));

    /// <summary>
    /// Identifies the <see cref="ValidRange" /> property.
    /// </summary>
    public static readonly StyledProperty<CalendarDateRange?> ValidRangeProperty =
        AvaloniaProperty.Register<Calendar, CalendarDateRange?>(nameof(ValidRange));

    /// <summary>
    /// Identifies the <see cref="DisabledDate" /> property.
    /// </summary>
    public static readonly StyledProperty<Func<DateTime, bool>?> DisabledDateProperty =
        AvaloniaProperty.Register<Calendar, Func<DateTime, bool>?>(nameof(DisabledDate));

    /// <summary>
    /// Identifies the <see cref="CellTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty =
        AvaloniaProperty.Register<Calendar, IDataTemplate?>(nameof(CellTemplate));

    /// <summary>
    /// Identifies the <see cref="FullCellTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FullCellTemplateProperty =
        AvaloniaProperty.Register<Calendar, IDataTemplate?>(nameof(FullCellTemplate));

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<Calendar, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Identifies the <see cref="RangeBars" /> property.
    /// </summary>
    public static readonly DirectProperty<Calendar, CalendarRangeBarCollection> RangeBarsProperty =
        AvaloniaProperty.RegisterDirect<Calendar, CalendarRangeBarCollection>(
            nameof(RangeBars),
            o => o.RangeBars,
            (o, v) => o.RangeBars = v);

    /// <summary>
    /// The selected date, and the anchor of the panel that is shown. Any time of day is dropped.
    /// Defaults to <see cref="DateTime.Today" /> taken when the instance is created.
    /// </summary>
    public DateTime Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Whether the panel shows the days of a month or the months of a year.
    /// Defaults to <see cref="CalendarMode.Month" />.
    /// </summary>
    public CalendarMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// <c>true</c> shows the full-size layout with the complete cell content, <c>false</c> the compact layout.
    /// Range bars are drawn only when it is <c>true</c>. The default is <c>true</c>.
    /// </summary>
    public bool Fullscreen
    {
        get => GetValue(FullscreenProperty);
        set => SetValue(FullscreenProperty, value);
    }

    /// <summary>
    /// <c>true</c> shows a column with the week numbers beside the date grid. The default is <c>false</c>.
    /// </summary>
    public bool ShowWeek
    {
        get => GetValue(ShowWeekProperty);
        set => SetValue(ShowWeekProperty, value);
    }

    /// <summary>
    /// The range of dates the user may select, both ends included; dates outside it cannot be selected.
    /// <c>null</c>, the default, sets no limit.
    /// </summary>
    public CalendarDateRange? ValidRange
    {
        get => GetValue(ValidRangeProperty);
        set => SetValue(ValidRangeProperty, value);
    }

    /// <summary>
    /// A rule that returns <c>true</c> for each date the user may not select.
    /// <c>null</c>, the default, disables no date.
    /// </summary>
    public Func<DateTime, bool>? DisabledDate
    {
        get => GetValue(DisabledDateProperty);
        set => SetValue(DisabledDateProperty, value);
    }

    /// <summary>
    /// Replaces the content area of the default cell, while the default date or month value and the cell state are
    /// kept. The data context is a <see cref="CalendarCellContext" />.
    /// <c>null</c>, the default, shows the default content.
    /// </summary>
    public IDataTemplate? CellTemplate
    {
        get => GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    /// <summary>
    /// Replaces the complete content of a cell and takes precedence over <see cref="CellTemplate" />. Its data
    /// context is a <see cref="CalendarCellContext" />. <c>null</c>, the default, shows the default content.
    /// </summary>
    public IDataTemplate? FullCellTemplate
    {
        get => GetValue(FullCellTemplateProperty);
        set => SetValue(FullCellTemplateProperty, value);
    }

    /// <summary>
    /// Replaces the default header. Its data context is a <see cref="CalendarHeaderContext" />, which provides the
    /// commands that submit <see cref="Value" /> and <see cref="Mode" />.
    /// <c>null</c>, the default, shows the default header.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    private CalendarRangeBarCollection _rangeBars = new();

    /// <summary>
    /// The range bars drawn above the date grid, one per continuous range of dates.
    /// They are shown only while <see cref="Fullscreen" /> is <c>true</c>, <see cref="Mode" /> is
    /// <see cref="CalendarMode.Month" /> and the collection is not empty.
    /// Setting <c>null</c> replaces the collection with an empty one.
    /// </summary>
    public CalendarRangeBarCollection RangeBars
    {
        get => _rangeBars;
        set
        {
            value ??= new CalendarRangeBarCollection();
            if (ReferenceEquals(_rangeBars, value))
            {
                return;
            }

            _rangeBars.CollectionChanged -= OnRangeBarsCollectionChanged;
            DetachAllRangeBars();
            var oldValue = _rangeBars;
            _rangeBars = value;
            _rangeBars.CollectionChanged += OnRangeBarsCollectionChanged;
            if (this.IsAttachedToVisualTree())
            {
                AttachRangeBars(_rangeBars);
            }

            RaisePropertyChanged(RangeBarsProperty, oldValue, value);
            InvalidateRangeBars();
        }
    }

    #endregion

    #region Public Events

    /// <summary>
    /// Raised after a user selection changed the selected date; the new value is committed before the event.
    /// Setting <see cref="Value" /> in code does not raise it.
    /// </summary>
    public event EventHandler<CalendarValueChangedEventArgs>? ValueChanged;

    /// <summary>
    /// Raised after every user selection, also when the selected date did not change.
    /// The value is committed before the event, and the event reports where the selection came from.
    /// </summary>
    public event EventHandler<CalendarSelectedEventArgs>? Selected;

    /// <summary>
    /// Raised after a user selection moved the shown panel to another month or year, or after a user switched
    /// <see cref="Mode" />. The new value or mode is committed first. It is raised before
    /// <see cref="ValueChanged" /> and <see cref="Selected" />.
    /// Setting <see cref="Value" /> or <see cref="Mode" /> in code does not raise it.
    /// </summary>
    public event EventHandler<CalendarPanelChangedEventArgs>? PanelChanged;

    #endregion

    #region Internal Presentation Properties

    internal static readonly StyledProperty<Avalonia.Styling.ControlTheme?> DateCellThemeProperty =
        AvaloniaProperty.Register<Calendar, Avalonia.Styling.ControlTheme?>(nameof(DateCellTheme));
    internal Avalonia.Styling.ControlTheme? DateCellTheme { get => GetValue(DateCellThemeProperty); set => SetValue(DateCellThemeProperty, value); }
    // Like DateCellTheme, each terminal supplies the panel theme that reads its own week-header Tokens.
    internal static readonly StyledProperty<Avalonia.Styling.ControlTheme?> DatePanelThemeProperty =
        AvaloniaProperty.Register<Calendar, Avalonia.Styling.ControlTheme?>(nameof(DatePanelTheme));
    internal Avalonia.Styling.ControlTheme? DatePanelTheme { get => GetValue(DatePanelThemeProperty); set => SetValue(DatePanelThemeProperty, value); }

    // Template-part metrics from the terminal's Own Tokens. Each terminal theme assigns them on the root and the
    // template passes them to its parts, so the values do not inherit through the calendar tree.
    internal static readonly StyledProperty<IBrush?> FullPanelBgProperty =
        AvaloniaProperty.Register<Calendar, IBrush?>(nameof(FullPanelBg));
    internal IBrush? FullPanelBg { get => GetValue(FullPanelBgProperty); set => SetValue(FullPanelBgProperty, value); }
    internal static readonly StyledProperty<double> YearControlWidthProperty =
        AvaloniaProperty.Register<Calendar, double>(nameof(YearControlWidth));
    internal double YearControlWidth { get => GetValue(YearControlWidthProperty); set => SetValue(YearControlWidthProperty, value); }
    internal static readonly StyledProperty<double> MonthControlWidthProperty =
        AvaloniaProperty.Register<Calendar, double>(nameof(MonthControlWidth));
    internal double MonthControlWidth { get => GetValue(MonthControlWidthProperty); set => SetValue(MonthControlWidthProperty, value); }
    internal static readonly StyledProperty<double> RangeBarHeightProperty =
        AvaloniaProperty.Register<Calendar, double>(nameof(RangeBarHeight));
    internal double RangeBarHeight { get => GetValue(RangeBarHeightProperty); set => SetValue(RangeBarHeightProperty, value); }
    internal static readonly StyledProperty<double> RangeBarGapProperty =
        AvaloniaProperty.Register<Calendar, double>(nameof(RangeBarGap));
    internal double RangeBarGap { get => GetValue(RangeBarGapProperty); set => SetValue(RangeBarGapProperty, value); }
    internal static readonly StyledProperty<Thickness> RangeBarLabelPaddingProperty =
        AvaloniaProperty.Register<Calendar, Thickness>(nameof(RangeBarLabelPadding));
    internal Thickness RangeBarLabelPadding { get => GetValue(RangeBarLabelPaddingProperty); set => SetValue(RangeBarLabelPaddingProperty, value); }
    internal static readonly StyledProperty<IDataTemplate?> DefaultDateCellTemplateProperty =
        AvaloniaProperty.Register<Calendar, IDataTemplate?>(nameof(DefaultDateCellTemplate));
    internal IDataTemplate? DefaultDateCellTemplate { get => GetValue(DefaultDateCellTemplateProperty); set => SetValue(DefaultDateCellTemplateProperty, value); }
    internal static readonly DirectProperty<Calendar, DateViewerPresentation> DatePresentationProperty =
        AvaloniaProperty.RegisterDirect<Calendar, DateViewerPresentation>(nameof(DatePresentation), calendar => calendar.DatePresentation);
    internal DateViewerPresentation DatePresentation => Fullscreen ? DateViewerPresentation.Content : DateViewerPresentation.Compact;

    internal static readonly DirectProperty<Calendar, DateViewer?> EffectiveDateViewerProperty =
        AvaloniaProperty.RegisterDirect<Calendar, DateViewer?>(nameof(EffectiveDateViewer), calendar => calendar.EffectiveDateViewer);
    internal DateViewer? EffectiveDateViewer => _dateViewer;

    internal static readonly StyledProperty<double> EffectiveMiniContentHeightProperty =
        AvaloniaProperty.Register<Calendar, double>(nameof(EffectiveMiniContentHeight), double.NaN);

    internal static readonly StyledProperty<double> EffectiveFullCellMinHeightProperty =
        AvaloniaProperty.Register<Calendar, double>(nameof(EffectiveFullCellMinHeight), double.NaN);

    internal static readonly StyledProperty<double> EffectiveRangeBarTopOffsetProperty =
        AvaloniaProperty.Register<Calendar, double>(nameof(EffectiveRangeBarTopOffset), double.NaN);

    internal double EffectiveMiniContentHeight
    {
        get => GetValue(EffectiveMiniContentHeightProperty);
        set => SetValue(EffectiveMiniContentHeightProperty, value);
    }

    internal double EffectiveFullCellMinHeight
    {
        get => GetValue(EffectiveFullCellMinHeightProperty);
        set => SetValue(EffectiveFullCellMinHeightProperty, value);
    }

    internal double EffectiveRangeBarTopOffset
    {
        get => GetValue(EffectiveRangeBarTopOffsetProperty);
        set => SetValue(EffectiveRangeBarTopOffsetProperty, value);
    }

    #endregion

    public Calendar()
    {
        SetCurrentValue(ValueProperty, DateTime.Today);
        _rangeBars.CollectionChanged += OnRangeBarsCollectionChanged;
        UpdatePresentationMetricTokenBindings();
    }

    internal const string DateViewerPart = "PART_DateViewer";
    internal const string RangeBarPanelPart = "PART_RangeBarPanel";
    internal const string DefaultHeaderPart = "PART_DefaultHeader";
    internal const string CustomHeaderPart = "PART_CustomHeader";

    private DateViewer? _dateViewer;
    private IDisposable? _dateViewerOwnerBinding;
    private CalendarRangeBarPanelControl? _rangeBarPanel;
    private CalendarHeader? _defaultHeader;
    private ContentControl? _customHeader;
    private ICalendarPresentationAdapter _presentationAdapter = DefaultCalendarPresentationAdapter.Instance;
    private OwnerTokenPropertyBindings? _metricBindings;
    private readonly Dictionary<CalendarRangeBar, CalendarRangeBarAttachment> _rangeBarAttachments =
        new(ReferenceEqualityComparer.Instance);

    internal CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// Connects the template parts: the date viewer, the range bar panel, the default header and the custom header.
    /// The parts of an earlier call are disconnected first.
    /// Derived overrides must call the base member, otherwise no part is connected.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _dateViewer?.SetContentFactory(null);
        _dateViewer?.SetAutomationNameFactory(null);
        _dateViewer?.SetHost(null);
        _dateViewerOwnerBinding?.Dispose();
        SetAndRaise(EffectiveDateViewerProperty, ref _dateViewer, e.NameScope.Find<DateViewer>(DateViewerPart));
        _dateViewer?.SetHost(this);
        _dateViewer?.SetContentFactory(CreateDateCellContext);
        _dateViewer?.SetAutomationNameFactory(CreateDateAutomationName);
        // The Calendar supplies the panel and cell themes of its date grid, so the grid takes the Calendar's Token owner
        // (instead of the DateViewer identity): Controls[Calendar] / Controls[LunarCalendar] reach the cells. The
        // DateViewer's Own Tokens are still read from the DateViewer identity.
        _dateViewerOwnerBinding = _dateViewer is null ? null : TokenOwnerForwarding.Forward(this, _dateViewer);

        _rangeBarPanel = e.NameScope.Find<CalendarRangeBarPanelControl>(RangeBarPanelPart);
        UpdateRangeBarPanelVisibility();

        if (_defaultHeader is not null)
        {
            _defaultHeader.YearSelected -= OnHeaderYearSelected;
            _defaultHeader.MonthSelected -= OnHeaderMonthSelected;
            _defaultHeader.ModeSwitched -= OnHeaderModeSwitched;
        }

        _defaultHeader = e.NameScope.Find<CalendarHeader>(DefaultHeaderPart);
        if (_defaultHeader is not null)
        {
            _defaultHeader.PresentationAdapter = _presentationAdapter;
            _defaultHeader.YearSelected += OnHeaderYearSelected;
            _defaultHeader.MonthSelected += OnHeaderMonthSelected;
            _defaultHeader.ModeSwitched += OnHeaderModeSwitched;
        }

        _customHeader = e.NameScope.Find<ContentControl>(CustomHeaderPart);

        RefreshCustomHeaderContent();
        ApplyCulture();
        InvalidateRangeBars();
        UpdateRootPseudoClasses();
    }

    /// <summary>
    /// Sets the content of the custom header to a new <see cref="CalendarHeaderContext" />, or to <c>null</c> when
    /// <see cref="HeaderTemplate" /> is <c>null</c>. The context lets the template submit <see cref="Value" /> and
    /// <see cref="Mode" /> through its commands, and it is rebuilt whenever one of them changes.
    /// </summary>
    private void RefreshCustomHeaderContent()
    {
        if (_customHeader is not null)
        {
            _customHeader.Content = HeaderTemplate is null ? null : BuildHeaderContext();
        }
    }

    private ILanguageManager? _subscribedLanguageManager;

    /// <summary>
    /// Makes this calendar the host of the date viewer and starts observing the language and <see cref="RangeBars" />.
    /// Derived overrides must call the base member for both.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _dateViewer?.SetHost(this);
        _dateViewer?.SetContentFactory(CreateDateCellContext);
        _dateViewer?.SetAutomationNameFactory(CreateDateAutomationName);
        AttachLanguageListener();
        AttachRangeBars(RangeBars);
        ApplyCulture();
    }

    /// <summary>
    /// Releases the date viewer, the range bars and the language listener.
    /// Derived overrides must call the base member, otherwise the range bars and the listener stay attached.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _dateViewer?.SetContentFactory(null);
        _dateViewer?.SetAutomationNameFactory(null);
        _dateViewer?.SetHost(null);
        base.OnDetachedFromVisualTree(e);
        DetachAllRangeBars();
        DetachLanguageListener();
    }

    private void AttachRangeBars(IEnumerable<CalendarRangeBar> rangeBars)
    {
        foreach (var rangeBar in rangeBars)
        {
            AttachRangeBar(rangeBar);
        }
    }

    private void AttachRangeBarsFromList(IList rangeBars)
    {
        for (var i = 0; i < rangeBars.Count; i++)
        {
            if (rangeBars[i] is CalendarRangeBar rangeBar)
            {
                AttachRangeBar(rangeBar);
            }
        }
    }

    private void DetachRangeBarsFromList(IList rangeBars)
    {
        for (var i = 0; i < rangeBars.Count; i++)
        {
            if (rangeBars[i] is CalendarRangeBar rangeBar)
            {
                DetachRangeBar(rangeBar);
            }
        }
    }

    private void AttachRangeBar(CalendarRangeBar rangeBar)
    {
        if (_rangeBarAttachments.TryGetValue(rangeBar, out var attachment))
        {
            attachment.ReferenceCount++;
            return;
        }

        rangeBar.PropertyChanged += OnRangeBarPropertyChanged;
        _rangeBarAttachments.Add(rangeBar, new CalendarRangeBarAttachment(rangeBar.AttachResourceHost(this)));
    }

    private void DetachRangeBar(CalendarRangeBar rangeBar)
    {
        if (!_rangeBarAttachments.TryGetValue(rangeBar, out var attachment))
        {
            return;
        }

        attachment.ReferenceCount--;
        if (attachment.ReferenceCount > 0)
        {
            return;
        }

        rangeBar.PropertyChanged -= OnRangeBarPropertyChanged;
        attachment.Dispose();
        _rangeBarAttachments.Remove(rangeBar);
    }

    private void DetachAllRangeBars()
    {
        foreach (var (rangeBar, attachment) in _rangeBarAttachments)
        {
            rangeBar.PropertyChanged -= OnRangeBarPropertyChanged;
            attachment.Dispose();
        }

        _rangeBarAttachments.Clear();
    }

    private void ResetRangeBarAttachments()
    {
        DetachAllRangeBars();
        if (this.IsAttachedToVisualTree())
        {
            AttachRangeBars(RangeBars);
        }
    }

    private void OnRangeBarsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (this.IsAttachedToVisualTree())
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems is not null)
                    {
                        AttachRangeBarsFromList(e.NewItems);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is not null)
                    {
                        DetachRangeBarsFromList(e.OldItems);
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems is not null)
                    {
                        DetachRangeBarsFromList(e.OldItems);
                    }

                    if (e.NewItems is not null)
                    {
                        AttachRangeBarsFromList(e.NewItems);
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetRangeBarAttachments();
                    break;
            }
        }

        InvalidateRangeBars();
    }

    private void OnRangeBarPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        InvalidateRangeBars();
    }

    private void InvalidateRangeBars()
    {
        UpdateRangeBarPanelVisibility();
        _rangeBarPanel?.InvalidateRangeBars();
    }

    private void UpdateRangeBarPanelVisibility()
    {
        if (_rangeBarPanel is not null)
        {
            _rangeBarPanel.IsVisible = Mode == CalendarMode.Month && Fullscreen && RangeBars.Count > 0;
        }
    }

    private void AttachLanguageListener()
    {
        if (_subscribedLanguageManager is not null)
        {
            return;
        }

        var languageManager = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
            : null;
        if (languageManager is null)
        {
            return;
        }

        languageManager.LanguageChanged += OnLanguageChanged;
        _subscribedLanguageManager = languageManager;
    }

    private void DetachLanguageListener()
    {
        if (_subscribedLanguageManager is null)
        {
            return;
        }

        _subscribedLanguageManager.LanguageChanged -= OnLanguageChanged;
        _subscribedLanguageManager = null;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => ApplyCulture();

    /// <summary>
    /// Resolves the culture of the current language and hands it to the date panel and the default header.
    /// Both refresh themselves. The custom header context is rebuilt as well.
    /// </summary>
    private void ApplyCulture()
    {
        var culture = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)?.Current.FormattingCulture
            : null;
        culture ??= CultureInfo.CurrentCulture;
        CurrentCulture = culture;
        _dateViewer?.RefreshHost();

        if (_defaultHeader is not null)
        {
            _defaultHeader.Culture = culture;
        }

        RefreshCustomHeaderContent();
    }

    internal void SetPresentationAdapter(ICalendarPresentationAdapter presentationAdapter)
    {
        ArgumentNullException.ThrowIfNull(presentationAdapter);
        if (ReferenceEquals(_presentationAdapter, presentationAdapter))
        {
            return;
        }

        _presentationAdapter = presentationAdapter;
        UpdatePresentationMetricTokenBindings();
        _dateViewer?.RefreshHost();
        _dateViewer?.RefreshContent();

        if (_defaultHeader is not null)
        {
            _defaultHeader.PresentationAdapter = presentationAdapter;
        }

        RefreshCustomHeaderContent();
    }

    private void UpdatePresentationMetricTokenBindings()
    {
        // The active adapter chooses the owner and keys at runtime. A fixed ControlTheme cannot follow this switch.
        var metrics = _presentationAdapter.Metrics;
        var bindings = new OwnerTokenPropertyBindings(this, 3, BindingPriority.Template);
        try
        {
            bindings.Bind(0, EffectiveMiniContentHeightProperty, MetricKey(metrics.OwnerType, metrics.MiniContentHeightResourceKey));
            bindings.Bind(1, EffectiveFullCellMinHeightProperty, MetricKey(metrics.OwnerType, metrics.FullCellMinHeightResourceKey));
            bindings.Bind(2, EffectiveRangeBarTopOffsetProperty, MetricKey(metrics.OwnerType, metrics.RangeBarTopOffsetResourceKey));
        }
        catch
        {
            bindings.Dispose();
            throw;
        }

        var previous = _metricBindings;
        _metricBindings = bindings;
        previous?.Dispose();
    }

    private static object MetricKey(Type ownerType, Enum tokenKind) =>
        tokenKind is SharedTokenKind globalKind
            ? ControlTokenResourceKey.Global(ownerType, globalKind)
            : ControlTokenResourceKey.Own(ownerType, tokenKind);

    internal void RefreshPresentation()
    {
        _dateViewer?.RefreshHost();
        _dateViewer?.RefreshContent();
        _defaultHeader?.RefreshPresentation();
        RefreshCustomHeaderContent();
    }

    private void OnHeaderYearSelected(object? sender, DateTime target) =>
        CommitUserSelection(target, CalendarSelectSource.Year);

    private void OnHeaderMonthSelected(object? sender, DateTime target) =>
        CommitUserSelection(target, CalendarSelectSource.Month);

    private void OnHeaderModeSwitched(object? sender, CalendarMode mode) =>
        CommitModeChange(mode);

    /// <summary>
    /// Refreshes the date panel, the headers, the range bar panel and the root pseudo classes after
    /// <see cref="Mode" />, <see cref="Value" />, <see cref="Fullscreen" />, <see cref="ShowWeek" />,
    /// <see cref="ValidRange" />, <see cref="DisabledDate" /> or <see cref="HeaderTemplate" /> changed.
    /// A <see cref="Value" /> with a time of day is replaced by its date part.
    /// Derived overrides must call the base member for these updates.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ModeProperty)
        {
            RefreshCustomHeaderContent();
            _dateViewer?.RefreshHost();
            UpdateRangeBarPanelVisibility();
            UpdateRootPseudoClasses();
        }
        else if (change.Property == ValueProperty)
        {
            var value = (DateTime)change.NewValue!;
            if (value != value.Date)
            {
                SetCurrentValue(ValueProperty, value.Date);
                return;
            }

            RefreshCustomHeaderContent();
            _dateViewer?.RefreshHost();
        }
        else if (change.Property == FullscreenProperty ||
                 change.Property == ShowWeekProperty)
        {
            RaisePropertyChanged(DatePresentationProperty, default, DatePresentation);
            _dateViewer?.RefreshHost();
            UpdateRangeBarPanelVisibility();
            UpdateRootPseudoClasses();
        }
        else if (change.Property == HeaderTemplateProperty)
        {
            RefreshCustomHeaderContent();
        }
        else if (change.Property == ValidRangeProperty || change.Property == DisabledDateProperty)
        {
            _dateViewer?.RefreshHost();
        }
    }

    private void UpdateRootPseudoClasses()
    {
        PseudoClasses.Set(CalendarRootPseudoClass.Fullscreen, Fullscreen);
        PseudoClasses.Set(CalendarRootPseudoClass.Mini, !Fullscreen);
        PseudoClasses.Set(CalendarRootPseudoClass.Month, Mode == CalendarMode.Month);
        PseudoClasses.Set(CalendarRootPseudoClass.Year, Mode == CalendarMode.Year);
        PseudoClasses.Set(CalendarRootPseudoClass.ShowWeek, ShowWeek);
    }

    /// <summary>
    /// Commits a user selection: writes <see cref="Value" /> and raises <see cref="PanelChanged" />,
    /// <see cref="ValueChanged" /> and <see cref="Selected" /> in that order.
    /// <see cref="ValidRange" /> and <see cref="DisabledDate" /> are not checked here.
    /// </summary>
    /// <param name="target">
    /// The selected date. A time of day is dropped.
    /// </param>
    /// <param name="source">
    /// Where the selection came from; <see cref="Selected" /> reports it.
    /// </param>
    internal void CommitUserSelection(DateTime target, CalendarSelectSource source)
    {
        target = target.Date;
        var oldValue = Value.Date;
        var panelChanged = Mode == CalendarMode.Year
            ? oldValue.Year != target.Year
            : oldValue.Year != target.Year || oldValue.Month != target.Month;

        SetCurrentValue(ValueProperty, target);

        if (panelChanged)
        {
            PanelChanged?.Invoke(this, new CalendarPanelChangedEventArgs(target, Mode));
        }

        if (target != oldValue)
        {
            ValueChanged?.Invoke(this, new CalendarValueChangedEventArgs(oldValue, target));
        }

        Selected?.Invoke(this, new CalendarSelectedEventArgs(target, source));
    }

    /// <summary>
    /// Commits a user Mode switch: writes <see cref="Mode" /> and raises <see cref="PanelChanged" /> once.
    /// <see cref="ValueChanged" /> and <see cref="Selected" /> are not raised.
    /// A switch to the current Mode does nothing.
    /// </summary>
    internal void CommitModeChange(CalendarMode mode)
    {
        if (mode == Mode)
        {
            return;
        }

        SetCurrentValue(ModeProperty, mode);

        PanelChanged?.Invoke(this, new CalendarPanelChangedEventArgs(Value.Date, mode));
    }

    /// <summary>
    /// Builds a strongly typed <see cref="CalendarHeaderContext" /> for a custom header template.
    /// The CanExecute of both commands checks the type of the parameter. <c>ChangeValueCommand</c> commits with
    /// <see cref="CalendarSelectSource.Customize" /> and applies neither <see cref="ValidRange" /> nor
    /// <see cref="DisabledDate" />: a custom header enforces them itself.
    /// The mode command accepts only <see cref="CalendarMode.Month" /> and <see cref="CalendarMode.Year" />.
    /// </summary>
    internal CalendarHeaderContext BuildHeaderContext()
    {
        var changeValue = new CalendarRelayCommand(
            execute: p => CommitUserSelection((DateTime)p!, CalendarSelectSource.Customize),
            canExecute: p => p is DateTime);

        var changeMode = new CalendarRelayCommand(
            execute: p => CommitModeChange((CalendarMode)p!),
            canExecute: p => p is CalendarMode mode && (mode == CalendarMode.Month || mode == CalendarMode.Year));

        return _presentationAdapter.CreateHeaderContext(this, changeValue, changeMode);
    }

    private sealed class CalendarRangeBarAttachment : IDisposable
    {
        private readonly IDisposable _resourceHostAttachment;

        public int ReferenceCount { get; set; } = 1;

        public CalendarRangeBarAttachment(IDisposable resourceHostAttachment)
        {
            _resourceHostAttachment = resourceHostAttachment;
        }

        public void Dispose()
        {
            _resourceHostAttachment.Dispose();
        }
    }
}
