using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AtomUI.Controls.Commons;

/// <summary>
/// 浮动表面（FloatButton / FloatButtonGroup）的表面宿主（规则 3）：浮动控件先包进一个表面宿主，再由宿主作为
/// <see cref="ScopeAwareOverlayLayer"/> 的直接子项，浮动控件内的焦点描边、Badge 因此紧贴自己的表面，
/// 不会画到同层更高的兄弟表面之上。
/// <para>
/// 为什么不在 ControlTheme 中声明：浮动控件由 FloatButtonHost / FloatButtonGroupHost 在运行时创建，并移入另一个
/// 视觉根（作用域宿主）的 T4 层，宿主自身的模板够不到那里；FloatButton 自身的模板还用于内联场景
/// （FloatButtonGroup 内的子按钮、IsEmbedMode），在模板里声明会让每个内联按钮也各带一个宿主。
/// </para>
/// <para>
/// 浮动控件仍以自身的 Canvas.Left / Canvas.Top 表达在层中的位置；表面宿主单向跟随这两个值，层按宿主排布。
/// </para>
/// </summary>
internal static class FloatingSurfaceHost
{
    /// <summary>把浮动控件包进新建的表面宿主并加入层；位置跟随的绑定登记到 disposables。</summary>
    internal static VisualLayerManager Attach(ScopeAwareOverlayLayer layer,
                                              Control floating,
                                              CompositeDisposable disposables)
    {
        var host = LayerLadder.CreateSurfaceHost();
        disposables.Add(BindUtils.RelayBind(floating, Canvas.LeftProperty, host, Canvas.LeftProperty, BindingMode.OneWay));
        disposables.Add(BindUtils.RelayBind(floating, Canvas.TopProperty, host, Canvas.TopProperty, BindingMode.OneWay));
        // 先入层再挂 Child：浮动控件的 Parent 变化发生时，宿主已经在层里，浮动控件据此解析到所在层。
        layer.Children.Add(host);
        host.Child = floating;
        return host;
    }

    /// <summary>把表面宿主移出层，并解除浮动控件与宿主的父子关系（浮动控件据此退订层的尺寸变化）。</summary>
    internal static void Detach(ScopeAwareOverlayLayer? layer, VisualLayerManager? host)
    {
        if (host is null)
        {
            return;
        }

        layer?.Children.Remove(host);
        host.Child = null;
    }

    /// <summary>浮动控件所在的 T4 层：逻辑父级是层本身，或是层中的表面宿主。</summary>
    internal static ScopeAwareOverlayLayer? ResolveLayer(StyledElement? parent)
    {
        if (parent is ScopeAwareOverlayLayer layer)
        {
            return layer;
        }

        if (parent is VisualLayerManager manager && LayerLadder.GetIsSurfaceHost(manager))
        {
            return manager.Parent as ScopeAwareOverlayLayer;
        }

        return null;
    }
}
