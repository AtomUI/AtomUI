using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class DatePickerFlyout : Flyout
{
    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        DatePicker.SelectedDateTimeProperty.AddOwner<DatePickerFlyout>();
    
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        DatePicker.IsNeedConfirmProperty.AddOwner<DatePickerFlyout>();
    
    public static readonly StyledProperty<bool> IsShowNowProperty =
        DatePicker.IsShowNowProperty.AddOwner<DatePickerFlyout>();
    
    public static readonly StyledProperty<bool> IsShowTimeProperty =
        DatePicker.IsShowTimeProperty.AddOwner<DatePickerFlyout>();
    
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        TimePicker.ClockIdentifierProperty.AddOwner<DatePickerFlyout>();
    
    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
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
    
    internal DatePickerPresenter? DatePickerPresenter { get; set; }
    private CompositeDisposable? _presenterBindingDisposables;
    
    protected override Control CreatePresenter()
    {
        var flyoutPresenter = base.CreatePresenter() as FlyoutPresenter;
        Debug.Assert(flyoutPresenter != null);
        DatePickerPresenter = new DatePickerPresenter();
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new  CompositeDisposable(7);
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, DatePickerPresenter, DatePickerPresenter.IsMotionEnabledProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, SelectedDateTimeProperty, DatePickerPresenter, DatePickerPresenter.SelectedDateTimeProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsNeedConfirmProperty, DatePickerPresenter, DatePickerPresenter.IsNeedConfirmProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsShowNowProperty, DatePickerPresenter, DatePickerPresenter.IsShowNowProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsShowTimeProperty, DatePickerPresenter, DatePickerPresenter.IsShowTimeProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ClockIdentifierProperty, DatePickerPresenter, DatePickerPresenter.ClockIdentifierProperty));

        flyoutPresenter.Content = DatePickerPresenter;
        return flyoutPresenter;
    }
}