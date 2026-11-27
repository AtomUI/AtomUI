using Mono.Cecil;

namespace AtomUI.Registration.ILLink8;

/// <summary>
/// Resolved definitions from the metadata/ABI validator, using this LinkContext's Cecil objects.
/// </summary>
public sealed record ConditionalRegistrationGroup(
    string Identity,
    MethodDefinition Collector,
    MethodDefinition FullCollector,
    MethodDefinition SelectedCollector,
    IReadOnlyList<ConditionalRegistrationFragment> Fragments);

/// <summary>
/// One condition-guarded part of a <see cref="ConditionalRegistrationGroup" />, identified by <c>Identity</c>.
/// It is selected once the group is requested and any of its conditions is satisfied, which marks its
/// <c>RegistrationMethod</c>. The selected collector calls the selected fragments in ascending
/// <c>CollectionOrderKey</c> ordinal order.
/// </summary>
public sealed record ConditionalRegistrationFragment(
    string Identity,
    string CollectionOrderKey,
    MethodDefinition RegistrationMethod,
    IReadOnlyList<ConditionalRegistrationCondition> Conditions);

/// <summary>
/// One trigger of a <see cref="ConditionalRegistrationFragment" />.
/// The condition is satisfied when the mark step observes its <c>TriggerType</c> as relevant, which is what
/// selects the fragment that declares it.
/// </summary>
public sealed record ConditionalRegistrationCondition(string Identity, TypeDefinition TriggerType);

/// <summary>
/// Why the mark step observed a type as relevant, which satisfies the conditions that use the type as trigger.
/// Only the first reason observed for a type is recorded.
/// </summary>
public enum RelevantTypeReason
{
    /// <summary>
    /// The type was already marked when the mark step initialized.
    /// </summary>
    InitialRoot,
    /// <summary>
    /// The type became instantiated during marking.
    /// </summary>
    Instantiation,
    /// <summary>
    /// The type became visible to reflection.
    /// </summary>
    Reflection,
    /// <summary>
    /// The target of an <c>isinst</c> instruction in a marked method, even when the official marker later removes
    /// the check as unused.
    /// </summary>
    TypeCheck,
    /// <summary>
    /// The element type of an array whose construction was marked.
    /// </summary>
    ArrayElement,
    /// <summary>
    /// The type appeared as a generic argument of a marked generic instantiation.
    /// </summary>
    GenericArgument
}

/// <summary>
/// One entry in the evidence that the mark step records while it runs, ordered by <c>Sequence</c>.
/// An entry with no <c>ConditionIdentity</c> reports that the group became requested. An entry with a
/// <c>Reason</c> reports that the condition's trigger type became relevant, and names the
/// <c>FragmentIdentity</c> when that selected the fragment.
/// </summary>
public sealed record ConditionalRegistrationObservation(
    int Sequence,
    string GroupIdentity,
    string? ConditionIdentity,
    string? FragmentIdentity,
    RelevantTypeReason? Reason);

/// <summary>
/// The outcome of one <see cref="ConditionalRegistrationGroup" /> after marking.
/// <c>Requested</c> is <see langword="true" /> when the group's selected collector was marked.
/// <c>SelectedFragments</c> lists the identities of its selected fragments in <c>CollectionOrderKey</c>
/// ordinal order.
/// </summary>
public sealed record ConditionalRegistrationGroupResult(
    string Identity,
    bool Requested,
    IReadOnlyList<string> SelectedFragments);

/// <summary>
/// Completed Mark/materialization evidence; this is not a final-output or publish receipt.
/// </summary>
public sealed record ConditionalRegistrationResult(
    IReadOnlyList<ConditionalRegistrationGroupResult> Groups,
    IReadOnlyList<ConditionalRegistrationObservation> Observations);
