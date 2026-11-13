namespace AtomUI.Theme.Schema;

internal static class SchemaIdentifier
{
    internal static void Validate(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (!IsValid(value))
        {
            throw new ArgumentException($"'{value}' is not a valid theme schema identifier.", parameterName);
        }
    }

    /// <summary>
    /// Returns whether the value is a theme schema identifier, without throwing.
    /// </summary>
    internal static bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 128 || !IsStart(value[0]))
        {
            return false;
        }

        for (var index = 1; index < value.Length; index++)
        {
            if (!IsPart(value[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsStart(char value)
    {
        return value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';
    }

    private static bool IsPart(char value)
    {
        return IsStart(value) || value is >= '0' and <= '9' or '.' or '-';
    }
}
