using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input.Raw;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls.Primitives;

[PseudoClasses(InfoPickerPseudoClass.Choosing, InfoPickerPseudoClass.FlyoutOpen)]
public abstract class InfoPickerInput : TemplatedControl,
                                        IMotionAwareControl,
                                        IControlSharedTokenResourcesHost
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

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<InfoPickerInput, string?>(nameof(Watermark));
    
    public static readonly StyledProperty<PathIcon?> InfoIconProperty =
        AvaloniaProperty.Register<InfoPickerInput, PathIcon?>(nameof(InfoIcon));

    public static readonly StyledProperty<PlacementMode> PickerPlacementProperty =
        AvaloniaProperty.Register<InfoPickerInput, PlacementMode>(nameof(PickerPlacement),
            PlacementMode.BottomEdgeAlignedLeft,
            coerce:CoercePickerPlacement);

    public static readonly StyledProperty<bool> IsShowArrowProperty =
        ArrowDecoratedBox.IsShowArrowProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        Flyout.IsPointAtCenterProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        Avalonia.Controls.TextBox.IsReadOnlyProperty.AddOwner<InfoPickerInput>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<InfoPickerInput>();

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

    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
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

    public bool IsShowArrow
    {
        get => GetValue(IsShowArrowProperty);
        set => SetValue(IsShowArrowProperty, value);
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

    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
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
    
    protected string? Text
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

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => InfoPickerInputToken.ID;

    #endregion

    private protected AddOnDecoratedBox? DecoratedBox;
    private protected PickerClearUpButton? PickerClearUpButton;
    private protected readonly FlyoutStateHelper FlyoutStateHelper;
    private protected Flyout? PickerFlyout;
    protected bool CurrentValidSelected;
    protected TextBox? InfoInputBox;
    protected Border? PickerInnerBox;

    private protected bool IsFlyoutOpen;
    private protected bool IsChoosing;
    private CompositeDisposable? _flyoutBindingDisposables;
    private CompositeDisposable? _flyoutHelperBindingDisposables;

    static InfoPickerInput()
    {
        AffectsMeasure<InfoPickerInput>(PreferredInputWidthProperty, SizeTypeProperty);
    }

    public InfoPickerInput()
    {
        this.RegisterResources();
        FlyoutStateHelper = new FlyoutStateHelper
        {
            TriggerType = FlyoutTriggerType.Click
        };
        FlyoutStateHelper.FlyoutAboutToShow        += HandleFlyoutAboutToShow;
        FlyoutStateHelper.FlyoutAboutToClose       += HandleFlyoutAboutToClose;
        FlyoutStateHelper.FlyoutOpened             += HandleFlyoutOpened;
        FlyoutStateHelper.FlyoutClosed             += HandleFlyoutClosed;
        FlyoutStateHelper.OpenFlyoutPredicate      =  FlyoutOpenPredicate;
        FlyoutStateHelper.ClickHideFlyoutPredicate =  ClickHideFlyoutPredicate;
    }

    public virtual void Clear()
    {
        InfoInputBox?.Clear();
    }

    public void ClosePickerFlyout()
    {
        FlyoutStateHelper.HideFlyout(true);
    }

    private void HandleFlyoutAboutToShow(object? sender, EventArgs args)
    {
        CurrentValidSelected = false;
        NotifyFlyoutAboutToShow();
    }

    protected virtual void NotifyFlyoutAboutToShow()
    {
    }

    private void HandleFlyoutAboutToClose(object? sender, EventArgs args)
    {
        NotifyFlyoutAboutToClose(CurrentValidSelected);
    }

    protected virtual void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
    }

    private void HandleFlyoutOpened(object? sender, EventArgs args)
    {
        NotifyFlyoutOpened();
    }

    protected virtual void NotifyFlyoutOpened()
    {
    }

    private void HandleFlyoutClosed(object? sender, EventArgs args)
    {
        NotifyFlyoutClosed();
    }

    protected virtual void NotifyFlyoutClosed()
    {
    }

    protected virtual bool ClickHideFlyoutPredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        if (hostProvider.PopupHost != args.Root)
        {
            if (!IsPointerInInfoInputBox(args.Position) || ClickInClearUpButtonWithClearMode(args.Position))
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool FlyoutOpenPredicate(Point position)
    {
        if (!IsEnabled)
        {
            return false;
        }
        
        return IsPointerInInfoInputBox(position) && !ClickInClearUpButtonWithClearMode(position);
    }

    protected bool ClickInClearUpButtonWithNormalMode(Point position)
    {
        if (PickerClearUpButton != null)
        {
            var pos = PickerClearUpButton.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(this)!);
            if (pos.HasValue)
            {
                var clearUpButtonBounds = new Rect(pos.Value, PickerClearUpButton.Bounds.Size);
                if (clearUpButtonBounds.Contains(position) && !PickerClearUpButton.IsInClearMode)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    protected bool ClickInClearUpButtonWithClearMode(Point position)
    {
        if (PickerClearUpButton != null)
        {
            var pos = PickerClearUpButton.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(this)!);
            if (pos.HasValue)
            {
                var clearUpButtonBounds = new Rect(pos.Value, PickerClearUpButton.Bounds.Size);
                if (clearUpButtonBounds.Contains(position) && PickerClearUpButton.IsInClearMode)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsPointerInInfoInputBox(Point position)
    {
        if (PickerInnerBox is not null)
        {
            var pos = PickerInnerBox.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(this)!);
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
                var leftContentPos = leftContent.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(this)!);
                if (leftContentPos.HasValue)
                {
                    startOffsetX = leftContentPos.Value.X + leftContent.Bounds.Width;
                }
            }
            
            targetWidth = endOffsetX - startOffsetX;
            var bounds = new Rect(new Point(startOffsetX, offsetY), new Size(targetWidth, targetHeight));
            if (bounds.Contains(position))
            {
                return true;
            }
        }
    
        return false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (PickerFlyout is null)
        {
            PickerFlyout = CreatePickerFlyout();
            PickerFlyout.Opened += (sender, args) =>
            {
                IsFlyoutOpen = true;
                UpdatePseudoClasses();
            };
            PickerFlyout.Closed += (sender, args) =>
            {
                IsFlyoutOpen = false;
                UpdatePseudoClasses();
            };
            PickerFlyout.PresenterCreated += (sender, args) => { NotifyFlyoutPresenterCreated(args.Presenter); };
            FlyoutStateHelper.Flyout      =  PickerFlyout;
        }

        DecoratedBox = e.NameScope.Get<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        InfoInputBox = e.NameScope.Get<TextBox>(InfoPickerInputThemeConstants.InfoInputBoxPart);
        
        if (DecoratedBox != null)
        {
            if (DecoratedBox.ContentRightAddOn is Control rightContent)
            {
                PickerClearUpButton = rightContent.FindDescendantOfType<PickerClearUpButton>();
            }

            DecoratedBox.TemplateApplied += (sender, args) =>
            {
                PickerInnerBox                 = DecoratedBox.ContentFrame;
                FlyoutStateHelper.AnchorTarget = PickerInnerBox;
            };
            DecoratedBox.PropertyChanged += (sender, args) =>
            {
                if (args.Property == AddOnDecoratedBox.IsInnerBoxHoverProperty)
                {
                    ConfigureIsClearButtonVisible();
                }
            };
        }

        if (PickerClearUpButton is not null)
        {
            PickerClearUpButton.ClearRequest += (sender, args) => { NotifyClearButtonClicked(); };
        }
        
        SetupFlyoutProperties();
    }

    protected virtual void ConfigureIsClearButtonVisible()
    {
        if (DecoratedBox is not null)
        {
            SetCurrentValue(IsClearButtonVisibleProperty, DecoratedBox.IsInnerBoxHover && InfoInputBox?.IsReadOnly == false && InfoInputBox.Text?.Length > 0);
        }
    }

    protected virtual void NotifyClearButtonClicked()
    {
        Clear();
        SetCurrentValue(IsClearButtonVisibleProperty, false);
    }

    protected virtual void NotifyFlyoutPresenterCreated(Control flyoutPresenter)
    {
    }

    protected virtual void SetupFlyoutProperties()
    {
        if (PickerFlyout is not null)
        {
            _flyoutBindingDisposables?.Dispose();
            _flyoutBindingDisposables = new CompositeDisposable(5);
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PickerPlacementProperty, PickerFlyout, PopupFlyoutBase.PlacementProperty));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsShowArrowProperty, PickerFlyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsPointAtCenterProperty, PickerFlyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, MarginToAnchorProperty, PickerFlyout));
            _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, PickerFlyout, Flyout.IsMotionEnabledProperty));
        }
    }

    protected abstract Flyout CreatePickerFlyout();

    protected virtual void UpdatePseudoClasses()
    {
        PseudoClasses.Set(InfoPickerPseudoClass.FlyoutOpen, IsFlyoutOpen);
        PseudoClasses.Set(InfoPickerPseudoClass.Choosing, IsChoosing);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _flyoutHelperBindingDisposables?.Dispose();
        _flyoutHelperBindingDisposables = new CompositeDisposable(2);
        _flyoutHelperBindingDisposables.Add(BindUtils.RelayBind(this, MouseEnterDelayProperty, FlyoutStateHelper,
            FlyoutStateHelper.MouseEnterDelayProperty));
        _flyoutHelperBindingDisposables.Add(BindUtils.RelayBind(this, MouseLeaveDelayProperty, FlyoutStateHelper,
            FlyoutStateHelper.MouseLeaveDelayProperty));
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _flyoutHelperBindingDisposables?.Dispose();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        FlyoutStateHelper.NotifyAttachedToVisualTree();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        FlyoutStateHelper.NotifyDetachedFromVisualTree();
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
}