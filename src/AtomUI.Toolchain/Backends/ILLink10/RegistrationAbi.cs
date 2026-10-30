using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;

namespace AtomUI.TypeMap.Linker;

internal sealed record MapEntry(string Key, TypeDefinition Target);
internal sealed record MapSlot(string PackageId, TypeDefinition Group, MethodDefinition Accessor,
    MethodDefinition Create, MethodDefinition Add, MethodDefinition Complete, IReadOnlyList<MapEntry> Entries)
{
    internal string AccessorIdentity { get; } = Accessor.FullName;
    internal LoweredMapContract Contract { get; set; } = null!;
}

internal sealed class RegistrationAbi
{
    private const string MarkerName = "AtomUI.Registration.ControlPackageMarkerAttribute";
    private const string AccessorName = "AtomUI.Registration.GeneratedTypeMapAccessorAttribute";
    private const string FragmentName = "AtomUI.Registration.ControlRegistrationFragmentAttribute";
    private readonly LinkContext _context;
    private readonly RegistrationMetadataIndex _metadata;

    internal RegistrationAbi(LinkContext context, RegistrationMetadataIndex metadata)
    {
        _context = context;
        _metadata = metadata;
    }

    internal IReadOnlyList<MapSlot> Read()
    {
        var assemblies = _context.GetAssemblies();
        var packages = new List<(AssemblyDefinition Assembly, string Id, TypeDefinition Group, AssemblyDefinition Core)>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var groups = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assembly in assemblies)
        {
            var markers = assembly.CustomAttributes.Where(a => a.AttributeType.FullName == MarkerName).ToArray();
            if (markers.Length == 0)
            {
                continue;
            }

            RequireLink(assembly);
            if (markers.Length != 1)
            {
                throw BackendDiagnostic.Conflict($"Assembly '{assembly.Name.FullName}' has multiple package markers.");
            }

            var marker = markers[0];
            var markerType = Resolve(marker.AttributeType);
            if (markerType.Module.Assembly.Name.Name != "AtomUI.Core")
            {
                throw BackendDiagnostic.Unsupported($"Package marker '{markerType.FullName}' must resolve to AtomUI.Core, not '{markerType.Module.Assembly.Name.FullName}'.");
            }

            if (marker.ConstructorArguments.Count != 3 || marker.ConstructorArguments[0].Value is not string id ||
                string.IsNullOrWhiteSpace(id) || marker.ConstructorArguments[1].Value is not TypeReference groupRef ||
                marker.ConstructorArguments[2].Value is not int version || version != 1)
            {
                throw BackendDiagnostic.Unsupported($"Package '{assembly.Name.FullName}' has an invalid marker or unknown ABI; rebuild with ABI 1.");
            }

            var group = Resolve(groupRef);
            if (group.Module.Assembly != assembly || !group.IsPublic || !group.IsSealed || group.HasGenericParameters || group.DeclaringType != null)
            {
                throw BackendDiagnostic.Unsupported($"Group '{Identity(group)}' must be a public, sealed, non-generic type owned by '{assembly.Name.FullName}'.");
            }

            if (!ids.Add(id) || !groups.Add(Identity(group)))
            {
                throw BackendDiagnostic.Conflict($"Package '{id}' or Group '{Identity(group)}' is declared more than once.");
            }

            packages.Add((assembly, id, group, markerType.Module.Assembly));
        }
        var slots = new List<MapSlot>();
        foreach (var (assembly, id, group, core) in packages)
        {
            var markedAccessors = _metadata.Methods(assembly)
                .Where(m => _context.Annotations.IsMarked(m) && m.CustomAttributes.Any(a => a.AttributeType.FullName == AccessorName)).ToArray();
            if (markedAccessors.Length == 0)
            {
                continue;
            }

            if (markedAccessors.Length != 1)
            {
                throw BackendDiagnostic.Conflict($"Package '{id}' has multiple reachable map accessors.");
            }

            var accessor = markedAccessors[0];
            var markers = accessor.CustomAttributes.Where(a => a.AttributeType.FullName == AccessorName).ToArray();
            if (markers.Length != 1 || Resolve(markers[0].AttributeType).Module.Assembly != core ||
                markers[0].ConstructorArguments.Count != 1 || markers[0].ConstructorArguments[0].Value is not TypeReference accessorGroup ||
                Resolve(accessorGroup) != group)
            {
                throw BackendDiagnostic.Unsupported($"Accessor '{accessor.FullName}' has an invalid Group/marker identity.");
            }

            if (!accessor.IsAssembly || !IsManagedStatic(accessor) || accessor.HasGenericParameters || !IsClosedOwner(accessor.DeclaringType) || accessor.HasParameters ||
                !IsDictionary(accessor.ReturnType, true) || !accessor.HasBody)
            {
                throw BackendDiagnostic.Unsupported($"Accessor '{accessor.FullName}' does not implement the ABI 1 method signature.");
            }

            ValidateOriginalBody(accessor, group);
            var helperTypes = group.NestedTypes.Where(t => t.Name == "BrowserMap").ToArray();
            if (helperTypes.Length != 1 || !helperTypes[0].IsNestedAssembly || !helperTypes[0].IsAbstract || !helperTypes[0].IsSealed || helperTypes[0].HasGenericParameters)
            {
                throw BackendDiagnostic.Unsupported($"Group '{Identity(group)}' has no valid owned BrowserMap helper.");
            }

            var helper = helperTypes[0];
            RequireMarked(group); RequireMarked(helper);
            var create = Helper(helper, "Create", m => !m.HasParameters && IsDictionary(m.ReturnType, false));
            var add = Helper(helper, "Add", m => IsBcl(m.ReturnType, "System.Void") && m.Parameters.Count == 3 &&
                IsDictionary(m.Parameters[0].ParameterType, false) && IsBcl(m.Parameters[1].ParameterType, "System.String") &&
                IsBcl(m.Parameters[2].ParameterType, "System.RuntimeTypeHandle"));
            var complete = Helper(helper, "Complete", m => m.NoInlining && IsDictionary(m.ReturnType, true) &&
                m.Parameters.Count == 1 && IsDictionary(m.Parameters[0].ParameterType, false));
            var entries = Entries(assemblies, assembly, group, core);
            var slot = new MapSlot(id, group, accessor, create, add, complete, entries);
            slot.Contract = new LoweredMapContract(slot, Resolve, _context.Annotations.IsMarked);
            slots.Add(slot);
        }
        return slots;
    }

    private IReadOnlyList<MapEntry> Entries(AssemblyDefinition[] assemblies, AssemblyDefinition owner, TypeDefinition group, AssemblyDefinition core)
    {
        var entries = new SortedDictionary<string, MapEntry>(StringComparer.Ordinal);
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assembly in assemblies)
        {
            foreach (var attr in assembly.CustomAttributes)
        {
            if (attr.AttributeType is not GenericInstanceType generic || generic.ElementType.FullName != "System.Runtime.InteropServices.TypeMapAttribute`1")
                {
                    continue;
                }

                if (generic.GenericArguments.Count != 1 || Resolve(generic.GenericArguments[0]) != group)
                {
                    continue;
                }

                if (!IsBcl(generic.ElementType, "System.Runtime.InteropServices.TypeMapAttribute`1"))
                {
                    throw BackendDiagnostic.Unsupported($"TypeMap declaration for '{Identity(group)}' is not the platform TypeMap attribute.");
                }

                if (assembly != owner)
                {
                    throw BackendDiagnostic.Unsupported($"TypeMap declarations for '{Identity(group)}' must be owned by their package assembly, not '{assembly.Name.FullName}'.");
                }

                if (attr.ConstructorArguments.Count != 3 || attr.ConstructorArguments[0].Value is not string key || string.IsNullOrWhiteSpace(key) ||
                attr.ConstructorArguments[1].Value is not TypeReference targetRef || attr.ConstructorArguments[2].Value is not TypeReference)
                {
                    throw BackendDiagnostic.Unsupported($"Group '{Identity(group)}' has a malformed TypeMap declaration/key.");
                }

                if (!declared.Add(key))
                {
                    throw BackendDiagnostic.Conflict($"Group '{Identity(group)}' declares duplicate key '{key}'.");
                }

                if (!_context.Annotations.IsMarked(attr))
                {
                    continue;
                }

                var target = Resolve(targetRef);
            if (target.Module.Assembly != owner || !target.IsSealed || target.HasGenericParameters || target.BaseType is null ||
                Resolve(target.BaseType).FullName != FragmentName || Resolve(target.BaseType).Module.Assembly != core ||
                !target.CustomAttributes.Any(a => Resolve(a.AttributeType) == target))
                {
                    throw BackendDiagnostic.Unsupported($"Selected target '{Identity(target)}' is not an owned, self-attributed ABI 1 fragment proxy.");
                }

                RequireMarked(target);
            entries.Add(key, new(key, target));
        }
        }

        return entries.Values.ToArray();
    }

    private void ValidateOriginalBody(MethodDefinition method, TypeDefinition group)
    {
        var instructions = method.Body.Instructions.Where(i => i.OpCode != OpCodes.Nop).ToArray();
        // Release expression-body accessors have exactly call/ret. Debug compilers may use a local and branch.
        var calls = instructions.Where(i => i.OpCode == OpCodes.Call).ToArray();
        if (calls.Length != 1 || calls[0].Operand is not GenericInstanceMethod call ||
            call.HasThis || call.ExplicitThis || call.CallingConvention == MethodCallingConvention.VarArg ||
            call.ElementMethod.Name != "GetOrCreateExternalTypeMapping" || !IsBcl(call.DeclaringType, "System.Runtime.InteropServices.TypeMapping") ||
            call.GenericArguments.Count != 1 || Resolve(call.GenericArguments[0]) != group ||
            call.HasParameters || !IsDictionary(call.ReturnType, true) || method.Body.HasExceptionHandlers ||
            instructions.Any(i => i.OpCode != OpCodes.Call && i.OpCode != OpCodes.Ret &&
                i.OpCode != OpCodes.Stloc_0 && i.OpCode != OpCodes.Ldloc_0 && i.OpCode != OpCodes.Br_S && i.OpCode != OpCodes.Br))
        {
            throw BackendDiagnostic.Unlowered($"Accessor '{method.FullName}' has a malformed, previously lowered or mismatched TypeMapping body. Rebuild the generated ABI 1 package.");
        }
        // A supported accessor returns the single query unmodified; reject arbitrary branch/local arrangements.
        if (instructions.Length != 2 && !(instructions.Length == 5 && instructions[1].OpCode == OpCodes.Stloc_0 &&
            (instructions[2].OpCode == OpCodes.Br_S || instructions[2].OpCode == OpCodes.Br) &&
            ReferenceEquals(instructions[2].Operand, instructions[3]) && instructions[3].OpCode == OpCodes.Ldloc_0))
        {
            throw BackendDiagnostic.Unlowered($"Accessor '{method.FullName}' is not a direct generated TypeMap query.");
        }

        if (instructions[0] != calls[0] || instructions[^1].OpCode != OpCodes.Ret)
        {
            throw BackendDiagnostic.Unlowered($"Accessor '{method.FullName}' does not return its direct TypeMap query.");
        }
    }

    private MethodDefinition Helper(TypeDefinition type, string name, Func<MethodDefinition, bool> signature)
    {
        var methods = type.Methods.Where(m => m.Name == name).ToArray();
        if (methods.Length != 1 || !methods[0].IsPublic || !IsManagedStatic(methods[0]) || methods[0].HasGenericParameters ||
            !methods[0].HasBody || !signature(methods[0]))
        {
            throw BackendDiagnostic.Unsupported($"Helper '{Identity(type)}::{name}' is missing or has an incompatible ABI 1 signature.");
        }

        RequireMarked(methods[0]);
        return methods[0];
    }

    internal static bool IsManagedStatic(MethodDefinition method) => method.IsStatic && !method.HasThis &&
        !method.ExplicitThis && method.CallingConvention == MethodCallingConvention.Default &&
        method.IsIL && method.IsManaged && !method.IsPInvokeImpl && !method.IsInternalCall;

    private static bool IsClosedOwner(TypeDefinition type)
    {
        for (var current = type; current is not null; current = current.DeclaringType)
        {
            if (current.HasGenericParameters)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsDictionary(TypeReference type, bool readOnly) => type is GenericInstanceType generic &&
        generic.GenericArguments.Count == 2 && IsBcl(generic.ElementType, readOnly ? "System.Collections.Generic.IReadOnlyDictionary`2" : "System.Collections.Generic.Dictionary`2") &&
        IsBcl(generic.GenericArguments[0], "System.String") && IsBcl(generic.GenericArguments[1], "System.Type");

    private bool IsBcl(TypeReference type, string fullName)
    {
        if (type.FullName != fullName)
        {
            return false;
        }

        var resolved = Resolve(type);
        return resolved.Module.Assembly.Name.Name == "System.Private.CoreLib" &&
            Convert.ToHexString(resolved.Module.Assembly.Name.PublicKeyToken) == "7CEC85D7BEA7798E";
    }

    private TypeDefinition Resolve(TypeReference type)
    {
        if (type is TypeSpecification)
        {
            throw BackendDiagnostic.Unsupported($"Expected a closed definition, not '{type.FullName}'.");
        }

        if (type is TypeDefinition definition)
        {
            return definition;
        }

        if (type.Scope is ModuleDefinition module)
        {
            return FindDefinition(module, type.FullName) ?? throw BackendDiagnostic.Unsupported($"Cannot resolve ABI type '{type.FullName}' in '{module.Name}'.");
        }

        if (type.Scope is not AssemblyNameReference requested)
        {
            throw BackendDiagnostic.Unsupported($"Unsupported ABI type scope '{type.Scope}' for '{type.FullName}'.");
        }

        // Do not call TypeReference/ExportedType resolution first: it follows forwarding chains using
        // ILLink's simple-name cache, losing destination version/culture/key checks and cycle bounds.
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (true)
        {
            if (!visited.Add(requested.FullName))
            {
                throw BackendDiagnostic.Conflict($"Type-forwarding cycle for '{type.FullName}' at '{requested.FullName}'.");
            }

            var assembly = _context.TryResolve(requested) ??
                throw BackendDiagnostic.Unsupported($"Cannot resolve '{type.FullName}' from '{requested.FullName}'.");
            if (assembly.Name.FullName != requested.FullName)
            {
                throw BackendDiagnostic.Conflict($"Requested identity '{type.FullName}, {requested.FullName}' resolved to assembly '{assembly.Name.FullName}'.");
            }

            var direct = FindDefinition(assembly.MainModule, type.FullName);
            var forwards = assembly.MainModule.ExportedTypes.Where(e => e.FullName == type.FullName).ToArray();
            if (direct is not null && forwards.Length == 0)
            {
                return direct;
            }

            if (direct is not null || forwards.Length != 1 || !forwards[0].IsForwarder || forwards[0].Scope is not AssemblyNameReference destination)
            {
                throw BackendDiagnostic.Unsupported($"Cannot find one exact definition or assembly forwarder for '{type.FullName}' in '{requested.FullName}'.");
            }

            requested = destination;
        }
    }

    private TypeDefinition? FindDefinition(ModuleDefinition module, string fullName)
    {
        var matches = _metadata.Types(module.Assembly).Where(t => t.FullName == fullName).Take(2).ToArray();
        if (matches.Length > 1)
        {
            throw BackendDiagnostic.Conflict($"Duplicate metadata type '{fullName}' in '{module.Name}'.");
        }

        return matches.SingleOrDefault();
    }

    private void RequireMarked(IMetadataTokenProvider member)
    {
        if (!_context.Annotations.IsMarked(member))
        {
            throw BackendDiagnostic.Unsupported($"Required map target/helper '{member}' was not retained during Mark. Rebuild with the generated DynamicDependency contract; the backend cannot add dependencies after Mark.");
        }
    }
    private void RequireLink(AssemblyDefinition assembly)
    {
        var action = _context.Annotations.GetAction(assembly);
        if (action != AssemblyAction.Link)
        {
            throw BackendDiagnostic.Unsupported($"Assembly '{assembly.Name.FullName}' has action '{action}', expected Link. Enable actual assembly linking before TypeMap materialization.");
        }
    }
    internal static string Identity(TypeDefinition type) => $"{type.FullName}, {type.Module.Assembly.Name.FullName}";
    internal static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (var type in types)
        {
            yield return type;
            foreach (var nested in AllTypes(type.NestedTypes))
            {
                yield return nested;
            }
        }
    }
}
