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

    // 已打开的同类演示窗口只 Activate，不重复创建；关闭后从登记表移除，允许再次打开。
    private void ShowDemoWindow<TDemoWindow>(Func<TDemoWindow> factory)
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
        if (TopLevel.GetTopLevel(this) is AvaloniaWindow owner)
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
