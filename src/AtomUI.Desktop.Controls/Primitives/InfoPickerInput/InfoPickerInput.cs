using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls.Primitives;

[PseudoClasses(InfoPickerPseudoClass.Choosing, InfoPickerPseudoClass.FlyoutOpen)]
public abstract class InfoPickerInput : TemplatedControl,
                                        IMotionAwareControl,
                                        ICompactSpaceAware,
                                        IFormItemAware,
                                        IInputControlStatusAware,
                                        IInputControlStyleVariantAware,
                                        ICustomizableSizeTypeAware,
                                        IFormItemFeedbackAware
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<InfoPickerInput>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<InfoPickerInput>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<InfoPickerInput>();
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<InfoPickerInput>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<InfoPickerInput>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<InfoPickerInput, string?>(nameof(PlaceholderText));
    
    public static readonly StyledProperty<PathIcon?> InfoIconProperty =
        AvaloniaProperty.Register<InfoPickerInput, PathIcon?>(nameof(InfoIcon));

    public static readonly StyledProperty<PlacementMode> PickerPlacementProperty =
        AvaloniaProperty.Register<InfoPickerInput, PlacementMode>(nameof(PickerPlacement),
            PlacementMode.BottomEdgeAlignedLeft,
            coerce:CoercePickerPlacement);

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        Flyout.IsPointAtCenterProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        Avalonia.Controls.TextBox.IsReadOnlyProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<InfoPickerInput>();
    
    public static readonly StyledProperty<Control?> PickerPresenterProperty =
        AvaloniaProperty.Register<InfoPickerInput, Control?>(nameof(PickerPresenter));
    
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
    
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }
    
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }
    
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    public PathIcon? InfoIcon
    {
        get => GetValue(InfoIconProperty);
        set => SetValue(InfoIconProperty, value);
    }

    public PlacementMode PickerPlacement
    {
        get => GetValue(PickerPlacementProperty);
        set => SetValue(PickerPlacementProperty, value);
    }

    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public Control? PickerPresenter
    {
        get => GetValue(PickerPresenterProperty);
        set => SetValue(PickerPresenterProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<InfoPickerInput, string?>(nameof(Text));
    
    internal static readonly DirectProperty<InfoPickerInput, double> PreferredInputWidthProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, double>(nameof(PreferredInputWidth),
            o => o.PreferredInputWidth,
            (o, v) => o.PreferredInputWidth = v);
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsClearButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsClearButtonVisible),
            o => o.IsClearButtonVisible,
            (o, v) => o.IsClearButtonVisible = v);
    
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<InfoPickerInput>();

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly StyledProperty<IFormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<InfoPickerInput, IFormValidateFeedback?>(nameof (FormFeedback));
    
    internal static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<InfoPickerInput, bool>(nameof(ShouldUseOverlayPopup), true);

    public static readonly StyledProperty<bool> IsPickerOpenProperty =
        AvaloniaProperty.Register<InfoPickerInput, bool>(nameof(IsPickerOpen), coerce: CoerceIsPickerOpen);

    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsArrowVisibleEffectiveProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsArrowVisibleEffective),
            o => o.IsArrowVisibleEffective,
            (o, v) => o.IsArrowVisibleEffective = v);

    internal static readonly DirectProperty<InfoPickerInput, InputControlStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, InputControlStatus>(
            nameof(EffectiveStatus),
            o => o.EffectiveStatus,
            (o, v) => o.EffectiveStatus = v);
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsPopupHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsPopupHorizontalFlipped),
            o => o.IsPopupHorizontalFlipped,
            (o, v) => o.IsPopupHorizontalFlipped = v);
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsPopupVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsPopupVerticalFlipped),
            o => o.IsPopupVerticalFlipped,
            (o, v) => o.IsPopupVerticalFlipped = v);
    
    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<InfoPickerInput>();

    internal string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private double _preferredInputWidth = double.NaN;

    internal double PreferredInputWidth
    {
        get => _preferredInputWidth;
        set => SetAndRaise(PreferredInputWidthProperty, ref _preferredInputWidth, value);
    }

    private bool _isClearButtonVisible;

    internal bool IsClearButtonVisible
    {
        get => _isClearButtonVisible;
        set => SetAndRaise(IsClearButtonVisibleProperty, ref _isClearButtonVisible, value);
    }

    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }
    
    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }
    
    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }
    
    internal IFormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }
    
    internal bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    public bool IsPickerOpen
    {
        get => GetValue(IsPickerOpenProperty);
        set => SetValue(IsPickerOpenProperty, value);
    }

    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    
    private bool _isArrowVisibleEffective;

    internal bool IsArrowVisibleEffective
    {
        get => _isArrowVisibleEffective;
        private set => SetAndRaise(IsArrowVisibleEffectiveProperty, ref _isArrowVisibleEffective, value);
    }

    private InputControlStatus _effectiveStatus;

    internal InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    private bool _isPopupHorizontalFlipped;

    internal bool IsPopupHorizontalFlipped
    {
        get => _isPopupHorizontalFlipped;
        private set => SetAndRaise(IsPopupHorizontalFlippedProperty, ref _isPopupHorizontalFlipped, value);
    }
    
    private bool _isPopupVerticalFlipped;

    internal bool IsPopupVerticalFlipped
    {
        get => _isPopupVerticalFlipped;
        private set => SetAndRaise(IsPopupVerticalFlippedProperty, ref _isPopupVerticalFlipped, value);
    }
    
    internal ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    #endregion

    private protected AddOnDecoratedBox? DecoratedBox;
    private protected PickerClearUpButton? PickerClearUpButton;
    private CompositeDisposable? _contentRightAddOnBindings;
    private protected Popup? PickerPopup;
    protected bool CurrentValidSelected;
    protected TextBox? InfoInputBox;
    protected Control? PickerInnerBox;

    private protected bool IsChoosing;
    private AddOnDecoratedBox? _addOnDecoratedBox;
    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;
    private int _popupLifecycleCloseDepth;
    private Control? _ownedPickerPresenter;
    

    static InfoPickerInput()
    {
        AffectsMeasure<InfoPickerInput>(PreferredInputWidthProperty, SizeTypeProperty);
        IsPickerOpenProperty.Changed.AddClassHandler<InfoPickerInput>((picker, args) => picker.HandleIsPickerOpenChanged(args));
    }

    public InfoPickerInput()
    {
    }

    public virtual void Clear()
    {
        InfoInputBox?.Clear();
    }

    public void ClosePickerFlyout()
    {
        SetPickerOpenIfChanged(false);
    }

    private void HandleIsPickerOpenChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var isOpen = args.NewValue is true;
        if (isOpen)
        {
            EnsurePickerPresenter();
            CurrentValidSelected = false;
            NotifyFlyoutAboutToShow();
            NotifyPickerOpened();
        }
        else
        {
            NotifyFlyoutAboutToClose(CurrentValidSelected);
            NotifyPickerClosed();
        }
        UpdatePseudoClasses();
        ApplyPopupOpenState(isOpen);
    }

    private void HandleFlyoutAboutToShow(object? sender, EventArgs args)
    {
    }

    protected virtual void NotifyFlyoutAboutToShow()
    {
    }

    private void HandleFlyoutAboutToClose(object? sender, EventArgs args)
    {
    }

    protected virtual void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
    }

    private void HandleFlyoutOpened(object? sender, EventArgs args)
    {
    }

    protected virtual void NotifyPickerOpened()
    {
        NotifyFlyoutOpened();
    }

    protected virtual void NotifyFlyoutOpened()
    {
    }

    private void HandleFlyoutClosed(object? sender, EventArgs args)
    {
    }

    protected virtual void NotifyPickerClosed()
    {
        NotifyFlyoutClosed();
    }

    protected virtual void NotifyFlyoutClosed()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _popupPinnedOpenBinding?.Dispose();
        _popupPinnedOpenBinding = null;

        if (PickerPopup != null)
        {
            PickerPopup.Closed -= HandlePickerPopupClosed;
        }

        if (DecoratedBox != null)
        {
            DecoratedBox.TemplateApplied -= HandleDecoratedBoxTemplateApplied;
            DecoratedBox.PropertyChanged -= HandleDecoratedBoxPropertyChanged;
        }

        if (PickerClearUpButton is not null)
        {
            PickerClearUpButton.ClearRequest -= HandleClearRequest;
        }

        base.OnApplyTemplate(e);

        PickerPopup = e.NameScope.Find<Popup>("PART_Popup");

        DecoratedBox = e.NameScope.Get<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        InfoInputBox = e.NameScope.Get<TextBox>("PART_InfoInputBox");
        PickerClearUpButton = e.NameScope.Find<PickerClearUpButton>("PART_ClearUpButton");
        if (PickerPopup != null && InfoInputBox != null)
        {
            // 与 AutoComplete/Select 一致:始终指定穿透元素,保证遮罩存在时输入框仍可交互。
            PickerPopup.OverlayInputPassThroughElement = InfoInputBox;
        }
        if (PickerPopup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                PickerPopup,
                Popup.IsPopupPinnedOpenProperty);
            PickerPopup.Closed += HandlePickerPopupClosed;

        }
        if (DecoratedBox != null)
        {
            KeyboardNavigation.SetTabOnceActiveElement(this, DecoratedBox);
            DecoratedBox.TemplateApplied += HandleDecoratedBoxTemplateApplied;
            DecoratedBox.PropertyChanged += HandleDecoratedBoxPropertyChanged;
        }

        if (PickerClearUpButton is not null)
        {
            PickerClearUpButton.ClearRequest += HandleClearRequest;
        }

        _addOnDecoratedBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);

        RelayFrameBorderBrush();
        SetupPopupProperties();
        SetupContentRightAddOnBindings(e);
        ConfigureArrowPosition();
        ConfigureShowArrowEffective();
        if (IsPickerOpen)
        {
            EnsurePickerPresenter();
            ApplyPopupOpenState(true);
        }
    }
    
    private void HandleDecoratedBoxTemplateApplied(object? sender, TemplateAppliedEventArgs args)
    {
        PickerInnerBox = DecoratedBox!.ContentFrame;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs args)
    {
        base.OnPointerReleased(args);

        if (!args.Handled && !IsReadOnly && args.Source is Visual source)
        {
            // Check if click is inside the input area (not in popup)
            if (PickerPopup?.IsInsidePopup(source) != true)
            {
                if (IsPointerInInfoInputBox(args.GetPosition(this)) &&
                    !ClickInClearUpButtonWithClearMode(args))
                {
                    SetPickerOpenIfChanged(true);
                }
                else
                {
                    SetPickerOpenIfChanged(false);
                }
                args.Handled = true;
            }
        }
    }
    
    protected bool ClickInClearUpButtonWithClearMode(PointerReleasedEventArgs args)
    {
        if (PickerClearUpButton != null)
        {
            var sourceControl = args.Source as Control;
            return sourceControl.FindLogicalAncestorOfType<InputClearIconButton>() is not null;
        }
        return false;
    }

    private bool IsPointerInInfoInputBox(Point position)
    {
        if (PickerInnerBox is not null)
        {
            var pos = PickerInnerBox.TranslatePoint(new Point(0, 0), this);
            if (!pos.HasValue)
            {
                return false;
            }
        
            var targetWidth  = PickerInnerBox.Bounds.Width;
            var targetHeight = PickerInnerBox.Bounds.Height;
            var startOffsetX = pos.Value.X;
            var endOffsetX   = startOffsetX + targetWidth;
            var offsetY      = pos.Value.Y;
            if (ContentLeftAddOn is Control leftContent)
            {
                var leftContentPos = leftContent.TranslatePoint(new Point(0, 0), this);
                if (leftContentPos.HasValue)
                {
                    startOffsetX = leftContentPos.Value.X + leftContent.Bounds.Width;
                }
            }
            
            targetWidth = endOffsetX - startOffsetX;
            var bounds = new Rect(new Point(startOffsetX, offsetY), new Size(targetWidth, targetHeight));
            return bounds.Contains(position);
        }
    
        return false;
    }

    private void SetupContentRightAddOnBindings(TemplateAppliedEventArgs e)
    {
        _contentRightAddOnBindings?.Dispose();
        _contentRightAddOnBindings = new CompositeDisposable();

        if (PickerClearUpButton is { } clearUpButton)
        {
            _contentRightAddOnBindings.Add(BindUtils.RelayBind(this, IsClearButtonVisibleProperty, clearUpButton,
                PickerClearUpButton.IsInClearModeProperty));
            _contentRightAddOnBindings.Add(BindUtils.RelayBind(this, InfoIconProperty, clearUpButton,
                PickerClearUpButton.IconProperty));
            _contentRightAddOnBindings.Add(BindUtils.RelayBind(this, FormFeedbackProperty, clearUpButton,
                PickerClearUpButton.FormFeedbackProperty));
            _contentRightAddOnBindings.Add(BindUtils.RelayBind(this, InfoIconProperty, clearUpButton,
                Visual.IsVisibleProperty, value => value is not null));
        }

        if (e.NameScope.Find<ContentPresenter>("PART_ContentRightAddOnPresenter") is { } contentPresenter)
        {
            _contentRightAddOnBindings.Add(BindUtils.RelayBind(this, ContentRightAddOnProperty, contentPresenter,
                ContentPresenter.ContentProperty));
            _contentRightAddOnBindings.Add(BindUtils.RelayBind(this, ContentRightAddOnTemplateProperty,
                contentPresenter, ContentPresenter.ContentTemplateProperty));
        }
    }

    protected virtual void ConfigureIsClearButtonVisible()
    {
        if (DecoratedBox is not null)
        {
            SetClearButtonVisibleIfChanged(
                DecoratedBox.IsInnerBoxHover &&
                InfoInputBox?.IsReadOnly == false &&
                InfoInputBox.Text?.Length > 0);
        }
    }

    protected virtual void NotifyClearButtonClicked()
    {
        Clear();
        SetClearButtonVisibleIfChanged(false);
    }

    protected virtual void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        NotifyFlyoutPresenterCreated(pickerPresenter);
    }

    protected virtual void NotifyPickerPresenterCleared(Control pickerPresenter)
    {
        NotifyFlyoutPresenterCleared(pickerPresenter);
    }

    protected virtual void NotifyFlyoutPresenterCreated(Control flyoutPresenter)
    {
    }

    protected virtual void NotifyFlyoutPresenterCleared(Control flyoutPresenter)
    {
    }

    private void EnsurePickerPresenter()
    {
        if (PickerPresenter is not null)
        {
            return;
        }

        var pickerPresenter = CreatePickerPresenter();
        _ownedPickerPresenter = pickerPresenter;
        SetCurrentValue(PickerPresenterProperty, pickerPresenter);
        NotifyPickerPresenterCreated(pickerPresenter);
    }

    private void ClearOwnedPickerPresenter()
    {
        var pickerPresenter = _ownedPickerPresenter;
        if (pickerPresenter is null)
        {
            return;
        }

        if (IsPickerOpen)
        {
            SetPickerOpenIfChanged(false);
        }

        NotifyPickerPresenterCleared(pickerPresenter);
        if (ReferenceEquals(PickerPresenter, pickerPresenter))
        {
            SetCurrentValue(PickerPresenterProperty, null);
        }
        _ownedPickerPresenter = null;
    }

    protected virtual void SetupPopupProperties()
    {
        SetupFlyoutProperties();
    }

    protected virtual void SetupFlyoutProperties()
    {
    }

    protected abstract Control CreatePickerPresenter();

    protected void SetPickerOpenIfChanged(bool isOpen)
    {
        if (IsPickerOpen != isOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, isOpen);
        }
    }

    protected void SetClearButtonVisibleIfChanged(bool isVisible)
    {
        if (IsClearButtonVisible != isVisible)
        {
            SetCurrentValue(IsClearButtonVisibleProperty, isVisible);
        }
    }

    protected virtual Flyout CreatePickerFlyout()
    {
        throw new NotImplementedException("Use CreatePickerPresenter instead");
    }

    protected virtual void UpdatePseudoClasses()
    {
        PseudoClasses.Set(InfoPickerPseudoClass.FlyoutOpen, IsPickerOpen);
        PseudoClasses.Set(InfoPickerPseudoClass.Choosing, IsChoosing);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen)
        {
            SetPickerOpenIfChanged(true);
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetPickerOpenIfChanged(false);
    }

    private void HandleDecoratedBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == AddOnDecoratedBox.IsInnerBoxHoverProperty)
        {
            ConfigureIsClearButtonVisible();
        }
    }

    private void HandleClearRequest(object? sender, EventArgs args)
    {
        NotifyClearButtonClicked();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ClosePickerForLifecycle();
        base.OnDetachedFromVisualTree(e);

        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;

        if (DecoratedBox != null)
        {
            DecoratedBox.PropertyChanged -= HandleDecoratedBoxPropertyChanged;
            DecoratedBox.TemplateApplied -= HandleDecoratedBoxTemplateApplied;
        }

        if (PickerClearUpButton is not null)
        {
            PickerClearUpButton.ClearRequest -= HandleClearRequest;
        }

        ClearOwnedPickerPresenter();
    }

    private void ClosePickerForLifecycle()
    {
        try
        {
            ++_popupLifecycleCloseDepth;
            PickerPopup?.CloseForLifecycle();
            SetPickerOpenIfChanged(false);
        }
        finally
        {
            --_popupLifecycleCloseDepth;
        }
    }

    protected virtual bool ShowClearButtonPredicate()
    {
        return false;
    }
    
    private static PlacementMode CoercePickerPlacement(object o, PlacementMode value)
    {
        if (value == PlacementMode.Pointer ||
            value == PlacementMode.Custom ||
            value == PlacementMode.AnchorAndGravity ||
            value == PlacementMode.Center) 
        {
            return PlacementMode.BottomEdgeAlignedLeft;
        }

        return value;
    }

    private static bool CoerceIsPickerOpen(AvaloniaObject sender, bool value)
    {
        return !value &&
               sender is InfoPickerInput { IsPopupPinnedOpen: true, _popupLifecycleCloseDepth: 0 }
            ? true
            : value;
    }
    
    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        var isUsedInCompactSpace = position != null;
        if (IsUsedInCompactSpace != isUsedInCompactSpace)
        {
            IsUsedInCompactSpace = isUsedInCompactSpace;
        }

        if (CompactSpaceItemPosition != position)
        {
            CompactSpaceItemPosition = position;
        }
    }
    
    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        if (CompactSpaceOrientation != orientation)
        {
            CompactSpaceOrientation = orientation;
        }
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        return GetBorderThicknessForCompactSpace();
    }
    
    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        if (_addOnDecoratedBox == null || _addOnDecoratedBox.StyleVariant != InputControlStyleVariant.Outlined)
        {
            return 0.0;
        }

        // 都一样宽
        return _addOnDecoratedBox.InnerBoxBorderThickness.Left;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderBrushProperty)
        {
            RelayFrameBorderBrush();
        }

        if (change.Property == IsArrowVisibleProperty ||
            change.Property == PickerPlacementProperty)
        {
            ConfigureShowArrowEffective();
        }

        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdateEffectiveStatus();
        }

        if (change.Property == PickerPlacementProperty ||
            change.Property == IsPopupHorizontalFlippedProperty ||
            change.Property == IsPopupVerticalFlippedProperty)
        {
            ConfigureArrowPosition();
        }

        if (change.Property == IsPopupPinnedOpenProperty)
        {

            if (change.GetNewValue<bool>())
            {
                SetPickerOpenIfChanged(true);
            }
            else if (IsPickerOpen)
            {
                if (PickerPopup is { IsOpen: true })
                {
                    SetCurrentValue(IsPickerOpenProperty, true);
                }
                else
                {
                    ClosePickerForLifecycle();
                }
            }
        }
    }


    private void ApplyPopupOpenState(bool isOpen)
    {
        if (PickerPopup is null)
        {
            return;
        }

        if (PickerPopup.IsOpen != isOpen)
        {
            PickerPopup.IsOpen = isOpen;
        }
    }

    private void HandlePickerPopupClosed(object? sender, EventArgs e)
    {
        // 弹层关闭(含 light-dismiss 路径)时回写宿主状态,保持 IsPickerOpen 与弹层同步。
        if (IsPickerOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, false);
        }
    }

    /// <summary>
    /// 将 owner 根的 BorderBrush 以 LocalValue 中继到 AddOnDecoratedBox,使 root 级
    /// Semantic 定制(对齐 antd styles.root.border)能覆盖输入框状态机的默认描边;
    /// owner 未设置时清除中继并恢复状态机。中继必须使用 LocalValue 优先级才能胜过
    /// 框体主题的状态触发器,ControlTheme 的 TemplateBinding 无法达到该优先级,
    /// 因此放在控件代码中。
    /// </summary>
    private void RelayFrameBorderBrush()
    {
        if (_addOnDecoratedBox is not { } decoratedBox)
        {
            return;
        }

        var value = GetValue(BorderBrushProperty);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            decoratedBox.ClearValue(BorderBrushProperty);
        }
        else
        {
            decoratedBox.SetValue(BorderBrushProperty, value, BindingPriority.LocalValue);
        }
    }

    private void UpdateEffectiveStatus()
    {
        var effectiveStatus = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus);
        if (EffectiveStatus != effectiveStatus)
        {
            EffectiveStatus = effectiveStatus;
        }
    }
    
    protected void ConfigureShowArrowEffective()
    {
        var isArrowVisibleEffective = IsArrowVisible && PopupUtils.CanEnabledArrow(PickerPlacement);
        if (IsArrowVisibleEffective != isArrowVisibleEffective)
        {
            SetCurrentValue(IsArrowVisibleEffectiveProperty, isArrowVisibleEffective);
        }
    }

    protected void ConfigureArrowPosition()
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(PickerPlacement, null, null);
        if (arrowPosition.HasValue)
        {
            arrowPosition = ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, IsPopupHorizontalFlipped, IsPopupVerticalFlipped);
            if (ArrowPosition != arrowPosition)
            {
                SetCurrentValue(ArrowPositionProperty, arrowPosition);
            }
        }
    }
    
    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);
    
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(object? value)
    {
    }

    protected virtual object? NotifyGetFormValue()
    {
        return null;
    }

    protected virtual void NotifyClearFormValue()
    {
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }
    
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            FormFeedback = value;
        }
    }
    #endregion
}
