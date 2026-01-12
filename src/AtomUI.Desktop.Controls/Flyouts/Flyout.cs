using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using PopupControl = Popup;

public class Flyout : PopupFlyoutBase
{
    #region 公共属性定义

    /// <summary>
    /// 是否显示指示箭头
    /// </summary>
    public static readonly StyledProperty<bool> IsShowArrowProperty =
        ArrowDecoratedBox.IsShowArrowProperty.AddOwner<Flyout>();

    public static readonly StyledProperty<BoxShadows> MaskShadowsProperty =
        PopupControl.MaskShadowsProperty.AddOwner<Flyout>();

    /// <summary>
    /// 箭头是否始终指向中心
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(IsPointAtCenter));
    
    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<Flyout, object>(nameof(Content));
    
    public static readonly StyledProperty<ControlTheme?> FlyoutPresenterThemeProperty =
        AvaloniaProperty.Register<Flyout, ControlTheme?>(nameof(FlyoutPresenterTheme));

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
    
    public ControlTheme? FlyoutPresenterTheme
    {
        get => GetValue(FlyoutPresenterThemeProperty);
        set => SetValue(FlyoutPresenterThemeProperty, value);
    }
    
    [Content]
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public BoxShadows MaskShadows
    {
        get => GetValue(MaskShadowsProperty);
        set => SetValue(MaskShadowsProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<Flyout, bool> IsShowArrowEffectiveProperty =
        AvaloniaProperty.RegisterDirect<Flyout, bool>(nameof(IsShowArrowEffective),
            o => o.IsShowArrowEffective,
            (o, v) => o.IsShowArrowEffective = v);
    
    internal static readonly DirectProperty<Flyout, bool> IsPopupFlippedProperty =
        AvaloniaProperty.RegisterDirect<Flyout, bool>(nameof(IsPopupFlipped),
            o => o.IsPopupFlipped,
            (o, v) => o.IsPopupFlipped = v);
    
    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<Flyout>();
    
    private bool _isShowArrowEffective;

    internal bool IsShowArrowEffective
    {
        get => _isShowArrowEffective;
        private set => SetAndRaise(IsShowArrowEffectiveProperty, ref _isShowArrowEffective, value);
    }
    
    private bool _isPopupFlipped;

    internal bool IsPopupFlipped
    {
        get => _isPopupFlipped;
        private set => SetAndRaise(IsPopupFlippedProperty, ref _isPopupFlipped, value);
    }
    
    internal ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    #endregion

    private Classes? _classes;
    private CompositeDisposable? _presenterBindingDisposables;
    private CompositeDisposable? _popupBindingDisposables;
    
    public Classes FlyoutPresenterClasses => _classes ??= new Classes();

    protected CompositeDisposable? CompositeDisposable;

    static Flyout()
    {
        IsShowArrowProperty.OverrideDefaultValue<Flyout>(false);
    }
    
    public new static double GetHorizontalOffset(AvaloniaObject control)
    {
        return control.GetValue(HorizontalOffsetProperty);
    }
    
    public new static void SetHorizontalOffset(AvaloniaObject control, double value)
    {
        control.SetValue(HorizontalOffsetProperty, value);
    }
    
    public new static bool IsSetHorizontalOffset(AvaloniaObject control)
    {
        return control.IsSet(HorizontalOffsetProperty);
    }
    
    public new static double GetVerticalOffset(AvaloniaObject control)
    {
        return control.GetValue(VerticalOffsetProperty);
    }
    
    public new static void SetVerticalOffset(AvaloniaObject control, double value)
    {
        control.SetValue(VerticalOffsetProperty, value);
    }
    
    public new static bool IsSetVerticalOffset(AvaloniaObject control)
    {
        return control.IsSet(VerticalOffsetProperty);
    }

    protected override Control CreatePresenter()
    {
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new CompositeDisposable(4);
        var presenter = new FlyoutPresenter();
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ContentProperty, presenter, FlyoutPresenter.ContentProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, presenter, FlyoutPresenter.IsMotionEnabledProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsShowArrowEffectiveProperty, presenter, FlyoutPresenter.IsShowArrowProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ArrowPositionProperty, presenter, FlyoutPresenter.ArrowPositionProperty));
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        return presenter;
    }

    protected internal override void NotifyPopupCreated(Popup popup)
    {
        base.NotifyPopupCreated(popup);
        _popupBindingDisposables?.Dispose();
        _popupBindingDisposables = new CompositeDisposable(5);
        _popupBindingDisposables.Add(BindUtils.RelayBind(this, PlacementProperty, popup, Popup.PlacementProperty));
        _popupBindingDisposables.Add(BindUtils.RelayBind(this, PlacementAnchorProperty, popup, Popup.PlacementAnchorProperty));
        _popupBindingDisposables.Add(BindUtils.RelayBind(this, PlacementGravityProperty, popup, Popup.PlacementGravityProperty));
        _popupBindingDisposables.Add(BindUtils.RelayBind(this, MaskShadowsProperty, popup, Popup.MaskShadowsProperty));
        _popupBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, popup, Popup.IsMotionEnabledProperty));
        _popupBindingDisposables.Add(BindUtils.RelayBind(popup, Popup.IsFlippedProperty, this, IsPopupFlippedProperty));
    }

    protected override void OnOpening(CancelEventArgs args)
    {
        CompositeDisposable = new CompositeDisposable();
        if (Popup.Child is { } presenter)
        {
            if (_classes != null)
            {
                SetPresenterClasses(presenter, FlyoutPresenterClasses);
            }

            if (FlyoutPresenterTheme is { } theme)
            {
                presenter.SetValue(StyledElement.ThemeProperty, theme);
            }
        }

        base.OnOpening(args);

        CompositeDisposable.Add(TokenResourceBinder.CreateGlobalTokenBinding(this, MotionDurationProperty,
            SharedTokenKey.MotionDurationMid));
        CompositeDisposable.Add(TokenResourceBinder.CreateGlobalTokenBinding(this, MaskShadowsProperty,
            SharedTokenKey.BoxShadowsSecondary));
    }

    protected override void OnClosed()
    {
        base.OnClosed();
        CompositeDisposable?.Dispose();
    }

    private Point CalculatePopupPositionDelta(Control anchorTarget,
                                              Control? flyoutPresenter,
                                              PlacementMode placement,
                                              PopupAnchor? anchor = null,
                                              PopupGravity? gravity = null)
    {
        var offsetX = 0d;
        var offsetY = 0d;
        if (IsShowArrow && IsPointAtCenter)
        {
            if (PopupUtils.CanEnabledArrow(placement, anchor, gravity))
            {
                if (flyoutPresenter is ArrowDecoratedBox arrowDecoratedBox)
                {
                    var arrowVertexPoint = arrowDecoratedBox.ArrowVertexPoint;

                    var anchorSize = anchorTarget.Bounds.Size;
                    var centerX    = anchorSize.Width / 2;
                    var centerY    = anchorSize.Height / 2;
                    // 这里计算不需要全局坐标
                    if (placement == PlacementMode.TopEdgeAlignedLeft ||
                        placement == PlacementMode.BottomEdgeAlignedLeft)
                    {
                        offsetX += centerX - arrowVertexPoint.Item1;
                    }
                    else if (placement == PlacementMode.TopEdgeAlignedRight ||
                             placement == PlacementMode.BottomEdgeAlignedRight)
                    {
                        offsetX -= centerX - arrowVertexPoint.Item2;
                    }
                    else if (placement == PlacementMode.RightEdgeAlignedTop ||
                             placement == PlacementMode.LeftEdgeAlignedTop)
                    {
                        offsetY += centerY - arrowVertexPoint.Item1;
                    }
                    else if (placement == PlacementMode.RightEdgeAlignedBottom ||
                             placement == PlacementMode.LeftEdgeAlignedBottom)
                    {
                        offsetY -= centerY - arrowVertexPoint.Item2;
                    }
                }
            }
        }

        return new Point(offsetX, offsetY);
    }

    // 因为在某些 placement 下箭头是不能显示的
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowArrowProperty ||
            change.Property == PlacementProperty ||
            change.Property == PlacementAnchorProperty ||
            change.Property == PlacementGravityProperty)
        {
            ConfigureShowArrowEffective();
        }

        if (change.Property == PlacementProperty ||
            change.Property == IsPopupFlippedProperty)
        {
            ConfigureArrowPosition();
        }
    }

    protected void ConfigureShowArrowEffective()
    {
        if (!IsShowArrow)
        {
            SetCurrentValue(IsShowArrowEffectiveProperty, false);
        }
        else
        {
            SetCurrentValue(IsShowArrowEffectiveProperty, PopupUtils.CanEnabledArrow(Placement, PlacementAnchor, PlacementGravity));
        }
    }

    protected void ConfigureArrowPosition()
    {
        var placement = Placement;
        var anchor    = PlacementAnchor;
        var gravity   = PlacementGravity;
        
        var arrowPosition = PopupUtils.CalculateArrowPosition(placement, anchor, gravity);
        if (arrowPosition.HasValue)
        {
            if (IsPopupFlipped)
            {
                if (arrowPosition == ArrowPosition.Top)
                {
                    arrowPosition = ArrowPosition.Bottom;
                }
                else if (arrowPosition == ArrowPosition.Bottom)
                {
                    arrowPosition = ArrowPosition.Top;
                }
                else if (arrowPosition == ArrowPosition.Left)
                {
                    arrowPosition = ArrowPosition.Right;
                }
                else if (arrowPosition == ArrowPosition.Right)
                {
                    arrowPosition = ArrowPosition.Left;
                }
                else if (arrowPosition == ArrowPosition.TopEdgeAlignedLeft)
                {
                    arrowPosition = ArrowPosition.BottomEdgeAlignedLeft;
                }
                else if (arrowPosition == ArrowPosition.TopEdgeAlignedRight)
                {
                    arrowPosition = ArrowPosition.BottomEdgeAlignedRight;
                }
                else if (arrowPosition == ArrowPosition.BottomEdgeAlignedLeft)
                {
                    arrowPosition = ArrowPosition.TopEdgeAlignedLeft;
                }
                else if (arrowPosition == ArrowPosition.BottomEdgeAlignedRight)
                {
                    arrowPosition = ArrowPosition.TopEdgeAlignedRight;
                }
                else if (arrowPosition == ArrowPosition.LeftEdgeAlignedTop)
                {
                    arrowPosition = ArrowPosition.RightEdgeAlignedTop;
                }
                else if (arrowPosition == ArrowPosition.LeftEdgeAlignedBottom)
                {
                    arrowPosition = ArrowPosition.RightEdgeAlignedBottom;
                }
                else if (arrowPosition == ArrowPosition.RightEdgeAlignedTop)
                {
                    arrowPosition = ArrowPosition.LeftEdgeAlignedTop;
                }
                else if (arrowPosition == ArrowPosition.RightEdgeAlignedBottom)
                {
                    arrowPosition = ArrowPosition.RightEdgeAlignedBottom;
                }
            }
            SetCurrentValue(ArrowPositionProperty, arrowPosition);
        }
    }

    protected internal override void NotifyPositionPopup(bool showAtPointer)
    {
        if (Popup.Child!.DesiredSize == default)
        {
            LayoutHelper.MeasureChild(Popup.Child, Size.Infinity, new Thickness());
        }

        Popup.PlacementAnchor  = PlacementAnchor;
        Popup.PlacementGravity = PlacementGravity;

        if (showAtPointer)
        {
            Popup.Placement = PlacementMode.Pointer;
        }
        else
        {
            Popup.Placement                     = Placement;
            Popup.PlacementConstraintAdjustment = PlacementConstraintAdjustment;
        }

        var pointAtCenterOffset =
            CalculatePopupPositionDelta(Target!, Popup.Child, Popup.Placement, Popup.PlacementAnchor,
                Popup.PlacementGravity);

        var offset = GetPopupOffset();
        var offsetX = offset.X;
        var offsetY = offset.Y;

        if (IsPointAtCenter)
        {
            offsetX += pointAtCenterOffset.X;
            offsetY += pointAtCenterOffset.Y;
        }

        // 更新弹出信息是否指向中点
        Popup.HorizontalOffset = offsetX;
        Popup.VerticalOffset   = offsetY;
    }

    protected override bool ShowAtCore(Control placementTarget, bool showAtPointer = false)
    {
        if (IsOpen)
        {
            return false;
        }

        if (!PrepareShowPopup(placementTarget, showAtPointer))
        {
            return false;
        }

        IsOpen = true;
        Popup.MotionAwareOpen(() => { HandlePopupOpened(placementTarget); });
        return true;
    }

    protected override bool HideCore(bool canCancel = true)
    {
        if (!IsOpen)
        {
            return false;
        }

        if (canCancel)
        {
            if (CancelClosing())
            {
                return false;
            }
        }

        if (Popup.PlacementTarget?.GetVisualRoot() is null)
        {
            return base.HideCore(false);
        }

        NotifyAboutToClose();
        IsOpen = false;
        Popup.MotionAwareClose(HandlePopupClosed);
        return true;
    }

    protected bool CancelClosing()
    {
        var eventArgs = new CancelEventArgs();
        OnClosing(eventArgs);
        return eventArgs.Cancel;
    }
}