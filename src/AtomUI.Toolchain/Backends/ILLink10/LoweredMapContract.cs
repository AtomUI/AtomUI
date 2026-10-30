using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AtomUI.TypeMap.Linker;

// Immutable pre-Sweep expectations. Both the surviving model and reread PE/PDB must satisfy this contract.
internal sealed class LoweredMapContract
{
    private readonly Func<TypeReference, TypeDefinition> _resolve;
    private readonly string _assemblyIdentity;
    private readonly string _groupName;
    private readonly string _helperName;
    private readonly string _accessorName;
    private readonly string _accessorSignature;
    private readonly Dictionary<string, string> _helperSignatures;
    private readonly EntryContract[] _entries;

    private sealed record TargetMemberContract(string Signature, bool IsVirtual, bool IsNewSlot);
    private sealed record EntryContract(string Key, string Name, string Identity, string BaseIdentity, string Constructor,
        TargetMemberContract[] Members);

    internal LoweredMapContract(MapSlot slot, Func<TypeReference, TypeDefinition> resolve, Func<IMetadataTokenProvider, bool> marked)
    {
        _resolve = resolve;
        _assemblyIdentity = slot.Group.Module.Assembly.Name.FullName;
        _groupName = slot.Group.FullName;
        _helperName = slot.Create.DeclaringType.FullName;
        _accessorName = slot.Accessor.FullName;
        _accessorSignature = Signature(slot.Accessor);
        _helperSignatures = new[] { slot.Create, slot.Add, slot.Complete }.ToDictionary(m => m.Name, Signature);
        _entries = slot.Entries.Select(entry =>
        {
            var identity = TypeIdentity(entry.Target);
            var self = entry.Target.CustomAttributes.First(a => TypeIdentity(a.AttributeType) == identity);
            return new EntryContract(entry.Key, entry.Target.FullName, identity,
                TypeIdentity(entry.Target.BaseType), Signature(self.Constructor),
                entry.Target.Methods.Where(m => marked(m) && (m.Name == "Add" || m.Name == "get_FragmentId"))
                    .Select(m => new TargetMemberContract(Signature(m), m.IsVirtual, m.IsNewSlot)).ToArray());
        }).ToArray();
    }

    internal void Validate(AssemblyDefinition assembly, MethodDefinition method, RegistrationMetadataIndex metadata)
    {
        try { ValidateStructure(assembly, method, metadata); }
        catch (BackendDiagnostic error)
        {
            throw BackendDiagnostic.Unlowered($"Lowered accessor '{_accessorName}' is invalid: {error.Message}");
        }
    }

