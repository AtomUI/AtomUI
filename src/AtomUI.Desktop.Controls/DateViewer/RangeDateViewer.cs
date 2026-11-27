using AtomUI.Controls;
using System.Globalization;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Two side-by-side calendar panels for picking a date range: the first pick sets the start of
/// <see cref="Value" />, the second sets its end, and the second panel shows the period after the first one.
/// </summary>
public partial class RangeDateViewer : TemplatedControl, IDatePanelHost
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<DateViewerRange?> ValueProperty = AvaloniaProperty.Register<RangeDateViewer, DateViewerRange?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    /// <summary>
    /// Identifies the <see cref="DisplayDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime> DisplayDateProperty = DateViewer.DisplayDateProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="SelectionUnit" /> property.
    /// </summary>
    public static readonly StyledProperty<DateViewerSelectionUnit> SelectionUnitProperty = DateViewer.SelectionUnitProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="Presentation" /> property.
    /// </summary>
    public static readonly StyledProperty<DateViewerPresentation> PresentationProperty = DateViewer.PresentationProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="MinDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty = DateViewer.MinDateProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="MaxDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty = DateViewer.MaxDateProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="DisabledDate" /> property.
    /// </summary>
    public static readonly StyledProperty<Func<DateTime, bool>?> DisabledDateProperty = DateViewer.DisabledDateProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="FirstDayOfWeek" /> property.
    /// </summary>
    public static readonly StyledProperty<DayOfWeek?> FirstDayOfWeekProperty = DateViewer.FirstDayOfWeekProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="ShowWeek" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowWeekProperty = DateViewer.ShowWeekProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="ShowHeader" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowHeaderProperty = DateViewer.ShowHeaderProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty = DateViewer.HeaderTemplateProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="CellTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty = DateViewer.CellTemplateProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="FullCellTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FullCellTemplateProperty = DateViewer.FullCellTemplateProperty.AddOwner<RangeDateViewer>();
    /// <summary>
    /// Identifies the <see cref="PanelKind" /> property.
    /// </summary>
    public static readonly DirectProperty<RangeDateViewer, DateViewerPanelKind> PanelKindProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DateViewerPanelKind>(nameof(PanelKind), control => control.PanelKind);
    /// <summary>
    /// Identifies the <see cref="FocusedValue" /> property.
    /// </summary>
    public static readonly DirectProperty<RangeDateViewer, DateTime?> FocusedValueProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DateTime?>(nameof(FocusedValue), control => control.FocusedValue);
    /// <summary>
    /// Identifies the <see cref="HoveredValue" /> property.
    /// </summary>
    public static readonly DirectProperty<RangeDateViewer, DateTime?> HoveredValueProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DateTime?>(nameof(HoveredValue), control => control.HoveredValue);
    /// <summary>
    /// The selected date range. One pick sets <see cref="DateViewerRange.Start" /> and clears
    /// <see cref="DateViewerRange.End" />; the next pick sets the end and orders the two dates, and the pick after
    /// that starts a new range. Setting a range that has a start but no end makes the next pick complete it.
    /// <see langword="null" />, the default, means that nothing is selected.
    /// The default binding mode is <see cref="BindingMode.TwoWay" /> and data validation is enabled.
    /// </summary>
    public DateViewerRange? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    /// <summary>
    /// The date that anchors the first panel; the second panel shows the period that follows it.
    /// Navigation inside the viewer moves it together with <see cref="PanelKind" />. It starts at today.
    /// Bindings to it default to <see cref="BindingMode.TwoWay" />.
    /// </summary>
    public DateTime DisplayDate { get => GetValue(DisplayDateProperty); set => SetValue(DisplayDateProperty, value); }
    /// <summary>
    /// What a pick selects: a day, a week, a month, a quarter or a year. It also decides the panel in which picks
    /// select; a change of the unit resets <see cref="PanelKind" /> to that panel. The default is a day.
    /// </summary>
    public DateViewerSelectionUnit SelectionUnit { get => GetValue(SelectionUnitProperty); set => SetValue(SelectionUnitProperty, value); }
    /// <summary>
    /// How the cells of both panels are laid out. <see cref="DateViewerPresentation.Compact" />, the default, keeps
    /// fixed-size cells as in a calendar; <see cref="DateViewerPresentation.Content" /> leaves room for cell content.
    /// </summary>
    public DateViewerPresentation Presentation { get => GetValue(PresentationProperty); set => SetValue(PresentationProperty, value); }
    /// <summary>
    /// The earliest date a cell may show; the boundary date itself is not disabled.
    /// Earlier cells are disabled, and the panels cannot be navigated to a period before the one that contains it.
    /// <see langword="null" />, the default, leaves the range open at this side.
    /// </summary>
    public DateTime? MinDate { get => GetValue(MinDateProperty); set => SetValue(MinDateProperty, value); }
    /// <summary>
    /// The latest date a cell may show; the boundary date itself is not disabled.
    /// Later cells are disabled, and the panels cannot be navigated to a period after the one that contains it.
    /// <see langword="null" />, the default, leaves the range open at this side.
    /// </summary>
    public DateTime? MaxDate { get => GetValue(MaxDateProperty); set => SetValue(MaxDateProperty, value); }
    /// <summary>
    /// A predicate that receives the date of a cell and returns <see langword="true" /> to disable the cell.
    /// It applies in addition to <see cref="MinDate" /> and <see cref="MaxDate" />. <see langword="null" />, the
    /// default, disables nothing.
    /// </summary>
    public Func<DateTime, bool>? DisabledDate { get => GetValue(DisabledDateProperty); set => SetValue(DisabledDateProperty, value); }
    /// <summary>
    /// The weekday that starts each week row of the date panels. <see langword="null" />, the default, uses the first
    /// day of the week of the current culture.
    /// </summary>
    public DayOfWeek? FirstDayOfWeek { get => GetValue(FirstDayOfWeekProperty); set => SetValue(FirstDayOfWeekProperty, value); }
    /// <summary>
    /// Shows a column of week numbers in front of each date panel. The default is <see langword="false" />.
    /// </summary>
    public bool ShowWeek { get => GetValue(ShowWeekProperty); set => SetValue(ShowWeekProperty, value); }
    /// <summary>
    /// Shows the headers above the panels, whether they show the default navigation or the content of
    /// <see cref="HeaderTemplate" />. The default is <see langword="true" />.
    /// </summary>
    public bool ShowHeader { get => GetValue(ShowHeaderProperty); set => SetValue(ShowHeaderProperty, value); }
    /// <summary>
    /// A template for custom header content. It replaces the default navigation of both panels while it is set.
    /// Each header passes its own <see cref="DateViewerHeaderContext" />, which exposes the state and the
    /// navigation of that panel. <see langword="null" />, the default, shows the default navigation.
    /// </summary>
    public IDataTemplate? HeaderTemplate { get => GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }
    /// <summary>
    /// A template for extra content shown below the date in every cell of both panels. Its data context is the
    /// <c>DateViewerCellContext</c> of the cell, which exposes the cell state. <see langword="null" />, the
    /// default, shows the date alone.
    /// </summary>
    public IDataTemplate? CellTemplate { get => GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
    /// <summary>
    /// A template that replaces the whole content of every cell of both panels. It takes precedence over
    /// <see cref="CellTemplate" />. <see langword="null" />, the default, shows the default cell: the date and,
    /// when <see cref="CellTemplate" /> is set, its content.
    /// </summary>
    public IDataTemplate? FullCellTemplate { get => GetValue(FullCellTemplateProperty); set => SetValue(FullCellTemplateProperty, value); }
    /// <summary>
    /// The kind that both panels show, as a <see cref="DateViewerPanelKind" /> value. Navigation inside the viewer
    /// changes it, and a change of <see cref="SelectionUnit" /> resets it to the panel in which picks select.
    /// </summary>
    public DateViewerPanelKind PanelKind => _host?.ReadInput().PanelKind ?? _panelKind;
    /// <summary>
    /// The date of the cell that keyboard navigation is positioned on. It moves with keyboard navigation and starts
    /// at the start of <see cref="Value" />, or at <see cref="DisplayDate" /> when nothing is selected.
    /// </summary>
    public DateTime? FocusedValue => _session?.FocusedValue;
    /// <summary>
    /// The date of the cell under the pointer, or <see langword="null" /> while no cell is hovered.
    /// </summary>
    public DateTime? HoveredValue => _session?.HoveredValue;
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Raised after a pick changed <see cref="Value" />: the new range is committed and the panels are updated.
    /// A pick that results in the same range, and an assignment to <see cref="Value" />, do not raise it.
    /// </summary>
    public event EventHandler<RangeDateViewerValueChangedEventArgs>? ValueChanged;
    /// <summary>
    /// Raised after a pick that produces a selection, also when the range stays the same.
    /// <see cref="ValueChanged" /> is raised first when the range did change.
    /// </summary>
    public event EventHandler<RangeDateViewerSelectedEventArgs>? Selected;
    /// <summary>
    /// Raised after navigation moved the panels to another period or kind, once <see cref="DisplayDate" /> and
    /// <see cref="PanelKind" /> are updated.
    /// Navigation covers the header buttons and picks of month, quarter or year cells while the selection unit
    /// is finer. A change of <see cref="SelectionUnit" /> that resets the kind does not raise it.
    /// </summary>
    public event EventHandler<DateViewerPanelChangedEventArgs>? PanelChanged;
    /// <summary>
    /// Raised when the date under the pointer in a panel changes. The event argument carries the new date, or
    /// <see langword="null" /> when no cell is hovered.
    /// </summary>
    public event EventHandler<DateViewerHoveredValueChangedEventArgs>? HoveredValueChanged;
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<Avalonia.Styling.ControlTheme?> PanelThemeProperty = DateViewer.PanelThemeProperty.AddOwner<RangeDateViewer>();
    internal Avalonia.Styling.ControlTheme? PanelTheme { get => GetValue(PanelThemeProperty); set => SetValue(PanelThemeProperty, value); }
    internal static readonly StyledProperty<IDataTemplate?> DefaultFullCellTemplateProperty = DateViewer.DefaultFullCellTemplateProperty.AddOwner<RangeDateViewer>();
    internal IDataTemplate? DefaultFullCellTemplate { get => GetValue(DefaultFullCellTemplateProperty); set => SetValue(DefaultFullCellTemplateProperty, value); }
    internal static readonly StyledProperty<Avalonia.Styling.ControlTheme?> CellThemeProperty = DateViewer.CellThemeProperty.AddOwner<RangeDateViewer>();
    internal Avalonia.Styling.ControlTheme? CellTheme { get => GetValue(CellThemeProperty); set => SetValue(CellThemeProperty, value); }
    internal static readonly DirectProperty<RangeDateViewer, DatePanelSession> PanelSessionProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DatePanelSession>(nameof(PanelSession), control => control.PanelSession);
    internal DatePanelSession PanelSession => _session;
    internal static readonly DirectProperty<RangeDateViewer, DateViewerHeaderContext?> HeaderContextProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DateViewerHeaderContext?>(nameof(HeaderContext), control => control.HeaderContext);
    internal DateViewerHeaderContext? HeaderContext => _headerContext;
    internal static readonly DirectProperty<RangeDateViewer, DateViewerHeaderContext?> SecondaryHeaderContextProperty =
        AvaloniaProperty.RegisterDirect<RangeDateViewer, DateViewerHeaderContext?>(nameof(SecondaryHeaderContext), control => control.SecondaryHeaderContext);
    internal DateViewerHeaderContext? SecondaryHeaderContext => _secondaryHeaderContext;
    internal static readonly DirectProperty<RangeDateViewer, bool> HasSecondaryPanelProperty =
        AvaloniaProperty.RegisterDirect<RangeDateViewer, bool>(nameof(HasSecondaryPanel), control => control.HasSecondaryPanel);
    internal bool HasSecondaryPanel => _session.Input.PanelCount == 2;
    internal static readonly StyledProperty<double> PanelSpacingProperty =
        AvaloniaProperty.Register<RangeDateViewer, double>(nameof(PanelSpacing), double.NaN);
    internal double PanelSpacing { get => GetValue(PanelSpacingProperty); set => SetValue(PanelSpacingProperty, value); }
    #endregion

    private readonly DatePanelSession _session;
    private IDatePanelHost? _host;
    private DatePanelInput? _headerInput;
    private DateViewerHeaderContext? _headerContext;
    private DateViewerHeaderContext? _secondaryHeaderContext;
    private DateViewerPanelKind _panelKind;
    private DateRangeActivePart _activePart;
    private CultureInfo _culture = CultureInfo.CurrentCulture;
    private ILanguageManager? _languageManager;
    private DateTime? _lastNotifiedFocus;
    private DateTime? _lastNotifiedHover;
    private bool _lastSecondaryPanel = true;

    public RangeDateViewer()
    {
        SetCurrentValue(DisplayDateProperty, DateTime.Today);
        _session = new DatePanelSession(this);
        _session.Changed += OnSessionChanged;
        PseudoClasses.Set(":single-panel", !HasSecondaryPanel);
        RefreshHeaderContext();
    }

    /// <summary>
    /// Calls the base member, then keeps the panels in step with the change: a changed <see cref="SelectionUnit" />
    /// resets <see cref="PanelKind" /> to the panel of the unit, a changed <see cref="Value" /> decides which part of
    /// the range the next pick sets, and a change of a property that feeds the panels refreshes them.
    /// An override must call this member to keep these updates.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectionUnitProperty)
        {
            var old = _panelKind;
            _panelKind = DateViewer.TargetPanel(SelectionUnit);
            RaisePropertyChanged(PanelKindProperty, old, _panelKind);
        }
        if (change.Property == ValueProperty)
        {
            _activePart = Value is { Start: not null, End: null } ? DateRangeActivePart.End : DateRangeActivePart.Start;
        }

        if (change.Property == ValueProperty || change.Property == DisplayDateProperty || change.Property == SelectionUnitProperty ||
            change.Property == MinDateProperty || change.Property == MaxDateProperty || change.Property == DisabledDateProperty ||
            change.Property == FirstDayOfWeekProperty || change.Property == ShowWeekProperty)
        {
            _session?.UpdateInput();
        }
    }

    /// <summary>
    /// Calls the base member, then adopts the formatting culture of the language manager, subscribes to
    /// its language changes and refreshes the panels.
    /// An override must call this member for the panels to follow language changes.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _languageManager = Application.Current is { } application ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application) : null;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += OnLanguageChanged;
            _culture = _languageManager.Current.FormattingCulture;
        }
        _session.UpdateInput();
    }

    /// <summary>
    /// Unsubscribes from the language manager, then calls the base member.
    /// An override must call this member to release the language subscription.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged -= OnLanguageChanged;
        }

        _languageManager = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        _culture = _languageManager?.Current.FormattingCulture ?? CultureInfo.CurrentCulture;
        _session.UpdateInput();
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        PseudoClasses.Set(":single-panel", !HasSecondaryPanel);
        RaisePropertyChanged(HasSecondaryPanelProperty, _lastSecondaryPanel, HasSecondaryPanel);
        _lastSecondaryPanel = HasSecondaryPanel;
        var focus = FocusedValue;
        var hover = HoveredValue;
        RaisePropertyChanged(FocusedValueProperty, _lastNotifiedFocus, focus);
        RaisePropertyChanged(HoveredValueProperty, _lastNotifiedHover, hover);
        _lastNotifiedFocus = focus;
        _lastNotifiedHover = hover;
        RefreshHeaderContext();
    }

    private void RefreshHeaderContext()
    {
        var input = ((IDatePanelHost)this).ReadInput();
        if (_headerInput is { } previous && previous.DisplayDate == input.DisplayDate &&
            previous.PanelKind == input.PanelKind && previous.SelectionUnit == input.SelectionUnit &&
            previous.PanelCount == input.PanelCount &&
            Equals(previous.Culture, input.Culture))
        {
            return;
        }

        _headerInput = input;
        var old = _headerContext;
        var oldSecondary = _secondaryHeaderContext;
        _headerContext = DateViewerHeaderContext.Create(input, _session.Apply);
        var secondaryInput = input with { DisplayDate = DatePanelAlgorithms.Navigate(input.DisplayDate, input.PanelKind, 1) };
        _secondaryHeaderContext = DateViewerHeaderContext.Create(secondaryInput, _session.Apply, 1);
        RaisePropertyChanged(HeaderContextProperty, old, _headerContext);
        RaisePropertyChanged(SecondaryHeaderContextProperty, oldSecondary, _secondaryHeaderContext);
    }

    internal void SetHost(IDatePanelHost? host)
    {
        if (ReferenceEquals(_host, host))
        {
            return;
        }

        var oldKind = PanelKind;
        _host = host;
        _session.ResetInteraction();
        RaisePropertyChanged(PanelKindProperty, oldKind, PanelKind);
    }

    internal void RefreshHost()
    {
        var oldKind = _session.Input.PanelKind;
        _session.UpdateInput();
        RaisePropertyChanged(PanelKindProperty, oldKind, PanelKind);
    }

    DatePanelInput IDatePanelHost.ReadInput() => _host?.ReadInput() ?? new()
    {
        DisplayDate = DisplayDate, PanelKind = _panelKind, SelectionUnit = SelectionUnit,
        Range = Value, ActiveRangePart = _activePart, PanelCount = 2, IsRangeSelection = true,
        Today = DateTime.Today, Culture = _culture, FirstDayOfWeek = FirstDayOfWeek, ShowWeek = ShowWeek,
        MinDate = MinDate, MaxDate = MaxDate, DisabledDate = DisabledDate
    };

    void IDatePanelHost.NavigateTo(DateTime displayDate, DateViewerPanelKind panelKind)
    {
        if (_host is { } host)
        {
            host.NavigateTo(displayDate, panelKind);
            return;
        }
        var old = _panelKind;
        var oldAnchor = DatePanelAlgorithms.GetPanelAnchor(DisplayDate, old);
        _panelKind = panelKind;
        SetCurrentValue(DisplayDateProperty, displayDate);
        RaisePropertyChanged(PanelKindProperty, old, _panelKind);
        _session.UpdateInput();
        if (old != _panelKind || oldAnchor != DatePanelAlgorithms.GetPanelAnchor(DisplayDate, _panelKind))
        {
            PanelChanged?.Invoke(this, new DateViewerPanelChangedEventArgs(DisplayDate, _panelKind));
        }
    }

    void IDatePanelHost.Activate(DateCellSelection selection)
    {
        if (_host is { } host)
        {
            host.Activate(selection);
            return;
        }
        var target = DateViewer.TargetPanel(SelectionUnit);
        var cellPanel = selection.Kind switch
        {
            DateViewerCellType.Month => DateViewerPanelKind.Month,
            DateViewerCellType.Quarter => DateViewerPanelKind.Quarter,
            DateViewerCellType.Year => DateViewerPanelKind.Year,
            _ => DateViewerPanelKind.Date
        };
        if (cellPanel != target)
        {
            ((IDatePanelHost)this).NavigateTo(selection.Value, cellPanel == DateViewerPanelKind.Year && target == DateViewerPanelKind.Date ? DateViewerPanelKind.Month : target);
            return;
        }
        var old = Value;
        var firstDay = FirstDayOfWeek ?? DatePanelAlgorithms.GetWeekFirstDay(_culture);
        var value = DatePanelAlgorithms.Normalize(selection.Value, SelectionUnit, firstDay);
        DateViewerRange result;
        if (_activePart == DateRangeActivePart.Start)
        {
            result = new DateViewerRange(value, null);
            _activePart = DateRangeActivePart.End;
        }
        else
        {
            var start = old?.Start ?? value;
            result = start <= value ? new DateViewerRange(start, value) : new DateViewerRange(value, start);
            _activePart = DateRangeActivePart.Start;
        }
        SetCurrentValue(ValueProperty, result);
        _session.UpdateInput();
        if (old != result)
        {
            ValueChanged?.Invoke(this, new RangeDateViewerValueChangedEventArgs(old, result));
        }

        Selected?.Invoke(this, new RangeDateViewerSelectedEventArgs(result, value, SelectionUnit));
    }

    void IDatePanelHost.Preview(DateTime? value)
    {
        if (_host is { } host)
        {
            host.Preview(value);
        }
        else
        {
            HoveredValueChanged?.Invoke(this, new DateViewerHoveredValueChangedEventArgs(value));
        }
    }
}
