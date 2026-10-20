using System.Collections;
using System.Globalization;
using System.Text;

namespace AtomUI.Localization.Data;

// Shared source: the generator/linker and runtime must accept precisely the same bounded format.
internal static class LanguageModuleDataCodec
{
    internal const int Version = 1;
    internal const int MaximumDataLength = 16 * 1024 * 1024;
    internal const int MaximumRecords = 65536;
    internal const string Header = "AUIL1;";

    internal static LanguageModuleData Read(string moduleId, string data)
    {
        if (string.IsNullOrWhiteSpace(moduleId) || data is null ||
            data.Length > MaximumDataLength || !data.StartsWith(Header, StringComparison.Ordinal))
        {
            throw new FormatException("Invalid or unsupported language module data.");
        }
        var reader = new Reader(data, Header.Length);
        if (!string.Equals(reader.Text(), moduleId, StringComparison.Ordinal))
        {
            throw new FormatException("The language module identity does not match its data.");
        }
        var catalogs = new LanguageModuleCatalog[reader.Count()];
        var catalogIds = new HashSet<string>(StringComparer.Ordinal);
        var typeIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < catalogs.Length; index++)
        {
            var catalogId = reader.Identity();
            var assemblyName = reader.Identity();
            var metadataName = reader.Identity();
            if (!catalogId.StartsWith(moduleId + ":", StringComparison.Ordinal) ||
                !catalogIds.Add(catalogId) || !typeIds.Add(assemblyName + ":" + metadataName))
            {
                throw new FormatException("Duplicate or invalid language catalog identity.");
            }
            var units = new LanguageModuleUnit[reader.Count(nonEmpty: true)];
            var keys = new HashSet<string>(StringComparer.Ordinal);
            var enumValues = new HashSet<int>();
            for (var slot = 0; slot < units.Length; slot++)
            {
                var key = reader.Identity();
                var enumValue = reader.Count();
                var formatted = reader.Count();
                if (!keys.Add(key) || !enumValues.Add(enumValue) || enumValue >= units.Length || formatted > 1)
                {
                    throw new FormatException("Duplicate or invalid language unit slot.");
                }
                units[slot] = new LanguageModuleUnit(key, enumValue, formatted == 1);
            }
            catalogs[index] = new LanguageModuleCatalog(catalogId, assemblyName, metadataName, units);
        }
        var bundles = new LanguageModuleBundle[reader.Count()];
        for (var index = 0; index < bundles.Length; index++)
        {
            var catalogId = reader.Identity();
            var language = reader.Identity();
            var sourceKind = reader.Count();
            var sourceIdentity = reader.Identity();
            if (sourceKind > 2)
            {
                throw new FormatException("Invalid translation source priority.");
            }
            var values = reader.Values();
            bundles[index] = new LanguageModuleBundle(catalogId, language, sourceKind, sourceIdentity, values);
        }
        if (!reader.AtEnd)
        {
            throw new FormatException("Trailing language module data.");
        }
        return new LanguageModuleData(catalogs, bundles);
    }

    internal static void WriteNumber(StringBuilder builder, int value)
    {
        builder.Append(value.ToString(CultureInfo.InvariantCulture)).Append(':');
    }

    internal static void WriteText(StringBuilder builder, string? value)
    {
        WriteNumber(builder, value?.Length ?? -1);
        if (value is not null)
        {
            builder.Append(value);
        }
    }

    private sealed class Reader
    {
        private readonly string _data;
        private int _position;
        internal Reader(string data, int position) { _data = data; _position = position; }
        internal bool AtEnd => _position == _data.Length;

        internal int Count(bool nonEmpty = false)
        {
            var count = Number();
            if (count < (nonEmpty ? 1 : 0) || count > MaximumRecords || count > _data.Length - _position)
            {
                throw new FormatException("Invalid language module record count.");
            }
            return count;
        }

        internal string Text()
        {
            var length = Length();
            if (length < 0)
            {
                throw new FormatException("A required language module field is null.");
            }
            var result = _data.Substring(_position, length);
            _position += length;
            return result;
        }

        internal string Identity()
        {
            var value = Text();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException("A language module identity is empty.");
            }
            return value;
        }

        internal LanguageModuleValues Values()
        {
            var count = Count(nonEmpty: true);
            var offsets = new int[count];
            var lengths = new int[count];
            for (var slot = 0; slot < count; slot++)
            {
                var length = Length();
                offsets[slot] = _position;
                lengths[slot] = length;
                _position += Math.Max(length, 0);
            }
            return new LanguageModuleValues(_data, offsets, lengths);
        }

        private int Length()
        {
            var length = Number();
            if (length < -1 || length > _data.Length - _position)
            {
                throw new FormatException("Invalid language module field length.");
            }
            return length;
        }

        private int Number()
        {
            var negative = _position < _data.Length && _data[_position] == '-';
            if (negative)
            {
                _position++;
            }
            var start = _position;
            var value = 0;
            while (_position < _data.Length && _data[_position] != ':')
            {
                var digit = _data[_position] - '0';
                if ((uint)digit > 9 || value > (int.MaxValue - digit) / 10 || _position - start >= 10)
                {
                    throw new FormatException("Invalid language module number.");
                }
                value = value * 10 + digit;
                _position++;
            }
            if (_position == start || _position == _data.Length)
            {
                throw new FormatException("Invalid language module number.");
            }
            _position++;
            return negative ? -value : value;
        }
    }
}

