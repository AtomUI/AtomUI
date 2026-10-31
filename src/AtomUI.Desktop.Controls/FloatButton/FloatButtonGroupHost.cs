using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

public class FloatButtonGroupHost : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        FloatButton.PlacementProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        FloatButton.FloatOffsetXProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        FloatButton.FloatOffsetYProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        FloatButton.IsMotionEnabledProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        FloatButtonGroup.IconProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        FloatButtonGroup.CloseIconProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        FloatButtonGroup.ButtonTypeProperty.AddOwner<FloatButtonGroupHost>();

    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        FloatButtonGroup.ShapeProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        FloatButtonGroup.BoxShadowProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<FloatButtonGroupMenuPlacement> MenuPlacementProperty =
        FloatButtonGroup.MenuPlacementProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<FloatButtonGroupTrigger> TriggerProperty =
        FloatButtonGroup.TriggerProperty.AddOwner<FloatButtonGroupHost>();
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        FloatButtonGroup.IsOpenProperty.AddOwner<FloatButtonGroupHost>();
    
    
    public FloatButtonPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    public double FloatOffsetX
    {
        get => GetValue(FloatOffsetXProperty);
        set => SetValue(FloatOffsetXProperty, value);
    }

    public double FloatOffsetY
    {
        get => GetValue(FloatOffsetYProperty);
        set => SetValue(FloatOffsetYProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }
    
    public FloatButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
    
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public FloatButtonGroupMenuPlacement MenuPlacement
    {
        get => GetValue(MenuPlacementProperty);
        set => SetValue(MenuPlacementProperty, value);
    }
    
    public FloatButtonGroupTrigger Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    #endregion
    
    #region 内部属性定义
    
    [Content] 
    public ControlList Children { get; } = new ();

    #endregion
    
    private protected FloatButtonGroup? FloatButtonGroup;
    private protected CompositeDisposable? Disposables;
    private ScopeAwareOverlayLayer? _overlayLayer;
    
    public FloatButtonGroupHost()
    {
        Children.CollectionChanged += NotifyChildrenChanged;
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PrepareScopedOverlayLayer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CleanupScopedOverlayLayer();
    }

    private void PrepareScopedOverlayLayer()
    {
        _overlayLayer = ScopeAwareOverlayLayer.GetLayer(this);
        Disposables?.Dispose();
        Disposables =   new CompositeDisposable();
        FloatButtonGroup ??= NotifyCreateFloatButtonGroup(Disposables);
        _overlayLayer?.Children.Add(FloatButtonGroup);
    }

    private void CleanupScopedOverlayLayer()
    {
        if (FloatButtonGroup != null)
        {
            DetachFloatButtonGroupChildren();
            _overlayLayer?.Children.Remove(FloatButtonGroup);
            Disposables?.Dispose();
            Disposables      = null;
            FloatButtonGroup = null;
        }

        _overlayLayer = null;
    }

    private void DetachFloatButtonGroupChildren()
    {
        if (FloatButtonGroup is { Children.Count: > 0 } floatButtonGroup)
        {
            floatButtonGroup.Children.RemoveRange(0, floatButtonGroup.Children.Count);
        }
    }
    
    protected virtual FloatButtonGroup NotifyCreateFloatButtonGroup(CompositeDisposable disposables)
    {
        var floatButtonGroup = new FloatButtonGroup();
        disposables.Add(BindUtils.RelayBind(this, DataContextProperty, floatButtonGroup, DataContextProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, floatButtonGroup, IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, PlacementProperty, floatButtonGroup, PlacementProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetXProperty, floatButtonGroup, FloatOffsetXProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetYProperty, floatButtonGroup, FloatOffsetYProperty));
        disposables.Add(BindUtils.RelayBind(this, IconProperty, floatButtonGroup, IconProperty));
        disposables.Add(BindUtils.RelayBind(this, CloseIconProperty, floatButtonGroup, CloseIconProperty));
        disposables.Add(BindUtils.RelayBind(this, ButtonTypeProperty, floatButtonGroup, ButtonTypeProperty));
        disposables.Add(BindUtils.RelayBind(this, ShapeProperty, floatButtonGroup, ShapeProperty));
        disposables.Add(BindUtils.RelayBind(this, BoxShadowProperty, floatButtonGroup, BoxShadowProperty));
        disposables.Add(BindUtils.RelayBind(this, MenuPlacementProperty, floatButtonGroup, MenuPlacementProperty));
        disposables.Add(BindUtils.RelayBind(this, TriggerProperty, floatButtonGroup, TriggerProperty));
        disposables.Add(BindUtils.RelayBind(this, IsOpenProperty, floatButtonGroup, IsOpenProperty));
        
        floatButtonGroup.Children.AddRange(Children);
        return floatButtonGroup;
    }

    protected virtual void NotifyChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (FloatButtonGroup != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ControlCollectionChangedUtils.InsertControls(FloatButtonGroup.Children, e.NewStartingIndex, e.NewItems!);
                    break;

                case NotifyCollectionChangedAction.Move:
                    FloatButtonGroup.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    FloatButtonGroup.Children.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems!.Count; ++i)
                    {
                        var index = i + e.OldStartingIndex;
                        var child = (Control)e.NewItems![i]!;
                        FloatButtonGroup.Children[index] = child;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
    }
}
