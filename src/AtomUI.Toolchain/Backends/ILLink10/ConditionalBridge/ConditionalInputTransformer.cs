using System.Security.Cryptography;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using AtomUI.Registration.Shared;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

/// <summary>Produces isolated official-TypeMap input copies for either .NET10 ILLink or ILC.</summary>
public static class ConditionalInputTransformer
{
    /// <returns>The transformation manifest path. Downstream compilation still owns selection and final verification.</returns>
    public static string Transform(string snapshotPath, string runtimePackDirectory, string applicationIdentity, string outputDirectory)
    {
        var input = ConditionalInputSnapshot.Read(snapshotPath);
        if (!Path.IsPathFullyQualified(outputDirectory) || Directory.Exists(outputDirectory))
        {
            throw new InvalidDataException("The bridge requires a new absolute isolated output directory.");
        }
        var metadata = input.Assemblies.Select(assembly =>
        {
            var value = ConditionalRegistrationMetadata.Read(assembly.Path, input.CoreAssemblyIdentity);
            if (value.AssemblyIdentity != assembly.AssemblyIdentity || value.ContentHash != assembly.Sha256)
            {
                throw new InvalidDataException("Conditional implementation identity/hash mismatch: " + assembly.Path);
            }
            return value;
        }).ToArray();
        var manifests = metadata.Where(value => value.Records.Count != 0).Select(ConditionalRegistrationManifest.Parse).ToArray();
        using var resolver = new SnapshotResolver(input);
        var owners = manifests.Select(manifest => resolver.Resolve(AssemblyNameReference.Parse(manifest.Metadata.AssemblyIdentity))).ToArray();
        var framework = manifests.Length == 0
            ? new FrameworkBindingSet(new Dictionary<string, string>(StringComparer.Ordinal), [])
            : FrameworkBindingPolicy.Create(runtimePackDirectory, input, owners);
        resolver.SetFrameworkBindings(framework.Bindings);
        ConditionalProtocolGate.Validate(metadata, input.CoreAssemblyIdentity, name => resolver.Resolve(name));
        var binder = new ConditionalDefinitionBinder(name => resolver.Resolve(name), resolver.Location, input, framework.Bindings);
        var groups = manifests.Select(binder.Bind).ToArray();
        var application = resolver.Resolve(AssemblyNameReference.Parse(applicationIdentity));
        if (application.EntryPoint is null)
        {
            throw new InvalidDataException("The bridge requires the declared executable entry assembly.");
        }
        var core = resolver.Resolve(AssemblyNameReference.Parse(input.CoreAssemblyIdentity));
        var coreLibIdentity = input.Assemblies.Single(assembly => AssemblyNameReference.Parse(assembly.AssemblyIdentity).Name == "System.Private.CoreLib").AssemblyIdentity;
        var coreLib = resolver.Resolve(AssemblyNameReference.Parse(coreLibIdentity));
        new ConditionalTypeMapBridge(application, core, coreLib).Apply(groups);
        binder.RemoveConsumedRecords(manifests);
        FrameworkBindingPolicy.NormalizeReferences(owners, framework.Bindings);

        var changed = groups.Length == 0 ? [] : owners.Append(application).Distinct().ToArray();
        var outputNames = changed.Select(assembly => Path.GetFileName(resolver.Location(assembly))).ToArray();
        if (outputNames.Distinct(StringComparer.Ordinal).Count() != changed.Length)
        {
            throw new InvalidDataException("Bridged implementations have conflicting output file names.");
        }
        var staging = outputDirectory + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var transformed = new List<object>();
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            Directory.CreateDirectory(staging);
            foreach (var assembly in changed)
            {
                var original = input.Assemblies.Single(value => value.AssemblyIdentity == assembly.Name.FullName);
                var fileName = Path.GetFileName(original.Path);
                var stagedPath = Path.Combine(staging, fileName);
                var outputPath = Path.Combine(outputDirectory, fileName);
                assembly.Write(stagedPath, new WriterParameters
                {
                    WriteSymbols = assembly.MainModule.HasSymbols,
                    SymbolWriterProvider = assembly.MainModule.HasSymbols ? new PortablePdbWriterProvider() : null
                });
                replacements.Add(original.AssemblyIdentity, outputPath);
                var symbolPath = Path.ChangeExtension(stagedPath, ".pdb");
                transformed.Add(new
                {
                    original.AssemblyIdentity,
                    originalPath = original.Path,
                    originalHash = original.Sha256,
                    outputPath,
                    transformedHash = Hash(stagedPath),
                    originalSymbolsHash = resolver.SymbolHash(assembly),
                    transformedSymbolsHash = File.Exists(symbolPath) ? Hash(symbolPath) : null
                });
            }
            var report = new
            {
                format = 1,
                stage = groups.Length == 0 ? "unchanged-input" : "transformed-input",
                originalInputHash = input.ContentHash,
                inputFormat = input.Format,
                additionalInputs = input.AdditionalInputs,
                applicationIdentity,
                frameworkBindings = framework.Evidence,
                transformed,
                inputs = input.Assemblies.Select(assembly => new
                {
                    assembly.AssemblyIdentity,
                    path = replacements.TryGetValue(assembly.AssemblyIdentity, out var path) ? path : assembly.Path
                }).ToArray()
            };
            File.WriteAllText(Path.Combine(staging, "transformation.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            Directory.Move(staging, outputDirectory);
            return Path.Combine(outputDirectory, "transformation.json");
        }
        catch
        {
            if (Directory.Exists(staging))
            {
                Directory.Delete(staging, recursive: true);
            }
            throw;
        }
    }

    private static string Hash(string path) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));

    private sealed class SnapshotResolver : IAssemblyResolver
    {
        private readonly Dictionary<string, ConditionalInputAssembly> _inputs;
        private readonly Dictionary<string, AssemblyDefinition> _loaded = new(StringComparer.Ordinal);
        private readonly Dictionary<AssemblyDefinition, string> _locations = new();
        private readonly Dictionary<AssemblyDefinition, string?> _symbolHashes = new();
        private readonly List<Stream> _streams = [];
        private IReadOnlyDictionary<string, string> _frameworkBindings = new Dictionary<string, string>();

        internal SnapshotResolver(ConditionalInputSnapshot input)
        {
            _inputs = input.Assemblies.ToDictionary(assembly => assembly.AssemblyIdentity, StringComparer.Ordinal);
        }

        internal void SetFrameworkBindings(IReadOnlyDictionary<string, string> bindings) => _frameworkBindings = bindings;
        internal string Location(AssemblyDefinition assembly) => _locations[assembly];
        internal string? SymbolHash(AssemblyDefinition assembly) => _symbolHashes[assembly];

        public AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, new ReaderParameters());

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            var identity = _frameworkBindings.TryGetValue(name.FullName, out var bound) ? bound : name.FullName;
            if (_loaded.TryGetValue(identity, out var loaded))
            {
                return loaded;
            }
            if (!_inputs.TryGetValue(identity, out var input))
            {
                throw new AssemblyResolutionException(name);
            }
            var bytes = File.ReadAllBytes(input.Path);
            if (Convert.ToHexStringLower(SHA256.HashData(bytes)) != input.Sha256)
            {
                throw new InvalidDataException("Implementation bytes changed during bridge preparation: " + input.Path);
            }
            var stream = new MemoryStream(bytes, writable: false);
            _streams.Add(stream);
            var reader = new ReaderParameters { AssemblyResolver = this };
            string? symbolHash = null;
            var symbolPath = Path.ChangeExtension(input.Path, ".pdb");
            if (File.Exists(symbolPath))
            {
                var symbols = File.ReadAllBytes(symbolPath);
                var symbolStream = new MemoryStream(symbols, writable: false);
                _streams.Add(symbolStream);
                symbolHash = Convert.ToHexStringLower(SHA256.HashData(symbols));
                reader.ReadSymbols = true;
                reader.SymbolReaderProvider = new PortablePdbReaderProvider();
                reader.SymbolStream = symbolStream;
            }
            var assembly = AssemblyDefinition.ReadAssembly(stream, reader);
            if (assembly.Name.FullName != identity)
            {
                assembly.Dispose();
                throw new InvalidDataException("Implementation identity differs from the bridge snapshot: " + input.Path);
            }
            _loaded.Add(identity, assembly);
            _locations.Add(assembly, input.Path);
            _symbolHashes.Add(assembly, symbolHash);
            return assembly;
        }

        public void Dispose()
        {
            foreach (var assembly in _loaded.Values)
            {
                assembly.Dispose();
            }
            foreach (var stream in _streams)
            {
                stream.Dispose();
            }
        }
    }
}
