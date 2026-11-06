using System.Reflection;
using System.Security.Cryptography;
using AtomUI.Registration.Shared;
using Mono.Cecil;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

internal static class BridgeProgram
{
    // The Windows .NET host exposes long-path TPA entries as extended DOS/UNC paths in
    // Assembly.Location. Compare that documented filesystem spelling to the frozen input,
    // while retaining both exact file membership and byte verification; never probe caches.
    internal static bool SameToolPath(string first, string second) =>
        string.Equals(ToolPath(first), ToolPath(second), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static string ToolPath(string path)
    {
        var full = Path.GetFullPath(path);
        if (OperatingSystem.IsWindows())
        {
            if (full.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(@"\\" + full[8..]);
            }
            if (full.StartsWith(@"\\?\", StringComparison.Ordinal) && full.Length > 6 &&
                char.IsAsciiLetter(full[4]) && full[5] == ':' && full[6] == '\\')
            {
                return Path.GetFullPath(full[4..]);
            }
        }
        return full;
    }

    internal static void VerifyToolInput(ConditionalInputSnapshot input, string path)
    {
        var frozen = input.AdditionalInputs?.SingleOrDefault(file => file.Kind == "tool" && SameToolPath(file.Path, path));
        if (frozen is null || Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))) != frozen.Sha256)
        {
            throw new InvalidDataException("The bridge host/dependency must match its verified frozen tool input: " + path);
        }
    }

    private static int Main(string[] args)
    {
        try
        {
            if (Environment.Version.Major != 10 || args.Length != 11 || args[0] != "bridge")
            {
                throw new ArgumentException("Usage on .NET10: bridge --input <snapshot-v2> --runtime-pack <sdk-pack> --application <identity> --output <new-directory> --cecil <frozen-cecil>");
            }
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            var required = new[] { "--input", "--runtime-pack", "--application", "--output", "--cecil" };
            for (var index = 1; index < args.Length; index += 2)
            {
                if (!required.Contains(args[index], StringComparer.Ordinal) || !values.TryAdd(args[index], args[index + 1]))
                {
                    throw new ArgumentException("Unknown or duplicate bridge command argument.");
                }
            }
            var input = ConditionalInputSnapshot.Read(values["--input"]);
            if (input.Format != 2)
            {
                throw new InvalidDataException("The controlled bridge host requires SDK input transport format 2.");
            }
            var ownAssembly = typeof(BridgeProgram).Assembly.Location;
            var cecil = typeof(AssemblyDefinition).Assembly.Location;
            if (!SameToolPath(values["--cecil"], cecil) ||
                AssemblyName.GetAssemblyName(values["--cecil"]).FullName != typeof(AssemblyDefinition).Assembly.FullName)
            {
                throw new InvalidDataException("The bridge host loaded a different Cecil implementation than its frozen input.");
            }
            foreach (var path in new[] { ownAssembly, cecil, Path.ChangeExtension(ownAssembly, ".runtimeconfig.json"), Path.ChangeExtension(ownAssembly, ".deps.json") })
            {
                VerifyToolInput(input, path);
            }
            Console.WriteLine(ConditionalInputTransformer.Transform(values["--input"], values["--runtime-pack"],
                values["--application"], values["--output"]));
            return 0;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or
                                     ArgumentException or BadImageFormatException or InvalidOperationException or AssemblyResolutionException)
        {
            Console.Error.WriteLine("ATOMUIREG006: " + error.Message);
            return 1;
        }
    }
}
