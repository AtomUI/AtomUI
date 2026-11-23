using System.Text;

namespace AtomUI.Startup.Data;

internal static class StartupImageFormat
{
    internal const uint Id = 0x314D4941;
}

internal static class StartupManifestCodec
{
    internal const int Version = 3;
    private const int Magic = 0x41554953;
    private const int MaximumExpandedBytes = 4 * 1024 * 1024;
    private const int MaximumWireBytes = MaximumExpandedBytes + 4 + 5 * (MaximumExpandedBytes / 2);
    internal const int MaximumBase64Length = 4 * ((MaximumWireBytes + 2) / 3);
    private const int MaximumRecords = 65536;
    private static readonly UTF8Encoding s_utf8 = new(false, true);

    internal static string Encode(StartupPackageData package)
    {
        using var stream = new MemoryStream();
        using var binary = new BinaryWriter(stream, s_utf8, true);
        var writer = new ManifestWriter(binary);
        WritePackage(writer, package);
        writer.WriteHeaderAndTable();
        WritePackage(writer, package);
        if (stream.Length > MaximumWireBytes)
        {
            throw new InvalidDataException("Startup package manifest exceeds its wire budget.");
        }
        return Convert.ToBase64String(stream.GetBuffer(), 0, checked((int)stream.Length));
    }

    internal static StartupPackageData Decode(string data)
    {
        if (data is null || data.Length > MaximumBase64Length || !IsCanonicalBase64(data))
        {
            throw new InvalidDataException("Invalid startup package manifest Base64.");
        }
        try
        {
            var bytes = Convert.FromBase64String(data);
            if (bytes.Length > MaximumWireBytes)
            {
                throw new InvalidDataException("Startup package manifest exceeds its wire budget.");
            }
            using var stream = new MemoryStream(bytes, false);
            using var binary = new BinaryReader(stream, s_utf8, true);
            if (binary.ReadInt32() != Magic || binary.ReadInt32() != Version)
            {
                throw new InvalidDataException("Unsupported startup package manifest format; rebuild the package.");
            }

            var reader = new ManifestReader(binary, bytes);
            ReadPackage(reader);
            reader.BeginMaterialization();
            return ReadPackage(reader)!;
        }
        catch (Exception error) when (error is FormatException || error is EndOfStreamException ||
                                      error is DecoderFallbackException || error is ArgumentException)
        {
            throw new InvalidDataException("Malformed startup package manifest.", error);
        }
    }

    private static bool IsCanonicalBase64(string value)
    {
        if (value.Length == 0 || (value.Length & 3) != 0)
        {
            return false;
        }
        int end = value.Length;
        while (end > value.Length - 2 && value[end - 1] == '=')
        {
            end--;
        }
        int lastDigit = 0;
        for (var index = 0; index < end; index++)
        {
            var ch = value[index];
            lastDigit = ch switch
            {
                >= 'A' and <= 'Z' => ch - 'A',
                >= 'a' and <= 'z' => ch - 'a' + 26,
                >= '0' and <= '9' => ch - '0' + 52,
                '+' => 62,
                '/' => 63,
                _ => -1
            };
            if (lastDigit < 0)
            {
                return false;
            }
        }
        return (value.Length - end) switch
        {
            2 => (lastDigit & 15) == 0,
            1 => (lastDigit & 3) == 0,
            _ => true
        };
    }

    private static void WritePackage(ManifestWriter writer, StartupPackageData package)
    {
        writer.Write(package.Id);
        writer.Write(package.AssemblyName);
        writer.Write(package.SchemaFingerprint);
        writer.Write(package.GlobalResourceTokens is not null);
        if (package.GlobalResourceTokens is not null)
        {
            writer.WriteTexts(package.GlobalResourceTokens);
        }
        writer.WriteCount(package.Controls.Count);
        foreach (var control in package.Controls)
        {
            writer.Write(control.Catalog);
            writer.Write(control.Id);
            writer.Write(control.TypeName);
            writer.Write(control.AssemblyName ?? package.AssemblyName);
            writer.Write(control.HasSemantic);
            writer.Write(control.HasTokenDescriptor);
            writer.WriteTexts(control.Keys);
            writer.WriteTexts(control.Assets);
            writer.WriteCount(control.OwnTokens.Count);
            foreach (var token in control.OwnTokens)
            {
                writer.Write(token.Name);
                writer.Write(token.ValueType);
                writer.Write(token.KeyType);
                writer.Write(token.KeyMember);
                writer.Write(token.KeyAssembly ?? package.AssemblyName);
            }
            writer.WriteCount(control.SemanticThemes.Count);
            foreach (var theme in control.SemanticThemes)
            {
                WriteTheme(writer, theme);
            }
        }
        writer.WriteCount(package.Assets.Count);
        foreach (var asset in package.Assets)
        {
            writer.Write(asset.Id);
            writer.Write(asset.Uri);
            writer.Write(asset.Phase);
            writer.Write(asset.Order);
            writer.Write(asset.ResourceKeys is not null);
            if (asset.ResourceKeys is not null)
            {
                writer.WriteTexts(asset.ResourceKeys);
            }
            writer.WriteTexts(asset.Owners);
            writer.WriteTexts(asset.SemanticOwners);
            writer.WriteTexts(asset.Exports);
            writer.WriteCount(asset.RequiredOwners.Count);
            foreach (var owner in asset.RequiredOwners)
            {
                WriteOwner(writer, owner);
            }
            writer.WriteCount(asset.SemanticBindings.Count);
            foreach (var binding in asset.SemanticBindings)
            {
                WriteOwner(writer, binding.Owner);
                WriteTheme(writer, binding.Theme);
            }
        }
    }

