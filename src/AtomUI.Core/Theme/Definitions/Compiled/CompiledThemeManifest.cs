using System;
using System.IO;
using System.Text;

namespace AtomUI.Theme.Definitions.Compiled;

// Build-only requirements. Source-linked into the generator; never reflectively read at startup.
internal sealed class CompiledThemeManifest
{
    internal CompiledThemeRequirement[] Requirements = Array.Empty<CompiledThemeRequirement>();

    internal string Encode()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, new UTF8Encoding(false, true), true))
        {
            writer.Write(1); writer.Write(Requirements.Length);
            foreach (var value in Requirements)
            {
                writer.Write(value.ThemeId); writer.Write(value.Catalog); writer.Write(value.ControlId);
                writer.Write(value.OwnerAssemblyName); writer.Write(value.OwnerMetadataName);
                writer.Write(value.Scope); writer.Write(value.TokenName); writer.Write(value.ValueType);
                writer.Write(value.ValueContractVersion); writer.Write(value.Stage);
                writer.Write(value.ResourceKeyType); writer.Write(value.ResourceKeyMember);
            }
        }
        if (stream.Length > 8 * 1024 * 1024) { throw new InvalidDataException("Compiled theme manifest exceeds its byte budget."); }
        return Convert.ToBase64String(stream.ToArray());
    }

    internal static CompiledThemeManifest Decode(string data)
    {
        if (data.Length > 16 * 1024 * 1024) { throw new InvalidDataException("Compiled theme manifest exceeds its byte budget."); }
        var bytes = Convert.FromBase64String(data);
        if (bytes.Length > 8 * 1024 * 1024) { throw new InvalidDataException("Compiled theme manifest exceeds its byte budget."); }
        using var stream = new MemoryStream(bytes, false);
        using var reader = new BinaryReader(stream, new UTF8Encoding(false, true));
        if (reader.ReadInt32() != 1) { throw new InvalidDataException("Unsupported compiled theme manifest version."); }
        var count = reader.ReadInt32();
        if (count < 0 || count > 65536) { throw new InvalidDataException("Compiled theme manifest record budget exceeded."); }
        var values = new CompiledThemeRequirement[count];
        for (var index = 0; index < count; index++)
        {
            values[index] = new CompiledThemeRequirement
            {
                ThemeId = reader.ReadString(), Catalog = reader.ReadString(), ControlId = reader.ReadString(),
                OwnerAssemblyName = reader.ReadString(), OwnerMetadataName = reader.ReadString(),
                Scope = reader.ReadString(), TokenName = reader.ReadString(), ValueType = reader.ReadString(),
                ValueContractVersion = reader.ReadInt32(), Stage = reader.ReadInt32(),
                ResourceKeyType = reader.ReadString(), ResourceKeyMember = reader.ReadString()
            };
            var value = values[index];
            if (string.IsNullOrWhiteSpace(value.ThemeId) ||
                value.Scope is not ("control" or "global" or "controlGlobal" or "own") ||
                (value.Scope != "control" && (string.IsNullOrWhiteSpace(value.TokenName) || value.ValueContractVersion != 1 || value.Stage < 0 || value.Stage > 3)) ||
                (value.Scope != "global" && (string.IsNullOrWhiteSpace(value.Catalog) || string.IsNullOrWhiteSpace(value.ControlId) || string.IsNullOrWhiteSpace(value.OwnerMetadataName))))
            { throw new InvalidDataException("Invalid compiled theme requirement."); }
        }
        if (stream.Position != stream.Length) { throw new InvalidDataException("Unexpected compiled theme manifest data."); }
        return new CompiledThemeManifest { Requirements = values };
    }
}

internal sealed class CompiledThemeRequirement
{
    internal string ThemeId = "";
    internal string Catalog = "";
    internal string ControlId = "";
    internal string OwnerAssemblyName = "";
    internal string OwnerMetadataName = "";
    internal string Scope = "";
    internal string TokenName = "";
    internal string ValueType = "";
    internal int ValueContractVersion;
    internal int Stage;
    internal string ResourceKeyType = "";
    internal string ResourceKeyMember = "";
}
