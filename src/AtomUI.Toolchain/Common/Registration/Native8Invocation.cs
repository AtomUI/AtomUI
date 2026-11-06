namespace AtomUI.Build.Tasks.Registration;

// This authority comes only from caller parameters, never from receipt JSON.
internal sealed class Native8Invocation
{
    internal static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    internal static StringComparison PathComparison => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    internal static bool SameFilePath(string left, string right) => PathComparer.Equals(CanonicalFilePath(left), CanonicalFilePath(right));

    private static string CanonicalFilePath(string path)
    {
        if (!Path.IsPathFullyQualified(path))
        {
            throw new InvalidDataException("Native file identity requires an absolute path.");
        }
        string full = Path.GetFullPath(path);
        if (!OperatingSystem.IsWindows() || !full.StartsWith(@"\\?\", StringComparison.Ordinal))
        {
            return full;
        }
        string ordinary;
        int skip;
        if (full.Length >= 7 && char.IsAsciiLetter(full[4]) && full[5] == ':' && full[6] == '\\')
        {
            ordinary = full[4..];
            skip = 3;
        }
        else if (full.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
        {
            ordinary = @"\\" + full[8..];
            skip = 2;
        }
        else
        {
            throw new InvalidDataException("Unsupported Windows device path for native file identity.");
        }
        // Extended paths bypass Win32 normalization. Strip only an equivalent safe
        // DOS/UNC prefix; never equate trailing-dot/space or navigation aliases.
        if (ordinary[skip..].Split('\\').Any(part => part.Length == 0 || part is "." or ".." || part.EndsWith('.') || part.EndsWith(' ')) ||
            !PathComparer.Equals(Path.GetFullPath(ordinary), ordinary))
        {
            throw new InvalidDataException("Noncanonical extended Windows native file path.");
        }
        return ordinary;
    }
    internal string Id { get; }
    internal string Root { get; }

    internal Native8Invocation(string id, string root)
    {
        if (string.IsNullOrEmpty(id) || id.Length != 32 || id.Any(c => !char.IsAsciiHexDigitLower(c)) || !Path.IsPathFullyQualified(root))
        {
            throw new InvalidDataException("Native invocation requires a caller-generated lowercase GUID and absolute owned root.");
        }
        Root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        if (Path.GetFileName(Root) != id || !Directory.Exists(Root))
        {
            throw new InvalidDataException("Native invocation root must be the caller-created directory named by its invocation ID.");
        }
        EnsureNoLinks(Root);
        Id = id;
    }

    internal string File(string relative) => RequireOwned(Path.Combine(Root, relative));

    internal string RequireOwned(string path)
    {
        string full = Path.GetFullPath(path);
        if (!full.StartsWith(Root + Path.DirectorySeparatorChar, PathComparison))
        {
            throw new InvalidDataException("Native output escaped its invocation directory.");
        }
        EnsureNoLinks(full);
        return full;
    }

    internal string RequireExact(string actual, string relative)
    {
        string expected = File(relative);
        if (!Path.IsPathFullyQualified(actual) || !PathComparer.Equals(Path.GetFullPath(actual), expected))
        {
            throw new InvalidDataException("Native receipt path is not the expected invocation file: " + relative);
        }
        return expected;
    }

    internal void DeleteOwned(IEnumerable<string> paths)
    {
        foreach (string path in paths.Distinct(PathComparer))
        {
            try
            {
                string owned = RequireOwned(path);
                if (System.IO.File.Exists(owned))
                {
                    System.IO.File.Delete(owned);
                }
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
            {
                // Cleanup must never follow a replaced directory/link or hide the original failure.
            }
        }
    }

    internal static string RequireUnlinkedAbsolute(string path)
    {
        if (!Path.IsPathFullyQualified(path))
        {
            throw new InvalidDataException("Expected an absolute native output path.");
        }
        string full = Path.GetFullPath(path);
        EnsureNoLinks(full);
        return full;
    }

    private static void EnsureNoLinks(string path)
    {
        for (string? current = path; current is not null; current = Path.GetDirectoryName(current))
        {
            if (new FileInfo(current).LinkTarget is not null || new DirectoryInfo(current).LinkTarget is not null ||
                ((System.IO.File.Exists(current) || Directory.Exists(current)) &&
                 (System.IO.File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0))
            {
                throw new InvalidDataException("Native invocation paths cannot contain symbolic links or reparse points.");
            }
            if (Path.GetPathRoot(current) == current)
            {
                break;
            }
        }
    }
}
