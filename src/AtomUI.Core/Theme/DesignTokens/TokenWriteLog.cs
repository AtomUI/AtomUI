namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Distinct names of the token properties written while a token records writes, in first-write order.
/// </summary>
internal sealed class TokenWriteLog
{
    private readonly List<string> _names = new();
    private readonly HashSet<string> _seen = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> Names => _names;

    public void Add(string tokenName)
    {
        if (_seen.Add(tokenName))
        {
            _names.Add(tokenName);
        }
    }
}
