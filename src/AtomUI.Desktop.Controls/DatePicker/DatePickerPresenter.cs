using System.Globalization;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public sealed class ChoosingStatusEventArgs(bool isChoosing) : EventArgs
{
    public bool IsChoosing { get; } = isChoosing;
}

public sealed class DateSelectedEventArgs(DateTime? value) : EventArgs
{
    public DateTime? Date { get; } = value;
}

internal class DatePickerPresenter : PickerPresenterBase, IDatePanelHost
{
    public static readonly StyledProperty<bool> IsNeedConfirmProperty = DatePicker.IsNeedConfirmProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<bool> IsShowNowProperty = DatePicker.IsShowNowProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<bool> IsShowTimeProperty = DatePicker.IsShowTimeProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<DatePickerMode> PickerModeProperty = DatePicker.PickerModeProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty = DatePicker.SelectedDateTimeProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<DateTime?> PickerDisplayDateProperty = DatePicker.PickerDisplayDateProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<DateTime?> MinDateProperty = DatePicker.MinDateProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<DateTime?> MaxDateProperty = DatePicker.MaxDateProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty = TimePicker.ClockIdentifierProperty.AddOwner<DatePickerPresenter>();
    public static readonly StyledProperty<TimeSpan?> TempSelectedTimeProperty =
        AvaloniaProperty.Register<DatePickerPresenter, TimeSpan?>(nameof(TempSelectedTime));
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DatePickerPresenter>();
    internal static readonly DirectProperty<DatePickerPresenter, bool> IsButtonsPanelVisibleProperty =
        AvaloniaProperty.RegisterDirect<DatePickerPresenter, bool>(nameof(IsButtonsPanelVisible), o => o.IsButtonsPanelVisible);
    internal static readonly DirectProperty<DatePickerPresenter, bool> IsTimeSelectionVisibleProperty =
        AvaloniaProperty.RegisterDirect<DatePickerPresenter, bool>(nameof(IsTimeSelectionVisible), o => o.IsTimeSelectionVisible);
    internal static readonly DirectProperty<DatePickerPresenter, double> PanelHeightProperty =
        AvaloniaProperty.RegisterDirect<DatePickerPresenter, double>(nameof(PanelHeight), o => o.PanelHeight);

    public bool IsNeedConfirm { get => GetValue(IsNeedConfirmProperty); set => SetValue(IsNeedConfirmProperty, value); }
    public bool IsShowNow { get => GetValue(IsShowNowProperty); set => SetValue(IsShowNowProperty, value); }
    public bool IsShowTime { get => GetValue(IsShowTimeProperty); set => SetValue(IsShowTimeProperty, value); }
    public DatePickerMode PickerMode { get => GetValue(PickerModeProperty); set => SetValue(PickerModeProperty, value); }
    public DateTime? SelectedDateTime { get => GetValue(SelectedDateTimeProperty); set => SetValue(SelectedDateTimeProperty, value); }
    public DateTime? PickerDisplayDate { get => GetValue(PickerDisplayDateProperty); set => SetValue(PickerDisplayDateProperty, value); }
    public DateTime? MinDate { get => GetValue(MinDateProperty); set => SetValue(MinDateProperty, value); }
    public DateTime? MaxDate { get => GetValue(MaxDateProperty); set => SetValue(MaxDateProperty, value); }
    public ClockIdentifierType ClockIdentifier { get => GetValue(ClockIdentifierProperty); set => SetValue(ClockIdentifierProperty, value); }
    public TimeSpan? TempSelectedTime { get => GetValue(TempSelectedTimeProperty); set => SetValue(TempSelectedTimeProperty, value); }
    internal bool IsMotionEnabled { get => GetValue(IsMotionEnabledProperty); set => SetValue(IsMotionEnabledProperty, value); }

    private bool _buttonsPanelVisible;
    private bool _isTimeSelectionVisible;
    internal bool IsButtonsPanelVisible
    {
        get => _buttonsPanelVisible;
        private set => SetAndRaise(IsButtonsPanelVisibleProperty, ref _buttonsPanelVisible, value);
    }
    internal bool IsTimeSelectionVisible
    {
        get => _isTimeSelectionVisible;
        private set => SetAndRaise(IsTimeSelectionVisibleProperty, ref _isTimeSelectionVisible, value);
    }
    internal double PanelHeight => PickerMode == DatePickerMode.Quarter ? 86 : 301;

