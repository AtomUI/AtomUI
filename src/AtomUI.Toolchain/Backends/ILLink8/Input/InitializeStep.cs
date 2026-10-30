using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using AtomUI.Registration.Shared;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.Registration.ILLink8.Input;

/// <summary>Validates SDK implementation inputs and generated registration ABI before ILLink8 Mark.</summary>
public sealed class InitializeStep : IStep
{
    public void Process(LinkContext context)
    {
        if (!context.TryGetCustomData("AtomUIConditionalInputs", out var path) || string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidDataException("AtomUIConditionalInputs must identify the verified implementation input snapshot.");
        }
        var input = ConditionalInputSnapshot.Read(path);
        // A failed rerun must not leave a previous successful analysis report at this invocation's path.
        if (File.Exists(input.AnalysisReportPath))
        {
            File.Delete(input.AnalysisReportPath);
        }
        var metadata = input.Assemblies.Select(assembly =>
        {
            var value = ConditionalRegistrationMetadata.Read(assembly.Path, input.CoreAssemblyIdentity);
            if (value.AssemblyIdentity != assembly.AssemblyIdentity || value.ContentHash != assembly.Sha256)
            {
                throw new InvalidDataException($"Conditional implementation input identity/hash mismatch: '{assembly.Path}'.");
            }
            return value;
        }).ToArray();
        foreach (var package in metadata.Where(value => value.HasPackageMarker && value.Records.Count == 0))
        {
            throw new InvalidDataException($"Assembly '{package.AssemblyIdentity}' has a package marker but no conditional registration records; rebuild the package with the supported .NET 8 registration generator.");
        }
        var manifests = metadata.Where(value => value.Records.Count != 0).Select(ConditionalRegistrationManifest.Parse).ToArray();
        if (manifests.Length == 0)
        {
            throw new InvalidDataException("Conditional initialization requires at least one generated package definition.");
        }
        var binder = new ConditionalDefinitionBinder(context.TryResolve, context.GetAssemblyLocation, input);
        var groups = manifests.Select(binder.Bind).Select(group => new ConditionalRegistrationGroup(
            group.Identity, group.Collector, group.FullCollector, group.SelectedCollector,
            group.Fragments.Select(fragment => new ConditionalRegistrationFragment(
                fragment.Identity, fragment.CollectionOrderKey, fragment.RegistrationMethod,
                fragment.Conditions.Select(condition => new ConditionalRegistrationCondition(condition.Identity, condition.TriggerType)).ToArray())).ToArray())).ToArray();
        binder.RemoveConsumedRecords(manifests);
        var core = context.TryResolve(Mono.Cecil.AssemblyNameReference.Parse(input.CoreAssemblyIdentity)) ??
            throw new InvalidDataException("The validated Core implementation disappeared before mode normalization.");
        CoreRegistrationModeNormalizer.Normalize(core, context.TryResolve, context.TryResolve);
        ConditionalRegistrationBackend.Install(context, groups, result =>
        {
            // This proves only Mark and materialization. Sweep/output and SDK consumption have not
            // completed at this callback and must be verified by the publish owner independently.
            var report = new
            {
                format = 1,
                stage = "analyzed-materialized",
                inputHash = input.ContentHash,
                backend = ConditionalRegistrationBackend.LinkerProductVersion,
                runtimeModeNormalization = CoreRegistrationModeNormalizer.TemplateId,
                result
            };
            Directory.CreateDirectory(Path.GetDirectoryName(input.AnalysisReportPath)!);
            var temporary = input.AnalysisReportPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temporary, input.AnalysisReportPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        });
    }
}
