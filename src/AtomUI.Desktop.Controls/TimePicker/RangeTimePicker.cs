using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class RangeTimePicker : RangeInfoPickerInput
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeSpan?> RangeStartSelectedTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeStartSelectedTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<TimeSpan?> RangeEndSelectedTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeEndSelectedTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<TimeSpan?> RangeStartDefaultTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeStartDefaultTime),
            enableDataValidation: true);

    public static readonly StyledProperty<TimeSpan?> RangeEndDefaultTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeEndDefaultTime),
            enableDataValidation: true);
    
    public static readonly StyledProperty<int> MinuteIncrementProperty =
        AvaloniaProperty.Register<RangeTimePicker, int>(nameof(MinuteIncrement), 1, coerce: CoerceMinuteIncrement);

    public static readonly StyledProperty<int> SecondIncrementProperty =
        AvaloniaProperty.Register<RangeTimePicker, int>(nameof(SecondIncrement), 1, coerce: CoerceSecondIncrement);
    
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        AvaloniaProperty.Register<RangeTimePicker, ClockIdentifierType>(nameof(ClockIdentifier), ClockIdentifierType.HourClock24);
    
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<RangeTimePicker, bool>(nameof(IsNeedConfirm), true);
    
    public static readonly StyledProperty<bool> IsChangeOnScrollProperty =
        TimePicker.IsChangeOnScrollProperty.AddOwner<RangeTimePicker>();

    public static readonly StyledProperty<bool> IsShowNowProperty =
        AvaloniaProperty.Register<RangeTimePicker, bool>(nameof(IsShowNow), true);

    public TimeSpan? RangeStartSelectedTime
    {
        get => GetValue(RangeStartSelectedTimeProperty);
        set => SetValue(RangeStartSelectedTimeProperty, value);
    }

    public TimeSpan? RangeEndSelectedTime
    {
        get => GetValue(RangeEndSelectedTimeProperty);
        set => SetValue(RangeEndSelectedTimeProperty, value);
    }

    public TimeSpan? RangeStartDefaultTime
    {
        get => GetValue(RangeStartDefaultTimeProperty);
        set => SetValue(RangeStartDefaultTimeProperty, value);
    }

    public TimeSpan? RangeEndDefaultTime
    {
        get => GetValue(RangeEndDefaultTimeProperty);
        set => SetValue(RangeEndDefaultTimeProperty, value);
    }
    
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

    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }
    
    public bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }
    
    public bool IsChangeOnScroll
    {
        get => GetValue(IsChangeOnScrollProperty);
        set => SetValue(IsChangeOnScrollProperty, value);
    }

    public bool IsShowNow
    {
        get => GetValue(IsShowNowProperty);
        set => SetValue(IsShowNowProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<RangeTimePicker, double> PreferredWidthProperty =
        AvaloniaProperty.RegisterDirect<RangeTimePicker, double>(nameof(PreferredWidth),
            o => o.PreferredWidth,
            (o, v) => o.PreferredWidth = v);
    
    internal static readonly DirectProperty<RangeTimePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeTimePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);
    
    internal static readonly DirectProperty<RangeTimePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeTimePicker, string?>(nameof(PmText),
            o => o.PmText,
            (o, v) => o.PmText = v);

    private double _preferredWidth;

    internal double PreferredWidth
    {
        get => _preferredWidth;
        set
        {
            if (MathUtils.AreClose(_preferredWidth, value))
            {
                return;
            }

            SetAndRaise(PreferredWidthProperty, ref _preferredWidth, value);
            InvalidateMeasure();
        }
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

    private const string AntDesignDefaultRangeStartInputWidthReferenceText = "Start time";
    private const string AntDesignDefaultRangeEndInputWidthReferenceText   = "End time";

    private TimePickerPresenter? _pickerPresenter;
    
    static RangeTimePicker()
    {
        AffectsMeasure<RangeTimePicker>(PreferredWidthProperty);
        RangeStartSelectedTimeProperty.Changed.AddClassHandler<RangeTimePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
        RangeEndSelectedTimeProperty.Changed.AddClassHandler<RangeTimePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
    }

    public RangeTimePicker()
    {
    }
    
    /// <summary>
    /// 清除时间选择器的值，不考虑默认值
    /// </summary>
    public override void Clear()
    {
        _pickerPresenter?.EndEditing();
        SetCurrentValue(RangeStartSelectedTimeProperty, null);
        SetCurrentValue(RangeEndSelectedTimeProperty, null);
        base.Clear();
        ClosePickerFlyout();
        if (IsPickerOpen)
        {
            NotifyRangeActivatedPartChanged();
        }
    }
    
    /// <summary>
    /// 重置时间选择器的值，当有默认值设置的时候，会将当前的值设置成默认值
    /// </summary>
    public void Reset()
    {
        RangeStartSelectedTime = RangeStartDefaultTime;
        RangeEndSelectedTime = RangeEndDefaultTime;
    }
    
    protected override Control CreatePickerPresenter()
    {
        // This presenter is created in an independent Content subtree. The owner's
        // ControlTheme cannot bind into its separate template owner.
        var timePickerPresenter = new TimePickerPresenter();
        timePickerPresenter[!TimePickerPresenter.IsChangeOnScrollProperty] = this[!IsChangeOnScrollProperty];
        timePickerPresenter[!TimePickerPresenter.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        timePickerPresenter[!TimePickerPresenter.MinuteIncrementProperty] = this[!MinuteIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.SecondIncrementProperty] = this[!SecondIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.ClockIdentifierProperty] = this[!ClockIdentifierProperty];
        timePickerPresenter[!TimePickerPresenter.IsNeedConfirmProperty]   = this[!IsNeedConfirmProperty];
        timePickerPresenter[!TimePickerPresenter.IsShowNowProperty]       = this[!IsShowNowProperty];
        return timePickerPresenter;
    }

    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCreated(pickerPresenter);
        _pickerPresenter = pickerPresenter as TimePickerPresenter;
    }

    protected override void NotifyPickerPresenterCleared(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCleared(pickerPresenter);
        _pickerPresenter = null;
    }

    protected override void NotifyPickerOpened()
    {
        if (RangeActivatedPart == RangeActivatedPart.None)
        {
            RangeActivatedPart = RangeActivatedPart.Start;
        }
        base.NotifyPickerOpened();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged += HandleChoosingStatusChanged;
            _pickerPresenter.HoverTimeChanged      += HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.Dismissed += HandleDismissed;
            _pickerPresenter.CandidateChanged += HandleCandidateChanged;
            _pickerPresenter.FocusLeft += HandleFocusLeft;
            NotifyRangeActivatedPartChanged();
            _pickerPresenter.ResetOpenPanelState();
        }
    }

    protected override void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
        // RangeInfoPickerInput clears the active endpoint here. Finalize the draft
        // while its endpoint is still known, before that shared lifecycle transition.
        if (_pickerPresenter is { IsEditing: true })
        {
            var candidate = _pickerPresenter.EndEditing();
            if (!IsNeedConfirm)
            {
                CommitEndpoint(RangeActivatedPart, candidate);
            }
        }
        base.NotifyFlyoutAboutToClose(selectedIsValid);
    }

    private void CommitEndpoint(RangeActivatedPart part, TimeSpan? value)
    {
        if (part == RangeActivatedPart.End)
        {
            SetCurrentValue(RangeEndSelectedTimeProperty, value);
        }
        else if (part == RangeActivatedPart.Start)
        {
            SetCurrentValue(RangeStartSelectedTimeProperty, value);
        }
    }

    protected override void NotifyPickerClosed()
    {
        base.NotifyPickerClosed();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged -= HandleChoosingStatusChanged;
            _pickerPresenter.HoverTimeChanged      -= HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             -= HandleConfirmed;
            _pickerPresenter.Dismissed -= HandleDismissed;
            _pickerPresenter.CandidateChanged -= HandleCandidateChanged;
            _pickerPresenter.FocusLeft -= HandleFocusLeft;
            _pickerPresenter.EndEditing();
            RefreshRangeTexts();
        }
    }
    
    private void HandleChoosingStatusChanged(object? sender, ChoosingStatusEventArgs args)
    {
        IsChoosing = args.IsChoosing;
        UpdatePseudoClasses();
        if (!args.IsChoosing)
        {
            ClearHoverSelectedInfo();
        }
    }
    
    private void ClearHoverSelectedInfo()
    {
        var display = _pickerPresenter?.DisplayTime;
        var text = _pickerPresenter?.PreviewTime == null && _pickerPresenter?.InputText != null
            ? _pickerPresenter.InputText
            : DateTimeUtils.FormatTimeSpan(display, ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SecondaryText = text;
        }
        else
        {
            Text = text;
        }
    }

    private void HandleCandidateChanged(object? sender, EventArgs args) => ClearHoverSelectedInfo();
    private void HandleHoverTimeChanged(object? sender, TimeSelectedEventArgs args) => ClearHoverSelectedInfo();
    private void HandleDismissed(object? sender, EventArgs args) => ClosePickerFlyout();

    private void HandleConfirmed(object? sender, EventArgs args)
    {
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            SetCurrentValue(RangeStartSelectedTimeProperty, _pickerPresenter?.SelectedTime);
            if (RangeEndSelectedTime is null)
            {
                RangeActivatedPart = RangeActivatedPart.End;
                _pickerPresenter?.ResetOpenPanelState();
                return;
            }
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SetCurrentValue(RangeEndSelectedTimeProperty, _pickerPresenter?.SelectedTime);
            if (RangeStartSelectedTime is null)
            {
                RangeActivatedPart = RangeActivatedPart.Start;
                _pickerPresenter?.ResetOpenPanelState();
                return;
            }
        }

        ClosePickerFlyout();
        if (IsPickerOpen)
        {
            _pickerPresenter?.ResetOpenPanelState();
        }
    }
    
    private static int CoerceMinuteIncrement(AvaloniaObject sender, int value)
    {
        if (value < 1 || value > 59)
        {
            throw new ArgumentOutOfRangeException(null, "1 >= MinuteIncrement <= 59");
        }

        return value;
    }

    private static int CoerceSecondIncrement(AvaloniaObject sender, int value)
    {
        if (value < 1 || value > 59)
        {
            throw new ArgumentOutOfRangeException(null, "1 >= SecondIncrement <= 59");
        }

        return value;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == RangeActivatedPartProperty && _pickerPresenter is { IsEditing: true })
        {
            var candidate = _pickerPresenter.EndEditing();
            if (!IsNeedConfirm)
            {
                CommitEndpoint(change.GetOldValue<RangeActivatedPart>(), candidate);
            }
            RefreshRangeTexts();
        }
        base.OnPropertyChanged(change);
        if (IsPickerOpen && _pickerPresenter is { IsEditing: true } &&
            ((change.Property == RangeStartSelectedTimeProperty && RangeActivatedPart == RangeActivatedPart.Start) ||
             (change.Property == RangeEndSelectedTimeProperty && RangeActivatedPart == RangeActivatedPart.End)))
        {
            _pickerPresenter.SetCurrentValue(TimePickerPresenter.SelectedTimeProperty,
                RangeActivatedPart == RangeActivatedPart.Start ? RangeStartSelectedTime : RangeEndSelectedTime);
            _pickerPresenter.ResetOpenPanelState();
        }
        if (IsFormattedTextAffectingProperty(change.Property))
        {
            RefreshRangeTexts();
            CalculatePreferredWidth();
        }
        else if (IsPreferredWidthAffectingProperty(change.Property))
        {
            CalculatePreferredWidth();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == RangeStartSelectedTimeProperty)
            {
                if (RangeStartSelectedTime.HasValue)
                {
                    Text = DateTimeUtils.FormatTimeSpan(RangeStartSelectedTime.Value,
                        ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
                }
                else
                {
                    ResetRangeStartTimeValue();
                }
                CalculatePreferredWidth();
            }
            else if (change.Property == RangeEndSelectedTimeProperty)
            {
                if (RangeEndSelectedTime.HasValue)
                {
                    SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndSelectedTime.Value,
                        ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
                }
                else
                {
                    ResetRangeEndTimeValue();
                }
                CalculatePreferredWidth();
            }
        }
    }

    private static bool IsFormattedTextAffectingProperty(AvaloniaProperty property)
    {
        return property == ClockIdentifierProperty ||
               property == AmTextProperty ||
               property == PmTextProperty;
    }

    private static bool IsPreferredWidthAffectingProperty(AvaloniaProperty property)
    {
        return property == FontSizeProperty ||
               property == FontFamilyProperty ||
               property == FontStyleProperty ||
               property == FontWeightProperty ||
               property == SizeTypeProperty ||
               property == MinWidthProperty ||
               property == WidthProperty ||
               property == MaxWidthProperty ||
               property == HorizontalAlignmentProperty;
    }

    private void RefreshRangeTexts()
    {
        if (RangeStartSelectedTime.HasValue)
        {
            Text = DateTimeUtils.FormatTimeSpan(RangeStartSelectedTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            ResetRangeStartTimeValue();
        }

        if (RangeEndSelectedTime.HasValue)
        {
            SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndSelectedTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            ResetRangeEndTimeValue();
        }
    }
    
    private void CalculatePreferredWidth()
    {
        if (!double.IsNaN(Width) || HorizontalAlignment == HorizontalAlignment.Stretch)
        {
            PreferredInputWidth = double.NaN;
            PreferredWidth      = 0;
        }
        else
        {
            var preferredInputWidth = CalculateContentPreferredWidth();

            if (!double.IsNaN(MinWidth))
            {
                preferredInputWidth = Math.Max(MinWidth, preferredInputWidth);
            }

            if (!double.IsNaN(MaxWidth))
            {
                preferredInputWidth = Math.Min(MaxWidth, preferredInputWidth);
            }
            PreferredInputWidth = preferredInputWidth;
            PreferredWidth      = preferredInputWidth;
        }
    }

    private double CalculateContentPreferredWidth()
    {
        var formatWidth = DateTimeUtils.CalculateWidestFormattedTimeSpanSize(
            ClockIdentifier == ClockIdentifierType.HourClock12,
            AmText, PmText,
            FontSize, FontFamily, FontStyle, FontWeight).Width;
        var defaultInputBaselineWidth = DatePickerFormattingHelper.CalculateAntDesignInputBaselineWidth(
            FontSize,
            FontFamily,
            FontStyle,
            FontWeight,
            AntDesignDefaultRangeStartInputWidthReferenceText,
            AntDesignDefaultRangeEndInputWidthReferenceText);

        return Math.Max(formatWidth, defaultInputBaselineWidth);
    }
    
    protected void ResetRangeStartTimeValue()
    {
        if (RangeStartDefaultTime is not null)
        {
            Text = DateTimeUtils.FormatTimeSpan(RangeStartDefaultTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            Text = null;
        }
    }
    
    protected void ResetRangeEndTimeValue()
    {
        if (RangeEndDefaultTime is not null)
        {
            SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndDefaultTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            SecondaryText = null;
        }
    }
    
    protected override void NotifyRangeActivatedPartChanged()
    {
        base.NotifyRangeActivatedPartChanged();
        if (_pickerPresenter != null)
        {
            var value = RangeActivatedPart switch
            {
                RangeActivatedPart.Start => RangeStartSelectedTime,
                RangeActivatedPart.End => RangeEndSelectedTime,
                _ => (TimeSpan?)null
            };
            _pickerPresenter.SetCurrentValue(TimePickerPresenter.SelectedTimeProperty, value);
            if (IsPickerOpen && RangeActivatedPart != RangeActivatedPart.None)
            {
                _pickerPresenter.ResetOpenPanelState();
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size   = base.MeasureOverride(availableSize);
        var width  = size.Width;
        var height = size.Height;
        if (PreferredWidth > 0 &&
            InfoInputBox is not null &&
            SecondaryInfoInputBox is not null)
        {
            var currentInputWidth = InfoInputBox.DesiredSize.Width + SecondaryInfoInputBox.DesiredSize.Width;
            var preferredWidth    = size.Width - currentInputWidth + PreferredWidth * 2;
            width = Math.Max(width, preferredWidth);
        }

        return new Size(width, height);
    }

    protected override bool ShowClearButtonPredicate()
    {
        return RangeStartSelectedTime is not null || RangeEndSelectedTime is not null;
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (RangeStartDefaultTime is not null && RangeStartSelectedTime is null)
        {
            RangeStartSelectedTime = RangeStartDefaultTime;
        }
        
        if (RangeEndDefaultTime is not null && RangeEndSelectedTime is null)
        {
            RangeEndSelectedTime = RangeEndDefaultTime;
        }
    }
    
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        HandleFocusLeft(this, EventArgs.Empty);
    }

    private void HandleFocusLeft(object? sender, EventArgs args)
    {
        if (!IsPickerOpen || IsPopupPinnedOpen)
        {
            return;
        }
        var target = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
        if (target != null && (target == this || target.GetVisualAncestors().Contains(this) ||
            (PickerPresenter is Visual presenter && (target == presenter || target.GetVisualAncestors().Contains(presenter)))))
        {
            return;
        }
        ClosePickerFlyout();
    }

    private void ReleaseInputHandlers()
    {
        if (InfoInputBox != null)
        {
            InfoInputBox.PropertyChanged -= HandleInputTextChanged;
        }
        if (SecondaryInfoInputBox != null)
        {
            SecondaryInfoInputBox.PropertyChanged -= HandleInputTextChanged;
        }
    }

    private void AttachInputHandlers()
    {
        ReleaseInputHandlers();
        if (InfoInputBox != null)
        {
            InfoInputBox.PropertyChanged += HandleInputTextChanged;
        }
        if (SecondaryInfoInputBox != null)
        {
            SecondaryInfoInputBox.PropertyChanged += HandleInputTextChanged;
        }
    }

    private void HandleInputTextChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != TextBox.TextProperty || sender is not TextBox input ||
            string.Equals(input.Text ?? string.Empty, (ReferenceEquals(input, SecondaryInfoInputBox) ? SecondaryText : Text) ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }
        var text = input.Text;
        RangeActivatedPart = ReferenceEquals(input, SecondaryInfoInputBox) ? RangeActivatedPart.End : RangeActivatedPart.Start;
        SetCurrentValue(IsPickerOpenProperty, true);
        var valid = TimePickerPresenter.TryParseInput(text, ClockIdentifier, AmText, PmText, out var time);
        _pickerPresenter?.SetInputCandidate(text, time, valid);
        if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SetCurrentValue(SecondaryTextProperty, text);
        }
        else
        {
            SetCurrentValue(TextProperty, text);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && e.Key == Key.Enter)
        {
            if (_pickerPresenter?.InputText is { Length: 0 })
            {
                Clear();
            }
            else
            {
                _pickerPresenter?.ConfirmCandidate();
            }
            e.Handled = true;
        }
        else if (!e.Handled && e.Key == Key.Escape && IsPickerOpen)
        {
            ClosePickerFlyout();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachInputHandlers();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseInputHandlers();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseInputHandlers();
        base.OnApplyTemplate(e);
        AttachInputHandlers();
        if (InfoIcon is null)
        {
            SetValue(InfoIconProperty, new ClockCircleOutlined(), BindingPriority.Template);
        }
        RefreshRangeTexts();
        CalculatePreferredWidth();
    }
    
    #region 实现 FormItem 接口
    protected override void NotifySetFormValue(object? value)
    {
        var rangeValue = value as (TimeSpan?, TimeSpan?)?;
        if (rangeValue != null)
        {
            RangeStartSelectedTime = rangeValue.Value.Item1;
            RangeEndSelectedTime   = rangeValue.Value.Item2;
        }
    }

    protected override object? NotifyGetFormValue()
    {
        if (RangeStartSelectedTime is null || RangeEndSelectedTime is null)
        {
            return null;
        }
        return (RangeStartSelectedTime, RangeEndSelectedTime);
    }

    protected override void NotifyClearFormValue()
    {
        RangeStartSelectedTime = null;
        RangeEndSelectedTime   = null;
    }

    private void HandleSelectedValueChanged(AvaloniaPropertyChangedEventArgs args)
    {
        NotifyFormValueChanged((RangeStartSelectedTime, RangeEndSelectedTime));
    }
    #endregion
}
