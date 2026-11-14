using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Registers the control token stage of one algorithm for one control: the pair
/// (<see cref="Algorithm"/>, <see cref="Control"/>) and an instance of the implementing class.
/// </summary>
/// <remarks>
/// Generated <c>GeneratedThemeAlgorithmRegistration.Register</c> creates one descriptor per
/// <see cref="ControlTokenAlgorithmAttribute"/> and passes them to
/// <see cref="IThemeManagerBuilder.AddControlTokenAlgorithms"/>. A descriptor whose algorithm or control is not
/// registered stays in the registry but is never called.
/// </remarks>
public sealed class ControlTokenAlgorithmDescriptor
{
    private readonly Evaluator _evaluator;

    private ControlTokenAlgorithmDescriptor(ThemeAlgorithm algorithm, ControlTokenIdentity control, int revision,
        Evaluator evaluator)
    {
        Algorithm  = algorithm;
        Control    = control;
        Revision   = revision;
        _evaluator = evaluator;
    }

    /// <summary>
    /// Creates the descriptor of <typeparamref name="TAlgorithm"/> as the control stage of
    /// <paramref name="algorithm"/> for the control that <typeparamref name="TToken"/> belongs to.
    /// The descriptor holds a single instance of <typeparamref name="TAlgorithm"/>.
    /// </summary>
    public static ControlTokenAlgorithmDescriptor Create<TToken, TAlgorithm>(ThemeAlgorithm algorithm, int revision)
        where TToken : AbstractControlDesignToken, IControlToken
        where TAlgorithm : class, IControlTokenAlgorithm<TToken>, new()
    {
        if (algorithm.IsUninitialized)
        {
            throw new ArgumentException("Theme algorithm identity is uninitialized.", nameof(algorithm));
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(revision);
        var control = TToken.ControlIdentity;
        if (control.Catalog is null)
        {
            throw new InvalidOperationException(
                $"Control token type '{typeof(TToken).FullName}' reports an uninitialized control identity.");
        }
        return new ControlTokenAlgorithmDescriptor(algorithm, control, revision,
            new Evaluator<TToken>(new TAlgorithm()));
    }

    /// <summary>
    /// The algorithm whose control stage this is.
    /// </summary>
    public ThemeAlgorithm Algorithm { get; }

    /// <summary>
    /// The control whose tokens this stage computes.
    /// </summary>
    public ControlTokenIdentity Control { get; }

    /// <summary>
    /// The revision declared for this control stage; it is part of the registry revision.
    /// </summary>
    public int Revision { get; }

    /// <summary>
    /// Identifies this registration in compilation caches. Two descriptors are never interchangeable, even with
    /// the same identities and revision.
    /// </summary>
    internal object CompilationIdentity { get; } = new();

    internal void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ArgumentNullException.ThrowIfNull(controlGlobal);
        _evaluator.EvaluateGlobal(in context, controlGlobal);
    }

    internal void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal,
        AbstractControlDesignToken token)
    {
        ArgumentNullException.ThrowIfNull(effectiveGlobal);
        ArgumentNullException.ThrowIfNull(token);
        _evaluator.EvaluateOwn(in context, effectiveGlobal, token, Control);
    }

    private abstract class Evaluator
    {
        internal abstract void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal);

        internal abstract void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal,
            AbstractControlDesignToken token, ControlTokenIdentity control);
    }

    private sealed class Evaluator<TToken>(IControlTokenAlgorithm<TToken> algorithm) : Evaluator
        where TToken : AbstractControlDesignToken
    {
        internal override void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
            algorithm.EvaluateGlobal(in context, controlGlobal);

        internal override void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal,
            AbstractControlDesignToken token, ControlTokenIdentity control)
        {
            if (token is not TToken typed)
            {
                throw new ArgumentException(
                    $"Control algorithm for control '{control}' expects a '{typeof(TToken).FullName}' token, " +
                    $"not '{token.GetType().FullName}'.", nameof(token));
            }
            algorithm.EvaluateOwn(in context, effectiveGlobal, typed);
        }
    }
}
