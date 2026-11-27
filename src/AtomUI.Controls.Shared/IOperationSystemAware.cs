using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// Receives the operating system type and version detected by the framework. It is implemented by
/// <c>Window</c>, <c>WindowTitleBar</c> and <c>CaptionButtonGroup</c> in AtomUI.Desktop.Controls.
/// </summary>
/// <remarks>
/// <c>ConfigureOsType</c> in this assembly detects the platform and calls <c>SetOsType</c> with <c>Windows</c>,
/// <c>macOS</c> (also for Mac Catalyst), <c>Linux</c> or <c>Unknown</c>, then passes
/// <c>Environment.OSVersion.Version</c> to <c>SetOsVersion</c>. The three implementations declare both setters as
/// explicit interface members.
/// </remarks>
internal interface IOperationSystemAware
{
    OsType OsType { get; }
    void SetOsType(OsType osType);
    
    Version OsVersion { get; }
    void SetOsVersion(Version version);
}

internal abstract class OperationSystemAwareControlProperty : AvaloniaObject
{
    public const string OsTypePropertyName = "OsType";
    public const string OsVersionPropertyName = "OsVersion";
    public static readonly StyledProperty<OsType> OsTypeProperty =
        AvaloniaProperty.Register<OperationSystemAwareControlProperty, OsType>(OsTypePropertyName, OsType.Unknown);
    public static readonly StyledProperty<Version> OsVersionProperty =
        AvaloniaProperty.Register<OperationSystemAwareControlProperty, Version>(OsVersionPropertyName);
}

internal static class OperationSystemAwareExtensions
{
    public static void ConfigureOsType(this IOperationSystemAware operationSystemAware)
    {
        if (OperatingSystem.IsWindows())
        {
            operationSystemAware.SetOsType(OsType.Windows);
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            operationSystemAware.SetOsType(OsType.macOS);
        }
        else if (OperatingSystem.IsLinux())
        {
            operationSystemAware.SetOsType(OsType.Linux);
        }
        else
        {
            operationSystemAware.SetOsType(OsType.Unknown);
        }
        operationSystemAware.SetOsVersion(Environment.OSVersion.Version);
    }
}