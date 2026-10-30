namespace AtomUI.Theme.Definitions;

internal sealed class UserDirectoryThemeDefinitionResolver : IBackgroundThemeDefinitionResolver
{
    internal const string ResolverId = "AtomUI.UserThemeDirectory";
    internal const int DefaultMaxFiles = 128;
    internal const long DefaultMaxTotalBytes = 32L * 1024 * 1024;

    private readonly string? _directory;
    private readonly int _maxFiles;
    private readonly long _maxTotalBytes;

    internal UserDirectoryThemeDefinitionResolver(
        string? directory = null,
        int maxFiles = DefaultMaxFiles,
        long maxTotalBytes = DefaultMaxTotalBytes)
    {
        if (directory is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(directory);
            _directory = Path.GetFullPath(directory);
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxFiles);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTotalBytes);

        _maxFiles      = maxFiles;
        _maxTotalBytes = maxTotalBytes;
    }

    public string Id => ResolverId;
    public bool SupportsReload => true;

    public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string directory;
        try
        {
            if (_directory is null && string.IsNullOrWhiteSpace(context.ApplicationDataRoot))
            {
                return Failed(
                    "ATMTHM4101",
                    context.ApplicationId,
                    "The application-data root is unavailable for the user theme directory.");
            }

            directory = _directory ?? Path.Combine(
                context.ApplicationDataRoot,
                context.ApplicationId,
                "Themes");
            directory = Path.GetFullPath(directory);
            Directory.CreateDirectory(directory);
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
            {
                return Failed(
                    "ATMTHM4103",
                    directory,
                    "The user theme directory cannot be a symbolic link or reparse point.");
            }
            directory = ThemeDefinitionFileStream.GetCanonicalDirectoryPath(directory);
        }
        catch (Exception exception)
        {
            return Failed(
                "ATMTHM4101",
                _directory ?? context.ApplicationDataRoot,
                $"The user theme directory could not be prepared: {exception.GetBaseException().Message}");
        }

        var paths = new List<string>();
        long fileCount = 0;
        try
        {
            foreach (var entry in Directory.EnumerateFiles(directory, "*.theme.xml", SearchOption.TopDirectoryOnly))
            {
                // Continue enumeration and normalization past the cap so later errors
                // retain precedence, and the limit diagnostic retains the exact count.
                var path = Path.GetFullPath(entry);
                fileCount++;
                if (fileCount <= _maxFiles)
                {
                    paths.Add(path);
                }
            }
        }
        catch (Exception exception)
        {
            return Failed(
                "ATMTHM4102",
                directory,
                $"The user theme directory could not be enumerated: {exception.GetBaseException().Message}");
        }

        if (fileCount > _maxFiles)
        {
            return Failed(
                "ATMTHM4104",
                directory,
                $"The user theme directory contains {fileCount} theme files; the limit is {_maxFiles}.");
        }
        paths.Sort(StringComparer.Ordinal);

        var rootPrefix = directory.EndsWith(Path.DirectorySeparatorChar)
            ? directory
            : directory + Path.DirectorySeparatorChar;
        var files = new FileInfo[paths.Count];
        long totalBytes = 0;
        for (var index = 0; index < paths.Count; index++)
        {
            var path = paths[index];
            try
            {
                if (!path.StartsWith(rootPrefix, StringComparison.Ordinal) ||
                    (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                {
                    return Failed(
                        "ATMTHM4103",
                        path,
                        "User theme files must be direct, non-symbolic-link children of the configured directory.");
                }

                var file = new FileInfo(path);
                totalBytes = checked(totalBytes + file.Length);
                if (totalBytes > _maxTotalBytes)
                {
                    return Failed(
                        "ATMTHM4105",
                        directory,
                        $"The user theme directory exceeds the {_maxTotalBytes}-byte total input limit.");
                }
                files[index] = file;
            }
            catch (Exception exception)
            {
                return Failed(
                    "ATMTHM4102",
                    path,
                    $"User theme file metadata could not be read: {exception.GetBaseException().Message}");
            }
        }

        var budget = new UserThemeReadBudget(_maxTotalBytes);
        var sources = new IThemeDefinitionSource[files.Length];
        for (var index = 0; index < files.Length; index++)
        {
            var file = files[index];
            sources[index] = new UserThemeFileSource(
                file.FullName,
                $"{file.Length}:{file.LastWriteTimeUtc.Ticks}",
                budget);
        }
        return new ThemeDefinitionResolveResult(sources, Array.Empty<ThemeDiagnostic>());
    }

    private static ThemeDefinitionResolveResult Failed(
        string code,
        string source,
        string message)
    {
        return new ThemeDefinitionResolveResult(
            Array.Empty<IThemeDefinitionSource>(),
            [new ThemeDiagnostic(
                code,
                ThemeDiagnosticSeverity.Error,
                source,
                "$",
                message)]);
    }

    private sealed class UserThemeFileSource : IThemeDefinitionSource
    {
        private readonly string _path;
        private readonly UserThemeReadBudget _budget;

        internal UserThemeFileSource(
            string path,
            string revision,
            UserThemeReadBudget budget)
        {
            _path          = path;
            _budget        = budget;
            SourceIdentity = path;
            SourceRevision = revision;
        }

        public string SourceIdentity { get; }
        public string SourceRevision { get; }

        public Stream OpenRead()
        {
            EnsureDirectRegularFile();
            var stream = ThemeDefinitionFileStream.OpenReadNoFollow(_path);
            try
            {
                EnsureDirectRegularFile();
                return new BudgetedReadStream(stream, _budget);
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        private void EnsureDirectRegularFile()
        {
            if ((File.GetAttributes(_path) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException(
                    $"User theme file '{_path}' became a symbolic link or reparse point.");
            }
        }
    }

    private sealed class UserThemeReadBudget(long maximumBytes)
    {
        private long _readBytes;

        internal void Add(int count)
        {
            if (Interlocked.Add(ref _readBytes, count) > maximumBytes)
            {
                throw new InvalidDataException(
                    $"User theme sources exceed the {maximumBytes}-byte total input limit.");
            }
        }
    }

    private sealed class BudgetedReadStream(
        Stream inner,
        UserThemeReadBudget budget) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = inner.Read(buffer, offset, count);
            budget.Add(read);
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            var read = inner.Read(buffer);
            budget.Add(read);
            return read;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
