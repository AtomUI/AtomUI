using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Input;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum ButtonSpinnerLocation
{
    Left,
    Right
}

[PseudoClasses(ButtonSpinnerPseudoClass.Left, ButtonSpinnerPseudoClass.Right)]
public class ButtonSpinner : Spinner,
                             IMotionAwareControl,
                             IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> AllowSpinProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof(AllowSpin), true);
    
    public static readonly StyledProperty<bool> ShowButtonSpinnerProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof(ShowButtonSpinner), true);
    
    public static readonly StyledProperty<ButtonSpinnerLocation> ButtonSpinnerLocationProperty =
        AvaloniaProperty.Register<ButtonSpinner, ButtonSpinnerLocation>(nameof(ButtonSpinnerLocation), ButtonSpinnerLocation.Right);

    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<ButtonSpinner>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<ButtonSpinner>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<object?> InnerLeftContentProperty =
        AvaloniaProperty.Register<ButtonSpinner, object?>(nameof(InnerLeftContent));
    
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<ButtonSpinner, IDataTemplate?>(nameof(InnerLeftContentTemplate));

    public static readonly StyledProperty<object?> InnerRightContentProperty =
        AvaloniaProperty.Register<ButtonSpinner, object?>(nameof(InnerRightContent));
    
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<ButtonSpinner, IDataTemplate?>(nameof(InnerRightContentTemplate));

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<ButtonSpinner>();
    
    public static readonly StyledProperty<bool> IsButtonSpinnerFloatableProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof (IsButtonSpinnerFloatable), false);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ButtonSpinner>();

    public bool AllowSpin
    {
        get => GetValue(AllowSpinProperty);
        set => SetValue(AllowSpinProperty, value);
    }

    public bool ShowButtonSpinner
    {
        get => GetValue(ShowButtonSpinnerProperty);
        set => SetValue(ShowButtonSpinnerProperty, value);
    }

    public ButtonSpinnerLocation ButtonSpinnerLocation
    {
        get => GetValue(ButtonSpinnerLocationProperty);
        set => SetValue(ButtonSpinnerLocationProperty, value);
    }
    
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }
        
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(InnerLeftContentTemplate))]
    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }
    
    public IDataTemplate? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }
    
    [DependsOn(nameof(InnerRightContentTemplate))]
    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    public IDataTemplate? InnerRightContentTemplate
    {
        get => GetValue(InnerRightContentTemplateProperty);
        set => SetValue(InnerRightContentTemplateProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public AddOnDecoratedVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public AddOnDecoratedStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public bool IsButtonSpinnerFloatable
    {
        get => GetValue(IsButtonSpinnerFloatableProperty);
        set => SetValue(IsButtonSpinnerFloatableProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> SpinnerHandleWidthProperty =
        AvaloniaProperty.Register<ButtonSpinner, double>(nameof (SpinnerHandleWidth));
    
    internal double SpinnerHandleWidth
    {
        get => GetValue(SpinnerHandleWidthProperty);
        set => SetValue(SpinnerHandleWidthProperty, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ButtonSpinnerToken.ID;

    #endregion
    
    private ButtonSpinnerDecoratedBox? _decoratedBox;
    private CompositeDisposable? _addOnBindingDisposables;

    static ButtonSpinner()
    {
        AllowSpinProperty.Changed.Subscribe(AllowSpinChanged);
    }
    
    public ButtonSpinner()
    {
        this.RegisterResources();
    }
    
    private IconButton? _decreaseButton;
    private IconButton? DecreaseButton
    {
        get => _decreaseButton;
        set
        {
            if (_decreaseButton != null)
            {
                _decreaseButton.Click -= HandleButtonClick;
            }
            _decreaseButton = value;
            if (_decreaseButton != null)
            {
                _decreaseButton.Click += HandleButtonClick;
            }
        }
    }
    
    private IconButton? _increaseButton;
    private IconButton? IncreaseButton
    {
        get => _increaseButton;
        set
        {
            if (_increaseButton != null)
            {
                _increaseButton.Click -= HandleButtonClick;
            }
            _increaseButton = value;
            if (_increaseButton != null)
            {
                _increaseButton.Click += HandleButtonClick;
            }
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var borderThickness = _decoratedBox?.BorderThickness ?? default;
        return base.ArrangeOverride(finalSize).Inflate(borderThickness);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == LeftAddOnProperty ||
                change.Property == RightAddOnProperty ||
                change.Property == InnerLeftContentProperty ||
                change.Property == InnerRightContentProperty)
            {
                ConfigureAddOns();
            }
        }
        
        if (change.Property == ButtonSpinnerLocationProperty)
        {
            UpdatePseudoClasses();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _decoratedBox = e.NameScope.Find<ButtonSpinnerDecoratedBox>(ButtonSpinnerThemeConstants.DecoratedBoxPart);
        base.OnApplyTemplate(e);
        if (_decoratedBox?.SpinnerContent is ButtonSpinnerHandle spinnerHandle)
        {
            spinnerHandle.ButtonsCreated += (sender, args) =>
            {
                IncreaseButton = spinnerHandle.IncreaseButton;
                DecreaseButton = spinnerHandle.DecreaseButton;
            };
        }
        SetButtonUsage();
        ConfigureAddOns();
    }

    private void ConfigureAddOns()
    {
        _addOnBindingDisposables?.Dispose();
        _addOnBindingDisposables = new CompositeDisposable();
        if (LeftAddOn is PathIcon leftAddOnIcon)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter()
            {
                Icon = leftAddOnIcon
            };
            _addOnBindingDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, iconPresenter, SizeTypeProperty));
            LeftAddOn = iconPresenter;
        }
        if (InnerLeftContent is PathIcon innerLeftContent)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter()
            {
                Icon = innerLeftContent
            };
            _addOnBindingDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, iconPresenter, SizeTypeProperty));
            InnerLeftContent = iconPresenter;
        }
        if (RightAddOn is PathIcon rightAddOnIcon)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter()
            {
                Icon = rightAddOnIcon
            };
            _addOnBindingDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, iconPresenter, SizeTypeProperty));
            RightAddOn = iconPresenter;
        }
        if (InnerRightContent is PathIcon innerRightContent)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter()
            {
                Icon = innerRightContent
            };
            _addOnBindingDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, iconPresenter, SizeTypeProperty));
            InnerRightContent = iconPresenter;
        }
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ButtonSpinnerPseudoClass.Left, ButtonSpinnerLocation == ButtonSpinnerLocation.Left);
        PseudoClasses.Set(ButtonSpinnerPseudoClass.Right, ButtonSpinnerLocation == ButtonSpinnerLocation.Right);
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        Point mousePosition;
        if (IncreaseButton != null && IncreaseButton.IsEnabled == false)
        {
            mousePosition = e.GetPosition(IncreaseButton);
            if (mousePosition.X > 0 && mousePosition.X < IncreaseButton.Width &&
                mousePosition.Y > 0 && mousePosition.Y < IncreaseButton.Height)
            {
                e.Handled = true;
            }
        }

        if (DecreaseButton != null && DecreaseButton.IsEnabled == false)
        {
            mousePosition = e.GetPosition(DecreaseButton);
            if (mousePosition.X > 0 && mousePosition.X < DecreaseButton.Width &&
                mousePosition.Y > 0 && mousePosition.Y < DecreaseButton.Height)
            {
                e.Handled = true;
            }
        }
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        // If XY navigation is enabled - do not spin with arrow keys, instead use spinner buttons.
        if (this.IsAllowedXYNavigationMode(e.KeyDeviceType))
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Up:
            {
                if (AllowSpin)
                {
                    OnSpin(new SpinEventArgs(SpinEvent, SpinDirection.Increase));
                    e.Handled = true;
                }
                break;
            }
            case Key.Down:
            {
                if (AllowSpin)
                {
                    OnSpin(new SpinEventArgs(SpinEvent, SpinDirection.Decrease));
                    e.Handled = true;
                }
                break;
            }
            case Key.Enter:
            {
                //Do not Spin on enter Key when spinners have focus
                if (((IncreaseButton != null) && (IncreaseButton.IsFocused))
                    || ((DecreaseButton != null) && DecreaseButton.IsFocused))
                {
                    e.Handled = true;
                }
                break;
            }
        }
    }
    
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (AllowSpin && IsKeyboardFocusWithin)
        {
            if (e.Delta.Y != 0)
            {
                var spinnerEventArgs = new SpinEventArgs(SpinEvent, (e.Delta.Y < 0) ? SpinDirection.Decrease : SpinDirection.Increase, true);
                OnSpin(spinnerEventArgs);
                e.Handled = true;
            }
        }
    }
    
    protected override void OnValidSpinDirectionChanged(ValidSpinDirections oldValue, ValidSpinDirections newValue)
    {
        SetButtonUsage();
    }
    
    protected virtual void OnAllowSpinChanged(bool oldValue, bool newValue)
    {
        SetButtonUsage();
    }
    
    private static void AllowSpinChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Sender is ButtonSpinner spinner)
        {
            var oldValue = (bool)change.OldValue!;
            var newValue = (bool)change.NewValue!;
            spinner.OnAllowSpinChanged(oldValue, newValue);
        }
    }
    
    private void SetButtonUsage()
    {
        if (IncreaseButton != null)
        {
            IncreaseButton.IsEnabled = AllowSpin && ((ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase);
        }

        if (DecreaseButton != null)
        {
            DecreaseButton.IsEnabled = AllowSpin && ((ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease);
        }
    }
    
    private void HandleButtonClick(object? sender, RoutedEventArgs e)
    {
        if (AllowSpin)
        {
            var direction = sender == IncreaseButton ? SpinDirection.Increase : SpinDirection.Decrease;
            OnSpin(new SpinEventArgs(SpinEvent, direction));
        }
    }
}