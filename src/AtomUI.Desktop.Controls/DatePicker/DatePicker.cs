using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

public enum DatePickerMode
{
    Date,
    Week,
    Month,
    Quarter,
    Year
}

public partial class DatePicker : InfoPickerInput
{
    #region 公共属性定义

    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(SelectedDateTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<DateTime?> DefaultDateTimeProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(DefaultDateTime),
            enableDataValidation: true);

    public static readonly StyledProperty<DateTime?> PickerDisplayDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(PickerDisplayDate));

    public static readonly StyledProperty<DateTime?> MinDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MinDate));

    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MaxDate));

    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(Format));

    public static readonly StyledProperty<DatePickerMode> PickerModeProperty =
        AvaloniaProperty.Register<DatePicker, DatePickerMode>(nameof(PickerMode));

    public static readonly StyledProperty<bool> IsShowTimeProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(IsShowTime), false);

    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(IsNeedConfirm));

    public static readonly StyledProperty<bool> IsShowNowProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(IsShowNow), true);

    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        TimePicker.ClockIdentifierProperty.AddOwner<DatePicker>();

    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public DateTime? DefaultDateTime
    {
        get => GetValue(DefaultDateTimeProperty);
        set => SetValue(DefaultDateTimeProperty, value);
    }

    public DateTime? PickerDisplayDate
    {
        get => GetValue(PickerDisplayDateProperty);
        set => SetValue(PickerDisplayDateProperty, value);
    }

    public DateTime? MinDate
    {
        get => GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    public DateTime? MaxDate
    {
        get => GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    public string? Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public DatePickerMode PickerMode
    {
        get => GetValue(PickerModeProperty);
        set => SetValue(PickerModeProperty, value);
    }

    public bool IsShowTime
    {
        get => GetValue(IsShowTimeProperty);
        set => SetValue(IsShowTimeProperty, value);
    }

    public bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }

    public bool IsShowNow
    {
        get => GetValue(IsShowNowProperty);
        set => SetValue(IsShowNowProperty, value);
    }

    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<DatePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<DatePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);

    internal static readonly DirectProperty<DatePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<DatePicker, string?>(nameof(PmText),
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

    public DatePicker()
    {
    }

    static DatePicker()
    {
        SelectedDateTimeProperty.Changed.AddClassHandler<DatePicker>((datePicker, args) => datePicker.NotifyFormValueChanged(args.NewValue));
    }

    private DatePickerPresenter? _pickerPresenter;
    private ILanguageManager? _languageManager;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _languageManager = Application.Current is { } app ? global::AtomUI.ApplicationExtensions.GetLanguageManager(app) : null;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += OnLanguageChanged;
        }

        Text = FormatDateTime(SelectedDateTime);
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
        Text = FormatDateTime(SelectedDateTime);
        CalculatePreferredWidth();
    }

    /// <summary>
    /// 清除时间选择器的值，不考虑默认值
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        SelectedDateTime = null;
    }

    /// <summary>
    /// 重置时间选择器的值，当有默认值设置的时候，会将当前的值设置成默认值
    /// </summary>
    public void Reset()
    {
        SelectedDateTime = DefaultDateTime;
    }

    protected string FormatDateTime(DateTime? dateTime)
    {
        if (dateTime is null)
        {
            return string.Empty;
        }

        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        return DatePickerFormattingHelper.FormatDateTime(dateTime.Value, Format, PickerMode, IsShowTime, ClockIdentifier, formatInfo);
    }

    protected override Control CreatePickerPresenter()
    {
        var presenter = new DatePickerPresenter();
        presenter[!DatePickerPresenter.IsMotionEnabledProperty]  = this[!IsMotionEnabledProperty];
        presenter[!DatePickerPresenter.SelectedDateTimeProperty] = this[!SelectedDateTimeProperty];
        presenter[!DatePickerPresenter.PickerDisplayDateProperty] = this[!PickerDisplayDateProperty];
        presenter[!DatePickerPresenter.MinDateProperty]          = this[!MinDateProperty];
        presenter[!DatePickerPresenter.MaxDateProperty]          = this[!MaxDateProperty];
        presenter[!DatePickerPresenter.IsNeedConfirmProperty]    = this[!IsNeedConfirmProperty];
        presenter[!DatePickerPresenter.IsShowNowProperty]        = this[!IsShowNowProperty];
        presenter[!DatePickerPresenter.IsShowTimeProperty]       = this[!IsShowTimeProperty];
        presenter[!DatePickerPresenter.ClockIdentifierProperty]  = this[!ClockIdentifierProperty];
        presenter[!DatePickerPresenter.PickerModeProperty]       = this[!PickerModeProperty];

        return presenter;
    }

    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCreated(pickerPresenter);
        _pickerPresenter = pickerPresenter as DatePickerPresenter;
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
            _pickerPresenter.HoverDateTimeChanged  += HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.ResetOpenPanelState();
        }
    }

    protected override void NotifyPickerClosed()
    {
        base.NotifyPickerClosed();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged -= HandleChoosingStatusChanged;
            _pickerPresenter.HoverDateTimeChanged  -= HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             -= HandleConfirmed;
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

    private void HandleHoverDateTimeChanged(object? sender, DateSelectedEventArgs args)
    {
        if (args.Date.HasValue)
        {
            Text = FormatDateTime(args.Date);
        }
        else
        {
            Text = null;
        }
        CalculatePreferredWidth();
    }

    private void HandleConfirmed(object? sender, EventArgs args)
    {
        SelectedDateTime = _pickerPresenter?.SelectedDateTime;
        ClosePickerFlyout();
    }

    private void ClearHoverSelectedInfo()
    {
        Text = FormatDateTime(_pickerPresenter?.SelectedDateTime ?? SelectedDateTime);
        CalculatePreferredWidth();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (DefaultDateTime is not null && SelectedDateTime is null)
        {
            SelectedDateTime = DefaultDateTime;
        }

        Text = FormatDateTime(SelectedDateTime);

        if (InfoIcon is null)
        {
            SetValue(InfoIconProperty, new CalendarOutlined(), BindingPriority.Template);
        }
        CalculatePreferredWidth();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDateTimeProperty)
        {
            Text = FormatDateTime(SelectedDateTime);
            CalculatePreferredWidth();
        }
        else if (IsFormattedTextAffectingProperty(change.Property))
        {
            Text = FormatDateTime(SelectedDateTime);
            CalculatePreferredWidth();
        }
        else if (IsPreferredWidthAffectingProperty(change.Property))
        {
            CalculatePreferredWidth();
        }
    }

    private static bool IsFormattedTextAffectingProperty(AvaloniaProperty property)
    {
        return DatePickerFormattingHelper.IsFormattedTextAffectingProperty(
            property,
            IsShowTimeProperty,
            FormatProperty,
            PickerModeProperty,
            ClockIdentifierProperty,
            AmTextProperty,
            PmTextProperty);
    }

    private static bool IsPreferredWidthAffectingProperty(AvaloniaProperty property)
    {
        return DatePickerFormattingHelper.IsPreferredWidthAffectingProperty(
            property,
            FontSizeProperty,
            FontFamilyProperty,
            FontStyleProperty,
            FontWeightProperty,
            SizeTypeProperty,
            MinWidthProperty,
            WidthProperty,
            MaxWidthProperty,
            HorizontalAlignmentProperty);
    }

    private void CalculatePreferredWidth()
    {
        // 输入框预留宽度始终按内容基线计算：placeholder 与选中值共用同一宽度基线，
        // 避免显式 Width / Stretch 场景下输入区宽度随文本内容跳变；控件总宽在显式
        // Width / Stretch 时仍交给外部布局决定。
        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        PreferredInputWidth = DatePickerFormattingHelper.CalculateBoundedPreferredInputWidth(
            Format,
            PickerMode,
            IsShowTime,
            ClockIdentifier,
            FontSize,
            FontFamily,
            FontStyle,
            FontWeight,
            MinWidth,
            MaxWidth,
            formatInfo);
    }

    protected override bool ShowClearButtonPredicate()
    {
        return SelectedDateTime is not null;
    }
    
    #region 实现 FormItem 接口
    protected override void NotifySetFormValue(object? value)
    {
        SelectedDateTime = value as DateTime?;
    }

    protected override object? NotifyGetFormValue()
    {
        return SelectedDateTime;
    }

    protected override void NotifyClearFormValue()
    {
        SelectedDateTime = null;
    }
    #endregion
}
