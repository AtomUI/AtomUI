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

public partial class DateViewer : TemplatedControl, IDatePanelHost
{
    #region 公共属性定义
    public static readonly StyledProperty<DateTime?> ValueProperty = AvaloniaProperty.Register<DateViewer, DateTime?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    public static readonly StyledProperty<DateTime> DisplayDateProperty = AvaloniaProperty.Register<DateViewer, DateTime>(nameof(DisplayDate), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<DateViewerSelectionUnit> SelectionUnitProperty = AvaloniaProperty.Register<DateViewer, DateViewerSelectionUnit>(nameof(SelectionUnit));
    public static readonly StyledProperty<DateViewerPresentation> PresentationProperty = AvaloniaProperty.Register<DateViewer, DateViewerPresentation>(nameof(Presentation));
    public static readonly StyledProperty<DateTime?> MinDateProperty = AvaloniaProperty.Register<DateViewer, DateTime?>(nameof(MinDate));
    public static readonly StyledProperty<DateTime?> MaxDateProperty = AvaloniaProperty.Register<DateViewer, DateTime?>(nameof(MaxDate));
    public static readonly StyledProperty<Func<DateTime, bool>?> DisabledDateProperty = AvaloniaProperty.Register<DateViewer, Func<DateTime, bool>?>(nameof(DisabledDate));
    public static readonly StyledProperty<DayOfWeek?> FirstDayOfWeekProperty = AvaloniaProperty.Register<DateViewer, DayOfWeek?>(nameof(FirstDayOfWeek));
    public static readonly StyledProperty<bool> ShowWeekProperty = AvaloniaProperty.Register<DateViewer, bool>(nameof(ShowWeek));
    public static readonly StyledProperty<bool> ShowHeaderProperty = AvaloniaProperty.Register<DateViewer, bool>(nameof(ShowHeader), true);
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty = AvaloniaProperty.Register<DateViewer, IDataTemplate?>(nameof(HeaderTemplate));
    public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty = AvaloniaProperty.Register<DateViewer, IDataTemplate?>(nameof(CellTemplate));
    public static readonly StyledProperty<IDataTemplate?> FullCellTemplateProperty = AvaloniaProperty.Register<DateViewer, IDataTemplate?>(nameof(FullCellTemplate));
    public static readonly DirectProperty<DateViewer, DateViewerPanelKind> PanelKindProperty = AvaloniaProperty.RegisterDirect<DateViewer, DateViewerPanelKind>(nameof(PanelKind), control => control.PanelKind);
    public static readonly DirectProperty<DateViewer, DateTime?> FocusedValueProperty = AvaloniaProperty.RegisterDirect<DateViewer, DateTime?>(nameof(FocusedValue), control => control.FocusedValue);
    public static readonly DirectProperty<DateViewer, DateTime?> HoveredValueProperty = AvaloniaProperty.RegisterDirect<DateViewer, DateTime?>(nameof(HoveredValue), control => control.HoveredValue);

    public DateTime? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public DateTime DisplayDate { get => GetValue(DisplayDateProperty); set => SetValue(DisplayDateProperty, value); }
    public DateViewerSelectionUnit SelectionUnit { get => GetValue(SelectionUnitProperty); set => SetValue(SelectionUnitProperty, value); }
    public DateViewerPresentation Presentation { get => GetValue(PresentationProperty); set => SetValue(PresentationProperty, value); }
    public DateTime? MinDate { get => GetValue(MinDateProperty); set => SetValue(MinDateProperty, value); }
    public DateTime? MaxDate { get => GetValue(MaxDateProperty); set => SetValue(MaxDateProperty, value); }
    public Func<DateTime, bool>? DisabledDate { get => GetValue(DisabledDateProperty); set => SetValue(DisabledDateProperty, value); }
    public DayOfWeek? FirstDayOfWeek { get => GetValue(FirstDayOfWeekProperty); set => SetValue(FirstDayOfWeekProperty, value); }
    public bool ShowWeek { get => GetValue(ShowWeekProperty); set => SetValue(ShowWeekProperty, value); }
    public bool ShowHeader { get => GetValue(ShowHeaderProperty); set => SetValue(ShowHeaderProperty, value); }
    public IDataTemplate? HeaderTemplate { get => GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }
    public IDataTemplate? CellTemplate { get => GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
    public IDataTemplate? FullCellTemplate { get => GetValue(FullCellTemplateProperty); set => SetValue(FullCellTemplateProperty, value); }
    public DateViewerPanelKind PanelKind => _host?.ReadInput().PanelKind ?? _panelKind;
    public DateTime? FocusedValue => _session?.FocusedValue;
    public DateTime? HoveredValue => _session?.HoveredValue;
    #endregion

    #region 公共事件定义
    public event EventHandler<DateViewerValueChangedEventArgs>? ValueChanged;
    public event EventHandler<DateViewerSelectedEventArgs>? Selected;
    public event EventHandler<DateViewerPanelChangedEventArgs>? PanelChanged;
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
