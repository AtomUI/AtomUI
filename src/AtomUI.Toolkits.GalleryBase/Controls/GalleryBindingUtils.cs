using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Binding helpers for Gallery views: one-way relays from an <see cref="INotifyPropertyChanged" /> source to
/// an Avalonia property, and a button command assignment that a disposable undoes.
/// </summary>
public static class GalleryBindingUtils
{
    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> one-way to the value that
    /// <paramref name="valueGetter" /> reads from <paramref name="source" />. The getter runs when the relay is
    /// created and again after each change the source reports for the property named
    /// <paramref name="sourcePropertyName" />, or with an empty name, which stands for all properties. The name is
    /// only compared with the reported name and is never resolved by reflection. Values are written at
    /// <paramref name="priority" />.
    /// </summary>
    /// <param name="source">
    /// The object whose property changes are observed.
    /// </param>
    /// <param name="sourcePropertyName">
    /// The property name that the source reports.
    /// </param>
    /// <param name="valueGetter">
    /// Reads the value to push from the source.
    /// </param>
    /// <param name="target">
    /// The object that receives the value.
    /// </param>
    /// <param name="targetProperty">
    /// The property of the target that receives the value.
    /// </param>
    /// <param name="priority">
    /// The priority at which the value is written to the target property.
    /// </param>
    /// <returns>
    /// A disposable that ends the relay.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source" /> or <paramref name="valueGetter" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sourcePropertyName" /> is <see langword="null" />, empty or white space only,
    /// or when <paramref name="targetProperty" /> is neither attached nor registered for the type of
    /// <paramref name="target" />.
    /// </exception>
    public static IDisposable OneWay<TSource, TValue>(
        TSource source,
        string sourcePropertyName,
        Func<TSource, TValue> valueGetter,
        AvaloniaObject target,
        AvaloniaProperty<TValue> targetProperty,
        BindingPriority priority = BindingPriority.LocalValue)
        where TSource : class, INotifyPropertyChanged
    {
        return BindUtils.RelayBind(source, sourcePropertyName, valueGetter, target, targetProperty, priority);
    }

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> one-way to the value that
    /// <paramref name="valueGetter" /> reads from <paramref name="source" />. The getter runs when the relay is
    /// created and again after each change the source reports for the property named
    /// <paramref name="sourcePropertyName" />, or with an empty name, which stands for all properties. The name is
    /// only compared with the reported name and is never resolved by reflection. Values are written at
    /// <paramref name="priority" />.
    /// </summary>
    /// <param name="source">
    /// The object whose property changes are observed.
    /// </param>
    /// <param name="sourcePropertyName">
    /// The property name that the source reports.
    /// </param>
    /// <param name="valueGetter">
    /// Reads the value to push from the source.
    /// </param>
    /// <param name="target">
    /// The object that receives the value.
    /// </param>
    /// <param name="targetProperty">
    /// The property of the target that receives the value.
    /// </param>
    /// <param name="priority">
    /// The priority at which the value is written to the target property.
    /// </param>
    /// <returns>
    /// A disposable that ends the relay.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source" />, <paramref name="valueGetter" />, <paramref name="target" /> or
    /// <paramref name="targetProperty" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sourcePropertyName" /> is <see langword="null" />, empty or white space only.
    /// </exception>
    public static IDisposable OneWay<TSource>(
        TSource source,
        string sourcePropertyName,
        Func<TSource, object?> valueGetter,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        BindingPriority priority = BindingPriority.LocalValue)
        where TSource : class, INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePropertyName);
        ArgumentNullException.ThrowIfNull(valueGetter);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(targetProperty);

        return target.Bind(
            targetProperty,
            new PropertyChangedValueObservable<TSource>(source, sourcePropertyName, valueGetter),
            priority);
    }

    /// <summary>
    /// Assigns the command to <see cref="Button.CommandProperty" /> on the target and returns a disposable that
    /// restores the command that was in effect before the call.
    /// </summary>
    /// <param name="target">
    /// The object that carries the property, normally a button.
    /// </param>
    /// <param name="command">
    /// The command to assign, or <see langword="null" /> to clear it.
    /// </param>
    /// <returns>
    /// A disposable that restores the previous command.
    /// </returns>
    public static IDisposable BindCommand(AvaloniaObject target, ICommand? command)
    {
        ArgumentNullException.ThrowIfNull(target);

        var previousCommand = target.GetValue(Button.CommandProperty);
        target.SetCurrentValue(Button.CommandProperty, command);
        return Disposable.Create(() => target.SetCurrentValue(Button.CommandProperty, previousCommand));
    }

    private sealed class PropertyChangedValueObservable<TSource> : IObservable<object?>
        where TSource : class, INotifyPropertyChanged
    {
        private readonly TSource _source;
        private readonly string _sourcePropertyName;
        private readonly Func<TSource, object?> _valueGetter;

        public PropertyChangedValueObservable(TSource source, string sourcePropertyName, Func<TSource, object?> valueGetter)
        {
            _source             = source;
            _sourcePropertyName = sourcePropertyName;
            _valueGetter        = valueGetter;
        }

        public IDisposable Subscribe(IObserver<object?> observer)
        {
            observer.OnNext(_valueGetter(_source));

            void HandlePropertyChanged(object? sender, PropertyChangedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.PropertyName) && args.PropertyName != _sourcePropertyName)
                {
                    return;
                }

                observer.OnNext(_valueGetter(_source));
            }

            _source.PropertyChanged += HandlePropertyChanged;
            return Disposable.Create(() => _source.PropertyChanged -= HandlePropertyChanged);
        }
    }
}
