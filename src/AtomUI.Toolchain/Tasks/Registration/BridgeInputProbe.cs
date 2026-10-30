using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.Versioning;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

// This probe only chooses whether the compatibility tool is needed. Its mixed-input path
// still runs the complete Cecil identity/template validation before any compiler consumes it.
internal static class BridgeInputProbe
{
    internal static bool RequiresBridge(IEnumerable<ITaskItem> assemblies)
    {
        var paths = assemblies.Select(item => Path.GetFullPath(item.ItemSpec)).Distinct(StringComparer.Ordinal)
            .Where(IsManagedAssembly).ToArray();
        var cores = paths.Select(path => AssemblyName.GetAssemblyName(path)).Where(name => name.Name == "AtomUI.Core")
            .Select(name => name.FullName!).Distinct(StringComparer.Ordinal).ToArray();
        if (cores.Length == 0)
        {
            return false;
        }
        if (cores.Length != 1)
        {
            throw new InvalidDataException("The bridge requires one resolved AtomUI.Core authority.");
        }
        var conditional = false;
        foreach (var path in paths)
        {
            var input = ConditionalRegistrationMetadata.Read(path, cores[0]);
            if (input.Records.Count != 0)
            {
                ConditionalRegistrationManifest.Parse(input);
                conditional = true;
            }
            else if (input.HasPackageMarker && !HasOfficialProtocol(path, cores[0]))
            {
                throw new InvalidDataException("Marked package has neither conditional records nor an official .NET 10+ TypeMap declaration: " + input.AssemblyIdentity);
            }
        }
        return conditional;
    }

