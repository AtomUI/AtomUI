using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Reflection;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>Records the distinct input-consumption, compiler-output and publish-copy boundaries.</summary>
public sealed class VerifyConditionalRegistrationBridgeTask : RegistrationBuildTask
{
    [Required] public string InputsPath { get; set; } = string.Empty;
    [Required] public string TransformationPath { get; set; } = string.Empty;
    public ITaskItem[] ConsumedAssemblies { get; set; } = [];
    public ITaskItem[] ConsumedAdditionalInputs { get; set; } = [];
    public string OutputDirectory { get; set; } = string.Empty;
    public ITaskItem[] OutputFiles { get; set; } = [];
    [Required] public string ReceiptPath { get; set; } = string.Empty;
    [Required] public string EngineKind { get; set; } = string.Empty;
    [Required] public string Phase { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            if (EngineKind is not ("illink" or "nativeaot" or "wasm") ||
                Phase is not ("analysis-input" or "linked-output" or "published-output"))
            {
                throw new InvalidDataException("Unknown bridge verification engine or phase.");
            }
            using var inputs = JsonDocument.Parse(File.ReadAllBytes(InputsPath));
            using var transformation = JsonDocument.Parse(File.ReadAllBytes(TransformationPath));
            RegistrationContractJson.ValidateUniqueFields(inputs.RootElement);
            RegistrationContractJson.ValidateUniqueFields(transformation.RootElement);
            var source = inputs.RootElement;
            var map = transformation.RootElement;
            RegistrationContractJson.Fields(source, "format", "coreAssemblyIdentity", "assemblies", "analysisReportPath", "additionalInputs");
            RegistrationContractJson.Fields(map, "format", "stage", "originalInputHash", "inputFormat", "additionalInputs", "applicationIdentity", "frameworkBindings", "transformed", "inputs");
            if (source.GetProperty("format").GetInt32() != 2 || map.GetProperty("inputFormat").GetInt32() != 2 ||
                map.GetProperty("format").GetInt32() != 1 || map.GetProperty("stage").GetString() != "transformed-input" ||
                map.GetProperty("originalInputHash").GetString() != Hash(InputsPath))
            {
                throw new InvalidDataException("Bridge receipt requires this invocation's format 2 transformed inputs.");
            }
            var original = source.GetProperty("assemblies").EnumerateArray().ToDictionary(
                entry => entry.GetProperty("assemblyIdentity").GetString()!, StringComparer.Ordinal);
            foreach (var entry in original.Values)
            {
                RegistrationContractJson.Fields(entry, "path", "assemblyIdentity", "sha256");
                VerifyFile(entry.GetProperty("path").GetString()!, entry.GetProperty("sha256").GetString()!,
                    entry.GetProperty("assemblyIdentity").GetString());
            }
            var additional = source.GetProperty("additionalInputs").EnumerateArray().ToArray();
            foreach (var entry in additional)
            {
                RegistrationContractJson.Fields(entry, "path", "sha256", "kind");
                VerifyFile(entry.GetProperty("path").GetString()!, entry.GetProperty("sha256").GetString()!);
            }
            var inputAdditional = additional.Select(entry => entry.GetProperty("path").GetString() + "\n" +
                entry.GetProperty("sha256").GetString() + "\n" + entry.GetProperty("kind").GetString()).ToArray();
            var transformedAdditional = map.GetProperty("additionalInputs").EnumerateArray().Select(entry =>
                entry.GetProperty("Path").GetString() + "\n" + entry.GetProperty("Sha256").GetString() + "\n" + entry.GetProperty("Kind").GetString()).ToArray();
            if (inputAdditional.Distinct(StringComparer.Ordinal).Count() != inputAdditional.Length ||
                !inputAdditional.Order(StringComparer.Ordinal).SequenceEqual(transformedAdditional.Order(StringComparer.Ordinal)))
            {
                throw new InvalidDataException("Bridge transformation omitted or changed frozen additional inputs.");
            }
            var replacements = map.GetProperty("transformed").EnumerateArray().ToDictionary(
                entry => entry.GetProperty("AssemblyIdentity").GetString()!, StringComparer.Ordinal);
            foreach (var pair in replacements)
            {
                var entry = pair.Value;
                RegistrationContractJson.Fields(entry, "AssemblyIdentity", "originalPath", "originalHash", "outputPath", "transformedHash", "originalSymbolsHash", "transformedSymbolsHash");
                if (!original.TryGetValue(pair.Key, out var from) ||
                    entry.GetProperty("originalPath").GetString() != from.GetProperty("path").GetString() ||
                    entry.GetProperty("originalHash").GetString() != from.GetProperty("sha256").GetString())
                {
                    throw new InvalidDataException("Bridge replacement is outside the frozen input set.");
                }
                VerifyFile(entry.GetProperty("outputPath").GetString()!, entry.GetProperty("transformedHash").GetString()!, pair.Key);
                VerifySymbols(entry, "originalPath", "originalSymbolsHash");
                VerifySymbols(entry, "outputPath", "transformedSymbolsHash");
            }
            var expected = original.ToDictionary(pair => pair.Key, pair => replacements.TryGetValue(pair.Key, out var replacement)
                ? replacement.GetProperty("outputPath").GetString()! : pair.Value.GetProperty("path").GetString()!, StringComparer.Ordinal);
            var listed = map.GetProperty("inputs").EnumerateArray().ToDictionary(
                entry => entry.GetProperty("AssemblyIdentity").GetString()!, entry => entry.GetProperty("path").GetString()!, StringComparer.Ordinal);
            if (expected.Count != listed.Count || expected.Any(pair => !listed.TryGetValue(pair.Key, out var path) || path != pair.Value))
            {
                throw new InvalidDataException("Bridge compiler input manifest is incomplete or inconsistent.");
            }
            var inputHash = Hash(InputsPath);
            var transformHash = Hash(TransformationPath);
            object receipt;
            if (Phase == "analysis-input")
            {
                var actual = ConsumedAssemblies.Select(item => Path.GetFullPath(item.ItemSpec)).Distinct(StringComparer.Ordinal)
                    .ToDictionary(path => AssemblyName.GetAssemblyName(path).FullName!, StringComparer.Ordinal);
                if (actual.Count != expected.Count || expected.Any(pair => !actual.TryGetValue(pair.Key, out var path) || path != pair.Value))
                {
                    throw new InvalidDataException("The SDK compiler did not consume exactly the prepared bridge assembly paths.");
                }
                var actualAdditional = ConsumedAdditionalInputs.Select(item => Path.GetFullPath(item.ItemSpec)).ToHashSet(StringComparer.Ordinal);
                var expectedAdditional = additional.Where(entry => entry.GetProperty("kind").GetString() is
                    "root-descriptor" or "reference" or "custom-step" or "opaque").Select(entry => entry.GetProperty("path").GetString()!).ToHashSet(StringComparer.Ordinal);
                if (!actualAdditional.SetEquals(expectedAdditional))
                {
                    throw new InvalidDataException("The SDK compiler additional inputs do not match the frozen input paths.");
                }
                receipt = new { format = 1, stage = Phase, engine = EngineKind, inputHash, transformationHash = transformHash,
                    consumed = expected.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => new
                    { assemblyIdentity = pair.Key, path = pair.Value, sha256 = Hash(pair.Value) }),
                    additional = additional.Select(entry => new { path = entry.GetProperty("path").GetString(),
                        kind = entry.GetProperty("kind").GetString(), sha256 = entry.GetProperty("sha256").GetString() }) };
            }
            else
            {
                using var previous = JsonDocument.Parse(File.ReadAllBytes(ReceiptPath));
                var prior = previous.RootElement;
                RegistrationContractJson.ValidateUniqueFields(prior);
                if (prior.GetProperty("inputHash").GetString() != inputHash || prior.GetProperty("transformationHash").GetString() != transformHash ||
                    prior.GetProperty("engine").GetString() != EngineKind ||
                    prior.GetProperty("stage").GetString() != (Phase == "linked-output" ? "analysis-input" : "linked-output"))
                {
                    throw new InvalidDataException("Bridge verification phases must consume the preceding receipt from the same invocation.");
                }
                var directory = Path.GetFullPath(OutputDirectory);
                var files = OutputFiles.Length == 0
                    ? Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Where(path =>
                        Path.GetExtension(path) is ".dll" or ".pdb" || EngineKind == "wasm" && Path.GetExtension(path) == ".wasm").ToArray()
                    : OutputFiles.Select(item => Path.GetFullPath(item.ItemSpec)).Distinct(StringComparer.Ordinal).ToArray();
                if (files.Length == 0)
                {
                    throw new InvalidDataException("Bridge verification has no actual compiler output.");
                }
                var outputs = files.Select(path => Output(path, directory)).ToArray();
                if (Phase == "linked-output")
                {
                    if (EngineKind != "nativeaot")
                    {
                        var managed = files.Where(path => Path.GetExtension(path) == ".dll").Select(path =>
                            ConditionalRegistrationMetadata.Read(path, source.GetProperty("coreAssemblyIdentity").GetString()!)).ToArray();
                        if (managed.Any(assembly => assembly.Records.Count != 0) ||
                            managed.Count(assembly => assembly.AssemblyIdentity == map.GetProperty("applicationIdentity").GetString()) != 1)
                        {
                            throw new InvalidDataException("Official linker output retains candidate records or omits the application.");
                        }
                    }
                    receipt = new { format = 1, stage = Phase, engine = EngineKind, inputHash, transformationHash = transformHash,
                        analysisReceiptHash = Hash(ReceiptPath), outputKind = EngineKind == "nativeaot" ? "compiled-output" : "linked-managed-output",
                        outputs };
                }
                else if (EngineKind == "wasm")
                {
                    var provenance = VerifyBrowserOutputs(prior, files, source.GetProperty("coreAssemblyIdentity").GetString()!);
                    receipt = new { format = 1, stage = Phase, engine = EngineKind, inputHash, transformationHash = transformHash,
                        linkedReceiptHash = Hash(ReceiptPath), outputKind = "sdk-final-wasm-assets", provenance, outputs };
                }
                else
                {
                    var published = outputs.ToDictionary(file => file.relativePath, StringComparer.Ordinal);
                    foreach (var output in prior.GetProperty("outputs").EnumerateArray())
                    {
                        var relative = output.GetProperty("relativePath").GetString()!;
                        if (!published.TryGetValue(relative, out var actual) || actual.sha256 != output.GetProperty("sha256").GetString())
                        {
                            throw new InvalidDataException("The SDK publish output differs from the verified compiler output: " + relative);
                        }
                    }
                    receipt = new { format = 1, stage = Phase, engine = EngineKind, inputHash, transformationHash = transformHash,
                        linkedReceiptHash = Hash(ReceiptPath), outputs };
                }
            }
            RegistrationFiles.ReplaceJson(ReceiptPath, receipt);
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or
                                     BadImageFormatException or InvalidOperationException or JsonException or KeyNotFoundException)
        {
            if (File.Exists(ReceiptPath))
            {
                File.Delete(ReceiptPath);
            }
            return Fail("ATOMUIREG006", error);
        }
    }

    private object VerifyBrowserOutputs(JsonElement linkedReceipt, string[] deployed, string coreIdentity)
    {
        var linked = linkedReceipt.GetProperty("outputs").EnumerateArray().Where(item =>
            Path.GetExtension(item.GetProperty("path").GetString()) == ".dll").ToDictionary(item =>
                AssemblyName.GetAssemblyName(item.GetProperty("path").GetString()!).FullName!, StringComparer.Ordinal);
        foreach (var item in linked.Values)
        {
            VerifyFile(item.GetProperty("path").GetString()!, item.GetProperty("sha256").GetString()!);
        }
        var final = ConsumedAssemblies.Select(item => Path.GetFullPath(item.ItemSpec)).Distinct(StringComparer.Ordinal)
            .ToDictionary(path => AssemblyName.GetAssemblyName(path).FullName!, StringComparer.Ordinal);
        var actual = deployed.Where(path => Path.GetExtension(path) == ".dll")
            .ToDictionary(path => AssemblyName.GetAssemblyName(path).FullName!, StringComparer.Ordinal);
        if (linked.Count == 0 || linked.Keys.Any(identity => !final.ContainsKey(identity)) || final.Count != actual.Count)
        {
            throw new InvalidDataException("The browser SDK final assembly set does not preserve its verified linked input identities.");
        }
        var managed = final.Select(pair =>
        {
            var metadata = ConditionalRegistrationMetadata.Read(pair.Value, coreIdentity);
            if (metadata.Records.Count != 0 || !actual.TryGetValue(pair.Key, out var path) || Hash(path) != metadata.ContentHash)
            {
                throw new InvalidDataException("Browser deployment differs from the SDK's actual final managed asset: " + pair.Key);
            }
            return new { assemblyIdentity = pair.Key,
                linkedSha256 = linked.TryGetValue(pair.Key, out var before) ? before.GetProperty("sha256").GetString() : null,
                finalCompilerPath = pair.Value, finalCompilerSha256 = metadata.ContentHash, publishedPath = path,
                publishedSha256 = Hash(path) };
        }).ToArray();
        var native = ConsumedAdditionalInputs.Select(item => Path.GetFullPath(item.ItemSpec)).Distinct(StringComparer.Ordinal).ToArray();
        if (native.Length == 0 || native.Any(path => Path.GetExtension(path) != ".wasm"))
        {
            throw new InvalidDataException("Browser deployment verification requires the SDK's final native WASM asset.");
        }
        var wasm = native.Select(path =>
        {
            var hash = Hash(path);
            var targets = deployed.Where(file => Path.GetExtension(file) == ".wasm" && Hash(file) == hash).ToArray();
            if (targets.Length != 1)
            {
                throw new InvalidDataException("The SDK's final native WASM asset is missing or ambiguous in deployment.");
            }
            return new { finalCompilerPath = path, finalCompilerSha256 = hash, publishedPath = targets[0], publishedSha256 = hash };
        }).ToArray();
        // AOT may legitimately strip IL after linking. Record that compiler boundary explicitly;
        // never compare its changed DLL bytes to the earlier transformation or linker input hash.
        return new { managed, wasm };
    }

    private static VerifiedBridgeOutput Output(string path, string directory)
    {
        var relative = Path.GetRelativePath(directory, path);
        if (!File.Exists(path) || Path.IsPathFullyQualified(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Compiler output lies outside the declared output directory.");
        }
        return new(path, relative, Hash(path));
    }

    private static void VerifySymbols(JsonElement entry, string pathField, string hashField)
    {
        var value = entry.GetProperty(hashField);
        if (value.ValueKind == JsonValueKind.String)
        {
            VerifyFile(Path.ChangeExtension(entry.GetProperty(pathField).GetString()!, ".pdb"), value.GetString()!);
        }
    }

    private static void VerifyFile(string path, string hash, string? identity = null)
    {
        if (!Path.IsPathFullyQualified(path) || hash.Length != 64 || hash.Any(c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')) ||
            Hash(path) != hash || identity is not null && AssemblyName.GetAssemblyName(path).FullName != identity)
        {
            throw new InvalidDataException("Bridge input identity/hash mismatch: " + path);
        }
    }

    private sealed record VerifiedBridgeOutput(string path, string relativePath, string sha256);
}
