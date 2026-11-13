using System.Reflection;

namespace AtomUI.Build.Tasks.Registration;

// Same-major admission for the official .NET 8 engines. Internal engine API drift is caught by the
// backend install and the existing frozen-input/receipt/output verification, not by servicing allowlists.
internal static class Net8EngineToolchain
{
    internal const int EngineMajor = 8;
    internal const string BackendCapability = "atomui-illink8-v1";
    private const string MicrosoftPublicKeyToken = "31BF3856AD364E35";
    private const string NativeCompilerPackageId = "runtime.osx-arm64.microsoft.dotnet.ilcompiler";

    internal static bool IsSupportedLinker(string identity, string informationalVersion)
    {
        if (!IsEngineVersion(informationalVersion))
        {
            return false;
        }

        try
        {
            var name = new AssemblyName(identity);
            return name.Name == "illink" && name.Version == new Version(EngineMajor, 0, 0, 0) &&
                   Convert.ToHexString(name.GetPublicKeyToken() ?? []) == MicrosoftPublicKeyToken;
        }
        catch (Exception error) when (error is ArgumentException or FileLoadException)
        {
            return false;
        }
    }

    internal static bool IsSameEngine(string tasksInformationalVersion, string linkerInformationalVersion) =>
        OfficialTypeMapToolchain.TryParseVersion(tasksInformationalVersion, out var tasks) &&
        OfficialTypeMapToolchain.TryParseVersion(linkerInformationalVersion, out var linker) &&
        tasks == linker;

    internal static bool IsSupportedNativeCompilerPackage(string packageDirectory)
    {
        var package = new DirectoryInfo(Path.TrimEndingDirectorySeparator(packageDirectory));
        return string.Equals(package.Parent?.Name, NativeCompilerPackageId, StringComparison.OrdinalIgnoreCase) && IsEngineVersion(package.Name);
    }

    internal static bool IsEngineVersion(string informationalVersion) =>
        OfficialTypeMapToolchain.TryParseVersion(informationalVersion, out var version) &&
        version.Major == EngineMajor;
}
