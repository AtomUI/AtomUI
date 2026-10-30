using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

internal sealed record TypeMapLinkOption(string Name, string Value);
internal sealed record TypeMapLinkItem(string Kind, string Identity, IReadOnlyDictionary<string, string> Metadata);
internal sealed record TypeMapLinkConfiguration(TypeMapLinkOption[] Options, TypeMapLinkItem[] Items)
{
    // These are the metadata consumed by the supported ILLink.Tasks AssemblyPaths boundary.
    internal static readonly string[] AssemblyMetadata = ["TrimMode", "IsTrimmable", "BeforeFieldInit", "OverrideRemoval",
        "UnreachableBodies", "UnusedInterfaces", "IPConstProp", "Sealer", "TrimmerSingleWarn"];

    internal static TypeMapLinkConfiguration Capture(ITaskItem[] options, ITaskItem[] items,
                                                     ITaskItem[] assemblies, ITaskItem[] references)
    {
        var result = items.Select(i => new TypeMapLinkItem(i.GetMetadata("Kind"), i.ItemSpec, Metadata(i, i.GetMetadata("Kind") switch
        {
            "RootAssemblyNames" => ["RootMode"],
            "FeatureSettings" or "CustomData" => ["Value"],
            "CustomSteps" => ["Type", "BeforeStep", "AfterStep"],
            "RootDescriptorFiles" or "KeepMetadata" => [],
            var kind => throw new InvalidDataException($"Unknown ILLink input collection '{kind}'.")
        }))).ToList();
        // Preserve effective array order (including first-wins assembly resolution and ordered custom steps).
        result.AddRange(assemblies.Select(i => new TypeMapLinkItem("AssemblyPaths", Path.GetFullPath(i.ItemSpec), Metadata(i, AssemblyMetadata))));
        result.AddRange(references.Select(i => new TypeMapLinkItem("ReferenceAssemblyPaths", Path.GetFullPath(i.ItemSpec), Metadata(i, []))));
        var scalarOptions = options.Select(i => new TypeMapLinkOption(i.ItemSpec, i.ItemSpec == "NoWarn" ? NormalizeNoWarn(i.GetMetadata("Value")) : i.GetMetadata("Value"))).OrderBy(o => o.Name, StringComparer.Ordinal).ToArray();
        if (scalarOptions.Select(o => o.Name).Distinct(StringComparer.Ordinal).Count() != scalarOptions.Length)
        {
            throw new InvalidDataException("Duplicate effective ILLink option names.");
        }

        return new(scalarOptions, result.ToArray());
    }

