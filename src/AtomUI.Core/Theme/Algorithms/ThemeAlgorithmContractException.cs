namespace AtomUI.Theme.Algorithms;

/// <summary>
/// Thrown when a theme algorithm breaks the contract of one of its optional stages: it writes a token outside
/// the stage, its writes do not stabilize, or it throws. A failed theme compilation reports it unchanged.
/// </summary>
public sealed class ThemeAlgorithmContractException : InvalidOperationException
{
    internal ThemeAlgorithmContractException(
        ThemeAlgorithm algorithm,
        string? tokenName,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Algorithm = algorithm;
        TokenName = tokenName;
    }

    /// <summary>
    /// The algorithm that broke the contract.
    /// </summary>
    public ThemeAlgorithm Algorithm { get; }

    /// <summary>
    /// The token involved, or <see langword="null"/> when the algorithm itself failed.
    /// </summary>
    public string? TokenName { get; }
}