    public event EventHandler<DateSelectedEventArgs>? HoverDateTimeChanged;
    public event EventHandler<ChoosingStatusEventArgs>? ChoosingStatusChanged;

    protected Button? NowButton;
    protected Button? TodayButton;
    protected Button? ConfirmButton;
    protected TimeView? TimeView;
    protected DateViewer? SingleViewer;
    protected RangeDateViewer? RangeViewer;
    protected readonly DatePickerEditSession EditSession = new();

    private DateTime _displayDate = DateTime.Today;
    private DateViewerPanelKind _panelKind;
    private bool _sessionOpened;
    protected bool UpdatingCandidate;
    private CultureInfo _culture = CultureInfo.CurrentCulture;
    private ILanguageManager? _languageManager;

    protected virtual bool IsRangeEditor => false;
    protected virtual DateTime? SecondarySelectedDateTimeCore => null;
    protected virtual DateRangeActivePart ActiveRangePart => DateRangeActivePart.Start;
    protected DateRangeActivePart EffectiveActiveRangePart => EditSession.IsOpen ? EditSession.ActivePart : ActiveRangePart;
    protected virtual int PanelCount => IsRangeEditor && !IsTimeSelectionVisible ? 2 : 1;
    protected DatePickerDateRangeConstraint EffectiveDateRange =>
        DatePickerDateRangeConstraint.Create(MinDate, MaxDate, PickerMode);

    internal void ResetOpenPanelState()
    {
        OpenSession();
        _displayDate = ResolveOpenDisplayAnchor() ?? DateTime.Today;
        _panelKind = DateViewer.TargetPanel(ToSelectionUnit(PickerMode));
        RefreshViewer();
        SyncTimeViewTimeValue();
        SetupButtonStatus();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplate();
        SingleViewer?.SetHost(null);
        RangeViewer?.SetHost(null);
        base.OnApplyTemplate(e);
        NowButton = e.NameScope.Find<Button>("PART_NowButton");
        TodayButton = e.NameScope.Find<Button>("PART_TodayButton");
        ConfirmButton = e.NameScope.Find<Button>("PART_ConfirmButton");
        TimeView = e.NameScope.Find<TimeView>("PART_TimeView");
        SingleViewer = e.NameScope.Find<DateViewer>("PART_DateViewer");
        RangeViewer = e.NameScope.Find<RangeDateViewer>("PART_RangeDateViewer");
        SingleViewer?.SetHost(this);
        RangeViewer?.SetHost(this);
        AttachTemplate();
        if (_sessionOpened)
        {
            RefreshViewer();
        }

        SetupButtonStatus();
        SetupConfirmButtonEnableStatus();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SingleViewer?.SetHost(this);
        RangeViewer?.SetHost(this);
        _languageManager = Application.Current is { } app ? global::AtomUI.ApplicationExtensions.GetLanguageManager(app) : null;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += OnLanguageChanged;
            _culture = _languageManager.Current.FormattingCulture;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SingleViewer?.SetHost(null);
        RangeViewer?.SetHost(null);
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged -= OnLanguageChanged;
        }

        _languageManager = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (UpdatingCandidate)
        {
            return;
        }

