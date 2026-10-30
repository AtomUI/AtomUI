using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.Registration.ILLink8;

internal sealed class ConditionalRegistrationMarkStep : MarkStep
{
    private readonly IReadOnlyList<ConditionalRegistrationGroup> _groups;
    private readonly Action<ConditionalRegistrationResult>? _onMaterialized;
    private readonly Dictionary<MethodDefinition, ConditionalRegistrationGroup> _collectors;
    private readonly Dictionary<TypeDefinition, List<ConditionBinding>> _conditions = new();
    private readonly Dictionary<TypeDefinition, RelevantTypeReason> _relevantTypes = new();
    private readonly HashSet<ConditionalRegistrationGroup> _requestedGroups = new();
    private readonly HashSet<ConditionalRegistrationFragment> _selectedFragments = new();
    private readonly List<ConditionalRegistrationObservation> _observations = new();

    public ConditionalRegistrationMarkStep(
        IReadOnlyList<ConditionalRegistrationGroup> groups,
        Action<ConditionalRegistrationResult>? onMaterialized)
    {
        _groups = groups;
        _onMaterialized = onMaterialized;
        _collectors = groups.ToDictionary(group => group.SelectedCollector);
        foreach (var group in groups)
        {
            foreach (var fragment in group.Fragments)
            {
                foreach (var condition in fragment.Conditions)
                {
                    if (!_conditions.TryGetValue(condition.TriggerType, out var bindings))
                    {
                        bindings = [];
                        _conditions.Add(condition.TriggerType, bindings);
                    }
                    bindings.Add(new ConditionBinding(group, fragment, condition));
                }
            }
        }
    }

    public override void Process(LinkContext context)
    {
        context.Pipeline.AppendMarkHandler(new InitialConditionsHandler(this));
        base.Process(context);

        foreach (var group in _groups)
        {
            if (!_requestedGroups.Contains(group))
            {
                continue;
            }
            var selected = SelectedFragments(group).ToArray();
            foreach (var fragment in selected)
            {
                if (!Annotations.IsMarked(fragment.RegistrationMethod) || !Annotations.IsProcessed(fragment.RegistrationMethod) ||
                    Annotations.GetAction(fragment.RegistrationMethod) != MethodAction.Parse)
                {
                    throw new InvalidOperationException($"Selected registration entry did not finish official marking: {fragment.Identity}.");
                }
            }
            ConditionalRegistrationBackend.RewriteCollector(group.SelectedCollector, selected.Select(fragment => fragment.RegistrationMethod).ToArray());
        }
        _onMaterialized?.Invoke(new ConditionalRegistrationResult(
            _groups.Select(group => new ConditionalRegistrationGroupResult(
                group.Identity, _requestedGroups.Contains(group), SelectedFragments(group).Select(fragment => fragment.Identity).ToArray())).ToArray(),
            _observations.ToArray()));
    }

    protected override TypeDefinition? MarkType(TypeReference reference, DependencyInfo reason, MessageOrigin? origin = null)
    {
        var result = base.MarkType(reference, reason, origin);
        // v8 GetOriginalType -> MarkGenericArguments sets variant relevance only AFTER its recursive
        // MarkType(argument) returns. Observe the original generic reference after that operation,
        // including repeat references to a TypeDefinition whose one-shot type callback already ran.
        for (var current = reference; current is TypeSpecification specification; current = specification.ElementType)
        {
            if (current is GenericInstanceType generic)
            {
                ObserveGenericArguments(generic);
            }
            if (current is FunctionPointerType)
            {
                break;
            }
        }
        return result;
    }

    protected override MethodDefinition? MarkMethod(MethodReference reference, DependencyInfo reason, in MessageOrigin origin)
    {
        var result = base.MarkMethod(reference, reason, origin);
        // v8 GetOriginalMethod has the other private MarkGenericArguments call site.
        if (reference is GenericInstanceMethod generic)
        {
            ObserveGenericArguments(generic);
        }
        // Multidimensional arrays use an array .ctor instead of the newarr instruction callback.
        if (reference.Name == ".ctor" && reference.DeclaringType is ArrayType array && Context.TryResolve(array) is { } element &&
            Annotations.IsRelevantToVariantCasting(element))
        {
            ObserveRelevantType(element, RelevantTypeReason.ArrayElement);
        }
        return result;
    }

    protected override TypeDefinition? MarkTypeVisibleToReflection(
        TypeReference type, TypeDefinition definition, in DependencyInfo reason, in MessageOrigin origin)
    {
        var result = base.MarkTypeVisibleToReflection(type, definition, reason, origin);
        ObserveRelevantType(definition, RelevantTypeReason.Reflection);
        return result;
    }

