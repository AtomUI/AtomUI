using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

/// <summary>
/// Host that creates one floating button group at run time and shows it in the scoped overlay layer of the nearest
/// layer host, instead of in its own visual tree. The content of the host becomes the items of the group, and the
/// properties set on the host are forwarded to the group, which exists only while the host is attached to a visual
/// tree.
/// </summary>
public class FloatButtonGroupHost : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        FloatButton.PlacementProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetX" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        FloatButton.FloatOffsetXProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetY" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        FloatButton.FloatOffsetYProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        FloatButton.IsMotionEnabledProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        FloatButtonGroup.IconProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        FloatButtonGroup.CloseIconProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="ButtonType" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        FloatButtonGroup.ButtonTypeProperty.AddOwner<FloatButtonGroupHost>();

    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        FloatButtonGroup.ShapeProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        FloatButtonGroup.BoxShadowProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="MenuPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonGroupMenuPlacement> MenuPlacementProperty =
        FloatButtonGroup.MenuPlacementProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="Trigger" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonGroupTrigger> TriggerProperty =
        FloatButtonGroup.TriggerProperty.AddOwner<FloatButtonGroupHost>();
    
    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        FloatButtonGroup.IsOpenProperty.AddOwner<FloatButtonGroupHost>();
    
    
    /// <summary>
    /// Gets or sets the edge or corner of the scoped overlay layer that the group is anchored to. The value is
    /// forwarded to the group. Unset means <see cref="FloatButtonPlacement.Top" />, and the theme of the host places
    /// the group at <see cref="FloatButtonPlacement.BottomRight" />.
    /// </summary>
    public FloatButtonPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal offset, in device-independent pixels, of the group from the position chosen by
    /// <see cref="Placement" />. A positive value moves the group right, or left when the placement is <c>Right</c>,
    /// <c>TopRight</c> or <c>BottomRight</c>; a negative value moves it the other way. The value is forwarded to the
    /// group. When unset, the theme of the host sets it from the <c>FloatOffsetX</c> Token of the floating button,
    /// unless the placement is <see cref="FloatButtonPlacement.Center" />.
    /// </summary>
    public double FloatOffsetX
    {
        get => GetValue(FloatOffsetXProperty);
        set => SetValue(FloatOffsetXProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset, in device-independent pixels, of the group from the position chosen by
    /// <see cref="Placement" />. A positive value moves the group down, or up when the placement is <c>Bottom</c>,
    /// <c>BottomLeft</c> or <c>BottomRight</c>; a negative value moves it the other way. The value is forwarded to the
    /// group. When unset, the theme of the host sets it from the <c>FloatOffsetY</c> Token of the floating button,
    /// unless the placement is <see cref="FloatButtonPlacement.Center" />.
    /// </summary>
    public double FloatOffsetY
    {
        get => GetValue(FloatOffsetYProperty);
        set => SetValue(FloatOffsetYProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the menu of the group animates when it opens and closes. The value is forwarded to the
    /// group. When unset, the theme of the host sets it from the <c>EnableMotion</c> Token of the group host.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon of the trigger button that opens the menu of the group. The value is forwarded to the
    /// group and applies when <see cref="Trigger" /> is <c>Click</c> or <c>Hover</c>; while the menu is open the
    /// trigger shows <see cref="CloseIcon" /> instead. When no icon is set, the group substitutes a default file icon
    /// when it is initialized; setting <see langword="null" /> after that leaves the trigger without an icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon that the trigger button shows while the menu of the group is open. The value is forwarded
    /// to the group. When no icon is set, the group substitutes a default close icon when it is initialized.
    /// </summary>
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the visual type of the trigger button of the group, which selects its icon color and background in
    /// the normal, hovered and pressed states. The value is forwarded to the group. Unset means
    /// <see cref="FloatButtonType.Default" />.
    /// </summary>
    public FloatButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the shape of the group. The value is forwarded to the group, which applies it to its trigger
    /// button and to the list of its items. Unset means <see cref="FloatButtonShape.Circle" />.
    /// </summary>
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the shadow drawn under the group. The value is forwarded to the group. When unset, the theme of
    /// the host sets it only for a square group, from the <c>FloatButtonShadows</c> Token of the floating button.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the side of the trigger button at which the menu of the group opens. The value is forwarded to
    /// the group. <c>Top</c> and <c>Bottom</c> stack the items vertically; <c>Left</c> and <c>Right</c> lay them
    /// out horizontally. Unset means <see cref="FloatButtonGroupMenuPlacement.Top" />.
    /// </summary>
    public FloatButtonGroupMenuPlacement MenuPlacement
    {
        get => GetValue(MenuPlacementProperty);
        set => SetValue(MenuPlacementProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how the menu of the group opens. With <see cref="FloatButtonGroupTrigger.Default" /> the group is
    /// a plain list of its items and has no trigger button; with <c>Click</c> or <c>Hover</c> a trigger button opens
    /// the menu when it is clicked or when the pointer rests on it. The value is forwarded to the group.
    /// </summary>
    public FloatButtonGroupTrigger Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the menu of the group is open. The value is forwarded to the group and applies when
    /// <see cref="Trigger" /> is <c>Click</c> or <c>Hover</c>: <see langword="true" /> shows the menu and
    /// <see langword="false" /> hides it.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    #endregion
    
    #region 内部属性定义
    
    /// <summary>
    /// Gets the controls that become the items of the group; the XAML content of the host is added here. A change of
    /// the collection is copied into the group while the host is attached to a visual tree, except a reset, which is
    /// not supported and throws <see cref="NotSupportedException" />.
    /// </summary>
    [Content] 
    public ControlList Children { get; } = new ();

    #endregion
    
    private protected FloatButtonGroup? FloatButtonGroup;
    private protected CompositeDisposable? Disposables;
    private ScopeAwareOverlayLayer? _overlayLayer;
    private VisualLayerManager? _surfaceHost;

    public FloatButtonGroupHost()
    {
        Children.CollectionChanged += NotifyChildrenChanged;
    }
    
    /// <summary>
    /// Called when the host is attached to a visual tree. It resolves the scoped overlay layer of the nearest layer
    /// host, creates the group through <see cref="NotifyCreateFloatButtonGroup" /> when the host has none, and shows
    /// the group in that layer. Overrides must call the base implementation, otherwise no group is created.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PrepareScopedOverlayLayer();
    }

    /// <summary>
    /// Called when the host is detached from a visual tree. It takes the items out of the group, removes the group
    /// from the scoped overlay layer and disposes its bindings, so that attaching the host again creates a new group.
    /// Overrides must call the base implementation, otherwise the group stays in the layer.
    /// </summary>
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
        if (_overlayLayer != null)
        {
            // The group enters the T4 layer wrapped in its own surface host. The surface host is created in
            // code: the group is created at run time and placed in the scoped overlay layer of another
            // visual root, which the template of this host cannot reach, so a ControlTheme of the host
            // cannot declare that host.
            _surfaceHost = FloatingSurfaceHost.Attach(_overlayLayer, FloatButtonGroup, Disposables);
        }
    }

    private void CleanupScopedOverlayLayer()
    {
        if (FloatButtonGroup != null)
        {
            DetachFloatButtonGroupChildren();
            FloatingSurfaceHost.Detach(_overlayLayer, _surfaceHost);
            _surfaceHost = null;
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
    
    /// <summary>
    /// Creates the <c>FloatButtonGroup</c> that the host shows in the scoped overlay layer and fills it: the items
    /// come from <see cref="Children" />, and the data context and the properties of the host are relay-bound to it.
    /// It is called when the host is attached to a visual tree and has no group yet. An override that creates
    /// bindings or other disposables adds them to <paramref name="disposables" /> as well.
    /// </summary>
    /// <param name="disposables">
    /// Receives the bindings between the host and the group. It is disposed when the host is detached.
    /// </param>
    /// <returns>
    /// The new group.
    /// </returns>
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

    /// <summary>
    /// Copies a change of <see cref="Children" /> into the items of the group while the host is attached to a visual
    /// tree: an add, move, remove or replace is applied to the group at the same index. A reset of the collection is
    /// not supported and throws <see cref="NotSupportedException" />. Before the group exists nothing is copied; the
    /// group receives all items when it is created.
    /// </summary>
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
