using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;

namespace ILCompiler;

internal sealed record RegistrationInputPlan(IReadOnlyDictionary<string, string> Inputs,
    IReadOnlyDictionary<string, string> References, string SnapshotPath,
    IReadOnlyDictionary<string, string> AdditionalPaths)
{
    public string Redirect(string path) => string.IsNullOrEmpty(path) ? path :
        AdditionalPaths.TryGetValue(Path.GetFullPath(path), out string frozen) ? frozen :
        throw new InvalidDataException("Uncovered additional compiler input: " + path);
    public string[] Redirect(string[] paths) => paths.Select(Redirect).ToArray();
}

internal static class RegistrationInputBootstrap
{
    internal static Native8Invocation Invocation { get; private set; }
    internal static bool TryCleanup(string[] arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments.Length != 6 || arguments[0] != "--atomui-cleanup-invocation")
        {
            return false;
        }
        var invocation = new Native8Invocation(arguments[2], arguments[1]);
        var outputs = arguments.Skip(3).Select(invocation.RequireOwned).Concat(new[]
        {
            invocation.File("analysis.json"), invocation.File("receipt.json"), invocation.File("verified.json")
        }).ToArray();
        if (!File.Exists(invocation.File("linked.json")))
        {
            invocation.DeleteOwned(outputs);
        }
        return true;
    }

    private static readonly List<string> AttemptOutputs = new();
    internal static void CleanupFailedAttempt()
    {
        Invocation?.DeleteOwned(AttemptOutputs);
    }

    private static readonly HashSet<string> Kinds = new(StringComparer.Ordinal)
    {
        "root-descriptor", "reference", "custom-step", "custom-step-file", "tool", "symbol", "configuration", "opaque"
    };

    public static RegistrationInputPlan Create(IReadOnlyDictionary<string, string> inputs,
        IReadOnlyDictionary<string, string> references, string objectFile, string exportsFile,
        IReadOnlyDictionary<string, string[]> additional, string[] effectiveArguments)
    {
        string supplied = Environment.GetEnvironmentVariable("ATOMUI_ILC8_INPUTS");
        string prepareRoot = Environment.GetEnvironmentVariable("ATOMUI_ILC8_PREPARE_ROOT");
        string[] originalPaths = inputs.Values.Concat(references.Values).Select(Path.GetFullPath).Distinct(StringComparer.Ordinal).ToArray();
        bool formal = !string.IsNullOrEmpty(supplied) || !string.IsNullOrEmpty(prepareRoot);
        if (formal)
        {
            Invocation = new Native8Invocation(Environment.GetEnvironmentVariable("ATOMUI_ILC8_INVOCATION_ID"),
                Environment.GetEnvironmentVariable("ATOMUI_ILC8_OWNED_ROOT"));
            using var owner = JsonDocument.Parse(File.ReadAllBytes(Invocation.File("invocation.json")));
            if (owner.RootElement.GetProperty("invocationId").GetString() != Invocation.Id ||
                owner.RootElement.GetProperty("ownedRoot").GetString() != Invocation.Root || File.Exists(Invocation.File("linked.json")))
            {
                throw new InvalidDataException("Native compiler invocation is foreign or has already been linked");
            }
            objectFile = Invocation.RequireOwned(Path.GetFullPath(objectFile));
            exportsFile = Invocation.RequireOwned(Path.GetFullPath(exportsFile));
            Invocation.RequireExact(Environment.GetEnvironmentVariable("ATOMUI_ILC8_RECEIPT"), "receipt.json");
        }
        if (formal)
        {
            var protectedPaths = originalPaths.Concat(additional.Values.SelectMany(p => p).Select(Path.GetFullPath)).ToHashSet(StringComparer.Ordinal);
            if (protectedPaths.Contains(Path.GetFullPath(objectFile)) ||
                (!string.IsNullOrEmpty(exportsFile) && protectedPaths.Contains(Path.GetFullPath(exportsFile))))
            {
                throw new InvalidDataException("Compiler output must not alias an input");
            }
            AttemptOutputs.Add(objectFile);
            if (!string.IsNullOrEmpty(exportsFile))
            {
                AttemptOutputs.Add(exportsFile);
            }
            AttemptOutputs.Add(Invocation.File("receipt.json"));
            AttemptOutputs.Add(Invocation.File("analysis.json"));
            CleanupFailedAttempt();
        }
        if (string.IsNullOrEmpty(supplied) && string.IsNullOrEmpty(prepareRoot))
        {
            return null;
        }
        if (!string.IsNullOrEmpty(supplied) && !string.IsNullOrEmpty(prepareRoot))
        {
            throw new InvalidDataException("Choose prepared inputs or an explicit preparation root, not both");
        }
        string toolDirectory = Path.GetDirectoryName(typeof(RegistrationInputBootstrap).Assembly.Location);
        var declared = additional.SelectMany(pair => pair.Value.Select(path => new ConditionalAdditionalInput(Path.GetFullPath(path), pair.Key))).ToList();
        foreach (string path in originalPaths.Concat(additional.GetValueOrDefault("reference") ?? Array.Empty<string>()))
        {
            string symbol = Path.ChangeExtension(path, ".pdb");
            if (File.Exists(symbol))
            {
                declared.Add(new ConditionalAdditionalInput(Path.GetFullPath(symbol), "symbol"));
            }
        }
        // Native helpers and managed dependency resolution are relative to this complete bundle.
        foreach (string path in Directory.GetFiles(toolDirectory))
        {
            declared.Add(new ConditionalAdditionalInput(path, "tool", toolDirectory));
        }
        if (!string.IsNullOrEmpty(prepareRoot))
        {
            string core = originalPaths.Single(p => Path.GetFileName(p) == "AtomUI.Core.dll");
            var configuration = JsonSerializer.Serialize(new { arguments = effectiveArguments });
            string cachedInputs = ConditionalRegistrationSnapshot.PrepareInputs(prepareRoot, originalPaths,
                AssemblyName.GetAssemblyName(core).FullName, configuration, declared);
            var snapshot = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(cachedInputs));
            snapshot["analysisReportPath"] = Invocation.File("analysis.json");
            supplied = Invocation.File("inputs.json");
            using (var stream = new FileStream(supplied, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using var writer = new Utf8JsonWriter(stream);
                snapshot.WriteTo(writer);
            }
            // Source mappings are hints only: Match still verifies each destination against
            // the public manifest hash/kind. Make paths absolute before moving this sidecar.
            string cachedState = Path.Combine(Path.GetDirectoryName(cachedInputs), "input-state.json");
            var state = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(cachedState));
            foreach (var entry in state["inputs"].AsArray())
            {
                entry["RelativePath"] = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(cachedState), entry["RelativePath"].GetValue<string>()));
            }
            File.WriteAllText(Invocation.File("input-state.json"), state.ToJsonString());
        }
        Invocation.RequireExact(supplied, "inputs.json");
        using var document = JsonDocument.Parse(File.ReadAllBytes(supplied));
        JsonElement root = document.RootElement;
        ValidateSnapshot(root);
        string reportPath = Invocation.RequireExact(RegistrationContractJson.String(root, "analysisReportPath"), "analysis.json");
        var frozenAssemblies = root.GetProperty("assemblies").EnumerateArray().ToArray();
        var frozenAdditional = root.GetProperty("additionalInputs").EnumerateArray().ToArray();
        var sourceMappings = new Dictionary<string, string>(StringComparer.Ordinal);
        string statePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(supplied)), "input-state.json");
        if (File.Exists(statePath))
        {
            using var state = JsonDocument.Parse(File.ReadAllBytes(statePath));
            foreach (JsonElement entry in state.RootElement.GetProperty("inputs").EnumerateArray())
            {
                sourceMappings[entry.GetProperty("SourcePath").GetString()] = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(statePath), entry.GetProperty("RelativePath").GetString()));
            }
        }
        string Match(string path, IEnumerable<JsonElement> candidates)
        {
            string hash = Hash(path);
            string[] matches = candidates.Where(entry => entry.GetProperty("sha256").GetString() == hash &&
                Path.GetFileName(entry.GetProperty("path").GetString()) == Path.GetFileName(path))
                .Select(entry => entry.GetProperty("path").GetString()).Distinct(StringComparer.Ordinal).ToArray();
            string fullPath = Path.GetFullPath(path);
            if (matches.Contains(fullPath, StringComparer.Ordinal))
            {
                return fullPath;
            }
            if (sourceMappings.TryGetValue(fullPath, out string mapped) && matches.Contains(mapped, StringComparer.Ordinal))
            {
                return mapped;
            }
            if (matches.Length != 1)
            {
                throw new InvalidDataException("Stale, ambiguous or uncovered compiler input: " + path);
            }
            return matches[0];
        }
        var redirectedAdditional = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (ConditionalAdditionalInput entry in declared)
        {
            redirectedAdditional[Path.GetFullPath(entry.Path)] = Match(entry.Path,
                frozenAdditional.Where(item => item.GetProperty("kind").GetString() == entry.Kind));
        }
        string frozenHost = redirectedAdditional[typeof(RegistrationInputBootstrap).Assembly.Location];
        if (Path.GetFullPath(frozenHost) != Path.GetFullPath(typeof(RegistrationInputBootstrap).Assembly.Location))
        {
            if (Environment.GetEnvironmentVariable("ATOMUI_ILC8_FROZEN_CHILD") == "1")
            {
                throw new InvalidDataException("Frozen compiler bundle did not resolve to the executing host");
            }
            // The initial process is only a parser/copier. All analysis and code generation
            // execute in the frozen bundle with official matching native helpers.
            var start = new ProcessStartInfo(Environment.ProcessPath) { UseShellExecute = false };
            start.ArgumentList.Add(frozenHost);
            foreach (string argument in effectiveArguments)
            {
                start.ArgumentList.Add(argument);
            }
            start.Environment.Remove("ATOMUI_ILC8_PREPARE_ROOT");
            start.Environment["ATOMUI_ILC8_INPUTS"] = Path.GetFullPath(supplied);
            start.Environment["ATOMUI_ILC8_FROZEN_CHILD"] = "1";
            using Process child = Process.Start(start) ?? throw new InvalidOperationException("Cannot start frozen compiler host");
            child.WaitForExit();
            Environment.Exit(child.ExitCode);
        }
        string[] allFrozen = frozenAssemblies.Concat(frozenAdditional).Select(entry => entry.GetProperty("path").GetString()).ToArray();
        if (Path.GetFullPath(reportPath) == Path.GetFullPath(supplied) || originalPaths.Contains(reportPath, StringComparer.Ordinal) ||
            allFrozen.Contains(reportPath, StringComparer.Ordinal))
        {
            throw new InvalidDataException("Analysis report must not overwrite an input snapshot");
        }
        AttemptOutputs.Add(reportPath);
        Invocation.DeleteOwned(new[] { reportPath });
        IReadOnlyDictionary<string, string> Redirect(IReadOnlyDictionary<string, string> source) =>
            source.ToDictionary(entry => entry.Key, entry => Match(entry.Value, frozenAssemblies), StringComparer.Ordinal);
        Console.WriteLine("[AtomUI ILC8] frozenInputs=" + supplied + "; executingHost=" + frozenHost);
        return new RegistrationInputPlan(Redirect(inputs), Redirect(references), Path.GetFullPath(supplied), redirectedAdditional);
    }

    internal static void ValidateSnapshot(JsonElement root)
    {
        RegistrationContractJson.ValidateShape(root);
        RegistrationContractJson.Fields(root, "format", "coreAssemblyIdentity", "assemblies", "additionalInputs", "analysisReportPath");
        if (!root.GetProperty("format").TryGetInt32(out int version) || version != 2 ||
            !Path.IsPathFullyQualified(RegistrationContractJson.String(root, "analysisReportPath")))
        {
            throw new InvalidDataException("Expected conditional input transport v2 with an absolute report path");
        }
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string field in new[] { "assemblies", "additionalInputs" })
        {
            foreach (JsonElement entry in root.GetProperty(field).EnumerateArray())
            {
                RegistrationContractJson.Fields(entry, field == "assemblies" ? new[] { "path", "assemblyIdentity", "sha256" } : new[] { "path", "sha256", "kind" });
                string path = RegistrationContractJson.String(entry, "path");
                string hash = RegistrationContractJson.String(entry, "sha256");
                string kind = field == "assemblies" ? field : RegistrationContractJson.String(entry, "kind");
                if (!Path.IsPathFullyQualified(path) || !seen.Add(kind + "\n" + path) || hash.Length != 64 ||
                    hash.Any(c => !char.IsAsciiHexDigitLower(c)) || Hash(path) != hash ||
                    (field == "additionalInputs" && !Kinds.Contains(kind)))
                {
                    throw new InvalidDataException("Invalid, stale or duplicate frozen compiler input: " + path);
                }
            }
        }
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }
}
