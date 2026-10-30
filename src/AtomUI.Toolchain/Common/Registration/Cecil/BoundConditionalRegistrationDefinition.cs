using AtomUI.Build.Tasks.Registration;
using Mono.Cecil;

namespace AtomUI.Registration.Shared;

// Source-only validated input entities. These records do not contain linker marking state.
internal sealed record BoundConditionalRegistrationGroup(
    string Identity,
    ConditionalRegistrationManifest Manifest,
    TypeDefinition GroupType,
    MethodDefinition Collector,
    MethodDefinition FullCollector,
    MethodDefinition SelectedCollector,
    IReadOnlyList<BoundConditionalRegistrationFragment> Fragments);

internal sealed record BoundConditionalRegistrationFragment(
    string Identity,
    string CollectionOrderKey,
    TypeDefinition ProxyType,
    MethodDefinition RegistrationMethod,
    IReadOnlyList<BoundConditionalRegistrationCondition> Conditions);

internal sealed record BoundConditionalRegistrationCondition(string Identity, TypeDefinition TriggerType);
