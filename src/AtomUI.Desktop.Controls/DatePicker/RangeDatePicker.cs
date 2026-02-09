using System.Globalization;
using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Desktop.Controls.CalendarView;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.TimePickerLang;
using AtomUI.Icons.AntDesign;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class RangeDatePicker : RangeInfoPickerInput,
                               IControlSharedTokenResourcesHost
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
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<RangeDatePicker, double> PreferredWidthProperty
        = AvaloniaProperty.RegisterDirect<RangeDatePicker, double>(nameof(PreferredWidth),
            o => o.PreferredWidth,
            (o, v) => o.PreferredWidth = v);

    private double _preferredWidth;

    internal double PreferredWidth
    {
        get => _preferredWidth;
        set => SetAndRaise(PreferredWidthProperty, ref _preferredWidth, value);
    }

    string IControlSharedTokenResourcesHost.TokenId => DatePickerToken.ID;
    Control IControlSharedTokenResourcesHost.HostControl => this;
    
    #endregion
    
    private RangeDatePickerPresenter? _pickerPresenter;
    private bool? _isNeedConfirmedBackup;
    private CompositeDisposable? _flyoutBindingDisposables;

    public RangeDatePicker()
    {
        this.RegisterResources();
    }
    
    protected override Flyout CreatePickerFlyout()
    {
        var flyout = new RangeDatePickerFlyout();
        flyout.IsDetectMouseClickEnabled = false;
        _flyoutBindingDisposables?.Dispose();
        _flyoutBindingDisposables = new CompositeDisposable(7);
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, flyout, RangeDatePickerFlyout.IsMotionEnabledProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, RangeStartSelectedDateProperty, flyout, RangeDatePickerFlyout.SelectedDateTimeProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, RangeEndSelectedDateProperty, flyout, RangeDatePickerFlyout.SecondarySelectedDateTimeProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, ClockIdentifierProperty, flyout, RangeDatePickerFlyout.ClockIdentifierProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsNeedConfirmProperty, flyout, RangeDatePickerFlyout.IsNeedConfirmProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsShowNowProperty, flyout, RangeDatePickerFlyout.IsShowNowProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsShowTimeProperty, flyout, RangeDatePickerFlyout.IsShowTimeProperty));
        
        return flyout;
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
    
    protected override void NotifyFlyoutPresenterCreated(Control flyoutPresenter)
    {
        if (PickerFlyout is RangeDatePickerFlyout datePickerFlyout)
        {
            _pickerPresenter = datePickerFlyout.DatePickerPresenter as RangeDatePickerPresenter;
            _pickerPresenter?.NotifyRepairReverseRange(true);
        }
    }
    
    protected override void NotifyFlyoutOpened()
    {
        base.NotifyFlyoutOpened();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatueChanged += HandleChoosingStatueChanged;
            _pickerPresenter.HoverDateTimeChanged  += HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.RangePartConfirmed    += HandleRangePartConfirmed;
            
            _pickerPresenter.SelectedDateTime          = RangeStartSelectedDate;
            _pickerPresenter.SecondarySelectedDateTime = RangeEndSelectedDate;
            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                _pickerPresenter.NotifySelectRangeStart(true);
            }
            else
            {
                _pickerPresenter.NotifySelectRangeStart(false);
            }
        }
    }
    
    protected override void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
        base.NotifyFlyoutAboutToClose(selectedIsValid);
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatueChanged -= HandleChoosingStatueChanged;
            _pickerPresenter.HoverDateTimeChanged  -= HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             -= HandleConfirmed;
            _pickerPresenter.RangePartConfirmed    -= HandleRangePartConfirmed;

            if (RangeStartSelectedDate == null || RangeEndSelectedDate == null)
            {
                RangeStartSelectedDate = null;
                RangeEndSelectedDate   = null;
            }
        }
    }

    private void HandleChoosingStatueChanged(object? sender, ChoosingStatusEventArgs args)
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
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            Text = FormatDateTime(RangeStartSelectedDate);
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SecondaryText = FormatDateTime(RangeEndSelectedDate);
        }
    }
    
    private string GetEffectiveFormat()
    {
        if (Format is not null)
        {
            return Format;
        }

        var format = "yyyy-MM-dd";
        if (IsShowTime)
        {
           
            if (ClockIdentifier == ClockIdentifierType.HourClock12)
            {
                format = $"{format} hh:mm:ss tt";
            }
            else
            {
                format = $"{format} HH:mm:ss";
            }
        }

        return format;
    }
    
    protected string FormatDateTime(DateTime? dateTime)
    {
        if (dateTime is null)
        {
            return string.Empty;
        }

        var format = GetEffectiveFormat();
        if (ClockIdentifier == ClockIdentifierType.HourClock12)
        {
            var formatInfo = new DateTimeFormatInfo();
            formatInfo.AMDesignator = LanguageResourceBinder.GetLangResource(TimePickerLangResourceKey.AMText)!;
            formatInfo.PMDesignator = LanguageResourceBinder.GetLangResource(TimePickerLangResourceKey.PMText)!;
            return dateTime.Value.ToString(format, formatInfo);
        }

        return dateTime.Value.ToString(format);
    }
    
    private void HandleHoverDateTimeChanged(object? sender, DateSelectedEventArgs args)
    {
        if (args.Date.HasValue)
        {
            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                Text = FormatDateTime(args.Date);
            }
            else if (RangeActivatedPart == RangeActivatedPart.End)
            {
                SecondaryText = FormatDateTime(args.Date);
            }
        }
    }

    private void HandleRangePartConfirmed(object? sender, EventArgs args)
    {
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            RangeStartSelectedDate = _pickerPresenter?.SelectedDateTime;
            RangeActivatedPart     = RangeActivatedPart.End;
            _pickerPresenter?.NotifySelectRangeStart(false);
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            RangeEndSelectedDate = _pickerPresenter?.SecondarySelectedDateTime;
            RangeActivatedPart   = RangeActivatedPart.Start;
            _pickerPresenter?.NotifySelectRangeStart(true);
        }
    }
    
    private void HandleConfirmed(object? sender, EventArgs args)
    {
        var rangeStart = _pickerPresenter?.SelectedDateTime;
        var rangeEnd   = _pickerPresenter?.SecondarySelectedDateTime;
        if (rangeStart is not null && rangeEnd is not null)
        {
            if (DateTimeHelper.CompareDays(rangeEnd.Value, rangeStart.Value) < 0)
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
            HandleRangeActivatedPartChanged();
        } 
        else if (change.Property == IsShowTimeProperty)
        {
            if (IsShowTime)
            {
                _isNeedConfirmedBackup = IsNeedConfirm;
                IsNeedConfirm          = true;
            }
            else
            {
                if (_isNeedConfirmedBackup is not null)
                {
                    IsNeedConfirm = _isNeedConfirmedBackup.Value;
                }
            }
        }
        else if (change.Property == FontSizeProperty ||
                 change.Property == FontFamilyProperty ||
                 change.Property == FontFamilyProperty ||
                 change.Property == FontStyleProperty ||
                 change.Property == ClockIdentifierProperty ||
                 change.Property == MinWidthProperty ||
                 change.Property == WidthProperty ||
                 change.Property == MaxWidthProperty ||
                 change.Property == HorizontalAlignmentProperty)
        {
            CalculatePreferredWidth();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == RangeStartSelectedDateProperty)
            {
                Text = FormatDateTime(RangeStartSelectedDate);
            }
            else if (change.Property == RangeEndSelectedDateProperty)
            {
                SecondaryText = FormatDateTime(RangeEndSelectedDate);
            }
        }
    }
    
    private void CalculatePreferredWidth()
    {
        if (!double.IsNaN(Width) || HorizontalAlignment == HorizontalAlignment.Stretch)
        {
            PreferredInputWidth = double.NaN;
        }
        else
        {
            var text                = FormatDateTime(DateTime.Today);
            var preferredInputWidth = TextUtils.CalculateTextSize(text, FontSize, FontFamily, FontStyle, FontWeight).Width;
            if (Watermark != null)
            {
                preferredInputWidth = Math.Max(preferredInputWidth, TextUtils.CalculateTextSize(Watermark, FontSize, FontFamily, FontStyle, FontWeight).Width);
            }

            if (SecondaryWatermark != null)
            {
                preferredInputWidth = Math.Max(preferredInputWidth, TextUtils.CalculateTextSize(SecondaryWatermark, FontSize, FontFamily, FontStyle, FontWeight).Width);
            }

            preferredInputWidth *= 1.1;
            
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
    }
    
    protected override void HandleRangeActivatedPartChanged()
    {
        SetupPickerIndicatorPosition();
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            if (RangeEndSelectedDate is null)
            {
                InfoInputBox?.Clear();
            }
            _pickerPresenter?.NotifySelectRangeStart(true);
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            if (RangeStartSelectedDate is null)
            {
                SecondaryInfoInputBox?.Clear();
            }
            _pickerPresenter?.NotifySelectRangeStart(false);
        }
        else
        {
            if (RangeStartSelectedDate is null)
            {
                InfoInputBox?.Clear();
            }
    
            if (RangeEndSelectedDate is null)
            {
                SecondaryInfoInputBox?.Clear();
            }
        }
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
        if (PickerInnerBox is not null)
        {
            var preferredWidth = 0d;
            if (DecoratedBox?.ContentRightAddOn is Control rightAddOnContent)
            {
                preferredWidth += PreferredWidth + rightAddOnContent.DesiredSize.Width +
                                  PickerInnerBox.Padding.Left +
                                  PickerInnerBox.Padding.Right;
            }

            if (RangePickerArrow is not null)
            {
                preferredWidth += RangePickerArrow.DesiredSize.Width;
            }

            preferredWidth += PreferredWidth;

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
        Text = FormatDateTime(RangeStartSelectedDate);
        SecondaryText = FormatDateTime(RangeEndSelectedDate);
    }
}