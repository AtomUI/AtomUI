using AtomUI.Controls;
using System.Globalization;
using System.Windows.Input;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A single calendar panel that shows a grid of days, months, quarters or years and stores the pick in
/// <see cref="Value" />: a date, a week, a month, a quarter or a year, as <see cref="SelectionUnit" /> selects.
/// When <see cref="Calendar" /> embeds the panel, navigation and picks are forwarded to it and the footer with
/// the Today button is hidden.
/// </summary>
public partial class DateViewer : TemplatedControl, IDatePanelHost
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> ValueProperty = AvaloniaProperty.Register<DateViewer, DateTime?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    /// <summary>
    /// Identifies the <see cref="DisplayDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime> DisplayDateProperty = AvaloniaProperty.Register<DateViewer, DateTime>(nameof(DisplayDate), defaultBindingMode: BindingMode.TwoWay);
    /// <summary>
    /// Identifies the <see cref="SelectionUnit" /> property.
    /// </summary>
    public static readonly StyledProperty<DateViewerSelectionUnit> SelectionUnitProperty = AvaloniaProperty.Register<DateViewer, DateViewerSelectionUnit>(nameof(SelectionUnit));
    /// <summary>
    /// Identifies the <see cref="Presentation" /> property.
    /// </summary>
    public static readonly StyledProperty<DateViewerPresentation> PresentationProperty = AvaloniaProperty.Register<DateViewer, DateViewerPresentation>(nameof(Presentation));
    /// <summary>
    /// Identifies the <see cref="MinDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty = AvaloniaProperty.Register<DateViewer, DateTime?>(nameof(MinDate));
    /// <summary>
    /// Identifies the <see cref="MaxDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty = AvaloniaProperty.Register<DateViewer, DateTime?>(nameof(MaxDate));
    /// <summary>
    /// Identifies the <see cref="DisabledDate" /> property.
    /// </summary>
    public static readonly StyledProperty<Func<DateTime, bool>?> DisabledDateProperty = AvaloniaProperty.Register<DateViewer, Func<DateTime, bool>?>(nameof(DisabledDate));
    /// <summary>
    /// Identifies the <see cref="FirstDayOfWeek" /> property.
    /// </summary>
    public static readonly StyledProperty<DayOfWeek?> FirstDayOfWeekProperty = AvaloniaProperty.Register<DateViewer, DayOfWeek?>(nameof(FirstDayOfWeek));
    /// <summary>
    /// Identifies the <see cref="ShowWeek" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowWeekProperty = AvaloniaProperty.Register<DateViewer, bool>(nameof(ShowWeek));
    /// <summary>
    /// Identifies the <see cref="ShowHeader" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowHeaderProperty = AvaloniaProperty.Register<DateViewer, bool>(nameof(ShowHeader), true);
    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty = AvaloniaProperty.Register<DateViewer, IDataTemplate?>(nameof(HeaderTemplate));
    /// <summary>
    /// Identifies the <see cref="CellTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty = AvaloniaProperty.Register<DateViewer, IDataTemplate?>(nameof(CellTemplate));
    /// <summary>
    /// Identifies the <see cref="FullCellTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FullCellTemplateProperty = AvaloniaProperty.Register<DateViewer, IDataTemplate?>(nameof(FullCellTemplate));
    /// <summary>
    /// Identifies the <see cref="PanelKind" /> property.
    /// </summary>
    public static readonly DirectProperty<DateViewer, DateViewerPanelKind> PanelKindProperty = AvaloniaProperty.RegisterDirect<DateViewer, DateViewerPanelKind>(nameof(PanelKind), control => control.PanelKind);
    /// <summary>
    /// Identifies the <see cref="FocusedValue" /> property.
    /// </summary>
    public static readonly DirectProperty<DateViewer, DateTime?> FocusedValueProperty = AvaloniaProperty.RegisterDirect<DateViewer, DateTime?>(nameof(FocusedValue), control => control.FocusedValue);
    /// <summary>
    /// Identifies the <see cref="HoveredValue" /> property.
    /// </summary>
    public static readonly DirectProperty<DateViewer, DateTime?> HoveredValueProperty = AvaloniaProperty.RegisterDirect<DateViewer, DateTime?>(nameof(HoveredValue), control => control.HoveredValue);

    /// <summary>
    /// The selected date, normalized for <see cref="SelectionUnit" />, or <see langword="null" /> when nothing
    /// is selected, which is the default. A pick in the panel writes it. Bindings to it default to
    /// <see cref="BindingMode.TwoWay" /> with data validation enabled.
    /// </summary>
    public DateTime? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    /// <summary>
    /// The date that anchors the shown panel: a day of the shown month on a date panel, of the shown year on a
    /// month or quarter panel, and of the shown decade on a year panel. Paging, drilling into a cell and a pick
    /// move it. It starts at today, and bindings to it default to <see cref="BindingMode.TwoWay" />.
    /// </summary>
    public DateTime DisplayDate { get => GetValue(DisplayDateProperty); set => SetValue(DisplayDateProperty, value); }
    /// <summary>
    /// What a pick selects: a date, a week, a month, a quarter or a year. It also decides the panel on which a pick
    /// selects: <see cref="DateViewerSelectionUnit.Date" /> and <see cref="DateViewerSelectionUnit.Week" /> select
    /// on the date panel, and each other unit selects on the panel of its own kind. A change of the unit resets
    /// <see cref="PanelKind" /> to that panel. The default is <see cref="DateViewerSelectionUnit.Date" />, the only
    /// unit that shows the default footer; its Today button is enabled only while today is not excluded by
    /// <see cref="MinDate" />, <see cref="MaxDate" /> or <see cref="DisabledDate" />.
    /// </summary>
    public DateViewerSelectionUnit SelectionUnit { get => GetValue(SelectionUnitProperty); set => SetValue(SelectionUnitProperty, value); }
    /// <summary>
    /// How the cells of the panel are laid out. <see cref="DateViewerPresentation.Compact" />, the default, keeps
    /// fixed-size cells in a panel of fixed width, as in a calendar. <see cref="DateViewerPresentation.Content" />
    /// leaves room for cell content and lets the panel stretch to the width it is given.
    /// </summary>
    public DateViewerPresentation Presentation { get => GetValue(PresentationProperty); set => SetValue(PresentationProperty, value); }
    /// <summary>
    /// The earliest date a cell may show; the boundary date itself is not disabled. Earlier cells are disabled, and
    /// the panel cannot be navigated to a period before the one that contains it. <see langword="null" />, the
    /// default, leaves the range open at this side.
    /// </summary>
    public DateTime? MinDate { get => GetValue(MinDateProperty); set => SetValue(MinDateProperty, value); }
    /// <summary>
    /// The latest date a cell may show; the boundary date itself is not disabled. Later cells are disabled, and the
    /// panel cannot be navigated to a period after the one that contains it. <see langword="null" />, the default,
    /// leaves the range open at this side.
    /// </summary>
    public DateTime? MaxDate { get => GetValue(MaxDateProperty); set => SetValue(MaxDateProperty, value); }
    /// <summary>
    /// A predicate that receives the date of a cell and returns <see langword="true" /> to disable the cell. It
    /// applies in addition to <see cref="MinDate" /> and <see cref="MaxDate" />. <see langword="null" />, the
    /// default, disables nothing.
    /// </summary>
    public Func<DateTime, bool>? DisabledDate { get => GetValue(DisabledDateProperty); set => SetValue(DisabledDateProperty, value); }
    /// <summary>
    /// The weekday that starts each week row of the date panel. <see langword="null" />, the default, uses the first
    /// day of the week of the current culture.
    /// </summary>
    public DayOfWeek? FirstDayOfWeek { get => GetValue(FirstDayOfWeekProperty); set => SetValue(FirstDayOfWeekProperty, value); }
    /// <summary>
    /// Shows a column of week numbers in front of the date panel. The default is <see langword="false" />.
    /// </summary>
    public bool ShowWeek { get => GetValue(ShowWeekProperty); set => SetValue(ShowWeekProperty, value); }
    /// <summary>
    /// Shows the header above the panel, whether it shows the default navigation or the content of
    /// <see cref="HeaderTemplate" />. The default is <see langword="true" />.
    /// </summary>
    public bool ShowHeader { get => GetValue(ShowHeaderProperty); set => SetValue(ShowHeaderProperty, value); }
    /// <summary>
    /// A template for custom header content. It replaces the default navigation of the panel while it is set. Its
    /// data context is a <see cref="DateViewerHeaderContext" />, which exposes the state and the navigation of the
    /// panel. <see langword="null" />, the default, shows the default navigation.
    /// </summary>
    public IDataTemplate? HeaderTemplate { get => GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }
    /// <summary>
    /// A template for extra content shown below the date in every cell. Its data context is the
    /// <c>DateViewerCellContext</c> of the cell, which exposes the cell state. <see langword="null" />, the
    /// default, shows the date alone.
    /// </summary>
    public IDataTemplate? CellTemplate { get => GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
    /// <summary>
    /// A template that replaces the whole content of every cell. It takes precedence over
    /// <see cref="CellTemplate" />. <see langword="null" />, the default, shows the default cell: the date and,
    /// when <see cref="CellTemplate" /> is set, its content.
    /// </summary>
    public IDataTemplate? FullCellTemplate { get => GetValue(FullCellTemplateProperty); set => SetValue(FullCellTemplateProperty, value); }
    /// <summary>
    /// The kind of grid the panel shows, as a <see cref="DateViewerPanelKind" /> value. Navigation inside the panel
    /// changes it, and a change of <see cref="SelectionUnit" /> resets it to the panel on which picks select.
    /// </summary>
    public DateViewerPanelKind PanelKind => _host?.ReadInput().PanelKind ?? _panelKind;
    /// <summary>
    /// The date of the cell that keyboard navigation is positioned on. It follows keyboard navigation and starts
    /// at <see cref="Value" />, or at <see cref="DisplayDate" /> when nothing is selected.
    /// </summary>
    public DateTime? FocusedValue => _session?.FocusedValue;
    /// <summary>
    /// The date of the cell under the pointer, or <see langword="null" /> while no cell is hovered.
    /// </summary>
    public DateTime? HoveredValue => _session?.HoveredValue;
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Raised after a pick changed <see cref="Value" />: the new value is committed. A pick that results in the
    /// same value, and an assignment to <see cref="Value" />, do not raise it.
    /// </summary>
    public event EventHandler<DateViewerValueChangedEventArgs>? ValueChanged;
    /// <summary>
    /// Raised after a pick that produces a selection, also when the value stays the same.
    /// <see cref="ValueChanged" /> is raised first when the value did change.
    /// </summary>
    public event EventHandler<DateViewerSelectedEventArgs>? Selected;
    /// <summary>
    /// Raised after the panel moved to another period or another kind, once <see cref="DisplayDate" /> and
    /// <see cref="PanelKind" /> are updated. Paging and picks that move the panel do so; a change of
    /// <see cref="SelectionUnit" /> that resets the kind does not raise it.
    /// </summary>
    public event EventHandler<DateViewerPanelChangedEventArgs>? PanelChanged;
    /// <summary>
    /// Raised when the date under the pointer changes. The event argument carries the new date, or
    /// <see langword="null" /> when no cell is hovered.
    /// </summary>
    public event EventHandler<DateViewerHoveredValueChangedEventArgs>? HoveredValueChanged;
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<Avalonia.Styling.ControlTheme?> PanelThemeProperty = AvaloniaProperty.Register<DateViewer, Avalonia.Styling.ControlTheme?>(nameof(PanelTheme));
    internal Avalonia.Styling.ControlTheme? PanelTheme { get => GetValue(PanelThemeProperty); set => SetValue(PanelThemeProperty, value); }
    internal static readonly StyledProperty<IDataTemplate?> DefaultFullCellTemplateProperty = AvaloniaProperty.Register<DateViewer, IDataTemplate?>(nameof(DefaultFullCellTemplate));
    internal IDataTemplate? DefaultFullCellTemplate { get => GetValue(DefaultFullCellTemplateProperty); set => SetValue(DefaultFullCellTemplateProperty, value); }
    internal static readonly StyledProperty<Avalonia.Styling.ControlTheme?> CellThemeProperty = AvaloniaProperty.Register<DateViewer, Avalonia.Styling.ControlTheme?>(nameof(CellTheme));
    internal Avalonia.Styling.ControlTheme? CellTheme { get => GetValue(CellThemeProperty); set => SetValue(CellThemeProperty, value); }
    internal static readonly DirectProperty<DateViewer, DatePanelSession> PanelSessionProperty = AvaloniaProperty.RegisterDirect<DateViewer, DatePanelSession>(nameof(PanelSession), control => control.PanelSession);
    internal DatePanelSession PanelSession => _session;
    internal static readonly DirectProperty<DateViewer, DateViewerHeaderContext?> HeaderContextProperty = AvaloniaProperty.RegisterDirect<DateViewer, DateViewerHeaderContext?>(nameof(HeaderContext), control => control.HeaderContext);
    internal DateViewerHeaderContext? HeaderContext => _headerContext;
    internal static readonly DirectProperty<DateViewer, bool> IsDefaultFooterVisibleProperty = AvaloniaProperty.RegisterDirect<DateViewer, bool>(nameof(IsDefaultFooterVisible), control => control.IsDefaultFooterVisible);
    internal bool IsDefaultFooterVisible => _host is null && SelectionUnit == DateViewerSelectionUnit.Date;
    internal static readonly DirectProperty<DateViewer, ICommand> TodayCommandProperty = AvaloniaProperty.RegisterDirect<DateViewer, ICommand>(nameof(TodayCommand), control => control.TodayCommand);
    internal ICommand TodayCommand => _todayCommand;
    #endregion

    private readonly DatePanelSession _session;
    private IDatePanelHost? _host;
    private DatePanel? _geometryPanel;
    private IReadOnlyList<DateCellLayout> _cellLayouts = Array.Empty<DateCellLayout>();
    internal IReadOnlyList<DateCellLayout> CellLayouts => _cellLayouts;
    internal event EventHandler? CellLayoutChanged;
    private DatePanelInput? _headerInput;
    private DateViewerHeaderContext? _headerContext;
    private readonly DateViewerCommand _todayCommand;
    private DateViewerPanelKind _panelKind;
    private CultureInfo _culture = CultureInfo.CurrentCulture;
    private ILanguageManager? _languageManager;
    private DateTime? _lastNotifiedFocus;
    private DateTime? _lastNotifiedHover;

    public DateViewer()
    {
        _todayCommand = new DateViewerCommand(_ => SelectToday(), _ => CanSelectToday());
        SetCurrentValue(DisplayDateProperty, DateTime.Today);
        _session = new DatePanelSession(this);
        _session.Changed += OnSessionChanged;
        RefreshHeaderContext();
    }

    /// <summary>
    /// Called when a styled property changes. A change of <see cref="SelectionUnit" /> resets
    /// <see cref="PanelKind" /> and the visibility of the default footer; a change of a property that feeds the
    /// panel refreshes the panel input; a change of <see cref="SelectionUnit" />, <see cref="MinDate" />,
    /// <see cref="MaxDate" /> or <see cref="DisabledDate" /> updates the enabled state of the Today button.
    /// Call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectionUnitProperty)
        {
            RaisePropertyChanged(IsDefaultFooterVisibleProperty,
                _host is null && change.GetOldValue<DateViewerSelectionUnit>() == DateViewerSelectionUnit.Date,
                IsDefaultFooterVisible);
            var old = _panelKind;
            _panelKind = TargetPanel(SelectionUnit);
            RaisePropertyChanged(PanelKindProperty, old, _panelKind);
        }
        if (change.Property == ValueProperty || change.Property == DisplayDateProperty || change.Property == SelectionUnitProperty ||
            change.Property == MinDateProperty || change.Property == MaxDateProperty || change.Property == DisabledDateProperty ||
            change.Property == FirstDayOfWeekProperty || change.Property == ShowWeekProperty)
        {
            _session?.UpdateInput();
        }

        if (change.Property == SelectionUnitProperty || change.Property == MinDateProperty ||
            change.Property == MaxDateProperty || change.Property == DisabledDateProperty)
        {
            _todayCommand?.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Called when the control is attached to a visual tree. It subscribes to the geometry of the template panel,
    /// to language changes, takes the formatting culture of the language manager and refreshes the panel input.
    /// Call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribePanelGeometry();
        _languageManager = Application.Current is { } application ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application) : null;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += OnLanguageChanged;
            _culture = _languageManager.Current.FormattingCulture;
        }
        _session.UpdateInput();
    }

    /// <summary>
    /// Called when the control is detached from a visual tree. It unsubscribes from the panel geometry and from
    /// language changes and drops the language manager. Call the base member last.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_geometryPanel is not null)
        {
            _geometryPanel.GeometryChanged -= OnPanelGeometryChanged;
        }

        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged -= OnLanguageChanged;
        }

        _languageManager = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Called when the control template is applied. It releases the previous template panel, locates
    /// <c>PART_PrimaryPanel</c> and publishes the cell geometry relative to this control. Call the base member
    /// before the parts are located.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_geometryPanel is not null)
        {
            _geometryPanel.GeometryChanged -= OnPanelGeometryChanged;
        }

        base.OnApplyTemplate(e);
        _geometryPanel = e.NameScope.Find<DatePanel>("PART_PrimaryPanel");
        SubscribePanelGeometry();
        PublishCellGeometry();
    }

    private void SubscribePanelGeometry()
    {
        if (_geometryPanel is null)
        {
            return;
        }

        _geometryPanel.GeometryChanged -= OnPanelGeometryChanged;
        _geometryPanel.GeometryChanged += OnPanelGeometryChanged;
    }

    private void OnPanelGeometryChanged(object? sender, EventArgs e) => PublishCellGeometry();

    /// <summary>
    /// Arranges the content and then publishes the cell bounds relative to this control. Call the base member
    /// first and return its size; the bounds are final only after the arrangement.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        PublishCellGeometry();
        return result;
    }

    private void PublishCellGeometry()
    {
        var transform = _geometryPanel?.TransformToVisual(this) ?? Matrix.Identity;
        var layouts = _geometryPanel?.GetCellLayouts().Select(cell => cell with { Bounds = cell.Bounds.TransformToAABB(transform) }).ToArray()
            ?? Array.Empty<DateCellLayout>();
        if (_cellLayouts.SequenceEqual(layouts))
        {
            return;
        }

        _cellLayouts = layouts;
        CellLayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        _culture = _languageManager?.Current.FormattingCulture ?? CultureInfo.CurrentCulture;
        _session.UpdateInput();
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        var focused = FocusedValue;
        var hovered = HoveredValue;
        RaisePropertyChanged(FocusedValueProperty, _lastNotifiedFocus, focused);
        RaisePropertyChanged(HoveredValueProperty, _lastNotifiedHover, hovered);
        _lastNotifiedFocus = focused;
        _lastNotifiedHover = hovered;
        RefreshHeaderContext();
    }

    internal static DateViewerPanelKind TargetPanel(DateViewerSelectionUnit unit) => unit switch
    {
        DateViewerSelectionUnit.Date or DateViewerSelectionUnit.Week => DateViewerPanelKind.Date,
        DateViewerSelectionUnit.Month => DateViewerPanelKind.Month,
        DateViewerSelectionUnit.Quarter => DateViewerPanelKind.Quarter,
        DateViewerSelectionUnit.Year => DateViewerPanelKind.Year,
        _ => throw new ArgumentOutOfRangeException(nameof(unit))
    };

    private void RefreshHeaderContext()
    {
        var input = ((IDatePanelHost)this).ReadInput();
        if (_headerInput is { } previous && previous.DisplayDate == input.DisplayDate &&
            previous.PanelKind == input.PanelKind && previous.SelectionUnit == input.SelectionUnit &&
            Equals(previous.Culture, input.Culture))
        {
            return;
        }

        _headerInput = input;
        var old = _headerContext;
        _headerContext = DateViewerHeaderContext.Create(input, _session.Apply);
        RaisePropertyChanged(HeaderContextProperty, old, _headerContext);
    }

    internal void SetHost(IDatePanelHost? host)
    {
        if (ReferenceEquals(_host, host))
        {
            return;
        }

        var oldKind = PanelKind;
        var oldFooterVisible = IsDefaultFooterVisible;
        _host = host;
        RaisePropertyChanged(IsDefaultFooterVisibleProperty, oldFooterVisible, IsDefaultFooterVisible);
        _todayCommand.RaiseCanExecuteChanged();
        _session.ResetInteraction();
        RaisePropertyChanged(PanelKindProperty, oldKind, PanelKind);
    }

    internal void RefreshHost()
    {
        var oldKind = _session.Input.PanelKind;
        _session.UpdateInput();
        RaisePropertyChanged(PanelKindProperty, oldKind, PanelKind);
    }

    internal void SetContentFactory(Func<DatePanelSession, DateViewerCellModel, DateViewerCellContext?>? factory) =>
        _session.SetContentFactory(factory);

    internal void SetAutomationNameFactory(Func<DatePanelSession, DateViewerCellModel, string?>? factory) =>
        _session.SetAutomationNameFactory(factory);

    internal void RefreshContent() => _session.RefreshContent();

    private bool CanSelectToday()
    {
        var today = DateTime.Today;
        return IsDefaultFooterVisible &&
               (MinDate is null || today >= MinDate.Value.Date) &&
               (MaxDate is null || today <= MaxDate.Value.Date) &&
               DisabledDate?.Invoke(today) != true;
    }

    private void SelectToday()
    {
        if (!CanSelectToday())
        {
            return;
        }

        var today = DateTime.Today;
        ((IDatePanelHost)this).NavigateTo(today, DateViewerPanelKind.Date);
        _session.Apply(new DatePanelAction.Activate(today, DateViewerCellType.Date));
    }

    DatePanelInput IDatePanelHost.ReadInput() => _host?.ReadInput() ?? new()
    {
        DisplayDate = DisplayDate, PanelKind = _panelKind, SelectionUnit = SelectionUnit, SelectedDate = Value,
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
        var oldAnchor = DatePanelAlgorithms.GetPanelAnchor(DisplayDate, _panelKind);
        var oldKind = _panelKind;
        _panelKind = panelKind;
        SetCurrentValue(DisplayDateProperty, displayDate);
        RaisePropertyChanged(PanelKindProperty, oldKind, _panelKind);
        _session.UpdateInput();
        if (oldKind != _panelKind || oldAnchor != DatePanelAlgorithms.GetPanelAnchor(DisplayDate, _panelKind))
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
        var target = TargetPanel(SelectionUnit);
        var cellPanel = selection.Kind switch
        {
            DateViewerCellType.Month => DateViewerPanelKind.Month,
            DateViewerCellType.Quarter => DateViewerPanelKind.Quarter,
            DateViewerCellType.Year => DateViewerPanelKind.Year,
            _ => DateViewerPanelKind.Date
        };
        if (cellPanel != target)
        {
            var nextPanel = cellPanel == DateViewerPanelKind.Year && target == DateViewerPanelKind.Date
                ? DateViewerPanelKind.Month : target;
            ((IDatePanelHost)this).NavigateTo(selection.Value, nextPanel);
            return;
        }
        var old = Value;
        var oldAnchor = DatePanelAlgorithms.GetPanelAnchor(DisplayDate, _panelKind);
        var firstDay = FirstDayOfWeek ?? DatePanelAlgorithms.GetWeekFirstDay(_culture);
        var value = DatePanelAlgorithms.Normalize(selection.Value, SelectionUnit, firstDay);
        SetCurrentValue(ValueProperty, value);
        SetCurrentValue(DisplayDateProperty, selection.Value);
        _session.UpdateInput();
        if (oldAnchor != DatePanelAlgorithms.GetPanelAnchor(DisplayDate, _panelKind))
        {
            PanelChanged?.Invoke(this, new DateViewerPanelChangedEventArgs(DisplayDate, _panelKind));
        }

        if (old != value)
        {
            ValueChanged?.Invoke(this, new DateViewerValueChangedEventArgs(old, value));
        }

        Selected?.Invoke(this, new DateViewerSelectedEventArgs(value, SelectionUnit));
    }

    void IDatePanelHost.Preview(DateTime? hoveredValue)
    {
        if (_host is { } host)
        {
            host.Preview(hoveredValue);
        }
        else
        {
            HoveredValueChanged?.Invoke(this, new DateViewerHoveredValueChangedEventArgs(hoveredValue));
        }
    }
}
