using AtomUI;
using AtomUI.Registration;
using Avalonia;
#if !REAL_DESKTOP
using Avalonia.Headless;
#endif
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Registration.Fixtures;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
#if REAL_DESKTOP
        AppBuilder.Configure<ProductApp>().UsePlatformDetect().UseAtomUI(ProductApp.Configure).SetupWithoutStarting();
#else
        AppBuilder.Configure<ProductApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).UseAtomUI(ProductApp.Configure).SetupWithoutStarting();
#endif
        var app = (ProductApp)Application.Current!;
#if REAL_DESKTOP && COMPLEX
        var titleButton = new AtomUI.Desktop.Controls.WindowTitleBarButton { Content = "Action" };
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = app.CreateContent(), Width = 600, Height = 500,
            Title = "Resource platform fixture", RightAddOn = titleButton
        };
#else
        var window = new Avalonia.Controls.Window { Content = app.CreateContent(), Width = 600, Height = 500 };
#endif
        window.Show();
        Dispatcher.UIThread.RunJobs();
#if REAL_DESKTOP && COMPLEX
        if (window.Template is null || titleButton.Template is null ||
            !titleButton.GetVisualDescendants().Any(control => control.GetType().Name == "CaptionButtonFrame"))
        {
            throw new InvalidOperationException("PRODUCT_FAIL native window resource classes did not create title bar templates");
        }
        Console.WriteLine("PRODUCT_CHECK native window and title bar resource classes execute");
#endif
        var verification = app.VerifyAsync();
        while (!verification.IsCompleted)
        {
            Dispatcher.UIThread.RunJobs();
        }
        verification.GetAwaiter().GetResult();
        window.Close();
        Console.WriteLine("PRODUCT_PASS " + (ControlRegistrationRuntime.IsTrimmed ? "selected" : "full"));
    }
}
