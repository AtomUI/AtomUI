using System.Reactive.Disposables;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Data;

/// <summary>
/// Projects fixed Token keys from one resource observer onto several Avalonia property bindings.
/// The property store still handles priority, conversion and binding values.
/// </summary>
internal sealed class OwnerTokenPropertyBindings : IDisposable
{
    private readonly StyledElement _target;
    private readonly BindingPriority _priority;
    private readonly OwnerTokenSet _tokens;
    private readonly Output?[] _outputs;

    internal OwnerTokenPropertyBindings(StyledElement target, int count, BindingPriority priority)
    {
        _target = target;
        _priority = priority;
        _outputs = new Output?[count];
        _tokens = new OwnerTokenSet(target, count, OnTokenChanged);
    }

    internal void Bind(int index, AvaloniaProperty property, object key)
    {
        if (_outputs[index] is { Binding: { } previous } existing)
        {
            existing.Binding = null;
            previous.Dispose();
        }
        var output = GetOutput(index, key);
        output.Binding = _target.Bind(property, output, _priority);
    }

    /// <summary>
    /// Shares a fixed key with several output properties whose bindings are owned by the caller.
    /// </summary>
    internal IObservable<object?> Observe(int index, object key) => GetOutput(index, key);

    internal void ClearKey(int index) => _tokens.SetResourceKey(index, null);

    private Output GetOutput(int index, object key)
    {
        _tokens.SetResourceKey(index, key);
        return _outputs[index] ??= new Output(_tokens, index);
    }

    public void Dispose()
    {
        _tokens.Dispose();
        foreach (var output in _outputs)
        {
            output?.Binding?.Dispose();
        }
        Array.Clear(_outputs);
    }

    private void OnTokenChanged(int index, object? oldValue, object? value) => _outputs[index]?.Publish(value);

    private sealed class Output(OwnerTokenSet tokens, int index) : IObservable<object?>
    {
        private Action<object?>? _notify;

        internal IDisposable? Binding { get; set; }

        public IDisposable Subscribe(IObserver<object?> observer)
        {
            Action<object?> notify = observer.OnNext;
            _notify += notify;
            try
            {
                notify(tokens.GetValue(index) ?? AvaloniaProperty.UnsetValue);
            }
            catch
            {
                _notify -= notify;
                throw;
            }
            return Disposable.Create(() => _notify -= notify);
        }

        internal void Publish(object? value) => _notify?.Invoke(value ?? AvaloniaProperty.UnsetValue);
    }
}
