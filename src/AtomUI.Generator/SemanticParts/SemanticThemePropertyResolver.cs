using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal static class SemanticThemePropertyResolver
{
    internal static IPropertySymbol? FindInstanceProperty(INamedTypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var property = current.GetMembers(name).OfType<IPropertySymbol>().FirstOrDefault();
            if (property is not null)
            {
                return property.IsStatic ? null : property;
            }
        }
        return null;
    }

    internal static bool IsSameSlot(IPropertySymbol? left, IPropertySymbol? right) =>
        left is not null && right is not null && SymbolEqualityComparer.Default.Equals(Slot(left), Slot(right));

    private static IPropertySymbol Slot(IPropertySymbol property)
    {
        // A qualified base setter still dispatches a real override. A new/hidden property is a
        // different slot even when its name and ControlTheme type happen to be identical.
        while (property.OverriddenProperty is { } overridden)
        {
            property = overridden;
        }

        return property;
    }
}
