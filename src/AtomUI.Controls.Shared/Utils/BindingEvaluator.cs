using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Controls.Utils;

/// <summary>
/// Resolves a binding for a data item without being attached to the visual tree and returns the value; ComboBox
/// and ListView in AtomUI.Desktop.Controls use it to resolve the display text or the selected value of an item.
/// </summary>
/// <typeparam name="T">
/// Type of the value that the binding produces.
/// </typeparam>
/// <remarks>
/// <see cref="UpdateBinding" /> binds <see cref="Value" /> to the given binding and reuses the existing expression
/// while the same binding instance is passed. <see cref="Evaluate" /> moves the evaluator to the given data item
/// and returns the value that the binding produced for it. Disposing the evaluator releases the binding expression.
/// </remarks>
internal sealed class BindingEvaluator<T> : StyledElement, IDisposable
{
    private BindingExpressionBase? _expression;
    private BindingBase? _lastBinding;

    [SuppressMessage(
        "AvaloniaProperty",
        "AVP1002:AvaloniaProperty objects should not be owned by a generic type",
        Justification = "This property is not supposed to be used from XAML.")]
    public static readonly StyledProperty<T> ValueProperty =
        AvaloniaProperty.Register<BindingEvaluator<T>, T>("Value");

    /// <summary>
    /// Target of the binding. Holds the value that it produces for the data item last passed to
    /// <see cref="Evaluate" />.
    /// </summary>
    public T Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public T Evaluate(object? dataContext)
    {
        // Only update the DataContext if necessary
        if (!Equals(dataContext, DataContext))
        {
            DataContext = dataContext;
        }

        return GetValue(ValueProperty);
    }

    public void UpdateBinding(BindingBase binding)
    {
        if (binding == _lastBinding)
        {
            return;
        }

        _expression?.Dispose();
        _expression = Bind(ValueProperty, binding);
        _lastBinding = binding;
    }

    public void ClearDataContext()
        => DataContext = null;

    public void Dispose()
    {
        _expression?.Dispose();
        _expression = null;
        _lastBinding = null;
        DataContext = null;
    }

    [return: NotNullIfNotNull(nameof(binding))]
    public static BindingEvaluator<T>? TryCreate(BindingBase? binding)
    {
        if (binding is null)
        {
            return null;
        }

        var evaluator = new BindingEvaluator<T>();
        evaluator.UpdateBinding(binding);
        return evaluator;
    }
}
