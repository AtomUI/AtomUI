using Mono.Cecil;

namespace AtomUI.TypeMap.Linker;

internal sealed class RegistrationMetadataIndex
{
    private readonly Dictionary<AssemblyDefinition, TypeDefinition[]> _types =
        new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AssemblyDefinition, MethodDefinition[]> _methods =
        new(ReferenceEqualityComparer.Instance);

    internal TypeDefinition[] Types(AssemblyDefinition assembly)
    {
        if (!_types.TryGetValue(assembly, out var types))
        {
            types = RegistrationAbi.AllTypes(assembly.MainModule.Types).ToArray();
            _types.Add(assembly, types);
        }
        return types;
    }

    internal MethodDefinition[] Methods(AssemblyDefinition assembly)
    {
        if (!_methods.TryGetValue(assembly, out var methods))
        {
            methods = Types(assembly).SelectMany(type => type.Methods).ToArray();
            _methods.Add(assembly, methods);
        }
        return methods;
    }
}
