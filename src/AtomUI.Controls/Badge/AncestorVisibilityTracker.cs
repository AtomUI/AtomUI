using Avalonia;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Raises the callback whenever <c>IsVisible</c> changes on the root itself or on one of its visual ancestors.
/// <see cref="Visual.IsEffectivelyVisible" /> is a read-only computed property with no public change notification,
/// so a visual rendered in a cross-root host such as an adorner layer is not hidden together with the subtree of an
/// ancestor whose <c>IsVisible</c> is <c>false</c>; this tracker synchronizes such a visual explicitly.
/// The ancestors are captured when the tracker is created and tracked from the root up to, but not including,
/// <c>boundary</c>, the adorner layer's host. The host and everything above it hide the layer together with its
/// adorners, so they need no subscription. Without a boundary the whole chain is tracked.
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