    private static void WriteOwner(ManifestWriter writer, StartupOwnerData owner)
    {
        writer.Write(owner.Identity);
        writer.Write(owner.AssemblyName);
        writer.Write(owner.TypeName);
    }

    private static void WriteTheme(ManifestWriter writer, StartupSemanticThemeData theme)
    {
        writer.Write(theme.Property);
        writer.Write(theme.TargetAssembly);
        writer.Write(theme.TargetType);
    }

    private static StartupPackageData? ReadPackage(ManifestReader reader)
    {
        var id = reader.ReadText();
        var assembly = reader.ReadText();
        var fingerprint = reader.ReadUInt64();
        var globalTokens = reader.ReadBoolean() ? reader.ReadTexts() : null;
        var controls = reader.ReadList(ReadControl);
        var assets = reader.ReadList(ReadAsset);
        reader.CheckEnd();
        return reader.Materialize ? new StartupPackageData(id, assembly, controls, assets, fingerprint, globalTokens) : null;
    }

    private static StartupControlData ReadControl(ManifestReader reader)
    {
        var catalog = reader.ReadText();
        var id = reader.ReadText();
        var type = reader.ReadText();
        var assembly = reader.ReadText();
        var hasSemantic = reader.ReadBoolean();
        var hasTokenDescriptor = reader.ReadBoolean();
        var keys = reader.ReadTexts();
        var assets = reader.ReadTexts();
        var tokens = reader.ReadList(ReadToken);
        var themes = reader.ReadList(ReadTheme);
        return reader.Materialize
            ? new StartupControlData(catalog, id, type, keys, tokens, hasSemantic, hasTokenDescriptor, assets, assembly, themes)
            : null!;
    }

    private static StartupTokenData ReadToken(ManifestReader reader)
    {
        var name = reader.ReadText();
        var valueType = reader.ReadText();
        var keyType = reader.ReadText();
        var keyMember = reader.ReadText();
        var keyAssembly = reader.ReadText();
        return reader.Materialize ? new StartupTokenData(name, valueType, keyType, keyMember, keyAssembly) : null!;
    }

    private static StartupAssetData ReadAsset(ManifestReader reader)
    {
        var id = reader.ReadText();
        var uri = reader.ReadText();
        var phase = reader.ReadInt32();
        var order = reader.ReadInt32();
        var resourceKeys = reader.ReadBoolean() ? reader.ReadTexts() : null;
        var owners = reader.ReadTexts();
        var semanticOwners = reader.ReadTexts();
        var exports = reader.ReadTexts();
        var required = reader.ReadList(ReadOwner);
        var bindings = reader.ReadList(ReadBinding);
        return reader.Materialize
            ? new StartupAssetData(id, uri, phase, order, resourceKeys, owners, semanticOwners, exports, required, bindings)
            : null!;
    }

    private static StartupOwnerData ReadOwner(ManifestReader reader)
    {
        var identity = reader.ReadText();
        var assembly = reader.ReadText();
        var type = reader.ReadText();
        return reader.Materialize ? new StartupOwnerData(identity, assembly, type) : null!;
    }

    private static StartupSemanticThemeData ReadTheme(ManifestReader reader)
    {
        var property = reader.ReadText();
        var assembly = reader.ReadText();
        var type = reader.ReadText();
        return reader.Materialize ? new StartupSemanticThemeData(property, assembly, type) : null!;
    }

    private static StartupSemanticBindingData ReadBinding(ManifestReader reader)
    {
        var owner = ReadOwner(reader);
        var theme = ReadTheme(reader);
        return reader.Materialize ? new StartupSemanticBindingData(owner, theme) : null!;
    }

