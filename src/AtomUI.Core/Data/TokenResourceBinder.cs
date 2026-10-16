using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using AtomUI.Theme.Resources;

namespace AtomUI.Data;

public static class TokenResourceBinder
{
    public static IDisposable CreateGlobalTokenBinding(
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        SharedTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
    {
        return BindResource(target, targetProperty, resourceKey, priority, converter);
    }

    internal static IDisposable CreateGlobalTokenBinding(
        Control context,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        SharedTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
    {
        return BindResource(target, targetProperty, resourceKey, priority, converter, context);
    }

    internal static IDisposable CreateControlTokenBinding(
        Type controlType,
        Control context,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        SharedTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        ArgumentNullException.ThrowIfNull(context);
        var resourceKeyObject = ControlTokenResourceKey.Global(controlType, resourceKey);
        return BindResource(target, targetProperty, resourceKeyObject, priority, converter, context);
    }

    public static IDisposable CreateControlTokenBinding(
        Control target,
        AvaloniaProperty targetProperty,
        SharedTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
    {
        return CreateControlTokenBinding(target, target, targetProperty, resourceKey, priority, converter);
    }

    public static IDisposable CreateControlTokenBinding(
        Control owner,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        SharedTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
    {
        var resourceKeyObject = ControlTokenResourceKey.Global(owner.GetType(), resourceKey);
        return BindResource(target, targetProperty, resourceKeyObject, priority, converter, owner);
    }

    public static IDisposable CreateControlTokenBinding<TTokenKind>(
        Control target,
        AvaloniaProperty targetProperty,
        TTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
        where TTokenKind : Enum
    {
        return CreateControlTokenBinding(target, target, targetProperty, resourceKey, priority, converter);
    }

    public static IDisposable CreateControlTokenBinding<TTokenKind>(
        Control owner,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        TTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
        where TTokenKind : Enum
    {
        var resourceKeyObject = ControlTokenResourceKey.Own(owner.GetType(), resourceKey);
        return BindResource(target, targetProperty, resourceKeyObject, priority, converter, owner);
    }

    internal static IDisposable CreateControlTokenBinding<TTokenKind>(
        Type controlType,
        Control context,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        TTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
        where TTokenKind : Enum
    {
        var resourceKeyObject = ControlTokenResourceKey.Own(controlType, resourceKey);
        return BindResource(target, targetProperty, resourceKeyObject, priority, converter, context);
    }

    public static IDisposable CreateGlobalResourceBinding(AvaloniaObject target,
                                                          AvaloniaProperty targetProperty,
                                                          object resourceKey,
                                                          BindingPriority priority = BindingPriority.Template,
                                                          Func<object?, object?>? converter = null)
    {
        return target.Bind(targetProperty, GetGlobalResourceObservable(resourceKey, null, converter), priority);
    }

    private static IDisposable BindResource(
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        object resourceKey,
        BindingPriority priority,
        Func<object?, object?>? converter,
        Control? owner = null)
    {
        if (owner is not null)
        {
            return target.Bind(targetProperty, owner.GetResourceObservable(resourceKey, converter), priority);
        }

        if (target is Control control)
        {
            return target.Bind(targetProperty, control.GetResourceObservable(resourceKey, converter), priority);
        }

        if (target is IResourceHost resourceHost)
        {
            return target.Bind(targetProperty, resourceHost.GetResourceObservable(resourceKey, converter), priority);
        }
        if (target is IResourceProvider provider)
        {
            return target.Bind(targetProperty, provider.GetResourceObservable(resourceKey, converter), priority);
        }
        return target.Bind(targetProperty, GetGlobalResourceObservable(resourceKey, converter: converter), priority);
    }

    public static IObservable<object?> GetGlobalResourceObservable(object resourceKey,
                                                                   ThemeVariant? themeVariant = null,
                                                                   Func<object?, object?>? converter = null)
    {
        var application = Application.Current;
        if (application is null)
        {
            throw new ApplicationException("The application instance does not exist");
        }

        return themeVariant is null
            ? application.Styles.GetResourceObservable(resourceKey, converter)
            : application.Styles.GetResourceObservable(resourceKey, themeVariant, converter);
    }
}
