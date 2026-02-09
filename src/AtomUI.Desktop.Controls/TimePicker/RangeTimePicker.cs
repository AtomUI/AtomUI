using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Utils;
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

public class RangeTimePicker : RangeInfoPickerInput,
                               IControlSharedTokenResourcesHost
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
        AvaloniaProperty.Register<RangeTimePicker, ClockIdentifierType>(nameof(ClockIdentifier));
    
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<RangeTimePicker, bool>(nameof(IsNeedConfirm));
    
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
    
    public bool IsShowNow
    {
        get => GetValue(IsShowNowProperty);
        set => SetValue(IsShowNowProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    
    string IControlSharedTokenResourcesHost.TokenId => TimePickerToken.ID;
    
    internal static readonly DirectProperty<RangeTimePicker, double> PreferredWidthProperty =
        AvaloniaProperty.RegisterDirect<RangeTimePicker, double>(nameof(PreferredWidth),
            o => o.PreferredWidth,
            (o, v) => o.PreferredWidth = v);

    private double _preferredWidth;

    internal double PreferredWidth
    {
        get => _preferredWidth;
        set => SetAndRaise(PreferredWidthProperty, ref _preferredWidth, value);
    }

    #endregion
    
    private TimePickerPresenter? _pickerPresenter;
    private CompositeDisposable? _flyoutBindingDisposables;
    
    static RangeTimePicker()
    {
        AffectsMeasure<RangeTimePicker>(PreferredWidthProperty);
    }

    public RangeTimePicker()
    {
        this.RegisterResources();
    }
    
    /// <summary>
    /// 清除时间选择器的值，不考虑默认值
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        
        RangeStartSelectedTime = null;
        RangeEndSelectedTime   = null;
    }
    
    /// <summary>
    /// 重置时间选择器的值，当有默认值设置的时候，会将当前的值设置成默认值
    /// </summary>
    public void Reset()
    {
        RangeStartSelectedTime = RangeStartDefaultTime;
        RangeEndSelectedTime = RangeEndDefaultTime;
    }
    
    protected override Flyout CreatePickerFlyout()
    {
        var timePickerFlyout = new TimePickerFlyout();
        _flyoutBindingDisposables?.Dispose();
        _flyoutBindingDisposables = new CompositeDisposable(6);
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, timePickerFlyout, TimePickerPresenter.IsMotionEnabledProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, MinuteIncrementProperty, timePickerFlyout, TimePickerPresenter.MinuteIncrementProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, SecondIncrementProperty, timePickerFlyout, TimePickerPresenter.SecondIncrementProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, ClockIdentifierProperty, timePickerFlyout, TimePickerPresenter.ClockIdentifierProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsNeedConfirmProperty, timePickerFlyout, TimePickerPresenter.IsNeedConfirmProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsShowNowProperty, timePickerFlyout, TimePickerPresenter.IsShowNowProperty));
        return timePickerFlyout;
    }
    
    protected override void NotifyFlyoutPresenterCreated(Control flyoutPresenter)
    {
        if (PickerFlyout is TimePickerFlyout timePickerFlyout)
        {
            _pickerPresenter = timePickerFlyout.TimePickerPresenter;
        }
    }

    protected override void NotifyFlyoutOpened()
    {
        base.NotifyFlyoutOpened();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatueChanged += HandleChoosingStatueChanged;
            _pickerPresenter.HoverTimeChanged      += HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
        }
    }
    
    protected override void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
        base.NotifyFlyoutAboutToClose(selectedIsValid);
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatueChanged -= HandleChoosingStatueChanged;
            _pickerPresenter.HoverTimeChanged      -= HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             -= HandleConfirmed;
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
            Text = DateTimeUtils.FormatTimeSpan(RangeStartSelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12);
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndSelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12);
        }
    }
    
    private void HandleHoverTimeChanged(object? sender, TimeSelectedEventArgs args)
    {
        if (args.Time.HasValue)
        {
            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                Text = DateTimeUtils.FormatTimeSpan(args.Time.Value,
                    ClockIdentifier == ClockIdentifierType.HourClock12);
            }
            else if (RangeActivatedPart == RangeActivatedPart.End)
            {
                SecondaryText = DateTimeUtils.FormatTimeSpan(args.Time.Value,
                    ClockIdentifier == ClockIdentifierType.HourClock12);
            }
        }
        else
        {
            Text = null;
        }
    }
    
    private void HandleConfirmed(object? sender, EventArgs args)
    {
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            RangeStartSelectedTime = _pickerPresenter?.SelectedTime;
            if (RangeEndSelectedTime is null)
            {
                RangeActivatedPart = RangeActivatedPart.End;
                return;
            }
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            RangeEndSelectedTime = _pickerPresenter?.SelectedTime;
            if (RangeStartSelectedTime is null)
            {
                RangeActivatedPart = RangeActivatedPart.Start;
                return;
            }
        }

        ClosePickerFlyout();
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
        base.OnPropertyChanged(change);
        if (change.Property == RangeActivatedPartProperty)
        {
            HandleRangeActivatedPartChanged();
        }
        else if (change.Property == FontSizeProperty ||
                 change.Property == FontFamilyProperty ||
                 change.Property == FontFamilyProperty ||
                 change.Property == FontStyleProperty ||
                 change.Property == ClockIdentifierProperty ||
                 change.Property == MinWidthProperty ||
                 change.Property == WidthProperty ||
                 change.Property == MaxWidthProperty)
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
                        ClockIdentifier == ClockIdentifierType.HourClock12);
                }
                else
                {
                    ResetRangeStartTimeValue();
                }
            }
            else if (change.Property == RangeEndSelectedTimeProperty)
            {
                if (RangeEndSelectedTime.HasValue)
                {
                    SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndSelectedTime.Value,
                        ClockIdentifier == ClockIdentifierType.HourClock12);
                }
                else
                {
                    ResetRangeEndTimeValue();
                }
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
            var text = DateTimeUtils.FormatTimeSpan(TimeSpan.Zero,
                ClockIdentifier == ClockIdentifierType.HourClock12);
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
    
    protected void ResetRangeStartTimeValue()
    {
        if (InfoInputBox is not null)
        {
            if (RangeStartDefaultTime is not null)
            {
                InfoInputBox.Text = DateTimeUtils.FormatTimeSpan(RangeStartDefaultTime.Value,
                    ClockIdentifier == ClockIdentifierType.HourClock12);
            }
            else
            {
                InfoInputBox.Clear();
            }
        }
    }
    
    protected void ResetRangeEndTimeValue()
    {
        if (SecondaryInfoInputBox is not null)
        {
            if (RangeEndDefaultTime is not null)
            {
                SecondaryInfoInputBox.Text = DateTimeUtils.FormatTimeSpan(RangeEndDefaultTime.Value,
                    ClockIdentifier == ClockIdentifierType.HourClock12);
            }
            else
            {
                SecondaryInfoInputBox.Clear();
            }
        }
    }
    
    protected override void HandleRangeActivatedPartChanged()
    {
        base.HandleRangeActivatedPartChanged();
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            if (RangeEndSelectedTime is null)
            {
                ResetRangeStartTimeValue();
            }
            if (_pickerPresenter is not null)
            {
                _pickerPresenter.SelectedTime = RangeStartSelectedTime;
            }

        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            if (RangeStartSelectedTime is null)
            {
                ResetRangeEndTimeValue();
            }
            if (_pickerPresenter is not null)
            {
                _pickerPresenter.SelectedTime = RangeEndSelectedTime;
            }
        }
        else
        {
            if (RangeStartSelectedTime is null)
            {
                ResetRangeStartTimeValue();
            }
    
            if (RangeEndSelectedTime is null)
            {
                ResetRangeEndTimeValue();
            }
            if (_pickerPresenter is not null)
            {
                _pickerPresenter.SelectedTime = null;
            }
        }
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
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (InfoIcon is null)
        {
            SetValue(InfoIconProperty, new ClockCircleOutlined(), BindingPriority.Template);
        }
    }
}