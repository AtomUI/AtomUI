using System.Globalization;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class TimePickerPresenter : PickerPresenterBase
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsNeedConfirmProperty = TimePicker.IsNeedConfirmProperty.AddOwner<TimePickerPresenter>();
    public static readonly StyledProperty<bool> IsShowNowProperty = TimePicker.IsShowNowProperty.AddOwner<TimePickerPresenter>();
    public static readonly StyledProperty<bool> IsChangeOnScrollProperty = TimePicker.IsChangeOnScrollProperty.AddOwner<TimePickerPresenter>();
    public static readonly StyledProperty<int> MinuteIncrementProperty = TimePicker.MinuteIncrementProperty.AddOwner<TimePickerPresenter>();
    public static readonly StyledProperty<int> SecondIncrementProperty = TimePicker.SecondIncrementProperty.AddOwner<TimePickerPresenter>();
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty = TimePicker.ClockIdentifierProperty.AddOwner<TimePickerPresenter>();
    public static readonly StyledProperty<TimeSpan?> SelectedTimeProperty = TimePicker.SelectedTimeProperty.AddOwner<TimePickerPresenter>();
    public static readonly StyledProperty<TimeSpan?> PickerDisplayTimeProperty = TimePicker.PickerDisplayTimeProperty.AddOwner<TimePickerPresenter>();

    public bool IsNeedConfirm { get => GetValue(IsNeedConfirmProperty); set => SetValue(IsNeedConfirmProperty, value); }
    public bool IsShowNow { get => GetValue(IsShowNowProperty); set => SetValue(IsShowNowProperty, value); }
    public bool IsChangeOnScroll { get => GetValue(IsChangeOnScrollProperty); set => SetValue(IsChangeOnScrollProperty, value); }
    public int MinuteIncrement { get => GetValue(MinuteIncrementProperty); set => SetValue(MinuteIncrementProperty, value); }
    public int SecondIncrement { get => GetValue(SecondIncrementProperty); set => SetValue(SecondIncrementProperty, value); }
    public ClockIdentifierType ClockIdentifier { get => GetValue(ClockIdentifierProperty); set => SetValue(ClockIdentifierProperty, value); }
    public TimeSpan? SelectedTime { get => GetValue(SelectedTimeProperty); set => SetValue(SelectedTimeProperty, value); }
    public TimeSpan? PickerDisplayTime { get => GetValue(PickerDisplayTimeProperty); set => SetValue(PickerDisplayTimeProperty, value); }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<TimeSpan?> TempSelectedTimeProperty = AvaloniaProperty.Register<TimePickerPresenter, TimeSpan?>(nameof(TempSelectedTime));
    internal static readonly StyledProperty<bool> IsButtonsPanelVisibleProperty = AvaloniaProperty.Register<TimePickerPresenter, bool>(nameof(IsButtonsPanelVisible), true);
    internal static readonly StyledProperty<bool> CanConfirmProperty = AvaloniaProperty.Register<TimePickerPresenter, bool>(nameof(CanConfirm), true);
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TimePickerPresenter>();

    internal TimeSpan? TempSelectedTime { get => GetValue(TempSelectedTimeProperty); set => SetValue(TempSelectedTimeProperty, value); }
    internal bool IsButtonsPanelVisible { get => GetValue(IsButtonsPanelVisibleProperty); set => SetCurrentValue(IsButtonsPanelVisibleProperty, value); }
    internal bool CanConfirm { get => GetValue(CanConfirmProperty); set => SetCurrentValue(CanConfirmProperty, value); }
    internal bool IsMotionEnabled { get => GetValue(IsMotionEnabledProperty); set => SetValue(IsMotionEnabledProperty, value); }
    internal bool IsEditing { get; private set; }
    internal TimeSpan? PreviewTime { get; private set; }
    internal string? InputText { get; private set; }
    internal bool IsInputValid { get; private set; } = true;
    internal TimeSpan? DisplayTime => PreviewTime ?? TempSelectedTime;

    #endregion

    #region 公共事件定义

    public event EventHandler<TimeSelectedEventArgs>? HoverTimeChanged;
    public event EventHandler<ChoosingStatusEventArgs>? ChoosingStatusChanged;

    #endregion

    #region 内部事件定义

    internal event EventHandler? CandidateChanged;
    internal event EventHandler? FocusLeft;

    #endregion

    private IDisposable? _choosingStateDisposable;
    private Button? _nowButton;
    private Button? _confirmButton;
    private TimeView? _timeView;
    private TimeSpan? _pendingOpenDisplayTime;

    internal void ResetOpenPanelState()
    {
        IsEditing = true;
        InputText = null;
        PreviewTime = null;
        IsInputValid = true;
        SetCurrentValue(TempSelectedTimeProperty, SelectedTime);
        _pendingOpenDisplayTime = SelectedTime ?? PickerDisplayTime ?? TimeSpan.Zero;
        ApplyPendingOpenPanelState();
        UpdateButtonState();
        CandidateChanged?.Invoke(this, EventArgs.Empty);
    }

    internal TimeSpan? EndEditing()
    {
        var candidate = IsInputValid ? TempSelectedTime : SelectedTime;
        IsEditing = false;
        InputText = null;
        PreviewTime = null;
        SetCurrentValue(TempSelectedTimeProperty, null);
        return candidate;
    }

    internal void SetInputCandidate(string? text, TimeSpan? time, bool valid)
    {
        if (!IsEditing)
        {
            return;
        }
        InputText = text ?? string.Empty;
        PreviewTime = null;
        IsInputValid = valid;
        if (valid)
        {
            SetCurrentValue(TempSelectedTimeProperty, time);
        }
        UpdateButtonState();
    }

    internal static bool TryParseInput(string? text, ClockIdentifierType clock, string? amText, string? pmText, out TimeSpan? time)
    {
        time = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }
        var formatInfo = new DateTimeFormatInfo { AMDesignator = amText ?? "AM", PMDesignator = pmText ?? "PM" };
        var formats = clock == ClockIdentifierType.HourClock12
            ? new[] { "hh:mm:ss tt", "h:mm:ss tt", "hh:mm tt", "h:mm tt" }
            : new[] { "HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm" };
        if (DateTime.TryParseExact(text.Trim(), formats, formatInfo, DateTimeStyles.NoCurrentDateDefault, out var parsed))
        {
            time = parsed.TimeOfDay;
            return true;
        }
        return false;
    }

    internal void ConfirmCandidate()
    {
        if (!IsEditing || !CanConfirm)
        {
            return;
        }
        var candidate = TempSelectedTime ?? SelectedTime ?? PickerDisplayTime ?? TimeSpan.Zero;
        IsEditing = false;
        SetCurrentValue(TempSelectedTimeProperty, candidate);
        SetCurrentValue(SelectedTimeProperty, candidate);
        PreviewTime = null;
        InputText = null;
        base.OnConfirmed();
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        FocusLeft?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmCandidate();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            OnDismiss();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedTimeProperty && IsEditing)
        {
            ResetOpenPanelState();
        }
        if (change.Property == IsNeedConfirmProperty && IsEditing)
        {
            ResetOpenPanelState();
        }
        if (change.Property == IsNeedConfirmProperty || change.Property == IsShowNowProperty ||
            change.Property == SelectedTimeProperty || change.Property == TempSelectedTimeProperty || change.Property == PickerDisplayTimeProperty)
        {
            UpdateButtonState();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateEventHandlers();
        base.OnApplyTemplate(e);
        _nowButton = e.NameScope.Get<Button>("PART_NowButton");
        _confirmButton = e.NameScope.Get<Button>("PART_ConfirmButton");
        _timeView = e.NameScope.Get<TimeView>("PART_TimeView");
        AttachTemplateEventHandlers();
        ApplyPendingOpenPanelState();
    }

    private void AttachTemplateEventHandlers()
    {
        DetachTemplateEventHandlers();
        if (_timeView != null)
        {
            _timeView.HoverTimeChanged += HandleTimeViewHoverChanged;
            _timeView.TimeSelected += HandleTimeViewTimeSelected;
            _timeView.TempTimeSelected += HandleTimeViewTempTimeSelected;
            _choosingStateDisposable = _timeView.GetObservable(TimeView.IsPointerInSelectorProperty).Subscribe(isChoosing =>
            {
                ChoosingStatusChanged?.Invoke(this, new ChoosingStatusEventArgs(isChoosing));
                if (!isChoosing)
                {
                    PreviewTime = null;
                    CandidateChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }
        if (_nowButton != null)
        {
            _nowButton.Click += HandleNowButtonClicked;
        }
        if (_confirmButton != null)
        {
            _confirmButton.Click += HandleConfirmButtonClicked;
        }
    }

    private void DetachTemplateEventHandlers()
    {
        _choosingStateDisposable?.Dispose();
        _choosingStateDisposable = null;
        if (_timeView != null)
        {
            _timeView.HoverTimeChanged -= HandleTimeViewHoverChanged;
            _timeView.TimeSelected -= HandleTimeViewTimeSelected;
            _timeView.TempTimeSelected -= HandleTimeViewTempTimeSelected;
        }
        if (_nowButton != null)
        {
            _nowButton.Click -= HandleNowButtonClicked;
        }
        if (_confirmButton != null)
        {
            _confirmButton.Click -= HandleConfirmButtonClicked;
        }
    }

    private void UpdateButtonState()
    {
        IsButtonsPanelVisible = IsShowNow || IsNeedConfirm;
        var candidate = TempSelectedTime ?? SelectedTime ?? PickerDisplayTime ?? TimeSpan.Zero;
        CanConfirm = IsInputValid && !(InputText != null && TempSelectedTime == null) &&
                     candidate >= TimeSpan.Zero && candidate < TimeSpan.FromDays(1);
    }

    private void HandleNowButtonClicked(object? sender, RoutedEventArgs args)
    {
        var now = DateTime.Now.TimeOfDay;
        var validTime = now - TimeSpan.FromMinutes(now.Minutes % MinuteIncrement) - TimeSpan.FromSeconds(now.Seconds % SecondIncrement);
        SetCandidate(validTime);
        ConfirmCandidate();
    }

    private void ApplyPendingOpenPanelState()
    {
        if (_pendingOpenDisplayTime is { } time && _timeView != null)
        {
            _timeView.SyncDisplayTimeToPanel(time);
            _pendingOpenDisplayTime = null;
        }
    }

    private void HandleConfirmButtonClicked(object? sender, RoutedEventArgs args) => ConfirmCandidate();

    private void HandleTimeViewHoverChanged(object? sender, TimeSelectedEventArgs args)
    {
        if (!IsEditing)
        {
            return;
        }
        PreviewTime = args.Time;
        HoverTimeChanged?.Invoke(this, args);
    }

    private void HandleTimeViewTimeSelected(object? sender, TimeSelectedEventArgs args)
    {
        if (IsNeedConfirm)
        {
            ConfirmCandidate();
        }
    }

    private void HandleTimeViewTempTimeSelected(object? sender, TimeSelectedEventArgs args)
    {
        if (IsEditing)
        {
            SetCandidate(args.Time);
        }
    }

    private void SetCandidate(TimeSpan? time)
    {
        PreviewTime = null;
        InputText = null;
        IsInputValid = true;
        SetCurrentValue(TempSelectedTimeProperty, time);
        CandidateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachTemplateEventHandlers();
        ResetOpenPanelState();
        ApplyPendingOpenPanelState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Popup detaches content before it raises Closed. The host owns finalizing
        // the edit; keep its candidate available until that close transition runs.
        DetachTemplateEventHandlers();
        base.OnDetachedFromVisualTree(e);
    }
}
