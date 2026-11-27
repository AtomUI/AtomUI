using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using AtomUI.Theme.Resources;

namespace AtomUI.Data;

/// <summary>
/// Code bindings from an Avalonia property to a Token resource or to an Application resource.
/// Dispose the returned object to remove the binding.
/// </summary>
public static class TokenResourceBinder
{
    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> to the shared Token
    /// <paramref name="resourceKey" />, for example <see cref="SharedTokenKind.FontSizeLG" />, and keeps the
    /// value current when the resource changes. The Token is looked up from the target: a control, resource
    /// host or resource provider sees the resources in its own scope, and any other object reads the
    /// Application resources.
    /// </summary>
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

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on the control <paramref name="target" /> to the shared
    /// Token <paramref name="resourceKey" />, resolved exactly as an AXAML <c>TokenResource</c> of that key
    /// on the control resolves it: Application, scoped and <c>ThemeConfigProvider</c> resources along the
    /// control's tree apply, and the value follows resource changes.
    /// </summary>
    public static IDisposable CreateControlTokenBinding(
        Control target,
        AvaloniaProperty targetProperty,
        SharedTokenKind resourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
    {
        return CreateControlTokenBinding(target, target, targetProperty, resourceKey, priority, converter);
    }

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> to the shared
    /// Token <paramref name="resourceKey" /> of the control <paramref name="owner" />, resolved in the scope
    /// of the owner rather than of the target, exactly as an AXAML <c>TokenResource</c> of that key on the
    /// owner resolves it. The value follows resource changes.
    /// </summary>
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

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on the control <paramref name="target" /> to the Token that
    /// the control defines itself, <paramref name="resourceKey" />, resolved exactly as an AXAML
    /// <c>TokenResource</c> of that key on the control resolves it. The value follows resource changes.
    /// </summary>
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

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> to the Token that the control
    /// <paramref name="owner" /> defines itself, <paramref name="resourceKey" />, resolved in the scope of
    /// the owner rather than of the target, exactly as an AXAML <c>TokenResource</c> of that key on the
    /// owner resolves it. The value follows resource changes.
    /// </summary>
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

    /// <summary>
    /// Binds <paramref name="targetProperty" /> to a Token resource key resolved from <paramref name="owner" />,
    /// exactly as an AXAML TokenResource of that key resolves it: Application, scoped and <c>ThemeConfigProvider</c>
    /// resources along the owner's tree apply, and the value follows resource changes. Used by control code whose theme
    /// variables pick the Token key at runtime (for example a variant color).
    /// </summary>
    internal static IDisposable CreateTokenResourceBinding(
        Control owner,
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        object tokenResourceKey,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(tokenResourceKey);
        return BindResource(target, targetProperty, tokenResourceKey, priority, converter, owner);
    }

    /// <summary>
    /// Binds <paramref name="targetProperty" /> on <paramref name="target" /> to the Application-level
    /// resource <paramref name="resourceKey" />, ignoring resources scoped to a control or to a
    /// <c>ThemeConfigProvider</c>, and keeps the value current when the resource changes.
    /// </summary>
    /// <exception cref="ApplicationException">
    /// Thrown when there is no <see cref="Application.Current" />.
    /// </exception>
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
        // A StyledElement host gets the AtomUI observable, which skips theme publishes that leave the key's value
        // unchanged; other hosts keep Avalonia's resource observable.
        if (owner is not null)
        {
            return target.Bind(targetProperty, new TokenResourceObservable(owner, resourceKey, converter), priority);
        }

        if (target is Control control)
        {
            return target.Bind(targetProperty, new TokenResourceObservable(control, resourceKey, converter), priority);
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

    /// <summary>
    /// Observes the Application-level resource <paramref name="resourceKey" /> for
    /// <paramref name="themeVariant" />, or for the application's current theme variant when it is
    /// <see langword="null" />.
    /// </summary>
    /// <returns>
    /// An observable that emits the current value and then every later change of the resource, each passed
    /// through <paramref name="converter" /> when it is given.
    /// </returns>
    /// <exception cref="ApplicationException">
    /// Thrown when there is no <see cref="Application.Current" />.
    /// </exception>
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
