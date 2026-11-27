using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AtomUI.Reflection;

/// <summary>
/// Looks up the metadata of properties, fields, methods and events of a type. The <c>Try</c> methods
/// report a missing member through their return value; the <c>OrThrow</c> methods throw for one.
/// </summary>
public static class TypeMemberExtension
{
    /// <summary>
    /// Looks for the property <paramref name="name" /> on <paramref name="type" />. The default flags match
    /// non-public instance properties only, so <c>BindingFlags.Public</c> is needed to find a public property.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the metadata in <paramref name="info" /> when the property exists.
    /// <see langword="false" /> and <see langword="null" /> in <paramref name="info" /> otherwise.
    /// </returns>
    public static bool TryGetPropertyInfo(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        this Type type,
        string name,
        [NotNullWhen(true)] out PropertyInfo? info,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
    {
        info = type.GetProperty(name, flags);
        return info is not null;
    }

    /// <summary>
    /// Looks for the field <paramref name="name" /> on <paramref name="type" />. The default flags match
    /// non-public instance fields only, so <c>BindingFlags.Public</c> is needed to find a public field.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the metadata in <paramref name="info" /> when the field exists.
    /// <see langword="false" /> and <see langword="null" /> in <paramref name="info" /> otherwise.
    /// </returns>
    public static bool TryGetFieldInfo(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields |
                                    DynamicallyAccessedMemberTypes.NonPublicFields)]
        this Type type,
        string name,
        [NotNullWhen(true)] out FieldInfo? info,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
    {
        info = type.GetField(name, flags);
        return info is not null;
    }

    /// <summary>
    /// Looks for the method <paramref name="name" /> on <paramref name="type" />. The lookup is by name only,
    /// so the method must not be overloaded.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the metadata in <paramref name="info" /> when the method exists.
    /// <see langword="false" /> and <see langword="null" /> in <paramref name="info" /> otherwise.
    /// </returns>
    /// <exception cref="AmbiguousMatchException">
    /// More than one method with that name exists.
    /// </exception>
    public static bool TryGetMethodInfo(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                    DynamicallyAccessedMemberTypes.NonPublicMethods)]
        this Type type,
        string name,
        [NotNullWhen(true)] out MethodInfo? info,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                             BindingFlags.FlattenHierarchy)
    {
        info = type.GetMethod(name, flags);
        return info is not null;
    }
    
    /// <summary>
    /// Looks for the event <paramref name="name" /> on <paramref name="type" />.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the metadata in <paramref name="info" /> when the event exists.
    /// <see langword="false" /> and <see langword="null" /> in <paramref name="info" /> otherwise.
    /// </returns>
    public static bool TryGetEventInfo(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents |
                                    DynamicallyAccessedMemberTypes.NonPublicEvents)]
        this Type type,
        string name,
        [NotNullWhen(true)] out EventInfo? info,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                             BindingFlags.FlattenHierarchy)
    {
        info = type.GetEvent(name, flags);
        return info is not null;
    }

    /// <summary>
    /// Returns the metadata of the property <paramref name="name" /> of <paramref name="type" />.
    /// </summary>
    /// <exception cref="MissingMemberException">
    /// No property with that name exists.
    /// </exception>
    public static PropertyInfo GetPropertyInfoOrThrow(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        this Type type,
        string name,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                             BindingFlags.FlattenHierarchy)
    {
        if (!type.TryGetPropertyInfo(name, out var info, flags))
        {
            throw new MissingMemberException($"Can not find the '{name}' from type '{type}'. We can not reflect it.");
        }

        return info;
    }

    /// <summary>
    /// Returns the metadata of the field <paramref name="name" /> of <paramref name="type" />.
    /// </summary>
    /// <exception cref="MissingFieldException">
    /// No field with that name exists.
    /// </exception>
    public static FieldInfo GetFieldInfoOrThrow(
                                                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields |
                                                                            DynamicallyAccessedMemberTypes.NonPublicFields)]
                                                this Type type, string name,
                                                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
                                                                     BindingFlags.Public |
                                                                     BindingFlags.FlattenHierarchy)
    {
        if (!type.TryGetFieldInfo(name, out var info, flags))
        {
            throw new MissingFieldException($"Can not find the '{name}' from type '{type}'. We can not reflect it.");
        }

        return info;
    }

    /// <summary>
    /// Returns the metadata of the method <paramref name="name" /> of <paramref name="type" />. The lookup is
    /// by name only, so the method must not be overloaded.
    /// </summary>
    /// <exception cref="MissingMethodException">
    /// No method with that name exists.
    /// </exception>
    /// <exception cref="AmbiguousMatchException">
    /// More than one method with that name exists.
    /// </exception>
    public static MethodInfo GetMethodInfoOrThrow(
                                                  [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                                                              DynamicallyAccessedMemberTypes.NonPublicMethods)]
                                                  this Type type, string name,
                                                  BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
                                                                       BindingFlags.Public |
                                                                       BindingFlags.FlattenHierarchy)
    {
        if (!type.TryGetMethodInfo(name, out var info, flags))
        {
            throw new MissingMethodException($"Can not find the '{name}' from type '{type}'. We can not reflect it.");
        }

        return info;
    }

    /// <summary>
    /// Returns the metadata of the event <paramref name="name" /> of <paramref name="type" />.
    /// </summary>
    /// <exception cref="MissingMemberException">
    /// No event with that name exists.
    /// </exception>
    public static EventInfo GetEventInfoOrThrow(
                                                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents |
                                                                            DynamicallyAccessedMemberTypes.NonPublicEvents)]
                                                this Type type, string name,
                                                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
                                                                     BindingFlags.Public |
                                                                     BindingFlags.FlattenHierarchy)
    {
        if (!type.TryGetEventInfo(name, out var info, flags))
        {
            throw new MissingMemberException($"Can not find the event '{name}' from type '{type}'. We can not reflect it.");
        }

        return info;
    }
}
