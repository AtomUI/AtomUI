using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using AtomUI.Registration.Shared;
using Mono.Cecil;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

/// <summary>Pre-Mark compatibility bridge. Official ILLink10 still owns all conditional selection.</summary>
public sealed class InitializeStep : IStep
{
    public void Process(LinkContext context)
    {
        ToolchainContract.ValidateIdentity(typeof(LinkContext).Assembly.FullName!, ToolchainContract.ActualLinkerVersion);
        if (!context.Pipeline.GetSteps().Any(step => step is MarkStep))
        {
            throw new InvalidDataException("The conditional TypeMap bridge must run before official marking.");
        }
        if (!context.TryGetCustomData("AtomUIConditionalInputs", out var inputPath) ||
            !context.TryGetCustomData("AtomUIConditionalRuntimePack", out var runtimePack))
        {
            throw new InvalidDataException("The conditional bridge requires an input snapshot and SDK-resolved runtime pack.");
        }
        var input = ConditionalInputSnapshot.Read(inputPath);
        if (File.Exists(input.AnalysisReportPath))
        {
            File.Delete(input.AnalysisReportPath);
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
        ConditionalProtocolGate.Validate(metadata, input.CoreAssemblyIdentity, context.TryResolve);
        if (manifests.Length == 0)
        {
            throw new InvalidDataException("The conditional bridge requires at least one unconverted net8 package.");
        }
        var owners = manifests.Select(manifest => context.TryResolve(AssemblyNameReference.Parse(manifest.Metadata.AssemblyIdentity)) ??
            throw new InvalidDataException("Cannot resolve conditional package owner.")).ToArray();
        if (owners.Any(owner => context.Annotations.GetAction(owner) != AssemblyAction.Link))
        {
            throw new InvalidDataException("Bridged package implementations must use actual Link action.");
        }
        var bindings = FrameworkBindingPolicy.Create(runtimePack, input, owners);
        var binder = new ConditionalDefinitionBinder(context.TryResolve, context.GetAssemblyLocation, input, bindings.Bindings);
        var groups = manifests.Select(binder.Bind).ToArray();
        var core = context.TryResolve(AssemblyNameReference.Parse(input.CoreAssemblyIdentity)) ?? throw new InvalidDataException("Missing Core implementation.");
        var coreLib = context.TryResolve("System.Private.CoreLib") ?? throw new InvalidDataException("Missing target CoreLib.");
        var application = context.Annotations.GetEntryPointAssembly() ?? throw new InvalidDataException("Missing application entry assembly.");
        new ConditionalTypeMapBridge(application, core, coreLib).Apply(groups);
        binder.RemoveConsumedRecords(manifests);
        FrameworkBindingPolicy.NormalizeReferences(owners, bindings.Bindings);

        // This only certifies the transformation handed to official Mark. It is not a selection,
        // output or consumed-publish receipt. Final artifact verification has a separate owner.
        var report = new
        {
            format = 1,
            stage = "bridged-input",
            inputHash = input.ContentHash,
            inputFormat = input.Format,
            additionalInputs = input.AdditionalInputs,
            backend = ToolchainContract.ActualLinkerVersion,
            frameworkBindings = bindings.Evidence,
            packages = groups.Select(group => new
            {
                group.Manifest.Metadata.AssemblyIdentity,
                originalHash = group.Manifest.Metadata.ContentHash,
                group.Identity,
                candidates = group.Fragments.Sum(fragment => fragment.Conditions.Count)
            }).ToArray()
        };
        Directory.CreateDirectory(Path.GetDirectoryName(input.AnalysisReportPath)!);
        File.WriteAllText(input.AnalysisReportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }
}
