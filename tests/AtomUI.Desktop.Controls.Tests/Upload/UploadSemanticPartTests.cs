using System.Collections.ObjectModel;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIUpload = AtomUI.Desktop.Controls.Upload;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Upload;

public class UploadSemanticPartTests
{
    private const string ListClass = "semantic-list";
    private const string ItemClass = "semantic-item";

    static UploadSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_The_Upload_List_And_Item_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIUpload), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe([
            "root",
            "item",
            "list"
        ]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "list"),
            ListClass,
            "/template/ .semantic-list",
            typeof(ItemsControl),
            SemanticPartCardinality.Single,
            runtimeCreated: false,
            typeof(UploadListStyle));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "item"),
            ItemClass,
            "/template/ .semantic-list > .semantic-item",
            typeof(TemplatedControl),
            SemanticPartCardinality.Multiple,
            runtimeCreated: true,
            typeof(UploadItemStyle));
    }

    [Fact]
    public void Upload_Theme_Declares_The_List_Marker_For_Every_ListType_Template()
    {
        var document = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/UploadTheme.axaml"),
            LoadOptions.SetLineInfo);
        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe([
                                "semantic-list:UploadList",
                                "semantic-list:UploadPictureShapeList"
                            ]);
    }

    [Theory]
    [InlineData(UploadListType.Text)]
    [InlineData(UploadListType.Picture)]
    [InlineData(UploadListType.PictureCard)]
    [InlineData(UploadListType.PictureCircle)]
    public void Every_ListType_Realizes_One_List_Part_And_One_Item_Part_Per_File(UploadListType listType)
    {
        var upload = CreateUpload(listType);

        using var window = Show(upload);

        var list = GetSemanticElements(upload, ListClass).Single();
        list.ShouldBeAssignableTo<ItemsControl>();

        var items = GetSemanticElements(upload, ItemClass);
        items.Length.ShouldBe(2);
        items.ShouldAllBe(static item => item is AbstractUploadListItem);
        upload.Classes.ShouldNotContain("semantic-root");

        if (listType is UploadListType.PictureCard or UploadListType.PictureCircle)
        {
            list.GetVisualDescendants()
                .OfType<UploadAppendContentItem>()
                .Single()
                .Classes.ShouldNotContain(ItemClass);
        }
    }

    [Theory]
    [InlineData(UploadListType.Text)]
    [InlineData(UploadListType.Picture)]
    [InlineData(UploadListType.PictureCard)]
    [InlineData(UploadListType.PictureCircle)]
    public void Generated_Styles_Apply_Through_The_List_And_Item_Routes(UploadListType listType)
    {
        var upload = CreateUpload(listType);
        upload.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUIUpload>().Class("semantic-owner"));
        ownerStyle.Children.Add(new UploadListStyle
        {
            Setters = { new Setter(Control.TagProperty, "list") }
        });
        ownerStyle.Children.Add(new UploadItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        upload.Styles.Add(ownerStyle);

        using var window = Show(upload);

        GetSemanticElements(upload, ListClass).Single().Tag.ShouldBe("list");
        GetSemanticElements(upload, ItemClass).ShouldAllBe(static item => Equals(item.Tag, "item"));
    }

    private static AtomUIUpload CreateUpload(UploadListType listType)
    {
        return new AtomUIUpload
        {
            AutoUpload = false,
            ListType   = listType,
            TriggerContent = new UploadTrigger
            {
                Content = "Upload"
            },
            Files = new ObservableCollection<UploadFileItem>
            {
                new()
                {
                    Name        = "first.png",
                    Status      = FileUploadStatus.Pending,
                    IsImageFile = true
                },
                new()
                {
                    Name   = "second.txt",
                    Status = FileUploadStatus.Pending
                }
            }
        };
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIUpload));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBeNull();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        string selectorRoute,
        Type contractType,
        SemanticPartCardinality cardinality,
        bool runtimeCreated,
        Type styleType)
    {
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.StyleType.ShouldBe(styleType);
        part.Since.ShouldBe("6.2.0");
    }

    private static Control[] GetSemanticElements(Control owner, string semanticClass)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => control.Classes.Contains(semanticClass))
                    .ToArray();
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 900,
            Height  = 600,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
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

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
