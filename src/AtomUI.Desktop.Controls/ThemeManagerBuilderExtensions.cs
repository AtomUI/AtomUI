using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Registration;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Extension methods that register the desktop control package on an <see cref="IAtomUIBuilder" />.
/// </summary>
public static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls";

    /// <summary>
    /// Registers the desktop control package on the builder, with the common controls package as its dependency.
    /// When the builder commits, it also initializes the desktop runtime: the dialog input capture tracker,
    /// a <c>ToolTipService</c> when an input manager is registered, and on platforms with native windows the media
    /// break point bootstrap of the theme manager and, on Windows, the window style compatibility.
    /// </summary>
    /// <param name="builder">
    /// The builder that assembles the application's packages.
    /// </param>
    /// <returns>
    /// The <paramref name="builder" />, for chaining.
    /// </returns>
    public static IAtomUIBuilder UseDesktopControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return PackageMetadata.Definition.Register(builder, static package => package
            .DependsOn(static builder => builder.UseCommonControls())
            .OnCommit(InitializeDesktopRuntime));
    }

    private static void InitializeDesktopRuntime(ControlPackageContext context)
    {
        DialogInputCaptureTracker.Initialize();
        var inputManager = AvaloniaLocator.CurrentMutable.GetService(typeof(IInputManager)) as IInputManager;
        if (inputManager is not null)
        {
            AvaloniaLocator.CurrentMutable.BindToSelf(context.Own(new ToolTipService(inputManager)));
        }

        if (!RuntimePlatform.Features.SupportsNativeWindow)
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            context.Own(new WindowsWindowStyleCompatibility());
        }

        if (context.ThemeManager is ThemeManager themeManager)
        {
            context.Own(MediaBreakPointThemeBootstrapper.Attach(themeManager));
        }
    }

}
