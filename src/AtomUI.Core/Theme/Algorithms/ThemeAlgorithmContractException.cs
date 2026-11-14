namespace AtomUI.Theme.Algorithms;

/// <summary>
/// The rule of an optional algorithm stage that a <see cref="ThemeAlgorithmContractException"/> reports as broken.
/// </summary>
public enum ThemeAlgorithmContractViolationKind
{
    /// <summary>
    /// The algorithm's writes did not settle within the compiler's round limit, usually because a token's new value
    /// was computed from that token's own current value. <see cref="ThemeAlgorithmContractException.TokenName"/> names
    /// the unstable token.
    /// </summary>
    Unstable,

    /// <summary>
    /// The algorithm wrote a token that does not belong to the stage, or wrote to an input it may only read.
    /// <see cref="ThemeAlgorithmContractException.TokenName"/> names the written token.
    /// </summary>
    StageBoundary,

    /// <summary>
    /// The algorithm threw. The exception it threw is the <see cref="Exception.InnerException"/>.
    /// </summary>
    Failed
}

/// <summary>
/// Thrown when a theme algorithm breaks the contract of one of its optional stages: it writes a token outside
/// the stage, its writes do not stabilize, or it throws. A failed theme compilation reports it unchanged.
/// </summary>
public sealed class ThemeAlgorithmContractException : InvalidOperationException
{
    internal ThemeAlgorithmContractException(
        ThemeAlgorithm algorithm,
        string? tokenName,
        ThemeAlgorithmContractViolationKind violation,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Algorithm = algorithm;
        TokenName = tokenName;
        Violation = violation;
    }

    /// <summary>
    /// The algorithm that broke the contract.
    /// </summary>
    public ThemeAlgorithm Algorithm { get; }

    /// <summary>
    /// The token involved, or <see langword="null"/> when the algorithm itself failed.
    /// </summary>
    public string? TokenName { get; }

    /// <summary>
    /// Which rule was broken.
    /// </summary>
    public ThemeAlgorithmContractViolationKind Violation { get; }
}