    private static string NormalizeNoWarn(string value)
    {
        // ILLink 10.0.8 Driver.ProcessWarningCodes + NoWarn.UnionWith: non-IL compiler diagnostics
        // are ignored. Csc appends 1701/1702/8002 in the outer WASM build, but not its nested publish.
        if (value.Length > 1 && value[0] == '"' && value[^1] == '"')
        {
            value = value[1..^1];
        }

        var codes = new SortedSet<ushort>();
        foreach (var part in value.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries))
        {
            var id = part.Trim();
            if (id.StartsWith("IL", StringComparison.Ordinal) && ushort.TryParse(id.AsSpan(2), out var code))
            {
                codes.Add(code);
            }
        }
        return string.Join(";", codes.Select(code => "IL" + code.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    private static IReadOnlyDictionary<string, string> Metadata(ITaskItem item, string[] names) =>
        names.ToDictionary(n => n, item.GetMetadata, StringComparer.Ordinal);

    internal IEnumerable<string> InputFiles(IEnumerable<string>? tools = null)
    {
        var files = new HashSet<string>(StringComparer.Ordinal);
        void Add(string path) => files.Add(Path.GetFullPath(path));
        void Assembly(string path)
        {
            Add(path);
            var pdb = Path.ChangeExtension(path, ".pdb");
            if (File.Exists(pdb))
            {
                Add(pdb);
            }
        }
        void Tool(string path)
        {
            path = Path.GetFullPath(path);
            if (!files.Add(path))
            {
                return;
            }

            foreach (var suffix in new[] { ".deps.json", ".runtimeconfig.json" })
            {
                if (File.Exists(Path.ChangeExtension(path, suffix)))
                {
                    Add(Path.ChangeExtension(path, suffix));
                }
            }

            var deps = Path.ChangeExtension(path, ".deps.json");
            if (File.Exists(deps))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(deps));
                foreach (var target in document.RootElement.GetProperty("targets").EnumerateObject())
                {
                    foreach (var library in target.Value.EnumerateObject())
                    {
                        foreach (var kind in new[] { "runtime", "native", "runtimeTargets" })
                        {
                            if (library.Value.TryGetProperty(kind, out var assets))
                            {
                                foreach (var asset in assets.EnumerateObject())
                                {
                                    var dependency = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileName(asset.Name));
                                    if (File.Exists(dependency))
                                    {
                                        Tool(dependency);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Path.GetExtension(path) is not (".dll" or ".exe"))
            {
                return;
            }

            using var stream = File.OpenRead(path);
            using var pe     = new PEReader(stream);
            if (!pe.HasMetadata)
            {
                return;
            }

            var metadata = pe.GetMetadataReader();
            foreach (var reference in metadata.AssemblyReferences)
            {
                var name       = metadata.GetString(metadata.GetAssemblyReference(reference).Name);
                var dependency = Path.Combine(Path.GetDirectoryName(path)!, name + ".dll");
                if (File.Exists(dependency))
                {
                    Tool(dependency);
                }
            }
        }
        foreach (var tool in tools ?? [])
        {
            Tool(tool);
        }

        foreach (var item in Items)
        {
            switch (item.Kind)
            {
                case "AssemblyPaths": case "ReferenceAssemblyPaths": Assembly(item.Identity); break;
                case "RootAssemblyNames": if (File.Exists(item.Identity)) { Assembly(item.Identity); } break;
                case "RootDescriptorFiles": Add(item.Identity); break;
                case "CustomSteps": Tool(item.Identity); break;
            }
        }
        var args = SplitArguments(Options.SingleOrDefault(o => o.Name == "ExtraArgs")?.Value ?? "");
        for (var index = 0; index < args.Count; index++)
        {
            string Value() => ++index < args.Count ? args[index] : throw new InvalidDataException("Missing ILLink file option argument.");
            switch (args[index])
            {
                case "--substitutions": case "-x": case "/x": Add(Value()); break;
                case "--link-attributes":
                    var attributes = Value();
                    if (attributes.StartsWith('@'))
                    {
                        var list = attributes[1..]; Add(list);
                        // ILLink's @list format is one file per line, resolved from the linker working directory.
                        foreach (var file in File.ReadAllLines(list))
                        {
                            Add(file);
                        }
                    }
                    else
                    {
                        Add(attributes);
                    }

                    break;
                case "-reference": case "/reference": Assembly(Value()); break;
                case "-a": case "/a":
                    var root = Value(); if (File.Exists(root))
                    {
                        Assembly(root);
                    }

                    break;
                case "--custom-step":
                    var step = Value(); var comma = step.LastIndexOf(',');
                    if (comma < 0)
                    {
                        throw new InvalidDataException("ILLink custom-step must contain its assembly file path.");
                    }

                    Tool(step[(comma + 1)..]); break;
                case "-d": case "/d":
                    // Directory membership is part of resolution: additions/removals must change the signature too.
                    foreach (var file in Directory.EnumerateFiles(Value()).Where(p => Path.GetExtension(p) is ".dll" or ".exe" or ".winmd"))
                    {
                        Assembly(file);
                    }

                    break;
            }
        }
        // DotNetHostPath is the SDK/ToolTask's actual host. ToolExe/ToolPath overrides are recorded separately.
        var host = Options.SingleOrDefault(o => o.Name == "EffectiveToolPath")?.Value;
        if (!string.IsNullOrWhiteSpace(host))
        {
            Add(host);
        }

        return files.Order(StringComparer.Ordinal);
    }

    // Match the supported ILLink response-file quoting rules: whitespace, doubled quotes, and backslashes before quotes.
    // ExtraArgs is appended to ILLink's response file; it is not parsed by a shell.
    internal static IReadOnlyList<string> SplitArguments(string text)
    {
        var result = new List<string>();
        for (var index = 0; index < text.Length;)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            if (index == text.Length)
            {
                break;
            }

            var argument = new StringBuilder(); var quoted = false;
            while (index < text.Length)
            {
                var slashes = 0;
                while (index < text.Length && text[index] == '\\') { slashes++; index++; }
                if (index < text.Length && text[index] == '"')
                {
                    argument.Append('\\', slashes / 2);
                    if (slashes % 2 != 0) { argument.Append('"'); index++; }
                    else if (quoted && index + 1 < text.Length && text[index + 1] == '"') { argument.Append('"'); index += 2; }
                    else { quoted = !quoted; index++; }
                    continue;
                }
                argument.Append('\\', slashes);
                if (index == text.Length || (!quoted && char.IsWhiteSpace(text[index])))
                {
                    break;
                }

                argument.Append(text[index++]);
            }
            result.Add(argument.ToString());
        }
        return result;
    }
}
