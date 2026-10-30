using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Internal.IL;
using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

namespace ILCompiler;

internal sealed record ConditionalInputBinding(RegistrationDefinitions.Group[] Groups,
    MetadataType RecordAttribute, string ReportPath, string InputHash, MethodDesc TrimmedGetter);

internal static class ConditionalInputBinder
{
    private const string StubMessage = "AtomUI conditional registration was not materialized by a supported publish backend.";
    private sealed record Instruction(ILOpcode OpCode, object Operand = null);

    public static ConditionalInputBinding Read(string path, CompilerTypeSystemContext context, ILProvider original)
    {
        byte[] inputBytes = File.ReadAllBytes(path);
        using var json = JsonDocument.Parse(inputBytes);
        JsonElement root = json.RootElement;
        RegistrationInputBootstrap.ValidateSnapshot(root);
        string coreIdentity = RegistrationContractJson.String(root, "coreAssemblyIdentity");
        string report = RegistrationContractJson.String(root, "analysisReportPath");
        if (!Path.IsPathFullyQualified(report))
        {
            throw new InvalidDataException("Analysis report path must be absolute");
        }
        var manifests = new List<ConditionalRegistrationManifest>();
        var supplied = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (JsonElement file in root.GetProperty("assemblies").EnumerateArray())
        {
            RegistrationContractJson.Fields(file, "path", "assemblyIdentity", "sha256");
            string filePath = RegistrationContractJson.String(file, "path");
            string identity = RegistrationContractJson.String(file, "assemblyIdentity");
            string hash = RegistrationContractJson.String(file, "sha256");
            if (!Path.IsPathFullyQualified(filePath) || !supplied.TryAdd(identity, hash))
            {
                throw new InvalidDataException("Input paths must be absolute and assembly identities unique");
            }
            ConditionalRegistrationMetadata metadata = ConditionalRegistrationMetadata.Read(filePath, coreIdentity);
            if (metadata.AssemblyIdentity != identity || metadata.ContentHash != hash)
            {
                throw new InvalidDataException("Conditional input snapshot identity/hash mismatch: " + filePath);
            }
            var resolved = (EcmaModule)context.ResolveAssembly(new AssemblyName(identity), true);
            string consumedHash = Convert.ToHexStringLower(SHA256.HashData(resolved.PEReader.GetEntireImage().GetContent().AsSpan()));
            if (resolved.Assembly.GetName().FullName != identity || consumedHash != hash)
            {
                throw new InvalidDataException("ILC resolver consumed different implementation bytes: " + identity);
            }
            if (metadata.HasPackageMarker != (metadata.Records.Count > 0))
            {
                throw new InvalidDataException("Control package marker and conditional records must both be present: " + identity);
            }
            if (metadata.Records.Count > 0)
            {
                manifests.Add(ConditionalRegistrationManifest.Parse(metadata));
            }
        }
        foreach (string actualPath in context.InputFilePaths.Values.Concat(context.ReferenceFilePaths.Values).Distinct(StringComparer.Ordinal))
        {
            EcmaModule actual = context.GetModuleFromPath(actualPath);
            string identity = actual.Assembly.GetName().FullName;
            string actualHash = Convert.ToHexStringLower(SHA256.HashData(actual.PEReader.GetEntireImage().GetContent().AsSpan()));
            if (!supplied.TryGetValue(identity, out string expectedHash) || expectedHash != actualHash)
            {
                throw new InvalidDataException("Input snapshot does not cover the actual ILC input: " + identity);
            }
        }
        if (manifests.Count == 0)
        {
            throw new InvalidDataException("No conditional package records in the frozen ILC inputs");
        }
        TypeDesc builder = BindType(context, new RegistrationTypeIdentity(coreIdentity, "AtomUI.Registration.ControlPackageRegistrationBuilder"));
        TypeDesc fragmentBase = BindType(context, new RegistrationTypeIdentity(coreIdentity, "AtomUI.Registration.ControlRegistrationFragmentAttribute"));
        var attribute = (MetadataType)BindType(context, new RegistrationTypeIdentity(coreIdentity, "AtomUI.Registration.ConditionalRegistrationRecordAttribute"));
        MethodDesc addFragment = builder.GetMethods().Single(m => m.Name == "AddFragment" && !m.Signature.IsStatic &&
            m.Signature.Length == 1 && m.Signature[0] == fragmentBase && m.Signature.ReturnType.IsVoid);
        TypeDesc runtime = BindType(context, new RegistrationTypeIdentity(coreIdentity, "AtomUI.Registration.ControlRegistrationRuntime"));
        MethodDesc trimmedGetter = runtime.GetMethods().Single(m => m.Name == "get_IsTrimmed");
        TrimmedSwitchTemplate.Validate(trimmedGetter, context.GetWellKnownType(WellKnownType.Boolean), original);
        var groups = new List<RegistrationDefinitions.Group>();
        foreach (ConditionalRegistrationManifest manifest in manifests)
        {
            ConditionalRegistrationPackage package = manifest.Package;
            TypeDesc groupType = BindType(context, package.Group);
            var ownerModule = (EcmaModule)context.ResolveAssembly(new AssemblyName(manifest.Metadata.AssemblyIdentity), true);
            ValidatePackageMarker(ownerModule, package, groupType, coreIdentity);
            MethodDesc collect = BindMethod(context, package.Collect, builder);
            MethodDesc full = BindMethod(context, package.CollectFull, builder);
            MethodDesc selected = BindMethod(context, package.CollectSelected, builder);
            if (((MetadataType)groupType).Module != ownerModule || ((MetadataType)collect.OwningType).Module != ownerModule)
            {
                throw new InvalidDataException("Group and collector defining modules differ");
            }
            ValidateVisibility(collect, MethodAttributes.Private);
            ValidateVisibility(full, MethodAttributes.Private);
            ValidateVisibility(selected, MethodAttributes.Private);
            Match(original, collect, new Instruction(ILOpcode.ldarg_0), new Instruction(ILOpcode.call, full), new Instruction(ILOpcode.ret));
            ValidateSelectedStub(original, selected, context);
            var fragmentMethods = new Dictionary<string, MethodDesc>(StringComparer.Ordinal);
            var fullBody = new List<Instruction>();
            foreach (ConditionalRegistrationFragment fragment in manifest.Fragments)
            {
                MethodDesc method = BindMethod(context, fragment.RegistrationMethod, builder);
                ValidateVisibility(method, MethodAttributes.Assembly);
                if (((MetadataType)method.OwningType).Module != ((MetadataType)collect.OwningType).Module || method.OwningType.HasStaticConstructor)
                {
                    throw new InvalidDataException("Registration thunk requires a stateless owner in the package module");
                }
                var proxy = (EcmaType)BindType(context, fragment.Proxy);
                MethodDesc constructor = ValidateProxy(original, proxy, fragmentBase, fragment.FragmentId);
                if (proxy.Module != ((MetadataType)collect.OwningType).Module)
                {
                    throw new InvalidDataException("Proxy must be defined by the package module");
                }
                Match(original, method, new Instruction(ILOpcode.ldarg_0), new Instruction(ILOpcode.newobj, constructor),
                    new Instruction(ILOpcode.callvirt, addFragment), new Instruction(ILOpcode.ret));
                fragmentMethods.Add(fragment.Identity, method);
                fullBody.Add(new Instruction(ILOpcode.ldarg_0));
                fullBody.Add(new Instruction(ILOpcode.call, method));
            }
            fullBody.Add(new Instruction(ILOpcode.ret));
            Match(original, full, fullBody.ToArray());
            var entries = new List<RegistrationDefinitions.Entry>();
            foreach (ConditionalRegistrationFragment fragment in manifest.Fragments)
            {
                foreach (ConditionalRegistrationCondition condition in manifest.Conditions.Where(c => c.Fragment == fragment.Identity))
                {
                    entries.Add(new RegistrationDefinitions.Entry(condition.Identity, fragment.FragmentId, condition.Trigger,
                        BindType(context, condition.Trigger), fragmentMethods[fragment.Identity]));
                }
            }
            string symbol = "__atomui_registration_" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(package.Group.QualifiedName)));
            groups.Add(new RegistrationDefinitions.Group
            {
                Id = package.Group.QualifiedName,
                PackageId = package.Id,
                Builder = builder,
                Entry = collect,
                FullCollector = full,
                Collector = selected,
                Entries = entries.ToArray(),
                SymbolName = symbol
            });
        }
        return new ConditionalInputBinding(groups.OrderBy(g => g.SymbolName, StringComparer.Ordinal).ToArray(), attribute, report,
            Convert.ToHexStringLower(SHA256.HashData(inputBytes)), trimmedGetter);
    }

    private static void ValidatePackageMarker(EcmaModule module, ConditionalRegistrationPackage package,
        TypeDesc group, string coreIdentity)
    {
        foreach (CustomAttributeHandle handle in module.MetadataReader.GetAssemblyDefinition().GetCustomAttributes())
        {
            CustomAttribute attribute = module.MetadataReader.GetCustomAttribute(handle);
            EntityHandle typeHandle;
            if (attribute.Constructor.Kind == HandleKind.MemberReference)
            {
                typeHandle = module.MetadataReader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent;
            }
            else if (attribute.Constructor.Kind == HandleKind.MethodDefinition)
            {
                typeHandle = module.MetadataReader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType();
            }
            else
            {
                continue;
            }
            string name;
            string ns;
            if (typeHandle.Kind == HandleKind.TypeReference)
            {
                var type = module.MetadataReader.GetTypeReference((TypeReferenceHandle)typeHandle);
                name = module.MetadataReader.GetString(type.Name);
                ns = module.MetadataReader.GetString(type.Namespace);
            }
            else if (typeHandle.Kind == HandleKind.TypeDefinition)
            {
                var type = module.MetadataReader.GetTypeDefinition((TypeDefinitionHandle)typeHandle);
                name = module.MetadataReader.GetString(type.Name);
                ns = module.MetadataReader.GetString(type.Namespace);
            }
            else
            {
                continue;
            }
            if (ns != "AtomUI.Registration" || name != "ControlPackageMarkerAttribute")
            {
                continue;
            }
            MethodDesc constructor = module.GetMethod(attribute.Constructor);
            var owner = (MetadataType)constructor.OwningType;
            var arguments = attribute.DecodeValue(new CustomAttributeTypeProvider(module));
            if (owner.Module.Assembly.GetName().FullName != coreIdentity || arguments.FixedArguments.Length != 3 ||
                arguments.NamedArguments.Length != 0 || !Equals(arguments.FixedArguments[0].Value, package.Id) ||
                !Equals(arguments.FixedArguments[1].Value, group) || !Equals(arguments.FixedArguments[2].Value, 1))
            {
                throw new InvalidDataException("Package marker arguments differ from conditional package records");
            }
            return;
        }
        throw new InvalidDataException("Package marker is missing from conditional package");
    }

    private static TypeDesc BindType(CompilerTypeSystemContext context, RegistrationTypeIdentity identity)
    {
        ModuleDesc reference = context.ResolveAssembly(new AssemblyName(identity.Assembly), true);
        if (reference.Assembly.GetName().FullName != identity.Assembly)
        {
            throw new InvalidDataException("Declared assembly identity did not resolve exactly: " + identity.QualifiedName);
        }
        // Normal ILC resolution follows legitimate forwarding (notably System.Void
        // from the reference System.Runtime facade into target System.Private.CoreLib).
        return reference.GetTypeByCustomAttributeTypeName(identity.MetadataName);
    }

    private static MethodDesc BindMethod(CompilerTypeSystemContext context, RegistrationMethodIdentity identity, TypeDesc builder)
    {
        TypeDesc owner = BindType(context, identity.DeclaringType);
        TypeDesc result = BindType(context, identity.ReturnType);
        TypeDesc parameter = BindType(context, identity.Parameters.Single());
        if (owner.HasInstantiation || owner.IsGenericDefinition || parameter != builder || result != context.GetWellKnownType(WellKnownType.Void))
        {
            throw new InvalidDataException("Registration method has an incompatible resolved signature: " + identity.QualifiedName);
        }
        MethodDesc method = owner.GetMethods().SingleOrDefault(m => m.Name == identity.Name && !m.HasInstantiation &&
            m.Signature.Flags == MethodSignatureFlags.Static && !m.Signature.HasEmbeddedSignatureData &&
            m.Signature.ReturnType == result && m.Signature.Length == 1 && m.Signature[0] == builder);
        if (method is not EcmaMethod || method.IsPInvoke || method.IsInternalCall || method.IsAbstract)
        {
            throw new InvalidDataException("Registration method definition is unavailable: " + identity.QualifiedName);
        }
        return method;
    }

    private static void ValidateVisibility(MethodDesc method, MethodAttributes expected)
    {
        if ((((EcmaMethod)method).Attributes & MethodAttributes.MemberAccessMask) != expected)
        {
            throw new InvalidDataException("Registration method visibility differs from the generator template: " + method);
        }
    }

    private static MethodDesc ValidateProxy(ILProvider provider, EcmaType proxy, TypeDesc fragmentBase, string fragmentId)
    {
        if (!proxy.IsSealed || proxy.BaseType != fragmentBase || proxy.HasInstantiation || proxy.IsGenericDefinition)
        {
            throw new InvalidDataException("Invalid registration proxy inheritance: " + proxy);
        }
        MethodDesc constructor = proxy.GetMethods().Single(m => m.IsConstructor && !m.Signature.IsStatic && m.Signature.Length == 0);
        var visibility = ((EcmaMethod)constructor).Attributes & MethodAttributes.MemberAccessMask;
        if (visibility is not (MethodAttributes.Public or MethodAttributes.Assembly))
        {
            throw new InvalidDataException("Proxy constructor is not accessible to its registration thunk");
        }
        int selfAttributes = 0;
        foreach (CustomAttributeHandle handle in proxy.MetadataReader.GetTypeDefinition(proxy.Handle).GetCustomAttributes())
        {
            CustomAttribute attribute = proxy.MetadataReader.GetCustomAttribute(handle);
            if (proxy.EcmaModule.GetMethod(attribute.Constructor) == constructor)
            {
                BlobReader value = proxy.MetadataReader.GetBlobReader(attribute.Value);
                if (value.ReadUInt16() != 1 || value.ReadUInt16() != 0 || value.RemainingBytes != 0)
                {
                    throw new InvalidDataException("Proxy self Attribute must be parameterless");
                }
                selfAttributes++;
            }
        }
        if (selfAttributes != 1)
        {
            throw new InvalidDataException("Registration proxy must have exactly one matching self Attribute");
        }
        MethodDesc getter = proxy.GetMethods().Single(m => m.Name == "get_FragmentId" && !m.Signature.IsStatic && m.Signature.Length == 0);
        Match(provider, getter, new Instruction(ILOpcode.ldstr, fragmentId), new Instruction(ILOpcode.ret));
        return constructor;
    }

    private static void ValidateSelectedStub(ILProvider provider, MethodDesc selected, CompilerTypeSystemContext context)
    {
        MetadataType exception = context.SystemModule.GetKnownType("System", "InvalidOperationException");
        MethodDesc constructor = exception.GetMethods().Single(m => m.IsConstructor && !m.Signature.IsStatic &&
            m.Signature.Length == 1 && m.Signature[0] == context.GetWellKnownType(WellKnownType.String));
        Match(provider, selected, new Instruction(ILOpcode.ldstr, StubMessage), new Instruction(ILOpcode.newobj, constructor), new Instruction(ILOpcode.throw_));
    }

    private static void Match(ILProvider provider, MethodDesc method, params Instruction[] expected)
    {
        MethodIL body = provider.GetMethodIL(method);
        if (body == null || body.GetExceptionRegions().Length != 0)
        {
            throw new InvalidDataException("Registration template has missing IL or exception regions: " + method);
        }
        var actual = new List<Instruction>();
        byte[] bytes = body.GetILBytes();
        int position = 0;
        while (position < bytes.Length)
        {
            ILOpcode code = (ILOpcode)bytes[position++];
            if (code == ILOpcode.nop)
            {
                continue;
            }
            object operand = null;
            if (code is ILOpcode.call or ILOpcode.callvirt or ILOpcode.newobj or ILOpcode.ldstr)
            {
                if (position + 4 > bytes.Length)
                {
                    throw new InvalidDataException("Truncated registration IL token");
                }
                operand = body.GetObject(System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(position, 4)));
                position += 4;
            }
            else if (code is not (ILOpcode.ldarg_0 or ILOpcode.ret or ILOpcode.throw_))
            {
                throw new InvalidDataException("Unsupported registration template opcode: " + code + " in " + method);
            }
            actual.Add(new Instruction(code, operand));
        }
        if (!actual.SequenceEqual(expected))
        {
            throw new InvalidDataException("Registration semantic IL template mismatch: " + method);
        }
    }
}