    protected override void MarkRequirementsForInstantiatedTypes(TypeDefinition type)
    {
        var alreadyInstantiated = Annotations.IsInstantiated(type);
        base.MarkRequirementsForInstantiatedTypes(type);
        if (!alreadyInstantiated)
        {
            ObserveRelevantType(type, RelevantTypeReason.Instantiation);
        }
    }

    protected override void MarkInstruction(Instruction instruction, MethodDefinition method, ref bool requiresReflectionMethodBodyScanner)
    {
        base.MarkInstruction(instruction, method, ref requiresReflectionMethodBodyScanner);
        if (instruction.Operand is not TypeReference operand)
        {
            return;
        }
        if (instruction.OpCode.Code == Code.Newarr && Context.TryResolve(operand) is { } element)
        {
            ObserveRelevantType(element, RelevantTypeReason.ArrayElement);
        }
        if (instruction.OpCode.Code == Code.Isinst && operand is not TypeSpecification && operand is not GenericParameter &&
            Context.CanApplyOptimization(CodeOptimizations.UnusedTypeChecks, method.DeclaringType.Module.Assembly) &&
            Context.TryResolve(operand) is { IsInterface: false } checkedType)
        {
            // The official marker may defer and eventually remove this isinst. Its occurrence still
            // participates in external TypeMap selection; waiting for IsMarked would miss this case.
            ObserveRelevantType(checkedType, RelevantTypeReason.TypeCheck);
        }
    }

    private void ObserveGenericArguments(IGenericInstance instance)
    {
        foreach (var argument in instance.GenericArguments)
        {
            if (Context.TryResolve(argument) is { } definition && Annotations.IsRelevantToVariantCasting(definition))
            {
                ObserveRelevantType(definition, RelevantTypeReason.GenericArgument);
            }
        }
    }

    private void ObserveGroup(MethodDefinition method)
    {
        if (!_collectors.TryGetValue(method, out var group) || !_requestedGroups.Add(group))
        {
            return;
        }
        _observations.Add(new ConditionalRegistrationObservation(_observations.Count, group.Identity, null, null, null));
        foreach (var fragment in group.Fragments)
        {
            foreach (var condition in fragment.Conditions)
            {
                if (_relevantTypes.TryGetValue(condition.TriggerType, out var reason))
                {
                    Select(new ConditionBinding(group, fragment, condition), reason);
                }
            }
        }
    }

    private void ObserveRelevantType(TypeDefinition type, RelevantTypeReason reason)
    {
        if (!_conditions.TryGetValue(type, out var bindings) || !_relevantTypes.TryAdd(type, reason))
        {
            return;
        }
        foreach (var binding in bindings)
        {
            _observations.Add(new ConditionalRegistrationObservation(_observations.Count, binding.Group.Identity, binding.Condition.Identity, null, reason));
            if (_requestedGroups.Contains(binding.Group))
            {
                Select(binding, reason);
            }
        }
    }

    private void Select(ConditionBinding binding, RelevantTypeReason reason)
    {
        if (!_selectedFragments.Add(binding.Fragment))
        {
            return;
        }
        _observations.Add(new ConditionalRegistrationObservation(
            _observations.Count, binding.Group.Identity, binding.Condition.Identity, binding.Fragment.Identity, reason));
        using var scope = ScopeStack.PushScope(new MessageOrigin(binding.Condition.TriggerType));
        MarkMethod(binding.Fragment.RegistrationMethod,
            new DependencyInfo(DependencyKind.Custom, binding.Condition.TriggerType), new MessageOrigin(binding.Condition.TriggerType));
    }

    private IEnumerable<ConditionalRegistrationFragment> SelectedFragments(ConditionalRegistrationGroup group) =>
        group.Fragments.Where(_selectedFragments.Contains).OrderBy(fragment => fragment.CollectionOrderKey, StringComparer.Ordinal);

    private sealed record ConditionBinding(
        ConditionalRegistrationGroup Group, ConditionalRegistrationFragment Fragment, ConditionalRegistrationCondition Condition);

    private sealed class InitialConditionsHandler(ConditionalRegistrationMarkStep owner) : IMarkHandler
    {
        public void Initialize(LinkContext context, MarkContext markContext)
        {
            // IMarkHandler implementations may append more initialization handlers. Keep the snapshot
            // after them, matching TypeMap10 initialization before ProcessMarkedPending, without polling
            // the Mark loop or confusing later metadata-only reachability with an initial explicit root.
            if (context.Pipeline.MarkHandlers.Count > 1)
            {
                context.Pipeline.AppendMarkHandler(new InitialConditionsHandler(owner));
                return;
            }
            markContext.RegisterMarkMethodAction(owner.ObserveGroup);
            foreach (var type in owner._conditions.Keys)
            {
                if (context.Annotations.IsMarked(type))
                {
                    owner.ObserveRelevantType(type, RelevantTypeReason.InitialRoot);
                }
            }
        }
    }
}
