namespace AtomUI.Theme.Build;

// Build-side mirror of AtomUI.Theme.Schema.SchemaIdentifier for code that cannot reference Core
// (the netstandard2.0 generator). Keep both rule sets identical.
internal static class ThemeBuildIdentifier
{
    internal static bool IsValid(string? value)
    {
        if (value is null || value.Length == 0 || value.Length > 128 || !IsStart(value[0]))
        {
            return false;
        }
        for (var index = 1; index < value.Length; index++)
        {
            if (!IsStart(value[index]) && !(value[index] is >= '0' and <= '9' or '.' or '-'))
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsStart(char value) => value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';
}
