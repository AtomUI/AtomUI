using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using AtomUI.Build.Tasks.Isolation;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>Connects verified linked inputs to SDK R2R/bundle outputs without repeating dependency analysis.</summary>
public sealed class ConditionalPublishTransformTask : RegistrationBuildTask
{
    public string Operation { get; set; } = string.Empty;
    public string InputsPath { get; set; } = string.Empty;
    public string VerifiedReceiptPath { get; set; } = string.Empty;
    public string StatePath { get; set; } = string.Empty;
    public string PublishDirectory { get; set; } = string.Empty;
    public string BundlePath { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty;
    public ITaskItem[] Files { get; set; } = [];
    public ITaskItem[] References { get; set; } = [];
    public ITaskItem[] Tools { get; set; } = [];
    public ITaskItem[] ExtraOutputs { get; set; } = [];
    [Output] public ITaskItem[] PreparedTools { get; set; } = [];

    public override bool Execute()
    {
        try
        {
            var state = File.Exists(StatePath) ? JsonSerializer.Deserialize<TransformState>(File.ReadAllText(StatePath))! : InitialState();
            if (state.Format != 1 || state.InputHash != Hash(InputsPath) || state.VerifiedHash != Hash(VerifiedReceiptPath))
            {
                throw new InvalidDataException("Transform state does not match this verified link invocation.");
            }
            switch (Operation)
            {
                case "capture-r2r": CaptureR2R(state); break;
                case "verify-r2r": VerifyR2R(state); break;
                case "capture-bundle": CaptureBundle(state); break;
                case "verify-bundle": VerifyBundle(state); break;
                case "verify-published": VerifyPublished(state); break;
                default: throw new InvalidDataException("Unknown SDK publish transformation operation.");
            }
            WriteState(state);
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or BadImageFormatException or JsonException)
        {
            if (File.Exists(StatePath))
            {
                File.Delete(StatePath);
            }
            return Fail("ATOMUIREG007", error);
        }
    }

    private TransformState InitialState()
    {
        using var receipt = JsonDocument.Parse(File.ReadAllText(VerifiedReceiptPath));
        var root = receipt.RootElement;
        if (root.GetProperty("stage").GetString() != "verified" || root.GetProperty("inputHash").GetString() != Hash(InputsPath))
        {
            throw new InvalidDataException("SDK transformation requires this invocation's verified linked output.");
        }
        var files = root.GetProperty("outputs").EnumerateArray().Select(file => new TransformFile(
            file.GetProperty("path").GetString()!, file.GetProperty("relativePath").GetString()!, file.GetProperty("sha256").GetString()!)).ToArray();
        CheckFiles(files);
        return new TransformState { InputHash = Hash(InputsPath), VerifiedHash = Hash(VerifiedReceiptPath), Current = files };
    }

    private void CaptureR2R(TransformState state)
    {
        RequireStage(state, "linked");
        CheckFiles(state.Current);
        var byPath = state.Current.ToDictionary(file => Path.GetFullPath(file.Path), StringComparer.Ordinal);
        var compilations = Files.Select(item =>
        {
            var path = Path.GetFullPath(item.ItemSpec);
            if (!byPath.TryGetValue(path, out var file) || string.IsNullOrWhiteSpace(item.GetMetadata("OutputR2RImage")))
            {
                throw new InvalidDataException("R2R compilation input is outside the verified linked set or has no SDK output mapping: " + path);
            }
            return new R2REntry(file, Path.GetFullPath(item.GetMetadata("OutputR2RImage")), ManagedContent(path));
        }).ToArray();
        foreach (var reference in References)
        {
            var path = Path.GetFullPath(reference.ItemSpec);
            if (!byPath.TryGetValue(path, out var file) || Hash(path) != file.Hash)
            {
                throw new InvalidDataException("R2R reference is outside the verified implementation closure: " + path);
            }
        }
        var outputs = ExtraOutputs.Select(item => (Path: Path.GetFullPath(item.ItemSpec), Relative: Relative(item.GetMetadata("RelativePath")))).ToArray();
        // Composite R2R emits one native owner plus updated component assemblies. The SDK's
        // output list identifies the components; compare each one's original managed content.
        state.R2R = outputs.Length == 0 ? compilations : state.Current
            .Where(file => outputs.Any(output => output.Relative == file.RelativePath))
            .Select(file => new R2REntry(file, outputs.Single(output => output.Relative == file.RelativePath).Path, ManagedContent(file.Path))).ToArray();
        state.GeneratedOutputs = outputs.Where(output => !state.Current.Any(file => file.RelativePath == output.Relative))
            .Select(output => new TransformFile(output.Path, output.Relative, string.Empty)).ToArray();
        state.Options = Options;
        state.ToolFiles = FreezeTools();
        state.Stage = "r2r-inputs";
    }

    private void VerifyR2R(TransformState state)
    {
        RequireStage(state, "r2r-inputs");
        CheckFiles(state.Current);
        CheckFiles(state.ToolFiles);
        var replacements = new Dictionary<string, TransformFile>(StringComparer.Ordinal);
        foreach (var entry in state.R2R)
        {
            using var pe = new PEReader(File.OpenRead(entry.Output));
            if (pe.PEHeaders.CorHeader?.ManagedNativeHeaderDirectory.Size is not > 0 || ManagedContent(entry.Output) != entry.ManagedHash)
            {
                throw new InvalidDataException("ReadyToRun output must retain the verified managed metadata, IL and resources: " + entry.Output);
            }
            replacements.Add(entry.Input.Path, new(entry.Output, entry.Input.RelativePath, Hash(entry.Output)));
        }
        var generated = state.GeneratedOutputs.Select(file =>
        {
            if (file.RelativePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                using var pe = new PEReader(File.OpenRead(file.Path));
                if (pe.PEHeaders.CorHeader?.ManagedNativeHeaderDirectory.Size is not > 0)
                {
                    throw new InvalidDataException("The SDK composite owner is not a ReadyToRun image: " + file.Path);
                }
            }
            return file with { Hash = Hash(file.Path) };
        }).ToArray();
        state.Current = state.Current.Select(file => replacements.GetValueOrDefault(file.Path) ?? file).Concat(generated).ToArray();
        state.GeneratedOutputs = generated;
        state.Stage = "r2r-verified";
    }

    private void CaptureBundle(TransformState state)
    {
        if (state.Stage is not ("linked" or "r2r-verified"))
        {
            throw new InvalidDataException("Bundle input must follow verified linking or verified ReadyToRun conversion.");
        }
        CheckFiles(state.Current);
        var files = Files.Select(item => new TransformFile(Path.GetFullPath(item.ItemSpec),
            Relative(item.GetMetadata("RelativePath")), Hash(item.ItemSpec))).ToArray();
        foreach (var expected in state.Current)
        {
            var matches = files.Where(file => file.RelativePath == expected.RelativePath).ToArray();
            // ExcludeFromSingleFile is a legal SDK option. Such files are verified in PublishDirectory.
            if (matches.Length > 1 || matches.Length == 1 && matches[0].Hash != expected.Hash)
            {
                throw new InvalidDataException("Bundle input differs from verified managed output: " + expected.RelativePath);
            }
        }
        if (files.Select(file => file.RelativePath).Distinct(StringComparer.Ordinal).Count() != files.Length)
        {
            throw new InvalidDataException("Ambiguous bundle-relative input paths.");
        }
        state.BundleInputs = files;
        state.BundlePath = Path.GetFullPath(BundlePath);
        state.Options += "|bundle:" + Options;
        state.ToolFiles = state.ToolFiles.Concat(Tools.Select(item => new TransformFile(Path.GetFullPath(item.ItemSpec), Path.GetFileName(item.ItemSpec), Hash(item.ItemSpec)))).ToArray();
        state.Stage = "bundle-inputs";
    }

    private void VerifyBundle(TransformState state)
    {
        RequireStage(state, "bundle-inputs");
        CheckFiles(state.BundleInputs);
        CheckFiles(state.ToolFiles);
        var embedded = ConditionalBundleReader.ReadHashes(state.BundlePath);
        foreach (var entry in embedded)
        {
            var original = state.BundleInputs.SingleOrDefault(file => file.RelativePath == entry.Key);
            if (original is null || original.Hash != entry.Value)
            {
                throw new InvalidDataException("Bundle payload is not the declared SDK input: " + entry.Key);
            }
        }
        state.Embedded = embedded;
        state.BundleHash = Hash(state.BundlePath);
        state.Stage = "bundle-verified";
    }

    private void VerifyPublished(TransformState state)
    {
        if (state.Stage is not ("r2r-verified" or "bundle-verified" or "consumed"))
        {
            throw new InvalidDataException("Only verified SDK transformations can be marked consumed.");
        }
        CheckFiles(state.ToolFiles);
        if (state.BundlePath.Length != 0 && (Path.GetFullPath(state.BundlePath) != Path.GetFullPath(Path.Combine(PublishDirectory, Path.GetFileName(state.BundlePath))) ||
                Hash(state.BundlePath) != state.BundleHash))
        {
            throw new InvalidDataException("The published bundle changed after payload verification.");
        }
        foreach (var file in state.Current.Concat(state.BundleInputs).DistinctBy(file => file.RelativePath))
        {
            if (state.Embedded.TryGetValue(file.RelativePath, out var embedded))
            {
                if (embedded != file.Hash)
                {
                    throw new InvalidDataException("Embedded managed output differs from the verified transform chain.");
                }
                continue;
            }
            // The apphost is transformed into the bundle itself; its source hash cannot equal the final executable.
            if (state.BundlePath.Length != 0 && file.RelativePath == Path.GetFileName(state.BundlePath))
            {
                continue;
            }
            var published = Path.Combine(Path.GetFullPath(PublishDirectory), Relative(file.RelativePath));
            if (!File.Exists(published) || Hash(published) != file.Hash)
            {
                throw new InvalidDataException("SDK-published transform output differs from the verified file: " + file.RelativePath);
            }
        }
        state.Stage = "consumed";
    }

    private TransformFile[] FreezeTools()
    {
        var results = new List<TransformFile>();
        var prepared = new List<ITaskItem>();
        var domain = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(StatePath))!, "r2r-tools");
        foreach (var tool in Tools)
        {
            if (tool.GetMetadata("Kind") == "profile")
            {
                var profile = Path.Combine(domain, "profiles", Hash(tool.ItemSpec), Path.GetFileName(tool.ItemSpec));
                Directory.CreateDirectory(Path.GetDirectoryName(profile)!);
                File.Copy(tool.ItemSpec, profile, overwrite: true);
                results.Add(new(profile, Path.GetFileName(profile), Hash(profile)));
                var profileItem = TaskWireItem.FromItem(tool);
                profileItem.ItemSpec = profile;
                profileItem.FullPath = profile;
                prepared.Add(profileItem);
                continue;
            }
            var sourceRoot = Path.GetDirectoryName(Path.GetFullPath(tool.ItemSpec))!;
            var destination = Path.Combine(domain, Hash(tool.ItemSpec));
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(sourceRoot, file);
                var target = Path.Combine(destination, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(target, File.GetUnixFileMode(file));
                }
                results.Add(new(target, relative, Hash(target)));
            }
            var item = TaskWireItem.FromItem(tool);
            foreach (var key in item.Metadata.Keys.ToArray())
            {
                var value = item.Metadata[key];
                if (Path.IsPathFullyQualified(value) && value.StartsWith(sourceRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                {
                    item.SetMetadata(key, Path.Combine(destination, Path.GetRelativePath(sourceRoot, value)));
                }
            }
            item.ItemSpec = Path.Combine(destination, Path.GetFileName(tool.ItemSpec));
            item.FullPath = item.ItemSpec;
            prepared.Add(item);
        }
        PreparedTools = prepared.ToArray();
        return results.ToArray();
    }

