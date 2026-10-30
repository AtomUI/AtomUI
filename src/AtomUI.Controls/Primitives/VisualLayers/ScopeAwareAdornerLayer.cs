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

public class ScopeAwareAdornerLayer : Canvas
{
    #region 公共属性定义

    public static readonly AttachedProperty<Control?> AdornedElementProperty =
        AvaloniaProperty.RegisterAttached<ScopeAwareAdornerLayer, Visual, Control?>("AdornedElement");

    public static readonly AttachedProperty<Control?> AdornerProperty =
        AvaloniaProperty.RegisterAttached<ScopeAwareAdornerLayer, Visual, Control?>("Adorner");

    public static readonly StyledProperty<Visual?> LayerHostProperty = AvaloniaProperty
        .Register<ScopeAwareAdornerLayer, Visual?>(nameof(LayerHost));

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

    public static Visual? GetAdornedElement(Visual control)
    {
        return control.GetValue(AdornedElementProperty);
    }

    public static void SetAdornedElement(Visual control, Visual? adorned)
    {
        control.SetValue(AdornedElementProperty, adorned);
    }

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

        if (layerHost == null &&
            visual.FindAncestorOfType<ScopeAwareAdornerLayer>(true) is { } containingLayer)
        {
            return containingLayer;
        }

        layerHost ??= visual.FindAncestorOfType<VisualLayerManager>();
        if (layerHost == null && TopLevel.GetTopLevel(visual) is { } topLevel)
        {
            layerHost = FindFirstDescendantLayerManager(topLevel);
            layerHost ??= topLevel;
        }

        if (layerHost == null)
        {
            return null;
        }

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

    public static Control? GetAdorner(Visual visual)
    {
        return visual.GetValue(AdornerProperty);
    }

    public static void SetAdorner(Visual visual, Control? adorner)
    {
        visual.SetValue(AdornerProperty, adorner);
    }

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

                // 层与被装饰元素位于同一内容子树时（层注入路径：层是被装饰元素
                // 所在内容根的兄弟节点），直接换算到层坐标：滚动平移在链路上抵消，
                // 结果对滚动不变。回退路径按 LayerHost 换算并补偿宿主
                // ScrollContentPresenter 的滚动偏移。
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
                // 尺寸必须取被装饰元素的排列盒（Bounds，不含 Margin）：DesiredSize
                // 包含 Margin，直接取大会把容器放大成 margin box，遮罩与面板会
                // 越出宿主的可见边界（带边距的宿主元素尤甚）。仅在尚未排列
                // （Bounds 为空）时回退到「DesiredSize - Margin」的测量值。
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

            // 层注入把层挂进全新的包裹面板，首次快照发生在面板/层尚未 arrange 的
            // 瞬态，换算链不可用（得到未补偿滚动的错误位置，且被装饰元素的 Bounds
            // 不会随之再变，错误会一直保持到下一次开合）。订阅层自身 Bounds：注入
            // 后层被 arrange（0 → 实际值）时换算链就绪、快照自愈；正常滚动不改变
            // 层的 Bounds，不会引入「旧位置 + 新偏移」的混态重算。
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

        // Detach 可同步触发用户更换 adorner；只让当前属性指向的实例进入层。
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
        // 加入视觉树会同步触发资源绑定与用户回调；回调可能立即移除 adorner。
        // 必须先记录退出所需的层，避免重入 Detach 找不到尚未写入的宿主。
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
            ZIndex              = VisualLayerManagerUtils.ScopeAwareAdornerLayerZIndex
        };

        layer.LayerHost = layerHost;
        if (layerHost is VisualLayerManager visualLayerManager)
        {
            visualLayerManager.AddLayer(layer, layer.ZIndex);
        }
        else if (layerHost is ScrollContentPresenter scrollContentPresenter)
        {
            if (scrollContentPresenter.Content is Control controlContent)
            {
                var oldOffset = scrollContentPresenter.Offset;
                // 直接内容控件
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
                // 模版处理
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

    private static VisualLayerManager? FindFirstDescendantLayerManager(Visual visual)
    {
        foreach (var descendant in visual.GetVisualDescendants())
        {
            if (descendant is VisualLayerManager manager)
            {
                return manager;
            }
        }

        return null;
    }

    private static ScopeAwareAdornerLayer? FindAdornerLayer(Layoutable layerHost)
    {
        ScopeAwareAdornerLayer? layer = null;
        if (layerHost is ScrollContentPresenter scrollContentPresenter)
        {
            // 在 Panel 下面
            var panel = scrollContentPresenter.FindChildOfType<Panel>();
            if (panel is not null)
            {
                layer = panel.FindChildOfType<ScopeAwareAdornerLayer>();
            }
        }
        else if (layerHost is VisualLayerManager visualLayerManager)
        {
            // 直接就在下面
            layer = visualLayerManager.FindChildOfType<ScopeAwareAdornerLayer>();
        }
        else if (layerHost is ContentControl contentControl)
        {
            if (contentControl.Content is Control content)
            {
                layer = content.FindChildOfType<ScopeAwareAdornerLayer>();
            }
        }

        return layer;
    }

    private class AdornedElementInfo
    {
        public IDisposable? Subscription { get; set; }

        public TransformedBounds? Bounds { get; set; }

        public MatrixTransform? Transform { get; set; }
    }
}
