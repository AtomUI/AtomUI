using System.Diagnostics;

namespace AtomUI.Build.Tasks;

// Only immutable bundle I/O is shared. Callers own fingerprints, engine identity and lock policy.
internal static class ToolBundleCache
{
    internal static FileStream Acquire(string path, TimeSpan timeout, int retryMilliseconds)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var timer = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (timer.Elapsed < timeout)
            {
                Thread.Sleep(retryMilliseconds);
            }
        }
    }

    internal static void Prepare(string destination, IReadOnlyDictionary<string, byte[]> files,
        string invalidCacheMessage, Action<string, string>? initializeFile = null)
    {
        if (!Directory.Exists(destination))
        {
            var staging = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
            Directory.CreateDirectory(staging);
            try
            {
                foreach (var file in files)
                {
                    var path = Path.Combine(staging, file.Key);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.WriteAllBytes(path, file.Value);
                    initializeFile?.Invoke(file.Key, path);
                }
                try
                {
                    Directory.Move(staging, destination);
                }
                catch (IOException) when (Directory.Exists(destination))
                {
                    // A concurrent writer won. Validate its complete bundle below before reuse.
                }
            }
            finally
            {
                if (Directory.Exists(staging))
                {
                    Directory.Delete(staging, recursive: true);
                }
            }
        }
        var actual = Directory.GetFiles(destination, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(destination, path).Replace('\\', '/')).ToHashSet(StringComparer.Ordinal);
        if (!actual.SetEquals(files.Keys) || files.Any(file =>
                RegistrationFiles.Hash(Path.Combine(destination, file.Key)) != RegistrationFiles.Hash(file.Value)))
        {
            throw new InvalidDataException(invalidCacheMessage);
        }
    }
}
