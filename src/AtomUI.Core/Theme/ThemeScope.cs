using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Theme;

internal static class ThemeScope
{
    public static readonly AttachedProperty<ThemeContext?> ContextProperty =
        AvaloniaProperty.RegisterAttached<ThemeConfigProvider, StyledElement, ThemeContext?>(
            "Context",
            inherits: true);

    internal static ThemeContext? ResolveContext(
        StyledElement owner,
        ILogicalRoot? knownRoot = null)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (owner.GetValue(ContextProperty) is { } inheritedContext)
        {
            return inheritedContext;
        }

        var root = knownRoot ?? FindLogicalRoot(owner);
        if (root is not TopLevel topLevel)
        {
            return null;
        }

        if (topLevel.GetValue(ContextProperty) is { } rootContext)
        {
            return rootContext;
        }

        // Resolve through the application's own scope, which unregisters on disposal and failed commit;
        // a process-wide locator entry would outlive the manager and serve another application's instance.
        if (Application.Current is not { } application ||
            ApplicationScopeRegistry.Get(application)?.ThemeManager is not { } manager)
        {
            return null;
        }

        rootContext = manager.RootContext;
        topLevel.SetValue(ContextProperty, rootContext);
        return rootContext;
    }

    private static ILogicalRoot? FindLogicalRoot(StyledElement owner)
    {
        ILogical? current = owner;
        while (current is not null)
        {
            if (current is ILogicalRoot root)
            {
                return root;
            }
            current = current.GetLogicalParent();
        }
        return null;
    }
}
