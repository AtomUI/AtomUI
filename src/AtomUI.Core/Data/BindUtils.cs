using Avalonia;
using Avalonia.Data;

namespace AtomUI.Data;

public static class BindUtils
{
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

        var binding = new Binding
        {
            Source   = source,
            Path     = sourceProperty.Name,
            Priority = priority,
            Mode     = mode
        };
        return target.Bind(targetProperty, binding);
    }
    
    public static IDisposable RelayBind(object source, 
                                        string sourcePropertyName, 
                                        AvaloniaObject target,
                                        AvaloniaProperty targetProperty,
                                        BindingMode mode = BindingMode.Default,
                                        BindingPriority priority = BindingPriority.LocalValue)
    {
        var binding = new Binding
        {
            Source   = source,
            Path     = sourcePropertyName,
            Priority = priority,
            Mode     = mode
        };
        return target.Bind(targetProperty, binding);
    }

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
}