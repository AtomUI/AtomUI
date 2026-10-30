using System.Text;

namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class Bcp47LanguageTagParser
{
    private const int ExtlangPhase = 0;
    private const int ScriptPhase = 1;
    private const int RegionPhase = 2;
    private const int VariantPhase = 3;

    public static bool TryParse(string value, out string canonicalValue)
    {
        canonicalValue = string.Empty;
        var source = value.AsSpan();

        if (source.IsEmpty || source[0] == '-' || source[source.Length - 1] == '-')
        {
            return false;
        }

        if (Bcp47LanguageTagAliases.TryGetGrandfatheredPreferredValue(source, out canonicalValue))
        {
            return true;
        }

        var position = 0;
        if (!TryReadSubtag(source, ref position, out var language))
        {
            return false;
        }

        var builder = new StringBuilder(source.Length);
        if (IsPrivateUseSingleton(language))
        {
            AppendLowerInvariant(builder, language);
            if (!TryParsePrivateUse(source, ref position, builder))
            {
                return false;
            }

            canonicalValue = builder.ToString();
            return true;
        }

        if (!IsLanguage(language))
        {
            return false;
        }

        if (Bcp47LanguageTagAliases.TryGetLanguagePreferredValue(language, out var preferredLanguage))
        {
            builder.Append(preferredLanguage);
        }
        else
        {
            AppendLowerInvariant(builder, language);
        }

        var phase = language.Length <= 3 ? ExtlangPhase : ScriptPhase;
        var extlangCount = 0;
        var extensionSingletons = 0UL;
        var parsingExtension = false;
        var extensionHasSubtag = false;
        var parsingPrivateUse = false;
        var privateUseSubtagCount = 0;

        while (position < source.Length)
        {
            if (!TryReadSubtag(source, ref position, out var subtag))
            {
                return false;
            }

            if (parsingPrivateUse)
            {
                if (!IsPrivateUseSubtag(subtag))
                {
                    return false;
                }

                AppendLowerInvariant(builder, subtag);
                privateUseSubtagCount++;
                continue;
            }

            if (parsingExtension)
            {
                if (subtag.Length == 1)
                {
                    if (!extensionHasSubtag)
                    {
                        return false;
                    }

                    if (IsPrivateUseSingleton(subtag))
                    {
                        AppendLowerInvariant(builder, subtag);
                        parsingPrivateUse = true;
                        privateUseSubtagCount = 0;
                        continue;
                    }

                    if (!TryRegisterExtensionSingleton(subtag[0], ref extensionSingletons))
                    {
                        return false;
                    }

                    AppendLowerInvariant(builder, subtag);
                    extensionHasSubtag = false;
                    continue;
                }

                if (!IsExtensionSubtag(subtag))
                {
                    return false;
                }

                AppendLowerInvariant(builder, subtag);
                extensionHasSubtag = true;
                continue;
            }

            if (phase == ExtlangPhase && extlangCount < 3 && IsExtlang(subtag))
            {
                AppendLowerInvariant(builder, subtag);
                extlangCount++;
                continue;
            }

            if (phase == ExtlangPhase)
            {
                phase = ScriptPhase;
            }

            if (phase <= ScriptPhase && IsScript(subtag))
            {
                AppendTitleInvariant(builder, subtag);
                phase = RegionPhase;
                continue;
            }

            if (phase <= RegionPhase && IsRegion(subtag))
            {
                AppendUpperInvariant(builder, subtag);
                phase = VariantPhase;
                continue;
            }

            if (IsVariant(subtag))
            {
                AppendLowerInvariant(builder, subtag);
                phase = VariantPhase;
                continue;
            }

            if (subtag.Length != 1)
            {
                return false;
            }

            if (IsPrivateUseSingleton(subtag))
            {
                AppendLowerInvariant(builder, subtag);
                parsingPrivateUse = true;
                privateUseSubtagCount = 0;
                continue;
            }

            if (!TryRegisterExtensionSingleton(subtag[0], ref extensionSingletons))
            {
                return false;
            }

            AppendLowerInvariant(builder, subtag);
            parsingExtension = true;
            extensionHasSubtag = false;
        }

        if ((parsingExtension && !parsingPrivateUse && !extensionHasSubtag) ||
            (parsingPrivateUse && privateUseSubtagCount == 0))
        {
            return false;
        }

        canonicalValue = builder.ToString();
        return true;
    }

    private static bool TryParsePrivateUse(
        ReadOnlySpan<char> source,
        ref int position,
        StringBuilder builder)
    {
        var subtagCount = 0;
        while (position < source.Length)
        {
            if (!TryReadSubtag(source, ref position, out var subtag) || !IsPrivateUseSubtag(subtag))
            {
                return false;
            }

            AppendLowerInvariant(builder, subtag);
            subtagCount++;
        }

        return subtagCount > 0;
    }

    private static bool TryReadSubtag(
        ReadOnlySpan<char> source,
        ref int position,
        out ReadOnlySpan<char> subtag)
    {
        var start = position;
        while (position < source.Length && source[position] != '-')
        {
            if (!IsAsciiAlphaNumeric(source[position]))
            {
                subtag = default;
                return false;
            }

            position++;
        }

        subtag = source.Slice(start, position - start);
        if (subtag.IsEmpty)
        {
            return false;
        }

        if (position < source.Length)
        {
            position++;
        }

        return true;
    }

    private static bool IsLanguage(ReadOnlySpan<char> value)
    {
        return value.Length is >= 2 and <= 8 && IsAsciiAlpha(value);
    }

    private static bool IsExtlang(ReadOnlySpan<char> value)
    {
        return value.Length == 3 && IsAsciiAlpha(value);
    }

    private static bool IsScript(ReadOnlySpan<char> value)
    {
        return value.Length == 4 && IsAsciiAlpha(value);
    }

    private static bool IsRegion(ReadOnlySpan<char> value)
    {
        return value.Length == 2 && IsAsciiAlpha(value) ||
               value.Length == 3 && IsAsciiDigit(value);
    }

    private static bool IsVariant(ReadOnlySpan<char> value)
    {
        return value.Length is >= 5 and <= 8 ||
               value.Length == 4 && IsAsciiDigit(value[0]);
    }

    private static bool IsExtensionSubtag(ReadOnlySpan<char> value)
    {
        return value.Length is >= 2 and <= 8;
    }

    private static bool IsPrivateUseSubtag(ReadOnlySpan<char> value)
    {
        return value.Length is >= 1 and <= 8;
    }

    private static bool IsPrivateUseSingleton(ReadOnlySpan<char> value)
    {
        return value.Length == 1 && (value[0] == 'x' || value[0] == 'X');
    }

    private static bool TryRegisterExtensionSingleton(char value, ref ulong registeredSingletons)
    {
        if (!IsAsciiAlphaNumeric(value) || value == 'x' || value == 'X')
        {
            return false;
        }

        var normalizedValue = ToLowerInvariant(value);
        var offset = IsAsciiDigit(normalizedValue)
            ? normalizedValue - '0'
            : normalizedValue - 'a' + 10;
        var flag = 1UL << offset;
        if ((registeredSingletons & flag) != 0)
        {
            return false;
        }

        registeredSingletons |= flag;
        return true;
    }

    private static bool IsAsciiAlpha(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (!IsAsciiAlpha(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAsciiDigit(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (!IsAsciiDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAsciiAlpha(char value)
    {
        return value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    }

    private static bool IsAsciiDigit(char value)
    {
        return value is >= '0' and <= '9';
    }

    private static bool IsAsciiAlphaNumeric(char value)
    {
        return IsAsciiAlpha(value) || IsAsciiDigit(value);
    }

    private static void AppendLowerInvariant(StringBuilder builder, ReadOnlySpan<char> value)
    {
        AppendSeparator(builder);
        foreach (var character in value)
        {
            builder.Append(ToLowerInvariant(character));
        }
    }

    private static void AppendTitleInvariant(StringBuilder builder, ReadOnlySpan<char> value)
    {
        AppendSeparator(builder);
        builder.Append(ToUpperInvariant(value[0]));
        for (var index = 1; index < value.Length; index++)
        {
            builder.Append(ToLowerInvariant(value[index]));
        }
    }

    private static void AppendUpperInvariant(StringBuilder builder, ReadOnlySpan<char> value)
    {
        AppendSeparator(builder);
        foreach (var character in value)
        {
            builder.Append(ToUpperInvariant(character));
        }
    }

    private static void AppendSeparator(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.Append('-');
        }
    }

    private static char ToLowerInvariant(char value)
    {
        return value is >= 'A' and <= 'Z' ? (char)(value + ('a' - 'A')) : value;
    }

    private static char ToUpperInvariant(char value)
    {
        return value is >= 'a' and <= 'z' ? (char)(value - ('a' - 'A')) : value;
    }
}
