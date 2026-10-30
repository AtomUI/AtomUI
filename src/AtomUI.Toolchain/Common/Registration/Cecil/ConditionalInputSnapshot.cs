using System.Security.Cryptography;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;

namespace AtomUI.Registration.Shared;

internal sealed record ConditionalInputAssembly(string Path, string AssemblyIdentity, string Sha256);
internal sealed record ConditionalAdditionalInput(string Path, string Sha256, string Kind);

// The SDK owns the list of final implementation inputs. Discovering assemblies from the linker's
// current cache would omit unrequested packages and turn incomplete input into apparent success.
internal sealed record ConditionalInputSnapshot(string CoreAssemblyIdentity, string AnalysisReportPath,
    IReadOnlyList<ConditionalInputAssembly> Assemblies, string ContentHash,
    IReadOnlyList<ConditionalAdditionalInput>? AdditionalInputs = null, int Format = 1)
{
    internal static ConditionalInputSnapshot Read(string path)
    {
        RequireAbsolute(path);
        var bytes = File.ReadAllBytes(path);
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 16 });
        var root = document.RootElement;
        RegistrationContractJson.ValidateShape(root);
        if (!root.TryGetProperty("format", out var formatValue) || formatValue.ValueKind != JsonValueKind.Number ||
            !formatValue.TryGetInt32(out var format) || format is not (1 or 2))
        {
            throw new InvalidDataException("Unknown conditional registration input format.");
        }
        if (format == 2)
        {
            RegistrationContractJson.Fields(root, "format", "coreAssemblyIdentity", "assemblies", "analysisReportPath", "additionalInputs");
        }
        else
        {
            // Format 1 is retained for mechanism fixtures. SDK production preparation owns the
            // stricter requirement to emit format 2 and validate all consumed auxiliary inputs.
            RegistrationContractJson.Fields(root, "format", "coreAssemblyIdentity", "assemblies", "analysisReportPath");
        }
        var core = RegistrationContractJson.String(root, "coreAssemblyIdentity");
        var report = RegistrationContractJson.String(root, "analysisReportPath");
        RequireAbsolute(report);
        var inputs = root.GetProperty("assemblies");
        if (inputs.ValueKind != JsonValueKind.Array || inputs.GetArrayLength() == 0 || inputs.GetArrayLength() > 100000)
        {
            throw new InvalidDataException("Conditional registration requires a bounded implementation input list.");
        }
        var identities = new HashSet<string>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.Ordinal);
        var assemblies = new List<ConditionalInputAssembly>();
        foreach (var item in inputs.EnumerateArray())
        {
            RegistrationContractJson.Fields(item, "path", "assemblyIdentity", "sha256");
            var assemblyPath = RegistrationContractJson.String(item, "path");
            RequireAbsolute(assemblyPath);
            assemblyPath = Path.GetFullPath(assemblyPath);
            var identity = RegistrationContractJson.String(item, "assemblyIdentity");
            var sha = RegistrationContractJson.String(item, "sha256");
            if (!identities.Add(identity) || !paths.Add(assemblyPath) || sha.Length != 64 || sha.Any(c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
            {
                throw new InvalidDataException("Duplicate input or invalid implementation content identity.");
            }
            assemblies.Add(new(assemblyPath, identity, sha));
        }
        if (!identities.Contains(core) || paths.Contains(Path.GetFullPath(report)) || Path.GetFullPath(report) == Path.GetFullPath(path))
        {
            throw new InvalidDataException("Missing contract implementation or analysis report aliases an input.");
        }
        var additional = new List<ConditionalAdditionalInput>();
        if (format == 2)
        {
            var values = root.GetProperty("additionalInputs");
            if (values.ValueKind != JsonValueKind.Array || values.GetArrayLength() > 100000)
            {
                throw new InvalidDataException("Conditional additional inputs must be a bounded array.");
            }
            var unique = new HashSet<(string Path, string Kind)>();
            foreach (var value in values.EnumerateArray())
            {
                RegistrationContractJson.Fields(value, "path", "sha256", "kind");
                var additionalPath = RegistrationContractJson.String(value, "path");
                RequireAbsolute(additionalPath);
                additionalPath = Path.GetFullPath(additionalPath);
                var hash = RegistrationContractJson.String(value, "sha256");
                var kind = RegistrationContractJson.String(value, "kind");
                if (kind is not ("root-descriptor" or "reference" or "custom-step" or "custom-step-file" or "tool" or "symbol" or "configuration" or "opaque") ||
                    !unique.Add((additionalPath, kind)) || hash.Length != 64 || hash.Any(c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')) ||
                    additionalPath == Path.GetFullPath(report) || additionalPath == Path.GetFullPath(path))
                {
                    throw new InvalidDataException("Invalid, duplicate or aliased conditional additional input.");
                }
                using var content = File.OpenRead(additionalPath);
                if (Convert.ToHexStringLower(SHA256.HashData(content)) != hash)
                {
                    throw new InvalidDataException("Conditional additional input content mismatch: " + additionalPath);
                }
                additional.Add(new(additionalPath, hash, kind));
            }
        }
        return new(core, report, assemblies, Convert.ToHexStringLower(SHA256.HashData(bytes)), additional, format);
    }

    private static void RequireAbsolute(string path)
    {
        if (!Path.IsPathFullyQualified(path))
        {
            throw new InvalidDataException("Conditional registration input and report paths must be absolute.");
        }
    }
}
