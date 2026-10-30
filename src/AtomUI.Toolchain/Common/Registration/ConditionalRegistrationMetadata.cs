using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace AtomUI.Build.Tasks.Registration;

internal sealed record ConditionalRegistrationRecord(int Format, string Kind, string Identity, string Payload);

// Reads only the build contract. It never loads an application assembly or executes attributes.
internal sealed record ConditionalRegistrationMetadata(
    string AssemblyPath, string AssemblyIdentity, string ContentHash, string ContractAssemblyIdentity,
    IReadOnlyList<ConditionalRegistrationRecord> Records, bool HasPackageMarker = false)
{
    internal const int MaximumRecordCount = 100_000;
    internal const int MaximumIdentityLength = 65_536;
    internal const int MaximumPayloadLength = 1_048_576;
    private const string ContractNamespace = "AtomUI.Registration";
    private const string ContractName = "ConditionalRegistrationRecordAttribute";
    private const string PackageMarkerName = "ControlPackageMarkerAttribute";

    internal static ConditionalRegistrationMetadata Read(string path, string contractAssemblyIdentity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(contractAssemblyIdentity);
        // Hash exactly the immutable bytes parsed below, not a second read of a mutable build output.
        var bytes = File.ReadAllBytes(path);
        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata)
            {
                throw new InvalidDataException($"Registration input has no CLR metadata: '{path}'.");
            }
            var reader = pe.GetMetadataReader();
            if (!reader.IsAssembly)
            {
                throw new InvalidDataException($"Registration input must be an implementation assembly: '{path}'.");
            }

            var definition = reader.GetAssemblyDefinition();
            var assemblyIdentity = GetIdentity(reader, definition);
            var records = new List<ConditionalRegistrationRecord>();
            var identities = new HashSet<string>(StringComparer.Ordinal);
            var hasPackageMarker = false;
            foreach (var handle in definition.GetCustomAttributes())
            {
                var attribute = reader.GetCustomAttribute(handle);
                if (IsContract(reader, attribute.Constructor, assemblyIdentity, contractAssemblyIdentity, PackageMarkerName))
                {
                    if (hasPackageMarker)
                    {
                        throw new InvalidDataException("Duplicate control package marker in registration input.");
                    }
                    hasPackageMarker = true;
                    continue;
                }
                if (!IsContract(reader, attribute.Constructor, assemblyIdentity, contractAssemblyIdentity))
                {
                    continue;
                }
                if (records.Count == MaximumRecordCount)
                {
                    throw new InvalidDataException("Conditional registration record count exceeds the format 1 limit.");
                }

                var value = reader.GetBlobReader(attribute.Value);
                if (value.ReadUInt16() != 1)
                {
                    throw new InvalidDataException("Invalid conditional registration attribute prolog.");
                }
                var format = value.ReadInt32();
                var kind = value.ReadSerializedString();
                var identity = value.ReadSerializedString();
                var payload = value.ReadSerializedString();
                if (format != 1 || kind is not ("package" or "fragment" or "condition") ||
                    string.IsNullOrWhiteSpace(identity) || identity.Length > MaximumIdentityLength ||
                    string.IsNullOrWhiteSpace(payload) || payload.Length > MaximumPayloadLength ||
                    value.ReadUInt16() != 0 || value.RemainingBytes != 0)
                {
                    throw new InvalidDataException("Unknown or malformed conditional registration record envelope.");
                }
                if (!identities.Add(kind + "\n" + identity))
                {
                    throw new InvalidDataException($"Duplicate conditional registration {kind} identity '{identity}'.");
                }
                records.Add(new(format, kind, identity, payload));
            }
            return new(Path.GetFullPath(path), assemblyIdentity, Convert.ToHexStringLower(SHA256.HashData(bytes)), contractAssemblyIdentity, records.AsReadOnly(), hasPackageMarker);
        }
        catch (BadImageFormatException error)
        {
            throw new InvalidDataException($"Invalid registration metadata in '{path}'.", error);
        }
    }

    private static bool IsContract(MetadataReader reader, EntityHandle constructor,
        string containingAssembly, string expectedAssembly, string contractName = ContractName)
    {
        StringHandle typeName;
        StringHandle typeNamespace;
        StringHandle methodName;
        BlobHandle signature;
        string actualAssembly;
        if (constructor.Kind == HandleKind.MemberReference)
        {
            var member = reader.GetMemberReference((MemberReferenceHandle)constructor);
            if (member.Parent.Kind != HandleKind.TypeReference)
            {
                return false;
            }
            var type = reader.GetTypeReference((TypeReferenceHandle)member.Parent);
            typeName = type.Name;
            typeNamespace = type.Namespace;
            if (!reader.StringComparer.Equals(typeName, contractName) || !reader.StringComparer.Equals(typeNamespace, ContractNamespace))
            {
                return false;
            }
            if (type.ResolutionScope.Kind != HandleKind.AssemblyReference)
            {
                throw new InvalidDataException("Conditional registration contract must have a defining assembly identity.");
            }
            actualAssembly = GetIdentity(reader, reader.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope));
            methodName = member.Name;
            signature = member.Signature;
        }
        else if (constructor.Kind == HandleKind.MethodDefinition)
        {
            var method = reader.GetMethodDefinition((MethodDefinitionHandle)constructor);
            var type = reader.GetTypeDefinition(method.GetDeclaringType());
            typeName = type.Name;
            typeNamespace = type.Namespace;
            actualAssembly = containingAssembly;
            methodName = method.Name;
            signature = method.Signature;
        }
        else
        {
            return false;
        }

        if (!reader.StringComparer.Equals(typeName, contractName) || !reader.StringComparer.Equals(typeNamespace, ContractNamespace))
        {
            return false;
        }
        if (actualAssembly != expectedAssembly || !reader.StringComparer.Equals(methodName, ".ctor"))
        {
            throw new InvalidDataException($"Conditional registration contract identity mismatch: '{actualAssembly}', expected '{expectedAssembly}'.");
        }
        if (contractName == PackageMarkerName)
        {
            // Presence is independent of the conditional records: a stripped record set must not
            // make a product package look like an unrelated assembly. The binder validates the
            // marker arguments and resolved Group against the chosen registration protocol.
            return true;
        }
        // ECMA-335: instance default call, four parameters, void(int32, string, string, string).
        ReadOnlySpan<byte> expectedSignature = [0x20, 0x04, 0x01, 0x08, 0x0e, 0x0e, 0x0e];
        if (!reader.GetBlobBytes(signature).AsSpan().SequenceEqual(expectedSignature))
        {
            throw new InvalidDataException("Conditional registration attribute constructor has an unsupported signature.");
        }
        return true;
    }

    internal static string GetIdentity(MetadataReader reader, AssemblyDefinition definition)
    {
        var name = new AssemblyName
        {
            Name = reader.GetString(definition.Name), Version = definition.Version,
            CultureName = definition.Culture.IsNil ? "" : reader.GetString(definition.Culture)
        };
        name.SetPublicKey(reader.GetBlobBytes(definition.PublicKey));
        return name.FullName!;
    }

    internal static string GetIdentity(MetadataReader reader, AssemblyReference reference)
    {
        var name = new AssemblyName
        {
            Name = reader.GetString(reference.Name), Version = reference.Version,
            CultureName = reference.Culture.IsNil ? "" : reader.GetString(reference.Culture)
        };
        var key = reader.GetBlobBytes(reference.PublicKeyOrToken);
        if ((reference.Flags & AssemblyFlags.PublicKey) != 0)
        {
            name.SetPublicKey(key);
        }
        else
        {
            name.SetPublicKeyToken(key);
        }
        return name.FullName!;
    }
}
