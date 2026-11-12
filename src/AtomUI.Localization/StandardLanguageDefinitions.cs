using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AtomUI.Localization;

internal static class StandardLanguageDefinitions
{
    internal static bool TryCreate(
        LanguageTag tag,
        [NotNullWhen(true)] out LanguageDefinition? definition)
    {
        if (tag == default)
        {
            definition = null;
            return false;
        }

        return GeneratedStandardLanguageDefinitions.TryCreate(tag, out definition);
    }

    /// <summary>
    /// Resolves <paramref name="tag"/> in the generated standard-language records.
    /// </summary>
    /// <remarks>
    /// Each record is <c>"\n" + tag + "\t" + canonical tag (empty when equal to tag) + "\t" + culture name +
    /// "\t" + native name + "\t" + ("R" | "L")</c>. The generator rejects fields containing a separator, so a
    /// match on <c>"\n" + tag + "\t"</c> is an exact, ordinal tag match.
    /// </remarks>
    internal static bool TryCreateFromRecords(
        string records,
        string tag,
        [NotNullWhen(true)] out LanguageDefinition? definition)
    {
        definition = null;
        if (tag.Length == 0 || tag.AsSpan().IndexOfAny('\t', '\n') >= 0)
        {
            return false;
        }

        var start = records.IndexOf(string.Concat("\n", tag, "\t"), StringComparison.Ordinal);
        if (start < 0)
        {
            return false;
        }

        var record = records.AsSpan(start + tag.Length + 2);
        var end = record.IndexOf('\n');
        if (end >= 0)
        {
            record = record[..end];
        }

        var canonical = NextField(ref record);
        var culture = NextField(ref record);
        var nativeName = NextField(ref record);
        definition = new LanguageDefinition(
            LanguageTag.FromCanonical(canonical.IsEmpty ? tag : canonical.ToString()),
            CultureInfo.GetCultureInfo(culture.ToString()),
            nativeName.ToString(),
            record is "R" ? LanguageTextDirection.RightToLeft : LanguageTextDirection.LeftToRight);
        return true;
    }

    private static ReadOnlySpan<char> NextField(ref ReadOnlySpan<char> record)
    {
        var separator = record.IndexOf('\t');
        var field = record[..separator];
        record = record[(separator + 1)..];
        return field;
    }
}
