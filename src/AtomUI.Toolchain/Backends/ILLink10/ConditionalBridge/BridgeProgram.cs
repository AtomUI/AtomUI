using AtomUI.Registration.Shared;
using Mono.Cecil;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

internal static class BridgeProgram
{
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
            if (Path.GetFullPath(values["--cecil"]) != Path.GetFullPath(cecil))
            {
                throw new InvalidDataException("The bridge host loaded a different Cecil implementation than its frozen input.");
            }
            foreach (var path in new[] { ownAssembly, cecil, Path.ChangeExtension(ownAssembly, ".runtimeconfig.json"), Path.ChangeExtension(ownAssembly, ".deps.json") })
            {
                if (input.AdditionalInputs?.Any(file => file.Kind == "tool" && file.Path == Path.GetFullPath(path)) != true)
                {
                    throw new InvalidDataException("The bridge host/dependency must belong to its verified frozen tool inputs: " + path);
                }
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
