namespace AtomUI.Generator;

internal sealed class ControlTokenDefaultStep
{
    internal ControlTokenDefaultStep(string name, int slot, string declaringType, string methodName,
        IReadOnlyList<string> ownInputs)
    {
        Name = name;
        Slot = slot;
        DeclaringType = declaringType;
        MethodName = methodName;
        OwnInputs = ownInputs;
    }

    internal string Name { get; }
    internal int Slot { get; }
    internal string DeclaringType { get; }
    internal string MethodName { get; }
    internal IReadOnlyList<string> OwnInputs { get; }
}

internal sealed class ControlTokenDefaultPlan
{
    internal ControlTokenDefaultPlan(ulong revision, IReadOnlyList<ControlTokenDefaultStep> steps)
    {
        Revision = revision;
        Steps = steps;
    }

    internal ulong Revision { get; }
    internal IReadOnlyList<ControlTokenDefaultStep> Steps { get; }
}
