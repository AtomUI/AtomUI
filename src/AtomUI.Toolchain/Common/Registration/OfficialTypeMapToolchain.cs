using System.Reflection;

namespace AtomUI.Build.Tasks.Registration;

internal static class OfficialTypeMapToolchain
{
    // Existing backend ABI identifier; its historical suffix is not a servicing-version gate.
    internal const string Capability = "atomui-typemap-v1-illink-10.0.8";
    internal static readonly Version MinimumVersion = new(10, 0, 0);

    internal static bool TryParseVersion(string informationalVersion, out Version version)
    {
        var end = informationalVersion.IndexOfAny(['-', '+']);
        var numeric = end < 0 ? informationalVersion : informationalVersion[..end];
        return Version.TryParse(numeric.Trim(), out version!);
    }

    internal static bool IsSupportedLinker(string identity, string informationalVersion)
    {
        if (!TryParseVersion(informationalVersion, out var version) || version < MinimumVersion)
        {
            return false;
        }

        try
        {
            var name = new AssemblyName(identity);
            return name.Name == "illink" && name.Version?.Major == version.Major &&
                   Convert.ToHexString(name.GetPublicKeyToken() ?? []) == "31BF3856AD364E35";
        }
        catch (Exception error) when (error is ArgumentException or FileLoadException)
        {
            return false;
        }
    }
}
