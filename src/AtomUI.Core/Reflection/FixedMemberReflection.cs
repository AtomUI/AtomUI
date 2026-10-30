using System.Reflection;

namespace AtomUI.Reflection;

internal static class FixedMemberReflection
{
    internal static MethodInfo RequireMethod(MethodInfo? member, Type owner, string name) =>
        member ?? throw new MissingMethodException(owner.FullName, name);

    internal static FieldInfo RequireField(FieldInfo? member, Type owner, string name) =>
        member ?? throw new MissingFieldException(owner.FullName, name);

    internal static PropertyInfo RequireProperty(PropertyInfo? member, Type owner, string name) =>
        member ?? throw Missing(owner, name, "property");

    internal static EventInfo RequireEvent(EventInfo? member, Type owner, string name) =>
        member ?? throw Missing(owner, name, "event");

    private static MissingMemberException Missing(Type owner, string name, string kind) =>
        new($"The {kind} '{owner.FullName}.{name}' was not found.");
}
