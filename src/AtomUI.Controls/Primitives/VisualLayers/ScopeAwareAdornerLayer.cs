using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// T3 content overlay layer: a canvas that draws adorners above the content of one scope. An adorner is
/// bound to its target by the attached <c>Adorner</c> property; the layer places it over the bounds of the
/// target and clips it to them. <see cref="GetLayer" /> returns the layer of a scope and injects one into
/// the scope host when the host has none.
/// </summary>
public class ScopeAwareAdornerLayer : Canvas
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <c>AdornedElement</c> attached property, which an adorner carries for the element it
    /// adorns. See <see cref="SetAdornedElement" />.
    /// </summary>
    public static readonly AttachedProperty<Control?> AdornedElementProperty =
        AvaloniaProperty.RegisterAttached<ScopeAwareAdornerLayer, Visual, Control?>("AdornedElement");

    /// <summary>
    /// Identifies the <c>Adorner</c> attached property, which an adorned element carries for its adorner.
    /// See <see cref="SetAdorner" />.
    /// </summary>
    public static readonly AttachedProperty<Control?> AdornerProperty =
        AvaloniaProperty.RegisterAttached<ScopeAwareAdornerLayer, Visual, Control?>("Adorner");

    /// <summary>
    /// Identifies the <see cref="LayerHost" /> property.
    /// </summary>
    public static readonly StyledProperty<Visual?> LayerHostProperty = AvaloniaProperty
        .Register<ScopeAwareAdornerLayer, Visual?>(nameof(LayerHost));

    /// <summary>
    /// The visual that owns the scope of this layer: a <c>ScrollContentPresenter</c>, a <c>ContentControl</c>
    /// (a <c>TopLevel</c> included) or a <c>VisualLayerManager</c>. The layer itself, or the wrapping panel
    /// that holds it, is attached under this visual. The layer injection and the creators of derived layers
    /// assign it; until then it is <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an adorned element cannot be translated into the layer directly, its position is computed relative
    /// to this visual, plus the scroll offset when it is a <c>ScrollContentPresenter</c>.
    /// </para>
    /// </remarks>
    public Visual? LayerHost
    {
        get => GetValue(LayerHostProperty);
        set => SetValue(LayerHostProperty, value);
    }

    #endregion

    #region 内部属性定义

    private static readonly AttachedProperty<AdornedElementInfo?> AdornedElementInfoProperty =
        AvaloniaProperty.RegisterAttached<ScopeAwareAdornerLayer, Visual, AdornedElementInfo?>("AdornedElementInfo");

    private static readonly AttachedProperty<ScopeAwareAdornerLayer?> SavedAdornerLayerProperty =
        AvaloniaProperty.RegisterAttached<Visual, Visual, ScopeAwareAdornerLayer?>("SavedAdornerLayer");

    private static readonly AttachedProperty<ScopeAwareAdornerLayer?> CachedLayerProperty =
        AvaloniaProperty.RegisterAttached<ScopeAwareAdornerLayer, Visual, ScopeAwareAdornerLayer?>("CachedLayer");

    private static readonly RelativePoint TransformOrigin = new(new Point(0, 0), RelativeUnit.Absolute);

    #endregion

    static ScopeAwareAdornerLayer()
    {
        AdornedElementProperty.Changed.AddClassHandler<Visual>(HandleAdornedElementChanged);
        AdornerProperty.Changed.AddClassHandler<Visual>(HandleAdornerChanged);
    }

    public ScopeAwareAdornerLayer()
    {
        Children.CollectionChanged += ChildrenCollectionChanged;
    }

    /// <summary>
    /// Returns the element that the adorner <paramref name="control" /> adorns, or <see langword="null" /> when
    /// none is set.
    /// </summary>
    /// <param name="control">
    /// The adorner.
    /// </param>
    public static Visual? GetAdornedElement(Visual control)
    {
        return control.GetValue(AdornedElementProperty);
    }

    /// <summary>
    /// Sets the element that the adorner <paramref name="control" /> adorns.
    /// </summary>
    /// <param name="control">
    /// The adorner. The value is stored on it and does not propagate to its children.
    /// </param>
    /// <param name="adorned">
    /// The adorned element, or <see langword="null" /> to stop tracking the current one.
    /// </param>
    /// <remarks>
    /// <para>
    /// The layer sets the value when it adds an adorner and clears it when it removes the adorner. Setting it
    /// on an adorner that is already a child of a layer makes that layer track the new element: the adorner
    /// follows the bounds of the element and is clipped to them.
    /// </para>
    /// </remarks>
    public static void SetAdornedElement(Visual control, Visual? adorned)
    {
        control.SetValue(AdornedElementProperty, adorned);
    }

    /// <summary>
    /// Returns the content overlay layer of the scope that contains <paramref name="visual" />, and injects
    /// one into the scope host when the host has none.
    /// </summary>
    /// <param name="visual">
    /// A visual of the scope, usually the adorned element.
    /// </param>
    /// <returns>
    /// The layer of the scope, or <see langword="null" /> when no scope host can be resolved, for example
    /// because <paramref name="visual" /> is not attached to a visual tree.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The scope host is resolved in this order: the nearest <c>ScrollContentPresenter</c> at or above
    /// <paramref name="visual" />, moved up to the first one that contains the adorned element when
    /// <paramref name="visual" /> is an adorner; a surface host marked with
    /// <see cref="LayerLadder.SetIsSurfaceHost" /> that is nearer than that presenter, unless the adorned
    /// element lies outside it; when no host was found that way and <paramref name="visual" /> lies in a layer
    /// of this exact type, that layer, which is returned as it is; the <c>VisualLayerManager</c> that
    /// <c>LayerLadder.FindContentLayerManager</c> returns; and finally the <c>TopLevel</c>.
    /// </para>
    /// <para>
    /// Injecting a layer into a <c>ScrollContentPresenter</c> or a <c>ContentControl</c> replaces its content
    /// with a wrapping panel that holds the original content and the layer.
    /// </para>
    /// </remarks>
    public static ScopeAwareAdornerLayer? GetLayer(Visual visual)
    {
        Layoutable? layerHost = visual.FindAncestorOfType<ScrollContentPresenter>(true);
        var         adorned   = GetAdornedElement(visual);
        if (layerHost != null && adorned != null)
        {
            while (layerHost != null && layerHost.IsVisualAncestorOf(adorned) == false)
            {
                layerHost = layerHost.FindAncestorOfType<ScrollContentPresenter>();
            }
        }

        // Nearest scope wins: a surface host nearer than the scroll host takes precedence (for example the
        // surface of a scoped Drawer in scrolled content). When the adorned element lies outside that surface,
        // the scroll host chosen above for the element is kept, so that a Drawer and the like keep following
        // scrolling.
        if (LayerLadder.PreferNearerSurfaceHost(visual, layerHost) is VisualLayerManager surfaceHost &&
            (adorned is null || surfaceHost.IsVisualAncestorOf(adorned)))
        {
            layerHost = surfaceHost;
        }

        // The reuse of a containing layer accepts only T3 instances of the exact type: content overlays such as a
        // Watermark inside a presentation layer subclass (T5/T7) must not land in that presentation layer as a
        // sibling, or they would paint above a Drawer that is higher.
        if (layerHost == null &&
            visual.FindAncestorOfType<ScopeAwareAdornerLayer>(true) is { } containingLayer &&
            IsContentOverlayLayer(containingLayer))
        {
            return containingLayer;
        }

        layerHost ??= LayerLadder.FindContentLayerManager(visual);
        if (layerHost == null && TopLevel.GetTopLevel(visual) is { } topLevel)
        {
            layerHost = topLevel;
        }

        if (layerHost == null)
        {
            return null;
        }

        return GetOrInjectLayer(layerHost);
    }

    /// <summary>
    /// Finds (or injects) the T3 layer on the given host. When the host is a TopLevel, the layer is injected
    /// through the content wrapping panel, so that presentation layers on a TopLevel without a
    /// VisualLayerManager share that panel.
    /// </summary>
    internal static ScopeAwareAdornerLayer? GetOrInjectLayer(Layoutable layerHost)
    {
        var cachedLayer = layerHost.GetValue(CachedLayerProperty);
        if (cachedLayer?.GetVisualParent() != null)
        {
            return cachedLayer;
        }

        var layer = FindAdornerLayer(layerHost);
        layer ??= InjectLayer(layerHost);
        if (layer != null)
        {
            layerHost.SetValue(CachedLayerProperty, layer);
        }

        return layer;
    }

    /// <summary>
    /// Returns the adorner that <see cref="SetAdorner" /> set on <paramref name="visual" />, or
    /// <see langword="null" /> when it has none.
    /// </summary>
    /// <param name="visual">
    /// The adorned element.
    /// </param>
    public static Control? GetAdorner(Visual visual)
    {
        return visual.GetValue(AdornerProperty);
    }

    /// <summary>
    /// Sets the adorner that the content overlay layer of the scope draws over <paramref name="visual" />.
    /// </summary>
    /// <param name="visual">
    /// The adorned element. The value is stored on it and does not propagate to its children.
    /// </param>
    /// <param name="adorner">
    /// The adorner to show, or <see langword="null" /> to remove the current one. An element has one adorner,
    /// so setting another removes the previous one.
    /// </param>
    /// <remarks>
    /// <para>
    /// While <paramref name="visual" /> is attached to the visual tree, the adorner is a child of the layer
    /// that <see cref="GetLayer" /> resolves for it and a logical child of <paramref name="visual" />. The
    /// layer places the adorner over the bounds of the element and clips it to them.
    /// </para>
    /// <para>
    /// The adorner is added when the value is set on an attached element and each time the element attaches
    /// to the visual tree. It is removed when the value is replaced or cleared and each time the element
    /// detaches. Nothing is added while <see cref="GetLayer" /> finds no scope host for the element.
    /// </para>
    /// </remarks>
    public static void SetAdorner(Visual visual, Control? adorner)
    {
        visual.SetValue(AdornerProperty, adorner);
    }

    /// <summary>
    /// Measures every adorner with the size of the bounds snapshot of its adorned element, or with
    /// <paramref name="availableSize" /> while it has no snapshot. Returns a size of zero, so the layer adds
    /// no size to its host. An override must call this implementation, or the adorners are not measured.
    /// </summary>
    /// <param name="availableSize">
    /// The size available to the layer.
    /// </param>
    /// <returns>
    /// A size of zero.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            if (child is AvaloniaObject ao)
            {
                var info = ao.GetValue(AdornedElementInfoProperty);

                if (info != null && info.Bounds.HasValue)
                {
                    child.Measure(info.Bounds.Value.Bounds.Size);
                }
                else
                {
                    child.Measure(availableSize);
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Places every adorner at the bounds snapshot of its adorned element: translated by the snapshot
    /// transform and clipped to the snapshot bounds. An adorner without a snapshot is arranged as a plain
    /// child of the canvas. An override must call this implementation, or the adorners are not placed.
    /// </summary>
    /// <param name="finalSize">
    /// The size allocated to the layer.
    /// </param>
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            if (child is AvaloniaObject ao)
            {
                var info = ao.GetValue(AdornedElementInfoProperty);

                if (info != null && info.Bounds.HasValue)
                {
                    UpdateTransform(child, info, info.Bounds.Value.Transform);
                    UpdateClip(child, info.Bounds.Value);
                    child.Arrange(info.Bounds.Value.Bounds);
                }
                else
                {
                    ArrangeChild(child, finalSize);
                }
            }
        }

        return finalSize;
    }

    private static void UpdateTransform(Control child, AdornedElementInfo info, Matrix matrix)
    {
        info.Transform ??= new MatrixTransform();

        if (!ReferenceEquals(child.RenderTransform, info.Transform))
        {
            child.RenderTransform = info.Transform;
        }

        if (info.Transform.Matrix != matrix)
        {
            info.Transform.Matrix = matrix;
        }

        if (child.RenderTransformOrigin != TransformOrigin)
        {
            child.RenderTransformOrigin = TransformOrigin;
        }
    }

    private void ChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (Visual i in e.NewItems!)
                {
                    UpdateAdornedElement(i, i.GetValue(AdornedElementProperty));
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (Visual i in e.OldItems!)
                {
                    UpdateAdornedElement(i, null);
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                break;
        }

        InvalidateArrange();
    }

    private void UpdateAdornedElement(Visual adorner, Control? adorned)
    {
        var info = adorner.GetValue(AdornedElementInfoProperty);

        if (info != null)
        {
            info.Subscription?.Dispose();

            if (adorned == null)
            {
                adorner.ClearValue(AdornedElementInfoProperty);
            }
        }

        if (adorned != null)
        {
            if (info == null)
            {
                info = new AdornedElementInfo();
                adorner.SetValue(AdornedElementInfoProperty, info);
            }

            void TakeBoundsSnapshot()
            {
                Debug.Assert(LayerHost != null);

                // When the layer and the adorned element are in the same content subtree (the injection path: the
                // layer is a sibling of the content root that holds the adorned element), translate directly into
                // the layer coordinates. The scroll translation cancels out along the chain, so the result does not
                // change with scrolling. The fallback translates relative to LayerHost and compensates the scroll
                // offset of a host ScrollContentPresenter.
                var translation = adorned.TranslatePoint(new Point(0, 0), this);
                if (translation is null)
                {
                    double offsetX = 0d;
                    double offsetY = 0d;
                    var relateToHostPoint = adorned.TranslatePoint(new Point(0, 0), LayerHost) ?? new Point(0, 0);
                    if (LayerHost is ScrollContentPresenter scrollContentPresenter)
                    {
                        offsetX = scrollContentPresenter.Offset.X;
                        offsetY = scrollContentPresenter.Offset.Y;
                    }

                    offsetX += relateToHostPoint.X;
                    offsetY += relateToHostPoint.Y;
                    translation = new Point(offsetX, offsetY);
                }

                var translationMatrix = Matrix.CreateTranslation(translation.Value.X, translation.Value.Y);
                // The size must be the arranged box of the adorned element (Bounds, without Margin). DesiredSize
                // includes Margin: using it would enlarge the container to the margin box, and the mask and the panel
                // would extend beyond the visible bounds of the host (most visibly for a host element with a margin).
                // Only an element that is not arranged yet (Bounds is empty) falls back to the measured value
                // DesiredSize minus Margin.
                var adornedWidth  = adorned.Bounds.Width > 0
                    ? adorned.Bounds.Width
                    : Math.Max(0, adorned.DesiredSize.Width - adorned.Margin.Left - adorned.Margin.Right);
                var adornedHeight = adorned.Bounds.Height > 0
                    ? adorned.Bounds.Height
                    : Math.Max(0, adorned.DesiredSize.Height - adorned.Margin.Top - adorned.Margin.Bottom);
                info.Bounds = new TransformedBounds(new Rect(new Size(adornedWidth, adornedHeight)),
                    new Rect(new Size(adornedWidth, adornedHeight)),
                    translationMatrix);
                InvalidateMeasure();
            }

            // Layer injection attaches the layer to a brand-new wrapping panel, so the first snapshot is taken while
            // the panel and the layer are not arranged yet and the translation is unavailable. That yields a wrong
            // position with the scroll offset left uncompensated, and the Bounds of the adorned element do not change
            // afterwards, so the error would stay until the next open or close. Subscribing to the Bounds of the layer
            // itself heals the snapshot when the layer is arranged after the injection (from empty to its real
            // bounds). Scrolling does not change the Bounds of the layer, so it causes no recalculation that mixes
            // the old position with the new offset.
            var adornedSubscription = adorned.GetObservable(BoundsProperty).Subscribe(_ => TakeBoundsSnapshot());
            var layerSubscription   = this.GetObservable(BoundsProperty).Subscribe(_ => TakeBoundsSnapshot());
            info.Subscription = Disposable.Create(() =>
            {
                adornedSubscription.Dispose();
                layerSubscription.Dispose();
            });
        }
    }

    private void UpdateClip(Control control, TransformedBounds bounds)
    {
        if (!(control.Clip is RectangleGeometry clip))
        {
            clip         = new RectangleGeometry();
            control.Clip = clip;
        }

        var clipBounds = bounds.Bounds;

        clip.Rect = clipBounds;
    }

    private static void HandleAdornedElementChanged(Visual sender, AvaloniaPropertyChangedEventArgs e)
    {
        var adorned = e.NewValue as Control;
        var layer   = sender.GetVisualParent<ScopeAwareAdornerLayer>();
        layer?.UpdateAdornedElement(sender, adorned);
    }

    private static void HandleAdornerChanged(Visual visual, AvaloniaPropertyChangedEventArgs e)
    {
        var oldAdorner = e.OldValue as Control;
        var newAdorner = e.NewValue as Control;

        if (Equals(oldAdorner, newAdorner))
        {
            return;
        }

        if (oldAdorner is { })
        {
            visual.AttachedToVisualTree   -= VisualOnAttachedToVisualTree;
            visual.DetachedFromVisualTree -= VisualOnDetachedFromVisualTree;
            Detach(visual, oldAdorner);
        }

        // Detach may synchronously lead the user to replace the adorner; only the instance that the property
        // points to now may enter the layer.
        if (!ReferenceEquals(GetAdorner(visual), newAdorner))
        {
            return;
        }

        if (newAdorner is { })
        {
            visual.AttachedToVisualTree   += VisualOnAttachedToVisualTree;
            visual.DetachedFromVisualTree += VisualOnDetachedFromVisualTree;
            Attach(visual, newAdorner);
        }
    }

    private static void Attach(Visual visual, Control adorner)
    {
        var layer = ScopeAwareAdornerLayer.GetLayer(visual);
        // Adding to the visual tree synchronously raises resource bindings and user callbacks, and a callback may
        // remove the adorner at once. Record the layer that the exit needs first, so that a re-entrant Detach
        // does not miss the host that has not been written yet.
        visual.SetValue(SavedAdornerLayerProperty, layer);
        AddVisualAdorner(visual, adorner, layer);
    }

    private static void Detach(Visual visual, Control adorner)
    {
        var layer = visual.GetValue(SavedAdornerLayerProperty);
        visual.ClearValue(SavedAdornerLayerProperty);
        RemoveVisualAdorner(visual, adorner, layer);
    }

    private static void AddVisualAdorner(Visual visual, Control? adorner, ScopeAwareAdornerLayer? layer)
    {
        if (adorner is null || layer == null || ReferenceEquals(adorner.GetVisualParent(), layer))
        {
            return;
        }

        SetAdornedElement(adorner, visual);

        adorner.SetLogicalParent(visual);
        if (!ReferenceEquals(GetAdorner(visual), adorner))
        {
            adorner.SetLogicalParent(null);
            return;
        }
        layer.Children.Add(adorner);
    }

    private static void RemoveVisualAdorner(Visual visual, Control? adorner, ScopeAwareAdornerLayer? layer)
    {
        if (adorner is null || layer is null || !ReferenceEquals(adorner.GetVisualParent(), layer))
        {
            return;
        }

        if (ReferenceEquals(GetAdornedElement(adorner), visual))
        {
            SetAdornedElement(adorner, null);
        }

        layer.Children.Remove(adorner);
        adorner.SetLogicalParent(null);
    }

    private static void VisualOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Visual visual)
        {
            var adorner = GetAdorner(visual);
            if (adorner is { })
            {
                Attach(visual, adorner);
            }
        }
    }

    private static void VisualOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Visual visual)
        {
            var adorner = GetAdorner(visual);
            if (adorner is { })
            {
                Detach(visual, adorner);
            }
        }
    }

    private static ScopeAwareAdornerLayer? InjectLayer(Layoutable layerHost)
    {
        var layer = new ScopeAwareAdornerLayer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            ZIndex              = LayerLadder.ContentOverlay
        };

        layer.LayerHost = layerHost;
        if (layerHost is VisualLayerManager)
        {
            LayerLadder.AddLayer(layerHost, layer, LayerLadder.ContentOverlay);
        }
        // The wrapping panel injection below does not go through LayerLadder.AddLayer: the host is not a layer
        // container. The original content is taken off and put into a new wrapping Panel together with the layer,
        // and that Panel is put back into the host. The layer number still comes from LayerLadder (see the ZIndex
        // initialization above).
        else if (layerHost is ScrollContentPresenter scrollContentPresenter)
        {
            if (scrollContentPresenter.Content is Control controlContent)
            {
                var oldOffset = scrollContentPresenter.Offset;
                // The content is a control that needs no template.
                scrollContentPresenter.Content = null;
                scrollContentPresenter.UpdateChild();
                var panel = new Panel
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment   = VerticalAlignment.Stretch,
                };
                panel.Children.Add(controlContent);
                panel.Children.Add(layer);
                scrollContentPresenter.Content = panel;
                var scrollViewer = scrollContentPresenter.FindAncestorOfType<ScrollViewer>();
                if (scrollViewer != null)
                {
                    scrollViewer.Offset = oldOffset;
                }
            }
            else if (scrollContentPresenter.Content != null && scrollContentPresenter.ContentTemplate != null)
            {
                // The content is built by a content template.
                var injectTemplate = new FuncDataTemplate<object?>((o, scope) =>
                {
                    var originControl = scrollContentPresenter.ContentTemplate.Build(o);
                    var panel = new Panel
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment   = VerticalAlignment.Stretch,
                    };
                    Debug.Assert(originControl != null);
                    panel.Children.Add(originControl);
                    panel.Children.Add(layer);
                    return panel;
                });
                scrollContentPresenter.ContentTemplate = injectTemplate;
            }
        }
        else if (layerHost is ContentControl contentControl)
        {
            var content         = contentControl.Content;
            var contentTemplate = contentControl.ContentTemplate;
            contentControl.Content         = null;
            contentControl.ContentTemplate = null;

            var panel = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch,
            };

            if (content is Control controlContent && contentTemplate == null)
            {
                panel.Children.Add(controlContent);
            }
            else if (content != null)
            {
                panel.Children.Add(new ContentPresenter
                {
                    Content         = content,
                    ContentTemplate = contentTemplate
                });
            }

            panel.Children.Add(layer);
            contentControl.Content = panel;
        }

        return layer.GetVisualParent() == null ? null : layer;
    }

    private static ScopeAwareAdornerLayer? FindAdornerLayer(Layoutable layerHost)
    {
        ScopeAwareAdornerLayer? layer = null;
        if (layerHost is ScrollContentPresenter scrollContentPresenter)
        {
            // The layer sits under the wrapping panel that the injection created.
            var panel = scrollContentPresenter.FindChildOfType<Panel>();
            if (panel is not null)
            {
                layer = FindContentOverlayChild(panel);
            }
        }
        else if (layerHost is VisualLayerManager visualLayerManager)
        {
            // The layer is a direct child of the manager.
            layer = FindContentOverlayChild(visualLayerManager);
        }
        else if (layerHost is ContentControl contentControl)
        {
            if (contentControl.Content is Control content)
            {
                layer = FindContentOverlayChild(content);
            }
        }

        return layer;
    }

    private static bool IsContentOverlayLayer(ScopeAwareAdornerLayer layer)
    {
        return layer.GetType() == typeof(ScopeAwareAdornerLayer);
    }

    /// <summary>
    /// Returns only T3 instances of the exact type: presentation layer subclasses (T5/T7) in the same host
    /// must not be reused as the content overlay layer.
    /// </summary>
    private static ScopeAwareAdornerLayer? FindContentOverlayChild(Visual parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is ScopeAwareAdornerLayer layer && IsContentOverlayLayer(layer))
            {
                return layer;
            }
        }

        return null;
    }

    private class AdornedElementInfo
    {
        public IDisposable? Subscription { get; set; }

        public TransformedBounds? Bounds { get; set; }

        public MatrixTransform? Transform { get; set; }
    }
}
