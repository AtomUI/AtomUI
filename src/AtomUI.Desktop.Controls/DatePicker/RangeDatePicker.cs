using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Localization;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class RangeDatePicker : RangeInfoPickerInput
{
    #region 公共属性定义
    
    public static readonly StyledProperty<DateTime?> RangeStartSelectedDateProperty =
        AvaloniaProperty.Register<RangeDatePicker, DateTime?>(nameof(RangeStartSelectedDate),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<DateTime?> RangeEndSelectedDateProperty =
        AvaloniaProperty.Register<RangeDatePicker, DateTime?>(nameof(RangeEndSelectedDate),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<RangeDatePicker, bool>(nameof(IsNeedConfirm));
    
    public static readonly StyledProperty<bool> IsShowNowProperty =
        DatePicker.IsShowNowProperty.AddOwner<RangeDatePicker>();
    
    public static readonly StyledProperty<bool> IsShowTimeProperty =
        DatePicker.IsShowTimeProperty.AddOwner<RangeDatePicker>();

    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        DatePicker.ClockIdentifierProperty.AddOwner<RangeDatePicker>();

    public static readonly StyledProperty<string?> FormatProperty =
        DatePicker.FormatProperty.AddOwner<RangeDatePicker>();

    public static readonly StyledProperty<DatePickerMode> PickerModeProperty =
        DatePicker.PickerModeProperty.AddOwner<RangeDatePicker>();

    public static readonly StyledProperty<DateTime?> PickerDisplayDateProperty =
        DatePicker.PickerDisplayDateProperty.AddOwner<RangeDatePicker>();

    public static readonly StyledProperty<DateTime?> MinDateProperty =
        DatePicker.MinDateProperty.AddOwner<RangeDatePicker>();

    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        DatePicker.MaxDateProperty.AddOwner<RangeDatePicker>();
    
    public DateTime? RangeStartSelectedDate
    {
        get => GetValue(RangeStartSelectedDateProperty);
        set => SetValue(RangeStartSelectedDateProperty, value);
    }

    public DateTime? RangeEndSelectedDate
    {
        get => GetValue(RangeEndSelectedDateProperty);
        set => SetValue(RangeEndSelectedDateProperty, value);
    }

    public DateTime? RangeStartDefaultDate { get; set; }

    public DateTime? RangeEndDefaultDate { get; set; }
    
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
    
    public bool IsShowTime
    {
        get => GetValue(IsShowTimeProperty);
        set => SetValue(IsShowTimeProperty, value);
    }
    
    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
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
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<RangeDatePicker, double> PreferredWidthProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, double>(nameof(PreferredWidth),
            o => o.PreferredWidth,
            (o, v) => o.PreferredWidth = v);

    internal static readonly DirectProperty<RangeDatePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);

    internal static readonly DirectProperty<RangeDatePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, string?>(nameof(PmText),
            o => o.PmText,
            (o, v) => o.PmText = v);
    
    internal static readonly DirectProperty<RangeDatePicker, double> RangePickerIndicatorOffsetStartProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, double>(nameof(RangePickerIndicatorOffsetStart),
            o => o.RangePickerIndicatorOffsetStart,
            (o, v) => o.RangePickerIndicatorOffsetStart = v);
    
    internal static readonly DirectProperty<RangeDatePicker, double> RangePickerIndicatorOffsetEndProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, double>(nameof(RangePickerIndicatorOffsetEnd),
            o => o.RangePickerIndicatorOffsetEnd,
            (o, v) => o.RangePickerIndicatorOffsetEnd = v);
    
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
    
    private double _rangePickerIndicatorStart;

    internal double RangePickerIndicatorOffsetStart
    {
        get => _rangePickerIndicatorStart;
        set => SetAndRaise(RangePickerIndicatorOffsetStartProperty, ref _rangePickerIndicatorStart, value);
    }
    
    private double _rangePickerIndicatorEnd;

    internal double RangePickerIndicatorOffsetEnd
    {
        get => _rangePickerIndicatorEnd;
        set => SetAndRaise(RangePickerIndicatorOffsetEndProperty, ref _rangePickerIndicatorEnd, value);
    }

    #endregion

    private RangeDatePickerPresenter? _pickerPresenter;
    private ILanguageManager? _languageManager;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _languageManager = Application.Current is { } app ? global::AtomUI.ApplicationExtensions.GetLanguageManager(app) : null;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += OnLanguageChanged;
        }

        RefreshRangeTexts();
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
        RefreshRangeTexts();
        CalculatePreferredWidth();
    }

    public RangeDatePicker()
    {
    }

    static RangeDatePicker()
    {
        AffectsMeasure<RangeDatePicker>(PreferredWidthProperty);
        RangeStartSelectedDateProperty.Changed.AddClassHandler<RangeDatePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
        RangeEndSelectedDateProperty.Changed.AddClassHandler<RangeDatePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
    }
    
    protected override Control CreatePickerPresenter()
    {
        var presenter = new RangeDatePickerPresenter();
        presenter[!RangeDatePickerPresenter.IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
        presenter[!RangeDatePickerPresenter.SelectedDateTimeProperty]          = this[!RangeStartSelectedDateProperty];
        presenter[!RangeDatePickerPresenter.SecondarySelectedDateTimeProperty] = this[!RangeEndSelectedDateProperty];
        presenter[!RangeDatePickerPresenter.ClockIdentifierProperty]           = this[!ClockIdentifierProperty];
        presenter[!RangeDatePickerPresenter.IsNeedConfirmProperty]             = this[!IsNeedConfirmProperty];
        presenter[!RangeDatePickerPresenter.IsShowNowProperty]                 = this[!IsShowNowProperty];
        presenter[!RangeDatePickerPresenter.IsShowTimeProperty]                = this[!IsShowTimeProperty];
        presenter[!RangeDatePickerPresenter.PickerModeProperty]                = this[!PickerModeProperty];
        presenter[!RangeDatePickerPresenter.PickerDisplayDateProperty]         = this[!PickerDisplayDateProperty];
        presenter[!RangeDatePickerPresenter.MinDateProperty]                   = this[!MinDateProperty];
        presenter[!RangeDatePickerPresenter.MaxDateProperty]                   = this[!MaxDateProperty];

        return presenter;
    }

    public override void Clear()
    {
        base.Clear();

        RangeStartSelectedDate = null;
        RangeEndSelectedDate   = null;
    }

    public void Reset()
    {
        RangeStartSelectedDate = RangeStartDefaultDate;
        RangeEndSelectedDate   = RangeEndDefaultDate;
    }

    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCreated(pickerPresenter);
        _pickerPresenter = pickerPresenter as RangeDatePickerPresenter;
        _pickerPresenter?.NotifyRepairReverseRange(true);
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
            if (RangeActivatedPart == RangeActivatedPart.None)
            {
                RangeActivatedPart = RangeStartSelectedDate is null
                    ? RangeActivatedPart.Start
                    : RangeEndSelectedDate is null ? RangeActivatedPart.End : RangeActivatedPart.Start;
            }

            _pickerPresenter.ChoosingStatusChanged += HandleChoosingStatusChanged;
            _pickerPresenter.HoverDateTimeChanged  += HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.RangePartConfirmed    += HandleRangePartConfirmed;

            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                _pickerPresenter.NotifySelectRangeStart(true);
            }
            else
            {
                _pickerPresenter.NotifySelectRangeStart(false);
            }

            _pickerPresenter.SelectedDateTime          = RangeStartSelectedDate;
            _pickerPresenter.SecondarySelectedDateTime = RangeEndSelectedDate;
            _pickerPresenter.ResetRangePickState();
            _pickerPresenter.ResetRangeOpenPanelState();
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
            _pickerPresenter.RangePartConfirmed    -= HandleRangePartConfirmed;
            _pickerPresenter.RestoreCommittedRangeAfterClose(
                new DateViewerRange(RangeStartSelectedDate, RangeEndSelectedDate));
        }
        IsChoosing = false;
        UpdatePseudoClasses();
        RefreshRangeTexts();
        CalculatePreferredWidth();
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
        Text          = FormatDateTime(_pickerPresenter?.SelectedDateTime ?? RangeStartSelectedDate);
        SecondaryText = FormatDateTime(_pickerPresenter?.SecondarySelectedDateTime ?? RangeEndSelectedDate);
        CalculatePreferredWidth();
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
    
    private void HandleHoverDateTimeChanged(object? sender, DateSelectedEventArgs args)
    {
        if (args.Date.HasValue)
        {
            var previewStart = _pickerPresenter?.SelectedDateTime ?? RangeStartSelectedDate;
            var previewEnd   = _pickerPresenter?.SecondarySelectedDateTime ?? RangeEndSelectedDate;
            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                previewStart = args.Date;
            }
            else if (RangeActivatedPart == RangeActivatedPart.End)
            {
                previewEnd = args.Date;
            }
            (previewStart, previewEnd) = NormalizePreviewRange(previewStart, previewEnd);
            Text          = FormatDateTime(previewStart);
            SecondaryText = FormatDateTime(previewEnd);
            CalculatePreferredWidth();
        }
    }

    private static (DateTime? Start, DateTime? End) NormalizePreviewRange(DateTime? start, DateTime? end)
    {
        return start is not null && end is not null && end.Value.Date < start.Value.Date
            ? (end, start)
            : (start, end);
    }

    private void HandleRangePartConfirmed(object? sender, EventArgs args)
    {
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            RangeStartSelectedDate = _pickerPresenter?.SelectedDateTime;
            RangeActivatedPart     = RangeActivatedPart.End;
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            RangeEndSelectedDate = _pickerPresenter?.SecondarySelectedDateTime;
            RangeActivatedPart   = RangeActivatedPart.Start;
        }
    }
    
    private void HandleConfirmed(object? sender, EventArgs args)
    {
        var rangeStart = _pickerPresenter?.SelectedDateTime;
        var rangeEnd   = _pickerPresenter?.SecondarySelectedDateTime;
        if (rangeStart is not null && rangeEnd is not null)
        {
            if (rangeEnd.Value.Date < rangeStart.Value.Date)
            {
                RangeStartSelectedDate = rangeEnd;
                RangeEndSelectedDate   = rangeStart;
            }
            else
            {
                RangeStartSelectedDate = rangeStart;
                RangeEndSelectedDate   = rangeEnd;
            }
        }

        ClosePickerFlyout();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RangeActivatedPartProperty)
        {
            NotifyRangeActivatedPartChanged();
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
            if (change.Property == RangeStartSelectedDateProperty)
            {
                Text = FormatDateTime(RangeStartSelectedDate);
                CalculatePreferredWidth();
            }
            else if (change.Property == RangeEndSelectedDateProperty)
            {
                SecondaryText = FormatDateTime(RangeEndSelectedDate);
                CalculatePreferredWidth();
            }
        }
    }

    private void RefreshRangeTexts()
    {
        Text          = FormatDateTime(RangeStartSelectedDate);
        SecondaryText = FormatDateTime(RangeEndSelectedDate);
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
        // Width / Stretch 时仍交给外部布局决定（PreferredWidth 置 0 关闭总宽放大）。
        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        var preferredInputWidth = DatePickerFormattingHelper.CalculateBoundedRangePreferredInputWidth(
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
        PreferredInputWidth = preferredInputWidth;
        PreferredWidth      = (!double.IsNaN(Width) || HorizontalAlignment == HorizontalAlignment.Stretch)
            ? 0
            : preferredInputWidth;
    }
    
    protected override void NotifyRangeActivatedPartChanged()
    {
        SetupPickerIndicatorPosition();
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            _pickerPresenter?.NotifySelectRangeStart(true);
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            _pickerPresenter?.NotifySelectRangeStart(false);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (InfoInputBox is not null && SecondaryInfoInputBox != null)
        {
            Rect targetBounds = default;
            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                targetBounds = InfoInputBox.Bounds;
            }
            else if (RangeActivatedPart == RangeActivatedPart.End)
            {
                targetBounds = SecondaryInfoInputBox.Bounds;
            }

            var delta = targetBounds.Width * 0.1;
            RangePickerIndicatorOffsetStart = targetBounds.X + delta;
            RangePickerIndicatorOffsetEnd   = DesiredSize.Width - targetBounds.X - delta;
        }
        return size;
    }

    protected override bool ShowClearButtonPredicate()
    {
        return RangeStartSelectedDate is not null || RangeEndSelectedDate is not null;
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

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (RangeStartDefaultDate is not null && RangeStartSelectedDate is null)
        {
            RangeStartSelectedDate = RangeStartDefaultDate;
        }
        
        if (RangeEndDefaultDate is not null && RangeEndSelectedDate is null)
        {
            RangeEndSelectedDate = RangeEndDefaultDate;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (InfoIcon is null)
        {
            SetValue(InfoIconProperty, new CalendarOutlined(), BindingPriority.Template);
        }
        RefreshRangeTexts();
        CalculatePreferredWidth();
    }
    
    #region 实现 FormItem 接口
    protected override void NotifySetFormValue(object? value)
    {
        var rangeValue = value as (DateTime?, DateTime?)?;
        if (rangeValue != null)
        {
            RangeStartSelectedDate = rangeValue.Value.Item1;
            RangeEndSelectedDate   = rangeValue.Value.Item2;
        }
    }

    protected override object? NotifyGetFormValue()
    {
        if (RangeStartSelectedDate == null || RangeEndSelectedDate == null)
        {
            return null;
        }
        return (RangeStartSelectedDate, RangeEndSelectedDate);
    }

    protected override void NotifyClearFormValue()
    {
        RangeStartSelectedDate = null;
        RangeEndSelectedDate   = null;
    }

    private void HandleSelectedValueChanged(AvaloniaPropertyChangedEventArgs args)
    {
        NotifyFormValueChanged((RangeStartSelectedDate, RangeEndSelectedDate));
    }
    #endregion
}
