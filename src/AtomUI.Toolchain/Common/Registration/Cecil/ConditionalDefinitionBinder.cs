using System.Globalization;
using System.Security.Cryptography;
using AtomUI.Build.Tasks.Registration;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Manifest = AtomUI.Build.Tasks.Registration.ConditionalRegistrationManifest;

namespace AtomUI.Registration.Shared;

internal sealed class ConditionalDefinitionBinder
{
    private const string RecordName = "AtomUI.Registration.ConditionalRegistrationRecordAttribute";
    private const string BuilderName = "AtomUI.Registration.ControlPackageRegistrationBuilder";
    private const string FragmentName = "AtomUI.Registration.ControlRegistrationFragmentAttribute";
    private const string StubMessage = "AtomUI conditional registration was not materialized by a supported publish backend.";
    private readonly Dictionary<string, AssemblyDefinition> _assemblies = new(StringComparer.Ordinal);
    private readonly Dictionary<RegistrationTypeIdentity, TypeDefinition> _types = new();
    private readonly Dictionary<AssemblyDefinition, ILookup<string, TypeDefinition>> _definitions = new();
    private readonly Dictionary<AssemblyDefinition, ILookup<string, ExportedType>> _exports = new();
    private readonly IReadOnlyDictionary<string, string> _frameworkBindings;
    private readonly TypeDefinition _builder;
    private readonly TypeDefinition _fragment;
    private readonly TypeDefinition _void;
    private readonly MethodDefinition _addFragment;

