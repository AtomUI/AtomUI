using AtomUI.Theme.Schema;
using Avalonia;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Receives the new inherited <see cref="ThemeScope.TokenOwnerProperty" /> value of the element it registered for.
/// </summary>
internal interface ITokenOwnerListener
{
    void OnTokenOwnerChanged(ControlTokenIdentity? owner);
}

/// <summary>
/// Routes <see cref="ThemeScope.TokenOwnerProperty" /> changes to the per-target bindings of an element through one
/// static property observer instead of one <c>PropertyChanged</c> handler per binding: an element with N owner
/// bindings used to run N handlers for every property change (Bounds, IsPointerOver, …); now no per-element
/// handler exists and only an owner change costs one attached-property read on the element.
/// </summary>
internal static class TokenOwnerListeners
{
    private static readonly AttachedProperty<Listeners?> ListenersProperty =
        AvaloniaProperty.RegisterAttached<ThemeConfigProvider, StyledElement, Listeners?>("TokenOwnerListeners");

    // Kept alive for the process; the class-level observable fires for every element's owner change.
    private static readonly IDisposable s_ownerChanged = ThemeScope.TokenOwnerProperty.Changed
        .AddClassHandler<StyledElement>(static (element, e) =>
            element.GetValue(ListenersProperty)?.Notify((ControlTokenIdentity?)e.NewValue));

    internal static void Add(StyledElement element, ITokenOwnerListener listener)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(listener);
        _ = s_ownerChanged;
        var listeners = element.GetValue(ListenersProperty);
        if (listeners is null)
        {
            listeners = new Listeners();
            element.SetValue(ListenersProperty, listeners);
        }
        listeners.Add(listener);
    }

    internal static void Remove(StyledElement element, ITokenOwnerListener listener)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (element.GetValue(ListenersProperty) is { } listeners && listeners.Remove(listener) && listeners.Count == 0)
        {
            element.ClearValue(ListenersProperty);
        }
    }

    /// <summary>
    /// The number of registered listeners; contract tests pin that bindings release theirs.
    /// </summary>
    internal static int Count(StyledElement element) => element.GetValue(ListenersProperty)?.Count ?? 0;

    private sealed class Listeners
    {
        private ITokenOwnerListener[] _items = [];

        internal int Count => _items.Length;

        internal void Add(ITokenOwnerListener listener)
        {
            var items = new ITokenOwnerListener[_items.Length + 1];
            _items.CopyTo(items, 0);
            items[^1] = listener;
            _items = items;
        }

        internal bool Remove(ITokenOwnerListener listener)
        {
            var index = Array.IndexOf(_items, listener);
            if (index < 0)
            {
                return false;
            }
            var items = new ITokenOwnerListener[_items.Length - 1];
            Array.Copy(_items, 0, items, 0, index);
            Array.Copy(_items, index + 1, items, index, _items.Length - index - 1);
            _items = items;
            return true;
        }

        internal void Notify(ControlTokenIdentity? owner)
        {
            // Copy-on-write array: a listener that unsubscribes while being notified does not disturb the iteration.
            foreach (var listener in _items)
            {
                listener.OnTokenOwnerChanged(owner);
            }
        }
    }
}