internal sealed class LanguageModuleData
{
    internal LanguageModuleData(LanguageModuleCatalog[] catalogs, LanguageModuleBundle[] bundles)
    { Catalogs = catalogs; Bundles = bundles; }
    internal LanguageModuleCatalog[] Catalogs { get; }
    internal LanguageModuleBundle[] Bundles { get; }
}

internal sealed class LanguageModuleCatalog
{
    internal LanguageModuleCatalog(string catalogId, string assemblyName, string metadataName, LanguageModuleUnit[] units)
    { CatalogId = catalogId; AssemblyName = assemblyName; MetadataName = metadataName; Units = units; }
    internal string CatalogId { get; }
    internal string AssemblyName { get; }
    internal string MetadataName { get; }
    internal LanguageModuleUnit[] Units { get; }
}

internal readonly struct LanguageModuleUnit
{
    internal LanguageModuleUnit(string key, int enumValue, bool isFormatted)
    { Key = key; EnumValue = enumValue; IsFormatted = isFormatted; }
    internal string Key { get; }
    internal int EnumValue { get; }
    internal bool IsFormatted { get; }
}

internal sealed class LanguageModuleBundle
{
    internal LanguageModuleBundle(string catalogId, string language, int sourceKind, string sourceIdentity,
        LanguageModuleValues values)
    { CatalogId = catalogId; Language = language; SourceKind = sourceKind; SourceIdentity = sourceIdentity; Values = values; }
    internal string CatalogId { get; }
    internal string Language { get; }
    internal int SourceKind { get; }
    internal string SourceIdentity { get; }
    internal LanguageModuleValues Values { get; }
}

// Only integer slices are retained; translated strings are created by the requested catalog row.
internal sealed class LanguageModuleValues : IReadOnlyList<string?>
{
    private readonly string _data;
    private readonly int[] _offsets;
    private readonly int[] _lengths;
    internal LanguageModuleValues(string data, int[] offsets, int[] lengths)
    { _data = data; _offsets = offsets; _lengths = lengths; }
    public int Count => _offsets.Length;
    public string? this[int index] => _lengths[index] < 0 ? null : _data.Substring(_offsets[index], _lengths[index]);
    internal bool HasValue(int index) => _lengths[index] >= 0;
    public IEnumerator<string?> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
