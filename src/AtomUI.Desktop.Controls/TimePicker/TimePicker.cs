using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

public enum ClockIdentifierType
{
    HourClock12,
    HourClock24
}

public partial class TimePicker : InfoPickerInput
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(IsNeedConfirm), true);

    public static readonly StyledProperty<bool> IsChangeOnScrollProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(IsChangeOnScroll));

    public static readonly StyledProperty<bool> IsShowNowProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(IsShowNow), true);

    public static readonly StyledProperty<int> MinuteIncrementProperty =
        AvaloniaProperty.Register<TimePicker, int>(nameof(MinuteIncrement), 1, coerce: CoerceMinuteIncrement);

    public static readonly StyledProperty<int> SecondIncrementProperty =
        AvaloniaProperty.Register<TimePicker, int>(nameof(SecondIncrement), 1, coerce: CoerceSecondIncrement);

    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        AvaloniaProperty.Register<TimePicker, ClockIdentifierType>(nameof(ClockIdentifier), ClockIdentifierType.HourClock24);

    public static readonly StyledProperty<TimeSpan?> SelectedTimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan?>(nameof(SelectedTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<TimeSpan?> DefaultTimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan?>(nameof(DefaultTime),
            enableDataValidation: true);

    public static readonly StyledProperty<TimeSpan?> PickerDisplayTimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan?>(nameof(PickerDisplayTime));

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

    public TimeSpan? SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public TimeSpan? DefaultTime
    {
        get => GetValue(DefaultTimeProperty);
        set => SetValue(DefaultTimeProperty, value);
    }

    public TimeSpan? PickerDisplayTime
    {
        get => GetValue(PickerDisplayTimeProperty);
        set => SetValue(PickerDisplayTimeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<TimePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<TimePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);
    
    internal static readonly DirectProperty<TimePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<TimePicker, string?>(nameof(PmText),
            o => o.PmText,
            (o, v) => o.PmText = v);

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

    private const string AntDesignDefaultInputWidthReferenceText = "Select time";

    private TimePickerPresenter? _pickerPresenter;

    static TimePicker()
    {
        SelectedTimeProperty.Changed.AddClassHandler<TimePicker>((timePicker, args) => timePicker.NotifyFormValueChanged(args.NewValue));
    }

    public TimePicker()
    {
    }

    protected override Control CreatePickerPresenter()
    {
        // This presenter is created in an independent Content subtree. The owner's
        // ControlTheme cannot bind into its separate template owner.
        var timePickerPresenter = new TimePickerPresenter();
        timePickerPresenter[!TimePickerPresenter.IsChangeOnScrollProperty] = this[!IsChangeOnScrollProperty];
        timePickerPresenter[!TimePickerPresenter.IsMotionEnabledProperty]  = this[!IsMotionEnabledProperty];
        timePickerPresenter[!TimePickerPresenter.MinuteIncrementProperty]  = this[!MinuteIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.SecondIncrementProperty]  = this[!SecondIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.ClockIdentifierProperty]  = this[!ClockIdentifierProperty];
        timePickerPresenter[!TimePickerPresenter.SelectedTimeProperty]     = this[!SelectedTimeProperty];
        timePickerPresenter[!TimePickerPresenter.PickerDisplayTimeProperty] = this[!PickerDisplayTimeProperty];
        timePickerPresenter[!TimePickerPresenter.IsNeedConfirmProperty]    = this[!IsNeedConfirmProperty];
        timePickerPresenter[!TimePickerPresenter.IsShowNowProperty]        = this[!IsShowNowProperty];

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
        base.NotifyPickerOpened();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged += HandleChoosingStatusChanged;
            _pickerPresenter.HoverTimeChanged      += HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.Dismissed += HandleDismissed;
            _pickerPresenter.CandidateChanged += HandleCandidateChanged;
            _pickerPresenter.FocusLeft += HandleFocusLeft;
            _pickerPresenter.ResetOpenPanelState();
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
            var candidate = _pickerPresenter.EndEditing();
            if (!IsNeedConfirm)
            {
                SetCurrentValue(SelectedTimeProperty, candidate);
            }
            Text = DateTimeUtils.FormatTimeSpan(SelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
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
        var displayTime = IsPickerOpen ? _pickerPresenter?.DisplayTime : SelectedTime;
        Text = IsPickerOpen && _pickerPresenter?.PreviewTime == null && _pickerPresenter?.InputText != null
            ? _pickerPresenter.InputText
            : DateTimeUtils.FormatTimeSpan(displayTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
    }

    private void HandleCandidateChanged(object? sender, EventArgs args) => ClearHoverSelectedInfo();
    private void HandleHoverTimeChanged(object? sender, TimeSelectedEventArgs args) => ClearHoverSelectedInfo();
    private void HandleDismissed(object? sender, EventArgs args) => ClosePickerFlyout();

    private void HandleConfirmed(object? sender, EventArgs args)
    {
        SetCurrentValue(SelectedTimeProperty, _pickerPresenter?.SelectedTime);
        ClosePickerFlyout();
        if (IsPickerOpen)
        {
            _pickerPresenter?.ResetOpenPanelState();
        }
    }

    /// <summary>
    /// 清除时间选择器的值，不考虑默认值
    /// </summary>
    public override void Clear()
    {
        SetCurrentValue(SelectedTimeProperty, null);
        base.Clear();
        ClosePickerFlyout();
    }

    /// <summary>
    /// 重置时间选择器的值，当有默认值设置的时候，会将当前的值设置成默认值
    /// </summary>
    public void Reset()
    {
        SelectedTime = DefaultTime;
    }

    protected override bool ShowClearButtonPredicate()
    {
        return SelectedTime is not null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedTimeProperty)
        {
            Text = DateTimeUtils.FormatTimeSpan(SelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
            CalculatePreferredWidth();
        }
        else if (IsFormattedTextAffectingProperty(change.Property))
        {
            Text = DateTimeUtils.FormatTimeSpan(SelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
            CalculatePreferredWidth();
        }
        else if (IsPreferredWidthAffectingProperty(change.Property))
        {
            CalculatePreferredWidth();
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

    private void CalculatePreferredWidth()
    {
        // 输入框预留宽度始终按内容基线计算：placeholder 与选中值共用同一宽度基线，
        // 避免显式 Width / Stretch 场景下输入区宽度随文本内容跳变；控件总宽在显式
        // Width / Stretch 时仍交给外部布局决定。
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
    }

    private double CalculateContentPreferredWidth()
    {
        var formatWidth = DateTimeUtils.CalculateWidestFormattedTimeSpanSize(
            ClockIdentifier == ClockIdentifierType.HourClock12,
            AmText, PmText,
            FontSize, FontFamily, FontStyle, FontWeight).Width;
        var defaultInputBaselineWidth = DatePickerFormattingHelper.CalculateAntDesignInputBaselineWidth(
            FontSize, FontFamily, FontStyle, FontWeight, AntDesignDefaultInputWidthReferenceText);

        return Math.Max(formatWidth, defaultInputBaselineWidth);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (DefaultTime is not null && SelectedTime is null)
        {
            SelectedTime = DefaultTime;
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
    }

    private void AttachInputHandlers()
    {
        ReleaseInputHandlers();
        if (InfoInputBox != null)
        {
            InfoInputBox.PropertyChanged += HandleInputTextChanged;
        }
    }

    private void HandleInputTextChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != TextBox.TextProperty || sender is not TextBox input ||
            string.Equals(input.Text ?? string.Empty, Text ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }
        var text = input.Text;
        SetCurrentValue(IsPickerOpenProperty, true);
        var valid = TimePickerPresenter.TryParseInput(text, ClockIdentifier, AmText, PmText, out var time);
        _pickerPresenter?.SetInputCandidate(text, time, valid);
        SetCurrentValue(TextProperty, text);
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
        CalculatePreferredWidth();
    }
    
    #region 实现 FormItem 接口
    protected override void NotifySetFormValue(object? value)
    {
        SelectedTime = value as TimeSpan?;
    }

    protected override object? NotifyGetFormValue()
    {
        return SelectedTime;
    }

    protected override void NotifyClearFormValue()
    {
        SelectedTime = null;
    }
    #endregion
}
