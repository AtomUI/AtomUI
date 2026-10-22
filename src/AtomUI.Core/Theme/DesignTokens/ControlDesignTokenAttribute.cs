namespace AtomUI.Theme.DesignTokens;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ControlDesignTokenAttribute : Attribute
{
    private int _revision;

    public ControlDesignTokenAttribute()
        : this(ControlTokenEvaluationVersion.WholeEvaluatorV1)
    {
    }

    public ControlDesignTokenAttribute(ControlTokenEvaluationVersion evaluationVersion)
    {
        if (evaluationVersion is not (ControlTokenEvaluationVersion.WholeEvaluatorV1 or
            ControlTokenEvaluationVersion.DependencyDefaultsV2))
        {
            throw new ArgumentOutOfRangeException(nameof(evaluationVersion));
        }
        EvaluationVersion = evaluationVersion;
    }

    public ControlTokenEvaluationVersion EvaluationVersion { get; }

    public int Revision
    {
        get => _revision;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _revision = value;
        }
    }
}
