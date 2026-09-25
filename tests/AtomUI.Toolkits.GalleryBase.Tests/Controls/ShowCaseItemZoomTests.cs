using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using Button = Avalonia.Controls.Button;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class ShowCaseItemZoomTests
{
    public ShowCaseItemZoomTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Zoom_Action_Is_Visible_By_Default()
    {
        var item = new ShowCaseItem();

        item.IsZoomActionVisible.ShouldBeTrue();
    }

    [Fact]
    public void Disabling_Zoom_On_The_Item_Hides_The_Zoom_Action()
    {
        var item = new ShowCaseItem { IsZoomEnabled = false };

        item.IsZoomActionVisible.ShouldBeFalse();
    }

    [Fact]
    public void Disabling_Zoom_Globally_Hides_The_Zoom_Action()
    {
        var options = CreateOptions();
        options.ShowCaseZoom.IsEnabled = false;
        GalleryBaseConfigurationProvider.SetCurrent(options.BuildConfiguration());
        try
        {
            var item = new ShowCaseItem();

            item.IsZoomActionVisible.ShouldBeFalse();
        }
        finally
        {
            GalleryBaseConfigurationProvider.SetCurrent(null!);
        }
    }

    private static GalleryBaseOptions CreateOptions()
    {
        var options = new GalleryBaseOptions();
        options.Branding.VersionText = "v1";
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map(
            "Overview",
            screen => new TestRouteViewModel(screen),
            () => new TestRouteView());
        return options;
    }

    private sealed class TestRouteViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
    {
        public string UrlPathSegment => "Overview";

        public IScreen HostScreen { get; } = hostScreen;
    }

    private sealed class TestRouteView : ReactiveUserControl<TestRouteViewModel>
    {
    }

    [Fact]
    public void Zoom_Button_Click_Raises_ZoomRequested_With_Title_And_Item()
    {
        ShowInWindow(
            new ShowCaseItem { Title = "Zoom Me", Content = new Border() },
            item =>
            {
                var received = 0;
                ShowCaseZoomRequestedEventArgs? args = null;
                item.ZoomRequested += (_, e) => { received++; args = e; };

                var zoomButton = FindZoomButton(item);
                zoomButton.ShouldNotBeNull();
                zoomButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent) { Source = zoomButton });
                Dispatcher.UIThread.RunJobs();

                received.ShouldBe(1);
                args.ShouldNotBeNull();
                args.Item.ShouldBeSameAs(item);
                args.Title.ShouldBe("Zoom Me");
            });
    }

    [Fact]
    public void Zoom_Button_Is_Hidden_When_Zoom_Is_Disabled()
    {
        ShowInWindow(
            new ShowCaseItem { Title = "No Zoom", IsZoomEnabled = false, Content = new Border() },
            item =>
            {
                var zoomButton = FindZoomButton(item);
                zoomButton.ShouldNotBeNull();
                zoomButton.IsVisible.ShouldBeFalse();
            });
    }

    [Fact]
    public void Zoom_Button_Click_Materializes_Deferred_Content_First()
    {
        var item = new ShowCaseItem
        {
            Title = "Deferred",
            IsDeferredContentEnabled = true,
            DeferredContentTemplate = new FuncDataTemplate<object?>(
                (_, _) => new Border { Width = 40, Height = 40 })
        };

        ShowInWindow(item, shown =>
        {
            shown.IsDeferredContentMaterialized.ShouldBeFalse();

            var zoomButton = FindZoomButton(shown);
            zoomButton.ShouldNotBeNull();
            zoomButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent) { Source = zoomButton });
            Dispatcher.UIThread.RunJobs();

            shown.IsDeferredContentMaterialized.ShouldBeTrue();
        });
    }

    [Fact]
    public void Both_Control_Templates_Contain_A_Zoom_Button()
    {
        ShowInWindow(new ShowCaseItem { Title = "Plain", Content = new Border() }, plain =>
        {
            FindZoomButton(plain).ShouldNotBeNull();
        });

        ShowInWindow(
            new ShowCaseItem { Title = "Badge", BadgeText = "new", Content = new Border() },
            badge =>
            {
                FindZoomButton(badge).ShouldNotBeNull();
                badge.IsBadgeVisible.ShouldBeTrue();
            });
    }

    [Fact]
    public void Theme_File_Declares_Zoom_Button_In_Both_Templates()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Toolkits.GalleryBase/Controls/Themes/ShowCaseItemTheme.axaml"));

        // 两份模板各一个按钮声明 + 一个 IsFake 隐藏样式选择器
        CountOccurrences(source, "PART_ZoomButton").ShouldBe(3);
        source.ShouldContain("ExpandOutlined");
    }

    private static IconButton? FindZoomButton(ShowCaseItem item)
    {
        return item.GetVisualDescendants()
                   .OfType<IconButton>()
                   .FirstOrDefault(button => button.Name == "PART_ZoomButton");
    }

    private static void ShowInWindow<T>(T content, Action<T> assertion) where T : Control
    {
        var window = new AvaloniaWindow { Width = 500, Height = 500, Content = content };
        try
        {
            window.Show();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            assertion(content);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = AppContext.BaseDirectory;
        while (directory is not null)
        {
            var candidate = Path.Combine(directory, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            directory = Directory.GetParent(directory)?.FullName;
        }
        throw new FileNotFoundException(relativePath);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }
}
