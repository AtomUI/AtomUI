using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Data;
using Avalonia.VisualTree;

namespace AtomUI.Data;

/// <summary>
/// Code-created relays that keep one Avalonia property following the value of another object.
/// Dispose the object that a relay returns to end it.
/// </summary>
public static class BindUtils
{
    /// <summary>
    /// Relays the value of <paramref name="sourceProperty" /> on <paramref name="source" /> to a property on
    /// <paramref name="target" />, in the direction that <paramref name="mode" /> selects.
    /// <see cref="BindingMode.Default" /> uses the default binding mode of the target property, and a
    /// <see langword="null" /> <paramref name="targetProperty" /> selects <paramref name="sourceProperty" />.
    /// <see cref="BindingMode.OneTime" /> copies the current source value once, at
    /// <paramref name="priority" />.
    /// <see cref="BindingMode.TwoWay" /> and <see cref="BindingMode.OneWayToSource" /> also copy every change
    /// the target reports afterwards to the source property at <see cref="BindingPriority.LocalValue" />.
    /// A read-only source property is not written, and the value the target has when the relay is created is
    /// not copied to the source.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when a property that is not attached is not registered on the object that carries it.
    /// </exception>
    public static IDisposable RelayBind(AvaloniaObject source, 
                                        AvaloniaProperty sourceProperty, 
                                        AvaloniaObject target,
                                        AvaloniaProperty? targetProperty = null,
                                        BindingMode mode = BindingMode.Default,
                                        BindingPriority priority = BindingPriority.LocalValue)
    {
        targetProperty ??= sourceProperty;
        var registry = AvaloniaPropertyRegistry.Instance;
        if (!sourceProperty.IsAttached)
        {
            if (!registry.IsRegistered(source.GetType(), sourceProperty))
            {
                throw new ArgumentException($"Relay source property is not registered for: {source.GetType().Name}.");
            }
        }

        if (!targetProperty.IsAttached)
        {
            if (!registry.IsRegistered(target.GetType(), targetProperty))
            {
                throw new ArgumentException($"Relay target property is not registered for: {target.GetType().Name}.");
            }
        }

        mode = ResolveBindingMode(target, targetProperty, mode);
        return mode switch
        {
            BindingMode.OneWay         => target.Bind(targetProperty, source.GetObservable(sourceProperty), priority),
            BindingMode.TwoWay         => TwoWayRelayBind(source, sourceProperty, target, targetProperty, priority),
            BindingMode.OneWayToSource => OneWayToSourceRelayBind(source, sourceProperty, target, targetProperty),
            BindingMode.OneTime        => OneTimeRelayBind(source, sourceProperty, target, targetProperty, priority),
            _                          => throw new NotSupportedException($"{nameof(RelayBind)} does not support {mode}.")
        };
    }

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> to the ancestor of
    /// <paramref name="relativeTo" /> in the visual tree whose type is <paramref name="ancestorType" />.
    /// The value follows later changes of that ancestor and is <see langword="null" /> while no such
    /// ancestor exists. <paramref name="ancestorLevel" /> 1 selects the nearest matching ancestor, 2 the
    /// one above it, and so on. Values are written at <paramref name="priority" />.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ancestorLevel" /> is less than 1.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="ancestorType" /> is not assignable to <typeparamref name="TResult" />, or
    /// when a target property that is not attached is not registered on <paramref name="target" />.
    /// </exception>
    public static IDisposable BindVisualAncestor<TResult>(
        Visual relativeTo,
        AvaloniaObject target,
        AvaloniaProperty<TResult?> targetProperty,
        Type ancestorType,
        int ancestorLevel = 1,
        BindingPriority priority = BindingPriority.LocalValue)
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(relativeTo);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(targetProperty);
        ArgumentNullException.ThrowIfNull(ancestorType);
        if (ancestorLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ancestorLevel), "AncestorLevel may not be set to less than 1.");
        }

        if (!typeof(TResult).IsAssignableFrom(ancestorType))
        {
            throw new ArgumentException(
                $"Ancestor type {ancestorType.Name} cannot be assigned to {typeof(TResult).Name}.",
                nameof(ancestorType));
        }

        var registry = AvaloniaPropertyRegistry.Instance;
        if (!targetProperty.IsAttached && !registry.IsRegistered(target.GetType(), targetProperty))
        {
            throw new ArgumentException($"Relay target property is not registered for: {target.GetType().Name}.");
        }

        return target.Bind(
            targetProperty,
            VisualLocator.Track(relativeTo, ancestorLevel - 1, ancestorType)
                         .Select(static ancestor => (TResult?)(object?)ancestor),
            priority);
    }

    private static BindingMode ResolveBindingMode(AvaloniaObject target, AvaloniaProperty targetProperty, BindingMode mode)
    {
        return mode == BindingMode.Default
            ? targetProperty.GetMetadata(target).DefaultBindingMode
            : mode;
    }

    private static IDisposable TwoWayRelayBind(
        AvaloniaObject source,
        AvaloniaProperty sourceProperty,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        BindingPriority priority)
    {
        var sourceToTarget = target.Bind(targetProperty, source.GetObservable(sourceProperty), priority);
        var targetToSource = SubscribeChangesAfterInitialValue(
            target,
            targetProperty,
            value => SetBindingSourceValue(source, sourceProperty, value));

        return Disposable.Create(() =>
        {
            targetToSource.Dispose();
            sourceToTarget.Dispose();
        });
    }

    private static IDisposable OneWayToSourceRelayBind(
        AvaloniaObject source,
        AvaloniaProperty sourceProperty,
        AvaloniaObject target,
        AvaloniaProperty targetProperty)
    {
        return SubscribeChangesAfterInitialValue(
            target,
            targetProperty,
            value => SetBindingSourceValue(source, sourceProperty, value));
    }

    private static IDisposable OneTimeRelayBind(
        AvaloniaObject source,
        AvaloniaProperty sourceProperty,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        BindingPriority priority)
    {
        var disposable = target.SetValue(targetProperty, source.GetValue(sourceProperty), priority);
        if (disposable is not null)
        {
            return disposable;
        }

        return priority == BindingPriority.LocalValue || targetProperty.IsDirect
            ? Disposable.Create(() => target.ClearValue(targetProperty))
            : Disposable.Empty;
    }
    
    /// <summary>
    /// Removed string-path overload. Reflecting on a property name is not AOT-safe, so using it is a compile
    /// error and a call always throws <see cref="NotSupportedException" />. Use the overload that takes an
    /// <see cref="AvaloniaProperty" />, or the one that takes a getter delegate.
    /// </summary>
    [Obsolete("String-path relay binding is not AOT-safe. Use an AvaloniaProperty overload or the strongly typed getter overload.", true)]
    public static IDisposable RelayBind(object source,
                                        string sourcePropertyName,
                                        AvaloniaObject target,
                                        AvaloniaProperty targetProperty,
                                        BindingMode mode = BindingMode.Default,
                                        BindingPriority priority = BindingPriority.LocalValue)
    {
        throw new NotSupportedException(
            "String-path relay binding is not AOT-safe. Use an AvaloniaProperty overload or the strongly typed getter overload.");
    }

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> one-way to the value that
    /// <paramref name="valueGetter" /> returns for <paramref name="source" />, at
    /// <paramref name="priority" />. The getter runs once when the relay is created. When
    /// <paramref name="source" /> implements <see cref="INotifyPropertyChanged" />, it runs again after each
    /// change the source reports with the name <paramref name="sourcePropertyName" />, or with an empty name,
    /// which stands for all properties. A source that does not implement it produces the initial value only.
    /// The name is only compared with the reported name and is never resolved by reflection.
    /// </summary>
    public static IDisposable RelayBind<TSource, TResult>(TSource source,
                                                          string sourcePropertyName,
                                                          Func<TSource, TResult> valueGetter,
                                                          AvaloniaObject target,
                                                          AvaloniaProperty<TResult> targetProperty,
                                                          BindingPriority priority = BindingPriority.LocalValue)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePropertyName);
        ArgumentNullException.ThrowIfNull(valueGetter);

        var registry = AvaloniaPropertyRegistry.Instance;
        if (!targetProperty.IsAttached && !registry.IsRegistered(target.GetType(), targetProperty))
        {
            throw new ArgumentException($"Relay target property is not registered for: {target.GetType().Name}.");
        }

        return target.Bind(
            targetProperty,
            new PropertyChangedValueObservable<TSource, TResult>(source, sourcePropertyName, valueGetter),
            priority);
    }

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> one-way to
    /// <paramref name="sourceProperty" /> on <paramref name="source" />, passing each value to
    /// <paramref name="converter" /> as a <typeparamref name="TSource" />. Values are written at
    /// <paramref name="priority" />.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the source or target property is not registered on the object that carries it.
    /// </exception>
    public static IDisposable RelayBind<TSource, TResult>(AvaloniaObject source,
                                                          AvaloniaProperty<TSource> sourceProperty,
                                                          AvaloniaObject target,
                                                          AvaloniaProperty<TResult> targetProperty,
                                                          Func<TSource, TResult> converter,
                                                          BindingPriority priority = BindingPriority.LocalValue)
    {
        var registry = AvaloniaPropertyRegistry.Instance;
        if (!registry.IsRegistered(source.GetType(), sourceProperty))
        {
            throw new ArgumentException($"Relay source property is not registered for: {source.GetType().Name}.");
        }

        if (!registry.IsRegistered(target.GetType(), targetProperty))
        {
            throw new ArgumentException($"Relay target property is not registered for: {target.GetType().Name}.");
        }

        return target.Bind(targetProperty, source.GetObservable(sourceProperty, converter), priority);
    }

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> one-way to the untyped
    /// <paramref name="sourceProperty" /> on <paramref name="source" />, passing each value to
    /// <paramref name="converter" /> as an <see langword="object" />. Values are written at
    /// <paramref name="priority" />.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the source or target property is not registered on the object that carries it.
    /// </exception>
    public static IDisposable RelayBind<TResult>(AvaloniaObject source,
                                                 AvaloniaProperty sourceProperty,
                                                 AvaloniaObject target,
                                                 AvaloniaProperty<TResult> targetProperty,
                                                 Func<object?, TResult> converter,
                                                 BindingPriority priority = BindingPriority.LocalValue)
    {
        var registry = AvaloniaPropertyRegistry.Instance;
        if (!registry.IsRegistered(source.GetType(), sourceProperty))
        {
            throw new ArgumentException($"Relay source property is not registered for: {source.GetType().Name}.");
        }

        if (!registry.IsRegistered(target.GetType(), targetProperty))
        {
            throw new ArgumentException($"Relay target property is not registered for: {target.GetType().Name}.");
        }

        return target.Bind(targetProperty, source.GetObservable(sourceProperty, converter), priority);
    }

    private static IDisposable SubscribeChangesAfterInitialValue(
        AvaloniaObject source,
        AvaloniaProperty property,
        Action<object?> onChanged)
    {
        var isInitialValue = true;
        return source.GetObservable(property).Subscribe(value =>
        {
            if (isInitialValue)
            {
                isInitialValue = false;
                return;
            }

            onChanged(value);
        });
    }

    private static void SetBindingSourceValue(AvaloniaObject source, AvaloniaProperty sourceProperty, object? value)
    {
        if (sourceProperty.IsReadOnly)
        {
            return;
        }

        source.SetValue(sourceProperty, value, BindingPriority.LocalValue);
    }

    private sealed class PropertyChangedValueObservable<TSource, TResult> : IObservable<object?>
        where TSource : class
    {
        private readonly TSource _source;
        private readonly string _sourcePropertyName;
        private readonly Func<TSource, TResult> _valueGetter;

        public PropertyChangedValueObservable(
            TSource source,
            string sourcePropertyName,
            Func<TSource, TResult> valueGetter)
        {
            _source             = source;
            _sourcePropertyName = sourcePropertyName;
            _valueGetter        = valueGetter;
        }

        public IDisposable Subscribe(IObserver<object?> observer)
        {
            observer.OnNext(_valueGetter(_source));

            if (_source is not INotifyPropertyChanged notifyPropertyChanged)
            {
                return Disposable.Empty;
            }

            void HandlePropertyChanged(object? sender, PropertyChangedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.PropertyName) && args.PropertyName != _sourcePropertyName)
                {
                    return;
                }

                observer.OnNext(_valueGetter(_source));
            }

            notifyPropertyChanged.PropertyChanged += HandlePropertyChanged;
            return Disposable.Create(() =>
                notifyPropertyChanged.PropertyChanged -= HandlePropertyChanged);
        }
    }
}
