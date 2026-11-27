using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace AtomUI.Reflection;

/// <summary>
/// Reads and writes properties, fields and methods of an object through reflection.
/// </summary>
/// <remarks>
/// Overloads with a <c>declareType</c> parameter look the member up on that type, so trimming can keep the
/// member. The other overloads look it up on the runtime type of the object. An exception thrown by the
/// accessed getter, setter or method itself is not caught.
/// </remarks>
public static class ObjectExtension
{
    /// <summary>
    /// Reads the value of the property <paramref name="name" /> of <paramref name="source" />, looking the
    /// property up on its runtime type.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the value in <paramref name="result" /> when the property exists and holds
    /// an instance of <typeparamref name="T" />. <see langword="false" /> and the default value of
    /// <typeparamref name="T" /> in <paramref name="result" /> when the property is missing, or its value is
    /// <see langword="null" /> or of another type.
    /// </returns>
    [RequiresUnreferencedCode("Reflects over the runtime type of source.")]
    public static bool TryGetProperty<T>(
        this object source,
        string name,
        out T? result,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                             BindingFlags.FlattenHierarchy)
    {
        var property = source.GetType().GetProperty(name, flags);
        if (property is not null && property.GetValue(source) is T obj)
        {
            result = obj;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Reads the value of the property <paramref name="name" /> of <paramref name="source" />, looking the
    /// property up on <paramref name="declareType" />.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the value in <paramref name="result" /> when the property exists and holds
    /// an instance of <typeparamref name="T" />. <see langword="false" /> and the default value of
    /// <typeparamref name="T" /> in <paramref name="result" /> when the property is missing, or its value is
    /// <see langword="null" /> or of another type.
    /// </returns>
    public static bool TryGetProperty<T>(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type declareType,
        string name,
        out T? result,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
    {
        var property = declareType.GetProperty(name, flags);
        if (property is not null && property.GetValue(source) is T obj)
        {
            result = obj;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Reads the value of the property <paramref name="info" /> of <paramref name="source" />.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the value in <paramref name="result" /> when the property holds an instance
    /// of <typeparamref name="T" />. <see langword="false" /> and the default value of
    /// <typeparamref name="T" /> in <paramref name="result" /> when its value is <see langword="null" /> or
    /// of another type.
    /// </returns>
    public static bool TryGetProperty<T>(this object source, PropertyInfo info, out T? result)
    {
        if (info.GetValue(source) is T obj)
        {
            result = obj;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Reads the value of the property <paramref name="info" /> of <paramref name="source" /> and returns it
    /// as <typeparamref name="T" />.
    /// </summary>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="Exception">
    /// The property holds <see langword="null" /> or a value that is not an instance of
    /// <typeparamref name="T" />.
    /// </exception>
    public static T? GetPropertyOrThrow<T>(this object source, PropertyInfo info)
    {
        var obj = info.GetValue(source);
        if (obj is T propertyOrThrow)
        {
            return propertyOrThrow;
        }
        throw new Exception($"Get Property {info.Name} value failed.");
    }

    /// <summary>
    /// Reads the value of the property <paramref name="name" /> of <paramref name="source" />, looking the
    /// property up on its runtime type.
    /// </summary>
    /// <returns>
    /// The value of the property, or the default value of <typeparamref name="T" /> when the property holds
    /// <see langword="null" /> and <typeparamref name="T" /> is a reference type.
    /// </returns>
    /// <exception cref="MissingMemberException">
    /// No property with that name exists.
    /// </exception>
    /// <exception cref="Exception">
    /// The property holds <see langword="null" /> although <typeparamref name="T" /> is a value type, or its
    /// value is not an instance of <typeparamref name="T" />.
    /// </exception>
    [RequiresUnreferencedCode("Reflects over the runtime type of source.")]
    public static T? GetPropertyOrThrow<T>(this object source, string name,
                                           BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
                                                                BindingFlags.Public | BindingFlags.FlattenHierarchy)
    {
        var obj = source.GetType().GetPropertyInfoOrThrow(name, flags).GetValue(source);
        if (obj is T propertyOrThrow)
        {
            return propertyOrThrow;
        }

        if (obj == null)
        {
            if (typeof(T).IsValueType)
            {
                throw new Exception(name + " is a value type but the value is null.");
            }

            return default;
        }

        var stringBuilder = new StringBuilder(30);
        stringBuilder.Append(name);
        stringBuilder.Append("'s type is ");
        stringBuilder.Append(source.GetType().Name);
        stringBuilder.Append(" but the value is ");
        stringBuilder.Append(obj.GetType().Name);
        stringBuilder.Append(".");
        throw new Exception(stringBuilder.ToString());
    }

    /// <summary>
    /// Reads the value of the property <paramref name="name" /> of <paramref name="source" />, looking the
    /// property up on <paramref name="declareType" />.
    /// </summary>
    /// <returns>
    /// The value of the property, or the default value of <typeparamref name="T" /> when the property holds
    /// <see langword="null" /> and <typeparamref name="T" /> is a reference type.
    /// </returns>
    /// <exception cref="MissingMemberException">
    /// No property with that name exists.
    /// </exception>
    /// <exception cref="Exception">
    /// The property holds <see langword="null" /> although <typeparamref name="T" /> is a value type, or its
    /// value is not an instance of <typeparamref name="T" />.
    /// </exception>
    public static T? GetPropertyOrThrow<T>(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type declareType,
        string name,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
    {
        var obj = declareType.GetPropertyInfoOrThrow(name, flags).GetValue(source);
        if (obj is T propertyOrThrow)
        {
            return propertyOrThrow;
        }

        if (obj == null)
        {
            if (typeof(T).IsValueType)
            {
                throw new Exception(name + " is a value type but the value is null.");
            }

            return default;
        }

        var stringHandler = new StringBuilder(30);
        stringHandler.Append(name);
        stringHandler.Append("'s type is ");
        stringHandler.Append(declareType.Name);
        stringHandler.Append(" but the value is ");
        stringHandler.Append(obj.GetType().Name);
        stringHandler.Append(".");
        throw new Exception(stringHandler.ToString());
    }

    /// <summary>
    /// Sets the property <paramref name="name" /> of <paramref name="source" /> to <paramref name="value" />,
    /// looking the property up on <paramref name="declareType" />.
    /// </summary>
    /// <returns>
    /// <see langword="false" /> when no property with that name matches <paramref name="flags" />, which by
    /// default exclude public properties. Otherwise <see langword="true" />, after the setter has run.
    /// </returns>
    public static bool TrySetProperty<T>(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type declareType,
        string name,
        T? value,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        var property = declareType.GetProperty(name, flags);
        if (property is null)
        {
            return false;
        }

        property.SetValue(source, value);
        return true;
    }

    /// <summary>
    /// Reads the value of the field <paramref name="name" /> of <paramref name="source" />, looking the field
    /// up on its runtime type.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the value in <paramref name="result" /> when the field exists and holds
    /// an instance of <typeparamref name="T" />. <see langword="false" /> and the default value of
    /// <typeparamref name="T" /> in <paramref name="result" /> when the field is missing, or its value is
    /// <see langword="null" /> or of another type.
    /// </returns>
    [RequiresUnreferencedCode("Reflects over the runtime type of source.")]
    public static bool TryGetField<T>(
        this object source,
        string name,
        out T? result,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                             BindingFlags.FlattenHierarchy)
    {
        var field = source.GetType().GetField(name, flags);
        if (field is not null && field.GetValue(source) is T obj)
        {
            result = obj;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Reads the value of the field <paramref name="name" /> of <paramref name="source" />, looking the field
    /// up on <paramref name="declareType" />.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the value in <paramref name="result" /> when the field exists and holds
    /// an instance of <typeparamref name="T" />. <see langword="false" /> and the default value of
    /// <typeparamref name="T" /> in <paramref name="result" /> when the field is missing, or its value is
    /// <see langword="null" /> or of another type.
    /// </returns>
    public static bool TryGetField<T>(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields |
                                    DynamicallyAccessedMemberTypes.NonPublicFields)]
        Type declareType,
        string name,
        out T? result,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
    {
        var field = declareType.GetField(name, flags);
        if (field is not null && field.GetValue(source) is T obj)
        {
            result = obj;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Reads the value of the field <paramref name="info" /> of <paramref name="source" />.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the value in <paramref name="result" /> when the field holds an instance
    /// of <typeparamref name="T" />. <see langword="false" /> and the default value of
    /// <typeparamref name="T" /> in <paramref name="result" /> when its value is <see langword="null" /> or
    /// of another type.
    /// </returns>
    public static bool TryGetField<T>(this object source, FieldInfo info, out T? result)
    {
        if (info.GetValue(source) is T obj)
        {
            result = obj;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Reads the value of the field <paramref name="name" /> of <paramref name="source" />, looking the field
    /// up on its runtime type.
    /// </summary>
    /// <returns>
    /// The value of the field, or the default value of <typeparamref name="T" /> when the field holds
    /// <see langword="null" /> and <typeparamref name="T" /> is a reference type.
    /// </returns>
    /// <exception cref="MissingFieldException">
    /// No field with that name exists.
    /// </exception>
    /// <exception cref="Exception">
    /// The field holds <see langword="null" /> although <typeparamref name="T" /> is a value type, or its
    /// value is not an instance of <typeparamref name="T" />.
    /// </exception>
    [RequiresUnreferencedCode("Reflects over the runtime type of source.")]
    public static T? GetFieldOrThrow<T>(this object source, string name,
                                        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
                                                             BindingFlags.Public | BindingFlags.FlattenHierarchy)
    {
        var obj = source.GetType().GetFieldInfoOrThrow(name, flags).GetValue(source);
        if (obj is T fieldOrThrow)
        {
            return fieldOrThrow;
        }

        if (obj == null)
        {
            if (typeof(T).IsValueType)
            {
                throw new Exception(name + " is a value type but the value is null.");
            }

            return default;
        }

        var stringBuilder = new StringBuilder(30);
        stringBuilder.Append(name);
        stringBuilder.Append("'s type is ");
        stringBuilder.Append(source.GetType().Name);
        stringBuilder.Append(" but the value is ");
        stringBuilder.Append(obj.GetType().Name);
        stringBuilder.Append(".");
        throw new Exception(stringBuilder.ToString());
    }

    /// <summary>
    /// Reads the value of the field <paramref name="name" /> of <paramref name="source" />, looking the field
    /// up on <paramref name="declareType" />.
    /// </summary>
    /// <returns>
    /// The value of the field, or the default value of <typeparamref name="T" /> when the field holds
    /// <see langword="null" /> and <typeparamref name="T" /> is a reference type.
    /// </returns>
    /// <exception cref="MissingFieldException">
    /// No field with that name exists.
    /// </exception>
    /// <exception cref="Exception">
    /// The field holds <see langword="null" /> although <typeparamref name="T" /> is a value type, or its
    /// value is not an instance of <typeparamref name="T" />.
    /// </exception>
    public static T? GetFieldOrThrow<T>(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields |
                                    DynamicallyAccessedMemberTypes.NonPublicFields)]
        Type declareType,
        string name,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
    {
        var obj = declareType.GetFieldInfoOrThrow(name, flags).GetValue(source);
        if (obj is T fieldOrThrow)
        {
            return fieldOrThrow;
        }

        if (obj == null)
        {
            if (typeof(T).IsValueType)
            {
                throw new Exception(name + " is a value type but the value is null.");
            }

            return default;
        }

        var stringBuilder = new StringBuilder(30);
        stringBuilder.Append(name);
        stringBuilder.Append("'s type is ");
        stringBuilder.Append(declareType);
        stringBuilder.Append(" but the value is ");
        stringBuilder.Append(obj.GetType().Name);
        stringBuilder.Append(".");
        throw new Exception(stringBuilder.ToString());
    }

    /// <summary>
    /// Sets the field <paramref name="name" /> of <paramref name="source" /> to <paramref name="value" />,
    /// looking the field up on <paramref name="declareType" />.
    /// </summary>
    /// <returns>
    /// <see langword="false" /> when no field with that name matches <paramref name="flags" />. Otherwise
    /// <see langword="true" />, after the field has been written.
    /// </returns>
    public static bool TrySetField<T>(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields |
                                    DynamicallyAccessedMemberTypes.NonPublicFields)]
        Type declareType,
        string name,
        T? value,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
    {
        var field = declareType.GetField(name, flags);
        if (field is null)
        {
            return false;
        }

        field.SetValue(source, value);
        return true;
    }

    /// <summary>
    /// Calls the instance method <paramref name="name" /> of <paramref name="source" /> with
    /// <paramref name="parameters" />. The method is looked up on <paramref name="declareType" /> by name
    /// only, so it must not be overloaded.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> and the return value of the method in <paramref name="result" />, which is
    /// <see langword="null" /> for a method without a return value. <see langword="false" /> and
    /// <see langword="null" /> in <paramref name="result" /> when no method with that name exists.
    /// </returns>
    /// <exception cref="AmbiguousMatchException">
    /// More than one method with that name exists.
    /// </exception>
    public static bool TryInvokeMethod(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                    DynamicallyAccessedMemberTypes.NonPublicMethods)]
        Type declareType,
        string name,
        out object? result,
        params object[] parameters)
    {
        var method = declareType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (method is null)
        {
            result = null;
            return false;
        }

        result = method.Invoke(source, parameters);
        return true;
    }

    /// <summary>
    /// Calls the instance method <paramref name="name" /> of <paramref name="source" /> with
    /// <paramref name="parameters" /> and returns the return value of the method, or <see langword="null" />
    /// when it has none. The method is looked up on <paramref name="declareType" /> by name only, so it must
    /// not be overloaded.
    /// </summary>
    /// <exception cref="MissingMethodException">
    /// No method with that name exists.
    /// </exception>
    /// <exception cref="AmbiguousMatchException">
    /// More than one method with that name exists.
    /// </exception>
    public static object? InvokeMethodOrThrow(
        this object source,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                    DynamicallyAccessedMemberTypes.NonPublicMethods)]
        Type declareType,
        string name,
        params object[] parameters)
    {
        return declareType.GetMethodInfoOrThrow(name).Invoke(source, parameters);
    }
}
