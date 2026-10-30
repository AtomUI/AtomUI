using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace AtomUI.Build.Tasks.Registration;

// Independent post-Output inspection. It does not reuse the linker's mutable Cecil model.
internal sealed class ConditionalDispatchReader : IDisposable
{
    private readonly MemoryStream _stream;
    private readonly PEReader _pe;
    private readonly MetadataReader _reader;
    private readonly Dictionary<string, TypeDefinitionHandle> _types = new(StringComparer.Ordinal);
    internal string Identity { get; }
    internal string Hash { get; }

    internal ConditionalDispatchReader(string path)
    {
        var bytes = File.ReadAllBytes(path);
        Hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
        _stream = new MemoryStream(bytes, writable: false);
        _pe = new PEReader(_stream);
        _reader = _pe.GetMetadataReader();
        Identity = ConditionalRegistrationMetadata.GetIdentity(_reader, _reader.GetAssemblyDefinition());
        foreach (var type in _reader.TypeDefinitions)
        {
            if (!_types.TryAdd(TypeName(type), type))
            {
                throw new InvalidDataException("Duplicate type definitions in linked registration output.");
            }
        }
    }

    internal void VerifyCalls(RegistrationMethodIdentity method, IReadOnlyList<RegistrationMethodIdentity> targets,
        string coreIdentity)
    {
        var definition = Find(method, coreIdentity);
        var il = _pe.GetMethodBody(_reader.GetMethodDefinition(definition).RelativeVirtualAddress).GetILBytes()
            ?? throw new InvalidDataException("Linked registration collector has no IL.");
        var cursor = 0;
        foreach (var target in targets)
        {
            var expected = Find(target, coreIdentity);
            if (ReadOpcode(il, ref cursor) != 0x02 || ReadOpcode(il, ref cursor) != 0x28 || cursor + 4 > il.Length)
            {
                throw new InvalidDataException("Linked registration dispatch does not call exactly its analyzed entries.");
            }
            var token = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(cursor, 4));
            cursor += 4;
            if (token != MetadataTokens.GetToken(expected))
            {
                throw new InvalidDataException("Linked registration dispatch targets an unexpected method.");
            }
        }
        if (ReadOpcode(il, ref cursor) != 0x2a || ReadOpcode(il, ref cursor) != -1)
        {
            throw new InvalidDataException("Linked registration dispatch has a residual stub or additional operations.");
        }
        if (_pe.GetMethodBody(_reader.GetMethodDefinition(definition).RelativeVirtualAddress).ExceptionRegions.Length != 0)
        {
            throw new InvalidDataException("Unexpected exception region in linked registration dispatch.");
        }
    }

    internal void VerifyTrimmedMode()
    {
        if (!_types.TryGetValue("AtomUI.Registration.ControlRegistrationRuntime", out var owner))
        {
            return; // The official optimizer may remove the unused mode API together with its owner.
        }
        var methods = _reader.GetTypeDefinition(owner).GetMethods().Where(handle =>
            _reader.GetString(_reader.GetMethodDefinition(handle).Name) == "get_IsTrimmed").ToArray();
        if (methods.Length == 0)
        {
            return; // Constant call sites may have been folded and the getter swept.
        }
        if (methods.Length != 1)
        {
            throw new InvalidDataException("Ambiguous registration mode getter in linked Core.");
        }
        var method = _reader.GetMethodDefinition(methods[0]);
        ReadOnlySpan<byte> signature = [0x00, 0x00, 0x02]; // static bool(), no generic parameters
        if ((method.Attributes & MethodAttributes.Static) == 0 || method.RelativeVirtualAddress == 0 ||
            !_reader.GetBlobBytes(method.Signature).AsSpan().SequenceEqual(signature))
        {
            throw new InvalidDataException("Invalid registration mode getter in linked Core.");
        }
        var body = _pe.GetMethodBody(method.RelativeVirtualAddress);
        var bytes = body.GetILBytes() ?? throw new InvalidDataException("Missing registration mode IL.");
        var cursor = 0;
        if (body.ExceptionRegions.Length != 0 || ReadOpcode(bytes, ref cursor) != 0x17 ||
            ReadOpcode(bytes, ref cursor) != 0x2a || ReadOpcode(bytes, ref cursor) != -1)
        {
            throw new InvalidDataException("Linked registration mode must be folded to true.");
        }
    }

    private MethodDefinitionHandle Find(RegistrationMethodIdentity expected, string coreIdentity)
    {
        if (expected.DeclaringType.Assembly != Identity || !_types.TryGetValue(expected.DeclaringType.MetadataName, out var owner))
        {
            throw new InvalidDataException("Selected registration owner is missing from linked output: " + expected.QualifiedName);
        }
        var candidates = _reader.GetTypeDefinition(owner).GetMethods().Where(handle =>
        {
            var method = _reader.GetMethodDefinition(handle);
            return _reader.GetString(method.Name) == expected.Name && IsBuilderMethod(method, coreIdentity);
        }).ToArray();
        if (candidates.Length != 1 || _reader.GetMethodDefinition(candidates[0]).RelativeVirtualAddress == 0)
        {
            throw new InvalidDataException("Selected registration method is absent or ambiguous: " + expected.QualifiedName);
        }
        return candidates[0];
    }

    private bool IsBuilderMethod(MethodDefinition method, string coreIdentity)
    {
        if ((method.Attributes & MethodAttributes.Static) == 0)
        {
            return false;
        }
        var signature = _reader.GetBlobReader(method.Signature);
        if (signature.ReadByte() != 0 || signature.ReadCompressedInteger() != 1 ||
            signature.ReadByte() != 0x01 || signature.ReadByte() != 0x12)
        {
            return false;
        }
        var codedType = signature.ReadCompressedInteger();
        if (codedType < 0 || signature.RemainingBytes != 0)
        {
            return false;
        }
        var row = codedType >> 2;
        string typeName;
        string assembly;
        if ((codedType & 3) == 0)
        {
            typeName = TypeName(MetadataTokens.TypeDefinitionHandle(row));
            assembly = Identity;
        }
        else if ((codedType & 3) == 1)
        {
            var type = _reader.GetTypeReference(MetadataTokens.TypeReferenceHandle(row));
            if (type.ResolutionScope.Kind != HandleKind.AssemblyReference)
            {
                return false;
            }
            typeName = _reader.GetString(type.Namespace) + "." + _reader.GetString(type.Name);
            assembly = ConditionalRegistrationMetadata.GetIdentity(_reader,
                _reader.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope));
        }
        else
        {
            return false;
        }
        return typeName == "AtomUI.Registration.ControlPackageRegistrationBuilder" && assembly == coreIdentity;
    }

    private string TypeName(TypeDefinitionHandle handle)
    {
        var type = _reader.GetTypeDefinition(handle);
        var parent = type.GetDeclaringType();
        if (!parent.IsNil)
        {
            return TypeName(parent) + "+" + _reader.GetString(type.Name);
        }
        var space = _reader.GetString(type.Namespace);
        return (space.Length == 0 ? "" : space + ".") + _reader.GetString(type.Name);
    }

    private static int ReadOpcode(byte[] bytes, ref int cursor)
    {
        while (cursor < bytes.Length && bytes[cursor] == 0)
        {
            cursor++;
        }
        return cursor == bytes.Length ? -1 : bytes[cursor++];
    }

    public void Dispose()
    {
        _pe.Dispose();
        _stream.Dispose();
    }
}
