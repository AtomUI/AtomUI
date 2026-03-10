using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using AtomUI;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class OsInfoViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static EntityKey ID = "OsInfo";

    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }
    
    public ObservableCollection<SystemInfoRecord> SystemInfoRecords { get; }
    
    private string? _logoPath;

    public string? LogoPath
    {
        get => _logoPath;
        set => this.RaiseAndSetIfChanged(ref _logoPath, value);
    }

    public string UrlPathSegment { get; } = ID.ToString();
    
    public OsInfoViewModel(IScreen screen)
    {
        Activator         = new ViewModelActivator();
        HostScreen        = screen;
        SystemInfoRecords = new ObservableCollection<SystemInfoRecord>();
        
    }
}

public class SystemInfoRecord
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } =  string.Empty;
}

public class SystemInfo
{
    public string OSName { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;
    public string CPUName { get; set; } = string.Empty;
    public int CPUCores { get; set; }
    public string MemoryInfo { get; set; } = string.Empty;
    public TimeSpan SystemUptime { get; set; }
    public string CurrentCulture { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string AvaloniaVersion { get; set; } = string.Empty;
    public string AtomUIVersion { get; set; } = string.Empty;
}

public static class SystemInfoProvider
{
    public static SystemInfo GetSystemInfo()
    {
        var info = new SystemInfo
        {
            OSName = GetOSName(),
            OSVersion = Environment.OSVersion.VersionString,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
            RuntimeVersion = $"{Environment.Version} ({RuntimeInformation.FrameworkDescription})",
            CPUCores = Environment.ProcessorCount,
            SystemUptime = GetSystemUptime(),
            CurrentCulture = System.Globalization.CultureInfo.CurrentCulture.Name,
            TimeZone = TimeZoneInfo.Local.DisplayName,
            AvaloniaVersion = AtomUIRuntimeInfo.GetAvaloniaVersion(),
            AtomUIVersion = AtomUIRuntimeInfo.GetAtomUIVersion()
        };

        info.CPUName = GetCPUInfo();
        info.MemoryInfo = GetMemoryInfo();

        return info;
    }

    private static string GetOSName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "Windows";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // 尝试从标准文件获取Linux发行版信息
            try
            {
                if (File.Exists("/etc/os-release"))
                {
                    var lines = File.ReadAllLines("/etc/os-release");
                    var nameLine = lines.FirstOrDefault(l => l.StartsWith("PRETTY_NAME="));
                    if (nameLine != null)
                    {
                        return nameLine.Split('=')[1].Trim('"');
                    }
                }
                
                // 检查其他可能的发行版标识文件
                if (File.Exists("/etc/redhat-release"))
                    return File.ReadAllText("/etc/redhat-release").Trim();
                    
                if (File.Exists("/etc/debian_version"))
                    return $"Debian {File.ReadAllText("/etc/debian_version").Trim()}";
            }
            catch
            {
                // 忽略错误，返回通用名称
            }
            return "Linux";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "macOS";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return "FreeBSD";
        }

        return RuntimeInformation.OSDescription;
    }

    private static string GetCPUInfo()
    {
        try
        {
            // 在 .NET 8 中，我们可以使用 Process.GetCurrentProcess() 获取一些信息
            // 但对于CPU型号，仍然需要平台特定方法
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 使用注册表获取CPU信息（Windows）
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                return key?.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? "Unknown Processor";
            }
            else if (File.Exists("/proc/cpuinfo"))
            {
                // Linux - 读取 /proc/cpuinfo
                var lines = File.ReadLines("/proc/cpuinfo");
                var modelName = lines.FirstOrDefault(l => l.StartsWith("model name"))?
                    .Split(':').ElementAtOrDefault(1)?.Trim();
                return modelName ?? $"Unknown {RuntimeInformation.ProcessArchitecture} Processor";
            }
            
            // 默认返回架构信息
            return $"{RuntimeInformation.ProcessArchitecture} Processor";
        }
        catch (Exception ex)
        {
            return $"Unknown (Error: {ex.Message})";
        }
    }

    private static string GetMemoryInfo()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            long totalMemoryBytes = gcInfo.TotalAvailableMemoryBytes;
            long memoryLoadBytes = gcInfo.MemoryLoadBytes;
            long availableMemory = totalMemoryBytes - memoryLoadBytes;
            var  totalGB         = totalMemoryBytes / (1024 * 1024 * 1024);
            var  availableGB     = availableMemory / (1024 * 1024 * 1024);
            return $"{totalGB} GB Total, {availableGB} GB Available";
        }
        catch (Exception ex)
        {
            return $"Unknown (Error: {ex.Message})";
        }
    }

    private static TimeSpan GetSystemUptime()
    {
        try
        {
            // 使用 Environment.TickCount 获取系统运行时间（毫秒）
            var tickCount = Environment.TickCount;
            if (tickCount > 0)
                return TimeSpan.FromMilliseconds(tickCount);
        }
        catch
        {
            // 忽略错误
        }
        
        return TimeSpan.Zero;
    }

    // Windows API 声明
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        
        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(this);
        }
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
}