    internal ConditionalDefinitionBinder(
        Func<AssemblyNameReference, AssemblyDefinition?> resolveAssembly,
        Func<AssemblyDefinition, string> getAssemblyLocation,
        ConditionalInputSnapshot snapshot,
        IReadOnlyDictionary<string, string>? frameworkBindings = null)
    {
        _frameworkBindings = frameworkBindings ?? new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var input in snapshot.Assemblies)
        {
            // ILLink's resolver internally caches by simple name. Always verify the complete identity
            // and actual implementation bytes before accepting the resulting shared Cecil object.
            var assembly = resolveAssembly(AssemblyNameReference.Parse(input.AssemblyIdentity)) ??
                throw Invalid("Cannot resolve implementation input " + input.AssemblyIdentity);
            var location = getAssemblyLocation(assembly);
            if (assembly.Name.FullName != input.AssemblyIdentity || string.IsNullOrEmpty(location) ||
                Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(location))) != input.Sha256)
            {
                throw Invalid("LinkContext resolved different implementation bytes or identity for " + input.AssemblyIdentity + "; actual=" + assembly.Name.FullName + "; path=" + location + "; expectedPath=" + input.Path);
            }
            _assemblies.Add(input.AssemblyIdentity, assembly);
        }
        _builder = Resolve(new RegistrationTypeIdentity(snapshot.CoreAssemblyIdentity, BuilderName));
        _fragment = Resolve(new RegistrationTypeIdentity(snapshot.CoreAssemblyIdentity, FragmentName));
        _void = Resolve(_builder.Module.TypeSystem.Void);
        _addFragment = _builder.Methods.SingleOrDefault(method => method.Name == "AddFragment" && !method.IsStatic &&
            !method.HasGenericParameters && method.Parameters.Count == 1 && Resolve(method.Parameters[0].ParameterType) == _fragment &&
            Resolve(method.ReturnType) == _void) ?? throw Invalid("Core builder AddFragment ABI is missing or ambiguous.");
    }

    internal BoundConditionalRegistrationGroup Bind(Manifest manifest)
    {
        var package = manifest.Package;
        var owner = _assemblies[manifest.Metadata.AssemblyIdentity];
        var group = Resolve(package.Group);
        if (group.Module.Assembly != owner || !group.IsPublic || !IsClosed(group))
        {
            throw Invalid("Conditional Group must be a public closed type owned by its package.");
        }
        ValidateMarker(owner, group, package.Id);
        var collector = Resolve(package.Collect);
        var full = Resolve(package.CollectFull);
        var selected = Resolve(package.CollectSelected);
        RequireCalls(collector, [full]);
        RequireStub(selected);
        var fragments = new List<BoundConditionalRegistrationFragment>();
        var conditionsByFragment = manifest.Conditions.ToLookup(condition => condition.Fragment, StringComparer.Ordinal);
        foreach (var fragment in manifest.Fragments)
        {
            var proxy = Resolve(fragment.Proxy);
            var thunk = Resolve(fragment.RegistrationMethod);
            if (proxy.Module.Assembly != owner || thunk.Module.Assembly != owner || thunk.DeclaringType != collector.DeclaringType ||
                !proxy.IsSealed || proxy.IsAbstract || !IsClosed(proxy) || proxy.BaseType is null || Resolve(proxy.BaseType) != _fragment)
            {
                throw Invalid("Conditional proxy/thunk has an invalid owner or fragment base: " + fragment.Identity);
            }
            var self = proxy.CustomAttributes.Where(attribute => Resolve(attribute.AttributeType) == proxy).ToArray();
            if (self.Length != 1 || self[0].ConstructorArguments.Count != 0 || self[0].Fields.Count != 0 || self[0].Properties.Count != 0)
            {
                throw Invalid("Conditional proxy must have exactly one parameterless self attribute.");
            }
            var getter = proxy.Methods.SingleOrDefault(method => method.Name == "get_FragmentId" && !method.IsStatic &&
                method.Parameters.Count == 0 && !method.HasGenericParameters && method.IsVirtual && !method.IsNewSlot)
                ?? throw Invalid("Conditional proxy lacks its FragmentId override.");
            var idBody = Instructions(getter);
            if (idBody.Length != 2 || idBody[0].OpCode.Code != Code.Ldstr ||
                !Equals(idBody[0].Operand, fragment.FragmentId) || idBody[1].OpCode.Code != Code.Ret)
            {
                throw Invalid("Conditional proxy FragmentId differs from its record.");
            }
            RequireThunk(thunk, proxy);
            var conditions = conditionsByFragment[fragment.Identity]
                .Select(condition => new BoundConditionalRegistrationCondition(condition.Identity, Resolve(condition.Trigger))).ToArray();
            if (conditions.Any(condition => !IsClosed(condition.TriggerType) || !Accessible(condition.TriggerType)))
            {
                throw Invalid("Conditional triggers must be accessible closed named types.");
            }
            fragments.Add(new(fragment.Identity, fragment.Order.ToString("D10", CultureInfo.InvariantCulture), proxy, thunk, conditions));
        }
        RequireCalls(full, fragments.Select(fragment => fragment.RegistrationMethod).ToArray());
        return new(package.Group.QualifiedName, manifest, group, collector, full, selected, fragments);
    }

    internal void RemoveConsumedRecords(IEnumerable<Manifest> manifests)
    {
        foreach (var manifest in manifests)
        {
            var assembly = _assemblies[manifest.Metadata.AssemblyIdentity];
            var records = assembly.CustomAttributes.Where(attribute => attribute.AttributeType.FullName == RecordName).ToArray();
            if (records.Length != manifest.Metadata.Records.Count || records.Any(attribute =>
                    Resolve(attribute.AttributeType).Module.Assembly.Name.FullName != manifest.Metadata.ContractAssemblyIdentity))
            {
                throw Invalid("LinkContext conditional records differ from the verified implementation input.");
            }
            foreach (var record in records)
            {
                assembly.CustomAttributes.Remove(record);
            }
        }
    }

    private void ValidateMarker(AssemblyDefinition owner, TypeDefinition group, string packageId)
    {
        var markers = owner.CustomAttributes.Where(attribute => attribute.AttributeType.FullName == "AtomUI.Registration.ControlPackageMarkerAttribute").ToArray();
        if (markers.Length != 1 || Resolve(markers[0].AttributeType).Module.Assembly != _builder.Module.Assembly ||
            markers[0].ConstructorArguments.Count != 3 || !Equals(markers[0].ConstructorArguments[0].Value, packageId) ||
            markers[0].ConstructorArguments[1].Value is not TypeReference markerGroup || Resolve(markerGroup) != group ||
            !Equals(markers[0].ConstructorArguments[2].Value, 1))
        {
            throw Invalid("Conditional record Group differs from the package marker ABI.");
        }
    }

    private MethodDefinition Resolve(RegistrationMethodIdentity identity)
    {
        var owner = Resolve(identity.DeclaringType);
        var parameters = identity.Parameters.Select(Resolve).ToArray();
        var returnType = Resolve(identity.ReturnType);
        var methods = owner.Methods.Where(method => method.Name == identity.Name && method.IsStatic && !method.HasGenericParameters &&
            method.Parameters.Count == parameters.Length && method.Parameters.Select(parameter => Resolve(parameter.ParameterType)).SequenceEqual(parameters) &&
            Resolve(method.ReturnType) == returnType).ToArray();
        if (methods.Length != 1 || !IsClosed(owner) || parameters.Length != 1 || parameters[0] != _builder || returnType != _void || !methods[0].HasBody)
        {
            throw Invalid("Missing, ambiguous or unsupported registration entry: " + identity.QualifiedName);
        }
        return methods[0];
    }

    private MethodDefinition Resolve(MethodReference reference)
    {
        if (reference is GenericInstanceMethod || reference.HasGenericParameters || reference.CallingConvention != MethodCallingConvention.Default)
        {
            throw Invalid("Generic or non-default call in collector template.");
        }
        var owner = Resolve(reference.DeclaringType);
        var candidates = owner.Methods.Where(method => method.Name == reference.Name && method.HasThis == reference.HasThis &&
            !method.HasGenericParameters && method.Parameters.Count == reference.Parameters.Count &&
            method.Parameters.Select(parameter => Resolve(parameter.ParameterType)).SequenceEqual(reference.Parameters.Select(parameter => Resolve(parameter.ParameterType))) &&
            Resolve(method.ReturnType) == Resolve(reference.ReturnType)).ToArray();
        return candidates.Length == 1 ? candidates[0] : throw Invalid("Unknown or ambiguous collector call: " + reference.FullName);
    }

    private TypeDefinition Resolve(RegistrationTypeIdentity identity)
    {
        if (_types.TryGetValue(identity, out var cached))
        {
            return cached;
        }
        var name = identity.MetadataName.Replace('+', '/');
        var assemblyIdentity = BindFrameworkReference(identity.Assembly);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (visited.Add(assemblyIdentity))
        {
            if (!_assemblies.TryGetValue(assemblyIdentity, out var assembly))
            {
                throw Invalid("Type/forwarding assembly is outside the verified implementation snapshot: " + assemblyIdentity);
            }
            if (!_definitions.TryGetValue(assembly, out var allDefinitions))
            {
                _definitions.Add(assembly, allDefinitions = assembly.MainModule.GetTypes().ToLookup(type => type.FullName, StringComparer.Ordinal));
                _exports.Add(assembly, assembly.MainModule.ExportedTypes.ToLookup(type => type.FullName, StringComparer.Ordinal));
            }
            var definitions = allDefinitions[name].Take(2).ToArray();
            var exports = _exports[assembly][name].Take(2).ToArray();
            if (definitions.Length == 1 && exports.Length == 0)
            {
                _types.Add(identity, definitions[0]);
                return definitions[0];
            }
            if (definitions.Length != 0 || exports.Length != 1 || !IsForwarder(exports[0]) || exports[0].Scope is not AssemblyNameReference destination)
            {
                throw Invalid("No unique definition or legal forwarder for " + identity.QualifiedName);
            }
            assemblyIdentity = BindFrameworkReference(destination.FullName);
        }
        throw Invalid("Conditional type forwarding cycle: " + identity.QualifiedName);
    }

    // Optional entries are supplied only by a target framework/runtime-pack binding policy.
    // net8 callers pass no map; third-party version mismatches remain strict failures.
    private string BindFrameworkReference(string identity) =>
        _frameworkBindings.TryGetValue(identity, out var implementation) ? implementation : identity;

    private TypeDefinition Resolve(TypeReference reference)
    {
        if (reference is TypeSpecification || reference is GenericParameter)
        {
            throw Invalid("Registration ABI requires a non-generic named type: " + reference.FullName);
        }
        var scope = reference.Scope;
        var assembly = scope switch
        {
            AssemblyNameReference name => name.FullName,
            ModuleDefinition module => module.Assembly.Name.FullName,
            _ => throw Invalid("Unsupported registration type scope: " + reference.FullName)
        };
        return Resolve(new RegistrationTypeIdentity(assembly, reference.FullName.Replace('/', '+')));
    }

    private static bool IsForwarder(ExportedType exported)
    {
        while (exported.DeclaringType is not null)
        {
            exported = exported.DeclaringType;
        }
        return exported.IsForwarder;
    }

    private void RequireCalls(MethodDefinition method, IReadOnlyList<MethodDefinition> expected)
    {
        var body = Instructions(method);
        if (body.Length != expected.Count * 2 + 1 || body[^1].OpCode.Code != Code.Ret)
        {
            throw Invalid("Unsupported collector template: " + method.FullName);
        }
        for (var index = 0; index < expected.Count; index++)
        {
            if (body[index * 2].OpCode.Code != Code.Ldarg_0 || body[index * 2 + 1].OpCode.Code != Code.Call ||
                body[index * 2 + 1].Operand is not MethodReference call || Resolve(call) != expected[index])
            {
                throw Invalid("Collector does not call its declared entries in canonical order: " + method.FullName);
            }
        }
    }

    private void RequireStub(MethodDefinition method)
    {
        var body = Instructions(method);
        if (body.Length != 3 || body[0].OpCode.Code != Code.Ldstr || !Equals(body[0].Operand, StubMessage) ||
            body[1].OpCode.Code != Code.Newobj || body[1].Operand is not MethodReference constructor || body[2].OpCode.Code != Code.Throw)
        {
            throw Invalid("Selected collector is not the unmaterialized format 1 failure stub.");
        }
        var target = Resolve(constructor);
        if (target.Name != ".ctor" || target.IsStatic || target.Parameters.Count != 1 ||
            target.DeclaringType.FullName != "System.InvalidOperationException" || target.DeclaringType.Module.Assembly != _void.Module.Assembly ||
            Resolve(target.Parameters[0].ParameterType).FullName != "System.String" || Resolve(target.Parameters[0].ParameterType).Module.Assembly != _void.Module.Assembly)
        {
            throw Invalid("Selected collector throws an unexpected exception type.");
        }
    }

    private void RequireThunk(MethodDefinition method, TypeDefinition proxy)
    {
        var body = Instructions(method);
        if (body.Length != 4 || body[0].OpCode.Code != Code.Ldarg_0 || body[1].OpCode.Code != Code.Newobj ||
            body[1].Operand is not MethodReference constructor || Resolve(constructor) is not { IsConstructor: true, IsStatic: false, Parameters.Count: 0 } target ||
            target.DeclaringType != proxy || body[2].OpCode.Code != Code.Callvirt ||
            body[2].Operand is not MethodReference add || Resolve(add) != _addFragment || body[3].OpCode.Code != Code.Ret)
        {
            throw Invalid("Registration thunk must pass its own new proxy to Core builder.AddFragment: " + method.FullName);
        }
    }

    private static Instruction[] Instructions(MethodDefinition method)
    {
        if (!method.HasBody || method.Body.HasExceptionHandlers)
        {
            throw Invalid("Registration template requires an ordinary body without exception handlers.");
        }
        return method.Body.Instructions.Where(instruction => instruction.OpCode.Code != Code.Nop).ToArray();
    }

    private static bool IsClosed(TypeDefinition type)
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

    private static bool Accessible(TypeDefinition type)
    {
        for (var current = type; current is not null; current = current.DeclaringType)
        {
            if (current.IsNested ? !(current.IsNestedPublic || current.IsNestedAssembly || current.IsNestedFamilyOrAssembly) : !(current.IsPublic || current.IsNotPublic))
            {
                return false;
            }
        }
        return true;
    }

    private static InvalidDataException Invalid(string message) => new(message);
}
