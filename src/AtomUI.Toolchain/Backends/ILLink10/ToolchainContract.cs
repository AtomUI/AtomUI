using System.Reflection;
using AtomUI.Build.Tasks.Registration;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker;

internal static class ToolchainContract
{
    internal const string Capability = OfficialTypeMapToolchain.Capability;
    internal static string ActualLinkerVersion => typeof(LinkContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

    internal static void ValidateIdentity(string identity, string version)
    {
        if (!OfficialTypeMapToolchain.IsSupportedLinker(identity, version))
        {
            throw BackendDiagnostic.Unsupported($"Unsupported ILLink identity '{identity}' ({version}). Use official ILLink {OfficialTypeMapToolchain.MinimumVersion} or later with TypeMap support.");
        }
    }

    internal static void Validate(LinkContext context)
    {
        // Assembly metadata is public. No linker implementation fields, private handlers or reflection APIs are consulted.
        ValidateIdentity(typeof(LinkContext).Assembly.FullName!, ActualLinkerVersion);
        if (!context.TryGetCustomData("AtomUITypeMapBackend", out var value) || value != Capability)
        {
            throw BackendDiagnostic.Unsupported($"Missing or incompatible AtomUITypeMapBackend capability '{value}'. Expected '{Capability}'; rebuild the matching build assets and backend.");
        }

        var steps = context.Pipeline.GetSteps();
        if (steps.Any(s => s is MarkStep) || steps.Count(s => s is SweepStep) != 1 ||
            steps.Count(s => s.GetType().FullName == typeof(MaterializeTypeMapsStep).FullName) > 1 ||
            steps.Any(s => s is VerifyTypeMapsStep))
        {
            throw BackendDiagnostic.Unsupported("The TypeMap backend must run exactly once after Mark and before Sweep. Remove duplicate or incorrectly ordered custom-step injection.");
        }
    }
}