    private static int VarSize(uint value)
    {
        var size = 1;
        while (value >= 128)
        {
            value >>= 7;
            size++;
        }
        return size;
    }

    private static void WriteVar(BinaryWriter writer, uint value)
    {
        while (value >= 128)
        {
            writer.Write((byte)((value & 127) | 128));
            value >>= 7;
        }
        writer.Write((byte)value);
    }

    private static uint ReadVar(BinaryReader reader)
    {
        uint value = 0;
        for (var shift = 0; shift <= 28; shift += 7)
        {
            uint part = reader.ReadByte();
            if (shift == 28 && part > 15)
            {
                break;
            }
            value |= (part & 127) << shift;
            if ((part & 128) == 0)
            {
                if (shift != 0 && part == 0)
                {
                    break;
                }
                return value;
            }
        }
        throw new InvalidDataException("Noncanonical startup manifest integer.");
    }

    private static void AddBudget(ref int expanded, int bytes)
    {
        if (bytes > MaximumExpandedBytes - expanded)
        {
            throw new InvalidDataException("Startup package manifest exceeds its expanded byte budget.");
        }
        expanded += bytes;
    }

    private static string RequireText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException("Startup package manifest contains an empty identity.");
        }
        return value;
    }

    private sealed class ManifestWriter(BinaryWriter binary)
    {
        private readonly Dictionary<string, TextEntry> _byValue = new(StringComparer.Ordinal);
        private readonly List<TextEntry> _ordered = [];
        private int _expanded = 8;
        private bool _emit;

        internal void WriteHeaderAndTable()
        {
            var table = new List<TextEntry>();
            foreach (var entry in _ordered)
            {
                var inline = (long)entry.Uses * (entry.Size + VarSize((uint)entry.Size << 1 | 1));
                var shared = entry.Size + VarSize((uint)entry.Size) + (long)entry.Uses * VarSize((uint)table.Count << 1);
                if (shared < inline)
                {
                    entry.Index = table.Count;
                    table.Add(entry);
                }
            }
            binary.Write(Magic);
            binary.Write(Version);
            binary.Write(table.Count);
            foreach (var entry in table)
            {
                binary.Write(entry.Value);
            }
            _emit = true;
        }

        internal void Write(string value)
        {
            if (_emit)
            {
                var entry = _byValue[value];
                if (entry.Index >= 0)
                {
                    WriteVar(binary, (uint)entry.Index << 1);
                    return;
                }
                WriteVar(binary, (uint)entry.Size << 1 | 1);
                WriteUtf8(entry);
                return;
            }

            if (!_byValue.TryGetValue(value, out var text))
            {
                RequireText(value);
                text = new TextEntry(value, s_utf8.GetByteCount(value));
                _byValue.Add(value, text);
                _ordered.Add(text);
            }
            AddBudget(ref _expanded, text.Size);
            AddBudget(ref _expanded, VarSize((uint)text.Size));
            text.Uses++;
        }

        private byte[] _buffer = [];
        private void WriteUtf8(TextEntry entry)
        {
            if (_buffer.Length < entry.Size)
            {
                Array.Resize(ref _buffer, entry.Size);
            }
            var length = s_utf8.GetBytes(entry.Value, 0, entry.Value.Length, _buffer, 0);
            binary.Write(_buffer, 0, length);
        }

        internal void Write(int value)
        {
            if (_emit)
            {
                binary.Write(value);
            }
            else
            {
                AddBudget(ref _expanded, 4);
            }
        }

        internal void Write(ulong value)
        {
            if (_emit)
            {
                binary.Write(value);
            }
            else
            {
                AddBudget(ref _expanded, 8);
            }
        }

        internal void Write(bool value)
        {
            if (_emit)
            {
                binary.Write(value);
            }
            else
            {
                AddBudget(ref _expanded, 1);
            }
        }

        internal void WriteCount(int count)
        {
            if (count > MaximumRecords)
            {
                throw new InvalidDataException("Startup package manifest record budget exceeded.");
            }
            Write(count);
        }

        internal void WriteTexts(IReadOnlyList<string> values)
        {
            WriteCount(values.Count);
            foreach (var value in values)
            {
                Write(value);
            }
        }

        private sealed class TextEntry(string value, int size)
        {
            internal string Value { get; } = value;
            internal int Size { get; } = size;
            internal int Uses { get; set; }
            internal int Index { get; set; } = -1;
        }
    }

    private sealed class ManifestReader
    {
        private readonly BinaryReader _binary;
        private readonly byte[] _bytes;
        private readonly List<(string Value, int Size)> _table;
        private readonly bool[] _used;
        private readonly long _recordOffset;
        private int _remainingTableEntries;
        private int _expanded = 8;

        internal ManifestReader(BinaryReader binary, byte[] bytes)
        {
            _binary = binary;
            _bytes = bytes;
            var count = binary.ReadUInt32();
            if (count > MaximumExpandedBytes / 2 || count > Remaining / 2)
            {
                throw new InvalidDataException("Startup package manifest contains an invalid string table count.");
            }
            _table = new List<(string Value, int Size)>(Math.Min(4096, (int)count));
            var unique = new HashSet<string>(StringComparer.Ordinal);
            var tableBytes = 0;
            for (var index = 0; index < count; index++)
            {
                var rawLength = ReadVar(_binary);
                var length = CheckTextLength(rawLength);
                AddBudget(ref tableBytes, length);
                AddBudget(ref tableBytes, VarSize(rawLength));
                var value = ReadUtf8(length, true);
                if (!unique.Add(value))
                {
                    throw new InvalidDataException("Startup package manifest contains duplicate shared text.");
                }
                _table.Add((value, length));
            }
            _remainingTableEntries = _table.Count;
            _used = new bool[_table.Count];
            _recordOffset = binary.BaseStream.Position;
        }

        internal bool Materialize { get; private set; }
        private long Remaining => _bytes.Length - _binary.BaseStream.Position;

        internal void BeginMaterialization()
        {
            Materialize = true;
            _binary.BaseStream.Position = _recordOffset;
        }

        internal void CheckEnd()
        {
            if (Remaining != 0 || _remainingTableEntries != 0)
            {
                throw new InvalidDataException("Startup package manifest contains unused data.");
            }
        }

        internal string ReadText()
        {
            var tag = ReadVar(_binary);
            var indexOrLength = tag >> 1;
            if ((tag & 1) == 0)
            {
                if (indexOrLength >= _table.Count)
                {
                    throw new InvalidDataException("Startup package manifest has an out-of-range string index.");
                }
                var entry = _table[(int)indexOrLength];
                if (!Materialize && !_used[(int)indexOrLength])
                {
                    _used[(int)indexOrLength] = true;
                    _remainingTableEntries--;
                }
                AddTextBudget(entry.Size);
                return entry.Value;
            }

            var length = CheckTextLength(indexOrLength);
            AddTextBudget(length);
            return ReadUtf8(length, Materialize);
        }

        private int CheckTextLength(uint length)
        {
            if (length == 0 || length > MaximumExpandedBytes || length > Remaining)
            {
                throw new InvalidDataException("Startup package manifest contains an invalid text length.");
            }
            return (int)length;
        }

        private string ReadUtf8(int length, bool materialize)
        {
            var offset = checked((int)_binary.BaseStream.Position);
            _binary.BaseStream.Position += length;
            if (materialize)
            {
                return RequireText(s_utf8.GetString(_bytes, offset, length));
            }
            _ = s_utf8.GetCharCount(_bytes, offset, length);
            return string.Empty;
        }

        private void AddTextBudget(int length)
        {
            if (!Materialize)
            {
                AddBudget(ref _expanded, length);
                AddBudget(ref _expanded, VarSize((uint)length));
            }
        }

        internal int ReadInt32()
        {
            AddFixedBudget(4);
            return _binary.ReadInt32();
        }

        internal ulong ReadUInt64()
        {
            AddFixedBudget(8);
            return _binary.ReadUInt64();
        }

        internal bool ReadBoolean()
        {
            AddFixedBudget(1);
            return _binary.ReadByte() switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidDataException("Startup package manifest contains an invalid flag.")
            };
        }

        private void AddFixedBudget(int size)
        {
            if (!Materialize)
            {
                AddBudget(ref _expanded, size);
            }
        }

        internal string[] ReadTexts() => ReadList(static reader => reader.ReadText());

        internal T[] ReadList<T>(Func<ManifestReader, T> read)
        {
            var count = ReadInt32();
            if (count < 0 || count > MaximumRecords || count > Remaining)
            {
                throw new InvalidDataException("Startup package manifest record budget exceeded or list is truncated.");
            }

            if (!Materialize)
            {
                for (var index = 0; index < count; index++)
                {
                    read(this);
                }
                return Array.Empty<T>();
            }

            if (count == 0)
            {
                return Array.Empty<T>();
            }
            var values = new T[count];
            for (var index = 0; index < values.Length; index++)
            {
                values[index] = read(this);
            }
            return values;
        }
    }
}
