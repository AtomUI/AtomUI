using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Xml.Linq;

if (args.Length == 4 && args[0] == "asset-presence")
{
    using var input = File.OpenRead(args[1]);
    using var pe = new PEReader(input);
    var metadata = pe.GetMetadataReader();
    var hash = 14695981039346656037UL;
    foreach (var character in args[2]) hash = unchecked((hash ^ character) * 1099511628211UL);
    var names = new[] { "CreateResource_" + hash.ToString("X16"), "CreateAsset_" + hash.ToString("X16") };
    foreach (var name in names)
    {
        var present = metadata.TypeDefinitions.Where(handle =>
            metadata.GetString(metadata.GetTypeDefinition(handle).Name) == "GeneratedRegistrationFactories")
            .SelectMany(handle => metadata.GetTypeDefinition(handle).GetMethods())
            .Any(handle => metadata.GetString(metadata.GetMethodDefinition(handle).Name) == name);
        if (present != bool.Parse(args[3]))
            throw new InvalidOperationException($"Unexpected asset factory presence: {args[2]} / {name} = {present}");
    }
    Console.WriteLine($"ASSET_PLATFORM_PASS {args[2]} present={args[3]}");
    return;
}

if (args.Length == 4 && args[0] == "typed-resource-condition")
{
    using var input = File.OpenRead(args[1]);
    using var pe = new PEReader(input);
    var metadata = pe.GetMetadataReader();
    var roots = metadata.MethodDefinitions.Where(h => metadata.GetString(metadata.GetMethodDefinition(h).Name)
        .Contains(args[2], StringComparison.Ordinal)).ToArray();
    if (roots.Length == 0) throw new InvalidOperationException("Compiled AXAML factory not found: " + args[2]);
    var opcodes = typeof(System.Reflection.Emit.OpCodes).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(f => f.FieldType == typeof(System.Reflection.Emit.OpCode)).Select(f => (System.Reflection.Emit.OpCode)f.GetValue(null)!)
        .ToDictionary(op => unchecked((ushort)op.Value));
    var visited = new HashSet<MethodDefinitionHandle>();
    var queue = new Queue<MethodDefinitionHandle>(roots);
    var evidence = new List<string>();
    while (queue.TryDequeue(out var handle))
    {
        if (!visited.Add(handle)) continue;
        var method = metadata.GetMethodDefinition(handle);
        if (method.RelativeVirtualAddress == 0) continue;
        var il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!;
        for (var offset = 0; offset < il.Length;)
        {
            var code = (ushort)il[offset++];
            if (code == 0xfe) code = (ushort)(0xfe00 | il[offset++]);
            var op = opcodes[code];
            var operand = offset;
            var size = op.OperandType switch
            {
                System.Reflection.Emit.OperandType.InlineNone => 0,
                System.Reflection.Emit.OperandType.ShortInlineBrTarget or System.Reflection.Emit.OperandType.ShortInlineI or System.Reflection.Emit.OperandType.ShortInlineVar => 1,
                System.Reflection.Emit.OperandType.InlineVar => 2,
                System.Reflection.Emit.OperandType.InlineI8 or System.Reflection.Emit.OperandType.InlineR => 8,
                System.Reflection.Emit.OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, offset),
                _ => 4
            };
            offset += size;
            if (op.OperandType == System.Reflection.Emit.OperandType.InlineMethod)
            {
                var called = MetadataTokens.EntityHandle(BitConverter.ToInt32(il, operand));
                if (called.Kind == HandleKind.MethodDefinition) queue.Enqueue((MethodDefinitionHandle)called);
            }
            if (op != System.Reflection.Emit.OpCodes.Ldtoken) continue;
            var token = MetadataTokens.EntityHandle(BitConverter.ToInt32(il, operand));
            string? name = null;
            if (token.Kind == HandleKind.TypeDefinition)
            {
                var type = metadata.GetTypeDefinition((TypeDefinitionHandle)token);
                name = metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name);
            }
            else if (token.Kind == HandleKind.TypeReference)
            {
                var type = metadata.GetTypeReference((TypeReferenceHandle)token);
                name = metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name);
            }
            if (name == args[3]) evidence.Add(metadata.GetString(method.Name) + ": ldtoken " + name);
        }
    }
    if (evidence.Count == 0) throw new InvalidOperationException("No real type-key condition in the compiled consumer factory: " + args[3]);
    Console.WriteLine("TYPED_RESOURCE_IL_PASS root=" + args[2] + " " + string.Join("; ", evidence));
    return;
}

if (args.Length == 4 && args[0] == "type-presence")
{
    using var input = File.OpenRead(args[1]);
    using var pe = new PEReader(input);
    var metadata = pe.GetMetadataReader();
    var present = metadata.TypeDefinitions.Any(handle =>
    {
        var type = metadata.GetTypeDefinition(handle);
        return metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name) == args[2];
    });
    if (present != bool.Parse(args[3])) throw new InvalidOperationException($"Unexpected type presence: {args[2]} = {present}");
    Console.WriteLine($"ROOT_SEMANTICS_PASS {args[2]} present={present}");
    return;
}

