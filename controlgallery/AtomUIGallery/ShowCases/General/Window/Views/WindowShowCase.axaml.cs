using Avalonia.Controls;
using Avalonia.Interactivity;

using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.ShowCases.Window;

public partial class WindowShowCase : GalleryReactiveUserControl<WindowViewModel>
{
    public const string LanguageId = nameof(WindowShowCase);

    private readonly Dictionary<Type, AtomUIWindow> _openDemoWindows = new();

    public WindowShowCase()
    {
        InitializeComponent();
    }

    // An open demo window of the same kind is only activated, not created again; closing removes it from
    // the registry so that it can be opened again.
    private void ShowDemoWindow<TDemoWindow>(Func<TDemoWindow> factory)
        where TDemoWindow : AtomUIWindow
    {
        ShowDemoWindow(factory, TopLevel.GetTopLevel(this) as AvaloniaWindow);
    }

    private void ShowDemoWindow<TDemoWindow>(Func<TDemoWindow> factory, AvaloniaWindow? owner)
        where TDemoWindow : AtomUIWindow
    {
        if (_openDemoWindows.TryGetValue(typeof(TDemoWindow), out var opened) && opened.IsVisible)
        {
            opened.Activate();
            return;
        }

        var window = factory();
        window.Closed += (_, _) => _openDemoWindows.Remove(typeof(TDemoWindow));
        _openDemoWindows[typeof(TDemoWindow)] = window;
        if (owner is not null)
        {
            window.Show(owner);
        }
        else
        {
            window.Show();
        }
    }

    private void HandleBasicWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new BasicDemoWindow());
    }

    private void HandleTitleAlignmentWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new TitleAlignmentDemoWindow());
    }

    private void HandleIsTitleVisibleWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new IsTitleVisibleDemoWindow());
    }

    private void HandleLogoVisibilityWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new LogoVisibilityDemoWindow());
    }

    private void HandleAddOnWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new AddOnDemoWindow());
    }

    private void HandleCaptionButtonsWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new CaptionButtonsDemoWindow());
    }

    private void HandleFullScreenTitleBarWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        // macOS does not take an owned window into native full screen, so this demo has no owner.
        ShowDemoWindow(() => new FullScreenTitleBarDemoWindow(), owner: null);
    }

    private void HandleTitleBarlessWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new TitleBarlessDemoWindow());
    }

    private void HandleFrameLayerWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new FrameLayerDemoWindow());
    }

    private void HandlePlaygroundButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ShowDemoWindow(() => new WindowPlayground());
    }
}
