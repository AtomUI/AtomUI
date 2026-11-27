using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Wraps a value factory as an Avalonia <see cref="ITemplate" /> so that it can be the value of a <c>Setter</c>.
/// Avalonia builds a template setter value for every styled target, so each target receives a fresh object
/// from the factory instead of sharing one instance.
/// </summary>
/// <typeparam name="T">
/// The type of object the factory produces.
/// </typeparam>
public class SetterValueFactory<T> : ITemplate
    where T : class
{
    private Func<T?> _valueFactory;
    
    /// <summary>
    /// Creates a wrapper that calls <paramref name="factory" /> on every <see cref="Build" />.
    /// </summary>
    /// <param name="factory">
    /// Produces the value for one styled target.
    /// </param>
    public SetterValueFactory(Func<T?> factory)
    {
        _valueFactory = factory;
    }
    
    /// <summary>
    /// Returns the object the factory produces for one styled target.
    /// </summary>
    public object? Build()
    {
        return _valueFactory();
    }
}