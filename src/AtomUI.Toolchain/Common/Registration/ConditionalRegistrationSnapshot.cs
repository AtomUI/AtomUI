using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AtomUI.Build.Tasks.Registration;

internal sealed record ConditionalAdditionalInput(string Path, string Kind, string? BundleRoot = null);

// Input transport v2 binds every file consumed by the engine. Bundle layout is semantic input:
// custom steps may resolve sibling assemblies/configuration relative to their own location.
internal static class ConditionalRegistrationSnapshot
{
    internal static string Prepare(string outputRoot, IReadOnlyList<string> assemblyPaths,
        string coreAssemblyIdentity, string configuration, IReadOnlyList<string> additionalInputPaths) =>
        PrepareInputs(outputRoot, assemblyPaths, coreAssemblyIdentity, configuration,
            additionalInputPaths.Select(path => new ConditionalAdditionalInput(path, "configuration")).ToArray());

    internal static string PrepareInputs(string outputRoot, IReadOnlyList<string> assemblyPaths,
        string coreAssemblyIdentity, string configuration, IReadOnlyList<ConditionalAdditionalInput> additionalInputs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(coreAssemblyIdentity);
        if (assemblyPaths.Count == 0)
        {
            throw new InvalidDataException("Conditional registration requires the final implementation assembly inputs.");
        }
        var root = Path.GetFullPath(outputRoot);
        Directory.CreateDirectory(root);
        var staging = Path.Combine(root, ".staging-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        try
        {
            var assemblies = new Dictionary<string, FrozenFile>(StringComparer.Ordinal);
            var files = new List<FrozenFile>();
            var index = 0;
            foreach (var path in assemblyPaths.Distinct(StringComparer.Ordinal))
            {
                var copied = CopyInput(path, staging, index++);
                var metadata = ConditionalRegistrationMetadata.Read(copied, coreAssemblyIdentity);
                var file = new FrozenFile(metadata.AssemblyIdentity, metadata.ContentHash, Path.GetFileName(path), copied,
                    "assemblies", Path.GetFullPath(path), Path.Combine("assemblies", metadata.ContentHash, Path.GetFileName(path)));
                if (assemblies.TryGetValue(file.Identity, out var prior))
                {
                    if (prior.Hash != file.Hash || prior.Name != file.Name)
                    {
                        throw new InvalidDataException($"Conflicting implementation inputs for '{file.Identity}'.");
                    }
                    File.Delete(copied);
                    continue;
                }
                assemblies.Add(file.Identity, file);
            }
            files.AddRange(assemblies.Values.OrderBy(file => file.Identity, StringComparer.Ordinal));
            var expanded = ExpandInputs(additionalInputs, root);
            var additional = new List<FrozenFile>();
            foreach (var input in expanded)
            {
                var copied = CopyInput(input.Path, staging, index++);
                var hash = Hash(copied);
                additional.Add(new(Path.GetFileName(input.Path), hash, Path.GetFileName(input.Path), copied,
                    input.Kind, input.Path, Path.Combine("additional", input.Kind, hash, Path.GetFileName(input.Path)), input.BundleRoot));
            }
            foreach (var bundle in additional.Where(file => file.BundleRoot is not null).GroupBy(file => file.BundleRoot!, StringComparer.Ordinal))
            {
                var layout = bundle.Select(file => new { path = Path.GetRelativePath(bundle.Key, file.SourcePath).Replace('\\', '/'), file.Hash })
                    .Distinct().OrderBy(file => file.path, StringComparer.Ordinal).ToArray();
                var bundleHash = HashBytes(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(layout)));
                foreach (var file in bundle)
                {
                    file.RelativePath = Path.Combine("bundles", bundleHash, Path.GetRelativePath(bundle.Key, file.SourcePath));
                }
            }
            // Preserve the SDK's adjacent portable-PDB discovery for implementation and reference DLLs.
            var symbolOwners = files.Concat(additional.Where(file => file.Kind == "reference"))
                .Where(file => file.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                .GroupBy(file => Path.ChangeExtension(file.SourcePath, ".pdb"), PathComparer)
                .ToDictionary(group => group.Key, group => group.First(), PathComparer);
            foreach (var file in additional.Where(file => file.BundleRoot is null))
            {
                if (symbolOwners.TryGetValue(file.SourcePath, out var owner))
                {
                    file.RelativePath = Path.Combine(Path.GetDirectoryName(owner.RelativePath)!, file.Name);
                }
            }
            files.AddRange(additional);
            var state = JsonSerializer.Serialize(new
            {
                format = 2, coreAssemblyIdentity, configuration,
                inputs = files.Select(file => new { file.Kind, file.Identity, file.Hash, file.Name, file.SourcePath, file.RelativePath })
                    .OrderBy(file => file.Kind, StringComparer.Ordinal).ThenBy(file => file.SourcePath, StringComparer.Ordinal).ToArray()
            });
            var key = HashBytes(Encoding.UTF8.GetBytes(state));
            var destination = Path.Combine(root, key);
            var manifest = JsonSerializer.Serialize(new
            {
                format = 2, coreAssemblyIdentity,
                assemblies = assemblies.Values.OrderBy(file => file.Identity, StringComparer.Ordinal)
                    .Select(file => new { path = Path.Combine(destination, file.RelativePath), assemblyIdentity = file.Identity, sha256 = file.Hash }).ToArray(),
                additionalInputs = additional.Select(file => new { path = Path.Combine(destination, file.RelativePath), sha256 = file.Hash, kind = file.Kind })
                    .Distinct().OrderBy(file => file.path, StringComparer.Ordinal).ThenBy(file => file.kind, StringComparer.Ordinal).ToArray(),
                analysisReportPath = Path.Combine(destination, "analysis.json")
            });
            using var ownership = AcquireLock(Path.Combine(root, key + ".lock"));
            var manifestPath = Path.Combine(destination, "inputs.json");
            if (Directory.Exists(destination))
            {
                if (!File.Exists(manifestPath) || File.ReadAllText(manifestPath) != manifest ||
                    !File.Exists(Path.Combine(destination, "input-state.json")) || File.ReadAllText(Path.Combine(destination, "input-state.json")) != state ||
                    files.Any(file => !File.Exists(Path.Combine(destination, file.RelativePath)) || Hash(Path.Combine(destination, file.RelativePath)) != file.Hash))
                {
                    throw new InvalidDataException("Cached registration input snapshot was changed or is incomplete; refusing stale input reuse.");
                }
                return manifestPath;
            }
            foreach (var file in files)
            {
                var target = Path.Combine(staging, file.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                if (File.Exists(target))
                {
                    File.Delete(file.StagedPath);
                }
                else
                {
                    File.Move(file.StagedPath, target);
                }
            }
            Directory.Delete(Path.Combine(staging, "incoming"), recursive: true);
            File.WriteAllText(Path.Combine(staging, "inputs.json"), manifest);
            File.WriteAllText(Path.Combine(staging, "input-state.json"), state);
            Directory.Move(staging, destination);
            return manifestPath;
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                Directory.Delete(staging, recursive: true);
            }
        }
    }

    private static IReadOnlyList<ConditionalAdditionalInput> ExpandInputs(IReadOnlyList<ConditionalAdditionalInput> inputs, string outputRoot)
    {
        var files = new Dictionary<(string Path, string Kind), ConditionalAdditionalInput>();
        foreach (var input in inputs)
        {
            var path = Path.GetFullPath(input.Path);
            if (input.Kind is not ("root-descriptor" or "reference" or "custom-step" or "custom-step-file" or "tool" or "symbol" or "configuration" or "opaque"))
            {
                throw new InvalidDataException("Unknown additional registration input kind: " + input.Kind);
            }
            var bundleRoot = string.IsNullOrWhiteSpace(input.BundleRoot) ? null : Path.GetFullPath(input.BundleRoot);
            if (input.Kind == "custom-step" && bundleRoot is null)
            {
                throw new InvalidDataException($"Custom step '{path}' requires AtomUIBundleRoot declaring its complete dependency/configuration directory; external opaque files must declare CustomDataKey inputs.");
            }
            if (bundleRoot is not null && (!Contains(bundleRoot, path) || Contains(bundleRoot, outputRoot)))
            {
                throw new InvalidDataException("A declared input bundle must contain its input and must not contain the snapshot output directory.");
            }
            Add(new(path, input.Kind, bundleRoot));
        }
        foreach (var bundle in files.Values.Where(file => file.BundleRoot is not null).GroupBy(file => file.BundleRoot!, StringComparer.Ordinal).ToArray())
        {
            var extraKind = bundle.Any(file => file.Kind is "custom-step" or "custom-step-file") ? "custom-step-file" : "tool";
            var explicitlyDeclared = bundle.Select(file => file.Path).ToHashSet(PathComparer);
            foreach (var path in BundleFiles(bundle.Key))
            {
                if (!explicitlyDeclared.Contains(path))
                {
                    Add(new(path, extraKind, bundle.Key));
                }
            }
        }
        return files.Values.OrderBy(file => file.Path, StringComparer.Ordinal).ThenBy(file => file.Kind, StringComparer.Ordinal).ToArray();

        void Add(ConditionalAdditionalInput input)
        {
            if (!File.Exists(input.Path))
            {
                throw new FileNotFoundException("Declared registration input is missing.", input.Path);
            }
            if (new FileInfo(input.Path).LinkTarget is not null)
            {
                throw new InvalidDataException("Input bundle files must not be symbolic links: " + input.Path);
            }
            if (files.TryGetValue((input.Path, input.Kind), out var prior) && prior.BundleRoot != input.BundleRoot)
            {
                throw new InvalidDataException("Conflicting bundle declarations for " + input.Path);
            }
            files[(input.Path, input.Kind)] = input;
        }
    }

    private static IEnumerable<string> BundleFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.TryPop(out var directory))
        {
            if (new DirectoryInfo(directory).LinkTarget is not null)
            {
                throw new InvalidDataException("Input bundle directories must not be symbolic links: " + directory);
            }
            foreach (var file in Directory.GetFiles(directory))
            {
                yield return Path.GetFullPath(file);
            }
            foreach (var child in Directory.GetDirectories(directory))
            {
                pending.Push(child);
            }
        }
    }
    private static bool Contains(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return !Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }
    private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    private static string CopyInput(string path, string staging, int index)
    {
        var directory = Path.Combine(staging, "incoming", index.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, Path.GetFileName(path));
        using var input = File.OpenRead(path);
        using var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        input.CopyTo(output);
        return target;
    }
    private static FileStream AcquireLock(string path)
    {
        var elapsed = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (elapsed.Elapsed < TimeSpan.FromSeconds(30))
            {
                Thread.Sleep(100);
            }
        }
    }
    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }
    private static string HashBytes(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private sealed record FrozenFile(string Identity, string Hash, string Name, string StagedPath, string Kind,
        string SourcePath, string InitialRelativePath, string? BundleRoot = null)
    {
        internal string RelativePath { get; set; } = InitialRelativePath;
    }
}
