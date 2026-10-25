using System.Runtime.InteropServices;
using System.Text;

namespace AtomUI.Localization.IntegrationTests;

internal static class LanguagePackTestCleanup
{
    internal static void Delete(string root, Exception? primaryFailure)
    {
        try
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw CreateFailure(root, primaryFailure, error, DescribeRemainingBuildHosts(root));
        }
    }

    private static Exception CreateFailure(string root, Exception? primaryFailure, Exception cleanupFailure, string diagnostics)
    {
        var failure = new IOException($"Could not clean language-pack fixture '{root}'.{Environment.NewLine}{diagnostics}", cleanupFailure);
        return primaryFailure is null
            ? failure
            : new AggregateException("The language-pack consumer and fixture cleanup both failed.", primaryFailure, failure);
    }

    private static string DescribeRemainingBuildHosts(string root)
    {
        try
        {
            var paths = Directory.GetFiles(root, "build-host.log", SearchOption.AllDirectories);
            if (paths.Length == 0)
            {
                return "No build-host.log remains after the failed cleanup.";
            }
            return string.Join(Environment.NewLine, paths.Select(path =>
                $"Remaining file: {path}{Environment.NewLine}{DescribeWindowsOwners(path)}"));
        }
        catch (Exception error)
        {
            return $"File-lock diagnostics could not be collected: {error}";
        }
    }

    private static string DescribeWindowsOwners(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            return "Restart Manager owner query is available only on Windows.";
        }

        var status = RmStartSession(out var session, 0, new StringBuilder(33));
        if (status != 0)
        {
            return $"RmStartSession status={status}.";
        }
        var details = new StringBuilder();
        try
        {
            status = RmRegisterResources(session, 1, [path], 0, IntPtr.Zero, 0, IntPtr.Zero);
            if (status != 0)
            {
                details.Append($"RmRegisterResources status={status}.");
            }
            else
            {
                uint count = 0;
                status = RmGetList(session, out var needed, ref count, null, out var rebootReasons);
                if (status == 234)
                {
                    var owners = new RestartManagerProcessInfo[checked((int)needed)];
                    count = needed;
                    status = RmGetList(session, out needed, ref count, owners, out rebootReasons);
                    if (status == 0)
                    {
                        foreach (var owner in owners.Take(checked((int)count)))
                        {
                            details.AppendLine($"Owner PID={owner.Process.ProcessId}; start-filetime={owner.Process.StartTime.High:x8}{owner.Process.StartTime.Low:x8}; app='{owner.ApplicationName}'; service='{owner.ServiceName}'; type={owner.ApplicationType}; status={owner.Status}; session={owner.SessionId}.");
                        }
                    }
                }
                details.Append($"RmGetList status={status}; count={count}; needed={needed}; reboot-reasons={rebootReasons}.");
            }
        }
        finally
        {
            // This is an observation session: never shut down or restart resource owners.
            details.Append($" RmEndSession status={RmEndSession(session)}.");
        }
        return details.ToString();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeFileTime
    {
        internal uint Low;
        internal uint High;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RestartManagerUniqueProcess
    {
        internal uint ProcessId;
        internal NativeFileTime StartTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RestartManagerProcessInfo
    {
        internal RestartManagerUniqueProcess Process;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string ApplicationName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        internal string ServiceName;
        internal uint ApplicationType;
        internal uint Status;
        internal uint SessionId;
        [MarshalAs(UnmanagedType.Bool)]
        internal bool Restartable;
    }

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern uint RmStartSession(out uint session, uint flags, StringBuilder key);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern uint RmRegisterResources(uint session, uint fileCount, string[] fileNames,
        uint applicationCount, IntPtr applications, uint serviceCount, IntPtr serviceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern uint RmGetList(uint session, out uint needed, ref uint count,
        [In, Out] RestartManagerProcessInfo[]? owners, out uint rebootReasons);

    [DllImport("rstrtmgr.dll")]
    private static extern uint RmEndSession(uint session);
}
