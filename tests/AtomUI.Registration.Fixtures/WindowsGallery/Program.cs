// Explicit Windows specialist: run the shipping Gallery configuration and routes.
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase.Shell;
using AtomUIGallery;
using AtomUIGallery.Desktop;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI.Avalonia;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        return AppBuilder.Configure<ProbeApplication>()
            .UseReactiveUI(build => build.ConfigureViewLocator(AtomUIGalleryModule.RegisterViews))
            .UseAtomUIPlatformDetect()
            .WithAtomUIDefaultOptions()
            .UseAtomUI(GalleryApplication.Configure)
            .StartWithClassicDesktopLifetime(args);
    }
}

public sealed partial class ProbeApplication : GalleryApplication
{
    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
        Dispatcher.UIThread.Post(async () =>
        {
            var lifetime = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
            try
            {
                WorkspaceWindow? window = null;
                for (var retry = 0; retry < 300; retry++)
                {
                    window = lifetime.MainWindow as WorkspaceWindow;
                    if (window?.IsVisible == true) { break; }
                    await Task.Delay(100);
                }
                if (window?.IsVisible != true) { throw new InvalidOperationException("Gallery main window did not appear"); }
                await Verify(window);
                lifetime.Shutdown(0);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("GALLERY_FAIL " + exception);
                lifetime.Shutdown(1);
            }
        }, DispatcherPriority.Background);
    }

    private static async Task Verify(WorkspaceWindow window)
    {
        var checks = 0;
        void Require(bool condition, string message)
        {
            checks++;
            if (!condition) { throw new InvalidOperationException(message); }
        }
        var hwnd = window.TryGetPlatformHandle()!.Handle;
        GetWindowThreadProcessId(hwnd, out var pid);
        Require(window.TryGetPlatformHandle()!.HandleDescriptor == "HWND" && pid == Environment.ProcessId, "actual current-process Win32 HWND");
        Require(RuntimeFeature.IsDynamicCodeSupported || !Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module =>
            string.Equals(module.ModuleName, "coreclr.dll", StringComparison.OrdinalIgnoreCase)), "native process has no CoreCLR");
        var viewModel = window.ViewModel!;
        var host = window.GetVisualDescendants().OfType<GalleryShellView>().Single().RoutedViewHost;
        var routes = AtomUIGalleryModule.GetConfiguration().Routes.Routes;
        var families = new HashSet<string>();
        foreach (var route in routes)
        {
            await viewModel.Navigation.NavigateToCommand.Execute(route.RouteKey).ToTask();
            await Task.Delay(120);
            if (!route.ViewType.IsInstanceOfType(host.Content))
            {
                Console.WriteLine("GALLERY_WAIT route=" + route.RouteKey + " content=" + host.Content?.GetType().FullName +
                    " canNavigate=" + viewModel.Navigation.CanNavigateTo(route.RouteKey) +
                    " stack=" + string.Join(",", viewModel.Router.NavigationStack.Select(vm => vm.GetType().FullName)));
            }
            for (var wait = 0; !route.ViewType.IsInstanceOfType(host.Content) && wait < 40; wait++)
            {
                await Task.Delay(50);
            }
            var view = host.Content;
            Require(view is not null && route.ViewType.IsInstanceOfType(view), route.RouteKey + " explicit routed view");
            Require(((ReactiveUI.Binding.IViewFor)view!).ViewModel?.GetType() == route.ViewModelType, route.RouteKey + " exact view model");
            foreach (var control in ((Avalonia.Visual)view!).GetVisualDescendants())
            {
                var type = control.GetType();
                if (type.Namespace?.StartsWith("AtomUI.", StringComparison.Ordinal) == true) { families.Add(type.FullName!); }
            }
            await viewModel.ToggleDarkModeCommand.Execute(true).ToTask();
            Require(viewModel.IsDarkAppearanceMode && ReferenceEquals(view, host.Content), route.RouteKey + " dark command retains view");
            await viewModel.ToggleDarkModeCommand.Execute(false).ToTask();
            Require(!viewModel.IsDarkAppearanceMode && ReferenceEquals(view, host.Content), route.RouteKey + " light restore retains view");
            Console.WriteLine("GALLERY_ROUTE " + route.RouteKey + " view=" + view.GetType().FullName);
        }
        var first = routes[0];
        await viewModel.Navigation.NavigateToCommand.Execute(first.RouteKey).ToTask();
        await Task.Delay(150);
        Require(first.ViewType.IsInstanceOfType(host.Content), "return navigation after all pages");
        await viewModel.SwitchToZhCNCommand.Execute(System.Reactive.Unit.Default).ToTask();
        await viewModel.SwitchToEnUSCommand.Execute(System.Reactive.Unit.Default).ToTask();
        Require(!viewModel.IsDarkAppearanceMode, "language commands preserve appearance");
        Console.WriteLine("GALLERY_PASS routes=" + routes.Count + " checks=" + checks +
            " actualControlTypes=" + families.Count + " dynamic=" + RuntimeFeature.IsDynamicCodeSupported +
            " framework=" + RuntimeInformation.FrameworkDescription);
        File.WriteAllLines(Environment.GetEnvironmentVariable("GALLERY_TYPES") ?? Path.Combine(AppContext.BaseDirectory, "types.txt"), families.Order());
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(nint hwnd, out int processId);
}
