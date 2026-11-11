using Avalonia;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// 追踪 root 自身及其视觉祖先链的 <c>IsVisible</c> 变化并触发回调。
/// Avalonia 的 <see cref="Visual.IsEffectivelyVisible" /> 是只读计算属性，
/// 没有公共变更通知；渲染在 AdornerLayer 等跨根宿主中的视觉无法随
/// 祖先 <c>IsVisible=false</c> 子树自动隐藏，需要靠本追踪器显式同步。
/// Observation stops below the adorner layer's host: ancestors from the root up to, but not including,
/// <c>boundary</c> are tracked. The host (and everything above it) hides the layer together with its
/// adorners, so those ancestors need no subscription. Without a boundary the whole chain is tracked.
/// </summary>
internal sealed class AncestorVisibilityTracker : IDisposable
{
    private readonly List<IDisposable> _subscriptions = [];

    public AncestorVisibilityTracker(Visual root, Action changed, Visual? boundary = null)
    {
        Boundary = boundary;
        _subscriptions.Add(root.GetPropertyChangedObservable(Visual.IsVisibleProperty)
                               .Subscribe(_ => changed()));
        foreach (var ancestor in root.GetVisualAncestors())
        {
            if (ReferenceEquals(ancestor, boundary))
            {
                break;
            }

            _subscriptions.Add(ancestor.GetPropertyChangedObservable(Visual.IsVisibleProperty)
                                       .Subscribe(_ => changed()));
        }
    }

    // The first ancestor that is not tracked, or null when the whole chain is tracked.
    internal Visual? Boundary { get; }

    // Number of live subscriptions (root plus tracked ancestors); exposed for tests and diagnostics.
    internal int SubscriptionCount => _subscriptions.Count;

    public void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }

        _subscriptions.Clear();
    }
}
