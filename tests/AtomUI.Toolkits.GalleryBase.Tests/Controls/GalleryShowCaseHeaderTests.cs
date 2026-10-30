using System.Text.RegularExpressions;
using System.Xml.Linq;
using AtomUI.Desktop.Controls;
using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class GalleryShowCaseHeaderTests
{
    static GalleryShowCaseHeaderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Uses_Documented_Defaults()
    {
        var header = new GalleryShowCaseHeader();

        header.CategoryTagColor.ShouldBe("blue");
        header.StatusTagColor.ShouldBe("success");
        header.IntroducedVersionTagColor.ShouldBe("blue");
    }

    [Fact]
    public void Hides_Optional_Sections_When_Values_Are_Blank()
    {
        var header = new GalleryShowCaseHeader
        {
            Title             = "ImagePreviewer",
            Category          = "  ",
            Status            = null,
            IntroducedVersion = "",
            Subtitle          = " ",
            Description       = null,
            Namespace         = "",
            Package           = null,
            BaseClass         = " "
        };

        ShowInWindow(header, () =>
        {
            Find<Tag>(header, "PART_CategoryTag").IsVisible.ShouldBeFalse();
            Find<Tag>(header, "PART_StatusTag").IsVisible.ShouldBeFalse();
            Find<Tag>(header, "PART_IntroducedVersionTag").IsVisible.ShouldBeFalse();
            Find<AtomUITextBlock>(header, "PART_SubtitleText").IsVisible.ShouldBeFalse();
            Find<AtomUITextBlock>(header, "PART_DescriptionText").IsVisible.ShouldBeFalse();
            Find<Control>(header, "PART_MetadataCard").IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Shows_Provided_Tags_And_Metadata_In_Fixed_Order()
    {
        var header = new GalleryShowCaseHeader
        {
            Title             = "Splash",
            Category          = "Other",
            Status            = "Preview",
            StatusTagColor    = "processing",
            IntroducedVersion = "v6.0.7",
            Subtitle          = "Splash screen",
            Description       = "Display startup progress.",
            Namespace         = "AtomUI.Desktop.Controls",
            Package           = "AtomUI.Desktop.Controls.Extras",
            BaseClass         = "ContentControl"
        };

        ShowInWindow(header, () =>
        {
            Find<Tag>(header, "PART_CategoryTag").Text.ShouldBe("Other");
            Find<Tag>(header, "PART_StatusTag").Text.ShouldBe("Preview");
            Find<Tag>(header, "PART_StatusTag").TagColor.ShouldBe("processing");
            Find<Tag>(header, "PART_IntroducedVersionTag").Text.ShouldBe("v6.0.7");
            Find<Tag>(header, "PART_IntroducedVersionTag").Variant.ShouldBe(TagVariant.Filled);

            var metadataItems = Find<StackPanel>(header, "PART_MetadataItems")
                .Children
                .OfType<GalleryShowCaseMetadataRowPanel>()
                .ToList();

            metadataItems.Count.ShouldBe(3);
            ReadMetadataValue(metadataItems[0]).ShouldBe("AtomUI.Desktop.Controls");
            ReadMetadataValue(metadataItems[1]).ShouldBe("AtomUI.Desktop.Controls.Extras");
            ReadMetadataValue(metadataItems[2]).ShouldBe("ContentControl");
        });
    }

    [Fact]
    public void Metadata_Layout_Uses_Row_Space_For_Long_Localized_Labels_And_Values()
    {
        var header = new GalleryShowCaseHeader
        {
            Title              = "DataGrid",
            Namespace          = "AtomUI.Desktop.Controls",
            Package            = "AtomUI.Desktop.Controls.DataGrid.Controls.Internal.LongPackageName",
            BaseClass          = "TemplatedControl",
            MetadataValueWidth = 260
        };

        ShowInWindow(header, () =>
        {
            var namespaceLabel = FindMetadataTextBlock(header, "Namespace");
            var namespaceValue = FindMetadataTextBlock(header, header.Namespace!);
            var packageValue   = FindMetadataTextBlock(header, header.Package!);

            namespaceLabel.Text = "Espacio de nombres";
            header.InvalidateMeasure();
            Dispatcher.UIThread.RunJobs();

            namespaceLabel.Bounds.Width.ShouldBeGreaterThan(header.MetadataLabelWidth);
            namespaceLabel.Bounds.Right.ShouldBeLessThanOrEqualTo(namespaceValue.Bounds.Left);
            packageValue.Bounds.Width.ShouldBeGreaterThan(header.MetadataValueWidth);
        });
    }

    [Fact]
    public void Metadata_Layout_Treats_Nan_Width_Overrides_As_Auto_Hints()
    {
        var header = new GalleryShowCaseHeader
        {
            Title              = "DataGrid",
            Namespace          = "AtomUI.Desktop.Controls",
            Package            = "AtomUI.Desktop.Controls.DataGrid",
            BaseClass          = "TemplatedControl",
            MetadataLabelWidth = double.NaN,
            MetadataValueWidth = double.NaN
        };

        ShowInWindow(header, () =>
        {
            var namespaceRow = Find<GalleryShowCaseMetadataRowPanel>(header, "PART_NamespaceMetadataItem");
            var packageValue = FindMetadataTextBlock(header, header.Package!);

            double.IsNaN(namespaceRow.LabelWidth).ShouldBeTrue();
            double.IsNaN(namespaceRow.ValueWidth).ShouldBeTrue();
            packageValue.Bounds.Width.ShouldBeGreaterThan(0d);
        });
    }

    [Fact]
    public void Metadata_Layout_Does_Not_Overlap_When_Header_Is_Narrow()
    {
        var header = new GalleryShowCaseHeader
        {
            Title     = "ComboBox",
            Namespace = "AtomUI.Desktop.Controls",
            Package   = "AtomUI.Desktop.Controls",
            BaseClass = "ComboBox"
        };

        ShowInWindow(header, 360, 260, () =>
        {
            AssertTextBlocksDoNotOverlap(
                FindMetadataLabel(header, "PART_NamespaceMetadataItem"),
                FindMetadataValue(header, "PART_NamespaceMetadataItem"));
            AssertTextBlocksDoNotOverlap(
                FindMetadataLabel(header, "PART_PackageMetadataItem"),
                FindMetadataValue(header, "PART_PackageMetadataItem"));
            AssertTextBlocksDoNotOverlap(
                FindMetadataLabel(header, "PART_BaseClassMetadataItem"),
                FindMetadataValue(header, "PART_BaseClassMetadataItem"));
        });
    }

    [Fact]
    public void Metadata_Layout_Handles_Long_Localized_Label_And_Value_When_Header_Is_Narrow()
    {
        var header = new GalleryShowCaseHeader
        {
            Title     = "DataGrid",
            Namespace = "AtomUI.Desktop.Controls.DataGrid.Controls.Internal.LongNamespace",
            Package   = "AtomUI.Desktop.Controls.DataGrid",
            BaseClass = "TemplatedControl"
        };

        ShowInWindow(header, 380, 260, () =>
        {
            var row   = Find<GalleryShowCaseMetadataRowPanel>(header, "PART_NamespaceMetadataItem");
            var label = FindMetadataLabel(header, "PART_NamespaceMetadataItem");
            var value = FindMetadataValue(header, "PART_NamespaceMetadataItem");

            label.Text = "Espacio de nombres muy largo";
            row.InvalidateMeasure();
            Dispatcher.UIThread.RunJobs();

            AssertTextBlocksDoNotOverlap(label, value);
            value.Bounds.Right.ShouldBeLessThanOrEqualTo(row.Bounds.Width + 0.5);
            label.Bounds.Width.ShouldBeGreaterThan(0d);
            value.Bounds.Width.ShouldBeGreaterThan(0d);
        });
    }

    [Fact]
    public void Metadata_Layout_Uses_Consistent_Font_Family_For_Label_And_Value()
    {
        var header = new GalleryShowCaseHeader
        {
            Title     = "Breadcrumb",
            Namespace = "AtomUI.Desktop.Controls",
            Package   = "AtomUI.Desktop.Controls",
            BaseClass = "ItemsControl"
        };

        ShowInWindow(header, () =>
        {
            AssertMetadataTextBlocksUseConsistentFontFamily(header, "PART_NamespaceMetadataItem");
            AssertMetadataTextBlocksUseConsistentFontFamily(header, "PART_PackageMetadataItem");
            AssertMetadataTextBlocksUseConsistentFontFamily(header, "PART_BaseClassMetadataItem");
        });
    }

    [Fact]
    public void Metadata_Font_Uses_The_Global_Ui_Font_Token()
    {
        var tokenSource = File.ReadAllText(GetRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/GalleryShowCaseHeaderToken.cs"));
        Regex.IsMatch(tokenSource, @"\bMetadataValueFontFamily\s*=\s*EffectiveGlobalToken\.FontFamily\b")
            .ShouldBeTrue();

        var theme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Toolkits.GalleryBase/Controls/Themes/GalleryShowCaseHeaderTheme.axaml"));
        var metadataTexts = theme.Descendants()
            .Where(element => element.Name.LocalName == "GalleryShowCaseMetadataRowPanel")
            .SelectMany(row => row.Elements().Where(element => element.Name.LocalName == "TextBlock"))
            .ToArray();

        metadataTexts.ShouldNotBeEmpty();
        metadataTexts.ShouldAllBe(text =>
            (string?)text.Attribute("FontFamily") ==
            "{gallery:GalleryShowCaseHeaderTokenResource MetadataValueFontFamily}");
    }

    [Fact]
    public void Metadata_Row_Panel_Treats_Value_Width_As_Shrinkable_Preference()
    {
        var label = new FixedMeasureControl(new Size(180, 20));
        var value = new FixedMeasureControl(new Size(120, 20));
        var row = new GalleryShowCaseMetadataRowPanel
        {
            LabelWidth  = 84,
            ValueWidth  = 220,
            PairSpacing = 10
        };
        row.Children.Add(label);
        row.Children.Add(value);

        row.Measure(new Size(260, 100));
        row.Arrange(new Rect(0, 0, 260, row.DesiredSize.Height));

        label.Bounds.Width.ShouldBe(84d);
        value.Bounds.Left.ShouldBe(94d);
        value.Bounds.Width.ShouldBe(166d);
        value.Bounds.Right.ShouldBe(row.Bounds.Right);

        row.Measure(new Size(500, 100));
        row.Arrange(new Rect(0, 0, 500, row.DesiredSize.Height));

        label.Bounds.Width.ShouldBe(180d);
        value.Bounds.Left.ShouldBe(190d);
        value.Bounds.Width.ShouldBe(310d);
        value.Bounds.Right.ShouldBe(row.Bounds.Right);
    }

    private static string? ReadMetadataValue(GalleryShowCaseMetadataRowPanel item)
    {
        return item.Children
                   .OfType<AtomUITextBlock>()
                   .Last()
                   .Text;
    }

    private static AtomUITextBlock FindMetadataTextBlock(Control root, string text)
    {
        var control = root.GetVisualDescendants()
                          .OfType<AtomUITextBlock>()
                          .FirstOrDefault(descendant => descendant.Text == text);
        control.ShouldNotBeNull($"Expected metadata text '{text}' to exist.");
        return control;
    }

    private static AtomUITextBlock FindMetadataLabel(Control root, string itemName)
    {
        return Find<GalleryShowCaseMetadataRowPanel>(root, itemName)
               .Children
               .OfType<AtomUITextBlock>()
               .First();
    }

    private static AtomUITextBlock FindMetadataValue(Control root, string itemName)
    {
        return Find<GalleryShowCaseMetadataRowPanel>(root, itemName)
               .Children
               .OfType<AtomUITextBlock>()
               .Last();
    }

    private static void AssertTextBlocksDoNotOverlap(AtomUITextBlock label, AtomUITextBlock value)
    {
        label.Bounds.Right.ShouldBeLessThanOrEqualTo(
            value.Bounds.Left,
            $"Expected metadata label '{label.Text}' to stay before value '{value.Text}'.");
        GetVerticalCenter(label).ShouldBe(
            GetVerticalCenter(value),
            tolerance: 0.5,
            $"Expected metadata label '{label.Text}' to be vertically centered with value '{value.Text}'.");
        label.Bounds.Y.ShouldBe(
            value.Bounds.Y,
            tolerance: 0.5,
            $"Expected metadata label '{label.Text}' to share the value row top with '{value.Text}'.");
        label.Bounds.Height.ShouldBe(
            value.Bounds.Height,
            tolerance: 0.5,
            $"Expected metadata label '{label.Text}' to share the value row height with '{value.Text}'.");
    }

    private static void AssertMetadataTextBlocksUseConsistentFontFamily(Control root, string itemName)
    {
        var label = FindMetadataLabel(root, itemName);
        var value = FindMetadataValue(root, itemName);

        label.FontFamily.ShouldBe(
            value.FontFamily,
            $"Expected metadata label '{label.Text}' and value '{value.Text}' to share font metrics.");
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
    }

    private static double GetVerticalCenter(Control control)
    {
        return control.Bounds.Y + control.Bounds.Height / 2;
    }

    private static T Find<T>(Control root, string name)
        where T : Control
    {
        var control = root.GetVisualDescendants()
                          .OfType<T>()
                          .FirstOrDefault(descendant => descendant.Name == name);
        control.ShouldNotBeNull($"Expected template part {name} to exist.");
        return control;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        ShowInWindow(content, 760, 240, assertion);
    }

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = width,
            Height  = height,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        content.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        try
        {
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class FixedMeasureControl : Control
    {
        private readonly Size _desiredSize;

        public FixedMeasureControl(Size desiredSize)
        {
            _desiredSize = desiredSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return _desiredSize;
        }
    }
}
