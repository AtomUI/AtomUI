namespace AtomUI.Build.Tasks.Registration;

internal enum RegistrationFrameworkFamily
{
    Unsupported,
    Net8Compatibility,
    OfficialTypeMap
}

// Shared by the netstandard Generator and the net10 build worker. Framework selection does
// not certify a tool version/platform: the selected backend must still pass its capability gate.
internal static class RegistrationFrameworkPolicy
{
    internal static RegistrationFrameworkFamily Classify(string? identifier, string? versionText)
    {
        if (!string.Equals(identifier, ".NETCoreApp", StringComparison.OrdinalIgnoreCase) ||
            !TryParseVersion(versionText, out var version))
        {
            return RegistrationFrameworkFamily.Unsupported;
        }
        if (version.Major == 8 && version.Minor == 0)
        {
            return RegistrationFrameworkFamily.Net8Compatibility;
        }
        return version.Major >= 10
            ? RegistrationFrameworkFamily.OfficialTypeMap
            : RegistrationFrameworkFamily.Unsupported;
    }

    internal static bool TryParseVersion(string? text, out Version version)
    {
        var value = (text ?? string.Empty).Trim();
        if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring(1);
        }
        if (Version.TryParse(value, out var parsed))
        {
            version = parsed;
            return true;
        }
        version = new Version(0, 0);
        return false;
    }
}
