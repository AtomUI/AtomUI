using System.Collections.Frozen;

namespace AtomUI.Theme.Schema;

/// <summary>The descriptors of the global tokens, for languages that declare seeds (adjustable seeds, derivation sets).</summary>
public static class GlobalTokenSchema
{
    private static readonly FrozenDictionary<string, TokenDescriptor> s_byName =
        Generated.AtomUICore.GeneratedThemeSchema.GetGlobalTokens()
                 .ToFrozenDictionary(static token => token.Name, StringComparer.Ordinal);

    /// <summary>Every global token, by slot.</summary>
    public static IReadOnlyList<TokenDescriptor> Tokens => Generated.AtomUICore.GeneratedThemeSchema.GetGlobalTokens();

    /// <summary>The global token named <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentException">No global token has that name.</exception>
    public static TokenDescriptor Get(string name) =>
        s_byName.TryGetValue(name, out var descriptor)
            ? descriptor
            : throw new ArgumentException($"'{name}' is not a global token.", nameof(name));
}