    private void ValidateStructure(AssemblyDefinition assembly, MethodDefinition method, RegistrationMetadataIndex metadata)
    {
        Require(assembly.Name.FullName == _assemblyIdentity, "assembly identity changed");
        var types = metadata.Types(assembly);
        var group = One(types.Where(t => t.FullName == _groupName), "Group");
        Require(group.IsPublic && group.IsSealed && !group.HasGenericParameters && group.DeclaringType is null,
            "Group metadata changed");
        var helper = One(group.NestedTypes.Where(t => t.FullName == _helperName), "BrowserMap");
        Require(helper.IsNestedAssembly && helper.IsAbstract && helper.IsSealed && !helper.HasGenericParameters,
            "BrowserMap metadata changed");
        Require(types.Contains(method.DeclaringType) && method.DeclaringType.Methods.Contains(method) &&
            method.FullName == _accessorName && method.IsAssembly && SupportedDefinition(method) &&
            Signature(method) == _accessorSignature, "accessor definition/signature changed");
        foreach (var (name, signature) in _helperSignatures)
        {
            var definition = One(helper.Methods.Where(m => m.Name == name), name);
            Require(definition.IsPublic && SupportedDefinition(definition) && Signature(definition) == signature &&
                (name != "Complete" || definition.NoInlining), $"helper '{name}' definition/signature changed");
        }
        foreach (var entry in _entries)
        {
            var target = One(types.Where(t => t.FullName == entry.Name), entry.Name);
            Require(target.IsClass && target.IsSealed && !target.IsAbstract && !target.HasGenericParameters && target.BaseType is not null &&
                TypeIdentity(target) == entry.Identity && TypeIdentity(target.BaseType) == entry.BaseIdentity,
                $"target '{entry.Name}' metadata changed");
            Require(target.CustomAttributes.Any(a => TypeIdentity(a.AttributeType) == entry.Identity && Signature(a.Constructor) == entry.Constructor) &&
                target.Methods.Any(m => m.IsConstructor && m.HasBody && Signature(m) == entry.Constructor),
                $"target '{entry.Name}' lost its self attribute/constructor");
            foreach (var member in entry.Members)
            {
                Require(target.Methods.Any(m => m.HasBody && m.IsIL && m.IsManaged && !m.IsPInvokeImpl && !m.IsInternalCall &&
                    m.IsVirtual == member.IsVirtual && m.IsNewSlot == member.IsNewSlot && Signature(m) == member.Signature),
                    $"target '{entry.Name}' lost a marked fragment member");
            }
        }

        var body = method.Body;
        Require(!body.HasVariables && body.LocalVarToken.RID == 0 && !body.HasExceptionHandlers && !body.InitLocals &&
            body.MaxStackSize >= (_entries.Length == 0 ? 1 : 4), "locals, exception regions or body flags changed");
        Require(!method.DebugInformation.HasSequencePoints && method.DebugInformation.Scope is null &&
            method.DebugInformation.StateMachineKickOffMethod is null && !method.DebugInformation.HasCustomDebugInformations &&
            !method.HasCustomDebugInformations, "stale emitted debug information");
        var code = body.Instructions;
        Require(code.Count == 3 + 4 * _entries.Length, "instruction count changed");
        Require(Call(code[0], "Create") && Call(code[^2], "Complete") && code[^1].OpCode == OpCodes.Ret,
            "Create/Complete/return sequence changed");
        for (var index = 0; index < _entries.Length; index++)
        {
            var entry = _entries[index];
            var offset = 1 + index * 4;
            Require(code[offset].OpCode == OpCodes.Dup && code[offset + 1].OpCode == OpCodes.Ldstr &&
                Equals(code[offset + 1].Operand, entry.Key) && code[offset + 2].OpCode == OpCodes.Ldtoken &&
                code[offset + 2].Operand is TypeReference target && TypeIdentity(target) == entry.Identity &&
                Call(code[offset + 3], "Add"), $"entry '{entry.Key}' changed");
        }
    }

    private bool Call(Instruction instruction, string name) => instruction.OpCode == OpCodes.Call &&
        instruction.Operand is MethodReference method && method is not MethodSpecification && !method.HasGenericParameters &&
        !method.HasThis && !method.ExplicitThis && method.CallingConvention == MethodCallingConvention.Default &&
        Signature(method) == _helperSignatures[name];

    private static bool SupportedDefinition(MethodDefinition method)
    {
        if (!RegistrationAbi.IsManagedStatic(method) || method.HasGenericParameters || !method.HasBody)
        {
            return false;
        }

        for (var owner = method.DeclaringType; owner is not null; owner = owner.DeclaringType)
        {
            if (owner.HasGenericParameters)
            {
                return false;
            }
        }

        return true;
    }

    private string Signature(MethodReference method) => string.Join("|", TypeIdentity(method.DeclaringType), method.Name,
        method.HasThis, method.ExplicitThis, method.CallingConvention, method.GenericParameters.Count,
        TypeIdentity(method.ReturnType), string.Join(";", method.Parameters.Select(p => TypeIdentity(p.ParameterType))));

    private string TypeIdentity(TypeReference type)
    {
        if (type is GenericInstanceType generic)
        {
            return TypeIdentity(generic.ElementType) + "<" + string.Join(",", generic.GenericArguments.Select(TypeIdentity)) + ">";
        }

        return RegistrationAbi.Identity(_resolve(type));
    }

    private static T One<T>(IEnumerable<T> values, string name)
    {
        var candidates = values.Take(2).ToArray();
        Require(candidates.Length == 1, $"required metadata '{name}' is missing or ambiguous");
        return candidates[0];
    }
    private static void Require(bool condition, string reason)
    {
        if (!condition)
        {
            throw BackendDiagnostic.Unlowered(reason);
        }
    }
}
