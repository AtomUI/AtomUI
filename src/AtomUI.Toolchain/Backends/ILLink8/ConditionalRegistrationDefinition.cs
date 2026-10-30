using Mono.Cecil;

namespace AtomUI.Registration.ILLink8;

/// <summary>Resolved definitions from the metadata/ABI validator, using this LinkContext's Cecil objects.</summary>
public sealed record ConditionalRegistrationGroup(
    string Identity,
    MethodDefinition Collector,
    MethodDefinition FullCollector,
    MethodDefinition SelectedCollector,
    IReadOnlyList<ConditionalRegistrationFragment> Fragments);

public sealed record ConditionalRegistrationFragment(
    string Identity,
    string CollectionOrderKey,
    MethodDefinition RegistrationMethod,
    IReadOnlyList<ConditionalRegistrationCondition> Conditions);

public sealed record ConditionalRegistrationCondition(string Identity, TypeDefinition TriggerType);

public enum RelevantTypeReason
{
    InitialRoot,
    Instantiation,
    Reflection,
    TypeCheck,
    ArrayElement,
    GenericArgument
}

public sealed record ConditionalRegistrationObservation(
    int Sequence,
    string GroupIdentity,
    string? ConditionIdentity,
    string? FragmentIdentity,
    RelevantTypeReason? Reason);

public sealed record ConditionalRegistrationGroupResult(
    string Identity,
    bool Requested,
    IReadOnlyList<string> SelectedFragments);

/// <summary>Completed Mark/materialization evidence; this is not a final-output or publish receipt.</summary>
public sealed record ConditionalRegistrationResult(
    IReadOnlyList<ConditionalRegistrationGroupResult> Groups,
    IReadOnlyList<ConditionalRegistrationObservation> Observations);
