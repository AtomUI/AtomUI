using System.Reflection;

namespace AtomUI.Native;

// Checks only the explicitly requested members; no assembly/member discovery or native calls.
internal static class WaylandMemberContract
{
    internal static FieldInfo ReadField(FieldInfo? field, Type owner, string name, Type valueType)
    {
        if (field is null)
        {
            throw new MissingFieldException(owner.FullName, name);
        }
        if (field.IsStatic || field.FieldType != valueType)
        {
            throw new NotSupportedException($"{owner.FullName}.{name} requires an instance field of {valueType.FullName}; found {field.FieldType.FullName}, static={field.IsStatic}.");
        }
        return field;
    }

    internal static FieldInfo WriteField(FieldInfo? field, Type owner, string name, Type valueType)
    {
        var result = ReadField(field, owner, name, valueType);
        if (result.IsInitOnly || result.IsLiteral)
        {
            throw new NotSupportedException($"{owner.FullName}.{name} requires a writable instance field.");
        }
        return result;
    }

    internal static PropertyInfo ReadProperty(PropertyInfo? property, Type owner, string name, Type valueType)
    {
        if (property is null)
        {
            throw new MissingMemberException(owner.FullName, name);
        }
        if (property.GetMethod is not { IsStatic: false } || property.GetIndexParameters().Length != 0 ||
            property.PropertyType != valueType)
        {
            throw new NotSupportedException($"{owner.FullName}.{name} requires a readable, non-indexed instance property of {valueType.FullName}; found {property.PropertyType.FullName}.");
        }
        return property;
    }

    internal static MethodInfo InstanceMethod(MethodInfo? method, Type owner, string name, Type returnType,
        params Type[] parameterTypes)
    {
        if (method is null)
        {
            throw new MissingMethodException(owner.FullName, name);
        }
        if (method.IsStatic || method.ContainsGenericParameters || method.ReturnType != returnType ||
            (method.CallingConvention & CallingConventions.VarArgs) != 0)
        {
            throw new NotSupportedException($"{owner.FullName}.{name} requires a non-generic instance method returning {returnType.FullName}; found return={method.ReturnType.FullName}, static={method.IsStatic}.");
        }
        var parameters = method.GetParameters();
        if (parameters.Length != parameterTypes.Length)
        {
            throw new NotSupportedException($"{owner.FullName}.{name} requires {parameterTypes.Length} parameters; found {parameters.Length}.");
        }
        for (var index = 0; index < parameters.Length; index++)
        {
            if (parameters[index].ParameterType != parameterTypes[index])
            {
                throw new NotSupportedException($"{owner.FullName}.{name} parameter {index} requires {parameterTypes[index].FullName}; found {parameters[index].ParameterType.FullName}.");
            }
        }
        return method;
    }
}
