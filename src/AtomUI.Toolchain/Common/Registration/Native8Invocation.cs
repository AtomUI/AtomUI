namespace AtomUI.Build.Tasks.Registration;

// This authority comes only from caller parameters, never from receipt JSON.
internal sealed class Native8Invocation
{
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
        if (!full.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Native output escaped its invocation directory.");
        }
        EnsureNoLinks(full);
        return full;
    }

    internal string RequireExact(string actual, string relative)
    {
        string expected = File(relative);
        if (!Path.IsPathFullyQualified(actual) || Path.GetFullPath(actual) != expected)
        {
            throw new InvalidDataException("Native receipt path is not the expected invocation file: " + relative);
        }
        return expected;
    }

    internal void DeleteOwned(IEnumerable<string> paths)
    {
        foreach (string path in paths.Distinct(StringComparer.Ordinal))
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
            if (new FileInfo(current).LinkTarget is not null || new DirectoryInfo(current).LinkTarget is not null)
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
