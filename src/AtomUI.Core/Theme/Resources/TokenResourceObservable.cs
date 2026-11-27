using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Resource observable behind <c>TokenResourceBinder</c> bindings whose host is a <see cref="StyledElement" />.
/// </summary>
/// <remarks>
/// Resolves the key exactly like Avalonia's <c>ResourceObservable</c> (the host's resource chain, the host's actual
/// theme variant, the optional converter) and republishes on <see cref="StyledElement.ResourcesChanged" /> and
/// <see cref="StyledElement.ActualThemeVariantChanged" />. The one difference: a <c>ResourcesChanged</c> raised by a
/// theme publish is checked against the publish's <see cref="ThemePublishDelta" /> and skipped when the key's value
/// did not change; every other notification resolves fully. Avalonia keeps its observable base internal, so this
/// type carries its own observer list.
/// </remarks>
internal sealed class TokenResourceObservable : IObservable<object?>
{
    private readonly StyledElement _host;
    private readonly object _key;
    private readonly Func<object?, object?>? _converter;
    private IObserver<object?>[] _observers = [];
    private ThemePublishTracker _publishes;

    internal TokenResourceObservable(StyledElement host, object key, Func<object?, object?>? converter)
    {
        _host      = host ?? throw new ArgumentNullException(nameof(host));
        _key       = key ?? throw new ArgumentNullException(nameof(key));
        _converter = converter;
    }

    /// <summary>
    /// How often the observable resolved its resource; contract tests pin the publish early-exit with it.
    /// </summary>
    internal int ResolveCount { get; private set; }

    public IDisposable Subscribe(IObserver<object?> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (_observers.Length == 0)
        {
            _host.ResourcesChanged          += OnResourcesChanged;
            _host.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        }
        _observers = [.. _observers, observer];
        observer.OnNext(Resolve(ThemeScope.ResolveContext(_host)));
        return new Subscription(this, observer);
    }

    private void Remove(IObserver<object?> observer)
    {
        var index = Array.IndexOf(_observers, observer);
        if (index < 0)
        {
            return;
        }
        var observers = new IObserver<object?>[_observers.Length - 1];
        Array.Copy(_observers, 0, observers, 0, index);
        Array.Copy(_observers, index + 1, observers, index, _observers.Length - index - 1);
        _observers = observers;
        if (_observers.Length == 0)
        {
            _host.ResourcesChanged          -= OnResourcesChanged;
            _host.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        }
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        var context = ThemeScope.ResolveContext(_host);
        var delta = _publishes.Observe(context);
        if (delta is not null && !delta.IsChanged(_key))
        {
            return;
        }
        Publish(Resolve(context));
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) =>
        Publish(Resolve(ThemeScope.ResolveContext(_host)));

    private object? Resolve(ThemeContext? context)
    {
        ResolveCount++;
        _publishes.MarkCurrent(context);
        // As Avalonia's ResourceObservable (FindResource(theme, key) ?? UnsetValue): a missing key and a key whose
        // value is null both unset the binding, so lower-priority values stay reachable; the converter sees UnsetValue.
        var value = _host.TryFindResource(_key, _host.ActualThemeVariant, out var resolved)
            ? resolved ?? AvaloniaProperty.UnsetValue
            : AvaloniaProperty.UnsetValue;
        return _converter?.Invoke(value) ?? value;
    }

    private void Publish(object? value)
    {
        // The array is replaced on every change, so an observer that unsubscribes while being notified is harmless.
        foreach (var observer in _observers)
        {
            observer.OnNext(value);
        }
    }

    private sealed class Subscription(TokenResourceObservable owner, IObserver<object?> observer) : IDisposable
    {
        private TokenResourceObservable? _owner = owner;

        public void Dispose()
        {
            var current = Interlocked.Exchange(ref _owner, null);
            current?.Remove(observer);
        }
    }
}
