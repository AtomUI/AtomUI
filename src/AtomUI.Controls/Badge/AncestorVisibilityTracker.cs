using Avalonia;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// 追踪 root 自身及其视觉祖先链的 <c>IsVisible</c> 变化并触发回调。
/// Avalonia 的 <see cref="Visual.IsEffectivelyVisible" /> 是只读计算属性，
/// 没有公共变更通知；渲染在 AdornerLayer 等跨根宿主中的视觉无法随
/// 祖先 <c>IsVisible=false</c> 子树自动隐藏，需要靠本追踪器显式同步。
/// </summary>
internal sealed class AncestorVisibilityTracker : IDisposable
{
    private readonly List<IDisposable> _subscriptions = [];

    public AncestorVisibilityTracker(Visual root, Action changed)
    {
        _subscriptions.Add(root.GetPropertyChangedObservable(Visual.IsVisibleProperty)
                               .Subscribe(_ => changed()));
        foreach (var ancestor in root.GetVisualAncestors())
        {
            _subscriptions.Add(ancestor.GetPropertyChangedObservable(Visual.IsVisibleProperty)
                                       .Subscribe(_ => changed()));
        }
    }

    public void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }

        _subscriptions.Clear();
    }
}
