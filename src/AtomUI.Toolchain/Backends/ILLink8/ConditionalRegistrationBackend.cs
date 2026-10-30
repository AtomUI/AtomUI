using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.Registration.ILLink8;

/// <summary>Installs the pinned marking mechanism. Metadata parsing, source identity and receipts belong to the build adapter.</summary>
public static class ConditionalRegistrationBackend
{
    public const string RuntimeSourceCommit = "a6bde67c455f2ac219988c7a66171631090b6f65";
    public const string LinkerProductVersion = "8.0.27+" + RuntimeSourceCommit;

    /// <summary>
    /// Called after definition/collector-template validation and before Mark. The caller must isolate
    /// build-only declaration attributes before installing this mechanism. This method does not discover definitions.
    /// </summary>
    public static void Install(
        LinkContext context,
        IReadOnlyList<ConditionalRegistrationGroup> definitions,
        Action<ConditionalRegistrationResult>? onMaterialized = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(definitions);
        ValidateToolchain();
        var steps = context.Pipeline.GetSteps();
        if (steps.Count(step => step.GetType() == typeof(MarkStep)) != 1 ||
            steps.Any(step => step is ConditionalRegistrationMarkStep) ||
            Array.FindIndex(steps, step => step is SweepStep) < Array.FindIndex(steps, step => step.GetType() == typeof(MarkStep)))
        {
            throw new InvalidOperationException("Conditional registration must install once before the original ILLink8 MarkStep.");
        }

        // Snapshot collections so the loader cannot mutate the active graph after validation.
        var groups = definitions.Select(group => group with
        {
            Fragments = group.Fragments.Select(fragment => fragment with
            {
                Conditions = fragment.Conditions.ToArray()
            }).ToArray()
        }).ToArray();
        ValidateDefinitions(context, groups);
        foreach (var group in groups)
        {
            RewriteCollector(group.Collector, [group.SelectedCollector]);
            RewriteCollector(group.SelectedCollector, []);
        }
        context.Pipeline.ReplaceStep(typeof(MarkStep), new ConditionalRegistrationMarkStep(groups, onMaterialized));
    }

    private static void ValidateToolchain()
    {
        var actual = typeof(MarkStep).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (actual != LinkerProductVersion || Environment.Version.Major != 10)
        {
            throw new NotSupportedException($"Conditional registration requires ILLink {LinkerProductVersion} on a .NET10 tool host; found {actual} on {Environment.Version}.");
        }
    }

    private static void ValidateDefinitions(LinkContext context, IReadOnlyList<ConditionalRegistrationGroup> groups)
    {
        var identities = new HashSet<string>(StringComparer.Ordinal);
        var collectors = new HashSet<MethodDefinition>();
        foreach (var group in groups)
        {
            if (!collectors.Add(group.Collector) || !collectors.Add(group.FullCollector) || !collectors.Add(group.SelectedCollector))
            {
                throw new InvalidOperationException($"Conditional collector roles must be distinct: {group.Identity}.");
            }
        }
        foreach (var group in groups)
        {
            RequireIdentity(group.Identity);
            if (!identities.Add(group.Identity))
            {
                throw new InvalidOperationException($"Duplicate conditional group or collector: {group.Identity}.");
            }
            var owner = group.Collector.Module.Assembly;
            if (context.Annotations.GetAction(owner) != AssemblyAction.Link)
            {
                throw new InvalidOperationException($"Conditional registration requires Link action for {owner.Name.FullName}.");
            }
            ValidateEntry(group.Collector, owner, null);
            ValidateEntry(group.FullCollector, owner, group.Collector);
            ValidateEntry(group.SelectedCollector, owner, group.Collector);
            ValidateOriginalCollector(group);

            var fragmentIds = new HashSet<string>(StringComparer.Ordinal);
            var conditionIds = new HashSet<string>(StringComparer.Ordinal);
            var orders = new HashSet<string>(StringComparer.Ordinal);
            var thunks = new HashSet<MethodDefinition>();
            foreach (var fragment in group.Fragments)
            {
                RequireIdentity(fragment.Identity);
                RequireIdentity(fragment.CollectionOrderKey);
                if (!fragmentIds.Add(fragment.Identity) || !orders.Add(fragment.CollectionOrderKey) || !thunks.Add(fragment.RegistrationMethod))
                {
                    throw new InvalidOperationException($"Duplicate fragment identity, order or thunk in {group.Identity}.");
                }
                if (collectors.Contains(fragment.RegistrationMethod))
                {
                    throw new InvalidOperationException($"Registration thunk cannot alias a collector: {fragment.Identity}.");
                }
                ValidateEntry(fragment.RegistrationMethod, owner, group.Collector);
                if (fragment.Conditions.Count == 0)
                {
                    throw new InvalidOperationException($"Conditional fragment {fragment.Identity} has no condition.");
                }
                foreach (var condition in fragment.Conditions)
                {
                    RequireIdentity(condition.Identity);
                    if (!conditionIds.Add(condition.Identity))
                    {
                        throw new InvalidOperationException($"Duplicate condition {condition.Identity} in {group.Identity}.");
                    }
                    for (var type = condition.TriggerType; type is not null; type = type.DeclaringType)
                    {
                        if (type.HasGenericParameters)
                        {
                            throw new InvalidOperationException($"Open generic condition {condition.TriggerType.FullName} is outside the registration contract.");
                        }
                    }
                }
            }
        }
    }

