using System.Globalization;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

internal readonly record struct ThemeContentFingerprint(ulong Value)
{
    public override string ToString() => Value.ToString("X16", CultureInfo.InvariantCulture);

    internal static ThemeContentFingerprint Compute(
        ThemeDefinitionRevision definitionRevision,
        NormalizedThemeConfig effectiveConfig,
        ThemeSchemaRevision registryRevision,
        ThemeAppearance appearance)
    {
        var builder = new FingerprintBuilder();
        builder.Add(definitionRevision.SourceIdentity);
        builder.Add(definitionRevision.SourceRevision);
        builder.Add(definitionRevision.ContentDigest);
        builder.Add(definitionRevision.ProgramDigest ?? string.Empty);
        builder.Add(effectiveConfig.Fingerprint.Value);
        builder.Add(registryRevision.Value);
        builder.Add((byte)appearance);
        return new ThemeContentFingerprint(builder.Value);
    }

    private struct FingerprintBuilder
    {
        private const ulong Offset = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;
        private ulong _value;

        internal ulong Value => _value == 0 ? Offset : _value;

        internal void Add(string value)
        {
            Add((ulong)value.Length);
            foreach (var character in value)
            {
                Add(character);
            }
        }

        internal void Add(byte value) => Add((ulong)value);

        internal void Add(ulong value)
        {
            if (_value == 0)
            {
                _value = Offset;
            }

            unchecked
            {
                for (var shift = 0; shift < 64; shift += 8)
                {
                    _value ^= (byte)(value >> shift);
                    _value *= Prime;
                }
            }
        }
    }
}
