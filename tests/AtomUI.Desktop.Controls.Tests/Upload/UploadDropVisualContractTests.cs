using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Upload;

public class UploadDropVisualContractTests
{
    private static readonly XNamespace AvaloniaNamespace = "https://github.com/avaloniaui";

    [Fact]
    public void Drop_Themes_Keep_The_Existing_Default_Visual_Contract()
    {
        var dropZoneTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Upload/Themes/UploadDropZoneTheme.axaml"));
        var dropAreaTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Upload/Themes/UploadDefaultDropAreaTheme.axaml"));

        var dropZoneTemplate = dropZoneTheme.Descendants(AvaloniaNamespace + "ControlTemplate").Single();
        var dropZoneRoot = dropZoneTemplate.Elements().Single();
        dropZoneRoot.Name.LocalName.ShouldBe("ContentPresenter");
        ((string?)dropZoneRoot.Attribute("Name")).ShouldBe("PART_ContentPresenter");
        ((string?)dropZoneRoot.Attribute("Content")).ShouldBe("{TemplateBinding Content}");
        ((string?)dropZoneRoot.Attribute("ContentTemplate")).ShouldBe("{TemplateBinding ContentTemplate}");
        ((string?)dropZoneRoot.Attribute("HorizontalContentAlignment"))
            .ShouldBe("{TemplateBinding HorizontalContentAlignment}");
        ((string?)dropZoneRoot.Attribute("VerticalContentAlignment"))
            .ShouldBe("{TemplateBinding VerticalContentAlignment}");

        var dropAreaTemplate = dropAreaTheme.Descendants(AvaloniaNamespace + "ControlTemplate").Single();
        var frame = dropAreaTemplate.Elements().Single();
        frame.Name.LocalName.ShouldBe("DashedBorder");
        ((string?)frame.Attribute("Name")).ShouldBe("Frame");
        ((string?)frame.Attribute("Background")).ShouldBe("{TemplateBinding Background}");
        ((string?)frame.Attribute("BorderBrush")).ShouldBe("{TemplateBinding BorderBrush}");
        ((string?)frame.Attribute("BorderThickness")).ShouldBe("{TemplateBinding BorderThickness}");
        ((string?)frame.Attribute("CornerRadius")).ShouldBe("{TemplateBinding CornerRadius}");
        ((string?)frame.Attribute("Padding")).ShouldBe("{TemplateBinding Padding}");

        var stackPanel = frame.Elements().Single();
        stackPanel.Name.LocalName.ShouldBe("StackPanel");
        ((string?)stackPanel.Attribute("Orientation")).ShouldBe("Vertical");
        stackPanel.Elements().Select(element => (element.Name.LocalName, (string?)element.Attribute("Name")))
            .ShouldBe([
                ("IconPresenter", "IconPresenter"),
                ("ContentPresenter", "HeaderContentPresenter"),
                ("ContentPresenter", "SubHeaderContentPresenter")
            ]);

        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Upload/Themes/UploadDefaultDropAreaTheme.axaml"));
        source.ShouldContain("<Setter Property=\"Background\" Value=\"{atom:SharedTokenResource ColorFillAlter}\"/>");
        source.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"{atom:SharedTokenResource BorderRadiusLG}\"/>");
        source.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"{atom:SharedTokenResource ColorBorder}\"/>");
        source.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"{atom:SharedTokenResource BorderThickness}\"/>");
        source.ShouldContain("<Setter Property=\"Padding\" Value=\"{atom:SharedTokenResource Padding}\"/>");
        source.ShouldContain("<Setter Property=\"Header\" Value=\"{atom:UploadLangResource DragUploadHead}\"/>");
        source.ShouldContain("<Setter Property=\"IsMotionEnabled\" Value=\"{atom:SharedTokenResource EnableMotion}\"/>");
        source.ShouldContain("<Style Selector=\"^:pointerover\">");
        source.ShouldContain("<Style Selector=\"^:disabled\">");
        source.ShouldContain("<Style Selector=\"^[IsMotionEnabled=True]\">");
        source.ShouldContain("<atom:SolidColorBrushTransition Property=\"BorderBrush\"");
    }

    [Fact]
    public void Default_Drop_Area_Is_Visual_Only_And_Default_Themes_Ignore_Drag_State()
    {
        var dropAreaSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Upload/UploadDefaultDropArea.cs"));
        var dropAreaTheme = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Upload/Themes/UploadDefaultDropAreaTheme.axaml"));
        var dropZoneTheme = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Upload/Themes/UploadDropZoneTheme.axaml"));

        dropAreaSource.ShouldNotContain("Avalonia.Input");
        dropAreaSource.ShouldNotContain("IStorageFile");
        dropAreaSource.ShouldNotContain("IDataTransfer");
        dropAreaSource.ShouldNotContain("FilesDropped");
        dropAreaSource.ShouldNotContain("HandleDrop");
        dropAreaTheme.ShouldNotContain("DragDrop.AllowDrop");

        foreach (var pseudoClass in new[]
                 {
                     ":drag-over",
                     ":drag-accepting",
                     ":drag-rejecting",
                     ":drop-processing"
                 })
        {
            dropAreaTheme.ShouldNotContain(pseudoClass);
            dropZoneTheme.ShouldNotContain(pseudoClass);
        }
    }

    [Fact]
    public void Default_Drop_Area_Keeps_Stable_Logical_Layout_In_Theme_And_Scaling_Matrix()
    {
        var dropArea = new UploadDefaultDropArea();
        var upload = new Desktop.Controls.Upload
        {
            AutoUpload = false,
            TriggerContent = new UploadDropZone
            {
                Content = dropArea
            }
        };
        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 360,
            Content = upload,
            RequestedThemeVariant = ThemeVariant.Light
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var baselineSize = dropArea.Bounds.Size;
            baselineSize.Width.ShouldBeGreaterThan(0);
            baselineSize.Height.ShouldBeGreaterThan(0);
            var lightSizeByScaling = new Dictionary<double, Size>();

            foreach (var useDarkTheme in new[] { false, true })
            {
                foreach (var renderScaling in new[] { 1.0, 1.5 })
                {
                    var theme = useDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
                    window.RequestedThemeVariant = theme;
                    window.SetRenderScaling(renderScaling);
                    Dispatcher.UIThread.RunJobs();

                    dropArea.ActualThemeVariant.ShouldBe(theme);
                    window.RenderScaling.ShouldBe(renderScaling);
                    dropArea.Bounds.Width.ShouldBe(baselineSize.Width);
                    Math.Abs(dropArea.Bounds.Height - baselineSize.Height).ShouldBeLessThanOrEqualTo(1);

                    if (useDarkTheme)
                    {
                        dropArea.Bounds.Size.ShouldBe(lightSizeByScaling[renderScaling]);
                    }
                    else
                    {
                        lightSizeByScaling.Add(renderScaling, dropArea.Bounds.Size);
                    }

                    dropArea.GetVisualDescendants()
                            .OfType<Control>()
                            .Single(control => control.Name == "Frame")
                            .Bounds.Size.ShouldBe(dropArea.Bounds.Size);
                }
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
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

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
    }
}