        if (change.Property == IsShowTimeProperty || change.Property == PickerModeProperty)
        {
            UpdateTimeVisibility();
            RaisePropertyChanged(PanelHeightProperty, default, PanelHeight);
        }
        if (change.Property == SelectedDateTimeProperty || change.Property == PickerDisplayDateProperty ||
            change.Property == MinDateProperty || change.Property == MaxDateProperty ||
            change.Property == IsNeedConfirmProperty || change.Property == IsShowTimeProperty ||
            change.Property == PickerModeProperty || IsRangeProperty(change.Property))
        {
            if (_sessionOpened)
            {
                var input = CreateInput();
                if (IsRangeEditor)
                {
                    var committed = EditSession.Input.Committed;
                    if (change.Property == SelectedDateTimeProperty)
                    {
                        committed = committed with { Start = SelectedDateTime };
                    }
                    else if (IsRangeProperty(change.Property))
                    {
                        committed = committed with { End = SecondarySelectedDateTimeCore };
                    }

                    input = input with { Committed = committed };
                }
                EditSession.Apply(new DatePickerEditAction.Reconfigure(input));
                RefreshViewer();
            }
            SetupButtonStatus();
            SetupConfirmButtonEnableStatus();
        }
    }

    protected virtual bool IsRangeProperty(AvaloniaProperty property) => false;
    protected virtual DatePickerEditInput CreateInput() => new()
    {
        Mode = PickerMode,
        IsRange = IsRangeEditor,
        RequestedConfirmation = IsNeedConfirm,
        ShowTime = IsShowTime,
        Committed = new DateViewerRange(SelectedDateTime, SecondarySelectedDateTimeCore),
        ActivePart = ActiveRangePart,
        DisplayDate = PickerDisplayDate,
        MinDate = MinDate,
        MaxDate = MaxDate
    };

    protected virtual DateTime? ResolveOpenDisplayAnchor() => EditSession.OpenAnchor;

    protected void OpenSession()
    {
        EditSession.Open(CreateInput());
        _sessionOpened = true;
        ApplyDraft(EditSession.Draft);
    }

    protected void ApplyResult(DatePickerEditResult result)
    {
        if (result.HasPreview && result.CommitKind == DatePickerCommitKind.None)
        {
            EmitHoverDateTimeChanged(result.PreviewValue);
            return;
        }
        ApplyDraft(result.Draft);
        SetupConfirmButtonEnableStatus();
        RefreshViewer();
        if (result.HasPreview)
        {
            EmitHoverDateTimeChanged(result.PreviewValue);
        }

        switch (result.CommitKind)
        {
            case DatePickerCommitKind.Partial:
                OnPartialConfirmed(result);
                break;
            case DatePickerCommitKind.Final:
                ApplyCommitValue(result.CommitValue ?? result.Draft);
                EmitChoosingStatusChanged(false);
                base.OnConfirmed();
                break;
        }
    }

    protected virtual void ApplyDraft(DateViewerRange draft)
    {
        UpdatingCandidate = true;
        SetCurrentValue(SelectedDateTimeProperty, draft.Start);
        UpdatingCandidate = false;
    }

    protected virtual void ApplyCommitValue(DateViewerRange value) => ApplyDraft(value);
    protected virtual void OnPartialConfirmed(DatePickerEditResult result) { }

    protected virtual void SetupConfirmButtonEnableStatus()
    {
        if (ConfirmButton is not null)
        {
            ConfirmButton.IsEnabled = EditSession.CanConfirm;
        }
    }

    protected virtual void NotifyConfirmButtonClicked() =>
        ApplyResult(EditSession.Apply(new DatePickerEditAction.Confirm()));
    protected virtual void NotifyTodayButtonClicked()
    {
        _displayDate = DateTime.Today;
        ApplyResult(EditSession.Apply(new DatePickerEditAction.Today(DateTime.Today)));
    }
    protected virtual void NotifyNowButtonClicked()
    {
        var now = DateTime.Now;
        _displayDate = now;
        var result = EditSession.Apply(new DatePickerEditAction.Now(now));
        SyncTimeViewTimeValue();
        ApplyResult(result);
    }
    protected virtual void NotifyCalendarViewDateHoverChanged(DateTime? value) =>
        ApplyResult(EditSession.Apply(new DatePickerEditAction.PreviewDate(value)));
    protected virtual void NotifyCalendarViewDateSelected() { }
    protected virtual void NotifyTimeViewHoverChanged(TimeSpan? value) =>
        ApplyResult(EditSession.Apply(new DatePickerEditAction.PreviewTime(value)));

    protected virtual void TimeViewTempTimeSelected(TimeSpan? value)
    {
        SetCurrentValue(TempSelectedTimeProperty, value);
        ApplyResult(EditSession.Apply(new DatePickerEditAction.ChooseTime(value)));
    }

    protected virtual void SyncTimeViewTimeValue()
    {
        if (TimeView is not null)
        {
            TimeView.SelectedTime = EditSession.TimeDisplayValue;
        }
    }

    internal void EmitConfirmed() => base.OnConfirmed();
    protected void EmitChoosingStatusChanged(bool choosing) =>
        ChoosingStatusChanged?.Invoke(this, new ChoosingStatusEventArgs(choosing));
    protected void EmitHoverDateTimeChanged(DateTime? value) =>
        HoverDateTimeChanged?.Invoke(this, new DateSelectedEventArgs(value));

    protected override void OnDismiss()
    {
        ApplyResult(EditSession.Close(DatePickerCloseReason.Dismissed));
        base.OnDismiss();
    }

    DatePanelInput IDatePanelHost.ReadInput()
    {
        var unit = ToSelectionUnit(PickerMode);
        var firstDay = DatePanelAlgorithms.GetWeekFirstDay(_culture);
        return new DatePanelInput
        {
            DisplayDate = _displayDate,
            PanelKind = _panelKind,
            SelectionUnit = unit,
            Today = DateTime.Today,
            Culture = _culture,
            FirstDayOfWeek = firstDay,
            ShowWeek = unit == DateViewerSelectionUnit.Week,
            PanelCount = PanelCount,
            IsRangeSelection = IsRangeEditor,
            SelectedDate = IsRangeEditor ? null : EditSession.SelectedDateForDisplay,
            Range = IsRangeEditor ? EditSession.Draft : null,
            ActiveRangePart = EffectiveActiveRangePart,
            WeekNumbering = DateWeekNumbering.Culture,
            ConstraintMode = DatePanelConstraintMode.Picker,
            MinDate = EffectiveDateRange.Start,
            MaxDate = EffectiveDateRange.End
        };
    }

    void IDatePanelHost.NavigateTo(DateTime displayDate, DateViewerPanelKind panelKind)
    {
        _displayDate = displayDate;
        _panelKind = panelKind;
        RefreshViewer();
    }

    void IDatePanelHost.Activate(DateCellSelection selection)
    {
        var target = DateViewer.TargetPanel(ToSelectionUnit(PickerMode));
        var current = selection.Kind switch
        {
            DateViewerCellType.Month => DateViewerPanelKind.Month,
            DateViewerCellType.Quarter => DateViewerPanelKind.Quarter,
            DateViewerCellType.Year => DateViewerPanelKind.Year,
            _ => DateViewerPanelKind.Date
        };
        if (current != target)
        {
            _displayDate = selection.Value;
            _panelKind = current == DateViewerPanelKind.Year && target == DateViewerPanelKind.Date
                ? DateViewerPanelKind.Month : target;
            RefreshViewer();
            return;
        }
        ApplyResult(EditSession.Apply(new DatePickerEditAction.ChooseDate(selection.Value)));
    }

    void IDatePanelHost.Preview(DateTime? value) => NotifyCalendarViewDateHoverChanged(value);

    private void RefreshViewer()
    {
        SingleViewer?.RefreshHost();
        RangeViewer?.RefreshHost();
    }

    private void AttachTemplate()
    {
        if (SingleViewer is not null)
        {
            SingleViewer.PointerEntered += OnViewerPointerEntered;
            SingleViewer.PointerExited += OnViewerPointerExited;
        }
        if (RangeViewer is not null)
        {
            RangeViewer.PointerEntered += OnViewerPointerEntered;
            RangeViewer.PointerExited += OnViewerPointerExited;
        }
        if (TimeView is not null)
        {
            TimeView.HoverTimeChanged += OnTimeHover;
            TimeView.TimeSelected += OnTimeSelected;
            TimeView.TempTimeSelected += OnTempTimeSelected;
            SyncTimeViewTimeValue();
        }
        if (TodayButton is not null)
        {
            TodayButton.Click += OnTodayClick;
            TodayButton.PointerEntered += OnTodayEnter;
            TodayButton.PointerExited += OnActionExit;
        }
        if (NowButton is not null)
        {
            NowButton.Click += OnNowClick;
            NowButton.PointerEntered += OnNowEnter;
            NowButton.PointerExited += OnActionExit;
        }
        if (ConfirmButton is not null)
        {
            ConfirmButton.Click += OnConfirmClick;
            ConfirmButton.PointerEntered += OnConfirmEnter;
            ConfirmButton.PointerExited += OnActionExit;
        }
    }

    private void DetachTemplate()
    {
        if (SingleViewer is not null)
        {
            SingleViewer.PointerEntered -= OnViewerPointerEntered;
            SingleViewer.PointerExited -= OnViewerPointerExited;
        }
        if (RangeViewer is not null)
        {
            RangeViewer.PointerEntered -= OnViewerPointerEntered;
            RangeViewer.PointerExited -= OnViewerPointerExited;
        }
        if (TimeView is not null)
        {
            TimeView.HoverTimeChanged -= OnTimeHover;
            TimeView.TimeSelected -= OnTimeSelected;
            TimeView.TempTimeSelected -= OnTempTimeSelected;
        }
        if (TodayButton is not null)
        {
            TodayButton.Click -= OnTodayClick;
            TodayButton.PointerEntered -= OnTodayEnter;
            TodayButton.PointerExited -= OnActionExit;
        }
        if (NowButton is not null)
        {
            NowButton.Click -= OnNowClick;
            NowButton.PointerEntered -= OnNowEnter;
            NowButton.PointerExited -= OnActionExit;
        }
        if (ConfirmButton is not null)
        {
            ConfirmButton.Click -= OnConfirmClick;
            ConfirmButton.PointerEntered -= OnConfirmEnter;
            ConfirmButton.PointerExited -= OnActionExit;
        }
    }

    private void SetupButtonStatus()
    {
        UpdateTimeVisibility();
        if (NowButton is null || TodayButton is null || ConfirmButton is null)
        {
            return;
        }

        var input = CreateInput();
        ConfirmButton.IsVisible = input.RequiresConfirmation;
        TodayButton.IsEnabled = EffectiveDateRange.Contains(DateTime.Today);
        NowButton.IsEnabled = EffectiveDateRange.Contains(DateTime.Now);
        NowButton.IsVisible = IsShowNow && PickerMode == DatePickerMode.Date && IsTimeSelectionVisible;
        TodayButton.IsVisible = IsShowNow && PickerMode == DatePickerMode.Date && !IsTimeSelectionVisible;
        var centered = !input.RequiresConfirmation ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        NowButton.HorizontalAlignment = centered;
        TodayButton.HorizontalAlignment = centered;
        IsButtonsPanelVisible = NowButton.IsVisible || TodayButton.IsVisible || ConfirmButton.IsVisible;
    }

    private void UpdateTimeVisibility() => IsTimeSelectionVisible = IsShowTime && PickerMode == DatePickerMode.Date;
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        _culture = _languageManager?.Current.FormattingCulture ?? CultureInfo.CurrentCulture;
        RefreshViewer();
    }
    private void OnViewerPointerEntered(object? sender, PointerEventArgs e) => EmitChoosingStatusChanged(true);
    private void OnViewerPointerExited(object? sender, PointerEventArgs e) => EmitChoosingStatusChanged(false);
    private void OnTimeHover(object? sender, TimeSelectedEventArgs e) => NotifyTimeViewHoverChanged(e.Time);
    private void OnTempTimeSelected(object? sender, TimeSelectedEventArgs e) => TimeViewTempTimeSelected(e.Time);
    private void OnTimeSelected(object? sender, TimeSelectedEventArgs e)
    {
        if (!CreateInput().RequiresConfirmation)
        {
            ApplyResult(EditSession.Apply(new DatePickerEditAction.SubmitTime(e.Time ?? TimeSpan.Zero)));
        }
    }
    private void OnTodayClick(object? sender, RoutedEventArgs e) => NotifyTodayButtonClicked();
    private void OnNowClick(object? sender, RoutedEventArgs e) => NotifyNowButtonClicked();
    private void OnConfirmClick(object? sender, RoutedEventArgs e) => NotifyConfirmButtonClicked();
    private void OnTodayEnter(object? sender, PointerEventArgs e) => EmitHoverDateTimeChanged(DateTime.Today);
    private void OnNowEnter(object? sender, PointerEventArgs e) => EmitHoverDateTimeChanged(DateTime.Now);
    private void OnConfirmEnter(object? sender, PointerEventArgs e) =>
        ApplyResult(EditSession.Apply(new DatePickerEditAction.PreviewCandidate()));
    private void OnActionExit(object? sender, PointerEventArgs e) => EmitChoosingStatusChanged(false);

    internal static DateViewerSelectionUnit ToSelectionUnit(DatePickerMode mode) => mode switch
    {
        DatePickerMode.Date => DateViewerSelectionUnit.Date,
        DatePickerMode.Week => DateViewerSelectionUnit.Week,
        DatePickerMode.Month => DateViewerSelectionUnit.Month,
        DatePickerMode.Quarter => DateViewerSelectionUnit.Quarter,
        DatePickerMode.Year => DateViewerSelectionUnit.Year,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}