    internal static string ManagedContent(string path)
    {
        using var pe = new PEReader(File.OpenRead(path));
        var metadata = pe.GetMetadataReader();
        var bytes = pe.GetMetadata().GetContent().ToArray();
        // R2R relocates IL and RVA-backed fields. All other metadata must remain byte-identical.
        foreach (var table in new[] { TableIndex.MethodDef, TableIndex.FieldRva })
        {
            var offset = metadata.GetTableMetadataOffset(table);
            var rowSize = metadata.GetTableRowSize(table);
            for (var row = 0; row < metadata.GetTableRowCount(table); row++)
            {
                Array.Clear(bytes, offset + row * rowSize, 4);
            }
        }
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(bytes);
        foreach (var handle in metadata.MethodDefinitions)
        {
            var method = metadata.GetMethodDefinition(handle);
            if (method.RelativeVirtualAddress != 0)
            {
                var body = pe.GetMethodBody(method.RelativeVirtualAddress);
                hash.AppendData(pe.GetSectionData(method.RelativeVirtualAddress).GetContent(0, body.Size).AsSpan());
            }
        }
        var resources = pe.PEHeaders.CorHeader!.ResourcesDirectory;
        if (resources.Size != 0)
        {
            hash.AppendData(pe.GetSectionData(resources.RelativeVirtualAddress).GetContent(0, resources.Size).AsSpan());
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static string Relative(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathFullyQualified(path) || path.Split('/', '\\').Any(part => part == ".."))
        {
            throw new InvalidDataException("Invalid SDK output relative path.");
        }
        return path.Replace('\\', '/');
    }
    private static void RequireStage(TransformState state, string expected)
    {
        if (state.Stage != expected)
        {
            throw new InvalidDataException("Unexpected transformation stage: " + state.Stage);
        }
    }
    private static void CheckFiles(IEnumerable<TransformFile> files)
    {
        foreach (var file in files)
        {
            if (!File.Exists(file.Path) || Hash(file.Path) != file.Hash)
            {
                throw new InvalidDataException("A verified SDK transformation input changed: " + file.Path);
            }
        }
    }
    private void WriteState(TransformState state) => RegistrationFiles.ReplaceText(StatePath, JsonSerializer.Serialize(state));
    internal sealed record TransformFile(string Path, string RelativePath, string Hash);
    internal sealed record R2REntry(TransformFile Input, string Output, string ManagedHash);
    internal sealed class TransformState
    {
        public int Format { get; set; } = 1;
        public string Stage { get; set; } = "linked";
        public string InputHash { get; set; } = string.Empty;
        public string VerifiedHash { get; set; } = string.Empty;
        public string Options { get; set; } = string.Empty;
        public TransformFile[] Current { get; set; } = [];
        public R2REntry[] R2R { get; set; } = [];
        public TransformFile[] GeneratedOutputs { get; set; } = [];
        public TransformFile[] ToolFiles { get; set; } = [];
        public TransformFile[] BundleInputs { get; set; } = [];
        public Dictionary<string, string> Embedded { get; set; } = new(StringComparer.Ordinal);
        public string BundlePath { get; set; } = string.Empty;
        public string BundleHash { get; set; } = string.Empty;
    }
}
