using AtomUI.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerTitleTests
{
    public ImagePreviewerTitleTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void DefaultImagePreviewTitleResolver_Uses_File_Name_From_Supported_Source_Uris()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-title-{Guid.NewGuid():N}");
        var localPath = Path.Combine(directory, "local image.png");

        Resolve(localPath).ShouldBe("local image.png");
        Resolve(new Uri(localPath).AbsoluteUri).ShouldBe("local image.png");
        Resolve("https://example.com/assets/remote-image.jpg?size=large#preview").ShouldBe("remote-image.jpg");
        Resolve("https://picsum.photos/id/25/600/400").ShouldBe("25");
        Resolve("avares://AtomUI.Tests/Assets/resource-icon.svg").ShouldBe("resource-icon.svg");
    }

    [Fact]
    public void DefaultImagePreviewTitleResolver_Uses_DisplayName_From_Stream_Source()
    {
        var item = new ImagePreviewItem(new StreamImageSource(
            _ => new ValueTask<Stream>(Stream.Null),
            displayName: "stream image.png"));
        var context = new ImagePreviewTitleResolveContext(item, 0, 1);

        DefaultImagePreviewTitleResolver.Instance.ResolveTitle(context).ShouldBe("stream image.png");
    }

    [Fact]
    public void ImagePreviewerDialog_Title_Uses_Window_Title_Before_Resolver()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = CreateDialog(
                "avares://AtomUI.Tests/Assets/first.png",
                "avares://AtomUI.Tests/Assets/second.png");

            dialog.CurrentIndex = 1;
            dialog.Title = "Pinned title";

            dialog.EffectivePreviewTitle.ShouldBe("Pinned title");

            dialog.Title = " ";

            dialog.EffectivePreviewTitle.ShouldBe("second.png");
        });
    }

    [Fact]
    public void ImagePreviewerDialog_Title_Updates_With_Current_Item_And_Clamps_Display_Index()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = CreateDialog(
                "avares://AtomUI.Tests/Assets/first.png",
                "avares://AtomUI.Tests/Assets/second.png");

            dialog.CurrentIndex = 1;
            dialog.EffectivePreviewTitle.ShouldBe("second.png");

            dialog.CurrentIndex = 12;
            dialog.EffectivePreviewTitle.ShouldBe("second.png");

            dialog.CurrentIndex = -5;
            dialog.EffectivePreviewTitle.ShouldBe("first.png");
        });
    }

    [Fact]
    public void ImagePreviewerDialog_Title_Recomputes_When_Resolver_Changes()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = CreateDialog("avares://AtomUI.Tests/Assets/source.png");

            dialog.EffectivePreviewTitle.ShouldBe("source.png");

            dialog.PreviewTitleResolver = new PrefixTitleResolver("custom");

            dialog.EffectivePreviewTitle.ShouldBe("custom:source.png:1/1");
        });
    }

    [Fact]
    public void ImagePreviewerDialog_Item_Title_ShortCircuits_The_Resolver()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var resolver = new CountingTitleResolver();
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer();
            var dialog = new ImagePreviewerDialog(new Avalonia.Controls.Window(), previewer)
            {
                PreviewTitleResolver = resolver,
                ItemsSource =
                [
                    new ImagePreviewEntry(new ImagePreviewItem(
                        ImageSource.Parse("avares://AtomUI.Tests/Assets/source.png"))
                    {
                        Title = "Item title"
                    })
                ]
            };

            dialog.EffectivePreviewTitle.ShouldBe("Item title");
            resolver.CallCount.ShouldBe(0);
        });
    }

    [Fact]
    public void ImagePreviewerDialog_ItemsSource_Change_Preserves_CurrentIndex()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer();
            var dialog = new ImagePreviewerDialog(new Avalonia.Controls.Window(), previewer)
            {
                CurrentIndex = 1,
                ItemsSource =
                [
                    CreateEntry("avares://AtomUI.Tests/Assets/first.png"),
                    CreateEntry("avares://AtomUI.Tests/Assets/second.png")
                ]
            };

            dialog.CurrentIndex.ShouldBe(1);
            dialog.EffectivePreviewTitle.ShouldBe("second.png");
            dialog.IsFirstImage.ShouldBeFalse();
            dialog.IsLastImage.ShouldBeTrue();
        });
    }

    [Theory]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.Linux)]
    [InlineData(OsType.macOS)]
    public void ImagePreviewerDialog_Default_TitleAlignment_Is_WindowCenter_On_All_Platforms(OsType osType)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = new TestImagePreviewerDialog();
            dialog.SetValue(global::AtomUI.Desktop.Controls.Window.OsTypeProperty, osType);
            var titleBar = new ImagePreviewerTitleBar();

            dialog.ConfigureTitleBar(titleBar);

            dialog.TitleAlignment.ShouldBe(WindowTitleBarTitleAlignment.WindowCenter);
            titleBar.TitleAlignment.ShouldBe(WindowTitleBarTitleAlignment.WindowCenter);
        });
    }

    [Fact]
    public void ImagePreviewerDialog_Projects_Window_TitleBar_Layout_State_To_Derived_TitleBar()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = new TestImagePreviewerDialog
            {
                TitleAlignment = WindowTitleBarTitleAlignment.Right,
                WindowState    = Avalonia.Controls.WindowState.Maximized
            };
            dialog.SetValue(
                global::AtomUI.Desktop.Controls.Window.NativeChromeInsetsProperty,
                new Avalonia.Thickness(80, 0, 0, 0));
            dialog.SetValue(
                global::AtomUI.Desktop.Controls.Window.IsCsdEnabledProperty,
                true);
            var titleBar = new ImagePreviewerTitleBar();

            dialog.ConfigureTitleBar(titleBar);

            titleBar.TitleAlignment.ShouldBe(WindowTitleBarTitleAlignment.Right);
            titleBar.NativeChromeInsets.ShouldBe(new Avalonia.Thickness(80, 0, 0, 0));
            titleBar.IsCsdEnabled.ShouldBeTrue();
            titleBar.HostWindowState.ShouldBe(Avalonia.Controls.WindowState.Maximized);
        });
    }

    [Fact]
    public void ImagePreviewerDialog_Configures_Derived_TitleBar_With_Effective_Preview_Title()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = new TestImagePreviewerDialog
            {
                ItemsSource =
                [
                    CreateEntry("avares://AtomUI.Tests/Assets/source.png")
                ]
            };
            var titleBar = new ImagePreviewerTitleBar();

            dialog.ConfigureTitleBar(titleBar);

            titleBar.Title.ShouldBe("source.png");
        });
    }

    [Fact]
    public void ImagePreviewerDialog_Handles_TitleBar_Toolbar_Requests_Relayed_From_Csd_Overlay()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var dialog = new TestImagePreviewerDialog
            {
                ItemsSource =
                [
                    CreateEntry("avares://AtomUI.Tests/Assets/first.png"),
                    CreateEntry("avares://AtomUI.Tests/Assets/second.png")
                ],
                CurrentImage = new DrawingImage(
                    new GeometryDrawing
                    {
                        Brush = Brushes.Transparent,
                        Geometry = new RectangleGeometry(new Rect(0, 0, 10, 10))
                    }),
                ImageScaleX  = 1.0,
                ImageScaleY  = 1.0
            };
            var titleBar = new ImagePreviewerTitleBar();

            dialog.ConfigureTitleBar(titleBar);
            var toolbar = titleBar.LeftAddOn.ShouldNotBeNull()
                                  .ShouldBeAssignableTo<ImagePreviewToolbar>();

            var rotateArgs = RelayToolbarRequest(dialog, toolbar, ImagePreviewBaseToolbar.RotateRightRequestEvent);
            rotateArgs.Handled.ShouldBeTrue();
            dialog.ImageRotate.ShouldBe(Math.PI / 2, 0.000001);

            var flipArgs = RelayToolbarRequest(dialog, toolbar, ImagePreviewBaseToolbar.HorizontalFlipRequestEvent);
            flipArgs.Handled.ShouldBeTrue();
            dialog.ImageScaleX.ShouldBe(-1.0);

            var fitArgs = new ImageFitToWindowEventArgs(false)
            {
                RoutedEvent = ImagePreviewBaseToolbar.FitToWindowRequestEvent
            };
            dialog.RaiseRoutedEventFromOverlay(toolbar, fitArgs);

            fitArgs.Handled.ShouldBeTrue();
            dialog.IsImageFitToWindow.ShouldBeTrue();

            var nextArgs = RelayToolbarRequest(dialog, toolbar, ImagePreviewBaseToolbar.NextRequestEvent);
            nextArgs.Handled.ShouldBeTrue();
            dialog.CurrentIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void ImagePreviewToolbar_TitleBar_Requests_Use_Host_Window_Route_In_Csd()
    {
        var toolbarSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewBaseToolbar.cs"));
        var titleBarSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerTitleBar.cs"));
        var dialogSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerDialog.cs"));

        toolbarSource.ShouldContain("ResolveCsdTitleBarHostWindow()");
        toolbarSource.ShouldContain("ToolbarSource != ImagePreviewToolbarSource.TitleBar");
        toolbarSource.ShouldContain("hostWindow.RaiseRoutedEventFromOverlay(this, args);");
        toolbarSource.ShouldContain("RaiseEvent(args);");
        toolbarSource.IndexOf("hostWindow.RaiseRoutedEventFromOverlay(this, args);", StringComparison.Ordinal)
                     .ShouldBeLessThan(toolbarSource.IndexOf("RaiseEvent(args);", StringComparison.Ordinal));

        titleBarSource.ShouldNotContain("ToolbarHorizontalFlipRequest");
        dialogSource.ShouldNotContain("_titleBarRequestSource");
        dialogSource.ShouldContain("ImagePreviewBaseToolbar.NextRequestEvent.AddClassHandler<ImagePreviewerDialog>");
    }

    [Fact]
    public void ImagePreviewerOverlayHost_ItemsSource_Change_Preserves_CurrentIndex()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer();
            var host = new ImagePreviewerOverlayHost(new Avalonia.Controls.Window(), previewer)
            {
                CurrentIndex = 1,
                ItemsSource =
                [
                    CreateEntry("avares://AtomUI.Tests/Assets/first.png"),
                    CreateEntry("avares://AtomUI.Tests/Assets/second.png")
                ]
            };

            host.CurrentIndex.ShouldBe(1);
            host.IsFirstImage.ShouldBeFalse();
            host.IsLastImage.ShouldBeTrue();
        });
    }

    [Fact]
    public void ImagePreviewer_Exposes_PreviewTitleIcon_As_PathIcon_Without_Window_Icon_Fallback()
    {
        var previewerSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/AbstractImagePreviewer.cs"));
        var dialogSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerDialog.cs"));
        var titleBarSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerTitleBar.cs"));

        previewerSource.ShouldContain("StyledProperty<PathIcon?> PreviewTitleIconProperty");
        previewerSource.ShouldContain("nameof(PreviewTitleIcon)");
        previewerSource.ShouldContain("public PathIcon? PreviewTitleIcon");
        previewerSource.ShouldContain("BindUtils.RelayBind(this, PreviewTitleIconProperty, dialogHost, ImagePreviewerDialog.TitleIconProperty)");
        previewerSource.ShouldNotContain("StyledProperty<PathIcon?> IconProperty");
        previewerSource.ShouldNotContain("public PathIcon? Icon");
        previewerSource.ShouldNotContain("PreviewWindowIcon");
        previewerSource.ShouldNotContain("Window.IconProperty");

        dialogSource.ShouldContain("StyledProperty<PathIcon?> TitleIconProperty");
        dialogSource.ShouldContain("previewerTitleBar[!ImagePreviewerTitleBar.IconProperty]");
        dialogSource.ShouldContain("this[!TitleIconProperty]");
        dialogSource.ShouldNotContain("titleBar[!WindowTitleBar.LogoProperty]");
        dialogSource.ShouldNotContain("titleBar[!WindowTitleBar.LogoTemplateProperty]");

        titleBarSource.ShouldContain("StyledProperty<PathIcon?> IconProperty");
        titleBarSource.ShouldContain("public PathIcon? Icon");
    }

    private static string? Resolve(string source)
    {
        var item    = new ImagePreviewItem(ImageSource.Parse(source));
        var context = new ImagePreviewTitleResolveContext(item, 0, 1);
        return DefaultImagePreviewTitleResolver.Instance.ResolveTitle(context);
    }

    private static ImagePreviewerDialog CreateDialog(params string[] sources)
    {
        var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer();
        var dialog = new ImagePreviewerDialog(new Avalonia.Controls.Window(), previewer)
        {
            ItemsSource = sources.Select(CreateEntry).ToList()
        };
        return dialog;
    }

    private static ImagePreviewEntry CreateEntry(string source)
    {
        return new ImagePreviewEntry(new ImagePreviewItem(ImageSource.Parse(source)));
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }

    private static ImagePreviewToolbarRequestEventArgs RelayToolbarRequest(
        ImagePreviewerDialog dialog,
        ImagePreviewToolbar toolbar,
        Avalonia.Interactivity.RoutedEvent<ImagePreviewToolbarRequestEventArgs> routedEvent)
    {
        var args = new ImagePreviewToolbarRequestEventArgs(ImagePreviewToolbarSource.TitleBar)
        {
            RoutedEvent = routedEvent
        };
        dialog.RaiseRoutedEventFromOverlay(toolbar, args);
        return args;
    }

    private sealed class PrefixTitleResolver : IImagePreviewTitleResolver
    {
        private readonly string _prefix;

        public PrefixTitleResolver(string prefix)
        {
            _prefix = prefix;
        }

        public string? ResolveTitle(in ImagePreviewTitleResolveContext context)
        {
            var fileName = DefaultImagePreviewTitleResolver.Instance.ResolveTitle(context);
            return $"{_prefix}:{fileName}:{context.CurrentIndex + 1}/{context.Count}";
        }
    }

    private sealed class TestImagePreviewerDialog : ImagePreviewerDialog
    {
        public TestImagePreviewerDialog()
            : base(new Avalonia.Controls.Window(), new global::AtomUI.Desktop.Controls.ImagePreviewer())
        {
        }

        public void ConfigureTitleBar(WindowTitleBar titleBar)
        {
            titleBar.AttachHost(this);
            NotifyConfigureTitleBar(titleBar);
        }
    }

    private sealed class CountingTitleResolver : IImagePreviewTitleResolver
    {
        internal int CallCount { get; private set; }

        public string? ResolveTitle(in ImagePreviewTitleResolveContext context)
        {
            CallCount++;
            return "Resolver title";
        }
    }
}