    private static void ValidateEntry(MethodDefinition method, AssemblyDefinition owner, MethodDefinition? signature)
    {
        if (method.Module.Assembly != owner || !method.IsStatic || method.HasGenericParameters ||
            method.DeclaringType.ContainsGenericParameter || !method.HasBody || method.Parameters.Count != 1 ||
            method.ReturnType.MetadataType != MetadataType.Void || method.Parameters[0].ParameterType is TypeSpecification)
        {
            throw new InvalidOperationException($"Invalid conditional registration entry: {method.FullName}.");
        }
        if (signature is not null &&
            (method.Parameters[0].ParameterType.FullName != signature.Parameters[0].ParameterType.FullName ||
             method.Parameters[0].ParameterType.Resolve() != signature.Parameters[0].ParameterType.Resolve()))
        {
            throw new InvalidOperationException($"Conditional registration builder signature mismatch: {method.FullName}.");
        }
    }

    private static void ValidateOriginalCollector(ConditionalRegistrationGroup group)
    {
        var instructions = group.Collector.Body.Instructions.Where(instruction => instruction.OpCode.Code != Code.Nop).ToArray();
        if (group.Collector.Body.HasExceptionHandlers || instructions.Length != 3 ||
            instructions[0].OpCode.Code != Code.Ldarg_0 || instructions[1].OpCode.Code != Code.Call ||
            instructions[1].Operand is not MethodReference call || call.Resolve() != group.FullCollector ||
            instructions[2].OpCode.Code != Code.Ret)
        {
            throw new InvalidOperationException($"Unsupported full collector template: {group.Collector.FullName}.");
        }
        // The external ABI validator checks the exact stub template. Independently reject an already-neutral
        // or already-materialized method, so running this mechanism twice cannot turn stale output into success.
        if (group.SelectedCollector.Body.HasExceptionHandlers ||
            group.SelectedCollector.Body.Instructions.LastOrDefault()?.OpCode.Code != Code.Throw)
        {
            throw new InvalidOperationException($"Selected collector is not an unlowered failing stub: {group.SelectedCollector.FullName}.");
        }
    }

    private static void RequireIdentity(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
    }

    internal static void RewriteCollector(MethodDefinition method, IReadOnlyList<MethodDefinition> calls)
    {
        method.DebugInformation.SequencePoints.Clear();
        method.DebugInformation.Scope = null;
        method.DebugInformation.StateMachineKickOffMethod = null;
        method.DebugInformation.CustomDebugInformations.Clear();
        method.CustomDebugInformations.Clear();
        method.Body = new Mono.Cecil.Cil.MethodBody(method) { MaxStackSize = 1 };
        var il = method.Body.GetILProcessor();
        foreach (var target in calls)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, target);
        }
        il.Emit(OpCodes.Ret);
    }
}
