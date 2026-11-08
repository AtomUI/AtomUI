using System.Text.RegularExpressions;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect;

/// <summary>
/// Reads the global Tokens a primitive theme consumes through <c>OwnerTokenResource</c>. The consumption map attributes
/// those reads to <c>shared-primitive</c>, so family effect tests list them per owner by hand; this parse is the drift guard
/// that keeps such a hand list equal to the theme AXAML.
/// </summary>
public static class ThemeOwnerReads
{
    // `{atom:OwnerTokenResource ColorText}`, `{atom:OwnerTokenResource Kind=ColorText}` and the element form
    // `<atom:OwnerTokenResource Kind="ColorText" />`.
    private static readonly Regex s_markup = new(
        @"\{\s*(?:[\w.-]+:)?OwnerTokenResource(?:Extension)?\s+(?:Kind\s*=\s*)?['""]?(?<name>\w+)",
        RegexOptions.CultureInvariant);

    private static readonly Regex s_element = new(
        @"<\s*(?:[\w.-]+:)?OwnerTokenResource(?:Extension)?\b[^>]*\bKind\s*=\s*""(?<name>\w+)""",
        RegexOptions.CultureInvariant);

    /// <summary>The distinct Token names read with OwnerTokenResource in the given repository-relative AXAML files.</summary>
    public static IReadOnlySet<string> Read(params string[] repositoryRelativePaths)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in repositoryRelativePaths)
        {
            result.UnionWith(Parse(File.ReadAllText(Path.Combine(RepositoryRoot, path))));
        }

        return result;
    }

    /// <summary>The distinct Token names read with OwnerTokenResource in an AXAML text.</summary>
    internal static IReadOnlySet<string> Parse(string axaml)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in s_markup.Matches(axaml))
        {
            result.Add(match.Groups["name"].Value);
        }

        foreach (Match match in s_element.Matches(axaml))
        {
            result.Add(match.Groups["name"].Value);
        }

        return result;
    }

    private static readonly Lazy<string> s_repositoryRoot = new(static () =>
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    });

    private static string RepositoryRoot => s_repositoryRoot.Value;
}
