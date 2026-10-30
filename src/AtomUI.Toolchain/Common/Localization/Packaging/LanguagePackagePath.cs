namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class LanguagePackagePath
{
    internal static bool TryNormalize(string value, out string normalized)
    {
        normalized = value.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(value) ||
            Path.IsPathRooted(value) ||
            normalized.Split('/').Any(static segment => segment is "" or "." or ".."))
        {
            normalized = string.Empty;
            return false;
        }

        return true;
    }
}
