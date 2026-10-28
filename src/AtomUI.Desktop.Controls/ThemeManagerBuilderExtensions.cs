using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Registration;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

public static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls";

    public static IAtomUIBuilder UseDesktopControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Register(builder, static package => package
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

        if (context.ThemeManager is ThemeManager themeManager)
        {
            context.Own(MediaBreakPointThemeBootstrapper.Attach(themeManager));
        }
    }

}