    private static bool IsManagedAssembly(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata)
            {
                if (pe.PEHeaders.CorHeader is not null)
                {
                    throw new InvalidDataException("Bridge input has a CLR header but no metadata: " + path);
                }
                // Windows NativeAOT SDK references also contain native PE DLLs. They
                // cannot carry registration records; leave the SDK input items untouched.
                return false;
            }
            if (!pe.GetMetadataReader().IsAssembly)
            {
                throw new InvalidDataException("Bridge input must have an assembly manifest: " + path);
            }
            return true;
        }
        catch (BadImageFormatException error)
        {
            throw new InvalidDataException("Invalid bridge probe input: " + path, error);
        }
    }

    private static bool HasOfficialProtocol(string path, string core)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var reader = pe.GetMetadataReader();
        Version? frameworkVersion = null;
        string? packageGroup = null;
        foreach (var handle in reader.GetAssemblyDefinition().GetCustomAttributes())
        {
            var attribute = reader.GetCustomAttribute(handle);
            if (TryAttributeType(reader, attribute.Constructor, out var type) &&
                reader.GetString(type.Namespace) == "System.Runtime.Versioning" && reader.GetString(type.Name) == "TargetFrameworkAttribute")
            {
                var value = reader.GetBlobReader(attribute.Value);
                if (value.ReadUInt16() != 1 || value.ReadSerializedString() is not { } moniker)
                {
                    return false;
                }
                var framework = new FrameworkName(moniker);
                if (RegistrationFrameworkPolicy.Classify(framework.Identifier, framework.Version.ToString()) != RegistrationFrameworkFamily.OfficialTypeMap)
                {
                    return false;
                }
                // A package keeps its own framework references when consumed by a later application target.
                frameworkVersion = framework.Version;
            }
            if (TryAttributeType(reader, attribute.Constructor, out type) &&
                reader.GetString(type.Namespace) == "AtomUI.Registration" && reader.GetString(type.Name) == "ControlPackageMarkerAttribute")
            {
                var value = reader.GetBlobReader(attribute.Value);
                if (value.ReadUInt16() != 1 || string.IsNullOrWhiteSpace(value.ReadSerializedString()))
                {
                    return false;
                }
                packageGroup = LocalGroupName(reader, value.ReadSerializedString());
                value.ReadInt32();
                if (value.ReadUInt16() != 0 || value.RemainingBytes != 0)
                {
                    return false;
                }
            }
        }
        if (frameworkVersion is null || packageGroup is null)
        {
            return false;
        }
        foreach (var methodHandle in reader.MethodDefinitions)
        {
            var method = reader.GetMethodDefinition(methodHandle);
            if ((method.Attributes & MethodAttributes.Static) == 0 || method.RelativeVirtualAddress == 0)
            {
                continue;
            }
            foreach (var handle in method.GetCustomAttributes())
            {
                var attribute = reader.GetCustomAttribute(handle);
                if (TryAttributeType(reader, attribute.Constructor, out var type) &&
                    reader.GetString(type.Namespace) == "AtomUI.Registration" && reader.GetString(type.Name) == "GeneratedTypeMapAccessorAttribute" &&
                    type.ResolutionScope.Kind == HandleKind.AssemblyReference &&
                    ConditionalRegistrationMetadata.GetIdentity(reader, reader.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope)) == core)
                {
                    var constructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    var value = reader.GetBlobReader(attribute.Value);
                    if (reader.GetString(constructor.Name) != ".ctor" || value.ReadUInt16() != 1)
                    {
                        return false;
                    }
                    var group = LocalGroupName(reader, value.ReadSerializedString());
                    if (group == packageGroup && value.ReadUInt16() == 0 && value.RemainingBytes == 0 &&
                        IsClosedAccessor(reader, pe, method, group, frameworkVersion.Major))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static bool IsClosedAccessor(MetadataReader reader, PEReader pe, MethodDefinition method, string group, int frameworkMajor)
    {
        var signature = reader.GetBlobReader(method.Signature);
        if (signature.ReadByte() != 0 || signature.ReadCompressedInteger() != 0 || !ReadMapReturn(reader, ref signature, frameworkMajor))
        {
            return false;
        }
        var body = pe.GetMethodBody(method.RelativeVirtualAddress);
        var bytes = body.GetILBytes() ?? [];
        if (body.ExceptionRegions.Length != 0 || !body.LocalSignature.IsNil)
        {
            return false;
        }
        var cursor = 0;
        if (Opcode(bytes, ref cursor) != 0x28 || cursor + 4 > bytes.Length)
        {
            return false;
        }
        var token = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(cursor, 4));
        cursor += 4;
        if (Opcode(bytes, ref cursor) != 0x2a || Opcode(bytes, ref cursor) != -1)
        {
            return false;
        }
        var handle = MetadataTokens.EntityHandle(token);
        if (handle.Kind != HandleKind.MethodSpecification)
        {
            return false;
        }
        var closed = reader.GetMethodSpecification((MethodSpecificationHandle)handle);
        if (closed.Method.Kind != HandleKind.MemberReference)
        {
            return false;
        }
        var arguments = reader.GetBlobReader(closed.Signature);
        if (arguments.ReadByte() != 0x0a || arguments.ReadCompressedInteger() != 1 || arguments.ReadByte() != 0x12)
        {
            return false;
        }
        var argument = arguments.ReadTypeHandle();
        if (arguments.RemainingBytes != 0 || argument.Kind != HandleKind.TypeDefinition ||
            TypeName(reader, (TypeDefinitionHandle)argument) != group)
        {
            return false;
        }
        var target = reader.GetMemberReference((MemberReferenceHandle)closed.Method);
        if (reader.GetString(target.Name) != "GetOrCreateExternalTypeMapping" ||
            !IsFrameworkType(reader, target.Parent, "System.Runtime.InteropServices", "TypeMapping", frameworkMajor))
        {
            return false;
        }
        var targetSignature = reader.GetBlobReader(target.Signature);
        return targetSignature.ReadByte() == 0x10 && targetSignature.ReadCompressedInteger() == 1 &&
               targetSignature.ReadCompressedInteger() == 0 && ReadMapReturn(reader, ref targetSignature, frameworkMajor);
    }

    private static bool ReadMapReturn(MetadataReader reader, ref BlobReader signature, int frameworkMajor)
    {
        return signature.ReadByte() == 0x15 && signature.ReadByte() == 0x12 &&
               IsFrameworkType(reader, signature.ReadTypeHandle(), "System.Collections.Generic", "IReadOnlyDictionary`2", frameworkMajor) &&
               signature.ReadCompressedInteger() == 2 && signature.ReadByte() == 0x0e && signature.ReadByte() == 0x12 &&
               IsFrameworkType(reader, signature.ReadTypeHandle(), "System", "Type", frameworkMajor) && signature.RemainingBytes == 0;
    }

    private static bool IsFrameworkType(MetadataReader reader, EntityHandle handle, string space, string name, int frameworkMajor)
    {
        if (handle.Kind != HandleKind.TypeReference)
        {
            return false;
        }
        var type = reader.GetTypeReference((TypeReferenceHandle)handle);
        if (reader.GetString(type.Namespace) != space || reader.GetString(type.Name) != name ||
            type.ResolutionScope.Kind != HandleKind.AssemblyReference)
        {
            return false;
        }
        var authority = new AssemblyName(ConditionalRegistrationMetadata.GetIdentity(reader,
            reader.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope)));
        return authority.Version == new Version(frameworkMajor, 0, 0, 0) && string.IsNullOrEmpty(authority.CultureName) &&
               (authority.Name == "System.Private.CoreLib" && Convert.ToHexStringLower(authority.GetPublicKeyToken() ?? []) == "7cec85d7bea7798e" ||
                authority.Name is "System.Runtime" or "System.Runtime.InteropServices" or "System.Collections" &&
                Convert.ToHexStringLower(authority.GetPublicKeyToken() ?? []) == "b03f5f7f11d50a3a");
    }

    private static string? LocalGroupName(MetadataReader reader, string? serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return null;
        }
        var parts = serialized.Split(',', 2);
        if (parts.Length == 2 && parts[1].Trim() != ConditionalRegistrationMetadata.GetIdentity(reader, reader.GetAssemblyDefinition()))
        {
            return null;
        }
        return reader.TypeDefinitions.Any(handle => TypeName(reader, handle) == parts[0] &&
            reader.GetTypeDefinition(handle).GetGenericParameters().Count == 0) ? parts[0] : null;
    }

    private static string TypeName(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var type = reader.GetTypeDefinition(handle);
        var parent = type.GetDeclaringType();
        if (!parent.IsNil)
        {
            return TypeName(reader, parent) + "+" + reader.GetString(type.Name);
        }
        var space = reader.GetString(type.Namespace);
        return (space.Length == 0 ? "" : space + ".") + reader.GetString(type.Name);
    }

    private static int Opcode(byte[] bytes, ref int cursor)
    {
        while (cursor < bytes.Length && bytes[cursor] == 0)
        {
            cursor++;
        }
        return cursor == bytes.Length ? -1 : bytes[cursor++];
    }

    private static bool TryAttributeType(MetadataReader reader, EntityHandle constructor, out TypeReference type)
    {
        type = default;
        if (constructor.Kind != HandleKind.MemberReference)
        {
            return false;
        }
        var reference = reader.GetMemberReference((MemberReferenceHandle)constructor);
        if (reference.Parent.Kind != HandleKind.TypeReference)
        {
            return false;
        }
        type = reader.GetTypeReference((TypeReferenceHandle)reference.Parent);
        return true;
    }
}
