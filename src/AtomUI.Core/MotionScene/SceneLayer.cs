using AtomUI.Native;
using AtomUI.Reflection;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace AtomUI.MotionScene;

internal class SceneLayer : WindowBase, IHostedVisualTreeRoot, IDisposable
{
    protected BaseMotionActor? MotionActor;
    protected readonly IManagedPopupPositionerPopup? ManagedPopupPositionerPopup;

    static SceneLayer()
    {
        BackgroundProperty.OverrideDefaultValue<SceneLayer>(Brushes.Transparent);
    }

    public SceneLayer(TopLevel parent)
        : this(parent, parent.PlatformImpl!.CreatePopup()!, null)
    {
    }
    
    public SceneLayer(TopLevel parent, IPopupImpl impl)
        : this(parent, impl, null)
    {
    }

    /// <summary>
    /// 初始化一个新的动画顶层窗口
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="impl"></param>
    /// <param name="dependencyResolver">The dependency resolver to use. If null the default dependency resolver will be used.</param>
    public SceneLayer(TopLevel parent, IPopupImpl impl, IAvaloniaDependencyResolver? dependencyResolver)
        : base(impl, dependencyResolver)
    {
        ParentTopLevel   = parent;
        IsHitTestVisible = false;
        if (PlatformImpl is not null)
        {
            PlatformImpl.SetTransparencyLevelHint([WindowTransparencyLevel.Transparent]);
            PlatformImpl.SetWindowManagerAddShadowHint(false);
        }
        
        this.SetWindowIgnoreMouseEvents(true);
        if (PlatformImpl?.PopupPositioner is ManagedPopupPositioner managedPopupPositioner)
        {
            ManagedPopupPositionerPopup = managedPopupPositioner.GetManagedPopupPositionerPopup();
        }
        
        if (Parent != null && Parent != parent)
        {
            ((ISetLogicalParent)this).SetParent(null);
        }
        ((ISetLogicalParent)this).SetParent(parent);
        this.SetTemplatedParent(parent.TemplatedParent);
        
        Focusable = false;
    }

    /// <summary>
    /// Gets the platform-specific window implementation.
    /// </summary>
    public new IPopupImpl? PlatformImpl => (IPopupImpl?)base.PlatformImpl;

    /// <summary>
    /// Gets the control that is hosting the popup root.
    /// </summary>
    Visual IHostedVisualTreeRoot.Host
    {
        get
        {
            // If the parent is attached to a visual tree, then return that. However the parent
            // will possibly be a standalone Popup (i.e. a Popup not attached to a visual tree,
            // created by e.g. a ContextMenu): if this is the case, return the ParentTopLevel
            // if set. This helps to allow the focus manager to restore the focus to the outer
            // scope when the popup is closed.
            var parentVisual = Parent as Visual;
            if (parentVisual?.GetVisualRoot() != null)
            {
                return parentVisual;
            }

            return ParentTopLevel;
        }
    }

    public TopLevel ParentTopLevel { get; }

    public void Dispose()
    {
        PlatformImpl?.Dispose();
    }
    
    // 这个地方我们可以需要定制
    protected override Size MeasureOverride(Size availableSize)
    {
        var maxAutoSize = PlatformImpl?.MaxAutoSizeHint ?? Size.Infinity;
        var constraint  = availableSize;

        if (double.IsInfinity(constraint.Width))
        {
            constraint = constraint.WithWidth(maxAutoSize.Width);
        }

        if (double.IsInfinity(constraint.Height))
        {
            constraint = constraint.WithHeight(maxAutoSize.Height);
        }

        var measured    = base.MeasureOverride(constraint);
        var width       = measured.Width;
        var height      = measured.Height;
        var widthCache  = Width;
        var heightCache = Height;

        if (!double.IsNaN(widthCache))
        {
            width = widthCache;
        }

        width = Math.Min(width, MaxWidth);
        width = Math.Max(width, MinWidth);

        if (!double.IsNaN(heightCache))
        {
            height = heightCache;
        }

        height = Math.Min(height, MaxHeight);
        height = Math.Max(height, MinHeight);

        return new Size(width, height);
    }

    protected override Size ArrangeSetBounds(Size size)
    {
        return ClientSize;
    }

    public void MoveAndResize(Point point, Size size)
    {
        ManagedPopupPositionerPopup?.MoveAndResize(new Point(Math.Round(point.X), Math.Floor(point.Y + 0.5)), size);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        MotionActor = e.NameScope.Find<BaseMotionActor>(BaseMotionActor.MotionActorPart);
    }

    protected double GetRenderScaling()
    {
        return ParentTopLevel.RenderScaling;
    }
}