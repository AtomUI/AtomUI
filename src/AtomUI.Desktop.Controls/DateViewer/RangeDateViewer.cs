using System.Globalization;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

public partial class RangeDateViewer : TemplatedControl, IDatePanelHost
{
    #region 公共属性定义
    public static readonly StyledProperty<DateViewerRange?> ValueProperty = AvaloniaProperty.Register<RangeDateViewer, DateViewerRange?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    public static readonly StyledProperty<DateTime> DisplayDateProperty = DateViewer.DisplayDateProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<DateViewerSelectionUnit> SelectionUnitProperty = DateViewer.SelectionUnitProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<DateViewerPresentation> PresentationProperty = DateViewer.PresentationProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<DateTime?> MinDateProperty = DateViewer.MinDateProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<DateTime?> MaxDateProperty = DateViewer.MaxDateProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<Func<DateTime, bool>?> DisabledDateProperty = DateViewer.DisabledDateProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<DayOfWeek?> FirstDayOfWeekProperty = DateViewer.FirstDayOfWeekProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<bool> ShowWeekProperty = DateViewer.ShowWeekProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<bool> ShowHeaderProperty = DateViewer.ShowHeaderProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty = DateViewer.HeaderTemplateProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty = DateViewer.CellTemplateProperty.AddOwner<RangeDateViewer>();
    public static readonly StyledProperty<IDataTemplate?> FullCellTemplateProperty = DateViewer.FullCellTemplateProperty.AddOwner<RangeDateViewer>();
    public static readonly DirectProperty<RangeDateViewer, DateViewerPanelKind> PanelKindProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DateViewerPanelKind>(nameof(PanelKind), control => control.PanelKind);
    public static readonly DirectProperty<RangeDateViewer, DateTime?> FocusedValueProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DateTime?>(nameof(FocusedValue), control => control.FocusedValue);
    public static readonly DirectProperty<RangeDateViewer, DateTime?> HoveredValueProperty = AvaloniaProperty.RegisterDirect<RangeDateViewer, DateTime?>(nameof(HoveredValue), control => control.HoveredValue);
    public DateViewerRange? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
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
    public event EventHandler<RangeDateViewerValueChangedEventArgs>? ValueChanged;
    public event EventHandler<RangeDateViewerSelectedEventArgs>? Selected;
    public event EventHandler<DateViewerPanelChangedEventArgs>? PanelChanged;
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

    internal void SetContentFactory(Func<DatePanelSession, DateViewerCellModel, DateViewerCellContext?>? factory) =>
        _session.SetContentFactory(factory);

    internal void SetAutomationNameFactory(Func<DatePanelSession, DateViewerCellModel, string?>? factory) =>
        _session.SetAutomationNameFactory(factory);

    internal void RefreshContent() => _session.RefreshContent();

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