if (args.Length < 2) throw new ArgumentException("Inspect <directory> <selected|baseline-output.xml>");
var directory = args[0];
var selected = args[1] == "selected";
var pretrim = args[1] == "pretrim";
var candidates = new Dictionary<string, (string Type, string Factory)>
{
    ["AtomUI.Registration.Fixtures.ThirdParty"] = ("UnusedThirdPartyControl", "CreateResource_D5A990C94527F11A"),
    ["AtomUI.Registration.Fixtures.Resources"] = ("UnusedExternalControl", "CreateResource_19D652883E362684")
};
var provenCandidates = new HashSet<string>();
var root = new XElement("linker");
foreach (var path in Directory.EnumerateFiles(directory, "*.dll"))
{
    using var stream = File.OpenRead(path);
    using var pe = new PEReader(stream);
    if (!pe.HasMetadata) continue;
    var metadata = pe.GetMetadataReader();
    var assembly = metadata.GetString(metadata.GetAssemblyDefinition().Name);
    var element = new XElement("assembly", new XAttribute("fullname", assembly));
    if (pretrim && candidates.TryGetValue(assembly, out var candidate))
    {
        var hasType = metadata.TypeDefinitions.Any(h => metadata.GetString(metadata.GetTypeDefinition(h).Name) == candidate.Type);
        var hasFactory = metadata.MethodDefinitions.Any(h => metadata.GetString(metadata.GetMethodDefinition(h).Name) == candidate.Factory);
        var hasMapping = metadata.GetAssemblyDefinition().GetCustomAttributes().Any(h =>
        {
            var attribute = metadata.GetCustomAttribute(h);
            if (attribute.Constructor.Kind != HandleKind.MemberReference) return false;
            var member = metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
            if (member.Parent.Kind != HandleKind.TypeSpecification) return false;
            var signature = metadata.GetBlobReader(metadata.GetTypeSpecification((TypeSpecificationHandle)member.Parent).Signature);
            if (signature.ReadByte() != 0x15 || signature.ReadByte() != 0x12) return false;
            var codedType = signature.ReadCompressedInteger();
            if ((codedType & 3) != 1) return false;
            var type = metadata.GetTypeReference(MetadataTokens.TypeReferenceHandle(codedType >> 2));
            return metadata.GetString(type.Namespace) == "System.Runtime.InteropServices" && metadata.GetString(type.Name) == "TypeMapAttribute`1" &&
                Encoding.UTF8.GetString(metadata.GetBlobBytes(attribute.Value)).Contains(candidate.Type, StringComparison.Ordinal);
        });
        if (!hasType || !hasFactory || !hasMapping) throw new InvalidOperationException($"Pretrim candidate was not generated: {assembly}, type={hasType}, factory={hasFactory}, TypeMap={hasMapping}");
        provenCandidates.Add(assembly);
        Console.WriteLine($"PRETRIM_PASS {assembly}: {candidate.Type}, TypeMap conditional fragment and {candidate.Factory} exist");
    }
    foreach (var handle in metadata.TypeDefinitions)
    {
        var type = metadata.GetTypeDefinition(handle);
        var name = metadata.GetString(type.Name);
        var ns = metadata.GetString(type.Namespace);
        if (selected && (name is "UnusedThirdPartyControl" or "UnusedExternalControl" || name.StartsWith("UnusedThirdPartyControl", StringComparison.Ordinal) || name.StartsWith("UnusedExternalControl", StringComparison.Ordinal)))
            throw new InvalidOperationException("Unused type/asset retained: " + assembly + ":" + ns + "." + name);
        if (selected && name == "GeneratedRegistrationFactories")
            foreach (var methodHandle in type.GetMethods())
            {
                var method = metadata.GetString(metadata.GetMethodDefinition(methodHandle).Name);
                if (method is "CreateResource_D5A990C94527F11A" or "CreateAsset_D5A990C94527F11A" or "AddAsset_D5A990C94527F11A" or
                    "CreateResource_19D652883E362684" or "CreateAsset_19D652883E362684" or "AddAsset_19D652883E362684")
                    throw new InvalidOperationException("Unused registration asset factory retained: " + assembly + ":" + method);
            }
        if (!selected && ns.StartsWith("AtomUI.Generated.", StringComparison.Ordinal) && name.StartsWith("ControlFragment_", StringComparison.Ordinal))
            element.Add(new XElement("type", new XAttribute("fullname", ns + "." + name), new XAttribute("preserve", "all")));
    }
    if (element.HasElements) root.Add(element);
}
if (pretrim)
{
    if (provenCandidates.Count != candidates.Count) throw new InvalidOperationException("Missing pretrim candidate assemblies");
}
else if (selected)
{
    if (Directory.EnumerateFiles(directory).Any(p => Path.GetFileName(p) is "AtomUI.TypeMap.Linker.dll" or "illink.dll" or "Mono.Cecil.dll" or "AtomUI.Build.Tasks.dll"))
        throw new InvalidOperationException("Build tooling leaked into runtime output");
    Console.WriteLine("ARTIFACT_PASS: unused external/control/asset metadata absent; no build tool output");
}
else
{
    if (!root.HasElements) throw new InvalidOperationException("No generated fragment proxies found for full baseline");
    new XDocument(root).Save(args[1]);
    Console.WriteLine("Full registration baseline descriptor written: " + args[1]);
}
